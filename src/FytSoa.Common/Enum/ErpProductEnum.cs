using System.ComponentModel;

namespace FytSoa.Common.Enum;

public enum ErpProductTagEnum
{
    [Description("全部")]
    All=0,
    [Description("成品")]
    Finished=1,
    [Description("半成品")]
    Semi=2,
    [Description("物料")]
    Materiel=3,
    [Description("报废料")]
    Scrap=4,
    [Description("客供料")]
    Provided=5,
    [Description("处理")]
    Dispose=6,
}