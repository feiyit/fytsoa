<template>
  <el-dialog
    v-model="dialogVisible"
    :title="titleMap[type - 1]"
    :width="type === 1 ? 680 : 460"
    destroy-on-close
    append-to-body
    @closed="handleClosed"
  >
    <template v-if="type === 1">
      <div class="sc-user-select">
        <div class="sc-user-select__left">
          <div class="sc-user-select__search">
            <el-input v-model="keyword" placeholder="搜索成员">
              <template #prefix>
                <el-icon><Search /></el-icon>
              </template>
              <template #append>
                <el-button :icon="Search" @click="search" />
              </template>
            </el-input>
          </div>
          <div class="sc-user-select__select">
            <div class="sc-user-select__tree" v-loading="showGroupLoading">
              <el-scrollbar>
                <el-tree
                  ref="groupTree"
                  class="menu"
                  :data="group"
                  :node-key="groupProps.key"
                  :props="groupProps"
                  highlight-current
                  default-expand-all
                  :expand-on-click-node="false"
                  :current-node-key="groupId"
                  @node-click="groupClick"
                />
              </el-scrollbar>
            </div>
            <div class="sc-user-select__user" v-loading="showUserLoading">
              <div class="sc-user-select__user__list">
                <el-scrollbar ref="userScrollbar">
                  <el-tree
                    ref="userTree"
                    class="menu"
                    :data="user"
                    :node-key="userProps.key"
                    :props="userProps"
                    :default-checked-keys="selectedIds"
                    show-checkbox
                    default-expand-all
                    check-on-click-node
                    @check-change="userClick"
                  />
                </el-scrollbar>
              </div>
              <footer>
                <el-pagination
                  background
                  layout="prev,next"
                  small
                  :total="total"
                  :page-size="pageSize"
                  v-model:current-page="currentPage"
                  @current-change="paginationChange"
                />
              </footer>
            </div>
          </div>
        </div>
        <div class="sc-user-select__toicon">
          <el-icon><ArrowRight /></el-icon>
        </div>
        <div class="sc-user-select__selected">
          <header>已选 ({{ selected.length }})</header>
          <ul>
            <el-scrollbar>
              <li v-for="(item, index) in selected" :key="item.id">
                <span class="name">
                  <el-avatar size="small">{{
                    item.name.substring(0, 1)
                  }}</el-avatar>
                  <label>{{ item.name }}</label>
                </span>
                <span class="delete">
                  <el-button
                    type="danger"
                    :icon="Delete"
                    circle
                    size="small"
                    @click="deleteSelected(index)"
                  />
                </span>
              </li>
            </el-scrollbar>
          </ul>
        </div>
      </div>
    </template>

    <template v-if="type === 2">
      <div class="sc-user-select sc-user-select-role">
        <div class="sc-user-select__left">
          <div class="sc-user-select__select">
            <div class="sc-user-select__tree" v-loading="showGroupLoading">
              <el-scrollbar>
                <el-tree
                  ref="roleTree"
                  class="menu"
                  :data="role"
                  :node-key="roleProps.key"
                  :props="roleProps"
                  show-checkbox
                  check-strictly
                  default-expand-all
                  check-on-click-node
                  :expand-on-click-node="false"
                  :default-checked-keys="selectedIds"
                  @check-change="roleClick"
                />
              </el-scrollbar>
            </div>
          </div>
        </div>
        <div class="sc-user-select__toicon">
          <el-icon><ArrowRight /></el-icon>
        </div>
        <div class="sc-user-select__selected">
          <header>已选 ({{ selected.length }})</header>
          <ul>
            <el-scrollbar>
              <li v-for="(item, index) in selected" :key="item.id">
                <span class="name">
                  <label>{{ item.name }}</label>
                </span>
                <span class="delete">
                  <el-button
                    type="danger"
                    :icon="Delete"
                    circle
                    size="small"
                    @click="deleteSelected(index)"
                  />
                </span>
              </li>
            </el-scrollbar>
          </ul>
        </div>
      </div>
    </template>

    <template #footer>
      <el-button @click="dialogVisible = false">取 消</el-button>
      <el-button type="primary" @click="save">确 认</el-button>
    </template>
  </el-dialog>
</template>

<script setup lang="ts">
import { computed, nextTick, ref } from "vue";
import { ArrowRight, Delete, Search } from "@element-plus/icons-vue";
import { fetchAdminList, fetchRoleList, fetchOrgUnitList } from "@/api";
import { changeTree } from "@/utils/tools";
import workflowConfig from "./workflowConfig";
import type { SelectValueItem, WorkflowSelectType } from "./types";

const emit = defineEmits<{ (e: "closed"): void }>();

const config = workflowConfig;

const groupProps = config.group.props;
const userProps = config.user.props;
const roleProps = config.role.props;

const titleMap = ["人员选择", "角色选择"];

const dialogVisible = ref(false);
const showGroupLoading = ref(false);
const showUserLoading = ref(false);
const keyword = ref("");
const groupId = ref<string | number>("");
const pageSize = config.user.pageSize;
const total = ref(0);
const currentPage = ref(1);
const group = ref<any[]>([]);
const user = ref<any[]>([]);
const role = ref<any[]>([]);
const type = ref<WorkflowSelectType>(1);
const selected = ref<SelectValueItem[]>([]);
const target = ref<SelectValueItem[] | null>(null);

const groupTree = ref();
const userTree = ref();
const roleTree = ref();
const userScrollbar = ref<{ setScrollTop: (value: number) => void } | null>(
  null
);

const selectedIds = computed(() => selected.value.map((item) => item.id));

const handleClosed = () => {
  emit("closed");
};

const open = async (
  openType: WorkflowSelectType,
  data: SelectValueItem[] = []
) => {
  type.value = openType;
  target.value = data;
  selected.value = JSON.parse(JSON.stringify(data ?? []));
  keyword.value = "";
  groupId.value = "";
  currentPage.value = 1;
  dialogVisible.value = true;

  if (openType === 1) {
    await Promise.all([getGroup(), getUser()]);
    if (groupTree.value) {
      groupTree.value.setCurrentKey(groupId.value);
    }
  } else if (openType === 2) {
    await getRole();
  }
};

defineExpose({ open });

const getGroup = async () => {
  showGroupLoading.value = true;
  try {
    // 从后台获取组织机构列表
    const res = await fetchOrgUnitList({
      page: 1,
      limit: 1000,
    } as any);

    const items = Array.isArray(res) ? res : res?.items || [];

    // 映射为树构建所需结构
    const mapped = items.map((m: any) => ({
      id: m.id,
      parentId: m.parentId ?? "0",
      label: m.name,
    }));

    // 顶部“所有”节点
    const allNode = {
      [config.group.props.key]: "",
      [config.group.props.label]: "所有",
      parentId: "0",
    };

    // 使用项目中统一的 changeTree 工具构建树
    group.value = changeTree([allNode, ...mapped]);
  } finally {
    showGroupLoading.value = false;
  }
};

const getUser = async () => {
  showUserLoading.value = true;
  try {
    // 通过系统用户接口获取人员列表
    const params: Record<string, unknown> = {};
    if (keyword.value) {
      // 关键字搜索：登录账号 / 姓名等
      params.key = keyword.value;
    }
    if (groupId.value) {
      // 组织筛选：与后台约定为 orgId
      params.orgId = groupId.value;
    }

    const res: any = await fetchAdminList(params);

    let rows: any[] = [];
    let totalCount = 0;

    if (Array.isArray(res)) {
      // 纯数组：前端分页
      totalCount = res.length;
      const start = (currentPage.value - 1) * pageSize;
      const end = start + pageSize;
      rows = res.slice(start, end);
    } else {
      // 带分页结构：{ items, totalItems }
      rows = Array.isArray(res.items) ? res.items : [];
      totalCount = Number(res.totalItems) || rows.length;
    }

    // 适配为组件内部所需的结构：使用 config.user.props.key / label
    user.value = rows.map((u: any) => ({
      ...u,
      [config.user.props.key]: u.id,
      [config.user.props.label]:
        u.fullName ||
        u.displayName ||
        u.userName ||
        u.loginAccount ||
        u.mobile ||
        u.id,
    }));
    total.value = totalCount;

    await nextTick();
    if (userTree.value) {
      userTree.value.setCheckedKeys(selectedIds.value);
    }
    userScrollbar.value?.setScrollTop(0);
  } finally {
    showUserLoading.value = false;
  }
};

const getRole = async () => {
  showGroupLoading.value = true;
  try {
    // 通过系统角色接口获取角色列表
    const res: any = await fetchRoleList();
    const list: any[] = Array.isArray(res) ? res : res?.items || [];

    // 适配为树结构，字段与 config.role.props 对齐
    const mapped = list.map((r: any) => ({
      ...r,
      [config.role.props.key]: r.id,
      [config.role.props.label]: r.name || r.label,
    }));

    // 如果后端包含 parentId，则转换为树；否则保持平铺结构
    role.value = mapped.some((x) => x.parentId) ? changeTree(mapped) : mapped;

    await nextTick();
    if (roleTree.value) {
      roleTree.value.setCheckedKeys(selectedIds.value);
    }
  } finally {
    showGroupLoading.value = false;
  }
};

const groupClick = (data: Record<string, any>) => {
  keyword.value = "";
  currentPage.value = 1;
  groupId.value = data[config.group.props.key];
  getUser();
};

const userClick = (data: Record<string, any>, checked: boolean) => {
  const id = data[config.user.props.key];
  const name = data[config.user.props.label];
  if (checked) {
    if (!selected.value.find((item) => item.id === id)) {
      selected.value.push({ id, name });
    }
  } else {
    selected.value = selected.value.filter((item) => item.id !== id);
  }
};

const paginationChange = () => {
  getUser();
};

const search = () => {
  groupId.value = "";
  if (groupTree.value) {
    groupTree.value.setCurrentKey(groupId.value);
  }
  currentPage.value = 1;
  getUser();
};

const deleteSelected = (index: number) => {
  const [removed] = selected.value.splice(index, 1);
  if (type.value === 1 && removed && userTree.value) {
    userTree.value.setCheckedKeys(selectedIds.value);
  } else if (type.value === 2 && removed && roleTree.value) {
    roleTree.value.setCheckedKeys(selectedIds.value);
  }
};

const roleClick = (data: Record<string, any>, checked: boolean) => {
  const id = data[config.role.props.key];
  const name = data[config.role.props.label];
  if (checked) {
    if (!selected.value.find((item) => item.id === id)) {
      selected.value.push({ id, name });
    }
  } else {
    selected.value = selected.value.filter((item) => item.id !== id);
  }
};

const save = () => {
  if (!target.value) {
    dialogVisible.value = false;
    return;
  }
  target.value.splice(
    0,
    target.value.length,
    ...selected.value.map((item) => ({ ...item }))
  );
  dialogVisible.value = false;
};
</script>

<style scoped>
.sc-user-select {
  display: flex;
}

.sc-user-select__left {
  width: 400px;
}

.sc-user-select__right {
  flex: 1;
}

.sc-user-select__search {
  padding-bottom: 10px;
}

.sc-user-select__select {
  display: flex;
  border: 1px solid var(--el-border-color-light);
  /* background: var(--el-color-white); */
}

.sc-user-select__tree {
  width: 200px;
  height: 300px;
  border-right: 1px solid var(--el-border-color-light);
}

.sc-user-select__user {
  width: 200px;
  height: 300px;
  display: flex;
  flex-direction: column;
}

.sc-user-select__user__list {
  flex: 1;
  overflow: auto;
}

.sc-user-select__user footer {
  height: 36px;
  padding-top: 5px;
  border-top: 1px solid var(--el-border-color-light);
}

.sc-user-select__toicon {
  display: flex;
  justify-content: center;
  align-items: center;
  margin: 0 10px;
}

.sc-user-select__toicon i {
  display: flex;
  justify-content: center;
  align-items: center;
  /* background: #ccc; */
  width: 20px;
  height: 20px;
  text-align: center;
  line-height: 20px;
  border-radius: 50%;
  color: #fff;
}

.sc-user-select__selected {
  height: 345px;
  width: 200px;
  border: 1px solid var(--el-border-color-light);
  /* background: var(--el-color-white); */
}

.sc-user-select__selected header {
  height: 43px;
  line-height: 43px;
  border-bottom: 1px solid var(--el-border-color-light);
  padding: 0 15px;
  font-size: 12px;
}

.sc-user-select__selected ul {
  height: 300px;
  overflow: auto;
}

.sc-user-select__selected li {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 5px 5px 5px 15px;
  height: 38px;
}

.sc-user-select__selected li .name {
  display: flex;
  align-items: center;
}

.sc-user-select__selected li .name .el-avatar {
  background: #409eff;
  margin-right: 10px;
}

.sc-user-select__selected li .delete {
  display: none;
}

.sc-user-select__selected li:hover {
  background: var(--el-color-primary-light-9);
}

.sc-user-select__selected li:hover .delete {
  display: inline-block;
}

.sc-user-select-role .sc-user-select__left {
  width: 200px;
}

.sc-user-select-role .sc-user-select__tree {
  border: none;
  height: 343px;
}

[data-theme="dark"] .sc-user-select__selected li:hover {
  background: rgba(0, 0, 0, 0.2);
}

[data-theme="dark"] .sc-user-select__toicon i {
  background: #383838;
}
</style>
