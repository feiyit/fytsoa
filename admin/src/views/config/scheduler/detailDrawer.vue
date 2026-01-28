<script setup lang="ts">
import dayjs from "dayjs";
import { useSoaDrawer } from "@/component/soaDrawer/index.vue";
import { createBarChartOptions } from "@/component/soaChart/useSoaChart";
import {
  fetchQuartzTaskLogs,
  type QuartzTask,
  type QuartzTaskLog,
} from "@/api/sys/quartz";

defineOptions({ name: "ConfigSchedulerDetailDrawer" });

const title = ref("任务详情");
const task = ref<QuartzTask | null>(null);

const activeTab = ref("detail");

// 日志
const logLoading = ref(false);
const logList = ref<QuartzTaskLog[]>([]);
const logTotal = ref(0);
const logPage = ref(1);
const logSize = ref(20);

const logColumns = [
  { title: "开始时间", dataIndex: "beginDate", key: "beginDate", width: 170 },
  { title: "结束时间", dataIndex: "endDate", key: "endDate", width: 170 },
  {
    title: "耗时",
    dataIndex: "duration",
    key: "duration",
    width: 100,
    align: "center",
  },
  { title: "消息", dataIndex: "msg", key: "msg", minWidth: 200 },
];

const [Drawer, drawerApi] = useSoaDrawer({
  onCancel: () => {
    reset();
    drawerApi.close();
  },
});

const reset = () => {
  task.value = null;
  activeTab.value = "detail";
  logList.value = [];
  logTotal.value = 0;
  logPage.value = 1;
  logSize.value = 20;
};

const statusText = (v?: number) => {
  switch (v) {
    case 4:
      return "暂停";
    case 6:
      return "开启";
    case 5:
      return "停止";
    default:
      return v == null ? "-" : String(v);
  }
};

const statusTagType = (v?: number) => {
  if (v === 6) return "success";
  if (v === 4) return "warning";
  if (v === 5) return "danger";
  return "info";
};

const taskTypeText = (v?: number) => {
  if (v === 2) return "HTTP";
  if (v === 1) return "业务处理器";
  return v == null ? "-" : String(v);
};

const loadLogs = async () => {
  if (!task.value) return;
  logLoading.value = true;
  try {
    const res: any = await fetchQuartzTaskLogs({
      taskName: task.value.taskName,
      groupName: task.value.groupName,
      current: logPage.value,
      size: logSize.value,
    });
    logTotal.value = Number(res?.total || 0);
    logList.value = (res?.data || []) as QuartzTaskLog[];
  } finally {
    logLoading.value = false;
  }
};

watch([logPage, logSize], () => {
  if (task.value && activeTab.value !== "detail") {
    loadLogs();
  }
});

watch(activeTab, (v) => {
  if (v !== "detail" && task.value) {
    // 首次切换到日志/统计时加载
    if (!logList.value.length) loadLogs();
  }
});

const toDurationMs = (log: QuartzTaskLog) => {
  if (!log?.beginDate || !log?.endDate) return null;
  const a = dayjs(log.beginDate);
  const b = dayjs(log.endDate);
  const ms = b.diff(a, "millisecond");
  return Number.isFinite(ms) ? ms : null;
};

const logStats = computed(() => {
  const total = logList.value.length;
  let failed = 0;
  let durationSum = 0;
  let durationCnt = 0;
  const hours = new Array(24).fill(0);

  for (const x of logList.value) {
    const msg = (x.msg || "").toLowerCase();
    if (
      msg.includes("fail") ||
      msg.includes("error") ||
      msg.includes("异常") ||
      msg.includes("错误") ||
      msg.includes("失败")
    ) {
      failed++;
    }
    const ms = toDurationMs(x);
    if (ms != null) {
      durationSum += ms;
      durationCnt++;
    }
    const h = dayjs(x.beginDate).hour();
    if (h >= 0 && h <= 23) hours[h]++;
  }

  const success = total - failed;
  const avgMs = durationCnt ? Math.round(durationSum / durationCnt) : 0;
  return { total, success, failed, avgMs, hours };
});

const hourlyChart = computed(() => {
  const xAxis = Array.from({ length: 24 }, (_, i) => `${i}:00`);
  return createBarChartOptions({
    xAxis,
    series: [{ name: "执行次数", data: logStats.value.hours }],
  });
});

const openModal = async (row: QuartzTask) => {
  reset();
  task.value = row;
  title.value = `任务详情：${row.taskName || ""}`;
  drawerApi.open();
};

defineExpose({ openModal });
</script>

<template>
  <Drawer :title="title" size="70%" :closeOnClickModal="false" :footer="false">
    <div v-if="!task" class="text-slate-400">暂无数据</div>

    <div v-else class="space-y-2">
      <el-tabs v-model="activeTab">
        <el-tab-pane label="详情" name="detail">
          <el-descriptions :column="2" border>
            <el-descriptions-item label="任务名称">
              {{ task.taskName || "-" }}
            </el-descriptions-item>
            <el-descriptions-item label="分组">
              {{ task.groupName || "-" }}
            </el-descriptions-item>

            <el-descriptions-item label="任务类型">
              <el-tag :type="task.taskType === 2 ? 'info' : 'success'">
                {{ taskTypeText(task.taskType) }}
              </el-tag>
            </el-descriptions-item>
            <el-descriptions-item label="状态">
              <el-tag :type="statusTagType(task.status)">
                {{ statusText(task.status) }}
              </el-tag>
            </el-descriptions-item>

            <el-descriptions-item label="Cron" :span="2">
              <el-tag type="info" effect="plain">{{
                task.interval || "-"
              }}</el-tag>
            </el-descriptions-item>

            <el-descriptions-item
              label="HTTP"
              :span="2"
              v-if="task.taskType === 2"
            >
              <div class="space-y-1">
                <div>
                  {{ task.apiRequestType || "GET" }} {{ task.apiUrl || "-" }}
                </div>
                <div class="text-xs text-slate-500">
                  Header：{{ task.apiAuthKey || "-" }}={{
                    task.apiAuthValue || "-"
                  }}
                </div>
                <div class="text-xs text-slate-500">
                  参数：{{ task.apiParameter || "-" }}
                </div>
              </div>
            </el-descriptions-item>

            <el-descriptions-item label="业务处理器" :span="2" v-else>
              {{ task.dllClassName || "-" }}.{{ task.dllActionName || "-" }}
            </el-descriptions-item>

            <el-descriptions-item label="描述" :span="2">
              {{ task.describe || "-" }}
            </el-descriptions-item>

            <el-descriptions-item label="最近执行">
              <span v-if="task.lastRunTime">{{
                dayjs(task.lastRunTime).format("YYYY-MM-DD HH:mm:ss")
              }}</span>
              <span v-else class="text-slate-400">-</span>
            </el-descriptions-item>
            <el-descriptions-item label="更新时间">
              <span v-if="task.changeTime">{{
                dayjs(task.changeTime).format("YYYY-MM-DD HH:mm:ss")
              }}</span>
              <span v-else class="text-slate-400">-</span>
            </el-descriptions-item>
          </el-descriptions>
        </el-tab-pane>

        <el-tab-pane label="执行日志" name="logs">
          <div style="height: 60vh">
            <soaTable
              :columns="logColumns"
              :tableData="logList"
              :loading="logLoading"
              row-key="id"
              :footerShow="false"
              :toolbarShow="false"
              :showSelection="false"
            >
              <template #bodyCell="{ column, record }">
                <template v-if="column.key === 'beginDate'">
                  {{ dayjs(record.beginDate).format("YYYY-MM-DD HH:mm:ss") }}
                </template>
                <template v-else-if="column.key === 'endDate'">
                  <span v-if="record.endDate">
                    {{ dayjs(record.endDate).format("YYYY-MM-DD HH:mm:ss") }}
                  </span>
                  <span v-else class="text-slate-400">-</span>
                </template>
                <template v-else-if="column.key === 'duration'">
                  <span v-if="toDurationMs(record) != null">
                    {{ toDurationMs(record) }}ms
                  </span>
                  <span v-else class="text-slate-400">-</span>
                </template>
                <template v-else-if="column.key === 'msg'">
                  <el-popover
                    v-if="record.msg"
                    trigger="hover"
                    :width="520"
                    placement="top-start"
                  >
                    <template #reference>
                      <div class="truncate">{{ record.msg }}</div>
                    </template>
                    <pre class="whitespace-pre-wrap text-xs">{{
                      record.msg
                    }}</pre>
                  </el-popover>
                  <span v-else class="text-slate-400">-</span>
                </template>
              </template>
            </soaTable>
          </div>

          <div class="mt-3 flex items-center justify-end">
            <el-pagination
              v-model:current-page="logPage"
              v-model:page-size="logSize"
              :page-sizes="[10, 20, 50, 100]"
              layout="total, sizes, prev, pager, next, jumper"
              :total="logTotal"
            />
          </div>
        </el-tab-pane>

        <el-tab-pane label="统计（当前页）" name="stats">
          <div class="grid gap-2 md:grid-cols-4">
            <el-card
              shadow="never"
              class="bg-card border-border rounded-[.5vw]"
            >
              <div class="text-xs text-slate-500">日志条数</div>
              <div class="mt-1 text-2xl font-semibold">
                {{ logStats.total }}
              </div>
            </el-card>
            <el-card
              shadow="never"
              class="bg-card border-border rounded-[.5vw]"
            >
              <div class="text-xs text-slate-500">成功（粗略）</div>
              <div class="mt-1 text-2xl font-semibold">
                {{ logStats.success }}
              </div>
            </el-card>
            <el-card
              shadow="never"
              class="bg-card border-border rounded-[.5vw]"
            >
              <div class="text-xs text-slate-500">失败（粗略）</div>
              <div class="mt-1 text-2xl font-semibold">
                {{ logStats.failed }}
              </div>
            </el-card>
            <el-card
              shadow="never"
              class="bg-card border-border rounded-[.5vw]"
            >
              <div class="text-xs text-slate-500">平均耗时（ms）</div>
              <div class="mt-1 text-2xl font-semibold">
                {{ logStats.avgMs }}
              </div>
            </el-card>
          </div>

          <el-card
            shadow="never"
            class="bg-card border-border mt-2 rounded-[.5vw]"
          >
            <div class="mb-2 flex items-center gap-2 text-sm font-semibold">
              <el-icon><Histogram /></el-icon>
              按小时执行分布（当前页）
            </div>
            <soaChart :option="hourlyChart" style="height: 280px" />
          </el-card>
        </el-tab-pane>
      </el-tabs>
    </div>

    <template #footer>
      <div class="flex justify-end">
        <el-button @click="drawerApi.close()">关闭</el-button>
      </div>
    </template>
  </Drawer>
</template>
