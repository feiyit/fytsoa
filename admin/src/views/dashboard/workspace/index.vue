<script setup lang="ts">
import dayjs from "dayjs";
import { useRouter } from "vue-router";
import { useUserStore } from "@/stores/user";
import { appStorage } from "@/utils/storage";
import { createLineChartOptions } from "@/component/soaChart/useSoaChart";
import { fetchWorkspace, fetchArticleView } from "@/api/work";
import { formatRelativeDateDetail } from "@/utils/tools";

defineOptions({ name: "WorkspaceView" });

const userStore = useUserStore();
const router = useRouter();

// ========== 顶部欢迎信息 ==========
const now = ref(new Date());
let timer: number | undefined;

onMounted(() => {
  timer = window.setInterval(() => {
    now.value = new Date();
  }, 1000);
  initTotal();
  initChart();
});

onBeforeUnmount(() => {
  if (timer) {
    window.clearInterval(timer);
  }
});

const greeting = computed(() => {
  const hour = now.value.getHours();
  if (hour < 6) return "凌晨好";
  if (hour < 12) return "上午好";
  if (hour < 14) return "中午好";
  if (hour < 18) return "下午好";
  if (hour < 22) return "晚上好";
  return "夜深了";
});

const welcomeTitle = computed(
  () => `${greeting.value}，${userStore.displayName || "欢迎使用"}`
);

const dateTimeText = computed(() =>
  dayjs(now.value).format("YYYY-MM-DD HH:mm:ss")
);

const weekdayText = computed(() => {
  const map = [
    "星期日",
    "星期一",
    "星期二",
    "星期三",
    "星期四",
    "星期五",
    "星期六",
  ];
  return map[now.value.getDay()];
});

// ========== 快捷功能 ==========
interface QuickRouteItem {
  path: string;
  name: string;
  title: string;
  icon?: string;
}

const QUICK_STORAGE_KEY = "dashboard-quick-routes";

const allRouteCandidates = computed<QuickRouteItem[]>(() => {
  return router
    .getRoutes()
    .filter((r) => {
      // 只做业务路由：有 meta.title，且不是布局、登录或 404
      if (!r.meta?.title) return false;
      if (!r.path || r.path === "/" || r.path === "/login") return false;
      if (
        String(r.name || "")
          .toLowerCase()
          .includes("notfound")
      )
        return false;
      // 只取叶子路由，避免重复
      if (r.children && r.children.length) return false;
      return true;
    })
    .map((r) => ({
      path: r.path,
      name: String(r.name || r.path),
      title: (r.meta?.title as string) || String(r.name || r.path),
      icon: r.meta?.icon as string | undefined,
    }));
});

const storedQuickKeys = ref<string[]>(
  appStorage.get<string[]>(QUICK_STORAGE_KEY, []) ?? []
);

const quickRoutes = computed<QuickRouteItem[]>(() => {
  const all = allRouteCandidates.value;
  if (storedQuickKeys.value.length) {
    const map = new Map(all.map((r) => [r.name, r]));
    return storedQuickKeys.value
      .map((key) => map.get(key))
      .filter((v): v is QuickRouteItem => !!v);
  }
  // 默认取前 12 个作为快捷入口
  return all.slice(0, 12);
});

const isPinned = (item: QuickRouteItem) =>
  storedQuickKeys.value.includes(item.name);

const togglePin = (item: QuickRouteItem) => {
  const idx = storedQuickKeys.value.indexOf(item.name);
  if (idx === -1) {
    storedQuickKeys.value.push(item.name);
  } else {
    storedQuickKeys.value.splice(idx, 1);
  }
  appStorage.set(QUICK_STORAGE_KEY, storedQuickKeys.value);
};

const handleShortcutClick = (item: QuickRouteItem) => {
  router.push(item.path);
};

const currentYear = new Date().getFullYear();
const yearOptions = [
  currentYear,
  currentYear + 1,
  currentYear + 2,
  currentYear + 3,
];
const selectedYear = ref<number>(currentYear);
const viewType = ref<"year" | "month">("year");
const selectedMonth = ref<number>(new Date().getMonth() + 1);

const articleChartOptions = ref({});

const monthOptions = Array.from({ length: 12 }, (_, i) => i + 1);

interface ChartDataItem {
  name: string;
  pv: number;
  uv: number;
  value: number;
}
const parseChartData = (res: ChartDataItem[], viewType: string) => {
  const sortedData = [...res].sort((a, b) => {
    const numA = Number(a.name.replace(/月|日/g, ""));
    const numB = Number(b.name.replace(/月|日/g, ""));
    return numA - numB;
  });

  const xAxis = sortedData.map((item) => item.name);
  const pvData = sortedData.map((item) => item.value);
  const uvData = sortedData.map((item) => item.value);

  return { xAxis, pvData, uvData };
};
const initChart = async () => {
  const res = await fetchArticleView({
    year: selectedYear.value,
    month: selectedMonth.value,
    type: viewType.value == "year" ? 1 : 2,
  });
  const { xAxis, pvData, uvData } = parseChartData(res, viewType.value);
  let seriesPvData = [];
  let seriesUvData = [];

  if (viewType.value === "year") {
    // 年视图因子逻辑
    const factor = selectedYear.value === currentYear ? 1 : 0.85;
    seriesPvData = pvData.map((v) => Math.round(v * factor));
    seriesUvData = uvData.map((v) => Math.round(v * factor));
  } else {
    // 月视图因子逻辑
    const monthFactor = (selectedMonth.value % 6) / 10 + 0.7;
    seriesPvData = pvData.map((v) => Math.round(v * monthFactor));
    seriesUvData = uvData.map((v) => Math.round(v * monthFactor));
  }
  articleChartOptions.value = createLineChartOptions({
    title:
      viewType.value === "year"
        ? `${selectedYear.value} 年文章浏览趋势`
        : `${selectedYear.value} 年 ${selectedMonth.value} 月文章浏览趋势`,
    xAxis: xAxis,
    series: [
      { name: "PV", data: seriesPvData },
      { name: "UV", data: seriesUvData },
    ],
  });
};

const groupChange = () => {
  initChart();
};
// ========== CMS 内容概览（静态示例） ==========
const cmsSummary = reactive({
  articleTotal: 1280,
  articleToday: 18,
  articlePending: 12,
  draftCount: 34,
  commentPending: 9,
  advOnline: 16,
});
const latestContent = ref([]);
const initTotal = async () => {
  const res = await fetchWorkspace();
  cmsSummary.articleTotal = res.articleCountAll;
  cmsSummary.articleToday = res.articleCountDay;
  cmsSummary.articlePending = res.articleCountCheck;
  cmsSummary.draftCount = res.articleCountDraft;
  cmsSummary.commentPending = res.articleCountComment;
  latestContent.value = res.newestArticle;
};
</script>

<template>
  <div class="space-y-2">
    <!-- 顶部欢迎 + 内容概览 -->
    <el-row :gutter="12">
      <el-col :xs="24" :sm="24" :md="24" :lg="16" :xl="16">
        <el-card
          shadow="never"
          class="h-full bg-card border-border rounded-[.5vw] !border-slate-200/80 dark:!border-slate-750"
        >
          <div class="flex items-center justify-between gap-4">
            <div class="flex items-center gap-3">
              <el-avatar :size="40">
                {{ userStore.displayName.slice(0, 1).toUpperCase() || "U" }}
              </el-avatar>
              <div>
                <div class="text-base font-semibold">
                  {{ welcomeTitle }}
                </div>
                <div
                  class="mt-1 space-x-2 text-sm text-slate-500 dark:text-slate-400"
                >
                  <span>{{ dateTimeText }}</span>
                  <span
                    class="inline-block h-1 w-1 rounded-full bg-slate-400"
                  />
                  <span>{{ weekdayText }}</span>
                </div>
              </div>
            </div>

            <div
              class="flex flex-col items-end text-sm text-slate-500 dark:text-slate-400"
            >
              <span>今日文章：{{ cmsSummary.articleToday }}</span>
              <span>待审核：{{ cmsSummary.articlePending }}</span>
            </div>
          </div>
        </el-card>
      </el-col>

      <el-col :xs="24" :sm="24" :md="24" :lg="8" :xl="8">
        <el-card
          shadow="never"
          class="h-full bg-card border-border rounded-[.5vw] !border-slate-200/80 dark:!border-slate-750"
        >
          <div
            class="mb-2 flex items-center text-base gap-2 text-slate-700 dark:text-slate-200"
          >
            <el-icon :size="18" class="text-primary-500">
              <DataAnalysis />
            </el-icon>
            <span>内容概览</span>
          </div>
          <div class="grid grid-cols-3 gap-3 text-center">
            <div>
              <div
                class="text-lg font-semibold text-slate-900 dark:text-slate-50"
              >
                {{ cmsSummary.articleTotal }}
              </div>
              <div class="mt-1 text-xs text-slate-500 dark:text-slate-400">
                文章总数
              </div>
            </div>
            <div>
              <div class="text-lg font-semibold text-emerald-500">
                {{ cmsSummary.draftCount }}
              </div>
              <div class="mt-1 text-xs text-slate-500 dark:text-slate-400">
                草稿箱
              </div>
            </div>
            <div>
              <div class="text-lg font-semibold text-amber-500">
                {{ cmsSummary.commentPending }}
              </div>
              <div class="mt-1 text-xs text-slate-500 dark:text-slate-400">
                待审核评论
              </div>
            </div>
          </div>
        </el-card>
      </el-col>
    </el-row>

    <!-- 快捷功能 + 浏览趋势 -->
    <el-row :gutter="12">
      <el-col :xs="24" :sm="24" :md="24" :lg="14" :xl="14">
        <el-card
          shadow="never"
          class="h-full bg-card border-border rounded-[.5vw] !border-slate-200/80 dark:!border-slate-750"
        >
          <div class="mb-2 flex items-center justify-between">
            <div class="flex items-center text-base gap-2 font-medium">
              <el-icon :size="18" class="text-primary-500">
                <TrendCharts />
              </el-icon>
              <span>文章浏览趋势</span>
            </div>
            <div class="flex items-center gap-2 text-[13px]">
              <el-radio-group v-model="viewType" @change="groupChange">
                <el-radio-button label="year">按年</el-radio-button>
                <el-radio-button label="month">按月</el-radio-button>
              </el-radio-group>
              <el-select
                v-model="selectedYear"
                style="width: 110px"
                @change="groupChange"
              >
                <el-option
                  v-for="y in yearOptions"
                  :key="y"
                  :label="`${y}年`"
                  :value="y"
                />
              </el-select>
              <el-select
                v-if="viewType === 'month'"
                v-model="selectedMonth"
                style="width: 100px"
                @change="groupChange"
              >
                <el-option
                  v-for="m in monthOptions"
                  :key="m"
                  :label="`${m}月`"
                  :value="m"
                />
              </el-select>
            </div>
          </div>

          <soaChart :options="articleChartOptions" :height="240" />
        </el-card>
      </el-col>

      <el-col :xs="24" :sm="24" :md="24" :lg="10" :xl="10">
        <el-card
          shadow="never"
          class="h-full bg-card border-border rounded-[.5vw] !border-slate-200/80 dark:!border-slate-750"
        >
          <div class="mb-3 flex items-center justify-between">
            <div class="flex text-base items-center gap-2 font-medium">
              <el-icon :size="18" class="text-primary-500">
                <Grid />
              </el-icon>
              <span>功能快捷入口</span>
            </div>
            <div class="text-[13px] text-slate-400">
              点击卡片可跳转，星标可自定义常用入口
            </div>
          </div>

          <div class="grid grid-cols-3 gap-3">
            <button
              v-for="item in quickRoutes"
              :key="item.path"
              type="button"
              class="group flex items-center justify-between rounded-[10px] border border-slate-200/80 bg-white px-3 py-2 text-xs text-slate-600 shadow-sm transition hover:border-primary-400 hover:bg-primary-50/40 dark:border-slate-700 dark:bg-slate-900/80 dark:text-slate-200 dark:hover:border-sky-400 dark:hover:bg-slate-750"
              @click="handleShortcutClick(item)"
            >
              <div class="flex items-center gap-2">
                <el-icon
                  :size="16"
                  class="text-slate-400 group-hover:text-primary-500"
                >
                  <component :is="item.icon || 'Menu'" />
                </el-icon>
                <span class="truncate">{{ item.title }}</span>
              </div>
              <el-icon
                :size="14"
                class="text-slate-300 hover:text-amber-400"
                @click.stop="togglePin(item)"
              >
                <StarFilled v-if="isPinned(item)" />
                <Star v-else />
              </el-icon>
            </button>
          </div>
        </el-card>
      </el-col>
    </el-row>

    <!-- CMS 最新动态 / 待办事项 -->
    <el-row :gutter="12">
      <el-col :xs="24" :sm="24" :md="24" :lg="12" :xl="12">
        <el-card
          shadow="never"
          class="h-full bg-card border-border rounded-[.5vw] !border-slate-200/80 dark:!border-slate-750"
        >
          <div class="mb-2 flex items-center justify-between">
            <div class="flex items-center text-base gap-2 font-medium">
              <el-icon :size="18" class="text-primary-500">
                <Document />
              </el-icon>
              <span>最近内容动态</span>
            </div>
          </div>
          <ul class="space-y-2 text-[13px]">
            <li
              v-for="item in latestContent"
              :key="item.title"
              class="flex items-center justify-between rounded-md bg-slate-50 px-2 py-1.5 text-slate-600 dark:bg-slate-900/80 dark:text-slate-200"
            >
              <div class="truncate">
                <span
                  class="mr-2 inline-block h-1.5 w-1.5 rounded-full bg-emerald-400"
                />
                <span class="truncate align-middle">{{ item.title }}</span>
              </div>
              <div
                class="ml-3 flex flex-shrink-0 items-center gap-3 text-[13px] text-slate-400"
              >
                <span>{{ formatRelativeDateDetail(item.publishTime) }}</span>
                <el-tag size="small" type="info">
                  {{ item.status ? "已发布" : "未发布" }}
                </el-tag>
              </div>
            </li>
          </ul>
        </el-card>
      </el-col>

      <el-col :xs="24" :sm="24" :md="24" :lg="12" :xl="12">
        <el-card
          shadow="never"
          class="h-full bg-card border-border rounded-[.5vw] !border-slate-200/80 dark:!border-slate-750"
        >
          <div class="mb-2 flex items-center justify-between">
            <div class="flex items-center gap-2 text-base font-medium">
              <el-icon :size="18" class="text-primary-500">
                <List />
              </el-icon>
              <span>内容待办</span>
            </div>
          </div>
          <div class="grid grid-cols-2 gap-3 text-[13px]">
            <div class="rounded-lg bg-slate-50 p-3 dark:bg-slate-900/80">
              <div class="mb-1 text-slate-500 dark:text-slate-300">
                待审核文章
              </div>
              <div class="text-2xl font-semibold text-amber-500">
                {{ cmsSummary.articlePending }}
              </div>
              <div class="mt-1 text-[13px] text-slate-400">
                建议优先处理高流量栏目文章
              </div>
            </div>
            <div class="rounded-lg bg-slate-50 p-3 dark:bg-slate-900/80">
              <div class="mb-1 text-slate-500 dark:text-slate-300">
                待处理评论
              </div>
              <div class="text-2xl font-semibold text-rose-500">
                {{ cmsSummary.commentPending }}
              </div>
              <div class="mt-1 text-[13px] text-slate-400">
                建议尽快回复热门文章下的评论
              </div>
            </div>
          </div>
        </el-card>
      </el-col>
    </el-row>
  </div>
</template>
