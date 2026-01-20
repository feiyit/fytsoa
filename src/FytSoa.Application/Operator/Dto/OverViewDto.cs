namespace FytSoa.Application.Operator;

/// <summary>
/// 统计
/// </summary>
public class OverViewDto
{
    /// <summary>
    /// 客户关系
    /// </summary>
    public CrmTotal Crm { get; set; } = new();
    
    /// <summary>
    /// 采购
    /// </summary>
    public PurchaseTotal Purchase { get; set; } = new();
    
    /// <summary>
    /// 库存
    /// </summary>
    public StockTotal Stock { get; set; } = new();
    
    /// <summary>
    /// 生产
    /// </summary>
    public ManufactureTotal Manufacture { get; set; } = new();
    
    /// <summary>
    /// 审批
    /// </summary>
    public ApproveTotal Approve { get; set; } = new();
    
    public class CrmTotal
    {
        /// <summary>
        /// 项目
        /// </summary>
        public int ProJect { get; set; } = 0;

        /// <summary>
        /// 项目任务
        /// </summary>
        public int ProJectTask { get; set; } = 0;

        /// <summary>
        /// 合同
        /// </summary>
        public int Contract { get; set; } = 0;

        /// <summary>
        /// 回款
        /// </summary>
        public int Refund { get; set; } = 0;

        /// <summary>
        /// 发票
        /// </summary>
        public int Invoice { get; set; } = 0;
    }

    /// <summary>
    /// 采购
    /// </summary>
    public class PurchaseTotal
    {
        /// <summary>
        /// 申请
        /// </summary>
        public int Apply { get; set; } = 0;
        
        /// <summary>
        /// 订单
        /// </summary>
        public int Order { get; set; } = 0;
        
        /// <summary>
        /// 退货
        /// </summary>
        public int Return { get; set; } = 0;
    }
    
    /// <summary>
    /// 库存
    /// </summary>
    public class StockTotal
    {
        /// <summary>
        /// 入库
        /// </summary>
        public int In { get; set; } = 0;
        
        /// <summary>
        /// 出库
        /// </summary>
        public int Out { get; set; } = 0;
        
        /// <summary>
        /// 调拨
        /// </summary>
        public int Transfer { get; set; } = 0;
    }
    
    /// <summary>
    /// 生产
    /// </summary>
    public class ManufactureTotal
    {
        /// <summary>
        /// 订单
        /// </summary>
        public int Order { get; set; } = 0;
        
        /// <summary>
        /// 设计
        /// </summary>
        public int Design { get; set; } = 0;
        
        /// <summary>
        /// 任务
        /// </summary>
        public int Task { get; set; } = 0;
        
        /// <summary>
        /// 品控
        /// </summary>
        public int Quality { get; set; } = 0;
    }
    
    /// <summary>
    /// 审批
    /// </summary>
    public class ApproveTotal
    {
        /// <summary>
        /// 待办
        /// </summary>
        public int WaitProcessing { get; set; } = 0;
        
        /// <summary>
        /// 驳回
        /// </summary>
        public int Reject { get; set; } = 0;
    }

}