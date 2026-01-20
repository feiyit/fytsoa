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
            <el-option value="1" label="已审核">已审核</el-option>
            <el-option value="2" label="未审核">未审核</el-option>
          </el-select>
        </el-form-item>
        <el-form-item>
          <el-button type="primary" @click="handleSearch">查询</el-button>
          <el-button @click="handleReset">重置</el-button>
        </el-form-item>
      </el-form>
      <div>
        <!-- <el-button
          type="danger"
          plain
          :disabled="selectedRows.length == 0"
          @click="handleRecycle"
          >删除到回收站</el-button
        >
        <el-button
          type="primary"
          plain
          :disabled="selectedRows.length == 0"
          @click="handleRecovery"
          >回收站恢复</el-button
        > -->
        <el-dropdown
          split-button
          type="primary"
          :disabled="selectedRows.length == 0"
          class="mr-3"
          @command="handleDropdown"
        >
          更多
          <template #dropdown>
            <el-dropdown-menu>
              <el-dropdown-item command="a">删除到回收站</el-dropdown-item>
              <el-dropdown-item command="b">回收站恢复</el-dropdown-item>
            </el-dropdown-menu>
          </template>
        </el-dropdown>
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
          新增文章
        </el-button>
      </div>
    </el-header>
    <el-main class="bg-card border-border mt-2 rounded-[.5vw]">
      <soaTable
        ref="tableRef"
        :columns="columns"
        :apiObj="fetchCmsArticlePage"
        :params="{ id: columnId }"
        row-key="id"
        row-serial-number
        @selection-change="selectionChange"
      >
        <template #bodyCell="{ text, column, record }">
          <template v-if="column.key === 'title'">
            {{ "【" + record.columnName + "】-" + record.title }}
          </template>
          <template v-if="column.key === 'tag'">
            <span v-for="it in record.tag" :key="it">{{ it }}、</span>
          </template>
          <template v-if="column.key === 'attr'">
            <el-tag :type="record.attr.indexOf(1) > -1 ? 'success' : 'info'">
              推荐
            </el-tag>
            <el-tag :type="record.attr.indexOf(2) > -1 ? 'success' : 'info'">
              热点
            </el-tag>
            <el-tag :type="record.attr.indexOf(3) > -1 ? 'success' : 'info'">
              滚动
            </el-tag>
            <el-tag :type="record.attr.indexOf(4) > -1 ? 'success' : 'info'">
              评论
            </el-tag>
            <el-tag :type="record.attr.indexOf(5) > -1 ? 'success' : 'info'">
              回收站
            </el-tag>
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
import {
  fetchCmsArticlePage,
  recycleCmsArticle,
  recoveryCmsArticle,
  deleteCmsArticle,
} from "@/api/cms/article";
const modify = defineAsyncComponent(() => import("./modify.vue"));
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
const props = defineProps({
  id: { type: String, default: "0" },
  columnId: { type: String, default: "0" },
});

const selectionChange = (params: any) => {
  selectedRows.value = params.selectedRows;
};
const columns = [
  {
    title: "文章标题",
    dataIndex: "title",
    key: "title",
    resizable: true,
    fixed: true,
    ellipsis: true,
    minWidth: 300,
  },
  {
    title: "属性",
    dataIndex: "attr",
    key: "attr",
    minWidth: 250,
    resizable: true,
    align: "left",
  },
  {
    title: "发布状态",
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
    title: "标签",
    dataIndex: "tag",
    key: "tag",
    align: "left",
    width: 200,
  },
  {
    title: "点击量",
    dataIndex: "hits",
    key: "hits",
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
  await deleteCmsArticle([id]);
  ElMessage.success("删除成功");
  handleSearch();
};

const handleDeleteCode = async () => {
  const ids = selectedRows.value.map((m) => m.id);
  await deleteCmsArticle(ids);
  ElMessage.success("删除成功！");
  handleSearch();
};

const handleDropdown = async (command: string | number | object) => {
  const ids = selectedRows.value.map((m) => m.id);
  if (command === "a") {
    await recycleCmsArticle(ids);
  }
  if (command === "b") {
    await recoveryCmsArticle(ids);
  }
  ElMessage.success("操作成功！");
  handleSearch();
};

const openDialog = (row: any) => {
  modifyRef.value.openModal(row, query.id);
};
watch(
  () => props.columnId,
  (newId) => {
    query.id = newId;
    setTimeout(() => {
      handleSearch();
    }, 50);
  }
);
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
