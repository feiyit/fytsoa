<template>
  <div v-if="nodeConfig" class="node-wrap">
    <div class="node-wrap-box start-node" :class="statusClass" @click="show">
      <div class="title" style="background: #576a95">
        <el-icon class="icon"><UserFilled /></el-icon>
        <span>{{ nodeConfig.nodeName }}</span>
      </div>
      <div class="content">
        <span>{{ toText(nodeConfig) }}</span>
      </div>
    </div>
    <AddNode v-model="nodeConfig.childNode" :readonly="readonly" />
    <el-drawer
      v-model="drawer"
      title="发起人"
      destroy-on-close
      append-to-body
      :size="500"
    >
      <template #header>
        <div class="node-wrap-drawer__title">
          <label v-if="!isEditTitle" @click="editTitle">
            {{ form.nodeName }}
            <el-icon class="node-wrap-drawer__title-edit"><Edit /></el-icon>
          </label>
          <el-input
            v-else
            ref="nodeTitle"
            v-model="form.nodeName"
            clearable
            @blur="saveTitle"
            @keyup.enter="saveTitle"
          />
        </div>
      </template>
      <el-container>
        <el-main style="padding: 0 20px 20px 20px">
          <el-form label-position="top">
            <el-form-item label="谁可以发起此审批">
              <el-button
                type="primary"
                :icon="Plus"
                round
                @click="selectHandle(2, form.nodeRoleList)"
              >
                选择角色
              </el-button>
              <div class="tags-list">
                <el-tag
                  v-for="(role, index) in form.nodeRoleList"
                  :key="role.id"
                  type="info"
                  closable
                  @close="delRole(index)"
                >
                  {{ role.name }}
                </el-tag>
              </div>
            </el-form-item>
            <el-alert
              v-if="form.nodeRoleList.length === 0"
              title="不指定则默认所有人都可发起此审批"
              type="info"
              :closable="false"
            />
          </el-form>
        </el-main>
        <el-footer>
          <el-button type="primary" @click="save">保存</el-button>
          <el-button @click="drawer = false">取消</el-button>
        </el-footer>
      </el-container>
    </el-drawer>
  </div>
</template>

<script setup lang="ts">
import { inject, nextTick, reactive, ref, watch, computed } from "vue";
import { Edit, Plus, UserFilled } from "@element-plus/icons-vue";
import AddNode from "./addNode.vue";
import type {
  PromoterNode,
  SelectValueItem,
  WorkflowNode,
  WorkflowSelectType,
} from "../types";

const props = defineProps<{
  modelValue: PromoterNode | null;
  readonly?: boolean;
}>();
const emit = defineEmits<{
  (e: "update:modelValue", value: WorkflowNode | null): void;
}>();

const select =
  inject<(type: WorkflowSelectType, data: SelectValueItem[]) => void>("select");

const nodeConfig = ref<PromoterNode | null>(props.modelValue ?? null);
const drawer = ref(false);
const isEditTitle = ref(false);
const nodeTitle = ref<{ focus: () => void } | null>(null);

const createDefaultForm = (): PromoterNode => ({
  nodeName: "发起人",
  type: 0,
  nodeRoleList: [],
  childNode: null,
});

const form = reactive<PromoterNode>(
  nodeConfig.value ? deepClone(nodeConfig.value) : createDefaultForm()
);

const statusClass = computed(() => {
  const status = (nodeConfig.value as any)?._status as string | undefined;
  return status ? `node-status-${status}` : "";
});

watch(
  () => props.modelValue,
  (value) => {
    nodeConfig.value = value ?? null;
  },
  { deep: true }
);

watch(
  nodeConfig,
  (value) => {
    emit("update:modelValue", value ?? null);
    if (value) {
      Object.assign(form, deepClone(value));
    }
  },
  { deep: true }
);

const show = () => {
  if (!nodeConfig.value) {
    Object.assign(form, createDefaultForm());
  } else {
    Object.assign(form, deepClone(nodeConfig.value));
  }
  isEditTitle.value = false;
  drawer.value = true;
};

const editTitle = () => {
  isEditTitle.value = true;
  nextTick(() => {
    nodeTitle.value?.focus();
  });
};

const saveTitle = () => {
  isEditTitle.value = false;
};

const selectHandle = (type: WorkflowSelectType, data: SelectValueItem[]) => {
  select?.(type, data);
};

const delRole = (index: number) => {
  form.nodeRoleList.splice(index, 1);
};

const save = () => {
  const payload = deepClone(form);
  emit("update:modelValue", payload as WorkflowNode);
  drawer.value = false;
};

const toText = (node: PromoterNode) => {
  if (node.nodeRoleList && node.nodeRoleList.length > 0) {
    return node.nodeRoleList.map((item) => item.name).join("、");
  }
  return "所有人";
};

function deepClone<T>(value: T): T {
  return JSON.parse(JSON.stringify(value));
}
</script>
