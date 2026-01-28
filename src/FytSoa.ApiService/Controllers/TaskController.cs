using FytSoa.Application.Am;

// 命名空间：FytSoa
// 类名：TaskJob（和任务调度默认的 DllClassName 保持一致：FytSoa.TaskJob, FytSoa.ApiService）
namespace FytSoa
{
    /// <summary>
    /// 定时任务类（必须 public）
    /// </summary>
    public class TaskJob
    {
        private readonly AmMaintenancePlanSchedulerService _maintenancePlanSchedulerService;

        // 通过 DI 注入，便于在 Quartz(JobFactory) 场景下使用数据库/服务
        public TaskJob(AmMaintenancePlanSchedulerService maintenancePlanSchedulerService)
        {
            _maintenancePlanSchedulerService = maintenancePlanSchedulerService;
        }

        /// <summary>
        /// 示例任务：输出日志（必须 public、无参数）
        /// </summary>
        public void GetFunTask()
        {
            var logContent = $"【DLL方式】执行任务：{DateTime.Now:yyyy-MM-dd HH:mm:ss}\r\n";
            Console.WriteLine(logContent);
        }

        /// <summary>
        /// 保养计划提醒：供任务调度（DLL 方式）调用（必须 public、无参数）
        /// </summary>
        public async Task MaintenancePlanNoticeAsync()
        {
            await _maintenancePlanSchedulerService.NotifyDuePlansAsync();
        }
    }
}
