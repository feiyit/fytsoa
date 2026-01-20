using System.Text.Json;
using FytSoa.Common.Param;
using FytSoa.Common.Result;
using SqlSugar;
using Mapster;
using FytSoa.Domain.Wf;
using FytSoa.Sugar;
using Microsoft.AspNetCore.Mvc;
using FytSoa.Application.Sys;
using FytSoa.Common.Extensions;
using FytSoa.Common.Utils;

namespace FytSoa.Application.Wf;

/// <summary>
/// 工作流：流程实例应用服务
/// </summary>
[ApiExplorerSettings(GroupName = "v10")]
public class WorkflowInstanceService : IApplicationService
{
    private readonly SugarRepository<WorkflowInstance> _thisRepository;
    private readonly SugarRepository<WorkflowDefinitionModel> _modelRepository;
    private readonly SugarRepository<WorkflowTask> _taskRepository;
    private readonly SugarRepository<WorkflowInstanceHistory> _historyRepository;
    private readonly SysOrganizationService _organizationService;
    private readonly SysAdminService _adminService;

    public WorkflowInstanceService(
        SugarRepository<WorkflowInstance> thisRepository,
        SugarRepository<WorkflowDefinitionModel> modelRepository,
        SugarRepository<WorkflowTask> taskRepository,
        SugarRepository<WorkflowInstanceHistory> historyRepository,
        SysOrganizationService organizationService,
        SysAdminService adminService)
    {
        _thisRepository = thisRepository;
        _modelRepository = modelRepository;
        _taskRepository = taskRepository;
        _historyRepository = historyRepository;
        _organizationService = organizationService;
        _adminService = adminService;
    }

    public async Task<WorkflowInstanceDto?> GetAsync(long id)
    {
        var entity = await _thisRepository.Context.Queryable<WorkflowInstance>()
            .FirstAsync(x => x.Id == id);
        return entity == null ? null : entity.Adapt<WorkflowInstanceDto>();
    }

    public async Task<List<WorkflowInstanceDto>> GetByBusinessKeyAsync(long tenantId, string definitionKey, string businessKey)
    {
        var list = await _thisRepository.Context.Queryable<WorkflowInstance>()
            .Where(x => x.TenantId == tenantId && x.DefinitionKey == definitionKey && x.BusinessKey == businessKey)
            .OrderBy(x => x.StartTime, OrderByType.Desc)
            .ToListAsync();

        return list.Adapt<List<WorkflowInstanceDto>>();
    }

    /// <summary>
    /// 分页查询“我发起的流程实例”
    /// </summary>
    public async Task<PageResult<WorkflowInstanceDto>> GetMyStartPagesAsync(PageParam param)
    {
        var query = await _thisRepository.AsQueryable()
            .Where(x => x.StartUserId == param.userId)
            .OrderBy(x => x.StartTime, OrderByType.Desc)
            .ToPageAsync(param.Page, param.Limit);

        return query.Adapt<PageResult<WorkflowInstanceDto>>();
    }

    #region 启动流程：创建实例 + 首个任务 + 历史

    /// <summary>
    /// 创建流程实例：包括实例、首个任务、实例历史
    /// </summary>
    public async Task<long> CreateAsync(WorkflowInstanceDto input)
    {
        var db = _thisRepository.Context;

        var tranResult = await db.Ado.UseTranAsync(async () =>
        {
            // 1. 创建实例
            var entity = input.Adapt<WorkflowInstance>();
            
            entity.StartTime = entity.StartTime == default ? DateTime.Now : entity.StartTime;
            entity.Status = 0; // 运行中
            entity.CreatedAt = DateTime.Now;
            
            // 2. 读取最新模型，解析“发起人节点”角色配置并校验发起权限，再解析第一个审批节点
            var model = await db.Queryable<WorkflowDefinitionModel>()
                .Where(x => x.DefinitionId == entity.DefinitionId)
                .OrderBy(x => x.IsLatest, OrderByType.Desc)
                .OrderBy(x => x.CreatedAt, OrderByType.Desc)
                .FirstAsync();

            // 2.1 校验发起人是否具备发起权限（根据发起人节点 nodeRoleList 配置）
            if (model != null && !string.IsNullOrWhiteSpace(model.ModelJson))
            {
                await EnsureStartUserHasPermissionAsync(entity, model.ModelJson);
            }

            var instanceId= await _thisRepository.InsertReturnSnowflakeIdAsync(entity);
            entity.Id = instanceId;
            
            // 使用统一的模型解析辅助类，先定位“首个审批节点”的 JSON
            string? firstNodeJson = null;
            if (model != null && !string.IsNullOrWhiteSpace(model.ModelJson))
            {
                firstNodeJson = WorkflowModelHelper.FindFirstApproverNodeJson(model.ModelJson);
            }

            var firstNodeAssignees = new List<(long UserId, string UserName)>();
            string nodeKeyForFirst = "NODE_START";
            string nodeNameForFirst = "首个审批节点";

            if (!string.IsNullOrWhiteSpace(firstNodeJson))
            {
                using var nodeDoc = JsonDocument.Parse(firstNodeJson);
                var node = nodeDoc.RootElement;

                // 读取节点名称 / Id（用于任务 NodeId/NodeName）
                var nodeName = node.TryGetProperty("nodeName", out var nodeNameProp)
                    ? nodeNameProp.GetString() ?? string.Empty
                    : string.Empty;

                string nodeId = string.Empty;
                if (node.TryGetProperty("id", out var idProp))
                {
                    nodeId = idProp.ToString();
                }
                else if (node.TryGetProperty("nodeId", out var nodeIdProp))
                {
                    nodeId = nodeIdProp.ToString();
                }

                if (string.IsNullOrWhiteSpace(nodeId))
                {
                    nodeId = string.IsNullOrWhiteSpace(nodeName) ? "NODE_START" : nodeName;
                }

                if (string.IsNullOrWhiteSpace(nodeName))
                {
                    nodeName = "首个审批节点";
                }

                nodeKeyForFirst = nodeId;
                nodeNameForFirst = nodeName;

                // 根据 setType 解析该节点的实际审批人：
                // - 指定成员：nodeUserList
                // - 主管：通过 SysOrganizationService 计算发起人的主管
                // - 发起人自己：直接使用 StartUser
                firstNodeAssignees = await ResolveApproversForInstanceAsync(entity, node);
            }
            else
            {
                // 保持兼容：如果新解析失败，退回到旧逻辑（仅支持指定成员）
                var approvers = model == null
                    ? new List<WorkflowModelHelper.WorkflowUserJson>()
                    : WorkflowModelHelper.FindFirstApproverUsers(model.ModelJson);

                if (approvers.Count > 0)
                {
                    nodeKeyForFirst = string.IsNullOrWhiteSpace(approvers[0].NodeId)
                        ? (string.IsNullOrWhiteSpace(approvers[0].NodeName) ? "NODE_START" : approvers[0].NodeName)
                        : approvers[0].NodeId;
                    nodeNameForFirst = string.IsNullOrWhiteSpace(approvers[0].NodeName)
                        ? "首个审批节点"
                        : approvers[0].NodeName;

                    foreach (var user in approvers)
                    {
                        if (long.TryParse(user.Id, out var uid) && uid > 0)
                        {
                            firstNodeAssignees.Add((uid, user.Name));
                        }
                    }
                }
            }

            // 3. 为首个节点的每个审批人创建待办任务
            if (firstNodeAssignees.Count > 0)
            {
                var nodeIds = new List<string>();

                // 判断首个审批节点是否为“连续多级主管 + 顺序审批”，用于初始化 LevelIndex
                var isSequentialMultiManager = false;
                if (!string.IsNullOrWhiteSpace(firstNodeJson))
                {
                    using var nodeDoc2 = JsonDocument.Parse(firstNodeJson);
                    var n = nodeDoc2.RootElement;
                    var setType = n.TryGetProperty("setType", out var stProp2)
                        ? (stProp2.ValueKind == JsonValueKind.Number
                            ? stProp2.GetInt32()
                            : int.TryParse(stProp2.GetString() ?? "1", out var v2) ? v2 : 1)
                        : 1;
                    var examineMode = n.TryGetProperty("examineMode", out var exProp2)
                        ? (exProp2.ValueKind == JsonValueKind.Number
                            ? exProp2.GetInt32()
                            : int.TryParse(exProp2.GetString() ?? "2", out var v3) ? v3 : 2)
                        : 2;
                    isSequentialMultiManager = (setType == 7 && examineMode == 1);
                }

                foreach (var user in firstNodeAssignees)
                {
                    var task = new WorkflowTask
                    {
                        TenantId = entity.TenantId,
                        InstanceId = entity.Id,
                        NodeId = nodeKeyForFirst,
                        NodeName = nodeNameForFirst,
                        AssigneeId = user.UserId,
                        AssigneeName = user.UserName,
                        Status = 0,
                        CreatedAt = DateTime.Now,
                        // 多级主管 + 顺序审批：首个任务视为第 1 级主管，其它类型节点默认为 0
                        LevelIndex = isSequentialMultiManager ? 1 : 0,
                    };
                    await _taskRepository.InsertAsync(task);
                    nodeIds.Add(nodeKeyForFirst);
                }

                entity.CurrentNodeIds = string.Join(",", nodeIds.Distinct());
                await _thisRepository.UpdateAsync(entity);
            }

            // 4. 记录实例历史（发起）
            var history = new WorkflowInstanceHistory
            {
                TenantId = entity.TenantId,
                InstanceId = entity.Id,
                EventType = "Start",
                FromStatus = null,
                ToStatus = entity.Status,
                OperatorId = entity.StartUserId,
                OperatorName = entity.StartUserName,
                Remark = "发起流程",
                CreatedAt = DateTime.Now,
            };
            await _historyRepository.InsertAsync(history);

            return entity.Id;
        });

    if (!tranResult.IsSuccess)
        {
            throw new Exception("启动流程失败", tranResult.ErrorException);
        }

        return tranResult.Data;
    }

    /// <summary>
    /// 根据流程模型中的“发起人”节点角色配置，校验当前用户是否有权发起流程。
    /// 说明：
    /// - 如果发起人节点未配置 nodeRoleList，视为所有人均可发起；
    /// - 如果配置了角色集合，则当前用户必须至少属于其中一个角色；
    /// - 校验失败时抛出业务异常，阻止流程实例创建。
    /// </summary>
    private async Task EnsureStartUserHasPermissionAsync(WorkflowInstance instance, string modelJson)
    {
        if (string.IsNullOrWhiteSpace(modelJson))
        {
            return;
        }

        using var doc = JsonDocument.Parse(modelJson);
        var root = doc.RootElement;

        // 根节点约定为发起人节点（type = 0），包含 nodeRoleList
        if (!root.TryGetProperty("type", out var typeProp) ||
            typeProp.ValueKind != JsonValueKind.Number ||
            typeProp.GetInt32() != 0)
        {
            // 未按约定建模，则不做权限限制
            return;
        }

        if (!root.TryGetProperty("nodeRoleList", out var roleArr) ||
            roleArr.ValueKind != JsonValueKind.Array)
        {
            // 未配置角色：默认所有人可以发起
            return;
        }

        var roleIds = new List<long>();
        foreach (var item in roleArr.EnumerateArray())
        {
            var id = item.TryGetProperty("id", out var idProp) ? idProp.ToString() : string.Empty;
            if (long.TryParse(id, out var rid) && rid > 0)
            {
                roleIds.Add(rid);
            }
        }

        if (roleIds.Count == 0)
        {
            // 角色列表为空：仍视为无限制
            return;
        }

        var allowed = await _adminService.IsAdminInRolesAsync(
            instance.TenantId,
            instance.StartUserId,
            roleIds);

        if (!allowed)
        {
            throw new BusinessException("当前用户无权发起该流程，请联系管理员配置发起人角色。");
        }
    }


    /// <summary>
    /// 针对“首个审批节点”，根据节点配置解析实际审批人（与 WorkflowTaskService 中逻辑保持一致）：
    /// - setType = 1：指定成员（nodeUserList）；
    /// - setType = 2：主管（发起人的第 examineLevel 级主管）；
    /// - setType = 3：角色（nodeRoleList 对应的所有用户）；
    /// - setType = 7：连续多级主管（根据 directorMode/directorLevel 计算主管链）；
    /// - setType = 5：发起人自己；
    /// 其它类型暂时回退为按 nodeUserList 处理。
    /// </summary>
    private async Task<List<(long UserId, string UserName)>> ResolveApproversForInstanceAsync(
        WorkflowInstance instance,
        JsonElement node)
    {
        var result = new List<(long, string)>();

        var setType = 1;
        if (node.TryGetProperty("setType", out var setTypeProp))
        {
            try
            {
                if (setTypeProp.ValueKind == JsonValueKind.Number)
                {
                    setType = setTypeProp.GetInt32();
                }
                else
                {
                    var text = setTypeProp.GetString() ?? string.Empty;
                    if (!int.TryParse(text, out setType))
                    {
                        setType = 1;
                    }
                }
            }
            catch
            {
                setType = 1;
            }
        }

        // 1) 指定成员
        if (setType == 1)
        {
            if (node.TryGetProperty("nodeUserList", out var userArr) &&
                userArr.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in userArr.EnumerateArray())
                {
                    var id = item.TryGetProperty("id", out var idProp) ? idProp.ToString() : string.Empty;
                    var name = item.TryGetProperty("name", out var nameProp) ? nameProp.ToString() ?? string.Empty : string.Empty;
                    if (long.TryParse(id, out var uid) && uid > 0)
                    {
                        result.Add((uid, name));
                    }
                }
            }

            return result;
        }

        // 2) 主管：发起人的第 examineLevel 级主管
        if (setType == 2)
        {
            var level = 1;
            if (node.TryGetProperty("examineLevel", out var lvlProp))
            {
                try
                {
                    if (lvlProp.ValueKind == JsonValueKind.Number)
                    {
                        level = lvlProp.GetInt32();
                    }
                    else
                    {
                        var text = lvlProp.GetString() ?? string.Empty;
                        if (!int.TryParse(text, out level))
                        {
                            level = 1;
                        }
                    }
                }
                catch
                {
                    level = 1;
                }
            }

            if (level <= 0) level = 1;

            var manager = await _organizationService.GetNthManagerByUserAsync(
                instance.TenantId,
                instance.StartUserId,
                level);

            if (manager.HasValue && manager.Value.UserId > 0)
            {
                var name = string.IsNullOrWhiteSpace(manager.Value.UserName)
                    ? manager.Value.UserId.ToString()
                    : manager.Value.UserName!;
                result.Add((manager.Value.UserId, name));
            }

            return result;
        }

        // 3) 角色：根据节点配置的 nodeRoleList，解析对应角色下的所有用户
        if (setType == 3)
        {
            var roleIds = new List<long>();
            if (node.TryGetProperty("nodeRoleList", out var roleArr) &&
                roleArr.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in roleArr.EnumerateArray())
                {
                    var id = item.TryGetProperty("id", out var idProp) ? idProp.ToString() : string.Empty;
                    if (long.TryParse(id, out var rid) && rid > 0)
                    {
                        roleIds.Add(rid);
                    }
                }
            }

            if (roleIds.Count == 0)
            {
                // 未配置角色时直接返回空列表，由上层决定是结束流程还是继续
                return result;
            }

            var users = await _adminService.GetAdminsByRolesAsync(
                instance.TenantId,
                roleIds);

            foreach (var (userId, userName) in users)
            {
                var name = string.IsNullOrWhiteSpace(userName)
                    ? userId.ToString()
                    : userName!;
                result.Add((userId, name));
            }

            return result;
        }

        // 4) 连续多级主管：根据 directorMode/directorLevel 构建主管链
        if (setType == 7)
        {
            // 多级主管节点的审批方式：
            // - examineMode=1：严格按层级顺序审批（当前实现：仅首个节点按第 1 级主管处理，后续层级由 WorkflowTaskService 逐层生成）；
            // - examineMode!=1：一次性为所有层级主管创建任务，由 examineMode 控制多人审批方式。
            var examineMode = 2;
            if (node.TryGetProperty("examineMode", out var exProp))
            {
                try
                {
                    examineMode = exProp.ValueKind == JsonValueKind.Number
                        ? exProp.GetInt32()
                        : int.Parse(exProp.GetString() ?? "2");
                }
                catch
                {
                    examineMode = 2;
                }
            }

            int? maxLevel = null;
            var directorMode = 0;
            if (node.TryGetProperty("directorMode", out var modeProp))
            {
                try
                {
                    directorMode = modeProp.ValueKind == JsonValueKind.Number
                        ? modeProp.GetInt32()
                        : int.Parse(modeProp.GetString() ?? "0");
                }
                catch
                {
                    directorMode = 0;
                }
            }

            if (directorMode == 1)
            {
                // 自定义终点：直到发起人的第 directorLevel 级主管
                if (node.TryGetProperty("directorLevel", out var lvlProp))
                {
                    try
                    {
                        maxLevel = lvlProp.ValueKind == JsonValueKind.Number
                            ? lvlProp.GetInt32()
                            : int.Parse(lvlProp.GetString() ?? "1");
                    }
                    catch
                    {
                        maxLevel = 1;
                    }
                }
                if (maxLevel.HasValue && maxLevel.Value <= 0)
                {
                    maxLevel = 1;
                }
            }

            if (examineMode == 1)
            {
                // 严格顺序审批：首节点仅分配“第 1 级主管”，后续层级由任务服务逐层生成。
                var manager = await _organizationService.GetNthManagerByUserAsync(
                    instance.TenantId,
                    instance.StartUserId,
                    1);

                if (manager.HasValue && manager.Value.UserId > 0)
                {
                    var name = string.IsNullOrWhiteSpace(manager.Value.UserName)
                        ? manager.Value.UserId.ToString()
                        : manager.Value.UserName!;
                    result.Add((manager.Value.UserId, name));
                }
            }
            else
            {
                // 非顺序审批：一次性生成整个主管链，由 examineMode 控制多人审批方式。
                var chain = await _organizationService.GetManagerChainByUserAsync(
                    instance.TenantId,
                    instance.StartUserId,
                    maxLevel);

                foreach (var (userId, userName) in chain)
                {
                    var name = string.IsNullOrWhiteSpace(userName)
                        ? userId.ToString()
                        : userName!;
                    result.Add((userId, name));
                }
            }

            return result;
        }

        // 5) 发起人自己
        if (setType == 5)
        {
            var name = string.IsNullOrWhiteSpace(instance.StartUserName)
                ? instance.StartUserId.ToString()
                : instance.StartUserName!;
            result.Add((instance.StartUserId, name));
            return result;
        }

        // 其它类型兜底：尝试按 nodeUserList 解析
        if (node.TryGetProperty("nodeUserList", out var fallbackArr) &&
            fallbackArr.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in fallbackArr.EnumerateArray())
            {
                var id = item.TryGetProperty("id", out var idProp) ? idProp.ToString() : string.Empty;
                var name = item.TryGetProperty("name", out var nameProp) ? nameProp.ToString() ?? string.Empty : string.Empty;
                if (long.TryParse(id, out var uid) && uid > 0)
                {
                    result.Add((uid, name));
                }
            }
        }

        return result;
    }

    #endregion

    public async Task UpdateAsync(WorkflowInstanceDto input)
    {
        var entity = input.Adapt<WorkflowInstance>();
        await _thisRepository.UpdateAsync(entity);
    }
}
