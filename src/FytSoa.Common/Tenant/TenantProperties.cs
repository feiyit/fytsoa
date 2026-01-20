using System.Reflection;

namespace FytSoa.Common.Tenant;

public class TenantProperties
{
    /// <summary>
    /// 是否开启租户
    /// </summary>
    public static bool Enable = true;

    /// <summary>
    /// 设置忽略添加租户编号
    /// </summary>
    public static readonly List<string> IgnoreAddEntity = new(){"SysMenu","SysTenant"};
    
    /// <summary>
    /// 创建租户过程中，忽略自动添加租户编号
    /// </summary>
    public static readonly List<string> AddTenantIgnoreTable = new(){"SysAdmin","SysRole","SysOrganize","SysAdminRole"};
    
    private static List<Type> _domainEntity = new ();
    private static readonly List<string> IgnoreEntity = new(){"FytSoa.Domain.Sys.SysTenant"};
    /// <summary>
    /// 通过反射读取对象内容
    /// </summary>
    public static List<Type> DomainEntity
    {
        get
        {
            if (_domainEntity.Count != 0) return _domainEntity;
            var assemblyService = Assembly.Load("FytSoa.Domain");
            _domainEntity = assemblyService.GetTypes().Where(it=>it.FullName != null && it.IsClass && it.FullName.Contains("FytSoa.Domain.") && !it.FullName.Contains("FytSoa.Domain.Entity") && it.GetProperty("TenantId")!=null 
                                                                 && !IgnoreEntity.Contains(it.FullName)).ToList();
            return _domainEntity;
        }
    }
}