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
          <el-input v-model="query.key" placeholder="名称" clearable />
        </el-form-item>
        <el-form-item label="状态">
          <el-select
            v-model="query.status"
            placeholder="全部"
            style="width: 120px"
            clearable
          >
            <el-option label="启用" value="1" />
            <el-option label="禁用" value="2" />
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
          @confirm="handleDeleteCode"
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
          新增模版
        </el-button>
      </div>
    </el-header>
    <el-main class="bg-card border-border mt-2 rounded-[.5vw]">
      <soaTable
        ref="tableRef"
        :columns="columns"
        :apiObj="fetchCmsTemplatePage"
        row-key="id"
        row-serial-number
        @selection-change="selectionChange"
      >
        <template #bodyCell="{ text, column, record }">
          <template v-if="column.key === 'fullName'">
            <a>{{ text }}</a>
          </template>
          <template v-if="column.key === 'status'">
            <el-tag :type="record.status ? 'success' : 'danger'">
              {{ record.status ? "启用" : "禁用" }}
            </el-tag>
          </template>
          <template v-if="column.key === 'action'">
            <div class="flex items-center gap-1">
              <el-button link type="primary" @click="openDialog(record)"
                >编辑</el-button
              >
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
import { fetchCmsTemplatePage, deleteCmsTemplate } from "@/api/cms/template";
import { ElMessage } from "element-plus";
import modify from "./modify.vue";
const tableRef = ref(null);
const modifyRef = ref(null);
const selectedRows = ref([]);
const query = reactive({
  page: 1,
  tenantId: 0,
  key: "",
  id: "0",
  status: undefined,
});

const selectionChange = (params: any) => {
  selectedRows.value = params.selectedRows;
};
const columns = [
  {
    title: "模板名称",
    dataIndex: "name",
    key: "name",
    resizable: true,
    fixed: true,
    ellipsis: true,
  },
  {
    title: "模板地址",
    dataIndex: "urls",
    key: "urls",
    minWidth: 150,
    resizable: true,
    align: "left",
  },
  {
    title: "状态",
    dataIndex: "status",
    key: "status",
    align: "center",
    width: 80,
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
  tableRef?.value.upData(query);
};

const handleReset = () => {
  query.key = "";
  query.status = undefined;
  handleSearch();
};
const handleDelete = async (id: string) => {
  await deleteCmsTemplate([id]);
  ElMessage.success("删除成功");
  handleSearch();
};

const handleDeleteCode = async () => {
  const ids = selectedRows.value.map((m) => m.id);
  await deleteCmsTemplate(ids);
  ElMessage.success("删除成功！");
  handleSearch();
};

const openDialog = (row: any) => {
  modifyRef.value.openModal(row);
};

onMounted(async () => {});
</script>

<style scoped>
:deep(.org-tree .el-tree-node__content) {
  height: 36px;
  border-radius: 8px;
}
.custom-tree-node {
  display: flex;
  flex: 1;
  align-items: center;
  justify-content: space-between;
  font-size: 14px;
  padding-right: 24px;
  height: 100%;
}
.custom-tree-node .code {
  font-size: 12px;
  color: #999;
}
.custom-tree-node .do {
  display: none;
}
.custom-tree-node .do i {
  margin-left: 5px;
  color: #999;
}
.custom-tree-node:hover .code {
  display: none;
}
.custom-tree-node:hover .do {
  display: inline-block;
}
</style>
