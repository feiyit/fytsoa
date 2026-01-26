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
        <el-form-item label="分类">
          <el-select
            v-model="query.categoryId"
            placeholder="全部"
            clearable
            filterable
            style="width: 220px"
          >
            <el-option
              v-for="it in categoryOptions"
              :key="it.value"
              :label="it.label"
              :value="it.value"
            />
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
          新增配置
        </el-button>
      </div>
    </el-header>

    <el-main class="bg-card border-border mt-2 rounded-[.5vw]">
      <soaTable
        ref="tableRef"
        :columns="columns"
        :apiObj="fetchAmAssetCategoryProfilePage"
        row-key="id"
        row-serial-number
        @selection-change="selectionChange"
      >
        <template #bodyCell="{ column, record }">
          <template v-if="column.key === 'depMethod'">
            {{ methodText(record.depMethod) }}
          </template>
          <template v-if="column.key === 'categoryId'">
            {{ record.categoryObj?.name }}
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
  </el-container>
</template>

<script setup lang="ts">
import {
  fetchAmAssetCategoryProfilePage,
  deleteAmAssetCategoryProfile,
} from "@/api/am/assetCategoryProfile";
import { fetchSysCodeList } from "@/api/sys/code";
const modify = defineAsyncComponent(() => import("./modify.vue"));
const tableRef = ref<any>(null);
const modifyRef = ref<any>(null);
const selectedRows = ref<any[]>([]);

const categoryOptions = ref<{ label: string; value: string }[]>([]);
const AM_ASSET_CATEGORY_TYPE_ID = "2013975685776936960";

const query = reactive({
  page: 1,
  tenantId: "0",
  key: "",
  categoryId: undefined as string | undefined,
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

const columns = [
  { title: "分类", dataIndex: "categoryId", key: "categoryId", width: 120 },
  { title: "折旧方法", dataIndex: "depMethod", key: "depMethod" },
  {
    title: "折旧期(月)",
    dataIndex: "depLifeMonths",
    key: "depLifeMonths",
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
  {
    title: "操作",
    dataIndex: "action",
    key: "action",
    width: 110,
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
    categoryId: undefined,
  });
  handleSearch();
};

const openDialog = (row?: any) => {
  modifyRef.value?.openModal(row || {});
};

const handleDelete = async (id: string) => {
  await deleteAmAssetCategoryProfile([id]);
  ElMessage.success("删除成功");
  handleSearch();
};

const handleBatchDelete = async () => {
  if (selectedRows.value.length === 0) return;
  await deleteAmAssetCategoryProfile(selectedRows.value.map((x: any) => x.id));
  ElMessage.success("删除成功");
  handleSearch();
};
</script>
