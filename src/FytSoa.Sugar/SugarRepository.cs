using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using SqlSugar;
using FytSoa.Common.Enum;
using FytSoa.Common.Utils;
using FytSoa.Domain;
using FytSoa.Common.Jwt;
using FytSoa.Common.Tenant;

namespace FytSoa.Sugar;

public class SugarRepository<T> : SimpleClient<T> where T : class ,new()
{
    public SugarRepository(ISqlSugarClient? context = null) : base(context)
    {
        // 兼容后台任务：优先使用 DI 注入的 ISqlSugarClient，避免依赖 HttpContext。
        Context = context ?? AppUtils.GetService<ISqlSugarClient>();
        if (Context == null) return;
        #region 处理租户数据过滤
        /*foreach(var entityType in TenantProperties.DomainEntity){
            var lambda = DynamicExpressionParser.ParseLambda
            (new[] { Expression.Parameter(entityType, "it") },
                typeof(bool), nameof(EntityBase.TenantId)+"="+tenantId,
                false);
            Context.QueryFilter.Add(new TableFilterItem<object>(entityType, lambda , true));
        }*/

        #endregion
        //Console.WriteLine("租户编号："+token.TenantId);
        Context.Aop.DataExecuting = (oldValue, entityInfo) =>
        {
            // 非请求上下文（Quartz/后台任务）不做自动填充，避免 TenantId 被错误覆盖为 0
            if (AppUtils.HttpContext == null) return;

            var tenantId = AppUtils.TenantId;
            var loginUser = AppUtils.LoginUser;

            //新增操作
            if (entityInfo.OperationType == DataFilterType.InsertByObject 
                && !TenantProperties.IgnoreAddEntity.Contains(entityInfo.EntityName))
            {
                if (entityInfo.EntityColumnInfo.IsPrimarykey &&
                    entityInfo.EntityColumnInfo.PropertyInfo.PropertyType == typeof(long))
                {
                    var keyValue = (dynamic)entityInfo.EntityValue;
                    if (keyValue.Id == null || keyValue.Id == 0)
                    {
                        entityInfo.SetValue(Unique.Id());
                    }
                }

                if (entityInfo.PropertyName == "CreateUser")
                {
                    entityInfo.SetValue(loginUser);
                }
                
                if (tenantId==0 && entityInfo.PropertyName == "TenantId")
                {
                    if (!TenantProperties.AddTenantIgnoreTable.Contains(entityInfo.EntityName))
                    {
                        entityInfo.SetValue(tenantId);
                    }
                }
            }

            //更新操作
            if (entityInfo.OperationType != DataFilterType.UpdateByObject) return;
            if (entityInfo.PropertyName == "UpdateUser")
            {
                entityInfo.SetValue(loginUser);
            }

            if (entityInfo.PropertyName == "UpdateTime")
            {
                entityInfo.SetValue(DateTime.Now);
            }
            
            if (entityInfo.PropertyName == "TenantId")
            {
                entityInfo.SetValue(tenantId);
            }
        };
        /*Context.Aop.OnLogExecuting = (s, p) =>
        {
            var sqlValue = string.Empty;
            foreach (var item in p)
            {
                sqlValue += item.ParameterName + "参数：" + item.Value + "\r\n";
            }
            Console.WriteLine("Sql脚本："+s+"\r\n"+string.Join(",", p.Select(it => it.ParameterName + ":" + it.Value)));
            Logger.Info("Sql脚本："+s+"\r\n"+sqlValue);
        };*/
    }

    /// <summary>
    /// 分页查询
    /// </summary>
    /// <param name="where">条件</param>
    /// <param name="page">当前页</param>
    /// <param name="limit">每页条数</param>
    /// <param name="order">排序</param>
    /// <param name="orderEnum">枚举，1=desc 2=asc</param>
    /// <returns>集合、总条数、总页数</returns>
    public async Task<(List<T>,int totalItem,int totalPage)> GetPageResultAsync(Expression<Func<T, bool>> where, int page, int limit, Expression<Func<T, object>> order, OrderEnum orderEnum = OrderEnum.Desc)
    {
        RefAsync<int> totalItems = 0;
        var list = await Context.Queryable<T>()
            .Where(where)
            .OrderBy(order,(int)orderEnum==1 ? OrderByType.Desc : OrderByType.Asc)
            .ToPageListAsync(page, limit, totalItems);
        var sumPage = totalItems != 0 ? (totalItems % page) == 0 ? (totalItems / limit) : (totalItems / limit) + 1 : 0;
        return (list,totalItems,sumPage);
    }
        
    /// <summary>
    /// 分页查询
    /// </summary>
    /// <param name="where">Expression 条件</param>
    /// <param name="strWhere">string 条件</param>
    /// <param name="page">当前页</param>
    /// <param name="limit">每页条数</param>
    /// <param name="order">排序</param>
    /// <param name="orderEnum">枚举，1=desc 2=asc</param>
    /// <returns>集合、总条数、总页数</returns>
    public async Task<(List<T>,int totalItem,int totalPage)> GetPageResultAsync(Expression<Func<T, bool>> where, string strWhere , int page, int limit, Expression<Func<T, object>> order, OrderEnum orderEnum = OrderEnum.Desc)
    {
        RefAsync<int> totalItems = 0;
        var list = await Context.Queryable<T>()
            .Where(where)
            .WhereIF(!string.IsNullOrEmpty(strWhere),strWhere)
            .OrderBy(order,(int)orderEnum==1 ? OrderByType.Desc : OrderByType.Asc)
            .ToPageListAsync(page, limit, totalItems);
        var sumPage = totalItems != 0 ? (totalItems % page) == 0 ? (totalItems / limit) : (totalItems / limit) + 1 : 0;
        return (list,totalItems,sumPage);
    }

    /// <summary>
    /// 根据条件，获得最新的一条数据
    /// </summary>
    /// <param name="where">拉姆达条件</param>
    /// <param name="order">拉姆达排序</param>
    /// <param name="orderEnum">枚举，1=desc 2=asc</param>
    /// <returns></returns>
    public async Task<T> GetFirstAsync(Expression<Func<T, bool>> where, Expression<Func<T, object>> order, OrderEnum orderEnum = OrderEnum.Desc)
    {
        return await Context.Queryable<T>()
            .Where(where)
            .OrderBy(order, (int)orderEnum == 1 ? OrderByType.Desc : OrderByType.Asc)
            .FirstAsync() ?? new T();
    }

    /// <summary>
    /// 根据条件查询列表
    /// </summary>
    /// <param name="where">拉姆达条件</param>
    /// <param name="order">拉姆达排序</param>
    /// <param name="orderEnum">枚举，1=desc 2=asc</param>
    /// <returns></returns>
    public async Task<List<T>> GetListAsync(Expression<Func<T, bool>> where,
        Expression<Func<T, object>> order, OrderEnum orderEnum = OrderEnum.Desc)
    {
        var query = Context.Queryable<T>()
            .Where(where)
            .OrderBy(order, (int)orderEnum == 1 ? OrderByType.Desc : OrderByType.Asc);
        return await query.ToListAsync();
    }

    /// <summary>
    /// 根据条件查询列表
    /// </summary>
    /// <param name="where">拉姆达条件</param>
    /// <param name="order">order by id desc</param>
    /// <returns></returns>
    public async Task<List<T>> GetListAsync(Expression<Func<T, bool>> where,
        string order)
    {
        var query = Context.Queryable<T>()
            .Where(where)
            .OrderByIF(!string.IsNullOrEmpty(order), order);
        return await query.ToListAsync();
    }
    
    /// <summary>
    /// 根据条件查询列表
    /// </summary>
    /// <param name="where">拉姆达条件</param>
    /// <param name="expression">拉姆达排序</param>
    /// <returns></returns>
    public async Task<int> SumAsync(Expression<Func<T, bool>> where,Expression<Func<T, int>> expression)
    {
        var query =  Context.Queryable<T>()
            .Where(where);
        return await query.SumAsync(expression);
    }

}
