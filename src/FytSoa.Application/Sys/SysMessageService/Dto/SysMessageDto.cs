using System;
using FytSoa.Common.Utils;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using FytSoa.Common.Enum;
using SqlSugar;

namespace FytSoa.Application.Sys;

/// <summary>
/// 留言消息表
/// </summary>
public class SysMessageDto : AppEntity
{
    /// <summary>
    /// 栏目Id
    /// </summary>
    public long ColumnId { get; set; } = 0;

    /// <summary>
    /// 类型
    /// </summary>
    public MessageEnum Types { get; set; } = MessageEnum.System;
    
    /// <summary>
    /// 类型名称
    /// </summary>
    public string TypesName { get; set; }

    /// <summary>
    /// 留言标题
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// 邮箱信息
    /// </summary>
    public string Email { get; set; }

    /// <summary>
    /// 手机号码
    /// </summary>
    public string Mobile { get; set; }

    /// <summary>
    /// 留言标签
    /// </summary>
    public string Tags { get; set; }

    /// <summary>
    /// 留言内容
    /// </summary>
    public string Summary { get; set; }
    
    /// <summary>
    /// 地址
    /// </summary>
    public string Address { get; set; }

    /// <summary>
    /// 是否已读
    /// </summary>
    public bool IsRead { get; set; } = false;

    /// <summary>
    /// 是否删除
    /// </summary>
    public bool IsDelete { get; set; } = false;

    /// <summary>
    /// 用户编号
    /// </summary>
    public long UserId { get; set; }

    /// <summary>
    /// 用户姓名
    /// </summary>
    public string UserName { get; set; }

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
    public DateTime? UpdateTime { get; set; }

    /// <summary>
    /// 修改人
    /// </summary>
    public string UpdateUser { get; set; }


}