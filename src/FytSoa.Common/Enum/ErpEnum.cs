using System.ComponentModel;

namespace FytSoa.Common.Enum;

/// <summary>
/// Erp 操作日志类型
/// </summary>
public enum 
    ErpOperateEnum
{
    [Description("采购订单")]
    PurchaseOrder=1,
    
    [Description("采购退回")]
    PurchaseReturn=2,
    
    [Description("入库")]
    InStock=3,
    
    [Description("出库")]
    OutStock=4,
    
    [Description("调拨")]
    Transfer=5,
    Invoice=6,
    
    [Description("发票付款")]
    FinancePayment=7,
    
    [Description("回款")]
    FinanceRefund=8,
    
    [Description("报销")]
    Reimbursement=9,
    
    [Description("生产订单")]
    ProduceOrder=10,
    
    [Description("生产申请物料")]
    ProduceApply=11,
}

/// <summary>
/// Erp 生产阶段枚举
/// </summary>
public enum ErpProducePhaseEnum
{
    [Description("下单")]
    Placed = 1,

    [Description("工程")]
    Engineering = 2,

    [Description("设计")]
    Design = 3,

    [Description("生产")]
    Production = 4,

    [Description("品控")]
    QualityControl = 5,

    [Description("完成")]
    Completed = 6,
    
    [Description("关闭")]
    Close = 7
}

/// <summary>
/// Erp 生产品控枚举
/// </summary>
public enum ErpProduceStatusEnum
{
    [Description("待接收")]
    Pending=0,
    
    [Description("已接收")]
    Received=1,
    
    [Description("已开工")]
    InProgress=2,
    
    [Description("已完成")]
    Completed=3,
    
    [Description("驳回")]
    Reject=4
}

/// <summary>
/// Erp 生产审核枚举
/// </summary>
public enum ErpAuditEnum
{
    [Description("未审核")]
    NotReviewed = 0,

    [Description("审核中")]
    InReview = 1,

    [Description("已审核")]
    Reviewed = 2
}

/// <summary>
/// Erp 生产申请枚举
/// </summary>
public enum ErpProduceApplyEnum
{
    [Description("新申请")]
    NewApply = 0,

    [Description("采购中")]
    Purchasing = 1,

    [Description("入库中")]
    InStock = 2,

    [Description("已出库")]
    OutStock = 3,

    [Description("已领料")]
    Completed = 4
}

/// <summary>
/// Erp 物料出库类型
/// </summary>
public enum ErpOutStockTypeEnum
{
    [Description("销售")]
    Sales = 1,

    [Description("生产")]
    Production = 2,

    [Description("采购退货")]
    PurchaseReturn = 3,

    [Description("其他")]
    Other = 4
}

/// <summary>
/// Erp 物料入库类型
/// </summary>
public enum ErpInStockTypeEnum
{
    [Description("采购")]
    Purchase = 1,

    [Description("生产")]
    Production = 2,

    [Description("销售退货")]
    SalesReturn = 3,

    [Description("其他")]
    Other = 4
}

/// <summary>
/// Erp 销售状态
/// </summary>
public enum ErpSalesOrderStatusEnum
{
    [Description("未审核")]
    UnAudited = 1,

    [Description("已审核")]
    Audited = 2,

    [Description("部分发货")]
    PartDeliver = 3,

    [Description("已完成")]
    Completed = 4,
    
    [Description("关闭")]
    Close = 5,
}

/// <summary>
/// Erp 销售退货状态
/// </summary>
public enum ErpSalesReturnStatusEnum
{
    [Description("待审核")]
    UnAudited = 1,

    [Description("已审核")]
    Audited = 2,

    [Description("已退款")]
    Refund = 3,

    [Description("已驳回")]
    Reject = 4
}

