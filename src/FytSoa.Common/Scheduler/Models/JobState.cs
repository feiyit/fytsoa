namespace FytSoa.Common.Scheduler.Models;

/// <summary>
/// 与现有 SysQuartzService / 前端约定保持一致的任务状态枚举（历史兼容）。
/// </summary>
public enum JobState
{
    新增 = 1,
    删除 = 2,
    修改 = 3,
    暂停 = 4,
    停止 = 5,
    开启 = 6,
    立即执行 = 7,
}

