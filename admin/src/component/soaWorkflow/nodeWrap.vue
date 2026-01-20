<template>
  <Promoter v-if="nodeConfig?.type === 0" v-model="nodeConfig" :readonly="readonlyValue" />
  <Approver v-else-if="nodeConfig?.type === 1" v-model="nodeConfig" :readonly="readonlyValue" />
  <SendNode v-else-if="nodeConfig?.type === 2" v-model="nodeConfig" :readonly="readonlyValue" />
  <BranchNode v-else-if="nodeConfig?.type === 4" v-model="nodeConfig" :readonly="readonlyValue">
    <template #default="{ node }">
      <NodeWrap v-if="node" v-model="node.childNode" :readonly="readonlyValue" />
    </template>
  </BranchNode>
  <NodeWrap v-if="nodeConfig?.childNode" v-model="nodeConfig.childNode" :readonly="readonlyValue" />
</template>

<script setup lang="ts">
import type { WorkflowNode } from "./types";
import Approver from "./nodes/approver.vue";
import Promoter from "./nodes/promoter.vue";
import BranchNode from "./nodes/branch.vue";
import SendNode from "./nodes/send.vue";

const readonlyInject = inject<boolean | Ref<boolean>>("workflowReadonly", false);
const readonlyValue = computed(
  () => (readonlyInject && (readonlyInject as any).value !== undefined ? (readonlyInject as Ref<boolean>).value : !!readonlyInject)
);

defineOptions({ name: 'NodeWrap' });

const props = defineProps<{ modelValue: WorkflowNode | null }>();
const emit = defineEmits<{ (e: 'update:modelValue', value: WorkflowNode | null): void }>();

const nodeConfig = ref<WorkflowNode | null>(props.modelValue ?? null);

watch(
  () => props.modelValue,
  value => {
    nodeConfig.value = value ?? null;
  },
  { deep: true }
);

watch(
  nodeConfig,
  value => {
    emit('update:modelValue', value ?? null);
  },
  { deep: true }
);
</script>
