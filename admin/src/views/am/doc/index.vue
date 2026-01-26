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
          <el-input v-model="query.key" placeholder="单据号/类型" clearable />
        </el-form-item>
        <el-form-item label="单据类型">
          <el-select
            v-model="query.docType"
            placeholder="全部"
            style="width: 160px"
            clearable
          >
            <el-option label="入库" value="INBOUND" />
            <el-option label="出库" value="OUTBOUND" />
            <el-option label="归还" value="RETURN" />
            <el-option label="调拨" value="TRANSFER" />
            <el-option label="变更" value="CHANGE" />
            <el-option label="处置" value="DISPOSE" />
            <el-option label="盘盈盘亏" value="INV_ADJUST" />
          </el-select>
        </el-form-item>
        <el-form-item label="状态">
          <el-select
            v-model="query.docStatus"
            placeholder="全部"
            style="width: 140px"
            clearable
          >
            <el-option label="草稿" :value="0" />
            <el-option label="待审批" :value="1" />
            <el-option label="已通过" :value="2" />
            <el-option label="已驳回" :value="3" />
            <el-option label="执行中" :value="4" />
            <el-option label="已完成" :value="5" />
            <el-option label="已取消" :value="6" />
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
        <el-button :loading="exporting" @click="handleExport"
          >导出Excel</el-button
        >
        <el-button type="primary" @click="openDialog()">
          <el-icon><Plus /></el-icon>
          新增单据
        </el-button>
      </div>
    </el-header>

    <el-main class="bg-card border-border mt-2 rounded-[.5vw]">
      <soaTable
        ref="tableRef"
        :columns="columns"
        :apiObj="fetchAmDocPage"
        row-key="id"
        row-serial-number
        @selection-change="selectionChange"
      >
        <template #bodyCell="{ column, record }">
          <template v-if="column.key === 'docNo'">
            <el-link
              type="primary"
              :underline="false"
              @click="openDetail(record)"
            >
              {{ record.docNo }}
            </el-link>
          </template>
          <template v-if="column.key === 'status'">
            <el-tag
              :type="
                record.status === 0
                  ? 'info'
                  : record.status === 2
                    ? 'success'
                    : 'warning'
              "
            >
              {{ statusText(record.status) }}
            </el-tag>
          </template>
          <template v-if="column.key === 'action'">
            <div class="flex items-center gap-1">
              <el-button link type="primary" @click="openDialog(record)"
                >编辑</el-button
              >
              <el-popconfirm
                title="确认删除该单据？"
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
    <Detail ref="detailRef" />
  </el-container>
</template>

<script setup lang="ts">
import dayjs from "dayjs";
import { fetchAmDocPage, deleteAmDoc, exportAmDoc } from "@/api/am/doc";
import { downloadBlob } from "@/utils/download";
const modify = defineAsyncComponent(() => import("./modify.vue"));
const Detail = defineAsyncComponent(() => import("./detail.vue"));
const tableRef = ref<any>(null);
const modifyRef = ref<any>(null);
const detailRef = ref<any>(null);
const selectedRows = ref<any[]>([]);
const exporting = ref(false);

const query = reactive({
  page: 1,
  tenantId: "0",
  key: "",
  docType: "",
  docStatus: undefined as number | undefined,
  includeDeleted: false,
});

const columns = [
  { title: "单据号", dataIndex: "docNo", key: "docNo", minWidth: 180 },
  { title: "类型", dataIndex: "docType", key: "docType", width: 140 },
  { title: "子类型", dataIndex: "subType", key: "subType", width: 140 },
  {
    title: "状态",
    dataIndex: "status",
    key: "status",
    width: 110,
    align: "center",
  },
  {
    title: "金额",
    dataIndex: "totalAmount",
    key: "totalAmount",
    width: 120,
    align: "right",
  },
  { title: "业务时间", dataIndex: "bizTime", key: "bizTime", width: 170 },
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

const statusText = (v: number) => {
  switch (v) {
    case 0:
      return "草稿";
    case 1:
      return "待审批";
    case 2:
      return "已通过";
    case 3:
      return "已驳回";
    case 4:
      return "执行中";
    case 5:
      return "已完成";
    case 6:
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
    docType: "",
    docStatus: undefined,
    includeDeleted: false,
  });
  handleSearch();
};

const openDialog = (row?: any) => {
  modifyRef.value?.openModal(row || {});
};

const openDetail = (row: any) => {
  detailRef.value?.openModal(row);
};

const handleDelete = async (id: string) => {
  await deleteAmDoc([id]);
  ElMessage.success("删除成功");
  handleSearch();
};

const handleBatchDelete = async () => {
  if (selectedRows.value.length === 0) return;
  await deleteAmDoc(selectedRows.value.map((x: any) => x.id));
  ElMessage.success("删除成功");
  handleSearch();
};

const handleExport = async () => {
  if (exporting.value) return;
  exporting.value = true;
  try {
    const blob: any = await exportAmDoc({ ...query, page: 1, limit: 10000 });
    const fileName = `单据管理_${dayjs().format("YYYYMMDDHHmmss")}.xls`;
    downloadBlob(blob as Blob, fileName);
  } finally {
    exporting.value = false;
  }
};
</script>
