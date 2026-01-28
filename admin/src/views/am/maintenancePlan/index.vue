<template>
  <el-container class="h-full">
    <el-header
      class="bg-card border-border flex flex-wrap items-center justify-between gap-3 rounded-[.5vw]"
    >
      <el-form
        :inline="true"
        :model="query"
        class="mb-0 flex flex-wrap items-center gap-3"
      >
        <el-form-item label="关键词">
          <el-input v-model="query.key" placeholder="编号/名称" clearable />
        </el-form-item>
        <el-form-item label="启用">
          <el-select
            v-model="query.enabled"
            placeholder="全部"
            style="width: 120px"
            clearable
          >
            <el-option label="启用" :value="1" />
            <el-option label="停用" :value="2" />
          </el-select>
        </el-form-item>
        <el-form-item>
          <el-button type="primary" @click="handleSearch">查询</el-button>
          <el-button @click="handleReset">重置</el-button>
        </el-form-item>
      </el-form>
      <div>
        <el-popconfirm
          title="确认执行批量删除操作？"
          @confirm="handleBatchDelete"
        >
          <template #reference>
            <el-button type="danger" v-if="selectedRows.length > 0">
              <el-icon><Delete /></el-icon>
              批量删除
            </el-button>
          </template>
        </el-popconfirm>
        <el-button type="primary" @click="openDialog()">
          <el-icon><Plus /></el-icon>
          新增计划
        </el-button>
      </div>
    </el-header>

    <el-main class="bg-card border-border mt-2 rounded-[.5vw]">
      <soaTable
        ref="tableRef"
        :columns="columns"
        :apiObj="fetchAmMaintenancePlanPage"
        row-key="id"
        row-serial-number
        @selection-change="selectionChange"
      >
        <template #bodyCell="{ column, record }">
          <template v-if="column.key === 'planNo'">
            <el-link
              type="primary"
              :underline="false"
              @click="openDetail(record)"
            >
              {{ record.planNo || "-" }}
            </el-link>
          </template>
          <template v-if="column.key === 'managerId'">
            <span class="text-slate-600 dark:text-slate-300">
              {{ formatAdminName(record.managerId) }}
            </span>
          </template>
          <template v-if="column.key === 'nextRunTime'">
            <div class="flex items-center gap-2">
              <span class="text-slate-600 dark:text-slate-300">
                {{ formatDateTime(record.nextRunTime) }}
              </span>
              <span
                v-if="getNextRunBadge(record.nextRunTime)"
                :class="getNextRunBadge(record.nextRunTime)!.class"
              >
                {{ getNextRunBadge(record.nextRunTime)!.text }}
              </span>
            </div>
          </template>
          <template v-if="column.key === 'isEnabled'">
            <el-tag :type="record.isEnabled ? 'success' : 'danger'">
              {{ record.isEnabled ? "启用" : "停用" }}
            </el-tag>
          </template>
          <template v-if="column.key === 'action'">
            <div class="flex items-center gap-1">
              <el-button link type="primary" @click="openDialog(record)"
                >编辑</el-button
              >
              <el-popconfirm
                title="确认删除该计划？"
                @confirm="handleDelete(record.id)"
              >
                <template #reference>
                  <el-button link type="danger">删除</el-button>
                </template>
              </el-popconfirm>
            </div>
          </template>
        </template>
      </soaTable>
    </el-main>

    <modify ref="modifyRef" @complete="handleSearch" />
    <detail ref="detailRef" />
  </el-container>
</template>

<script setup lang="ts">
import dayjs from "dayjs";
import {
  fetchAmMaintenancePlanPage,
  deleteAmMaintenancePlan,
} from "@/api/am/maintenancePlan";
import { fetchAdminList } from "@/api";
const modify = defineAsyncComponent(() => import("./modify.vue"));
const detail = defineAsyncComponent(() => import("./detail.vue"));
const tableRef = ref<any>(null);
const modifyRef = ref<any>(null);
const detailRef = ref<any>(null);
const selectedRows = ref<any[]>([]);
const adminMap = ref<Record<string, string>>({});

const ensureAdminMap = async () => {
  if (Object.keys(adminMap.value).length) return;
  const list = (await fetchAdminList({ page: 1, limit: 1000 })) || [];
  const map: Record<string, string> = {};
  (list || []).forEach((u: any) => {
    const id = String(u?.id ?? "");
    if (!id) return;
    map[id] = u?.fullName || u?.loginAccount || id;
  });
  adminMap.value = map;
};

const getNextRunBadge = (nextRunTime?: string | null) => {
  if (!nextRunTime) return null;
  const target = dayjs(nextRunTime);
  if (!target.isValid()) return null;

  const diffDays = target.startOf("day").diff(dayjs().startOf("day"), "day");
  if (diffDays < 0) {
    return {
      text: "已到期",
      class:
        "inline-flex items-center rounded-full px-2 py-[2px] text-xs font-medium ring-1 ring-inset bg-rose-600 text-white ring-rose-600/80 dark:bg-rose-500 dark:ring-rose-500/70",
    } as const;
  }
  if (diffDays <= 7) {
    return {
      text: `剩${diffDays}天`,
      class:
        "inline-flex items-center rounded-full px-2 py-[2px] text-xs font-medium ring-1 ring-inset bg-rose-50 text-rose-700 ring-rose-200 dark:bg-rose-500/10 dark:text-rose-300 dark:ring-rose-500/20",
    } as const;
  }
  if (diffDays <= 14) {
    return {
      text: `剩${diffDays}天`,
      class:
        "inline-flex items-center rounded-full px-2 py-[2px] text-xs font-medium ring-1 ring-inset bg-amber-50 text-amber-700 ring-amber-200 dark:bg-amber-500/10 dark:text-amber-300 dark:ring-amber-500/20",
    } as const;
  }
  if (diffDays <= 30) {
    return {
      text: `剩${diffDays}天`,
      class:
        "inline-flex items-center rounded-full px-2 py-[2px] text-xs font-medium ring-1 ring-inset bg-blue-50 text-blue-700 ring-blue-200 dark:bg-blue-500/10 dark:text-blue-300 dark:ring-blue-500/20",
    } as const;
  }
  return null;
};

const formatDateTime = (v?: string | null) => {
  if (!v) return "-";
  const d = dayjs(v);
  return d.isValid() ? d.format("YYYY-MM-DD HH:mm") : String(v);
};

const formatAdminName = (id?: any) => {
  const key = String(id ?? "");
  if (!key || key === "0") return "-";
  return adminMap.value[key] || key;
};

const query = reactive({
  page: 1,
  tenantId: "0",
  key: "",
  enabled: undefined as number | undefined,
});

const columns = [
  { title: "计划编号", dataIndex: "planNo", key: "planNo", minWidth: 180 },
  { title: "计划名称", dataIndex: "name", key: "name", minWidth: 220 },
  { title: "周期", dataIndex: "cycleType", key: "cycleType", width: 120 },
  {
    title: "周期值",
    dataIndex: "cycleValue",
    key: "cycleValue",
    width: 90,
    align: "center",
  },
  {
    title: "保养管理员",
    dataIndex: "managerId",
    key: "managerId",
    width: 140,
  },
  {
    title: "下次执行",
    dataIndex: "nextRunTime",
    key: "nextRunTime",
    width: 220,
  },
  {
    title: "启用",
    dataIndex: "isEnabled",
    key: "isEnabled",
    width: 80,
    align: "center",
  },
  { title: "创建时间", dataIndex: "createTime", key: "createTime", width: 170 },
  {
    title: "操作",
    dataIndex: "action",
    key: "action",
    width: 140,
    fixed: "right",
  },
];

const selectionChange = (params: any) => {
  selectedRows.value = params.selectedRows || [];
};

const handleSearch = () => {
  tableRef.value?.upData(query, 1);
};

const handleReset = () => {
  Object.assign(query, {
    page: 1,
    tenantId: "0",
    key: "",
    enabled: undefined,
  });
  handleSearch();
};

const openDialog = (row?: any) => {
  modifyRef.value?.openModal(row || {});
};

const openDetail = (row?: any) => {
  detailRef.value?.openModal(row || {});
};

onMounted(() => {
  ensureAdminMap();
});

const handleDelete = async (id: string) => {
  await deleteAmMaintenancePlan([id]);
  ElMessage.success("删除成功");
  handleSearch();
};

const handleBatchDelete = async () => {
  if (selectedRows.value.length === 0) return;
  await deleteAmMaintenancePlan(selectedRows.value.map((x: any) => x.id));
  ElMessage.success("删除成功");
  handleSearch();
};
</script>
