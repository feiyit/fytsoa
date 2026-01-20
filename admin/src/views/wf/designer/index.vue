<script setup lang="ts">
import SoaWorkflow from "@/component/soaWorkflow/index.vue";
import type { WorkflowNode } from "@/component/soaWorkflow/types";
import {
  getWorkflowDefinition,
  getLatestWorkflowDefinitionModel,
  saveWorkflowDefinitionModel,
  type WorkflowDefinitionDto,
  type WorkflowDefinitionModelDto,
} from "@/api";

const route = useRoute();
const router = useRouter();

const tenantId = ref<string>("0");
const definitionId = ref<string | null>(null);

const definition = ref<WorkflowDefinitionDto | null>(null);
const latestModel = ref<WorkflowDefinitionModelDto | null>(null);
const loading = ref(false);

// 流程配置对象，双向绑定到 soaWorkflow
const workflowValue = ref<WorkflowNode | null>(null);

const pageTitle = computed(() => {
  if (!definition.value) return "流程设计器";
  const key = definition.value.defKey || "";
  const name = definition.value.defName || "";
  return key && name ? `${name}（${key}）` : name || key || "流程设计器";
});

const createInitialWorkflow = (): WorkflowNode => ({
  nodeName: "发起人",
  type: 0,
  nodeRoleList: [],
});

const workflowJson = computed(() =>
  workflowValue.value ? JSON.stringify(workflowValue.value, null, 2) : "null",
);

const initFromRoute = () => {
  const q = route.query;
  tenantId.value = q.tenantId ? q.tenantId || "0" : "0";
  definitionId.value = q.definitionId ? q.definitionId || null : null;
};

const loadData = async () => {
  if (!definitionId.value) {
    ElMessage.warning("缺少 definitionId 参数，无法加载流程配置");
    workflowValue.value = createInitialWorkflow();
    return;
  }
  loading.value = true;
  try {
    const [def, model] = await Promise.all([
      getWorkflowDefinition(definitionId.value),
      getLatestWorkflowDefinitionModel({
        tenantId: tenantId.value,
        definitionId: definitionId.value,
      }),
    ]);
    console.log("def", def);
    console.log("model", model);
    definition.value = def ?? null;
    latestModel.value = model ?? null;

    if (model?.modelJson) {
      try {
        workflowValue.value = JSON.parse(model.modelJson) as WorkflowNode;
      } catch {
        workflowValue.value = createInitialWorkflow();
      }
    } else {
      workflowValue.value = createInitialWorkflow();
    }
  } finally {
    loading.value = false;
  }
};

onMounted(() => {
  initFromRoute();
  loadData();
});

watch(
  () => route.query,
  () => {
    initFromRoute();
    loadData();
  },
);

const handleSave = async () => {
  if (!definitionId.value) {
    ElMessage.warning("缺少流程定义 Id，无法保存");
    return;
  }

  const payload: WorkflowDefinitionModelDto = {
    id: latestModel.value?.id,
    tenantId: tenantId.value,
    definitionId: definitionId.value,
    modelJson: workflowValue.value ? JSON.stringify(workflowValue.value) : "{}",
    isLatest: true,
  };

  await saveWorkflowDefinitionModel(payload);
  ElMessage.success("流程模型已保存");
};

const handleBack = () => {
  // 默认返回到流程定义列表
  router.push("/wf/definition");
};
</script>

<template>
  <el-container class="h-full space-y-4">
    <el-card shadow="hover" class="w-full">
      <template #header>
        <div class="flex items-center justify-between">
          <div class="flex items-center gap-2">
            <el-button link type="primary" @click="handleBack">
              <el-icon class="mr-1"><ArrowLeft /></el-icon>
              返回定义列表
            </el-button>
            <span class="text-base font-medium">{{ pageTitle }}</span>
            <span class="text-xs text-gray-500">
              定义 Id：{{ definitionId ?? "-" }}
            </span>
          </div>
          <div class="flex items-center gap-2">
            <el-button type="primary" @click="handleSave">
              保存流程配置
            </el-button>
          </div>
        </div>
      </template>

      <el-row :gutter="16">
        <el-col :lg="16" :md="24">
          <el-card shadow="never">
            <template #header> 工作流设计器 </template>
            <div v-loading="loading">
              <el-scrollbar height="70vh">
                <SoaWorkflow v-model="workflowValue" />
              </el-scrollbar>
            </div>
          </el-card>
        </el-col>

        <el-col :lg="8" :md="24">
          <el-card shadow="never">
            <template #header> 流程 JSON </template>
            <div class="mb-2 text-xs text-gray-500 dark:text-gray-400">
              当前流程配置数据（调试用，可根据需要隐藏）：
            </div>
            <el-scrollbar height="420px">
              <pre class="workflow-json">{{ workflowJson }}</pre>
            </el-scrollbar>
          </el-card>
        </el-col>
      </el-row>
    </el-card>
  </el-container>
</template>

<style scoped>
.workflow-json {
  margin: 0;
  white-space: pre-wrap;
  word-break: break-word;
  background-color: #0f172a;
  color: #e2e8f0;
  padding: 12px;
  border-radius: 6px;
  font-size: 12px;
}
</style>
