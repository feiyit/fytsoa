<template>
  <el-container class="h-full">
    <el-header
      class="bg-card border-border flex flex-wrap items-center justify-between gap-3 rounded-[.5vw]"
      style="height: auto; padding: 10px 20px"
    >
      <el-form
        :inline="true"
        :model="query"
        class="mb-0 flex flex-wrap items-center gap-3"
      >
        <el-form-item label="关键词">
          <el-input
            v-model="query.key"
            placeholder="任务名/分组/描述/地址/类名..."
            clearable
            style="width: 260px"
          />
        </el-form-item>
        <el-form-item label="分组">
          <el-select
            v-model="query.groupName"
            placeholder="全部"
            clearable
            style="width: 180px"
          >
            <el-option
              v-for="g in groupOptions"
              :key="g"
              :label="g"
              :value="g"
            />
          </el-select>
        </el-form-item>
        <el-form-item label="状态">
          <el-select
            v-model="query.status"
            placeholder="全部"
            clearable
            style="width: 140px"
          >
            <el-option label="开启" :value="6" />
            <el-option label="暂停" :value="4" />
            <el-option label="停止" :value="5" />
          </el-select>
        </el-form-item>
        <el-form-item label="类型">
          <el-select
            v-model="query.taskType"
            placeholder="全部"
            clearable
            style="width: 160px"
          >
            <el-option label="HTTP" :value="2" />
            <el-option label="业务处理器" :value="1" />
          </el-select>
        </el-form-item>
        <el-form-item>
          <el-button type="primary" @click="handleSearch">查询</el-button>
          <el-button @click="handleReset">重置</el-button>
        </el-form-item>
      </el-form>

      <div class="flex flex-wrap items-center gap-2">
        <el-button type="primary" @click="openModify()">
          <el-icon><Plus /></el-icon>
          新建任务
        </el-button>
        <el-button :loading="loading" @click="handleReload">
          <el-icon><Refresh /></el-icon>
          刷新
        </el-button>
      </div>
    </el-header>

    <el-main class="mt-2 space-y-2 !p-0">
      <div class="grid gap-2 px-2 md:grid-cols-4">
        <el-card shadow="never" class="bg-card border-border rounded-[.5vw]">
          <div class="flex items-center justify-between">
            <div>
              <div class="text-xs text-slate-500">任务总数</div>
              <div class="mt-1 text-2xl font-semibold">{{ kpi.total }}</div>
            </div>
            <div
              class="flex h-10 w-10 items-center justify-center rounded-[12px] bg-slate-500/10 text-slate-600 dark:text-slate-300"
            >
              <el-icon><List /></el-icon>
            </div>
          </div>
        </el-card>

        <el-card shadow="never" class="bg-card border-border rounded-[.5vw]">
          <div class="flex items-center justify-between">
            <div>
              <div class="text-xs text-slate-500">开启中</div>
              <div class="mt-1 text-2xl font-semibold">{{ kpi.running }}</div>
            </div>
            <div
              class="flex h-10 w-10 items-center justify-center rounded-[12px] bg-green-500/10 text-green-600 dark:text-green-300"
            >
              <el-icon><CircleCheck /></el-icon>
            </div>
          </div>
        </el-card>

        <el-card shadow="never" class="bg-card border-border rounded-[.5vw]">
          <div class="flex items-center justify-between">
            <div>
              <div class="text-xs text-slate-500">暂停中</div>
              <div class="mt-1 text-2xl font-semibold">{{ kpi.paused }}</div>
            </div>
            <div
              class="flex h-10 w-10 items-center justify-center rounded-[12px] bg-amber-500/10 text-amber-600 dark:text-amber-300"
            >
              <el-icon><VideoPause /></el-icon>
            </div>
          </div>
        </el-card>

        <el-card shadow="never" class="bg-card border-border rounded-[.5vw]">
          <div class="flex items-center justify-between">
            <div>
              <div class="text-xs text-slate-500">HTTP 任务</div>
              <div class="mt-1 text-2xl font-semibold">{{ kpi.httpTotal }}</div>
            </div>
            <div
              class="flex h-10 w-10 items-center justify-center rounded-[12px] bg-sky-500/10 text-sky-600 dark:text-sky-300"
            >
              <el-icon><Link /></el-icon>
            </div>
          </div>
        </el-card>
      </div>

      <div class="bg-card border-border rounded-[.5vw] p-2">
        <soaTable
          ref="tableRef"
          :columns="columns"
          :apiObj="fetchQuartzTaskPage"
          :params="query"
          row-key="id"
          row-serial-number
          :footerShow="true"
        >
          <template #bodyCell="{ column, record }">
            <template v-if="column.key === 'taskName'">
              <div class="flex items-start gap-2">
                <span
                  class="mt-0.5 inline-flex h-8 w-8 items-center justify-center rounded-[10px] bg-slate-500/10 text-slate-600 dark:text-slate-300"
                >
                  <el-icon v-if="record.taskType === 2"><Link /></el-icon>
                  <el-icon v-else><Setting /></el-icon>
                </span>
                <div class="min-w-0">
                  <div class="flex items-center gap-2">
                    <span class="truncate font-medium">{{
                      record.taskName
                    }}</span>
                    <el-tag
                      size="small"
                      :type="record.taskType === 2 ? 'info' : 'success'"
                    >
                      {{ taskTypeText(record.taskType) }}
                    </el-tag>
                  </div>
                  <div class="mt-0.5 truncate text-xs text-slate-500">
                    {{ record.describe || "-" }}
                  </div>
                </div>
              </div>
            </template>

            <template v-else-if="column.key === 'interval'">
              <el-tag type="info" effect="plain">{{ record.interval }}</el-tag>
            </template>

            <template v-else-if="column.key === 'target'">
              <div class="truncate" v-if="record.taskType === 2">
                {{ record.apiRequestType || "GET" }} {{ record.apiUrl || "-" }}
              </div>
              <div class="truncate" v-else>
                {{ record.dllClassName || "-" }}.{{
                  record.dllActionName || "-"
                }}
              </div>
            </template>

            <template v-else-if="column.key === 'status'">
              <el-tag :type="statusTagType(record.status)">
                {{ statusText(record.status) }}
              </el-tag>
            </template>

            <template v-else-if="column.key === 'lastRunTime'">
              <span v-if="record.lastRunTime">
                {{ dayjs(record.lastRunTime).format("YYYY-MM-DD HH:mm:ss") }}
              </span>
              <span v-else class="text-slate-400">-</span>
            </template>

            <template v-else-if="column.key === 'action'">
              <div class="flex items-center gap-1">
                <el-button link type="primary" @click="openModify(record)"
                  >编辑</el-button
                >
                <el-button link type="info" @click="openDetail(record)"
                  >详情</el-button
                >

                <el-button
                  v-if="record.status === 6"
                  link
                  type="warning"
                  @click="handlePause(record)"
                >
                  暂停
                </el-button>
                <el-button
                  v-else
                  link
                  type="success"
                  @click="handleStart(record)"
                >
                  开启
                </el-button>

                <el-button link type="primary" @click="handleRun(record)"
                  >立即执行</el-button
                >
                <el-button link type="danger" @click="handleDelete(record)"
                  >删除</el-button
                >
              </div>
            </template>
          </template>
        </soaTable>
      </div>
    </el-main>

    <modify ref="modifyRef" @complete="handleReload" />
    <detailDrawer ref="detailRef" />
  </el-container>
</template>

<script setup lang="ts">
import dayjs from "dayjs";
import { ElMessage, ElMessageBox } from "element-plus";
import {
  fetchQuartzTaskList,
  pauseQuartzTask,
  startQuartzTask,
  runQuartzTask,
  deleteQuartzTask,
  type QuartzTask,
} from "@/api/sys/quartz";

const modify = defineAsyncComponent(() => import("./modify.vue"));
const detailDrawer = defineAsyncComponent(() => import("./detailDrawer.vue"));

defineOptions({ name: "ConfigSchedulerView" });

const tableRef = ref<any>(null);
const modifyRef = ref<any>(null);
const detailRef = ref<any>(null);

const loading = ref(false);
const allList = ref<QuartzTask[]>([]);
const loadedOnce = ref(false);

const query = reactive({
  page: 1,
  tenantId: 0,
  key: "",
  groupName: "",
  status: undefined as number | undefined,
  taskType: undefined as number | undefined,
});

const statusText = (v?: number) => {
  // FytSoa.Quartz.Enum.JobState
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

const calcFiltered = (source: QuartzTask[], params: any) => {
  const key = String(params?.key || "")
    .trim()
    .toLowerCase();
  const groupName = String(params?.groupName || "").trim();
  const status = params?.status != null ? Number(params.status) : undefined;
  const taskType =
    params?.taskType != null ? Number(params.taskType) : undefined;

  return (source || []).filter((x) => {
    if (groupName && x.groupName !== groupName) return false;
    if (status != null && Number(x.status) !== status) return false;
    if (taskType != null && Number(x.taskType) !== taskType) return false;
    if (!key) return true;

    const hay = [
      x.taskName,
      x.groupName,
      x.describe,
      x.apiUrl,
      x.apiRequestType,
      x.dllClassName,
      x.dllActionName,
      x.interval,
    ]
      .filter(Boolean)
      .join(" ")
      .toLowerCase();
    return hay.includes(key);
  });
};

const groupOptions = computed(() => {
  const set = new Set<string>();
  (allList.value || []).forEach((x) => x.groupName && set.add(x.groupName));
  return Array.from(set).sort((a, b) => a.localeCompare(b));
});

const kpi = computed(() => {
  const total = allList.value.length;
  const running = allList.value.filter((x) => x.status === 6).length;
  const paused = allList.value.filter((x) => x.status === 4).length;
  const httpTotal = allList.value.filter((x) => x.taskType === 2).length;
  return { total, running, paused, httpTotal };
});

const loadAll = async () => {
  loading.value = true;
  try {
    const res = await fetchQuartzTaskList();
    allList.value = (res || []).slice().sort((a, b) => {
      const ga = String(a.groupName || "").localeCompare(
        String(b.groupName || ""),
      );
      if (ga !== 0) return ga;
      return String(a.taskName || "").localeCompare(String(b.taskName || ""));
    });
    loadedOnce.value = true;
  } finally {
    loading.value = false;
  }
};

onMounted(async () => {
  await loadAll();
});

// 让 soaTable 走“分页接口”模式：由前端在内存中完成过滤/分页，统一返回 items/totalItems
const fetchQuartzTaskPage = async (params: any) => {
  if (!loadedOnce.value && !loading.value) {
    await loadAll();
  }

  const page = Number(params?.page || 1);
  const limit = Number(params?.limit || 20);
  const filtered = calcFiltered(allList.value, params);

  const start = (page - 1) * limit;
  const items = filtered.slice(start, start + limit);

  return {
    items,
    totalItems: filtered.length,
  };
};

const columns = [
  { title: "任务", dataIndex: "taskName", key: "taskName", minWidth: 260 },
  { title: "分组", dataIndex: "groupName", key: "groupName", width: 140 },
  { title: "Cron", dataIndex: "interval", key: "interval", minWidth: 180 },
  { title: "目标", dataIndex: "target", key: "target", minWidth: 260 },
  {
    title: "状态",
    dataIndex: "status",
    key: "status",
    width: 110,
    align: "center",
  },
  {
    title: "最近执行",
    dataIndex: "lastRunTime",
    key: "lastRunTime",
    width: 170,
  },
  { title: "操作", key: "action", width: 300, fixed: "right" },
];

const handleSearch = () => {
  query.page = 1;
  tableRef?.value?.upData?.(query);
};

const handleReset = () => {
  query.key = "";
  query.groupName = "";
  query.status = undefined;
  query.taskType = undefined;
  handleSearch();
};

const handleReload = async () => {
  await loadAll();
  tableRef?.value?.refresh?.();
};

const openModify = (row?: QuartzTask) => {
  modifyRef.value?.openModal?.(row);
};

const openDetail = (row: QuartzTask) => {
  detailRef.value?.openModal?.(row);
};

const handlePause = async (row: QuartzTask) => {
  const res = await pauseQuartzTask(row);
  if (res?.message) ElMessage.success(res.message);
  await handleReload();
};

const handleStart = async (row: QuartzTask) => {
  const res = await startQuartzTask(row);
  if (res?.message) ElMessage.success(res.message);
  await handleReload();
};

const handleRun = async (row: QuartzTask) => {
  const res = await runQuartzTask(row);
  if (res?.message) ElMessage.success(res.message);
  await handleReload();
};

const handleDelete = async (row: QuartzTask) => {
  await ElMessageBox.confirm(
    `确认删除任务【${row.taskName}】？删除后不可恢复。`,
    "删除确认",
    { type: "warning" },
  );
  const res = await deleteQuartzTask(row);
  if (res?.message) ElMessage.success(res.message);
  await handleReload();
};
</script>
