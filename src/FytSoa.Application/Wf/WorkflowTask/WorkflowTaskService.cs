using System.Text.Json;
using FytSoa.Common.Param;
using FytSoa.Common.Result;
using FytSoa.Common.Utils;
using SqlSugar;
using Mapster;
using FytSoa.Domain.Wf;
using FytSoa.Sugar;
using MapsterMapper;
using Microsoft.AspNetCore.Mvc;
using FytSoa.Application.Sys;
using FytSoa.Common.Extensions;
using FytSoa.Common.Utils;

namespace FytSoa.Application.Wf;

/// <summary>
    /// 工作流：任务应用服务
/// </summary>
[ApiExplorerSettings(GroupName = "v10")]
public class WorkflowTaskService : IApplicationService
{
    private readonly SugarRepository<WorkflowTask> _thisRepository;
    private readonly WorkflowBusinessService _businessService;
    private readonly SysOrganizationService _organizationService;
    private readonly SysAdminService _adminService;
    public WorkflowTaskService(SugarRepository<WorkflowTask> thisRepository
    ,WorkflowBusinessService businessService
    ,SysOrganizationService organizationService
    ,SysAdminService adminService)
    {
        _thisRepository = thisRepository;
        _businessService=businessService;
        _organizationService = organizationService;
        _adminService = adminService;
    }
    
    /// <summary>
    /// 查询所有——分页
    /// </summary>
    /// <param name="param"></param>
    /// <returns></returns>
    public async Task<PageResult<WorkflowTaskDto>> GetPagesAsync(PageParam param)
    {
        var query = await _thisRepository.AsQueryable()
            .Where(x => x.AssigneeId == param.userId && x.Status == 0)
            .OrderByDescending(m=>m.Id)
            .ToPageAsync(param.Page, param.Limit);
        return query.Adapt<PageResult<WorkflowTaskDto>>();
    }

    public async Task<WorkflowTaskDto?> GetAsync(long id)
    {
        var entity = await _thisRepository.Context.Queryable<WorkflowTask>()
            .FirstAsync(x => x.Id == id);
        return entity == null ? null : entity.Adapt<WorkflowTaskDto>();
    }

    /// <summary>
    /// 查询当前用户待办任务（简单示例）
    /// </summary>
    public async Task<List<WorkflowTaskDto>> GetTodoListAsync(long tenantId, long userId)
    {
        var list = await _thisRepository.Context.Queryable<WorkflowTask>()
            .Where(x => x.TenantId == tenantId && x.AssigneeId == userId && x.Status == 0)
            .OrderBy(x => x.CreatedAt, OrderByType.Asc)
            .ToListAsync();

        return list.Adapt<List<WorkflowTaskDto>>();
    }

    public async Task<long> CreateAsync(WorkflowTaskDto input)
    {
        var entity = input.Adapt<WorkflowTask>();
        
        entity.CreatedAt = entity.CreatedAt == default ? DateTime.Now : entity.CreatedAt;
        entity.Status = 0;

        await _thisRepository.InsertAsync(entity);
        return entity.Id;
    }

    public async Task UpdateAsync(WorkflowTaskDto input)
    {
        var entity = input.Adapt<WorkflowTask>();
        await _thisRepository.UpdateAsync(entity);
    }

    /// <summary>
    /// 扫描所有待办任务，根据流程模型中的超时配置（termAuto/term/termMode）自动执行审批
    /// 注意：
    /// - termAuto=true 且 term&gt;0 的审批节点才会参与
    /// - termMode=0 表示超时自动同意（Agree），1 表示超时自动驳回（Reject）
    /// - 操作人默认为任务当前处理人（Assignee），以避免权限校验失败
    /// </summary>
    public async Task<int> AutoHandleTimeoutTasksAsync()
    {
        var db = _thisRepository.Context;
        var now = DateTime.Now;
        var handledCount = 0;

        // 预先缓存每个 DefinitionId 对应的最新流程模型，避免重复查询
        var modelCache = new Dictionary<long, WorkflowDefinitionModel>();

        // 查询所有待处理任务及其所属实例
        var pendingTasks = await db.Queryable<WorkflowTask, WorkflowInstance>((t, i) => t.InstanceId == i.Id)
            .Where((t, i) => t.Status == 0)
            .Select((t, i) => new { Task = t, Instance = i })
            .ToListAsync();

        foreach (var item in pendingTasks)
        {
            var task = item.Task;
            var instance = item.Instance;

            // 读取最新流程模型
            if (!modelCache.TryGetValue(instance.DefinitionId, out var model))
            {
                model = await db.Queryable<WorkflowDefinitionModel>()
                    .Where(x => x.DefinitionId == instance.DefinitionId)
                    .OrderBy(x => x.IsLatest, OrderByType.Desc)
                    .OrderBy(x => x.CreatedAt, OrderByType.Desc)
                    .FirstAsync();
                modelCache[instance.DefinitionId] = model;
            }

            if (model == null || string.IsNullOrWhiteSpace(model.ModelJson))
            {
                continue;
            }

            // 根据当前任务的 NodeId/NodeName 解析该节点的超时配置
            var timeoutCfg = WorkflowModelHelper.FindTimeoutConfig(
                model.ModelJson,
                task.NodeId,
                task.NodeName);

            if (timeoutCfg == null || !timeoutCfg.TermAuto || timeoutCfg.TermHours <= 0)
            {
                continue;
            }

            // 判断是否已经超时
            var dueTime = task.CreatedAt.AddHours(timeoutCfg.TermHours);
            if (now < dueTime)
            {
                continue;
            }

            // 根据 termMode 决定自动执行的动作：0=自动通过，1=自动拒绝
            var action = timeoutCfg.TermMode == 1 ? "Reject" : "Agree";
            var comment = timeoutCfg.TermMode == 1
                ? "系统超时自动驳回"
                : "系统超时自动同意";

            var input = new HandleTaskInput
            {
                TenantId = task.TenantId,
                TaskId = task.Id,
                Action = action,
                Comment = comment,
                // 为避免权限校验失败，操作人使用当前任务的处理人
                OperatorId = task.AssigneeId,
                OperatorName = string.IsNullOrWhiteSpace(task.AssigneeName)
                    ? "系统超时自动审批"
                    : task.AssigneeName,
            };

            try
            {
                await AddHandleAsync(input);
                handledCount++;
            }
            catch (Exception ex)
            {
                // 超时自动处理失败时仅记录日志，不中断整个扫描
                Console.WriteLine($"AutoHandleTimeoutTasks 处理任务 {task.Id} 失败：{ex.Message}");
            }
        }

        return handledCount;
    }
    
    /// <summary>
    /// 审批任务（同意 / 驳回）
    /// 多节点流程引擎（简化版）：
    /// - Agree：
    ///   - 当同一节点上仍有未处理任务时，只更新当前任务与历史，不流转
    ///   - 当该节点所有任务完成后，根据模型 JSON + 表单数据计算下一节点并创建新任务；
    ///     若不存在下一审批节点，则流程结束
    /// - Reject：
    ///   - 根据 RejectMode 决定退回目标：Start / Previous / Specific
    ///   - 为目标节点创建新的任务，实例保持运行状态；若 RejectMode 为空，则视为终止流程
    /// </summary>
    public async Task AddHandleAsync(HandleTaskInput input)
    {
        var db = _thisRepository.Context;

        var tranResult = await db.Ado.UseTranAsync(async () =>
        {
            // 1. 读取当前任务 & 实例
            var task = await db.Queryable<WorkflowTask>()
                .FirstAsync(x => x.TenantId == input.TenantId && x.Id == input.TaskId);

            if (task == null)
            {
                throw new BusinessException("任务不存在");
            }

            // 简单校验：当前操作人必须是任务处理人
            if (task.AssigneeId != input.OperatorId)
            {
                throw new BusinessException("当前用户不是该任务的处理人，无法操作");
            }

            if (task.Status != 0)
            {
                throw new BusinessException("任务已处理，无法重复操作");
            }

            var instance = await db.Queryable<WorkflowInstance>()
                .FirstAsync(x => x.Id == task.InstanceId);
            if (instance == null)
            {
                throw new BusinessException("流程实例不存在");
            }

            // 2. 更新任务状态 + 写入任务历史
            task.Status = 1; // 已处理
            var isCcNode = string.Equals(task.NodeName, "抄送人", StringComparison.Ordinal);
            // 抄送任务默认记录为 Read，其它节点按前端传入动作（Agree/Reject）
            task.Action = isCcNode
                ? (string.IsNullOrWhiteSpace(input.Action) ? "Read" : input.Action)
                : input.Action;
            task.Comment = input.Comment;
            task.CompletedAt = DateTime.Now;
            await _thisRepository.UpdateAsync(task);

            var taskHistory = new WorkflowTaskHistory
            {
                Id = Unique.Id(),
                TenantId = task.TenantId,
                InstanceId = task.InstanceId,
                TaskId = task.Id,
                NodeId = task.NodeId,
                NodeName = task.NodeName,
                AssigneeId = input.OperatorId,
                AssigneeName = input.OperatorName,
                Action = task.Action,
                Comment = task.Comment,
                CreatedAt = task.CreatedAt,
                CompletedAt = task.CompletedAt,
                LevelIndex = task.LevelIndex,
            };
            await db.Insertable(taskHistory).ExecuteCommandAsync();

            // 抄送节点：仅记录“已阅”，不参与后续流转和业务数据更新
            if (isCcNode)
            {
                return;
            }

            // 3. 解析表单数据：优先使用当前传入的 ExtraData，其次从业务数据中加载
            IDictionary<string, object?> formData = new Dictionary<string, object?>();
            if (input.ExtraData is not null)
            {
                formData = ExtractFormData(input.ExtraData);
            }
            else
            {
                var biz = await _businessService.GetAsync(instance.TenantId, instance.DefinitionKey, instance.BusinessKey);
                if (biz != null && !string.IsNullOrWhiteSpace(biz.FormDataJson))
                {
                    try
                    {
                        var json = JsonDocument.Parse(biz.FormDataJson);
                        formData = ExtractFormData(json.RootElement);
                    }
                    catch
                    {
                        formData = new Dictionary<string, object?>();
                    }
                }
            }

            // 4. 驳回逻辑
            var actionUpper = input.Action?.ToUpperInvariant() ?? "AGREE";
            if (actionUpper == "REJECT")
            {
                await HandleRejectAsync(instance, task, input, formData);
                return;
            }

            // 5. 同意逻辑：根据节点的多人审批方式（examineMode）决定是否立即流转
            // examineMode 约定：
            // 1 = 顺序审批（当前实现与会签一致，后续可扩展为真正顺序）
            // 2 = 会签（全部通过才流转）
            // 3 = 或签（任一人通过即可，其它人自动跳过）
            var model = await db.Queryable<WorkflowDefinitionModel>()
                .Where(x => x.DefinitionId == instance.DefinitionId)
                .OrderBy(x => x.IsLatest, OrderByType.Desc)
                .OrderBy(x => x.CreatedAt, OrderByType.Desc)
                .FirstAsync();

            var examineMode = 2;
            if (model != null && !string.IsNullOrWhiteSpace(model.ModelJson))
            {
                examineMode = WorkflowModelHelper.FindExamineMode(
                    model.ModelJson,
                    task.NodeId,
                    task.NodeName
                );
            }

            if (examineMode == 3)
            {
                // 或签：当前审批人同意后，直接通过本节点，其它未处理的同节点任务自动标记为“已自动跳过”
                var otherTasks = await db.Queryable<WorkflowTask>()
                    .Where(x => x.InstanceId == task.InstanceId &&
                                x.NodeId == task.NodeId &&
                                x.Id != task.Id &&
                                x.Status == 0)
                    .ToListAsync();

                if (otherTasks.Count > 0)
                {
                    foreach (var other in otherTasks)
                    {
                        other.Status = 1;
                        other.Action = "AutoSkip";
                        other.Comment = "或签：已由其他审批人处理";
                        other.CompletedAt = DateTime.Now;
                        await _thisRepository.UpdateAsync(other);

                        var autoHistory = new WorkflowTaskHistory
                        {
                            Id = Unique.Id(),
                            TenantId = other.TenantId,
                            InstanceId = other.InstanceId,
                            TaskId = other.Id,
                            NodeId = other.NodeId,
                            NodeName = other.NodeName,
                            AssigneeId = other.AssigneeId,
                            AssigneeName = other.AssigneeName,
                            Action = other.Action,
                            Comment = other.Comment,
                            CreatedAt = other.CreatedAt,
                            CompletedAt = other.CompletedAt,
                        };
                        await db.Insertable(autoHistory).ExecuteCommandAsync();
                    }
                }

                // 或签：一人通过即可，直接推进到下一节点
                {
                    await HandleAgreeAsync(instance, task, input, formData);
                }
            }
            else
            {
                // 会签 / 顺序：仍按“所有同节点任务都处理完”再推进到下一节点
                var hasPendingSameNode = await db.Queryable<WorkflowTask>()
                    .Where(x => x.InstanceId == task.InstanceId && x.NodeId == task.NodeId && x.Status == 0)
                    .AnyAsync();

                if (!hasPendingSameNode)
                {
                    await HandleAgreeAsync(instance, task, input, formData);
                }
            }

            // 6. 如果 ExtraData 里带有 formData，则更新业务表单数据
            if (input.ExtraData != null)
            {
                try
                {
                    await _businessService.SaveAsync(new WorkflowBusinessService.SaveBusinessInput
                    {
                        TenantId = instance.TenantId,
                        DefinitionId = instance.DefinitionId,
                        DefinitionKey = instance.DefinitionKey,
                        BusinessKey = instance.BusinessKey,
                        FormData = input.ExtraData,
                        CreatedBy = input.OperatorId,
                    });
                }
                catch
                {
                    // 忽略业务数据更新失败，避免影响主流程
                }
            }
        });

        if (!tranResult.IsSuccess)
        {
            Console.WriteLine(tranResult.ErrorException?.Message);
            Console.WriteLine(tranResult.ErrorException?.StackTrace);
            throw new BusinessException("处理任务失败"+ tranResult.ErrorException);
        }
    }

    #region 同意 / 驳回辅助逻辑

    /// <summary>
    /// 根据流程节点配置解析实际审批人列表。
    /// 当前支持：
    /// - setType = 1：指定成员（nodeUserList）；
    /// - setType = 2：主管（发起人的第 examineLevel 级主管，通过 SysOrganizationService 解析）；
    /// - setType = 3：角色（nodeRoleList 对应的所有管理员，通过 SysAdminService 解析）；
    /// - setType = 7：连续多级主管（根据 directorMode/directorLevel 计算主管链）；
    /// - setType = 5：发起人自己；
    /// 其它类型暂时回退为按 nodeUserList 处理，避免流程中断。
    /// </summary>
    private async Task<List<(long UserId, string UserName)>> ResolveApproversAsync(
        WorkflowInstance instance,
        JsonElement node)
    {
        var result = new List<(long, string)>();

        // 默认类型：指定成员
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

        // 1) 指定成员：直接从 nodeUserList 解析
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

            // 此处仅处理“非顺序审批”的多级主管（examineMode != 1）：
            // 顺序审批的下一层主管任务由 HandleAgreeAsync 中的专门逻辑生成。
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

        // 其它类型（角色、自选、连续多级主管等）暂未实现：
        // 为避免流程挂起，兜底仍然尝试按 nodeUserList 解析。
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

    /// <summary>
    /// 将对象/JsonElement 提取为简单的字典，便于按字段名读取值
    /// </summary>
    private static IDictionary<string, object?> ExtractFormData(object? data)
    {
        var dict = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        if (data == null) return dict;

        if (data is JsonElement je)
        {
            if (je.ValueKind == JsonValueKind.Object)
            {
                foreach (var prop in je.EnumerateObject())
                {
                    dict[prop.Name] = prop.Value.ValueKind switch
                    {
                        JsonValueKind.String => prop.Value.GetString(),
                        JsonValueKind.Number => prop.Value.TryGetInt64(out var l)
                            ? l
                            : prop.Value.TryGetDouble(out var d) ? d : (object?)prop.Value.ToString(),
                        JsonValueKind.True => true,
                        JsonValueKind.False => false,
                        _ => prop.Value.ToString(),
                    };
                }
            }
            return dict;
        }

        if (data is IDictionary<string, object?> dictionary)
        {
            foreach (var kv in dictionary)
            {
                dict[kv.Key] = kv.Value;
            }
            return dict;
        }

        // 其它情况尝试序列化再反序列化
        try
        {
            var json = JsonSerializer.Serialize(data);
            using var doc = JsonDocument.Parse(json);
            return ExtractFormData(doc.RootElement);
        }
        catch
        {
            return dict;
        }
    }

    /// <summary>
    /// 在 route 中查找当前任务对应的节点索引：
    /// - 发起人：返回 -1，表示在第一个审批节点之前
    /// - 其它审批节点：
    ///   1）优先根据节点 Id（NodeId）精确匹配当前任务的 NodeId
    ///   2）若无 Id 或匹配失败，则根据 NodeName + nodeUserList 中是否包含当前任务的 AssigneeId 来匹配
    ///   3）如果仍找不到，则退化为第一个同名节点
    /// </summary>
    private static int FindRouteIndexForTask(IReadOnlyList<WorkflowModelHelper.RouteNode> route, WorkflowTask task)
    {
        if (string.Equals(task.NodeName, "发起人", StringComparison.Ordinal))
        {
            return -1;
        }

        var firstNameIndex = -1;

        for (var i = 0; i < route.Count; i++)
        {
            var node = route[i];

            // 1. 优先使用节点 Id 精确匹配（推荐方式，不依赖节点名称）
            if (!string.IsNullOrWhiteSpace(task.NodeId) &&
                !string.IsNullOrWhiteSpace(node.NodeId) &&
                string.Equals(node.NodeId, task.NodeId, StringComparison.Ordinal))
            {
                return i;
            }

            // 2. 节点名称不相同，则直接跳过（用于后续的名称兜底匹配）
            if (!string.Equals(node.NodeName, task.NodeName, StringComparison.Ordinal))
            {
                continue;
            }

            // 记录第一个同名节点，作为后备
            if (firstNameIndex < 0)
            {
                firstNameIndex = i;
            }

            // 尝试根据 nodeUserList 中是否包含当前任务的处理人 Id 精确匹配
            if (string.IsNullOrWhiteSpace(node.NodeJson))
            {
                continue;
            }

            try
            {
                using var doc = JsonDocument.Parse(node.NodeJson);
                var elem = doc.RootElement;
                if (elem.TryGetProperty("nodeUserList", out var arr) &&
                    arr.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in arr.EnumerateArray())
                    {
                        var id = item.TryGetProperty("id", out var idProp)
                            ? idProp.ToString()
                            : string.Empty;
                        if (long.TryParse(id, out var uid) && uid == task.AssigneeId)
                        {
                            return i;
                        }
                    }
                }
            }
            catch
            {
                // ignore parse error, fallback to name-only
            }
        }

        return firstNameIndex;
    }

    /// <summary>
    /// 同意后，基于流程模型推进到下一节点（或结束流程）
    /// </summary>
    private async Task HandleAgreeAsync(WorkflowInstance instance, WorkflowTask task, HandleTaskInput input, IDictionary<string, object?> formData)
    {
        var db = _thisRepository.Context;
        // 读取最新流程模型（与启动流程时保持一致：按 DefinitionId 获取最新，不再限定 TenantId）
        var model = await db.Queryable<WorkflowDefinitionModel>()
            .Where(x => x.DefinitionId == instance.DefinitionId)
            .OrderBy(x => x.IsLatest, OrderByType.Desc)
            .OrderBy(x => x.CreatedAt, OrderByType.Desc)
            .FirstAsync();

        if (model == null || string.IsNullOrWhiteSpace(model.ModelJson))
        {
            // 无模型：直接结束流程
            instance.Status = 1;
            instance.EndTime = DateTime.Now;
            instance.CurrentNodeIds = null;
            instance.UpdatedAt = DateTime.Now;
            await db.Updateable(instance).ExecuteCommandAsync();

            var instHistory = new WorkflowInstanceHistory
            {
                Id = Unique.Id(),
                TenantId = instance.TenantId,
                InstanceId = instance.Id,
                EventType = "Complete",
                FromStatus = 0,
                ToStatus = instance.Status,
                OperatorId = input.OperatorId,
                OperatorName = input.OperatorName,
                Remark = input.Comment ?? "同意通过",
                CreatedAt = DateTime.Now,
            };
            await db.Insertable(instHistory).ExecuteCommandAsync();
            return;
        }

        // 使用统一的模型解析器计算本次实例的审批路由（仅包含 type=1 审批节点）
        var route = WorkflowModelHelper.BuildApproverRoute(model.ModelJson, formData);
        if (route.Count == 0)
        {
            // 没有任何审批节点，直接结束
            instance.Status = 1;
            instance.EndTime = DateTime.Now;
            instance.CurrentNodeIds = null;
            instance.UpdatedAt = DateTime.Now;
            await db.Updateable(instance).ExecuteCommandAsync();
            return;
        }

        // 当前节点在 route 中的索引
        var currentIndex = FindRouteIndexForTask(route, task);

        // 连续多级主管 + 顺序审批：在所有层级完成之前，不推进到下一路由节点，
        // 而是在当前审批节点内逐层生成“下一层主管”的任务。
        if (currentIndex >= 0)
        {
            using var curDoc = JsonDocument.Parse(route[currentIndex].NodeJson);
            var curNode = curDoc.RootElement;

            var setType = 1;
            if (curNode.TryGetProperty("setType", out var stProp))
            {
                try
                {
                    setType = stProp.ValueKind == JsonValueKind.Number
                        ? stProp.GetInt32()
                        : int.Parse(stProp.GetString() ?? "1");
                }
                catch
                {
                    setType = 1;
                }
            }

            var examineMode = 2;
            if (curNode.TryGetProperty("examineMode", out var exProp))
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

            if (setType == 7 && examineMode == 1)
            {
                // 仅当当前节点为“连续多级主管 + 顺序审批”时，尝试生成下一层主管任务
                // 约定：
                // - LevelIndex 表示当前任务对应的主管层级（1=第1级主管，2=第2级主管...）；
                // - 下一层主管 = 当前 LevelIndex + 1；
                // - directorMode=0：一直到最上层主管；
                // - directorMode=1：直到 directorLevel 指定的层级。

                // 1）解析连续多级主管配置
                int? maxLevel = null;
                var directorMode = 0;
                if (curNode.TryGetProperty("directorMode", out var modeProp2))
                {
                    try
                    {
                        directorMode = modeProp2.ValueKind == JsonValueKind.Number
                            ? modeProp2.GetInt32()
                            : int.Parse(modeProp2.GetString() ?? "0");
                    }
                    catch
                    {
                        directorMode = 0;
                    }
                }

                if (directorMode == 1)
                {
                    // 自定义终点：直到发起人的第 directorLevel 级主管
                    if (curNode.TryGetProperty("directorLevel", out var lvlProp2))
                    {
                        try
                        {
                            maxLevel = lvlProp2.ValueKind == JsonValueKind.Number
                                ? lvlProp2.GetInt32()
                                : int.Parse(lvlProp2.GetString() ?? "1");
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

                // 2）当前层级与下一层级
                var currentLevel = task.LevelIndex <= 0 ? 1 : task.LevelIndex;
                var nextLevel = currentLevel + 1;

                // 如果配置了自定义终点且已经超过终点，则不再生成下一层主管，转入下一路由节点
                if (maxLevel.HasValue && nextLevel > maxLevel.Value)
                {
                    // 主管链已超出自定义终点，直接进入下一节点
                }
                else
                {
                    // 3）一次性获取主管链（直到最上层或指定层级）
                    var chain = await _organizationService.GetManagerChainByUserAsync(
                        instance.TenantId,
                        instance.StartUserId,
                        maxLevel);

                    // 如果下一层级超出实际主管链长度，则不再生成下一层主管
                    if (nextLevel <= chain.Count)
                    {
                        var (userId, userName) = chain[nextLevel - 1];
                        var name = string.IsNullOrWhiteSpace(userName)
                            ? userId.ToString()
                            : userName!;

                        // 4）为下一层主管创建新的待办任务（仍然位于当前节点）
                        var newTask = new WorkflowTask
                        {
                            TenantId = instance.TenantId,
                            InstanceId = instance.Id,
                            NodeId = task.NodeId,
                            NodeName = task.NodeName,
                            AssigneeId = userId,
                            AssigneeName = name,
                            Status = 0,
                            CreatedAt = DateTime.Now,
                            LevelIndex = nextLevel,
                        };
                        await _thisRepository.InsertAsync(newTask);
                        instance.Status = 0;
                        instance.CurrentNodeIds = newTask.NodeId;
                        instance.UpdatedAt = DateTime.Now;
                        await db.Updateable(instance).ExecuteCommandAsync();
                        return;
                    }
                }

                // 没有下一层主管（主管链已结束），继续按原有逻辑推进到下一审批节点。
            }
        }

        // 计算下一节点索引：
        // - 发起人节点：视为在第一个审批节点之前，下一步为 route[0]
        // - 其它节点：使用 currentIndex + 1；若 currentIndex < 0，则视为异常，直接结束流程（保持原有兜底）
        int nextIndex;
        if (string.Equals(task.NodeName, "发起人", StringComparison.Ordinal))
        {
            nextIndex = 0;
        }
        else
        {
            if (currentIndex < 0)
            {
                // 未找到当前节点，保守起见视为最后一个节点：直接结束
                instance.Status = 1;
                instance.EndTime = DateTime.Now;
                instance.CurrentNodeIds = null;
                instance.UpdatedAt = DateTime.Now;
                await db.Updateable(instance).ExecuteCommandAsync();
                return;
            }

            nextIndex = currentIndex + 1;
        }

        if (nextIndex >= route.Count)
        {
            // 已是最后一个审批节点：流程结束，但需要处理抄送节点（type=2）

            // 查找首个抄送节点（通过统一的模型解析辅助类）
            var ccNodeJson = WorkflowModelHelper.FindFirstCcNodeJson(model.ModelJson);
            if (!string.IsNullOrWhiteSpace(ccNodeJson))
            {
                using var ccDoc = JsonDocument.Parse(ccNodeJson);
                var ccNode = ccDoc.RootElement;
                var ccUsers = new List<(long UserId, string UserName)>();
                if (ccNode.TryGetProperty("nodeUserList", out var ccArr) &&
                    ccArr.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in ccArr.EnumerateArray())
                    {
                        var id = item.TryGetProperty("id", out var idProp) ? idProp.ToString() : string.Empty;
                        var name = item.TryGetProperty("name", out var nameProp) ? nameProp.ToString() ?? string.Empty : string.Empty;
                        if (long.TryParse(id, out var uid) && uid > 0)
                        {
                            ccUsers.Add((uid, name));
                        }
                    }
                }

                // 为抄送人创建通知任务（不再影响流程状态）
                foreach (var user in ccUsers)
                {
                    var ccTask = new WorkflowTask
                    {
                        TenantId = instance.TenantId,
                        InstanceId = instance.Id,
                        NodeId = "抄送人",
                        NodeName = "抄送人",
                        AssigneeId = user.UserId,
                        AssigneeName = user.UserName,
                        Status = 0,
                        CreatedAt = DateTime.Now,
                    };
                    await _thisRepository.InsertAsync(ccTask);
                }
            }

            // 标记流程完成
            instance.Status = 1;
            instance.EndTime = DateTime.Now;
            instance.CurrentNodeIds = null;
            instance.UpdatedAt = DateTime.Now;
            await db.Updateable(instance).ExecuteCommandAsync();

            var instHistory = new WorkflowInstanceHistory
            {
                Id = Unique.Id(),
                TenantId = instance.TenantId,
                InstanceId = instance.Id,
                EventType = "Complete",
                FromStatus = 0,
                ToStatus = instance.Status,
                OperatorId = input.OperatorId,
                OperatorName = input.OperatorName,
                Remark = input.Comment ?? "同意通过",
                CreatedAt = DateTime.Now,
            };
            await db.Insertable(instHistory).ExecuteCommandAsync();
            return;
        }

        // 还有下一审批节点：为该节点创建待办任务（支持多种人员类型解析）
        var nextNodeName = route[nextIndex].NodeName;
        using var nextDoc = JsonDocument.Parse(route[nextIndex].NodeJson);
        var nextNode = nextDoc.RootElement;

        var assignees = await ResolveApproversAsync(instance, nextNode);

        if (assignees.Count == 0)
        {
            // 如果未配置具体人员，则直接结束流程，避免挂起
            instance.Status = 1;
            instance.EndTime = DateTime.Now;
            instance.CurrentNodeIds = null;
            instance.UpdatedAt = DateTime.Now;
            await db.Updateable(instance).ExecuteCommandAsync();
            return;
        }

        var nodeIds = new List<string>();
        foreach (var user in assignees)
        {
            var newTask = new WorkflowTask
            {
                TenantId = instance.TenantId,
                InstanceId = instance.Id,
                NodeId = nextNodeName,
                NodeName = nextNodeName,
                AssigneeId = user.UserId,
                AssigneeName = user.UserName,
                Status = 0,
                CreatedAt = DateTime.Now,
            };
            await _thisRepository.InsertAsync(newTask);
            nodeIds.Add(newTask.NodeId);
        }

        instance.Status = 0; // 仍然运行中
        instance.CurrentNodeIds = string.Join(",", nodeIds.Distinct());
        instance.UpdatedAt = DateTime.Now;
        await db.Updateable(instance).ExecuteCommandAsync();
    }

    /// <summary>
    /// 驳回逻辑：根据模式退回到发起人 / 上一节点 / 指定节点。
    /// 当前实现为：创建新任务分配给目标节点，对应节点的历史记录仍然保留。
    /// </summary>
    private async Task HandleRejectAsync( WorkflowInstance instance, WorkflowTask task, HandleTaskInput input, IDictionary<string, object?> formData)
    {
        var db = _thisRepository.Context;
        var mode = (input.RejectMode ?? string.Empty).ToUpperInvariant();

        // 未指定模式，视为终止流程
        if (string.IsNullOrEmpty(mode))
        {
            instance.Status = 2;
            instance.EndTime = DateTime.Now;
            instance.CurrentNodeIds = null;
            instance.UpdatedAt = DateTime.Now;
            await db.Updateable(instance).ExecuteCommandAsync();

            var instHistoryTerminate = new WorkflowInstanceHistory
            {
                Id = Unique.Id(),
                TenantId = instance.TenantId,
                InstanceId = instance.Id,
                EventType = "Reject",
                FromStatus = 0,
                ToStatus = instance.Status,
                OperatorId = input.OperatorId,
                OperatorName = input.OperatorName,
                Remark = input.Comment ?? "驳回并终止流程",
                CreatedAt = DateTime.Now,
            };
            await db.Insertable(instHistoryTerminate).ExecuteCommandAsync();
            return;
        }

        // 读取最新流程模型，构建执行路径（同样仅按 DefinitionId 获取最新）
        var model = await db.Queryable<WorkflowDefinitionModel>()
            .Where(x => x.DefinitionId == instance.DefinitionId)
            .OrderBy(x => x.IsLatest, OrderByType.Desc)
            .OrderBy(x => x.CreatedAt, OrderByType.Desc)
            .FirstAsync();

        var route = model != null && !string.IsNullOrWhiteSpace(model.ModelJson)
            ? WorkflowModelHelper.BuildApproverRoute(model.ModelJson, formData)
            : new List<WorkflowModelHelper.RouteNode>();

        // 目标节点名称
        string? targetNodeName = null;

        if (mode == "START")
        {
            // 发起人：虚拟节点，NodeName 固定为 "发起人"
            targetNodeName = "发起人";
        }
        else
        {
            // 先找到当前节点在 route 中的位置（使用 NodeName + AssigneeId 匹配）
            var currentIndex = FindRouteIndexForTask(route, task);

            if (mode == "PREVIOUS")
            {
                if (currentIndex > 0)
                {
                    targetNodeName = route[currentIndex - 1].NodeName;
                }
                else
                {
                    // 已经是第一审批节点，则退回发起人
                    targetNodeName = "发起人";
                }
            }
            else if (mode == "SPECIFIC" && !string.IsNullOrWhiteSpace(input.RejectToNodeId))
            {
                targetNodeName = input.RejectToNodeId;
            }
        }

        if (string.IsNullOrWhiteSpace(targetNodeName))
        {
            // 未能找到目标节点，保守终止流程
            instance.Status = 2;
            instance.EndTime = DateTime.Now;
            instance.CurrentNodeIds = null;
            instance.UpdatedAt = DateTime.Now;
            await db.Updateable(instance).ExecuteCommandAsync();
            return;
        }

        // 发起人节点：创建一个任务给发起人
        if (targetNodeName == "发起人")
        {
            var startTask = new WorkflowTask
            {
                TenantId = instance.TenantId,
                InstanceId = instance.Id,
                NodeId = "发起人",
                NodeName = "发起人",
                AssigneeId = instance.StartUserId,
                AssigneeName = instance.StartUserName,
                Status = 0,
                CreatedAt = DateTime.Now,
            };
            await _thisRepository.InsertAsync(startTask);

            instance.Status = 0;
            instance.CurrentNodeIds = startTask.NodeId;
            instance.UpdatedAt = DateTime.Now;
            await db.Updateable(instance).ExecuteCommandAsync();

            var instHistory = new WorkflowInstanceHistory
            {
                Id = Unique.Id(),
                TenantId = instance.TenantId,
                InstanceId = instance.Id,
                EventType = "RejectToStart",
                FromStatus = 0,
                ToStatus = instance.Status,
                OperatorId = input.OperatorId,
                OperatorName = input.OperatorName,
                Remark = input.Comment ?? "驳回到发起人",
                CreatedAt = DateTime.Now,
            };
            await db.Insertable(instHistory).ExecuteCommandAsync();
            return;
        }

        // 其它节点：根据 route 中的节点定义，重新创建待办任务
        var target = route.FirstOrDefault(x =>
            string.Equals(x.NodeName, targetNodeName, StringComparison.Ordinal));
        if (string.IsNullOrWhiteSpace(target.NodeJson))
        {
            // 找不到目标节点，保守终止流程
            instance.Status = 2;
            instance.EndTime = DateTime.Now;
            instance.CurrentNodeIds = null;
            instance.UpdatedAt = DateTime.Now;
            await db.Updateable(instance).ExecuteCommandAsync();
            return;
        }

        using var nodeDoc = JsonDocument.Parse(target.NodeJson);
        var node = nodeDoc.RootElement;
        var assignees = await ResolveApproversAsync(instance, node);

        if (assignees.Count == 0)
        {
            // 没有配置节点处理人，同样保守终止流程
            instance.Status = 2;
            instance.EndTime = DateTime.Now;
            instance.CurrentNodeIds = null;
            instance.UpdatedAt = DateTime.Now;
            await db.Updateable(instance).ExecuteCommandAsync();
            return;
        }

        var nodeIds = new List<string>();
        foreach (var user in assignees)
        {
            var newTask = new WorkflowTask
            {
                TenantId = instance.TenantId,
                InstanceId = instance.Id,
                NodeId = targetNodeName,
                NodeName = targetNodeName,
                AssigneeId = user.UserId,
                AssigneeName = user.UserName,
                Status = 0,
                CreatedAt = DateTime.Now,
            };
            await db.Insertable(newTask).ExecuteCommandAsync();
            nodeIds.Add(newTask.NodeId);
        }

        instance.Status = 0;
        instance.CurrentNodeIds = string.Join(",", nodeIds.Distinct());
        instance.UpdatedAt = DateTime.Now;
        await db.Updateable(instance).ExecuteCommandAsync();

            var instHistory2 = new WorkflowInstanceHistory
            {
                Id = Unique.Id(),
                TenantId = instance.TenantId,
                InstanceId = instance.Id,
                EventType = "RejectToNode",
                FromStatus = 0,
                ToStatus = instance.Status,
                OperatorId = input.OperatorId,
                OperatorName = input.OperatorName,
                Remark = input.Comment ?? $"驳回到节点：{targetNodeName}",
                CreatedAt = DateTime.Now,
            };
            await db.Insertable(instHistory2).ExecuteCommandAsync();
    }

    #endregion
}
