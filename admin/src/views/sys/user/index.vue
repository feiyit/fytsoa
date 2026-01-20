<template>
  <el-container class="h-full">
    <el-aside width="240px" class="h-full">
      <div
        class="bg-card border-border org-tree h-full w-full rounded-[.5vw] p-3"
      >
        <el-tree
          ref="groupRef"
          node-key="id"
          default-expand-all
          :data="group"
          :current-node-key="''"
          :highlight-current="true"
          :expand-on-click-node="false"
          @node-click="groupClick"
        ></el-tree>
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
            <el-input v-model="query.key" placeholder="登录名/姓名" clearable />
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
          <el-button
            type="success"
            @click="handleAssignRoles"
            v-if="selectedRows.length > 0"
          >
            <el-icon><Stamp /></el-icon>
            分配角色
          </el-button>

          <el-popconfirm
            title="确认执行批量重置密码操作？"
            @confirm="handleBatchResetPwd"
          >
            <template #reference>
              <el-button type="warning" v-if="selectedRows.length > 0">
                <el-icon><EditPen /></el-icon>
                重置密码
              </el-button>
            </template>
          </el-popconfirm>
          <el-button type="primary" @click="openDialog">
            <el-icon><Plus /></el-icon>
            新增用户
          </el-button>
        </div>
      </el-header>
      <el-main class="bg-card border-border mt-2 rounded-[.5vw]">
        <soaTable
          ref="tableRef"
          :columns="columns"
          :apiObj="fetchAdminPage"
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
                  title="确认重置密码？"
                  @confirm="handleResetPassword(record.id)"
                >
                  <template #reference>
                    <el-button link type="warning">重置密码</el-button>
                  </template>
                </el-popconfirm>
                <el-popconfirm
                  title="确认删除该用户？"
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
    <assignroles ref="assignrolesRef"></assignroles>
  </el-container>
</template>

<script setup lang="ts">
import {
  fetchAdminPage,
  fetchOrgUnitList,
  deleteAdmin,
  resetAdminPassword,
} from "@/api";
import { changeTree } from "@/utils/tools";
import { ElMessage } from "element-plus";
import modify from "./modify.vue";
import assignroles from "./assignroles.vue";
const apiURL = import.meta.env.BASE_URL;
const serverUrl = apiURL.replace(/\/api$/i, "");
const tableRef = ref(null);
const modifyRef = ref(null);
const assignrolesRef = ref();
const group = ref<Tree>();
const selectedRows = ref([]);
const query = reactive({
  page: 1,
  tenantId: 0,
  key: "",
  orgId: undefined,
  status: undefined,
});
interface Tree {
  id: string;
  label: string;
  value: string;
  parentId: string;
  children?: Tree[];
}
const fetchOrg = async () => {
  let res = await fetchOrgUnitList();
  let _tree: Array<Tree> = [
    { id: "1", value: "0", label: "所有", parentId: "0" },
  ];
  res.some((m) => {
    _tree.push({
      id: m.id,
      value: m.id,
      label: m.name,
      parentId: m.parentId,
    });
  });
  group.value = changeTree(_tree);
};
const groupClick = (data: any) => {
  query.orgId = data.id;
  handleSearch();
};

const selectionChange = (params: any) => {
  selectedRows.value = params.selectedRows;
};
const columns = [
  {
    title: "姓名",
    dataIndex: "fullName",
    key: "fullName",
    resizable: true,
    rowDrag: true,
    fixed: true,
    minWidth: 150,
  },
  {
    title: "登录名",
    dataIndex: "loginAccount",
    key: "loginAccount",
    width: 100,
    resizable: true,
  },
  {
    title: "头像",
    dataIndex: "avatar",
    key: "avatar",
    width: 60,
    resizable: true,
    align: "center",
  },
  {
    title: "部门",
    dataIndex: "org",
    key: "org",
    width: 130,
  },
  {
    title: "手机号码",
    dataIndex: "mobile",
    key: "mobile",
    width: 130,
  },
  {
    title: "状态",
    dataIndex: "status",
    key: "status",
    width: 80,
    align: "center",
  },
  {
    title: "性别",
    dataIndex: "sex",
    key: "sex",
    width: 60,
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
  await deleteAdmin(id);
  ElMessage.success("删除成功");
  handleSearch();
};

const handleResetPassword = async (id: string) => {
  await resetAdminPassword([id]);
  ElMessage.success("密码重置成功!");
};

const handleBatchResetPwd = async () => {
  const ids = selectedRows.value.map((m) => m.id);
  await resetAdminPassword(ids);
  ElMessage.success("密码重置成功!");
};

const handleAssignRoles = () => {
  const ids = selectedRows.value.map((m) => m.id);
  assignrolesRef.value.openModal(ids);
};

const openDialog = (row: any) => {
  modifyRef.value.openModal(row);
};

onMounted(async () => {
  await fetchOrg();
});
</script>

<style scoped>
:deep(.org-tree .el-tree-node__content) {
  height: 36px;
  border-radius: 8px;
}
</style>
