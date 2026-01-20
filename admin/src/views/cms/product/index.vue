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
        <el-form-item label="关键词">
          <el-input
            v-model="query.key"
            placeholder="产品名称 / 编号"
            clearable
          />
        </el-form-item>
        <el-form-item label="状态">
          <el-select
            v-model="query.status"
            placeholder="全部"
            style="width: 120px"
            clearable
          >
            <el-option value="1" label="上架">上架</el-option>
            <el-option value="2" label="下架">下架</el-option>
          </el-select>
        </el-form-item>
        <el-form-item label="售价区间">
          <el-input-number
            v-model="query.minPrice"
            :min="0"
            :step="1"
            placeholder="最低价"
            :controls="false"
            style="width: 120px"
          />
          <span class="mx-1">~</span>
          <el-input-number
            v-model="query.maxPrice"
            :min="0"
            :step="1"
            placeholder="最高价"
            :controls="false"
            style="width: 120px"
          />
        </el-form-item>
        <el-form-item>
          <el-button type="primary" @click="handleSearch">查询</el-button>
          <el-button @click="handleReset">重置</el-button>
        </el-form-item>
      </el-form>
      <div>
        <el-popconfirm
          title="确认执行批量删除操作？"
          @confirm="handleDeleteSelected"
        >
          <template #reference>
            <el-button type="danger" v-if="selectedRows.length > 0">
              <el-icon><Delete /></el-icon>
              批量删除
            </el-button>
          </template>
        </el-popconfirm>
        <el-button type="primary" @click="openDialog">
          <el-icon><Plus /></el-icon>
          新增产品
        </el-button>
      </div>
    </el-header>
    <el-main class="bg-card border-border mt-2 rounded-[.5vw]">
      <soaTable
        ref="tableRef"
        :columns="columns"
        :apiObj="fetchCmsProductPage"
        :params="{ id: columnId }"
        row-key="id"
        :rowHeight="70"
        row-serial-number
        @selection-change="selectionChange"
      >
        <template #bodyCell="{ text, column, record }">
          <template v-if="column.key === 'imgUrl'">
            <div v-if="record.imgUrl" class="pt-2">
              <el-image
                :src="serverUrl + record.imgUrl"
                :preview-src-list="[serverUrl + record.imgUrl]"
                style="width: 100px; height: 60px"
                preview-teleported
                fit="cover"
              >
                <template #error>
                  <div
                    class="flex items-center justify-center text-xs text-gray-400"
                    style="width: 100px; height: 60px; background: #f5f5f5"
                  >
                    无图
                  </div>
                </template>
              </el-image>
            </div>
            <div
              v-else
              class="flex items-center justify-center text-xs text-gray-400"
              style="width: 100px; height: 60px; background: #f5f5f5"
            >
              无图
            </div>
          </template>
          <template v-if="column.key === 'title'">
            {{ "【" + (record.columnName || "未分类") + "】-" + record.title }}
          </template>
          <template v-if="column.key === 'status'">
            <el-tag :type="record.status ? 'success' : 'danger'">
              {{ record.status ? "上架" : "下架" }}
            </el-tag>
          </template>
          <template v-if="column.key === 'action'">
            <div class="flex items-center gap-1">
              <el-button link type="primary" @click="openDialog(record)">
                编辑
              </el-button>
              <el-popconfirm
                title="确认删除该数据？"
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
    <modify ref="modifyRef" @complete="handleSearch"></modify>
  </el-container>
</template>

<script setup lang="ts">
import { fetchCmsProductPage, deleteCmsProduct } from "@/api/cms/product";

const apiURL = (import.meta as any).env.VITE_API_BASE_URL as string;
const serverUrl = apiURL.replace("/api", "");

const modify = defineAsyncComponent(() => import("./modify.vue"));
const tableRef = ref<any | null>(null);
const modifyRef = ref<any | null>(null);
const selectedRows = ref<any[]>([]);

const props = defineProps({
  id: { type: String, default: "0" },
  columnId: { type: String, default: "0" },
});

const query = reactive({
  page: 1,
  id: "0",
  key: "",
  status: undefined as string | undefined,
  minPrice: undefined as number | undefined,
  maxPrice: undefined as number | undefined,
});
watch(
  () => props.columnId,
  (newId) => {
    query.id = newId;
    setTimeout(() => {
      handleSearch();
    }, 50);
  }
);
const columns = [
  {
    title: "封面图",
    dataIndex: "imgUrl",
    key: "imgUrl",
    align: "center",
    width: 130,
  },
  {
    title: "产品名称",
    dataIndex: "title",
    key: "title",
    resizable: true,
    fixed: true,
    ellipsis: true,
    minWidth: 260,
  },
  {
    title: "销售价",
    dataIndex: "price",
    key: "price",
    align: "center",
    width: 100,
  },
  {
    title: "市场价",
    dataIndex: "marketPrice",
    key: "marketPrice",
    align: "center",
    width: 100,
  },
  {
    title: "状态",
    dataIndex: "status",
    key: "status",
    align: "center",
    width: 80,
  },
  {
    title: "权重",
    dataIndex: "sort",
    key: "sort",
    align: "center",
    width: 80,
  },
  {
    title: "点击量",
    dataIndex: "hits",
    key: "hits",
    align: "center",
    width: 90,
  },
  {
    title: "创建时间",
    dataIndex: "createTime",
    key: "createTime",
    width: 170,
  },
  {
    title: "操作",
    key: "action",
    width: 120,
    fixed: "right",
  },
];

const handleSearch = () => {
  query.page = 1;
  tableRef?.value?.upData(query);
};

const handleReset = () => {
  query.key = "";
  query.status = undefined;
  query.minPrice = undefined;
  query.maxPrice = undefined;
  handleSearch();
};

const selectionChange = (params: any) => {
  selectedRows.value = params.selectedRows;
};

const handleDelete = async (id: string) => {
  await deleteCmsProduct([id]);
  ElMessage.success("删除成功");
  handleSearch();
};

const handleDeleteSelected = async () => {
  const ids = selectedRows.value.map((m) => m.id);
  if (ids.length === 0) return;
  await deleteCmsProduct(ids);
  ElMessage.success("删除成功！");
  handleSearch();
};

const openDialog = (row?: any) => {
  modifyRef.value?.openModal(row || null, props.columnId);
};
</script>

<style scoped>
:deep(.org-tree .el-tree-node__content) {
  height: 36px;
  border-radius: 8px;
}
</style>
