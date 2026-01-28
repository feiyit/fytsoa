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
        private readonly AmAssetDepreciationSchedulerService _assetDepSchedulerService;
        private readonly AmDepreciationRunSchedulerService _depRunSchedulerService;

        // 通过 DI 注入，便于在 Quartz(JobFactory) 场景下使用数据库/服务
        public TaskJob(
            AmMaintenancePlanSchedulerService maintenancePlanSchedulerService,
            AmAssetDepreciationSchedulerService assetDepSchedulerService,
            AmDepreciationRunSchedulerService depRunSchedulerService)
        {
            _maintenancePlanSchedulerService = maintenancePlanSchedulerService;
            _assetDepSchedulerService = assetDepSchedulerService;
            _depRunSchedulerService = depRunSchedulerService;
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

        /// <summary>
        /// 折旧配置：按配置计算资产净值（供任务调度调用）
        /// </summary>
        public async Task AssetDepreciationNetValueAsync()
        {
            await _assetDepSchedulerService.RecalcAssetNetBookValueAsync();
        }

        /// <summary>
        /// 折旧计提：根据已确认明细回写资产净值（供任务调度调用）
        /// </summary>
        public async Task DepreciationRunNetValueAsync()
        {
            await _depRunSchedulerService.ApplyLatestConfirmedRunsAsync();
        }
    }
}
