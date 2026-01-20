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
            <el-button
              type="danger"
              v-auth="['cms:variate:delete']"
              v-if="selectedRows.length > 0"
            >
              <el-icon><Delete /></el-icon>
              批量删除
            </el-button>
          </template>
        </el-popconfirm>
        <el-button
          type="primary"
          v-auth="['cms:variate:add']"
          @click="openDialog"
        >
          <el-icon><Plus /></el-icon>
          新增变量
        </el-button>
      </div>
    </el-header>
    <el-main class="bg-card border-border mt-2 rounded-[.5vw]">
      <soaTable
        ref="tableRef"
        :columns="columns"
        :apiObj="fetchCmsVariatePage"
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
              <el-button
                link
                type="primary"
                v-auth="['cms:variate:edit']"
                @click="openDialog(record)"
                >编辑</el-button
              >
              <el-popconfirm
                title="确认删除该数据？"
                @confirm="handleDelete(record.id)"
              >
                <template #reference>
                  <el-button v-auth="['cms:variate:delete']" link type="danger"
                    >删除</el-button
                  >
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
import { fetchCmsVariatePage, deleteCmsVariate } from "@/api/cms/variate";
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
    title: "组名称",
    dataIndex: "group",
    key: "group",
    resizable: true,
    fixed: true,
    minWidth: 150,
    ellipsis: true,
  },
  {
    title: "标题",
    dataIndex: "title",
    minWidth: 150,
    key: "title",
  },
  {
    title: "字段名称",
    dataIndex: "field",
    key: "field",
    minWidth: 150,
    align: "left",
  },
  {
    title: "字段类型",
    dataIndex: "fieldTypeName",
    key: "fieldTypeName",
    align: "center",
    width: 120,
  },
  {
    title: "值",
    dataIndex: "value",
    key: "value",
    minWidth: 200,
    ellipsis: true,
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
  await deleteCmsVariate([id]);
  ElMessage.success("删除成功");
  handleSearch();
};

const handleDeleteCode = async () => {
  const ids = selectedRows.value.map((m) => m.id);
  await deleteCmsVariate(ids);
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
