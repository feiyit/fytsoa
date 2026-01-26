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
        <el-form-item label="期间">
          <el-date-picker
            v-model="query.period"
            type="month"
            value-format="YYYY-MM"
            format="YYYY年MM月"
            placeholder="选择期间"
            clearable
            style="width: 140px"
          />
        </el-form-item>
        <el-form-item label="状态">
          <el-select
            v-model="query.runStatus"
            placeholder="全部"
            style="width: 140px"
            clearable
          >
            <el-option label="草稿" :value="0" />
            <el-option label="已确认" :value="1" />
          </el-select>
        </el-form-item>
        <el-form-item>
          <el-button type="primary" @click="handleSearch">查询</el-button>
          <el-button @click="handleReset">重置</el-button>
        </el-form-item>
      </el-form>
      <div>
        <el-button
          type="success"
          v-if="selectedRows.length === 1"
          @click="handleConfirmSelected"
        >
          <el-icon><CircleCheck /></el-icon>
          确认过账
        </el-button>
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
          新增计提
        </el-button>
      </div>
    </el-header>

    <el-main class="bg-card border-border mt-2 rounded-[.5vw]">
      <soaTable
        ref="tableRef"
        :columns="columns"
        :apiObj="fetchAmDepreciationRunPage"
        :params="query"
        row-key="id"
        row-serial-number
        @selection-change="selectionChange"
        @loaded="onLoaded"
      >
        <template #bodyCell="{ column, record }">
          <template v-if="column.key === 'period'">
            <el-link type="primary" :underline="false" @click="openDetail(record)">
              {{ record.period || "-" }}
            </el-link>
          </template>
          <template v-if="column.key === 'status'">
            <el-tag :type="record.status === 1 ? 'success' : 'info'">
              {{ record.status === 1 ? "已确认" : "草稿" }}
            </el-tag>
          </template>
          <template v-if="column.key === 'runUserId'">
            <span>{{ executorLabel(record) }}</span>
          </template>
          <template v-if="column.key === 'action'">
            <div class="flex items-center gap-1">
              <el-button link type="primary" @click="openDialog(record)"
                >编辑</el-button
              >
              <el-button
                link
                type="success"
                :disabled="record.status === 1"
                @click="handleConfirm(record.id)"
                >确认</el-button
              >
              <el-popconfirm
                title="确认删除该计提批次？"
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
import {
  fetchAmDepreciationRunPage,
  deleteAmDepreciationRun,
  confirmAmDepreciationRun,
} from "@/api/am/depreciationRun";
import { fetchAdminById } from "@/api/sys/admin";
const modify = defineAsyncComponent(() => import("./modify.vue"));
const detail = defineAsyncComponent(() => import("./detail.vue"));
const tableRef = ref<any>(null);
const modifyRef = ref<any>(null);
const detailRef = ref<any>(null);
const selectedRows = ref<any[]>([]);

const query = reactive({
  page: 1,
  tenantId: "0",
  key: "",
  period: "",
  runStatus: undefined as number | undefined,
});

const columns = [
  { title: "期间", dataIndex: "period", key: "period", width: 120 },
  {
    title: "状态",
    dataIndex: "status",
    key: "status",
    width: 100,
    align: "center",
  },
  { title: "执行人", dataIndex: "runUserId", key: "runUserId" },
  { title: "执行时间", dataIndex: "runTime", key: "runTime", width: 170 },
  {
    title: "本期合计",
    dataIndex: "totalAmount",
    key: "totalAmount",
    width: 120,
    align: "right",
  },
  {
    title: "操作",
    dataIndex: "action",
    key: "action",
    width: 160,
    fixed: "right",
  },
];

const adminCache = reactive<Record<string, any>>({});
const adminLoading = ref<Set<string>>(new Set());

const getAdminLabel = (u: any) =>
  u?.fullName ||
  u?.displayName ||
  u?.userName ||
  u?.loginAccount ||
  u?.mobile ||
  u?.id;

const executorLabel = (record: any) => {
  const id = record?.runUserId != null ? String(record.runUserId) : "";
  const u = record?.runUserObj || adminCache[id];
  const name = getAdminLabel(u);
  if (name && id && id !== "0") return `${name}`;
  if (name) return `${name}`;
  return id && id !== "0" ? `ID:${id}` : "-";
};

const ensureAdmin = async (id: string) => {
  if (!id || id === "0") return;
  if (adminCache[id]) return;
  if (adminLoading.value.has(id)) return;
  adminLoading.value.add(id);
  try {
    adminCache[id] = await fetchAdminById(id);
  } catch {
    // ignore
  } finally {
    adminLoading.value.delete(id);
  }
};

const onLoaded = async (payload: { items: any[] }) => {
  const items = payload?.items || [];
  const ids = Array.from(
    new Set(
      items
        .filter(
          (it) =>
            it?.runUserId && String(it.runUserId) !== "0" && !it?.runUserObj,
        )
        .map((it) => String(it.runUserId)),
    ),
  );
  await Promise.all(ids.map((id) => ensureAdmin(id)));
};

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
    period: "",
    runStatus: undefined,
  });
  handleSearch();
};

const openDialog = (row?: any) => {
  modifyRef.value?.openModal(row || {});
};

const openDetail = (row?: any) => {
  detailRef.value?.openModal(row || {});
};

const handleConfirm = async (id: string) => {
  await confirmAmDepreciationRun(id);
  ElMessage.success("已确认");
  handleSearch();
};

const handleConfirmSelected = async () => {
  if (selectedRows.value.length !== 1) return;
  await handleConfirm(selectedRows.value[0].id);
};

const handleDelete = async (id: string) => {
  await deleteAmDepreciationRun([id]);
  ElMessage.success("删除成功");
  handleSearch();
};

const handleBatchDelete = async () => {
  if (selectedRows.value.length === 0) return;
  await deleteAmDepreciationRun(selectedRows.value.map((x: any) => x.id));
  ElMessage.success("删除成功");
  handleSearch();
};
</script>
