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
const yearOptions = Array.from({ length: 5 }, (_, i) => currentYear - 1 + i);

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

const dateTimeText = computed(() =>
  dayjs(now.value).format("YYYY-MM-DD HH:mm:ss"),
);

const formatNumber = (v: number) => (Number(v) || 0).toLocaleString();

const toMoney = (v: number) =>
  (Number(v) || 0).toLocaleString(undefined, {
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  });

const assetStatusOptions = computed(() =>
  createPieChartOptions({
    data: (summary.value.assetStatusStats || []).map((x) => ({
      name: x.name,
      value: x.value,
    })),
  }),
);

const reminderStatusOptions = computed(() =>
  createPieChartOptions({
    data: (summary.value.reminderTaskStatusStats || []).map((x) => ({
      name: x.name,
      value: x.value,
    })),
  }),
);

const maintenanceOrderStatusOptions = computed(() => {
  const xAxis = (summary.value.maintenanceOrderStatusStats || []).map(
    (x) => x.name,
  );
  const data = (summary.value.maintenanceOrderStatusStats || []).map(
    (x) => x.value,
  );
  return createBarChartOptions({
    xAxis,
    series: [{ name: "数量", data }],
  });
});

const maintenanceOrderTypeOptions = computed(() => {
  const xAxis = (summary.value.maintenanceOrderTypeStats || []).map(
    (x) => x.name,
  );
  const data = (summary.value.maintenanceOrderTypeStats || []).map(
    (x) => x.value,
  );
  return createBarChartOptions({
    xAxis,
    series: [{ name: "数量", data }],
  });
});

const assetCreatedOptions = computed(() => {
  const xAxis = (summary.value.assetCreatedByMonth || []).map((x) => x.name);
  const data = (summary.value.assetCreatedByMonth || []).map((x) => x.value);
  return createLineChartOptions({
    xAxis,
    series: [{ name: "新增资产", data }],
  });
});

const assetCategoryTopOptions = computed(() => {
  const list = summary.value.assetCategoryTopStats || [];
  const xAxis = list.map((x) => x.name);
  const data = list.map((x) => x.value);
  return createBarChartOptions({
    xAxis,
    series: [{ name: "数量", data }],
  });
});

const inventoryPlanStatusOptions = computed(() => {
  const xAxis = (summary.value.inventoryPlanStatusStats || []).map(
    (x) => x.name,
  );
  const data = (summary.value.inventoryPlanStatusStats || []).map(
    (x) => x.value,
  );
  return createBarChartOptions({
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
          <div class="flex items-center gap-2 text-base font-semibold">
            <span
              class="inline-flex h-8 w-8 items-center justify-center rounded-[10px] bg-primary-500/10 text-primary-600 dark:text-primary-300"
            >
              <el-icon :size="16"><DataAnalysis /></el-icon>
            </span>
            <span>资产数据统计中心</span>
          </div>
          <div class="mt-1 text-xs text-slate-500 dark:text-slate-400">
            数据时间：{{ dateTimeText }}（接口统计时间：{{
              dayjs(summary.statsTime).format("YYYY-MM-DD HH:mm:ss")
            }}）
          </div>
        </div>

        <div class="flex flex-wrap items-center gap-2">
          <el-select
            v-model="query.year"
            style="width: 110px"
            @change="refresh"
          >
            <el-option
              v-for="y in yearOptions"
              :key="y"
              :label="`${y}年`"
              :value="y"
            />
          </el-select>
          <el-input-number
            v-model="query.dueSoonDays"
            :min="1"
            :max="365"
            controls-position="right"
            style="width: 150px"
            @change="refresh"
          />
          <span class="text-xs text-slate-500 dark:text-slate-400"
            >提醒：到期天数</span
          >
          <el-button :loading="loading" type="primary" plain @click="refresh"
            >刷新</el-button
          >
        </div>
      </div>

      <el-divider class="!my-3" />

      <el-skeleton :loading="loading" animated :rows="6">
        <template #default>
          <el-row :gutter="12">
            <el-col :xs="24" :sm="12" :md="8" :lg="6" :xl="6">
              <el-card
                shadow="never"
                class="relative overflow-hidden bg-white/70 transition hover:-translate-y-[1px] hover:shadow-md dark:bg-slate-900/30"
              >
                <div
                  class="pointer-events-none absolute -right-10 -top-10 h-28 w-28 rounded-full bg-blue-500/20 blur-2xl"
                />
                <div class="flex items-start justify-between gap-2">
                  <div>
                    <div
                      class="flex items-center gap-2 text-xs text-slate-500 dark:text-slate-400"
                    >
                      <span
                        class="inline-flex h-7 w-7 items-center justify-center rounded-[10px] bg-blue-500/10 text-blue-600 dark:text-blue-300"
                      >
                        <el-icon :size="16"><Box /></el-icon>
                      </span>
                      <span>资产总数</span>
                    </div>
                    <div
                      class="mt-2 text-2xl font-semibold text-slate-900 dark:text-slate-100"
                    >
                      {{ formatNumber(summary.assetTotal) }}
                    </div>
                  </div>
                  <el-tag size="small" type="info" effect="plain">台账</el-tag>
                </div>

                <div class="mt-2 text-xs text-slate-500 dark:text-slate-400">
                  已删除：<span
                    class="font-medium text-slate-700 dark:text-slate-200"
                    >{{ formatNumber(summary.assetDelTotal) }}</span
                  >
                </div>
                <div class="mt-1 text-xs text-slate-500 dark:text-slate-400">
                  质保已过期：<span
                    class="font-medium text-rose-600 dark:text-rose-300"
                    >{{ formatNumber(summary.assetWarrantyOverdueTotal) }}</span
                  >
                  ，{{ summary.dueSoonDays }} 天内到期：<span
                    class="font-medium text-amber-600 dark:text-amber-300"
                    >{{ formatNumber(summary.assetWarrantyDueSoonTotal) }}</span
                  >
                </div>
              </el-card>
            </el-col>

            <el-col :xs="24" :sm="12" :md="8" :lg="6" :xl="6">
              <el-card
                shadow="never"
                class="relative overflow-hidden bg-white/70 transition hover:-translate-y-[1px] hover:shadow-md dark:bg-slate-900/30"
              >
                <div
                  class="pointer-events-none absolute -right-10 -top-10 h-28 w-28 rounded-full bg-emerald-500/20 blur-2xl"
                />
                <div class="flex items-start justify-between gap-2">
                  <div
                    class="flex items-center gap-2 text-xs text-slate-500 dark:text-slate-400"
                  >
                    <span
                      class="inline-flex h-7 w-7 items-center justify-center rounded-[10px] bg-emerald-500/10 text-emerald-600 dark:text-emerald-300"
                    >
                      <el-icon :size="16"><Coin /></el-icon>
                    </span>
                    <span>资产价值</span>
                  </div>
                  <el-tag size="small" type="success" effect="plain"
                    >金额</el-tag
                  >
                </div>

                <div class="mt-3 text-xs text-slate-500 dark:text-slate-400">
                  资产原值总额
                </div>
                <div
                  class="mt-1 text-lg font-semibold text-slate-900 dark:text-slate-100"
                >
                  {{ toMoney(summary.assetOriginalValueTotal) }}
                </div>
                <div class="mt-3 text-xs text-slate-500 dark:text-slate-400">
                  资产净值总额
                </div>
                <div
                  class="mt-1 text-lg font-semibold text-slate-900 dark:text-slate-100"
                >
                  {{ toMoney(summary.assetNetBookValueTotal) }}
                </div>
              </el-card>
            </el-col>

            <el-col :xs="24" :sm="12" :md="8" :lg="6" :xl="6">
              <el-card
                shadow="never"
                class="relative overflow-hidden bg-white/70 transition hover:-translate-y-[1px] hover:shadow-md dark:bg-slate-900/30"
              >
                <div
                  class="pointer-events-none absolute -right-10 -top-10 h-28 w-28 rounded-full bg-violet-500/20 blur-2xl"
                />
                <div class="flex items-start justify-between gap-2">
                  <div>
                    <div
                      class="flex items-center gap-2 text-xs text-slate-500 dark:text-slate-400"
                    >
                      <span
                        class="inline-flex h-7 w-7 items-center justify-center rounded-[10px] bg-violet-500/10 text-violet-600 dark:text-violet-300"
                      >
                        <el-icon :size="16"><UserFilled /></el-icon>
                      </span>
                      <span>供应商</span>
                    </div>
                    <div
                      class="mt-2 text-2xl font-semibold text-slate-900 dark:text-slate-100"
                    >
                      {{ formatNumber(summary.vendorTotal) }}
                    </div>
                  </div>
                  <el-tag size="small" type="warning" effect="plain"
                    >主数据</el-tag
                  >
                </div>
                <div class="mt-2 text-xs text-slate-500 dark:text-slate-400">
                  启用：<span
                    class="font-medium text-slate-700 dark:text-slate-200"
                    >{{ formatNumber(summary.vendorEnabledTotal) }}</span
                  >
                </div>
              </el-card>
            </el-col>

            <el-col :xs="24" :sm="12" :md="8" :lg="6" :xl="6">
              <el-card
                shadow="never"
                class="relative overflow-hidden bg-white/70 transition hover:-translate-y-[1px] hover:shadow-md dark:bg-slate-900/30"
              >
                <div
                  class="pointer-events-none absolute -right-10 -top-10 h-28 w-28 rounded-full bg-cyan-500/20 blur-2xl"
                />
                <div class="flex items-start justify-between gap-2">
                  <div>
                    <div
                      class="flex items-center gap-2 text-xs text-slate-500 dark:text-slate-400"
                    >
                      <span
                        class="inline-flex h-7 w-7 items-center justify-center rounded-[10px] bg-cyan-500/10 text-cyan-700 dark:text-cyan-300"
                      >
                        <el-icon :size="16"><LocationFilled /></el-icon>
                      </span>
                      <span>地点 / 仓库 / 库位</span>
                    </div>
                    <div
                      class="mt-2 text-2xl font-semibold text-slate-900 dark:text-slate-100"
                    >
                      {{ formatNumber(summary.locationTotal) }}
                    </div>
                  </div>
                  <el-tag size="small" type="info" effect="plain">位置</el-tag>
                </div>

                <div class="mt-2 text-xs text-slate-500 dark:text-slate-400">
                  地点启用：<span
                    class="font-medium text-slate-700 dark:text-slate-200"
                    >{{ formatNumber(summary.locationEnabledTotal) }}</span
                  >
                </div>
                <div class="mt-1 text-xs text-slate-500 dark:text-slate-400">
                  仓库：<span
                    class="font-medium text-slate-700 dark:text-slate-200"
                    >{{ formatNumber(summary.warehouseTotal) }}</span
                  >（启用
                  <span
                    class="font-medium text-slate-700 dark:text-slate-200"
                    >{{ formatNumber(summary.warehouseEnabledTotal) }}</span
                  >），库位：<span
                    class="font-medium text-slate-700 dark:text-slate-200"
                    >{{ formatNumber(summary.warehouseBinTotal) }}</span
                  >
                </div>
              </el-card>
            </el-col>
          </el-row>

          <el-row :gutter="12" class="mt-3">
            <el-col :xs="24" :sm="12" :md="8" :lg="6" :xl="6">
              <el-card
                shadow="never"
                class="relative overflow-hidden bg-white/70 transition hover:-translate-y-[1px] hover:shadow-md dark:bg-slate-900/30"
              >
                <div
                  class="pointer-events-none absolute -right-10 -top-10 h-28 w-28 rounded-full bg-indigo-500/20 blur-2xl"
                />
                <div class="flex items-start justify-between gap-2">
                  <div>
                    <div
                      class="flex items-center gap-2 text-xs text-slate-500 dark:text-slate-400"
                    >
                      <span
                        class="inline-flex h-7 w-7 items-center justify-center rounded-[10px] bg-indigo-500/10 text-indigo-600 dark:text-indigo-300"
                      >
                        <el-icon :size="16"><List /></el-icon>
                      </span>
                      <span>盘点计划</span>
                    </div>
                    <div
                      class="mt-2 text-2xl font-semibold text-slate-900 dark:text-slate-100"
                    >
                      {{ formatNumber(summary.inventoryPlanTotal) }}
                    </div>
                  </div>
                  <el-tag size="small" type="primary" effect="plain"
                    >盘点</el-tag
                  >
                </div>
                <div class="mt-2 text-xs text-slate-500 dark:text-slate-400">
                  进行中：<span
                    class="font-medium text-slate-700 dark:text-slate-200"
                    >{{ formatNumber(summary.inventoryPlanRunningTotal) }}</span
                  >
                </div>
              </el-card>
            </el-col>

            <el-col :xs="24" :sm="12" :md="8" :lg="6" :xl="6">
              <el-card
                shadow="never"
                class="relative overflow-hidden bg-white/70 transition hover:-translate-y-[1px] hover:shadow-md dark:bg-slate-900/30"
              >
                <div
                  class="pointer-events-none absolute -right-10 -top-10 h-28 w-28 rounded-full bg-teal-500/20 blur-2xl"
                />
                <div class="flex items-start justify-between gap-2">
                  <div>
                    <div
                      class="flex items-center gap-2 text-xs text-slate-500 dark:text-slate-400"
                    >
                      <span
                        class="inline-flex h-7 w-7 items-center justify-center rounded-[10px] bg-teal-500/10 text-teal-700 dark:text-teal-300"
                      >
                        <el-icon :size="16"><Calendar /></el-icon>
                      </span>
                      <span>保养计划</span>
                    </div>
                    <div
                      class="mt-2 text-2xl font-semibold text-slate-900 dark:text-slate-100"
                    >
                      {{ formatNumber(summary.maintenancePlanTotal) }}
                    </div>
                  </div>
                  <el-tag size="small" type="success" effect="plain"
                    >保养</el-tag
                  >
                </div>
                <div class="mt-2 text-xs text-slate-500 dark:text-slate-400">
                  启用：<span
                    class="font-medium text-slate-700 dark:text-slate-200"
                    >{{
                      formatNumber(summary.maintenancePlanEnabledTotal)
                    }}</span
                  >
                </div>
              </el-card>
            </el-col>

            <el-col :xs="24" :sm="12" :md="8" :lg="6" :xl="6">
              <el-card
                shadow="never"
                class="relative overflow-hidden bg-white/70 transition hover:-translate-y-[1px] hover:shadow-md dark:bg-slate-900/30"
              >
                <div
                  class="pointer-events-none absolute -right-10 -top-10 h-28 w-28 rounded-full bg-amber-500/20 blur-2xl"
                />
                <div class="flex items-start justify-between gap-2">
                  <div>
                    <div
                      class="flex items-center gap-2 text-xs text-slate-500 dark:text-slate-400"
                    >
                      <span
                        class="inline-flex h-7 w-7 items-center justify-center rounded-[10px] bg-amber-500/10 text-amber-700 dark:text-amber-300"
                      >
                        <el-icon :size="16"><Tools /></el-icon>
                      </span>
                      <span>维修/保养工单</span>
                    </div>
                    <div
                      class="mt-2 text-2xl font-semibold text-slate-900 dark:text-slate-100"
                    >
                      {{ formatNumber(summary.maintenanceOrderTotal) }}
                    </div>
                  </div>
                  <el-tag size="small" type="warning" effect="plain"
                    >工单</el-tag
                  >
                </div>
                <div class="mt-2 text-xs text-slate-500 dark:text-slate-400">
                  待处理：<span
                    class="font-medium text-slate-700 dark:text-slate-200"
                    >{{ formatNumber(summary.maintenanceOrderOpenTotal) }}</span
                  >
                </div>
              </el-card>
            </el-col>

            <el-col :xs="24" :sm="12" :md="8" :lg="6" :xl="6">
              <el-card
                shadow="never"
                class="relative overflow-hidden bg-white/70 transition hover:-translate-y-[1px] hover:shadow-md dark:bg-slate-900/30"
              >
                <div
                  class="pointer-events-none absolute -right-10 -top-10 h-28 w-28 rounded-full bg-rose-500/20 blur-2xl"
                />
                <div class="flex items-start justify-between gap-2">
                  <div>
                    <div
                      class="flex items-center gap-2 text-xs text-slate-500 dark:text-slate-400"
                    >
                      <span
                        class="inline-flex h-7 w-7 items-center justify-center rounded-[10px] bg-rose-500/10 text-rose-600 dark:text-rose-300"
                      >
                        <el-icon :size="16"><BellFilled /></el-icon>
                      </span>
                      <span>提醒任务</span>
                    </div>
                    <div
                      class="mt-2 text-2xl font-semibold text-slate-900 dark:text-slate-100"
                    >
                      {{ formatNumber(summary.reminderTaskTotal) }}
                    </div>
                  </div>
                  <el-tag size="small" type="danger" effect="plain"
                    >提醒</el-tag
                  >
                </div>
                <div class="mt-2 text-xs text-slate-500 dark:text-slate-400">
                  未关闭：<span
                    class="font-medium text-slate-700 dark:text-slate-200"
                    >{{ formatNumber(summary.reminderTaskOpenTotal) }}</span
                  >，已逾期：<span
                    class="font-medium text-rose-600 dark:text-rose-300"
                    >{{ formatNumber(summary.reminderTaskOverdueTotal) }}</span
                  >
                </div>
                <div class="mt-1 text-xs text-slate-500 dark:text-slate-400">
                  {{ summary.dueSoonDays }} 天内到期：<span
                    class="font-medium text-amber-600 dark:text-amber-300"
                    >{{ formatNumber(summary.reminderTaskDueSoonTotal) }}</span
                  >
                </div>
              </el-card>
            </el-col>
          </el-row>

          <el-row :gutter="12" class="mt-3">
            <el-col :xs="24" :sm="12" :md="8" :lg="6" :xl="6">
              <el-card
                shadow="never"
                class="relative overflow-hidden bg-white/70 transition hover:-translate-y-[1px] hover:shadow-md dark:bg-slate-900/30"
              >
                <div
                  class="pointer-events-none absolute -right-10 -top-10 h-28 w-28 rounded-full bg-slate-500/20 blur-2xl"
                />
                <div class="flex items-start justify-between gap-2">
                  <div>
                    <div
                      class="flex items-center gap-2 text-xs text-slate-500 dark:text-slate-400"
                    >
                      <span
                        class="inline-flex h-7 w-7 items-center justify-center rounded-[10px] bg-slate-500/10 text-slate-600 dark:text-slate-300"
                      >
                        <el-icon :size="16"><Document /></el-icon>
                      </span>
                      <span>单据</span>
                    </div>
                    <div
                      class="mt-2 text-2xl font-semibold text-slate-900 dark:text-slate-100"
                    >
                      {{ formatNumber(summary.docTotal) }}
                    </div>
                  </div>
                  <el-tag size="small" type="info" effect="plain">业务</el-tag>
                </div>
              </el-card>
            </el-col>

            <el-col :xs="24" :sm="12" :md="8" :lg="6" :xl="6">
              <el-card
                shadow="never"
                class="relative overflow-hidden bg-white/70 transition hover:-translate-y-[1px] hover:shadow-md dark:bg-slate-900/30"
              >
                <div
                  class="pointer-events-none absolute -right-10 -top-10 h-28 w-28 rounded-full bg-fuchsia-500/20 blur-2xl"
                />
                <div class="flex items-start justify-between gap-2">
                  <div>
                    <div
                      class="flex items-center gap-2 text-xs text-slate-500 dark:text-slate-400"
                    >
                      <span
                        class="inline-flex h-7 w-7 items-center justify-center rounded-[10px] bg-fuchsia-500/10 text-fuchsia-700 dark:text-fuchsia-300"
                      >
                        <el-icon :size="16"><Tickets /></el-icon>
                      </span>
                      <span>折旧配置(资产)</span>
                    </div>
                    <div
                      class="mt-2 text-2xl font-semibold text-slate-900 dark:text-slate-100"
                    >
                      {{ formatNumber(summary.assetDepreciationTotal) }}
                    </div>
                  </div>
                  <el-tag size="small" type="primary" effect="plain"
                    >折旧</el-tag
                  >
                </div>
                <div class="mt-2 text-xs text-slate-500 dark:text-slate-400">
                  折旧中：<span
                    class="font-medium text-slate-700 dark:text-slate-200"
                    >{{
                      formatNumber(summary.assetDepreciationEnabledTotal)
                    }}</span
                  >
                </div>
                <div class="mt-1 text-xs text-slate-500 dark:text-slate-400">
                  累计折旧：<span
                    class="font-medium text-slate-700 dark:text-slate-200"
                    >{{
                      toMoney(summary.assetDepreciationAccumAmountTotal)
                    }}</span
                  >
                </div>
              </el-card>
            </el-col>

            <el-col :xs="24" :sm="12" :md="8" :lg="6" :xl="6">
              <el-card
                shadow="never"
                class="relative overflow-hidden bg-white/70 transition hover:-translate-y-[1px] hover:shadow-md dark:bg-slate-900/30"
              >
                <div
                  class="pointer-events-none absolute -right-10 -top-10 h-28 w-28 rounded-full bg-orange-500/20 blur-2xl"
                />
                <div class="flex items-start justify-between gap-2">
                  <div>
                    <div
                      class="flex items-center gap-2 text-xs text-slate-500 dark:text-slate-400"
                    >
                      <span
                        class="inline-flex h-7 w-7 items-center justify-center rounded-[10px] bg-orange-500/10 text-orange-700 dark:text-orange-300"
                      >
                        <el-icon :size="16"><Timer /></el-icon>
                      </span>
                      <span>折旧计提批次</span>
                    </div>
                    <div
                      class="mt-2 text-2xl font-semibold text-slate-900 dark:text-slate-100"
                    >
                      {{ formatNumber(summary.depreciationRunTotal) }}
                    </div>
                  </div>
                  <el-tag size="small" type="warning" effect="plain"
                    >期间</el-tag
                  >
                </div>
                <div class="mt-2 text-xs text-slate-500 dark:text-slate-400">
                  最近期间：<span
                    class="font-medium text-slate-700 dark:text-slate-200"
                    >{{ summary.lastDepreciationRunPeriod || "-" }}</span
                  >
                </div>
                <div class="mt-1 text-xs text-slate-500 dark:text-slate-400">
                  计提合计：<span
                    class="font-medium text-slate-700 dark:text-slate-200"
                    >{{ toMoney(summary.depreciationRunTotalAmountAll) }}</span
                  >
                </div>
              </el-card>
            </el-col>

            <el-col :xs="24" :sm="12" :md="8" :lg="6" :xl="6">
              <el-card
                shadow="never"
                class="relative overflow-hidden bg-white/70 transition hover:-translate-y-[1px] hover:shadow-md dark:bg-slate-900/30"
              >
                <div
                  class="pointer-events-none absolute -right-10 -top-10 h-28 w-28 rounded-full bg-pink-500/20 blur-2xl"
                />
                <div class="flex items-start justify-between gap-2">
                  <div>
                    <div
                      class="flex items-center gap-2 text-xs text-slate-500 dark:text-slate-400"
                    >
                      <span
                        class="inline-flex h-7 w-7 items-center justify-center rounded-[10px] bg-pink-500/10 text-pink-700 dark:text-pink-300"
                      >
                        <el-icon :size="16"><Bell /></el-icon>
                      </span>
                      <span>提醒规则</span>
                    </div>
                    <div
                      class="mt-2 text-2xl font-semibold text-slate-900 dark:text-slate-100"
                    >
                      {{ formatNumber(summary.reminderRuleTotal) }}
                    </div>
                  </div>
                  <el-tag size="small" type="danger" effect="plain"
                    >规则</el-tag
                  >
                </div>
                <div class="mt-2 text-xs text-slate-500 dark:text-slate-400">
                  启用：<span
                    class="font-medium text-slate-700 dark:text-slate-200"
                    >{{ formatNumber(summary.reminderRuleEnabledTotal) }}</span
                  >
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
          <template #header>
            <div class="flex items-center justify-between">
              <div class="flex items-center gap-2 font-medium">
                <span
                  class="inline-flex h-8 w-8 items-center justify-center rounded-[10px] bg-blue-500/10 text-blue-600 dark:text-blue-300"
                >
                  <el-icon :size="16"><PieChart /></el-icon>
                </span>
                <span>资产状态分布</span>
              </div>
              <el-tag size="small" type="info" effect="plain"
                >总计 {{ formatNumber(summary.assetTotal) }}</el-tag
              >
            </div>
          </template>
          <soaChart :options="assetStatusOptions" :height="260" />
        </el-card>
      </el-col>

      <el-col :xs="24" :sm="24" :md="12" :lg="12" :xl="12">
        <el-card
          shadow="never"
          class="h-full bg-card border-border rounded-[.5vw] !border-slate-200/80 dark:!border-slate-750"
        >
          <template #header>
            <div class="flex items-center justify-between">
              <div class="flex items-center gap-2 font-medium">
                <span
                  class="inline-flex h-8 w-8 items-center justify-center rounded-[10px] bg-amber-500/10 text-amber-700 dark:text-amber-300"
                >
                  <el-icon :size="16"><TrendCharts /></el-icon>
                </span>
                <span>工单状态分布</span>
              </div>
              <el-tag size="small" type="warning" effect="plain"
                >待处理
                {{ formatNumber(summary.maintenanceOrderOpenTotal) }}</el-tag
              >
            </div>
          </template>
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
          <template #header>
            <div class="flex items-center justify-between">
              <div class="flex items-center gap-2 font-medium">
                <span
                  class="inline-flex h-8 w-8 items-center justify-center rounded-[10px] bg-emerald-500/10 text-emerald-600 dark:text-emerald-300"
                >
                  <el-icon :size="16"><Histogram /></el-icon>
                </span>
                <span>{{ summary.year }} 年新增资产趋势</span>
              </div>
              <el-tag size="small" type="success" effect="plain">年度</el-tag>
            </div>
          </template>
          <soaChart :options="assetCreatedOptions" :height="280" />
        </el-card>
      </el-col>

      <el-col :xs="24" :sm="24" :md="10" :lg="10" :xl="10">
        <el-card
          shadow="never"
          class="h-full bg-card border-border rounded-[.5vw] !border-slate-200/80 dark:!border-slate-750"
        >
          <template #header>
            <div class="flex items-center justify-between">
              <div class="flex items-center gap-2 font-medium">
                <span
                  class="inline-flex h-8 w-8 items-center justify-center rounded-[10px] bg-violet-500/10 text-violet-600 dark:text-violet-300"
                >
                  <el-icon :size="16"><CollectionTag /></el-icon>
                </span>
                <span>资产分类 Top 10</span>
              </div>
              <el-tag size="small" type="info" effect="plain">分类</el-tag>
            </div>
          </template>
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
          <template #header>
            <div class="flex items-center justify-between">
              <div class="flex items-center gap-2 font-medium">
                <span
                  class="inline-flex h-8 w-8 items-center justify-center rounded-[10px] bg-indigo-500/10 text-indigo-600 dark:text-indigo-300"
                >
                  <el-icon :size="16"><List /></el-icon>
                </span>
                <span>盘点计划状态分布</span>
              </div>
              <el-tag size="small" type="primary" effect="plain"
                >进行中
                {{ formatNumber(summary.inventoryPlanRunningTotal) }}</el-tag
              >
            </div>
          </template>
          <soaChart :options="inventoryPlanStatusOptions" :height="260" />
        </el-card>
      </el-col>

      <el-col :xs="24" :sm="24" :md="12" :lg="12" :xl="12">
        <el-card
          shadow="never"
          class="h-full bg-card border-border rounded-[.5vw] !border-slate-200/80 dark:!border-slate-750"
        >
          <template #header>
            <div class="flex items-center justify-between">
              <div class="flex items-center gap-2 font-medium">
                <span
                  class="inline-flex h-8 w-8 items-center justify-center rounded-[10px] bg-rose-500/10 text-rose-600 dark:text-rose-300"
                >
                  <el-icon :size="16"><BellFilled /></el-icon>
                </span>
                <span>提醒任务状态分布</span>
              </div>
              <el-tag size="small" type="danger" effect="plain"
                >逾期
                {{ formatNumber(summary.reminderTaskOverdueTotal) }}</el-tag
              >
            </div>
          </template>
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
          <template #header>
            <div class="flex items-center justify-between">
              <div class="flex items-center gap-2 font-medium">
                <span
                  class="inline-flex h-8 w-8 items-center justify-center rounded-[10px] bg-amber-500/10 text-amber-700 dark:text-amber-300"
                >
                  <el-icon :size="16"><Tools /></el-icon>
                </span>
                <span>工单类型分布</span>
              </div>
              <el-tag size="small" type="warning" effect="plain"
                >总计 {{ formatNumber(summary.maintenanceOrderTotal) }}</el-tag
              >
            </div>
          </template>
          <soaChart :options="maintenanceOrderTypeOptions" :height="240" />
        </el-card>
      </el-col>
    </el-row>
  </div>
</template>
