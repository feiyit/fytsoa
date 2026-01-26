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
        <el-form-item label="资产">
          <el-input
            :model-value="assetSearchDisplay"
            placeholder="请选择资产"
            readonly
            style="width: 260px"
            @click="openAssetPicker"
          >
            <template #append>
              <div class="flex items-center gap-1">
                <el-button type="primary" @click.stop="openAssetPicker">
                  选择
                </el-button>
                <el-button v-if="pickedAsset" @click.stop="clearPickedAsset">
                  清空
                </el-button>
              </div>
            </template>
          </el-input>
        </el-form-item>
        <el-form-item label="业务类型">
          <el-select
            v-model="query.bizType"
            placeholder="全部"
            clearable
            filterable
            allow-create
            default-first-option
            style="width: 220px"
          >
            <el-option
              v-for="it in bizTypeOptions"
              :key="it.value"
              :label="it.label"
              :value="it.value"
            />
          </el-select>
        </el-form-item>
        <el-form-item label="操作">
          <el-select
            v-model="query.operation"
            placeholder="全部"
            clearable
            filterable
            allow-create
            default-first-option
            style="width: 200px"
          >
            <el-option
              v-for="it in operationOptions"
              :key="it.value"
              :label="it.label"
              :value="it.value"
            />
          </el-select>
        </el-form-item>
        <el-form-item label="关键词">
          <el-input
            v-model="query.key"
            placeholder="BizType/Operation"
            clearable
          />
        </el-form-item>
        <el-form-item>
          <el-button type="primary" @click="handleSearch">查询</el-button>
          <el-button @click="handleReset">重置</el-button>
          <el-button :loading="exporting" @click="handleExport"
            >导出Excel</el-button
          >
        </el-form-item>
      </el-form>
    </el-header>

    <el-main class="bg-card border-border mt-2 rounded-[.5vw]">
      <soaTable
        ref="tableRef"
        :columns="columns"
        :apiObj="fetchAmAssetHistoryPage"
        :params="query"
        row-key="id"
        row-serial-number
        @loaded="onLoaded"
      >
        <template #bodyCell="{ column, record }">
          <template v-if="column.key === 'assetId'">
            <el-link
              v-if="record.assetId && String(record.assetId) !== '0'"
              type="primary"
              :underline="false"
              @click="openAssetDetail(record)"
            >
              {{ assetLabel(record) }}
            </el-link>
            <span v-else>-</span>
          </template>
          <template v-if="column.key === 'action'">
            <el-button link type="primary" @click="showJson(record)"
              >查看</el-button
            >
          </template>
        </template>
      </soaTable>
    </el-main>

    <Detail ref="detailRef" />
    <SoaAssetPicker ref="assetPickerRef" @select="onAssetPicked" />
    <assetDetailDrawer ref="assetDetailRef" />
  </el-container>
</template>

<script setup lang="ts">
import dayjs from "dayjs";
import {
  fetchAmAssetHistoryPage,
  exportAmAssetHistory,
} from "@/api/am/assetHistory";
import { fetchAmAssetById } from "@/api/am/asset";
import SoaAssetPicker from "@/component/soaAssetPicker/index.vue";
import { downloadBlob } from "@/utils/download";
const Detail = defineAsyncComponent(() => import("./detail.vue"));
const assetDetailDrawer = defineAsyncComponent(
  () => import("../asset/detailDrawer.vue"),
);
const tableRef = ref<any>(null);
const detailRef = ref<any>(null);
const assetPickerRef = ref<any>(null);
const assetDetailRef = ref<any>(null);
const exporting = ref(false);

const pickedAsset = ref<any | null>(null);

// 页面列表通常只返回 assetId；若未返回 assetObj，则按需补齐，避免列表只显示 id。
const assetCache = reactive<Record<string, any>>({});
const assetLoading = ref<Set<string>>(new Set());

// 业务类型/操作：下拉提供常用值，同时允许手动输入扩展值
const bizTypeOptions = [
  { value: "ASSET", label: "资产（ASSET）" },
  { value: "INBOUND", label: "入库（INBOUND）" },
  { value: "OUTBOUND", label: "出库/领用（OUTBOUND）" },
  { value: "RETURN", label: "归还（RETURN）" },
  { value: "TRANSFER", label: "调拨（TRANSFER）" },
  { value: "CHANGE", label: "变更（CHANGE）" },
  { value: "DISPOSE", label: "处置（DISPOSE）" },
  { value: "INV_ADJUST", label: "库存调整（INV_ADJUST）" },
  { value: "INVENTORY", label: "盘点（INVENTORY）" },
  { value: "MAINTENANCE", label: "维修/保养（MAINTENANCE）" },
];

const operationOptions = [
  { value: "CREATE", label: "新增（CREATE）" },
  { value: "UPDATE", label: "修改（UPDATE）" },
  { value: "DELETE", label: "删除（DELETE）" },
  { value: "SCAN", label: "扫码/录入（SCAN）" },
  { value: "STATUS", label: "状态变更（STATUS）" },
  { value: "LOCATION", label: "地点变更（LOCATION）" },
  { value: "OWNER", label: "责任人/使用人变更（OWNER）" },
];

const query = reactive({
  page: 1,
  tenantId: "0",
  key: "",
  assetId: 0 as number,
  bizType: "",
  operation: "",
});

const columns = [
  { title: "资产", dataIndex: "assetId", key: "assetId", minWidth: 240 },
  { title: "业务类型", dataIndex: "bizType", key: "bizType", width: 140 },
  { title: "业务Id", dataIndex: "bizId", key: "bizId", width: 120 },
  { title: "操作", dataIndex: "operation", key: "operation", width: 140 },
  { title: "操作人Id", dataIndex: "operatorId", key: "operatorId", width: 120 },
  {
    title: "操作时间",
    dataIndex: "operateTime",
    key: "operateTime",
    width: 170,
  },
  {
    title: "操作",
    dataIndex: "action",
    key: "action",
    width: 90,
    fixed: "right",
  },
];

const assetSearchDisplay = computed(() => {
  const a = pickedAsset.value;
  if (a?.assetNo || a?.name) {
    const left = a.assetNo ? String(a.assetNo) : "";
    const right = a.name ? String(a.name) : "";
    return left && right ? `${left} / ${right}` : left || right;
  }
  return "";
});

const openAssetPicker = () => {
  assetPickerRef.value?.openModal({
    multiple: false,
    title: "选择资产",
    picked: pickedAsset.value || null,
  });
};

const onAssetPicked = (payload: { rows: any[]; multiple: boolean }) => {
  pickedAsset.value = payload?.rows?.[0] || null;
  query.assetId = pickedAsset.value?.id ? Number(pickedAsset.value.id) : 0;
};

const clearPickedAsset = () => {
  pickedAsset.value = null;
  query.assetId = 0;
};

const handleSearch = () => {
  tableRef.value?.upData(query, 1);
};

const handleReset = () => {
  Object.assign(query, {
    page: 1,
    tenantId: "0",
    key: "",
    assetId: 0,
    bizType: "",
    operation: "",
  });
  pickedAsset.value = null;
  handleSearch();
};

const showJson = (row: any) => {
  detailRef.value?.openModal(row || {});
};

const handleExport = async () => {
  if (exporting.value) return;
  exporting.value = true;
  try {
    const blob: any = await exportAmAssetHistory({
      ...query,
      page: 1,
      limit: 10000,
    });
    const fileName = `资产留痕_${dayjs().format("YYYYMMDDHHmmss")}.xls`;
    downloadBlob(blob as Blob, fileName);
  } finally {
    exporting.value = false;
  }
};

const assetLabel = (record: any) => {
  const assetId = record?.assetId ? String(record.assetId) : "";
  const a = record?.assetObj || assetCache[assetId];
  const no = a?.assetNo || record?.assetNo;
  const name = a?.name || record?.assetName;
  if (no && name) return `${no} / ${name}`;
  return no || name || (assetId ? `ID:${assetId}` : "-");
};

const openAssetDetail = (record: any) => {
  const id = record?.assetId ? String(record.assetId) : "";
  if (!id || id === "0") return;
  assetDetailRef.value?.openModal?.({
    id,
    assetId: id,
    assetNo: record?.assetObj?.assetNo || record?.assetNo,
    name: record?.assetObj?.name || record?.assetName,
  });
};

const ensureAsset = async (id: string) => {
  if (!id || id === "0") return;
  if (assetCache[id]) return;
  if (assetLoading.value.has(id)) return;
  assetLoading.value.add(id);
  try {
    assetCache[id] = await fetchAmAssetById(id);
  } catch {
    // ignore
  } finally {
    assetLoading.value.delete(id);
  }
};

const onLoaded = async (payload: { items: any[] }) => {
  const items = payload?.items || [];
  const ids = Array.from(
    new Set(
      items
        .filter(
          (it) => it?.assetId && String(it.assetId) !== "0" && !it?.assetObj,
        )
        .map((it) => String(it.assetId)),
    ),
  );
  await Promise.all(ids.map((id) => ensureAsset(id)));
};
</script>
