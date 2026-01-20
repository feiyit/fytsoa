<script setup lang="ts">
import { Plus, Delete } from "@element-plus/icons-vue";
import soaFormTable from "@/component/soaFormTable/index.vue";
import soaIcon from "@/component/soaIcon/index.vue";

import { sysmenuList, sysmenuAdd, sysmenuPut, sysmenuDelete } from "@/api";
import { changeTree } from "@/utils/index";

type MenuType = "menu" | "iframe" | "external";

type MenuStatus = "enabled" | "disabled";

interface PermissionRow {
  name: string;
  code: string;
  type: string;
}

interface MenuNode {
  id: string;
  parentId: string | null;
  name: string;
  code: string;
  icon: string;
  type: MenuType;
  url: string;
  vuePath: string;
  status: MenuStatus;
  permissions: PermissionRow[];
  children?: MenuNode[];
}

interface MenuFormModel {
  id: number;
  parentId: string | null;
  name: string;
  code: string;
  icon: string;
  types: MenuType;
  urls: string;
  vuePath: string;
  status: MenuStatus;
  sort: number;
  api: Array;
}

const menuTree = ref<MenuNode[]>([]);

const treeProps = {
  label: "name",
  children: "children",
  disabled: "disabled",
};

const menuTreeRef = ref();

const permissionTemplate: PermissionRow = {
  name: "",
  code: "",
  type: "",
};

const permissions = ref<PermissionRow[]>([]);

const formModel = reactive<MenuFormModel>({
  parentId: null,
  name: "",
  code: "",
  icon: "",
  types: "menu",
  urls: "",
  vuePath: "",
  status: "enabled",
  api: [],
  sort: 0,
});

const currentMenuId = ref<string | null>(null);
const isCreating = ref(false);

const mapParentOptions = (nodes: MenuNode[]): MenuNode[] =>
  nodes.map((node) => ({
    ...node,
    disabled: currentMenuId.value === node.id,
    children: node.children ? mapParentOptions(node.children) : undefined,
  }));

const parentOptions = computed(() => mapParentOptions(menuTree.value));

function resetForm(defaultValues?: Partial<MenuFormModel>) {
  formModel.id = defaultValues?.id;
  formModel.parentId = defaultValues?.parentId ?? null;
  formModel.name = defaultValues?.name ?? "";
  formModel.code = defaultValues?.code ?? "";
  formModel.icon = defaultValues?.icon ?? "";
  formModel.types = defaultValues?.types ?? "menu";
  formModel.urls = defaultValues?.urls ?? "";
  formModel.vuePath = defaultValues?.vuePath ?? "";
  formModel.status = defaultValues?.status ?? "enabled";
  formModel.sort = defaultValues?.sort ?? 0;
  formModel.api = defaultValues?.api;
}

function clonePermissions(rows: PermissionRow[] = []): PermissionRow[] {
  return rows.map((item) => ({ ...item }));
}

function findNodeById(id: string, nodes = menuTree.value): MenuNode | null {
  for (const node of nodes) {
    if (node.id === id) {
      return node;
    }
    if (node.children?.length) {
      const found = findNodeById(id, node.children);
      if (found) {
        return found;
      }
    }
  }
  return null;
}

function findParentNode(
  id: string,
  nodes = menuTree.value,
  parent: MenuNode | null = null
): MenuNode | null {
  for (const node of nodes) {
    if (node.id === id) {
      return parent;
    }
    if (node.children?.length) {
      const found = findParentNode(id, node.children, node);
      if (found) {
        return found;
      }
    }
  }
  return null;
}

function removeNodesByIds(
  ids: Set<string>,
  nodes = menuTree.value
): MenuNode[] {
  return nodes
    .filter((node) => !ids.has(node.id))
    .map((node) => ({
      ...node,
      children: node.children
        ? removeNodesByIds(ids, node.children)
        : undefined,
    }));
}

function handleMenuClick(data: MenuNode) {
  currentMenuId.value = data.id;
  isCreating.value = false;
  resetForm({
    id: data.id,
    parentId: data.parentId,
    name: data.name,
    code: data.code,
    icon: data.icon,
    types: data.types,
    urls: data.urls,
    vuePath: data.vuePath,
    status: data.status,
    sort: data.sort,
    api: data.api,
  });
}

async function handleAddMenu(node, data) {
  console.log();
  let resetData = {
    id: 0,
    name: "未命名",
    parentId: undefined,
    status: true,
    types: "menu",
  };
  if (data) {
    resetData.parentId = data.id;
  }
  const res = await sysmenuAdd(resetData);
  isCreating.value = false;
  resetForm(res);
  currentMenuId.value = res.id;
  init();
}

function clonePermissionRow(name: string, code: string): PermissionRow {
  return {
    name,
    code: code ? `${code}` : "",
    type: code ? code.replace(/^(.*:)?/, "").toUpperCase() : "",
  };
}

function validateForm(): boolean {
  if (!formModel.name.trim()) {
    ElMessage.warning("请输入菜单名称");
    return false;
  }
  if (!formModel.code.trim()) {
    ElMessage.warning("请输入菜单别名（权限标识）");
    return false;
  }
  return true;
}

async function saveMenu() {
  if (!validateForm()) {
    return;
  }

  const payload: MenuNode = {
    id: formModel.id,
    parentId: formModel.parentId ?? null,
    name: formModel.name.trim(),
    code: formModel.code.trim(),
    icon: formModel.icon,
    types: formModel.types,
    urls: formModel.urls,
    vuePath: formModel.vuePath,
    status: formModel.status,
    sort: formModel.sort,
    api: formModel.api,
  };
  await sysmenuPut(payload);
  ElMessage.success("菜单保存成功");
  init();
}

async function handleBatchDelete() {
  const checked: string[] = menuTreeRef.value?.getCheckedKeys?.() ?? [];
  if (!checked.length) {
    ElMessage.warning("请选择需要批量删除的菜单");
    return;
  }

  await ElMessageBox.confirm(
    `确定删除选中的 ${checked.length} 个菜单吗？`,
    "提示",
    {
      type: "warning",
    }
  );
  await sysmenuDelete(checked);
  ElMessage.success("批量删除成功");
  init();
}

function handleReset() {
  if (!currentMenuId.value) {
    resetForm();
    permissions.value = [];
    return;
  }
  const current = findNodeById(currentMenuId.value);
  if (current) {
    handleMenuClick(current);
  }
}

const menuTypeOptions = [
  { label: "菜单", value: "menu" },
  { label: "IFrame", value: "iframe" },
  { label: "外链", value: "link" },
];

if (menuTree.value.length) {
  handleMenuClick(menuTree.value[0]);
}

async function init() {
  const res = await sysmenuList();
  menuTree.value = changeTree(res);
}
init();
</script>

<template>
  <div class="menu-page">
    <el-row :gutter="10">
      <el-col :lg="5" :sm="12">
        <el-card shadow="never" class="menu-card">
          <template #header>
            <div class="menu-card__header">
              <span>菜单列表</span>
              <div class="menu-card__actions">
                <el-button
                  type="primary"
                  :icon="Plus"
                  circle
                  @click="handleAddMenu"
                />
                <el-button
                  type="danger"
                  :icon="Delete"
                  circle
                  @click="handleBatchDelete"
                />
              </div>
            </div>
          </template>
          <div class="h-full overflow-auto">
            <el-tree
              ref="menuTreeRef"
              :data="menuTree"
              node-key="id"
              show-checkbox
              highlight-current
              :props="treeProps"
              default-expand-all
              @node-click="handleMenuClick"
              class="menu-tree"
            >
              <template #default="{ node, data }">
                <span class="custom-tree-node el-tree-node__label">
                  <span class="label">
                    {{ node.label }}
                  </span>
                  <span class="do" @click.stop="handleAddMenu(node, data)">
                    <el-icon>
                      <Plus />
                    </el-icon>
                  </span>
                </span> </template
            ></el-tree>
          </div>
        </el-card>
      </el-col>

      <el-col :lg="11" :sm="12">
        <el-card shadow="never" class="menu-card">
          <template #header>
            <div class="menu-card__header">
              <span>接口文档</span>
              <div class="menu-card__actions">
                <el-button type="primary" @click="saveMenu">保存</el-button>
                <el-button
                  type="info"
                  plain
                  :icon="Refresh"
                  @click="handleReset"
                >
                  重置
                </el-button>
              </div>
            </div>
          </template>
          <el-form label-width="100px" :model="formModel" class="menu-form">
            <el-form-item label="上级菜单">
              <el-tree-select
                v-model="formModel.parentId"
                :data="parentOptions"
                :props="treeProps"
                node-key="id"
                check-strictly
                placeholder="请选择上级菜单"
                clearable
              />
            </el-form-item>
            <el-form-item label="名称">
              <el-input v-model="formModel.name" placeholder="请输入菜单名称" />
            </el-form-item>
            <el-form-item label="别名">
              <el-input
                v-model="formModel.code"
                placeholder="权限标识，如 sys:menu"
              />
            </el-form-item>
            <el-form-item label="图标">
              <soa-icon v-model="formModel.icon" />
            </el-form-item>
            <el-form-item label="类型">
              <el-radio-group v-model="formModel.types">
                <el-radio-button
                  v-for="option in menuTypeOptions"
                  :key="option.value"
                  :value="option.value"
                >
                  {{ option.label }}
                </el-radio-button>
              </el-radio-group>
            </el-form-item>
            <el-form-item label="访问地址">
              <el-input
                v-model="formModel.urls"
                placeholder="请输入路由访问地址"
              />
            </el-form-item>
            <el-form-item label="Vue 文件">
              <el-input
                v-model="formModel.vuePath"
                placeholder="示例：sys/menu/index.vue"
                ><template #prepend>Views/</template></el-input
              >
            </el-form-item>
            <el-form-item label="状态">
              <el-switch
                v-model="formModel.status"
                class="ml-2"
                width="60"
                inline-prompt
                active-text="启用"
                inactive-text="关闭"
              />
            </el-form-item>
            <el-form-item label="排序">
              <el-input-number v-model="formModel.sort" :min="1" />
            </el-form-item>
          </el-form>
        </el-card>
      </el-col>

      <el-col :lg="8" :sm="12">
        <el-card shadow="never" class="menu-card">
          <template #header>
            <div class="menu-card__header">
              <span>菜单权限</span>
            </div>
          </template>
          <soa-form-table
            v-model="formModel.api"
            :add-template="permissionTemplate"
            placeholder="暂未配置权限"
          >
            <el-table-column label="权限名称" width="140">
              <template #default="{ row }">
                <el-input v-model="row.name" placeholder="示例：新增" />
              </template>
            </el-table-column>
            <el-table-column label="权限类型">
              <template #default="{ row }">
                <el-input v-model="row.code" placeholder="示例：ADD" />
              </template>
            </el-table-column>
          </soa-form-table>
        </el-card>
      </el-col>
    </el-row>
  </div>
</template>

<style scoped>
.menu-card {
  height: 86.5vh;
  display: flex;
  flex-direction: column;
}

.menu-card__header {
  display: flex;
  align-items: center;
  justify-content: space-between;
}

.menu-card__actions {
  display: flex;
  gap: 8px;
}

.menu-card :deep(.el-card__body) {
  flex: 1;
  display: flex;
  flex-direction: column;
  overflow: hidden;
  min-height: 0;
}

.menu-form {
  padding-right: 8px;
  flex: 1;
  overflow: auto;
  min-height: 0;
}

.menu-tree {
  height: 100%;
  flex: 1;
  overflow: auto;
  min-height: 0;
}

:deep(.el-card__header) {
  padding: 12px;
}
:deep(.el-tree-node__content) {
  height: auto;
  padding-top: 5px;
  padding-bottom: 5px;
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
.custom-tree-node .label {
  display: flex;
  align-items: center;
  height: 100%;
}
.custom-tree-node .label .el-tag {
  margin-left: 5px;
}
.custom-tree-node .do {
  display: none;
}
.custom-tree-node .do i {
  margin-left: 5px;
  color: #999;
}
.custom-tree-node .do i:hover {
  color: #999;
}

.custom-tree-node:hover .do {
  display: inline-block;
}
.add-column {
  padding: 8px !important;
  margin: 8px;
}
</style>
