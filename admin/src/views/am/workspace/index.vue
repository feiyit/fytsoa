<script setup lang="ts">
import dayjs from "dayjs";
import {
  fetchAmWorkspaceSummary,
  type AmWorkspaceSummaryDto,
} from "@/api/am/workspace";
import {
  createBarChartOptions,
  createLineChartOptions,
  createPieChartOptions,
} from "@/component/soaChart/useSoaChart";

defineOptions({ name: "AmWorkspaceView" });

const now = ref(new Date());
let timer: number | undefined;

onMounted(() => {
  timer = window.setInterval(() => (now.value = new Date()), 1000);
  refresh();
});
onBeforeUnmount(() => {
  if (timer) window.clearInterval(timer);
});

const currentYear = new Date().getFullYear();
const yearOptions = Array.from({ length: 5 }, (_, i) => currentYear - 4 + i);

const query = reactive({
  year: currentYear,
  dueSoonDays: 7,
});

const loading = ref(false);

const summary = ref<AmWorkspaceSummaryDto>({
  tenantId: 0,
  statsTime: dayjs().toISOString(),
  year: currentYear,
  dueSoonDays: 7,

  assetTotal: 0,
  assetDelTotal: 0,
  assetOriginalValueTotal: 0,
  assetNetBookValueTotal: 0,
  assetWarrantyOverdueTotal: 0,
  assetWarrantyDueSoonTotal: 0,
  assetStatusStats: [],
  assetCategoryTopStats: [],
  assetCreatedByMonth: [],

  vendorTotal: 0,
  vendorEnabledTotal: 0,
  locationTotal: 0,
  locationEnabledTotal: 0,
  warehouseTotal: 0,
  warehouseEnabledTotal: 0,
  warehouseBinTotal: 0,

  inventoryPlanTotal: 0,
  inventoryPlanRunningTotal: 0,
  inventoryPlanStatusStats: [],

  maintenancePlanTotal: 0,
  maintenancePlanEnabledTotal: 0,
  maintenanceOrderTotal: 0,
  maintenanceOrderOpenTotal: 0,
  maintenanceOrderStatusStats: [],
  maintenanceOrderTypeStats: [],

  docTotal: 0,

  assetDepreciationTotal: 0,
  assetDepreciationEnabledTotal: 0,
  assetDepreciationAccumAmountTotal: 0,
  depreciationRunTotal: 0,
  lastDepreciationRunPeriod: null,
  depreciationRunTotalAmountAll: 0,

  reminderRuleTotal: 0,
  reminderRuleEnabledTotal: 0,
  reminderTaskTotal: 0,
  reminderTaskOpenTotal: 0,
  reminderTaskOverdueTotal: 0,
  reminderTaskDueSoonTotal: 0,
  reminderTaskStatusStats: [],
});

const refresh = async () => {
  loading.value = true;
  try {
    summary.value = await fetchAmWorkspaceSummary({
      year: query.year,
      dueSoonDays: query.dueSoonDays,
    });
  } finally {
    loading.value = false;
  }
};

const dateTimeText = computed(() => dayjs(now.value).format("YYYY-MM-DD HH:mm:ss"));

const toMoney = (v: number) =>
  (Number(v) || 0).toLocaleString(undefined, {
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  });

const assetStatusOptions = computed(() =>
  createPieChartOptions({
    title: "资产状态分布",
    data: (summary.value.assetStatusStats || []).map((x) => ({
      name: x.name,
      value: x.value,
    })),
  })
);

const reminderStatusOptions = computed(() =>
  createPieChartOptions({
    title: "提醒任务状态分布",
    data: (summary.value.reminderTaskStatusStats || []).map((x) => ({
      name: x.name,
      value: x.value,
    })),
  })
);

const maintenanceOrderStatusOptions = computed(() => {
  const xAxis = (summary.value.maintenanceOrderStatusStats || []).map((x) => x.name);
  const data = (summary.value.maintenanceOrderStatusStats || []).map((x) => x.value);
  return createBarChartOptions({
    title: "工单状态分布",
    xAxis,
    series: [{ name: "数量", data }],
  });
});

const maintenanceOrderTypeOptions = computed(() => {
  const xAxis = (summary.value.maintenanceOrderTypeStats || []).map((x) => x.name);
  const data = (summary.value.maintenanceOrderTypeStats || []).map((x) => x.value);
  return createBarChartOptions({
    title: "工单类型分布",
    xAxis,
    series: [{ name: "数量", data }],
  });
});

const assetCreatedOptions = computed(() => {
  const xAxis = (summary.value.assetCreatedByMonth || []).map((x) => x.name);
  const data = (summary.value.assetCreatedByMonth || []).map((x) => x.value);
  return createLineChartOptions({
    title: `${summary.value.year} 年新增资产趋势`,
    xAxis,
    series: [{ name: "新增资产", data }],
  });
});

const assetCategoryTopOptions = computed(() => {
  const list = summary.value.assetCategoryTopStats || [];
  const xAxis = list.map((x) => x.name);
  const data = list.map((x) => x.value);
  return createBarChartOptions({
    title: "资产分类 Top 10",
    xAxis,
    series: [{ name: "数量", data }],
  });
});

const inventoryPlanStatusOptions = computed(() => {
  const xAxis = (summary.value.inventoryPlanStatusStats || []).map((x) => x.name);
  const data = (summary.value.inventoryPlanStatusStats || []).map((x) => x.value);
  return createBarChartOptions({
    title: "盘点计划状态分布",
    xAxis,
    series: [{ name: "数量", data }],
  });
});
</script>

<template>
  <div class="space-y-2">
    <el-card
      shadow="never"
      class="bg-card border-border rounded-[.5vw] !border-slate-200/80 dark:!border-slate-750"
    >
      <div class="flex flex-wrap items-center justify-between gap-3">
        <div>
          <div class="text-base font-semibold">资产数据统计中心</div>
          <div class="mt-1 text-xs text-slate-500 dark:text-slate-400">
            数据时间：{{ dateTimeText }}（接口统计时间：{{
              dayjs(summary.statsTime).format("YYYY-MM-DD HH:mm:ss")
            }}）
          </div>
        </div>

        <div class="flex flex-wrap items-center gap-2">
          <el-select v-model="query.year" style="width: 110px" @change="refresh">
            <el-option v-for="y in yearOptions" :key="y" :label="`${y}年`" :value="y" />
          </el-select>
          <el-input-number
            v-model="query.dueSoonDays"
            :min="1"
            :max="365"
            controls-position="right"
            style="width: 150px"
            @change="refresh"
          />
          <span class="text-xs text-slate-500 dark:text-slate-400">提醒：到期天数</span>
          <el-button :loading="loading" type="primary" plain @click="refresh">刷新</el-button>
        </div>
      </div>

      <el-divider class="!my-3" />

      <el-skeleton :loading="loading" animated :rows="6">
        <template #default>
          <el-row :gutter="12">
            <el-col :xs="24" :sm="12" :md="8" :lg="6" :xl="6">
              <el-card shadow="never" class="bg-white/60 dark:bg-slate-900/30">
                <el-statistic title="资产总数" :value="summary.assetTotal" />
                <div class="mt-2 text-xs text-slate-500">
                  已删除：{{ summary.assetDelTotal }}
                </div>
                <div class="mt-1 text-xs text-slate-500">
                  质保已过期：{{ summary.assetWarrantyOverdueTotal }}，{{ summary.dueSoonDays }} 天内到期：{{
                    summary.assetWarrantyDueSoonTotal
                  }}
                </div>
              </el-card>
            </el-col>

            <el-col :xs="24" :sm="12" :md="8" :lg="6" :xl="6">
              <el-card shadow="never" class="bg-white/60 dark:bg-slate-900/30">
                <div class="text-xs text-slate-500">资产原值总额</div>
                <div class="mt-1 text-lg font-semibold">{{ toMoney(summary.assetOriginalValueTotal) }}</div>
                <div class="mt-3 text-xs text-slate-500">资产净值总额</div>
                <div class="mt-1 text-lg font-semibold">{{ toMoney(summary.assetNetBookValueTotal) }}</div>
              </el-card>
            </el-col>

            <el-col :xs="24" :sm="12" :md="8" :lg="6" :xl="6">
              <el-card shadow="never" class="bg-white/60 dark:bg-slate-900/30">
                <el-statistic title="供应商" :value="summary.vendorTotal" />
                <div class="mt-2 text-xs text-slate-500">
                  启用：{{ summary.vendorEnabledTotal }}
                </div>
              </el-card>
            </el-col>

            <el-col :xs="24" :sm="12" :md="8" :lg="6" :xl="6">
              <el-card shadow="never" class="bg-white/60 dark:bg-slate-900/30">
                <el-statistic title="地点 / 仓库 / 库位" :value="summary.locationTotal" />
                <div class="mt-2 text-xs text-slate-500">
                  地点启用：{{ summary.locationEnabledTotal }}
                </div>
                <div class="mt-1 text-xs text-slate-500">
                  仓库：{{ summary.warehouseTotal }}（启用 {{ summary.warehouseEnabledTotal }}），库位：{{
                    summary.warehouseBinTotal
                  }}
                </div>
              </el-card>
            </el-col>
          </el-row>

          <el-row :gutter="12" class="mt-3">
            <el-col :xs="24" :sm="12" :md="8" :lg="6" :xl="6">
              <el-card shadow="never" class="bg-white/60 dark:bg-slate-900/30">
                <el-statistic title="盘点计划" :value="summary.inventoryPlanTotal" />
                <div class="mt-2 text-xs text-slate-500">
                  进行中：{{ summary.inventoryPlanRunningTotal }}
                </div>
              </el-card>
            </el-col>

            <el-col :xs="24" :sm="12" :md="8" :lg="6" :xl="6">
              <el-card shadow="never" class="bg-white/60 dark:bg-slate-900/30">
                <el-statistic title="保养计划" :value="summary.maintenancePlanTotal" />
                <div class="mt-2 text-xs text-slate-500">
                  启用：{{ summary.maintenancePlanEnabledTotal }}
                </div>
              </el-card>
            </el-col>

            <el-col :xs="24" :sm="12" :md="8" :lg="6" :xl="6">
              <el-card shadow="never" class="bg-white/60 dark:bg-slate-900/30">
                <el-statistic title="维修/保养工单" :value="summary.maintenanceOrderTotal" />
                <div class="mt-2 text-xs text-slate-500">
                  待处理：{{ summary.maintenanceOrderOpenTotal }}
                </div>
              </el-card>
            </el-col>

            <el-col :xs="24" :sm="12" :md="8" :lg="6" :xl="6">
              <el-card shadow="never" class="bg-white/60 dark:bg-slate-900/30">
                <el-statistic title="提醒任务" :value="summary.reminderTaskTotal" />
                <div class="mt-2 text-xs text-slate-500">
                  未关闭：{{ summary.reminderTaskOpenTotal }}，已逾期：{{ summary.reminderTaskOverdueTotal }}
                </div>
                <div class="mt-1 text-xs text-slate-500">
                  {{ summary.dueSoonDays }} 天内到期：{{ summary.reminderTaskDueSoonTotal }}
                </div>
              </el-card>
            </el-col>
          </el-row>

          <el-row :gutter="12" class="mt-3">
            <el-col :xs="24" :sm="12" :md="8" :lg="6" :xl="6">
              <el-card shadow="never" class="bg-white/60 dark:bg-slate-900/30">
                <el-statistic title="单据" :value="summary.docTotal" />
              </el-card>
            </el-col>

            <el-col :xs="24" :sm="12" :md="8" :lg="6" :xl="6">
              <el-card shadow="never" class="bg-white/60 dark:bg-slate-900/30">
                <el-statistic title="折旧配置(资产)" :value="summary.assetDepreciationTotal" />
                <div class="mt-2 text-xs text-slate-500">
                  折旧中：{{ summary.assetDepreciationEnabledTotal }}
                </div>
                <div class="mt-1 text-xs text-slate-500">
                  累计折旧：{{ toMoney(summary.assetDepreciationAccumAmountTotal) }}
                </div>
              </el-card>
            </el-col>

            <el-col :xs="24" :sm="12" :md="8" :lg="6" :xl="6">
              <el-card shadow="never" class="bg-white/60 dark:bg-slate-900/30">
                <el-statistic title="折旧计提批次" :value="summary.depreciationRunTotal" />
                <div class="mt-2 text-xs text-slate-500">
                  最近期间：{{ summary.lastDepreciationRunPeriod || "-" }}
                </div>
                <div class="mt-1 text-xs text-slate-500">
                  计提合计：{{ toMoney(summary.depreciationRunTotalAmountAll) }}
                </div>
              </el-card>
            </el-col>

            <el-col :xs="24" :sm="12" :md="8" :lg="6" :xl="6">
              <el-card shadow="never" class="bg-white/60 dark:bg-slate-900/30">
                <el-statistic title="提醒规则" :value="summary.reminderRuleTotal" />
                <div class="mt-2 text-xs text-slate-500">
                  启用：{{ summary.reminderRuleEnabledTotal }}
                </div>
              </el-card>
            </el-col>
          </el-row>
        </template>
      </el-skeleton>
    </el-card>

    <el-row :gutter="12">
      <el-col :xs="24" :sm="24" :md="12" :lg="12" :xl="12">
        <el-card
          shadow="never"
          class="h-full bg-card border-border rounded-[.5vw] !border-slate-200/80 dark:!border-slate-750"
        >
          <soaChart :options="assetStatusOptions" :height="260" />
        </el-card>
      </el-col>

      <el-col :xs="24" :sm="24" :md="12" :lg="12" :xl="12">
        <el-card
          shadow="never"
          class="h-full bg-card border-border rounded-[.5vw] !border-slate-200/80 dark:!border-slate-750"
        >
          <soaChart :options="maintenanceOrderStatusOptions" :height="260" />
        </el-card>
      </el-col>
    </el-row>

    <el-row :gutter="12">
      <el-col :xs="24" :sm="24" :md="14" :lg="14" :xl="14">
        <el-card
          shadow="never"
          class="h-full bg-card border-border rounded-[.5vw] !border-slate-200/80 dark:!border-slate-750"
        >
          <soaChart :options="assetCreatedOptions" :height="280" />
        </el-card>
      </el-col>

      <el-col :xs="24" :sm="24" :md="10" :lg="10" :xl="10">
        <el-card
          shadow="never"
          class="h-full bg-card border-border rounded-[.5vw] !border-slate-200/80 dark:!border-slate-750"
        >
          <soaChart :options="assetCategoryTopOptions" :height="280" />
        </el-card>
      </el-col>
    </el-row>

    <el-row :gutter="12">
      <el-col :xs="24" :sm="24" :md="12" :lg="12" :xl="12">
        <el-card
          shadow="never"
          class="h-full bg-card border-border rounded-[.5vw] !border-slate-200/80 dark:!border-slate-750"
        >
          <soaChart :options="inventoryPlanStatusOptions" :height="260" />
        </el-card>
      </el-col>

      <el-col :xs="24" :sm="24" :md="12" :lg="12" :xl="12">
        <el-card
          shadow="never"
          class="h-full bg-card border-border rounded-[.5vw] !border-slate-200/80 dark:!border-slate-750"
        >
          <soaChart :options="reminderStatusOptions" :height="260" />
        </el-card>
      </el-col>
    </el-row>

    <el-row :gutter="12">
      <el-col :xs="24" :sm="24" :md="24" :lg="24" :xl="24">
        <el-card
          shadow="never"
          class="bg-card border-border rounded-[.5vw] !border-slate-200/80 dark:!border-slate-750"
        >
          <soaChart :options="maintenanceOrderTypeOptions" :height="240" />
        </el-card>
      </el-col>
    </el-row>
  </div>
</template>
