<template>
  <div class="add-node-btn-box">
    <div class="add-node-btn">
      <el-popover
        placement="right-start"
        :width="270"
        trigger="click"
        :hide-after="0"
        :show-after="0"
      >
        <template #reference>
          <el-button type="primary" v-if="!readonly" :icon="Plus" circle />
        </template>
        <div class="add-node-popover-body">
          <ul>
            <li>
              <el-icon style="color: #ff943e" @click="addType(1)"
                ><UserFilled
              /></el-icon>
              <p>审批节点</p>
            </li>
            <li>
              <el-icon style="color: #3296fa" @click="addType(2)"
                ><Promotion
              /></el-icon>
              <p>抄送节点</p>
            </li>
            <li>
              <el-icon style="color: #15bc83" @click="addType(4)"
                ><Share
              /></el-icon>
              <p>条件分支</p>
            </li>
          </ul>
        </div>
      </el-popover>
    </div>
  </div>
</template>

<script setup lang="ts">
import { Plus, Promotion, Share, UserFilled } from "@element-plus/icons-vue";
import type { WorkflowNode } from "../types";

const props = defineProps<{
  modelValue: WorkflowNode | null;
  readonly?: boolean;
}>();
const emit = defineEmits<{
  (e: "update:modelValue", value: WorkflowNode | null): void;
}>();

const addType = (type: number) => {
  const nextChild = props.modelValue ?? null;
  let node: WorkflowNode | null = null;

  if (type === 1) {
    node = {
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
      childNode: nextChild,
    } as WorkflowNode;
  } else if (type === 2) {
    node = {
      nodeName: "抄送人",
      type: 2,
      userSelectFlag: true,
      nodeUserList: [],
      childNode: nextChild,
    } as WorkflowNode;
  } else if (type === 4) {
    node = {
      nodeName: "条件路由",
      type: 4,
      conditionNodes: [
        {
          nodeName: "条件1",
          type: 3,
          priorityLevel: 1,
          conditionMode: 1,
          conditionList: [],
        },
        {
          nodeName: "条件2",
          type: 3,
          priorityLevel: 2,
          conditionMode: 1,
          conditionList: [],
        },
      ],
      childNode: nextChild,
    } as WorkflowNode;
  }

  if (node) {
    emit("update:modelValue", node);
  }
};
</script>
