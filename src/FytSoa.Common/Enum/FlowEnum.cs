using System.ComponentModel;

namespace FytSoa.Common.Enum;

/// <summary>
/// 审批类型
/// </summary>
public enum FlowTypeEnum
{
    [Description("合同")]
    Contract = 1,

    [Description("回款")]
    PayMent = 2,

    [Description("发票")]
    Invoice = 3,

    [Description("财务报销")]
    Reimbursement = 4,
    
    [Description("财务付款")]
    FinancialPayment = 5,
    
    [Description("财务回款")]
    FinancialRefund = 6,
    
    [Description("生产订单")]
    ProductionOrder = 7,
    
    [Description("采购订单")]
    PurchaseOrder = 8,
    
    [Description("采购退单")]
    PurchaseReturn = 9,
    
    [Description("物料入库")]
    InStock = 10,
    
    [Description("物料出库")]
    OutStock = 11,
    
    [Description("物料调拨")]
    TransferStock = 12,
    
    [Description("车辆使用申请")]
    CarUse = 20,
    
    [Description("会议室申请")]
    MeetingRoom = 21,
    
    [Description("借款申请")]
    BorrowMoney = 22,
    
    [Description("请假申请")]
    AskForLeave = 23,
    
    [Description("出差申请")]
    BusinessTrip = 24,
}

/// <summary>
/// 审批记录状态
/// </summary>
public enum FlowAuditEnum
{
    [Description("发起")]
    Initiate = 0,
    
    [Description("待审核")]
    Await = 1,
    
    [Description("通过")]
    Agree = 2,
    
    [Description("催办")]
    Urge = 3,
    
    [Description("跳过")]
    Pass = 4,
    
    [Description("转给他人处理")]
    HandOn = 5,

    [Description("驳回")]
    Reject = 6,

    [Description("归档")]
    Archive = 7,

    [Description("作废")]
    Void = 8,
    
    [Description("结束")]
    Finish = 9
}