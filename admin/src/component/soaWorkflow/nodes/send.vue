<template>
  <div v-if="nodeConfig" class="node-wrap">
    <div class="node-wrap-box" :class="statusClass" @click="show">
      <div class="title" style="background: #3296fa">
        <el-icon class="icon"><Promotion /></el-icon>
        <span>{{ nodeConfig.nodeName }}</span>
        <el-icon class="close" @click.stop="delNode"><Close /></el-icon>
      </div>
      <div class="content">
        <span v-if="toText(nodeConfig)">{{ toText(nodeConfig) }}</span>
        <span v-else class="placeholder">请选择人员</span>
      </div>
    </div>
    <AddNode v-model="nodeConfig.childNode" />
    <el-drawer v-model="drawer" title="抄送人设置" destroy-on-close append-to-body :size="500">
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
            <el-form-item label="选择要抄送的人员">
              <el-button type="primary" :icon="Plus" round @click="selectHandle(1, form.nodeUserList)">
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
            <el-form-item>
              <el-checkbox v-model="form.userSelectFlag" label="允许发起人自选抄送人" />
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
import { inject, nextTick, reactive, ref, watch, computed } from 'vue';
import { Close, Edit, Plus, Promotion } from '@element-plus/icons-vue';
import AddNode from './addNode.vue';
import type { SelectValueItem, SendNode, WorkflowNode, WorkflowSelectType } from '../types';

const props = defineProps<{ modelValue: SendNode | null }>();
const emit = defineEmits<{ (e: 'update:modelValue', value: WorkflowNode | null): void }>();

const select = inject<(type: WorkflowSelectType, data: SelectValueItem[]) => void>('select');

const nodeConfig = ref<SendNode | null>(props.modelValue ?? null);
const drawer = ref(false);
const isEditTitle = ref(false);
const nodeTitle = ref<{ focus: () => void } | null>(null);

const createDefaultForm = (): SendNode => ({
  nodeName: '抄送人',
  type: 2,
  userSelectFlag: true,
  nodeUserList: [],
  childNode: null,
});

const form = reactive<SendNode>(nodeConfig.value ? deepClone(nodeConfig.value) : createDefaultForm());

const statusClass = computed(() => {
  const status = (nodeConfig.value as any)?._status as string | undefined;
  return status ? `node-status-${status}` : '';
});

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
  emit('update:modelValue', payload as WorkflowNode);
  drawer.value = false;
};

const delNode = () => {
  emit('update:modelValue', nodeConfig.value?.childNode ?? null);
};

const delUser = (index: number) => {
  form.nodeUserList.splice(index, 1);
};

const selectHandle = (type: WorkflowSelectType, data: SelectValueItem[]) => {
  select?.(type, data);
};

const toText = (node: SendNode) => {
  if (node.nodeUserList && node.nodeUserList.length > 0) {
    return node.nodeUserList.map(item => item.name).join('、');
  }
  if (node.userSelectFlag) {
    return '发起人自选';
  }
  return '';
};

function deepClone<T>(value: T): T {
  return JSON.parse(JSON.stringify(value));
}
</script>
