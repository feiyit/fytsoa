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
          <el-input v-model="query.key" placeholder="工单号/标题" clearable />
        </el-form-item>
        <el-form-item label="类型">
          <el-select
            v-model="query.orderType"
            placeholder="全部"
            style="width: 120px"
            clearable
          >
            <el-option label="报修" :value="1" />
            <el-option label="保养" :value="2" />
          </el-select>
        </el-form-item>
        <el-form-item label="状态">
          <el-select
            v-model="query.orderStatus"
            placeholder="全部"
            style="width: 140px"
            clearable
          >
            <el-option label="草稿" :value="0" />
            <el-option label="待受理" :value="1" />
            <el-option label="已指派" :value="2" />
            <el-option label="处理中" :value="3" />
            <el-option label="已完成" :value="4" />
            <el-option label="已关闭" :value="5" />
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
        <el-button type="primary" @click="openDialog()">
          <el-icon><Plus /></el-icon>
          新增工单
        </el-button>
      </div>
    </el-header>

    <el-main class="bg-card border-border mt-2 rounded-[.5vw]">
      <soaTable
        ref="tableRef"
        :columns="columns"
        :apiObj="fetchAmMaintenanceOrderPage"
        :params="query"
        row-key="id"
        row-serial-number
        @selection-change="selectionChange"
        @loaded="onLoaded"
      >
        <template #bodyCell="{ column, record }">
          <template v-if="column.key === 'type'">
            <el-tag :type="record.type === 1 ? 'warning' : 'info'">
              {{ record.type === 1 ? "报修" : "保养" }}
            </el-tag>
          </template>
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
          <template v-if="column.key === 'cost'">
            <span>
              {{
                record.cost == null || record.cost === ""
                  ? "-"
                  : Number(record.cost || 0).toFixed(2)
              }}
            </span>
          </template>
          <template v-if="column.key === 'status'">
            <el-tag
              :type="
                record.status === 4
                  ? 'success'
                  : record.status === 6
                    ? 'danger'
                    : 'info'
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
                title="确认删除该工单？"
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
    <assetDetailDrawer ref="assetDetailRef" />
  </el-container>
</template>

<script setup lang="ts">
import {
  fetchAmMaintenanceOrderPage,
  deleteAmMaintenanceOrder,
} from "@/api/am/maintenanceOrder";
import { fetchAmAssetById } from "@/api/am/asset";
const modify = defineAsyncComponent(() => import("./modify.vue"));
const assetDetailDrawer = defineAsyncComponent(
  () => import("../asset/detailDrawer.vue"),
);
const tableRef = ref<any>(null);
const modifyRef = ref<any>(null);
const assetDetailRef = ref<any>(null);
const selectedRows = ref<any[]>([]);

// 页面列表通常返回 assetObj；若后端未返回则按需补齐，避免列表只显示 assetId。
const assetCache = reactive<Record<string, any>>({});
const assetLoading = ref<Set<string>>(new Set());

const query = reactive({
  page: 1,
  tenantId: "0",
  key: "",
  orderType: undefined as number | undefined,
  orderStatus: undefined as number | undefined,
});

const columns = [
  { title: "工单号", dataIndex: "orderNo", key: "orderNo", minWidth: 180 },
  { title: "类型", dataIndex: "type", key: "type", width: 90, align: "center" },
  { title: "标题", dataIndex: "title", key: "title", minWidth: 220 },
  { title: "资产", dataIndex: "assetId", key: "assetId", minWidth: 240 },
  {
    title: "优先级",
    dataIndex: "priority",
    key: "priority",
    width: 90,
    align: "center",
  },
  {
    title: "状态",
    dataIndex: "status",
    key: "status",
    width: 110,
    align: "center",
  },
  { title: "报修时间", dataIndex: "reportTime", key: "reportTime", width: 170 },
  { title: "费用", dataIndex: "cost", key: "cost", width: 110, align: "right" },
  {
    title: "操作",
    dataIndex: "action",
    key: "action",
    width: 110,
    fixed: "right",
  },
];

const statusText = (v: number) => {
  switch (v) {
    case 0:
      return "草稿";
    case 1:
      return "待受理";
    case 2:
      return "已指派";
    case 3:
      return "处理中";
    case 4:
      return "已完成";
    case 5:
      return "已关闭";
    case 6:
      return "已取消";
    default:
      return "未知";
  }
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
    orderType: undefined,
    orderStatus: undefined,
  });
  handleSearch();
};

const openDialog = (row?: any) => {
  modifyRef.value?.openModal(row || {});
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
  // 并发补齐（列表每页通常不大）
  await Promise.all(ids.map((id) => ensureAsset(id)));
};

const handleDelete = async (id: string) => {
  await deleteAmMaintenanceOrder([id]);
  ElMessage.success("删除成功");
  handleSearch();
};

const handleBatchDelete = async () => {
  if (selectedRows.value.length === 0) return;
  await deleteAmMaintenanceOrder(selectedRows.value.map((x: any) => x.id));
  ElMessage.success("删除成功");
  handleSearch();
};
</script>
