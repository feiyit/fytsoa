<template>
  <div v-if="nodeConfig" class="node-wrap">
    <div class="node-wrap-box" :class="statusClass" @click="show">
      <div class="title" style="background: #ff943e">
        <el-icon class="icon"><UserFilled /></el-icon>
        <span>{{ nodeConfig.nodeName }}</span>
        <el-icon v-if="!readonly" class="close" @click.stop="delNode"
          ><Close
        /></el-icon>
      </div>
      <div class="content">
        <span v-if="toText(nodeConfig)">{{ toText(nodeConfig) }}</span>
        <span v-else class="placeholder">请选择</span>
      </div>
    </div>
    <AddNode v-model="nodeConfig.childNode" :readonly="readonly" />
    <el-drawer
      v-model="drawer"
      title="审批人设置"
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
            <el-form-item label="审批人员类型">
              <el-select v-model="form.setType">
                <el-option :value="1" label="指定成员" />
                <el-option :value="2" label="主管" />
                <el-option :value="3" label="角色" />
                <!-- <el-option :value="4" label="发起人自选" /> -->
                <el-option :value="5" label="发起人自己" />
                <el-option :value="7" label="连续多级主管" />
              </el-select>
            </el-form-item>

            <el-form-item v-if="form.setType === 1" label="选择成员">
              <el-button
                type="primary"
                :icon="Plus"
                round
                @click="selectHandle(1, form.nodeUserList)"
              >
                选择人员
              </el-button>
              <div class="tags-list">
                <el-tag
                  v-for="(user, index) in form.nodeUserList"
                  :key="user.id"
                  closable
                  @close="delUser(index)"
                >
                  {{ user.name }}
                </el-tag>
              </div>
            </el-form-item>

            <el-form-item v-if="form.setType === 2" label="指定主管">
              发起人的第
              <el-input-number v-model="form.examineLevel" :min="1" />
              级主管
            </el-form-item>

            <el-form-item v-if="form.setType === 3" label="选择角色">
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

            <el-form-item v-if="form.setType === 4" label="发起人自选">
              <el-radio-group v-model="form.selectMode">
                <el-radio :label="1">自选一个人</el-radio>
                <el-radio :label="2">自选多个人</el-radio>
              </el-radio-group>
            </el-form-item>

            <el-form-item v-if="form.setType === 7" label="连续主管审批终点">
              <el-radio-group v-model="form.directorMode">
                <el-radio :label="0">直到最上层主管</el-radio>
                <el-radio :label="1">自定义审批终点</el-radio>
              </el-radio-group>
              <p v-if="form.directorMode === 1">
                直到发起人的第
                <el-input-number v-model="form.directorLevel" :min="1" /> 级主管
              </p>
            </el-form-item>

            <el-divider />
            <el-form-item>
              <el-checkbox v-model="form.termAuto" label="超时自动审批" />
            </el-form-item>
            <template v-if="form.termAuto">
              <el-form-item label="审批期限（为 0 则不生效）">
                <el-input-number v-model="form.term" :min="0" /> 小时
              </el-form-item>
              <el-form-item label="审批期限超时后执行">
                <el-radio-group v-model="form.termMode">
                  <el-radio :label="0">自动通过</el-radio>
                  <el-radio :label="1">自动拒绝</el-radio>
                </el-radio-group>
              </el-form-item>
            </template>
            <el-divider />
            <el-form-item label="多人审批时审批方式">
              <el-radio-group v-model="form.examineMode">
                <p style="width: 100%">
                  <el-radio :label="1">按顺序依次审批</el-radio>
                </p>
                <p style="width: 100%">
                  <el-radio :label="2"
                    >会签 (可同时审批，每个人必须审批通过)</el-radio
                  >
                </p>
                <p style="width: 100%">
                  <el-radio :label="3">或签 (有一人审批通过即可)</el-radio>
                </p>
              </el-radio-group>
            </el-form-item>
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
import { Close, Edit, Plus, UserFilled } from "@element-plus/icons-vue";
import AddNode from "./addNode.vue";
import type {
  ApproverNode,
  SelectValueItem,
  WorkflowNode,
  WorkflowSelectType,
} from "../types";

const props = defineProps<{
  modelValue: ApproverNode | null;
  readonly?: boolean;
}>();
const emit = defineEmits<{
  (e: "update:modelValue", value: WorkflowNode | null): void;
}>();

const select =
  inject<(type: WorkflowSelectType, data: SelectValueItem[]) => void>("select");

const nodeConfig = ref<ApproverNode | null>(props.modelValue ?? null);
const drawer = ref(false);
const isEditTitle = ref(false);
const nodeTitle = ref<{ focus: () => void } | null>(null);

const createDefaultForm = (): ApproverNode => ({
  nodeName: "审核人",
  type: 1,
  setType: 1,
  nodeUserList: [],
  nodeRoleList: [],
  examineLevel: 1,
  directorLevel: 1,
  selectMode: 1,
  termAuto: false,
  term: 0,
  termMode: 1,
  examineMode: 1,
  directorMode: 0,
  childNode: null,
});

const form = reactive<ApproverNode>(
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

const save = () => {
  const payload = deepClone(form);
  emit("update:modelValue", payload as WorkflowNode);
  drawer.value = false;
};

const delNode = () => {
  emit("update:modelValue", nodeConfig.value?.childNode ?? null);
};

const delUser = (index: number) => {
  form.nodeUserList.splice(index, 1);
};

const delRole = (index: number) => {
  form.nodeRoleList.splice(index, 1);
};

const selectHandle = (type: WorkflowSelectType, data: SelectValueItem[]) => {
  select?.(type, data);
};

const toText = (node: ApproverNode | null) => {
  if (!node) return "";
  if (node.setType === 1) {
    if (node.nodeUserList && node.nodeUserList.length > 0) {
      return node.nodeUserList.map((item) => item.name).join("、");
    }
    return "";
  }
  if (node.setType === 2) {
    return node.examineLevel === 1
      ? "直接主管"
      : `发起人的第${node.examineLevel}级主管`;
  }
  if (node.setType === 3) {
    if (node.nodeRoleList && node.nodeRoleList.length > 0) {
      const roles = node.nodeRoleList.map((item) => item.name).join("、");
      return `角色-${roles}`;
    }
    return "";
  }
  if (node.setType === 4) {
    return "发起人自选";
  }
  if (node.setType === 5) {
    return "发起人自己";
  }
  if (node.setType === 7) {
    return "连续多级主管";
  }
  return "";
};

function deepClone<T>(value: T): T {
  return JSON.parse(JSON.stringify(value));
}
</script>
