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
        <el-form-item label="状态">
          <el-select
            v-model="query.planStatus"
            placeholder="全部"
            style="width: 140px"
            clearable
          >
            <el-option label="草稿" :value="0" />
            <el-option label="进行中" :value="1" />
            <el-option label="已完成" :value="2" />
            <el-option label="已取消" :value="3" />
          </el-select>
        </el-form-item>
        <el-form-item>
          <el-button type="primary" @click="handleSearch">查询</el-button>
          <el-button @click="handleReset">重置</el-button>
        </el-form-item>
      </el-form>
      <div>
        <el-popconfirm title="确认执行批量删除操作？" @confirm="handleBatchDelete">
          <template #reference>
            <el-button type="danger" v-if="selectedRows.length > 0">
              <el-icon><Delete /></el-icon>
              批量删除
            </el-button>
          </template>
        </el-popconfirm>
        <el-button type="primary" @click="openDialog()">
          <el-icon><Plus /></el-icon>
          新增盘点
        </el-button>
      </div>
    </el-header>

    <el-main class="bg-card border-border mt-2 rounded-[.5vw]">
      <soaTable
        ref="tableRef"
        :columns="columns"
        :apiObj="fetchAmInventoryPlanPage"
        row-key="id"
        row-serial-number
        @selection-change="selectionChange"
      >
        <template #bodyCell="{ column, record }">
          <template v-if="column.key === 'planNo'">
            <el-link :underline="false" type="primary" @click="openDetail(record)">
              {{ record.planNo }}
            </el-link>
          </template>
          <template v-if="column.key === 'status'">
            <el-tag :type="record.status === 2 ? 'success' : record.status === 3 ? 'danger' : 'info'">
              {{ statusText(record.status) }}
            </el-tag>
          </template>
          <template v-if="column.key === 'action'">
            <div class="flex items-center gap-1">
              <el-button
                link
                type="primary"
                :disabled="!canEdit(record)"
                @click="openDialog(record)"
              >
                编辑
              </el-button>
              <el-popconfirm
                title="确认删除该盘点？"
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
import { fetchAmInventoryPlanPage, deleteAmInventoryPlan } from "@/api/am/inventoryPlan";
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
  planStatus: undefined as number | undefined,
});

const columns = [
  { title: "盘点编号", dataIndex: "planNo", key: "planNo", minWidth: 180 },
  { title: "盘点名称", dataIndex: "name", key: "name", minWidth: 220 },
  { title: "状态", dataIndex: "status", key: "status", width: 110, align: "center" },
  { title: "开始时间", dataIndex: "startTime", key: "startTime", width: 170 },
  { title: "结束时间", dataIndex: "endTime", key: "endTime", width: 170 },
  { title: "创建时间", dataIndex: "createTime", key: "createTime", width: 170 },
  { title: "操作", dataIndex: "action", key: "action", width: 140, fixed: "right" },
];

const selectionChange = (params: any) => {
  selectedRows.value = params.selectedRows || [];
};

const statusText = (v: number) => {
  switch (v) {
    case 0:
      return "草稿";
    case 1:
      return "进行中";
    case 2:
      return "已完成";
    case 3:
      return "已取消";
    default:
      return "未知";
  }
};

const handleSearch = () => {
  tableRef.value?.upData(query, 1);
};

const handleReset = () => {
  Object.assign(query, {
    page: 1,
    tenantId: "0",
    key: "",
    planStatus: undefined,
  });
  handleSearch();
};

const openDialog = (row?: any) => {
  if (!canEdit(row)) {
    ElMessage.warning("该盘点已完成/已取消，不能编辑。");
    return;
  }
  modifyRef.value?.openModal(row || {});
};

const openDetail = (row?: any) => {
  detailRef.value?.openModal(row || {});
};

const canEdit = (row?: any) => {
  const s = Number(row?.status ?? 0);
  // 2=已完成,3=已取消
  return s !== 2 && s !== 3;
};

const handleDelete = async (id: string) => {
  await deleteAmInventoryPlan([id]);
  ElMessage.success("删除成功");
  handleSearch();
};

const handleBatchDelete = async () => {
  if (selectedRows.value.length === 0) return;
  await deleteAmInventoryPlan(selectedRows.value.map((x: any) => x.id));
  ElMessage.success("删除成功");
  handleSearch();
};
</script>
