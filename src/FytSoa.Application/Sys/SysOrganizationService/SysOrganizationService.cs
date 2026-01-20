using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FytSoa.Common.Param;
using FytSoa.Common.Result;
using FytSoa.Domain.Sys;
using FytSoa.Sugar;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;

namespace FytSoa.Application.Sys;

/// <summary>
/// 组织与任用管理服务。
/// </summary>
[ApiExplorerSettings(GroupName = "v1")]
public class SysOrganizationService : IApplicationService
{
    private readonly SugarRepository<SysOrgUnit> _orgUnits;
    private readonly SugarRepository<SysOrgUnitClosure> _closures;
    private readonly SugarRepository<SysPosition> _positions;
    private readonly SugarRepository<SysEmployment> _employment;
    private readonly SugarRepository<SysEmploymentReporting> _reporting;
    private readonly SugarRepository<SysOrgUnitHead> _heads;
    private readonly SugarRepository<SysAdmin> _admin;

    public SysOrganizationService(ISqlSugarClient db)
    {
        _orgUnits = new SugarRepository<SysOrgUnit>(db);
        _closures = new SugarRepository<SysOrgUnitClosure>(db);
        _positions = new SugarRepository<SysPosition>(db);
        _employment = new SugarRepository<SysEmployment>(db);
        _reporting = new SugarRepository<SysEmploymentReporting>(db);
        _heads = new SugarRepository<SysOrgUnitHead>(db);
        _admin= new SugarRepository<SysAdmin>(db);
    }

    /// <summary>
    /// 查询组织列表。
    /// </summary>
    public Task<List<SysOrgUnit>> GetOrgUnitsAsync(long tenantId) =>
        _orgUnits.AsQueryable()
            .Where(o => o.TenantId == tenantId)
            .OrderBy(o => o.Id)
            .ToListAsync();

    /// <summary>
    /// 分页查询组织。
    /// </summary>
    public async Task<PageResult<SysOrgUnit>> GetOrgUnitPageAsync(PageParam param)
    {
        var query = _orgUnits.AsQueryable();
        if (!string.IsNullOrEmpty(param.Key))
        {
            query = query.Where(o => o.Name.Contains(param.Key) || o.Code.Contains(param.Key));
        }
        return  await query
            .OrderBy(o => o.Id)
            .ToPageAsync(param.Page, param.Limit);
    }
    
    /// <summary>
    /// 查询组织。
    /// </summary>
    public async Task<List<SysOrgUnit>> GetOrgUnitListAsync(WhereParam param)
    {
        var query = _orgUnits.AsQueryable();
        if (!string.IsNullOrEmpty(param.Key))
        {
            query = query.Where(o => o.Name.Contains(param.Key) || o.Code.Contains(param.Key));
        }
        return  await query
            .OrderBy(o => o.Id)
            .ToListAsync();
    }

    /// <summary>
    /// 新建组织并维护闭包表。
    /// </summary>
    public async Task<long> CreateOrgUnitAsync(SysOrgUnit unit)
    {
        ArgumentNullException.ThrowIfNull(unit);
        unit.CreatedAt = DateTime.UtcNow;
        unit.UpdatedAt = unit.CreatedAt;

        var newId = await _orgUnits.InsertReturnIdentityAsync(unit);
        var closureRows = new List<SysOrgUnitClosure>
        {
            new SysOrgUnitClosure
            {
                TenantId = unit.TenantId,
                AncestorId = newId,
                DescendantId = newId,
                Depth = 0
            }
        };

        if (unit.ParentId!=0)
        {
            var ancestors = await _closures.AsQueryable()
                .Where(c => c.TenantId == unit.TenantId && c.DescendantId == unit.ParentId)
                .ToListAsync();

            foreach (var ancestor in ancestors)
            {
                closureRows.Add(new SysOrgUnitClosure
                {
                    TenantId = unit.TenantId,
                    AncestorId = ancestor.AncestorId,
                    DescendantId = newId,
                    Depth = ancestor.Depth + 1
                });
            }
        }

        await _closures.AsInsertable(closureRows).ExecuteCommandAsync();
        return newId;
    }

    /// <summary>
    /// 更新组织信息。
    /// </summary>
    public async Task<bool> UpdateOrgUnitAsync(SysOrgUnit unit)
    {
        ArgumentNullException.ThrowIfNull(unit);
        unit.UpdatedAt = DateTime.UtcNow;
        return await _orgUnits.UpdateAsync(unit);
    }

    /// <summary>
    /// 删除组织，需确保没有子节点。
    /// </summary>
    public async Task<bool> DeleteOrgUnitAsync(long orgUnitId)
    {
        var hasChildren = await _closures.AsQueryable()
            .Where(c => c.AncestorId == orgUnitId && c.Depth > 0)
            .AnyAsync();
        if (hasChildren)
        {
            throw new InvalidOperationException("Can not delete organization with descendants.");
        }

        await _closures.AsDeleteable()
            .Where(c => c.DescendantId == orgUnitId)
            .ExecuteCommandAsync();

        return await _orgUnits.DeleteByIdAsync(orgUnitId);
    }

    /// <summary>
    /// 获取闭包表数据。
    /// </summary>
    public Task<List<SysOrgUnitClosure>> GetClosuresAsync(long tenantId) =>
        _closures.AsQueryable()
            .Where(c => c.TenantId == tenantId)
            .ToListAsync();

    /// <summary>
    /// 创建岗位。
    /// </summary>
    public async Task<long> CreatePositionAsync(SysPosition position)
    {
        ArgumentNullException.ThrowIfNull(position);
        position.CreatedAt = DateTime.UtcNow;
        position.UpdatedAt = position.CreatedAt;
        return await _positions.InsertReturnIdentityAsync(position);
    }

    /// <summary>
    /// 更新岗位。
    /// </summary>
    public async Task<bool> UpdatePositionAsync(SysPosition position)
    {
        ArgumentNullException.ThrowIfNull(position);
        position.UpdatedAt = DateTime.UtcNow;
        return await _positions.UpdateAsync(position);
    }
    
    /// <summary>
    /// 删除岗位
    /// </summary>
    public async Task<bool> DeletePositionAsync(long positionId)
    {
        return await _positions.DeleteByIdAsync(positionId);
    }

    /// <summary>
    /// 分页查询岗位。
    /// </summary>
    public async Task<PageResult<SysPosition>> GetPositionPageAsync(PageParam param)
    {
        var query = _positions.AsQueryable();
        if (!string.IsNullOrEmpty(param.Key))
        {
            query = query.Where(p => p.Name.Contains(param.Key) || p.Code.Contains(param.Key));
        }

        return await query
            .OrderBy(p => p.Id)
            .ToPageAsync(param.Page,param.Limit);
    }

    /// <summary>
    /// 获取岗位列表。
    /// </summary>
    public Task<List<SysPosition>> GetPositionsAsync() =>
        _positions.AsQueryable()
            .OrderBy(p => p.Id)
            .ToListAsync();

    /// <summary>
    /// 创建任用关联。
    /// </summary>
    [UnitOfWork]
    public async Task<long> CreateEmploymentAsync(SysEmployment employment)
    {
        ArgumentNullException.ThrowIfNull(employment);
        employment.CreatedAt = DateTime.UtcNow;
        employment.UpdatedAt = employment.CreatedAt;
        
        await _admin.AsUpdateable().SetColumns(m => m.OrganizeId == employment.OrgId)
            .Where(m => m.Id == employment.UserId)
            .ExecuteCommandAsync();
        return await _employment.InsertReturnIdentityAsync(employment);
    }

    /// <summary>
    /// 更新任用关系。
    /// </summary>
    [UnitOfWork]
    public async Task<bool> UpdateEmploymentAsync(SysEmployment employment)
    {
        ArgumentNullException.ThrowIfNull(employment);
        employment.UpdatedAt = DateTime.UtcNow;
        
        await _admin.AsUpdateable().SetColumns(m => m.OrganizeId == employment.OrgId)
            .Where(m => m.Id == employment.UserId)
            .ExecuteCommandAsync();
        return await _employment.UpdateAsync(employment);
    }
    
    /// <summary>
    /// 删除任用关系
    /// </summary>
    [HttpDelete("{employmentId}"),UnitOfWork]
    public async Task<bool> DeleteEmploymentAsync(long employmentId)
    {
        await _admin.AsUpdateable().SetColumns(m => m.OrganizeId == 0)
            .Where(m => m.Id == employmentId)
            .ExecuteCommandAsync();
        return await _employment.DeleteByIdAsync(employmentId);
    }
    
    /// <summary>
    /// 获取任用记录，可选指定用户-分页。
    /// </summary>
    public async Task<PageResult<SysEmployment>> GetEmploymentPageAsync(PageParam param)
    {
        var query = _employment.AsQueryable();
        if (param.Id!=0)
        {
            query = query.Where(e => e.UserId == param.Id);
        }
        return await query
            .Includes(m=>m.User)
            .Includes(m=>m.Org)
            .Includes(m=>m.Position)
            .ToPageAsync(param.Page, param.Limit);
    }

    /// <summary>
    /// 获取任用记录，可选指定用户。
    /// </summary>
    public async Task<List<SysEmployment>> GetEmploymentListAsync(WhereParam param)
    {
        var query = _employment.AsQueryable();
        if (param.Id!=0)
        {
            query = query.Where(e => e.UserId == param.Id);
        }
        return await query
            .Includes(m=>m.User)
            .Includes(m=>m.Org)
            .Includes(m=>m.Position)
            .ToListAsync();
    }

    /// <summary>
    /// 兼容旧方法，按用户获取任用记录。
    /// </summary>
    public Task<List<SysEmployment>> GetEmploymentByUserAsync(long userId) =>
        GetEmploymentListAsync(new WhereParam(){Id = userId});

    /// <summary>
    /// 创建汇报关系。
    /// </summary>
    public async Task<long> CreateReportingAsync(SysEmploymentReporting reporting)
    {
        ArgumentNullException.ThrowIfNull(reporting);
        reporting.CreatedAt = DateTime.UtcNow;
        reporting.UpdatedAt = reporting.CreatedAt;
        return await _reporting.InsertReturnIdentityAsync(reporting);
    }

    /// <summary>
    /// 更新汇报关系。
    /// </summary>
    public async Task<bool> UpdateReportingAsync(SysEmploymentReporting reporting)
    {
        ArgumentNullException.ThrowIfNull(reporting);
        var existing = await _reporting.GetByIdAsync(reporting.Id)
                       ?? throw new InvalidOperationException($"Reporting relation {reporting.Id} does not exist.");

        existing.SubordinateEmploymentId = reporting.SubordinateEmploymentId;
        existing.ManagerEmploymentId = reporting.ManagerEmploymentId;
        existing.Relation = reporting.Relation;
        existing.ValidFrom = reporting.ValidFrom;
        existing.ValidTo = reporting.ValidTo;
        existing.Note = reporting.Note;
        existing.UpdatedAt = DateTime.UtcNow;

        return await _reporting.UpdateAsync(existing);
    }

    /// <summary>
    /// 获取汇报关系列表。
    /// </summary>
    public Task<PageResult<SysEmploymentReporting>> GetReportingListAsync(long? subordinateEmploymentId = null,
        long? managerEmploymentId = null,
        long? employmentId = null,
        string? relation = null,int page=1,int limit = 20)
    {
        var query = _reporting.AsQueryable();
        if (subordinateEmploymentId.HasValue)
        {
            query = query.Where(r => r.SubordinateEmploymentId == subordinateEmploymentId.Value);
        }

        if (managerEmploymentId.HasValue)
        {
            query = query.Where(r => r.ManagerEmploymentId == managerEmploymentId.Value);
        }

        if (employmentId.HasValue)
        {
            query = query.Where(r => r.ManagerEmploymentId == employmentId.Value || r.SubordinateEmploymentId == employmentId.Value);
        }
        if (!string.IsNullOrWhiteSpace(relation))
        {
            query = query.Where(r => r.Relation == relation);
        }

        return query.OrderBy(r => r.ValidFrom)
            .Includes(m=>m.managerUser)
            .Includes(m=>m.subordinateUser)
            .ToPageAsync(page,limit);
    }

    /// <summary>
    /// 删除汇报关系。
    /// </summary>
    [HttpDelete("{id}")]
    public Task<bool> DeleteReportingAsync(long id) => _reporting.DeleteByIdAsync(id);

    /// <summary>
    /// 创建组织负责人信息。
    /// </summary>
    public async Task<long> CreateOrgHeadAsync(SysOrgUnitHead head)
    {
        ArgumentNullException.ThrowIfNull(head);
        head.CreatedAt = DateTime.UtcNow;
        head.UpdatedAt = head.CreatedAt;
        return await _heads.InsertReturnIdentityAsync(head);
    }

    /// <summary>
    /// 更新组织负责人信息。
    /// </summary>
    public async Task<bool> UpdateOrgHeadAsync(SysOrgUnitHead head)
    {
        ArgumentNullException.ThrowIfNull(head);
        var existing = await _heads.GetByIdAsync(head.Id)
                       ?? throw new InvalidOperationException($"Org head {head.Id} does not exist.");

        existing.OrgId = head.OrgId;
        existing.EmploymentId = head.EmploymentId;
        existing.HeadType = head.HeadType;
        existing.ValidFrom = head.ValidFrom;
        existing.ValidTo = head.ValidTo;
        existing.Note = head.Note;
        existing.UpdatedAt = DateTime.UtcNow;

        return await _heads.UpdateAsync(existing);
    }

    /// <summary>
    /// 获取组织负责人列表。
    /// </summary>
    public Task<PageResult<SysOrgUnitHead>> GetOrgHeadsAsync(long tenantId, long? orgId = null, string? headType = null
    ,int page = 1,int limit = 20)
    {
        var query = _heads.AsQueryable();
        if (orgId.HasValue)
        {
            query = query.Where(h => h.OrgId == orgId.Value);
        }

        if (!string.IsNullOrWhiteSpace(headType))
        {
            query = query.Where(h => h.HeadType == headType);
        }

        return query.OrderBy(h => h.ValidFrom)
            .Includes(m=>m.Employment)
            .ToPageAsync(page,limit);
    }
    
    /// <summary>
    /// 删除负责人，兼容命名。
    /// </summary>
    [HttpDelete("{id}")]
    public Task<bool> DeleteOrgHeadAsync(long id) => _heads.DeleteByIdAsync(id);
    
    #region 工作流集成：查找用户的 N 级主管

    /// <summary>
    /// 根据用户和层级，查找其第 N 级主管。
    /// 说明：
    /// - level=1 表示“直接主管”，level=2 表示“直接主管的主管”，以此类推；
    /// - 当前实现基于任用表 + 汇报关系表（SysEmployment / SysEmploymentReporting）逐级向上查找；
    /// - 如果中途某一层找不到主管，则返回最后一次成功找到的主管；若一层都找不到则返回 null；
    /// - 为避免循环引用，仅返回账号 Id 与账号名（SysUserAccount.Id / UserName）。
    /// </summary>
    /// <param name="tenantId">租户 Id，用于限定账号表</param>
    /// <param name="userId">员工账号 Id（SysUserAccount.Id）</param>
    /// <param name="level">第几级主管，最小为 1</param>
    /// <returns>找到时返回 (UserId, UserName)，否则返回 null</returns>
    public async Task<(long UserId, string? UserName)?> GetNthManagerByUserAsync(long tenantId, long userId, int level)
    {
        if (level <= 0)
        {
            level = 1;
        }

        // 1. 找到该用户的一条任用记录（当前实现：简单取第一条）
        var employment = await _employment.AsQueryable()
            .Where(e => e.UserId == userId)
            .FirstAsync();

        if (employment == null)
        {
            return null;
        }

        // 当前任用记录 Id
        var currentEmploymentId = employment.Id;
        SysEmployment? lastManagerEmployment = null;

        // 2. 逐级向上查找主管：通过汇报关系 SubordinateEmploymentId -> ManagerEmploymentId
        for (var i = 0; i < level; i++)
        {
            var reporting = await _reporting.AsQueryable()
                .Where(r => r.SubordinateEmploymentId == currentEmploymentId)
                .FirstAsync();

            if (reporting == null)
            {
                // 当前层级没有配置汇报关系，停止向上查找
                break;
            }

            var managerEmployment = await _employment.GetByIdAsync(reporting.ManagerEmploymentId);
            if (managerEmployment == null)
            {
                break;
            }

            lastManagerEmployment = managerEmployment;
            currentEmploymentId = managerEmployment.Id;
        }

        if (lastManagerEmployment == null)
        {
            return null;
        }

        var managerUserId = lastManagerEmployment.UserId;

        // 3. 读取账号名称（UserName），仅用于展示，不强依赖
        var account = await _admin.AsQueryable()
            .Where(a =>  a.Id == managerUserId)
            .FirstAsync();

        var managerUserName = account?.FullName;
        return (managerUserId, managerUserName);
    }

    /// <summary>
    /// 根据用户，按汇报关系向上构建“主管链”：
    /// - level=1 表示直接主管，2 表示第二级主管，以此类推；
    /// - maxLevel=null 时，表示一直查到最上层主管；
    /// - 返回结果中，列表顺序为从“直接主管”开始依次向上。
    /// </summary>
    /// <param name="tenantId">租户 Id</param>
    /// <param name="userId">员工账号 Id（对应 SysEmployment.UserId）</param>
    /// <param name="maxLevel">最大层级；为 null 时表示不限层级</param>
    public async Task<List<(long UserId, string? UserName)>> GetManagerChainByUserAsync(
        long tenantId,
        long userId,
        int? maxLevel)
    {
        var result = new List<(long, string?)>();

        if (maxLevel.HasValue && maxLevel.Value <= 0)
        {
            return result;
        }

        // 1. 找到该用户的一条任用记录（取第一条）
        var employment = await _employment.AsQueryable()
            .Where(e => e.UserId == userId)
            .FirstAsync();

        if (employment == null)
        {
            return result;
        }

        var currentEmploymentId = employment.Id;
        int level = 0;

        // 2. 逐级向上查找主管，直到达到 maxLevel 或无更多汇报关系
        while (true)
        {
            var reporting = await _reporting.AsQueryable()
                .Where(r => r.SubordinateEmploymentId == currentEmploymentId)
                .FirstAsync();

            if (reporting == null)
            {
                break;
            }

            var managerEmployment = await _employment.GetByIdAsync(reporting.ManagerEmploymentId);
            if (managerEmployment == null)
            {
                break;
            }

            level++;

            var managerUserId = managerEmployment.UserId;
            var account = await _admin.AsQueryable()
                .Where(a => a.TenantId == tenantId && a.Id == managerUserId)
                .FirstAsync();

            result.Add((managerUserId, account?.FullName));

            currentEmploymentId = managerEmployment.Id;

            if (maxLevel.HasValue && level >= maxLevel.Value)
            {
                break;
            }
        }

        return result;
    }

    #endregion
}
