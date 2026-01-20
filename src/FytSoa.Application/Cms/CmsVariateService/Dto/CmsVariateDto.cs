using FytSoa.Common.Enum;

namespace FytSoa.Application.Cms.Dto;

public class CmsVariateDto : AppEntity
{
    /// <summary>
    /// 组
    /// </summary>
    public string Group { get; set; }
    
    /// <summary>
    /// 标题
    /// </summary>
    public string Title { get; set; }
    
    /// <summary>
    /// 字段名称
    /// </summary>
    public string Field { get; set; }
    
    /// <summary>
    /// 字段类型
    /// </summary>
    public CmsFieldTypeEnum FieldType { get; set; }
    
    /// <summary>
    /// 字段类型
    /// </summary>
    public string FieldTypeName { get; set; }
    
    /// <summary>
    /// 字段值
    /// </summary>
    public string Value { get; set; }
    
    /// <summary>
    /// 添加时间
    /// </summary>
    public DateTime CreateTime { get; set; }=DateTime.Now;

    /// <summary>
    /// 添加人
    /// </summary>
    public string CreateUser { get; set; }

    /// <summary>
    /// 修改时间
    /// </summary>
    public DateTime UpdateTime { get; set; }=DateTime.Now;

    /// <summary>
    /// 修改人
    /// </summary>
    public string UpdateUser { get; set; }
}