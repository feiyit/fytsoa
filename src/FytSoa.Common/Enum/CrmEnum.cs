using System.ComponentModel;

namespace FytSoa.Common.Enum;

public enum CrmProjectStatusEnum
{
    [Description("未开始")]
    NotStarted = 0,

    [Description("进行中")]
    InProgress = 1,

    [Description("已完成")]
    Completed = 2,

    [Description("已关闭")]
    Closed = 3
}

/// <summary>
/// Erp 生产审核枚举
/// </summary>
public enum CrmAuditEnum
{
    [Description("未审核")]
    NotReviewed = 1,

    [Description("审核中")]
    InReview = 2,

    [Description("已审核")]
    Reviewed = 3
}

/// <summary>
/// 跟进状态
/// </summary>
public enum CrmFollowEnum
{
    [Description("未处理")]
    NotReviewed = 0,

    [Description("已完成")]
    InReview = 1,

    [Description("已过期")]
    Reviewed = 2,
    
    [Description("已作废")]
    Cancel = 2
}

/// <summary>
/// 项目类型
/// </summary>
public enum CrmProjectTypeEnum
{
    [Description("商机")]
    Opportunity = 1,

    [Description("项目")]
    Project = 2,
}

/// <summary>
/// 商机状态
/// </summary>
public enum CrmOpportunityStatusEnum
{
    [Description("需求发现")]
    Discover = 0,

    [Description("方案报价")]
    InProgress = 1,

    [Description("商务谈判")]
    Negotiate = 2,

    [Description("赢单")]
    Winning = 3,
    
    [Description("输单")]
    Lose = 4,
    
    [Description("作废")]
    Closed = 5
}

/// <summary>
/// 项目任务状态
/// </summary>
public enum CrmProjectTaskStatusEnum
{
    [Description("未开始")]
    Opportunity = 0,

    [Description("进行中")]
    Proceed = 1,
    
    [Description("已完成")]
    Success = 2,
    
    [Description("已关闭")]
    Close = 3,
}

