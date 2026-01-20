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
          @confirm="handleDelete(0)"
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
          新增
        </el-button>
      </div>
    </el-header>
    <el-main class="bg-card border-border mt-2 rounded-[.5vw]">
      <soaTable
        ref="tableRef"
        :columns="columns"
        :apiObj="fetchCmsColumnList"
        row-key="id"
        tree
        row-serial-number
        :footerShow="false"
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
          <template v-if="column.key === 'isSystem'">
            <el-tag :type="record.isSystem ? 'success' : 'warning'">
              {{ record.status ? "是" : "否" }}
            </el-tag>
          </template>
          <template v-if="column.key === 'action'">
            <div class="flex items-center gap-1">
              <el-button link type="primary" @click="openDialog(record, true)"
                >添加子项</el-button
              >
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
      <modify ref="modifyRef" @complete="handleSearch"></modify>
    </el-main>
  </el-container>
</template>

<script setup lang="ts">
import { fetchCmsColumnList, deleteCmsColumn } from "@/api/cms/column";
import modify from "./modify.vue";
const tableRef = ref(null);
const modifyRef = ref(null);
const selectedRows = ref([]);
const query = reactive({
  key: "",
  status: undefined,
});
const selectionChange = (params: any) => {
  selectedRows.value = params.selectedRows;
};

const columns = [
  {
    title: "栏目标题",
    dataIndex: "title",
    key: "title",
    resizable: true,
    fixed: true,
  },
  {
    title: "栏目编号",
    dataIndex: "number",
    key: "number",
    width: 150,
    align: "center",
  },
  {
    title: "排序",
    dataIndex: "sort",
    key: "sort",
    width: 80,
    align: "center",
  },
  {
    title: "状态",
    dataIndex: "status",
    key: "status",
    width: 80,
    align: "center",
  },
  {
    title: "模板名称",
    dataIndex: "templateName",
    key: "templateName",
    width: 150,
    align: "center",
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
    width: 180,
    align: "center",
    fixed: "right",
  },
];

const handleSearch = () => {
  tableRef?.value.upData(query);
};

const handleReset = () => {
  query.key = "";
  query.status = undefined;
  handleSearch();
};

const handleDelete = async (id: number) => {
  if (id != 0) {
    await deleteCmsColumn([id]);
  } else {
    const ids = selectedRows.value.map((m) => m.id);
    await deleteCmsColumn(ids);
    selectedRows.value = [];
  }
  handleSearch();
  ElMessage.success("删除成功");
};

const openDialog = (row: any, subitem: boolean = false) => {
  modifyRef.value.openModal(row, subitem);
};

onMounted(async () => {});
</script>
