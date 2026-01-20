<template>
  <el-container class="h-full">
    <el-aside width="300px" class="h-full">
      <div
        class="bg-card border-border org-tree h-full w-full rounded-[.5vw] p-3"
      >
        <div
          class="mb-2 flex items-center justify-between border-b pb-2 dark:border-slate-800"
        >
          <span>菜单列表</span>
          <div class="menu-card__actions">
            <el-button
              type="primary"
              :icon="Plus"
              circle
              v-access:code="'config-code:add'"
              @click="handleColumnAdd"
            />
            <el-popconfirm
              title="确认执行批量删除吗？确定后会连同字典值同样删除？"
              width="220"
              @confirm="handleColumnDelete"
            >
              <template #reference>
                <el-button
                  type="danger"
                  :icon="Delete"
                  circle
                  v-if="selectColumn.id != '1'"
                />
              </template>
            </el-popconfirm>
          </div>
        </div>
        <el-scrollbar style="height: 78vh">
          <el-tree
            ref="groupRef"
            node-key="id"
            default-expand-all
            :data="group"
            :current-node-key="''"
            :highlight-current="true"
            :expand-on-click-node="false"
            @node-click="groupClick"
          >
            <template #default="{ node, data }">
              <span
                class="custom-tree-node block flex items-center justify-between"
              >
                <span class="label">{{ node.label }}</span>
                <span class="code">{{ data.code }}</span>
                <span class="do">
                  <el-icon @click.stop="editColumn(data)"><Edit /></el-icon>
                  <el-popconfirm
                    title="确认执行批量删除吗？确定后会连同字典值同样删除？"
                    width="220"
                  >
                    <template #reference>
                      <el-icon><Delete /></el-icon>
                    </template>
                    <template #actions="{ confirm, cancel }">
                      <el-button size="small" @click="cancel">取消</el-button>
                      <el-button
                        type="danger"
                        size="small"
                        @click.stop="removeColumn(node, data)"
                      >
                        确认
                      </el-button>
                    </template>
                  </el-popconfirm>
                </span>
              </span>
            </template>
          </el-tree>
        </el-scrollbar>
      </div>
    </el-aside>
    <el-container class="h-full pl-2">
      <el-header
        class="bg-card border-border flex flex-wrap items-center justify-between gap-3 rounded-[.5vw]"
      >
        <el-form
          :inline="true"
          :model="query"
          class="mb-0 flex flex-wrap items-center gap-3"
        >
          <el-form-item label="关键词">
            <el-input v-model="query.key" placeholder="字典值/阈值" clearable />
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
                <el-icon><EditPen /></el-icon>
                批量删除
              </el-button>
            </template>
          </el-popconfirm>
          <el-button
            type="primary"
            :disabled="selectColumn.id == '1'"
            @click="openDialog"
          >
            <el-icon><Plus /></el-icon>
            新增字典值
          </el-button>
        </div>
      </el-header>
      <el-main class="bg-card border-border mt-2 rounded-[.5vw]">
        <soaTable
          ref="tableRef"
          :columns="columns"
          :apiObj="fetchSysCodePage"
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
            <template v-if="column.key === 'org'">
              {{ record?.organizeObj?.name }}
            </template>
            <template v-if="column.key === 'avatar'">
              <el-avatar
                :src="serverUrl + record.avatar"
                size="small"
              ></el-avatar>
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
    </el-container>
    <modify ref="modifyRef" @complete="handleSearch"></modify>
    <column ref="columnRef" @complete="fetchColumn"></column>
  </el-container>
</template>

<script setup lang="ts">
import { Plus, Delete } from "@element-plus/icons-vue";
import {
  fetchSysCodePage,
  deleteSysCode,
  fetchSysColumnList,
  deleteSysColumn,
} from "@/api/sys/code";
import { changeTree } from "@/utils/tools";
import { ElMessage } from "element-plus";
import modify from "./modify.vue";
import column from "./column.vue";
const apiURL = import.meta.env.BASE_URL;
const serverUrl = apiURL.replace(/\/api$/i, "");
const tableRef = ref(null);
const modifyRef = ref(null);
const columnRef = ref();
const group = ref<Tree>();
const selectedRows = ref([]);
const selectColumn = ref({ id: "1" });
const query = reactive({
  page: 1,
  tenantId: 0,
  key: "",
  id: "0",
  status: undefined,
});
interface Tree {
  id: string;
  label: string;
  value: string;
  code: string;
  type: number;
  parentId: string;
  children?: Tree[];
}
const fetchColumn = async () => {
  let res = await fetchSysColumnList();
  let _tree: Array<Tree> = [
    { id: "1", value: "0", type: 0, label: "所有", parentId: "0", code: "" },
  ];
  res.some((m) => {
    _tree.push({
      id: m.id,
      value: m.id,
      label: m.name,
      code: m.code,
      type: m.types,
      parentId: m.parentId,
    });
  });
  group.value = changeTree(_tree);
};
const groupClick = (data: any) => {
  selectColumn.value = data;
  query.id = data.id;
  handleSearch();
};

const selectionChange = (params: any) => {
  selectedRows.value = params.selectedRows;
};
const columns = [
  {
    title: "字典值名称",
    dataIndex: "name",
    key: "name",
    resizable: true,
    fixed: true,
    minWidth: 150,
  },
  {
    title: "字典值阈值",
    dataIndex: "codeValues",
    key: "codeValues",
    minWidth: 150,
    resizable: true,
  },
  {
    title: "排序",
    dataIndex: "sort",
    key: "sort",
    width: 60,
    resizable: true,
    align: "center",
  },
  {
    title: "状态",
    dataIndex: "status",
    key: "status",
    align: "center",
    width: 80,
  },
  {
    title: "备注",
    dataIndex: "summary",
    key: "summary",
    width: 200,
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
  await deleteSysCode([id]);
  ElMessage.success("删除成功");
  selectColumn.value = { id: "1" };
  handleSearch();
};

const handleColumnDelete = async () => {
  await deleteSysColumn(selectColumn.value?.id);
  ElMessage.success("删除成功");
  selectColumn.value = { id: "1" };
  await fetchColumn();
  handleSearch();
};

const handleColumnAdd = () => {
  columnRef.value.openModal({});
};

const handleDeleteCode = async () => {
  const ids = selectedRows.value.map((m) => m.id);
  await deleteSysCode(ids);
  ElMessage.success("删除成功！");
  handleSearch();
};

const editColumn = (data: any) => {
  data.name = data.label;
  columnRef.value.openModal(data);
};

const removeColumn = async (node: any, data: any) => {
  await deleteSysColumn(data?.id);
  ElMessage.success("删除成功");
  await fetchColumn();
};

const openDialog = (row: any) => {
  modifyRef.value.openModal(row, selectColumn.value);
};

onMounted(async () => {
  await fetchColumn();
});
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
