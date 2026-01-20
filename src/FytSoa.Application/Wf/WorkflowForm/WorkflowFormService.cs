using SqlSugar;
using Mapster;
using FytSoa.Domain.Wf;
using FytSoa.Sugar;
using Microsoft.AspNetCore.Mvc;

namespace FytSoa.Application.Wf;

/// <summary>
/// 工作流：表单定义应用服务
/// </summary>
[ApiExplorerSettings(GroupName = "v10")]
public class WorkflowFormService : IApplicationService
{
    /// <summary>
    /// 仓储对象，用于对表单定义表进行增删改查
    /// </summary>
    private readonly SugarRepository<WorkflowForm> _thisRepository;

    /// <summary>
    /// 构造函数，注入当前实体对应的仓储
    /// </summary>
    /// <param name="thisRepository">表单定义仓储</param>
    public WorkflowFormService(SugarRepository<WorkflowForm> thisRepository)
    {
        _thisRepository = thisRepository;
    }

    /// <summary>
    /// 根据 Id 获取表单定义
    /// </summary>
    public async Task<WorkflowFormDto?> GetAsync(long id)
    {
        var entity = await _thisRepository.Context
            .Queryable<WorkflowForm>()
            .FirstAsync(x => x.Id == id);

        return entity?.Adapt<WorkflowFormDto>();
    }

    /// <summary>
    /// 根据租户简单查询表单列表（按关键字 / 状态）
    /// </summary>
    public async Task<List<WorkflowFormDto>> GetListAsync(
        long tenantId,
        string? keyword = null,
        byte? status = null)
    {
        var query = _thisRepository.Context.Queryable<WorkflowForm>()
            .Where(x => x.TenantId == tenantId);

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            query = query.Where(x =>
                x.Code.Contains(keyword!) || x.Name.Contains(keyword!));
        }

        if (status.HasValue)
        {
            query = query.Where(x => x.Status == status.Value);
        }

        var list = await query
            .OrderBy(x => x.Code).OrderByDescending(x => x.Name)
            .ToListAsync();

        return list.Adapt<List<WorkflowFormDto>>();
    }

    /// <summary>
    /// 创建表单定义（默认状态：草稿）
    /// </summary>
    public async Task<long> CreateAsync(WorkflowFormDto input)
    {
        var entity = input.Adapt<WorkflowForm>();
        entity.Status = entity.Status == 0 ? (byte)0 : entity.Status;

        await _thisRepository.InsertAsync(entity);
        return entity.Id;
    }

    /// <summary>
    /// 更新表单定义
    /// </summary>
    public async Task UpdateAsync(WorkflowFormDto input)
    {
        var entity = input.Adapt<WorkflowForm>();
        await _thisRepository.UpdateAsync(entity);
    }

    /// <summary>
    /// 删除表单定义（简单物理删除）
    /// </summary>
    public async Task DeleteAsync(long id)
    {
        await _thisRepository.DeleteAsync(x => x.Id == id);
    }
}

