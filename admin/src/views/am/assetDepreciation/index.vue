<template>
  <el-container class="h-full">
    <el-header
      class="bg-card border-border flex flex-wrap items-center justify-between gap-3 rounded-[.5vw]"
      style="height: auto; padding: 10px"
    >
      <el-form
        :inline="true"
        :model="query"
        class="mb-0 flex flex-wrap items-center gap-3"
      >
        <el-form-item label="资产">
          <el-input
            :model-value="queryAssetLabel"
            placeholder="请选择资产"
            readonly
            class="w-[320px]"
            @click="openAssetPicker('query')"
          >
            <template #append>
              <div class="flex items-center gap-1">
                <el-button
                  type="primary"
                  icon="Check"
                  @click.stop="openAssetPicker('query')"
                ></el-button>
                <el-button
                  v-if="pickedQueryAsset"
                  icon="CircleClose"
                  style="margin-left: 2px"
                  @click.stop="clearQueryAsset"
                ></el-button>
              </div>
            </template>
          </el-input>
        </el-form-item>
        <el-form-item label="方法">
          <el-select
            v-model="query.method"
            placeholder="全部"
            style="width: 160px"
            clearable
          >
            <el-option label="不折旧" :value="0" />
            <el-option label="直线法" :value="1" />
            <el-option label="双倍余额" :value="2" />
            <el-option label="年数总和" :value="3" />
          </el-select>
        </el-form-item>
        <el-form-item label="状态">
          <el-select
            v-model="query.depStatus"
            placeholder="全部"
            style="width: 160px"
            clearable
          >
            <el-option label="未启用" :value="0" />
            <el-option label="折旧中" :value="1" />
            <el-option label="已停用" :value="2" />
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
          新增折旧配置
        </el-button>
      </div>
    </el-header>

    <el-main class="bg-card border-border mt-2 rounded-[.5vw]">
      <soaTable
        ref="tableRef"
        :columns="columns"
        :apiObj="fetchAmAssetDepreciationPage"
        :params="query"
        row-key="id"
        row-serial-number
        @selection-change="selectionChange"
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
          <template v-if="column.key === 'method'">
            {{ methodText(record.method) }}
          </template>
          <template v-if="column.key === 'status'">
            <el-tag :type="record.status === 1 ? 'success' : 'info'">
              {{ statusText(record.status) }}
            </el-tag>
          </template>
          <template v-if="column.key === 'action'">
            <div class="flex items-center gap-1">
              <el-button link type="primary" @click="openDialog(record)"
                >编辑</el-button
              >
              <el-popconfirm
                title="确认删除该配置？"
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
    <SoaAssetPicker ref="assetPickerRef" @select="onAssetPicked" />
  </el-container>
</template>

<script setup lang="ts">
import {
  fetchAmAssetDepreciationPage,
  deleteAmAssetDepreciation,
} from "@/api/am/assetDepreciation";
import { fetchAmAssetById } from "@/api/am/asset";
import SoaAssetPicker from "@/component/soaAssetPicker/index.vue";
const modify = defineAsyncComponent(() => import("./modify.vue"));
const assetDetailDrawer = defineAsyncComponent(
  () => import("../asset/detailDrawer.vue"),
);
const tableRef = ref<any>(null);
const modifyRef = ref<any>(null);
const assetDetailRef = ref<any>(null);
const selectedRows = ref<any[]>([]);

const query = reactive({
  page: 1,
  tenantId: "0",
  key: "",
  assetId: undefined as string | undefined,
  method: undefined as number | undefined,
  depStatus: undefined as number | undefined,
});

const columns = [
  { title: "资产", dataIndex: "assetId", key: "assetId", minWidth: 220 },
  { title: "折旧方法", dataIndex: "method", key: "method", width: 140 },
  {
    title: "折旧期(月)",
    dataIndex: "lifeMonths",
    key: "lifeMonths",
    width: 110,
    align: "center",
  },
  {
    title: "残值率(%)",
    dataIndex: "salvageRate",
    key: "salvageRate",
    width: 110,
    align: "right",
  },
  { title: "起算日期", dataIndex: "startDate", key: "startDate", width: 170 },
  {
    title: "累计折旧",
    dataIndex: "accumAmount",
    key: "accumAmount",
    width: 120,
    align: "right",
  },
  {
    title: "状态",
    dataIndex: "status",
    key: "status",
    width: 90,
    align: "center",
  },
  {
    title: "操作",
    dataIndex: "action",
    key: "action",
    width: 140,
    fixed: "right",
  },
];

const methodText = (v: number) => {
  switch (v) {
    case 0:
      return "不折旧";
    case 1:
      return "直线法";
    case 2:
      return "双倍余额";
    case 3:
      return "年数总和";
    default:
      return "未知";
  }
};

const statusText = (v: number) => {
  switch (v) {
    case 0:
      return "未启用";
    case 1:
      return "折旧中";
    case 2:
      return "已停用";
    default:
      return "未知";
  }
};

const selectionChange = (params: any) => {
  selectedRows.value = params.selectedRows || [];
};

const assetPickerRef = ref<any>(null);
const pickedQueryAsset = ref<any | null>(null);

// 列表通常不返回资产对象；按需补拉用于展示
const assetCache = reactive<Record<string, any>>({});
const assetLoading = ref<Set<string>>(new Set());

const queryAssetLabel = computed(() => {
  const a = pickedQueryAsset.value;
  if (a?.assetNo || a?.name) {
    const left = a.assetNo ? String(a.assetNo) : "";
    const right = a.name ? String(a.name) : "";
    return left && right ? `${left} / ${right}` : left || right;
  }
  return query.assetId && String(query.assetId) !== "0"
    ? `ID:${query.assetId}`
    : "";
});

const openAssetPicker = (target: "query") => {
  if (target !== "query") return;
  assetPickerRef.value?.openModal({
    multiple: false,
    title: "选择资产",
    picked: pickedQueryAsset.value || null,
  });
};

const onAssetPicked = (payload: { rows: any[]; multiple: boolean }) => {
  const a = payload?.rows?.[0] || null;
  pickedQueryAsset.value = a;
  query.assetId = a?.id ? String(a.id) : undefined;
};

const clearQueryAsset = () => {
  pickedQueryAsset.value = null;
  query.assetId = undefined;
};

const assetLabel = (record: any) => {
  const assetId = record?.assetId ? String(record.assetId) : "";
  const a = assetCache[assetId];
  const no = a?.assetNo;
  const name = a?.name;
  if (no && name) return `${no} / ${name}`;
  return no || name || (assetId ? `ID:${assetId}` : "-");
};

const openAssetDetail = (record: any) => {
  const id = record?.assetId ? String(record.assetId) : "";
  if (!id || id === "0") return;
  assetDetailRef.value?.openModal?.({
    id,
    assetId: id,
    ...(assetCache[id] || {}),
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
        .filter((it) => it?.assetId && String(it.assetId) !== "0")
        .map((it) => String(it.assetId)),
    ),
  );
  await Promise.all(ids.map((id) => ensureAsset(id)));
};

const handleSearch = () => {
  tableRef.value?.upData(query, 1);
};

const handleReset = () => {
  Object.assign(query, {
    page: 1,
    tenantId: "0",
    key: "",
    assetId: undefined,
    method: undefined,
    depStatus: undefined,
  });
  clearQueryAsset();
  handleSearch();
};

const openDialog = (row?: any) => {
  modifyRef.value?.openModal(row || {});
};

const handleDelete = async (id: string) => {
  await deleteAmAssetDepreciation([id]);
  ElMessage.success("删除成功");
  handleSearch();
};

const handleBatchDelete = async () => {
  if (selectedRows.value.length === 0) return;
  await deleteAmAssetDepreciation(selectedRows.value.map((x: any) => x.id));
  ElMessage.success("删除成功");
  handleSearch();
};
</script>
