using System.ComponentModel;

namespace FytSoa.Common.Enum;

public enum CmsFieldTypeEnum
{
    [Description("单行文本")]
    SingleText = 1,
    
    [Description("多行文本")]
    MultiText = 2,
    
    [Description("单选项")]
    SingleSelect = 3,
    
    [Description("多选项")]
    MultiSelect = 4,
    
    [Description("开关")]
    Switch = 5,
    
    [Description("上传")]
    Upload = 6,
    
    [Description("下拉框")]
    DropSelect = 7,
    
    [Description("日期")]
    DateTime = 8,
    
    [Description("媒体")]
    Media = 9,
    
    [Description("HTML文本")]
    HtmlText = 10,
    
    [Description("其他")]
    Others = 11
}