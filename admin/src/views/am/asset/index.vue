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
            placeholder="编号/名称/标签"
            clearable
          />
        </el-form-item>
        <el-form-item label="分类">
          <el-select
            v-model="query.categoryId"
            placeholder="全部"
            style="width: 200px"
            clearable
            filterable
          >
            <el-option
              v-for="it in categoryOptions"
              :key="it.value"
              :label="it.label"
              :value="it.value"
            />
          </el-select>
        </el-form-item>
        <el-form-item label="状态">
          <el-select
            v-model="query.assetStatus"
            placeholder="全部"
            style="width: 140px"
            clearable
          >
            <el-option label="在库" :value="1" />
            <el-option label="在用" :value="2" />
            <el-option label="借出" :value="3" />
            <el-option label="维修中" :value="4" />
            <el-option label="闲置" :value="5" />
            <el-option label="在途" :value="6" />
            <el-option label="处置中" :value="7" />
            <el-option label="已处置" :value="8" />
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
          新增资产
        </el-button>
      </div>
    </el-header>
    <el-main class="bg-card border-border mt-2 rounded-[.5vw]">
      <soaTable
        ref="tableRef"
        :columns="columns"
        :apiObj="fetchAmAssetPage"
        :params="query"
        row-key="id"
        row-serial-number
        @selection-change="selectionChange"
      >
        <template #bodyCell="{ column, record }">
          <template v-if="column.key === 'originalValue'">
            {{ formatMoney(record.originalValue) }}
          </template>
          <template v-if="column.key === 'netBookValue'">
            {{ formatMoney(record.netBookValue) }}
          </template>
          <template v-if="column.key === 'categoryId'">
            {{ record.categoryObj?.name || record.categoryId || "-" }}
          </template>
          <template v-if="column.key === 'status'">
            <el-tag :type="statusTagType(record.status)">
              {{ statusText(record.status) }}
            </el-tag>
          </template>
          <template v-if="column.key === 'action'">
            <div class="flex items-center gap-1">
              <el-button link type="primary" @click="openDialog(record)"
                >编辑</el-button
              >
              <el-popconfirm
                title="确认删除该资产？"
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
  </el-container>
</template>

<script setup lang="ts">
import dayjs from "dayjs";
import { fetchAmAssetPage, deleteAmAsset, exportAmAsset } from "@/api/am/asset";
import { fetchSysCodeList } from "@/api/sys/code";
import { downloadBlob } from "@/utils/download";

const formatMoney = (v: any) =>
  (Number(v) || 0).toLocaleString(undefined, {
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  });

const modify = defineAsyncComponent(() => import("./modify.vue"));
const tableRef = ref<any>(null);
const modifyRef = ref<any>(null);
const selectedRows = ref<any[]>([]);
const exporting = ref(false);

const AM_ASSET_CATEGORY_TYPE_ID = "2013975685776936960";
const categoryOptions = ref<{ label: string; value: string }[]>([]);

const query = reactive({
  page: 1,
  tenantId: "0",
  key: "",
  categoryId: undefined as string | undefined,
  assetStatus: undefined as number | undefined,
});

onMounted(async () => {
  const codes = await fetchSysCodeList({
    id: AM_ASSET_CATEGORY_TYPE_ID,
    status: "1",
  });
  categoryOptions.value = (codes || []).map((m: any) => ({
    label: m.name,
    value: String(m.id),
  }));
});

const selectionChange = (params: any) => {
  selectedRows.value = params.selectedRows || [];
};

const columns = [
  { title: "资产编号", dataIndex: "assetNo", key: "assetNo", minWidth: 160 },
  { title: "资产名称", dataIndex: "name", key: "name", minWidth: 180 },
  { title: "分类", dataIndex: "categoryId", key: "categoryId", width: 140 },
  { title: "品牌", dataIndex: "brand", key: "brand", width: 120 },
  { title: "型号", dataIndex: "model", key: "model", width: 120 },
  { title: "标签码", dataIndex: "tagCode", key: "tagCode", width: 140 },
  {
    title: "原值",
    dataIndex: "originalValue",
    key: "originalValue",
    width: 130,
    align: "right",
  },
  {
    title: "净值",
    dataIndex: "netBookValue",
    key: "netBookValue",
    width: 130,
    align: "right",
  },
  {
    title: "状态",
    dataIndex: "status",
    key: "status",
    width: 100,
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

const statusText = (v: number) => {
  switch (v) {
    case 1:
      return "在库";
    case 2:
      return "在用";
    case 3:
      return "借出";
    case 4:
      return "维修中";
    case 5:
      return "闲置";
    case 6:
      return "在途";
    case 7:
      return "处置中";
    case 8:
      return "已处置";
    default:
      return "未知";
  }
};

const statusTagType = (v: number) => {
  switch (v) {
    case 1:
      return "info";
    case 2:
      return "success";
    case 3:
      return "warning";
    case 4:
      return "danger";
    case 5:
      return "info";
    case 6:
      return "info";
    case 7:
      return "warning";
    case 8:
      return "danger";
    default:
      return "info";
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
    categoryId: undefined,
    assetStatus: undefined,
  });
  handleSearch();
};

const openDialog = (row?: any) => {
  modifyRef.value?.openModal(row || {});
};

const handleDelete = async (id: string) => {
  await deleteAmAsset([id]);
  ElMessage.success("删除成功");
  handleSearch();
};

const handleBatchDelete = async () => {
  if (selectedRows.value.length === 0) return;
  const ids = selectedRows.value.map((x: any) => x.id);
  await deleteAmAsset(ids);
  ElMessage.success("删除成功");
  handleSearch();
};

const handleExport = async () => {
  if (exporting.value) return;
  exporting.value = true;
  try {
    const blob: any = await exportAmAsset({ ...query, page: 1, limit: 10000 });
    const fileName = `资产台账_${dayjs().format("YYYYMMDDHHmmss")}.xls`;
    downloadBlob(blob as Blob, fileName);
  } finally {
    exporting.value = false;
  }
};
</script>
