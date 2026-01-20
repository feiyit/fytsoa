<template>
  <div v-if="nodeConfig" class="branch-wrap">
    <div class="branch-box-wrap">
      <div class="branch-box">
        <el-button
          class="add-branch"
          type="success"
          plain
          round
          @click="addTerm"
          >{{ readonly ? "条件" : "添加条件" }}</el-button
        >
        <div
          v-for="(item, index) in nodeConfig.conditionNodes"
          :key="index"
          class="col-box"
        >
          <div class="condition-node">
            <div class="condition-node-box">
              <div
                class="auto-judge"
                :class="getStatusClass(item)"
                @click="show(index)"
              >
                <div
                  v-if="!readonly && index !== 0"
                  class="sort-left"
                  @click.stop="arrTransfer(index, -1)"
                >
                  <el-icon><ArrowLeft /></el-icon>
                </div>
                <div class="title">
                  <span class="node-title">{{ item.nodeName }}</span>
                  <span class="priority-title"
                    >优先级{{ item.priorityLevel }}</span
                  >
                  <el-icon
                    v-if="!readonly"
                    class="close"
                    @click.stop="delTerm(index)"
                    ><Close
                  /></el-icon>
                </div>
                <div class="content">
                  <span v-if="toText(nodeConfig, index)">{{
                    toText(nodeConfig, index)
                  }}</span>
                  <span v-else class="placeholder">请设置条件</span>
                </div>
                <div
                  v-if="
                    !readonly && index !== nodeConfig.conditionNodes.length - 1
                  "
                  class="sort-right"
                  @click.stop="arrTransfer(index, 1)"
                >
                  <el-icon><ArrowRight /></el-icon>
                </div>
              </div>
              <AddNode v-model="item.childNode" :readonly="readonly" />
            </div>
          </div>
          <slot v-if="item.childNode" :node="item" />
          <div v-if="index === 0" class="top-left-cover-line" />
          <div v-if="index === 0" class="bottom-left-cover-line" />
          <div
            v-if="index === nodeConfig.conditionNodes.length - 1"
            class="top-right-cover-line"
          />
          <div
            v-if="index === nodeConfig.conditionNodes.length - 1"
            class="bottom-right-cover-line"
          />
        </div>
      </div>
    </div>
    <AddNode
      v-if="!nodeConfig || !readonly"
      v-model="nodeConfig.childNode"
      :readonly="readonly"
    />
    <el-drawer
      v-model="drawer"
      title="条件设置"
      destroy-on-close
      append-to-body
      :size="600"
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
            <el-form-item label="条件关系">
              <el-radio-group v-model="form.conditionMode">
                <el-radio :label="1">且</el-radio>
                <el-radio :label="2">或</el-radio>
              </el-radio-group>
            </el-form-item>
            <el-divider />
            <el-form-item>
              <el-table :data="form.conditionList">
                <el-table-column prop="label" label="描述">
                  <template #default="scope">
                    <el-input v-model="scope.row.label" placeholder="描述" />
                  </template>
                </el-table-column>
                <el-table-column prop="field" label="条件字段" width="130">
                  <template #default="scope">
                    <el-input
                      v-model="scope.row.field"
                      placeholder="条件字段"
                    />
                  </template>
                </el-table-column>
                <el-table-column prop="operator" label="运算符" width="130">
                  <template #default="scope">
                    <el-select
                      v-model="scope.row.operator"
                      placeholder="Select"
                    >
                      <el-option label="等于" value="=" />
                      <el-option label="不等于" value="!=" />
                      <el-option label="大于" value=">" />
                      <el-option label="大于等于" value=">=" />
                      <el-option label="小于" value="<" />
                      <el-option label="小于等于" value="<=" />
                      <el-option label="包含" value="include" />
                      <el-option label="不包含" value="notinclude" />
                    </el-select>
                  </template>
                </el-table-column>
                <el-table-column prop="value" label="值" width="100">
                  <template #default="scope">
                    <el-input v-model="scope.row.value" placeholder="值" />
                  </template>
                </el-table-column>
                <el-table-column prop="value" label="移除" width="55">
                  <template #default="scope">
                    <el-link
                      type="danger"
                      :underline="false"
                      @click="deleteConditionList(scope.$index)"
                      >移除</el-link
                    >
                  </template>
                </el-table-column>
              </el-table>
            </el-form-item>
            <p>
              <el-button
                type="primary"
                :icon="Plus"
                round
                @click="addConditionList"
                >增加条件</el-button
              >
            </p>
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
import { nextTick, reactive, ref, watch } from "vue";
import {
  ArrowLeft,
  ArrowRight,
  Close,
  Edit,
  Plus,
} from "@element-plus/icons-vue";
import AddNode from "./addNode.vue";
import type { BranchNode, ConditionItem, WorkflowNode } from "../types";

const props = defineProps<{
  modelValue: BranchNode | null;
  readonly?: boolean;
}>();
const emit = defineEmits<{
  (e: "update:modelValue", value: WorkflowNode | null): void;
}>();

const nodeConfig = ref<BranchNode | null>(props.modelValue ?? null);
const drawer = ref(false);
const isEditTitle = ref(false);
const nodeTitle = ref<{ focus: () => void } | null>(null);
const currentIndex = ref(0);

const createDefaultCondition = (): ConditionItem => ({
  nodeName: "条件",
  type: 3,
  priorityLevel: 1,
  conditionMode: 1,
  conditionList: [],
});

const form = reactive<ConditionItem>({ ...createDefaultCondition() });

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
  },
  { deep: true }
);

const show = (index: number) => {
  if (!nodeConfig.value) return;
  currentIndex.value = index;
  Object.assign(form, deepClone(nodeConfig.value.conditionNodes[index]));
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
  if (!nodeConfig.value) return;
  nodeConfig.value.conditionNodes[currentIndex.value] = deepClone(form);
  emit("update:modelValue", deepClone(nodeConfig.value) as WorkflowNode);
  drawer.value = false;
};

const addTerm = () => {
  if (!nodeConfig.value) return;
  const len = nodeConfig.value.conditionNodes.length + 1;
  nodeConfig.value.conditionNodes.push({
    nodeName: `条件${len}`,
    type: 3,
    priorityLevel: len,
    conditionMode: 1,
    conditionList: [],
  });
};

const delTerm = (index: number) => {
  if (!nodeConfig.value) return;
  nodeConfig.value.conditionNodes.splice(index, 1);
  if (nodeConfig.value.conditionNodes.length === 1) {
    const onlyNode = nodeConfig.value.conditionNodes[0];
    if (nodeConfig.value.childNode) {
      if (onlyNode.childNode) {
        reData(onlyNode.childNode, nodeConfig.value.childNode);
      } else {
        onlyNode.childNode = nodeConfig.value.childNode;
      }
    }
    emit("update:modelValue", onlyNode.childNode ?? null);
  } else {
    nodeConfig.value.conditionNodes.forEach((item, idx) => {
      item.priorityLevel = idx + 1;
    });
    emit("update:modelValue", deepClone(nodeConfig.value) as WorkflowNode);
  }
};

const reData = (
  data: WorkflowNode | null | undefined,
  addData: WorkflowNode | null
) => {
  if (!data) return;
  if (!data.childNode) {
    data.childNode = addData ?? null;
  } else {
    reData(data.childNode, addData);
  }
};

const arrTransfer = (index: number, type = 1) => {
  if (!nodeConfig.value) return;
  const nodes = nodeConfig.value.conditionNodes;
  nodes[index] = nodes.splice(index + type, 1, nodes[index])[0];
  nodes.forEach((item, idx) => {
    item.priorityLevel = idx + 1;
  });
  emit("update:modelValue", deepClone(nodeConfig.value) as WorkflowNode);
};

const addConditionList = () => {
  form.conditionList.push({
    label: "",
    field: "",
    operator: "=",
    value: "",
  });
};

const deleteConditionList = (index: number) => {
  form.conditionList.splice(index, 1);
};

const toText = (node: BranchNode, index: number) => {
  const { conditionList } = node.conditionNodes[index];
  if (conditionList && conditionList.length === 1) {
    return conditionList
      .map((item) => `${item.label}${item.operator}${item.value}`)
      .join(" 和 ");
  }
  if (conditionList && conditionList.length > 1) {
    const conditionModeText =
      node.conditionNodes[index].conditionMode === 1 ? "且行" : "或行";
    return `${conditionList.length}个条件，${conditionModeText}`;
  }
  if (index === node.conditionNodes.length - 1) {
    return "其他条件进入此流程";
  }
  return "";
};

const getStatusClass = (item: ConditionItem) => {
  const status = (item as any)._status as string | undefined;
  return status ? `node-status-${status}` : "";
};

function deepClone<T>(value: T): T {
  return JSON.parse(JSON.stringify(value));
}
</script>
