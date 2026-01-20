<template>
  <div class="soa-select-admin" :style="{ width: widthStyle }">
    <div class="flex">
      <!-- 展示区域：单选输入 / 多选标签 -->
      <template v-if="multiple">
        <div class="flex flex-wrap items-center gap-2">
          <el-tag
            v-for="item in selectedList"
            :key="item.id"
            closable
            type="info"
            size="small"
            @close="removeSelected(item)"
          >
            {{ getUserLabel(item) }}
          </el-tag>

          <el-button size="small" type="primary" @click="openDialog">
            选择用户
          </el-button>
          <el-button
            v-if="selectedList.length"
            size="small"
            text
            type="danger"
            @click="clearSelected"
          >
            清空
          </el-button>
        </div>
      </template>
      <template v-else>
        <el-input
          :model-value="selectedList[0] ? getUserLabel(selectedList[0]) : ''"
          :placeholder="placeholder"
          readonly
          @click="openDialog"
        >
          <template #append>
            <el-button type="primary" @click.stop="openDialog">选择</el-button>
          </template>
        </el-input>
        <el-button
          v-if="selectedList.length"
          size="small"
          text
          type="danger"
          class="mt-1"
          @click="clearSelected"
        >
          清空
        </el-button>
      </template>

      <!-- 选择用户弹窗 -->
      <el-dialog
        v-model="dialogVisible"
        title="选择用户"
        width="900px"
        :close-on-click-modal="false"
        :append-to-body="true"
      >
        <el-row :gutter="16">
          <!-- 左侧：组织机构树 -->
          <el-col :span="8">
            <el-card
              body-class="p-0"
              class="h-[420px] overflow-hidden"
              v-loading="loadingOrg"
            >
              <div class="border-b px-3 py-2 text-xs dark:border-slate-750">
                组织机构
              </div>
              <el-scrollbar class="h-[380px] px-2 py-2">
                <el-tree
                  v-if="orgTree.length"
                  :data="orgTree"
                  node-key="id"
                  default-expand-all
                  highlight-current
                  @node-click="handleOrgClick"
                />
                <div
                  v-else
                  class="flex h-full items-center justify-center text-xs text-slate-400"
                >
                  暂无组织数据
                </div>
              </el-scrollbar>
            </el-card>
          </el-col>

          <!-- 右侧：用户列表 -->
          <el-col :span="16">
            <el-card
              class="h-[420px] overflow-hidden"
              body-class="flex h-full flex-col"
              v-loading="loadingUser"
            >
              <div class="mb-3 flex items-center gap-2">
                <el-input
                  v-model="keyword"
                  placeholder="输入姓名 / 账号 / 手机号搜索"
                  clearable
                  @keyup.enter="loadUsers"
                  @clear="loadUsers"
                >
                  <template #prefix>
                    <el-icon><Search /></el-icon>
                  </template>
                </el-input>
                <el-button
                  type="primary"
                  :loading="loadingUser"
                  @click="loadUsers"
                >
                  搜索
                </el-button>
              </div>

              <el-table
                :data="userList"
                border
                height="320"
                :highlight-current-row="!multiple"
                @selection-change="handleSelectionChange"
                @row-click="handleRowClick"
              >
                <el-table-column
                  v-if="multiple"
                  type="selection"
                  width="48"
                  align="center"
                />
                <el-table-column prop="fullName" label="姓名" min-width="120" />
                <el-table-column
                  prop="loginAccount"
                  label="账号"
                  min-width="120"
                />
                <el-table-column prop="org" label="部门" min-width="120" />
                <el-table-column prop="mobile" label="手机" min-width="120" />
              </el-table>
            </el-card>
          </el-col>
        </el-row>

        <template #footer>
          <span class="dialog-footer">
            <el-button @click="dialogVisible = false">取 消</el-button>
            <el-button type="primary" @click="handleConfirm">确 定</el-button>
          </span>
        </template>
      </el-dialog>
    </div>
  </div>
</template>

<script setup lang="ts">
import { Search } from "@element-plus/icons-vue";
import { fetchOrgUnitList, fetchAdminList } from "@/api";
import { changeTree } from "@/utils/tools";

type AdminItem = Record<string, any>;

interface OrgTreeNode {
  id: string;
  label: string;
  parentId: string;
  children?: OrgTreeNode[];
  [key: string]: any;
}

const props = withDefaults(
  defineProps<{
    modelValue: AdminItem | AdminItem[] | null;
    /** 是否多选 */
    multiple?: boolean;
    /** 展示占位文本 */
    placeholder?: string;
    /** 外层宽度 */
    width?: string | number;
  }>(),
  {
    multiple: false,
    placeholder: "请选择用户",
  }
);

const emit = defineEmits<{
  (e: "update:modelValue", value: AdminItem | AdminItem[] | null): void;
  (e: "change", value: AdminItem | AdminItem[] | null): void;
}>();

const dialogVisible = ref(false);
const loadingOrg = ref(false);
const loadingUser = ref(false);

const orgTree = ref<OrgTreeNode[]>([]);
const currentOrgId = ref<string | null>(null);
const keyword = ref("");

const userList = ref<AdminItem[]>([]);
const multiSelection = ref<AdminItem[]>([]);
const singleSelection = ref<AdminItem | null>(null);

const widthStyle = computed(() => {
  if (props.width == null) return "100%";
  return typeof props.width === "number" ? `${props.width}px` : props.width;
});

// 当前已选值转换为统一数组，便于展示
const selectedList = computed<AdminItem[]>(() => {
  if (props.multiple) {
    return Array.isArray(props.modelValue)
      ? (props.modelValue as AdminItem[])
      : [];
  }
  if (props.modelValue && !Array.isArray(props.modelValue)) {
    return [props.modelValue as AdminItem];
  }
  return [];
});

// 选中用户显示的标题
const getUserLabel = (user: AdminItem) => {
  return (
    user.fullName ||
    user.displayName ||
    user.userName ||
    user.loginAccount ||
    user.mobile ||
    user.id
  );
};

const openDialog = () => {
  dialogVisible.value = true;
  // 初次打开时加载组织 & 用户
  if (!orgTree.value.length) {
    loadOrgTree();
  }
  // 同步已选数据
  if (props.multiple) {
    multiSelection.value = [...selectedList.value];
  } else {
    singleSelection.value = selectedList.value[0] || null;
  }
  loadUsers();
};

// 删除当前选中的某个用户
const removeSelected = (item: AdminItem) => {
  if (props.multiple) {
    const list = selectedList.value.filter((u) => u.id !== item.id);
    emit("update:modelValue", list);
    emit("change", list);
  } else {
    emit("update:modelValue", null);
    emit("change", null);
  }
};

// 清空所有选择
const clearSelected = () => {
  if (props.multiple) {
    emit("update:modelValue", []);
    emit("change", []);
  } else {
    emit("update:modelValue", null);
    emit("change", null);
  }
};

// 加载组织机构树
const loadOrgTree = async () => {
  loadingOrg.value = true;
  try {
    let res = await fetchOrgUnitList({
      page: 1,
      limit: 1000,
    } as any);
    let _tree: OrgTreeNode[] = [
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
    orgTree.value = changeTree(_tree);
  } finally {
    loadingOrg.value = false;
  }
};

// 当前组织点击
const handleOrgClick = (node: OrgTreeNode) => {
  currentOrgId.value = node.id === "0" ? null : node.id;
  loadUsers();
};

// 加载用户列表
const loadUsers = async () => {
  loadingUser.value = true;
  try {
    const params: any = {};
    if (keyword.value) params.key = keyword.value;
    if (currentOrgId.value) params.orgId = currentOrgId.value;

    const res: any = await fetchAdminList(params);
    userList.value = Array.isArray(res) ? res : res?.items || [];
  } finally {
    loadingUser.value = false;
  }
};

// 多选表格选择变更
const handleSelectionChange = (rows: AdminItem[]) => {
  if (props.multiple) {
    multiSelection.value = rows;
  }
};

// 单选表格行点击
const handleRowClick = (row: AdminItem) => {
  if (!props.multiple) {
    singleSelection.value = row;
  }
};

// 确认选择
const handleConfirm = () => {
  if (props.multiple) {
    const value = [...multiSelection.value];
    emit("update:modelValue", value);
    emit("change", value);
  } else {
    const value = singleSelection.value || null;
    emit("update:modelValue", value);
    emit("change", value);
  }
  dialogVisible.value = false;
};
</script>

<style scoped>
.soa-select-admin {
  display: inline-block;
}
</style>
