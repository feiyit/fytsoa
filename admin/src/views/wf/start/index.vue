<script setup lang="ts">
import SoaForm from "@/component/soaForm/index.vue";
import {
  getWorkflowDefinitionList,
  getWorkflowForm,
  createWorkflowInstance,
  type WorkflowDefinitionDto,
  type WorkflowFormDto,
} from "@/api";
import { saveWorkflowBusiness } from "@/api";
import { useUserStore } from "@/stores/user";

interface StartQueryState {
  tenantId: string | null;
  keyword: string;
  category: string;
}

const userStore = useUserStore();

const query = reactive<StartQueryState>({
  tenantId: "0",
  keyword: "",
  category: "",
});

const loadingDefs = ref(false);
const definitions = ref<WorkflowDefinitionDto[]>([]);
const selectedDefId = ref<string | null>(null);

const currentDefinition = computed(
  () => definitions.value.find((d) => d.id === selectedDefId.value) ?? null
);

const currentFormDef = ref<WorkflowFormDto | null>(null);
const parsedFormConfig = computed(() => {
  const json = currentFormDef.value?.schemaJson;
  if (!json) return null;
  try {
    const payload = JSON.parse(json);
    return payload.formConfig ?? null;
  } catch {
    console.warn("[wf/start] 解析表单配置失败");
    return null;
  }
});
const formLoading = ref(false);
const formModel = ref<Record<string, any>>({});
const formRef = ref<InstanceType<typeof SoaForm> | null>(null);

const loadDefinitions = async () => {
  if (query.tenantId == null) {
    ElMessage.warning("请先填写租户编号");
    return;
  }
  loadingDefs.value = true;
  try {
    const list = await getWorkflowDefinitionList(
      Number(query.tenantId) || 0,
      query.keyword || undefined,
      undefined
    );
    // 只展示“已发布”的流程（Status=1），如果需要可以去掉过滤
    definitions.value = list.filter((x) => x.status === 1);
    if (!definitions.value.length) {
      selectedDefId.value = null;
      currentFormDef.value = null;
      formModel.value = {};
    }
  } finally {
    loadingDefs.value = false;
  }
};

onMounted(() => {
  loadDefinitions();
});

const handleSelectDefinition = async (row: WorkflowDefinitionDto) => {
  selectedDefId.value = row.id ?? null;
  currentFormDef.value = null;
  formModel.value = {};

  if (!row.formSchemaId) {
    ElMessage.warning("当前流程未绑定表单，请先在流程定义中配置");
    return;
  }

  const formId = row.formSchemaId;
  if (Number.isNaN(formId)) {
    ElMessage.error("绑定表单标识不是有效的 Id");
    return;
  }

  formLoading.value = true;
  try {
    const formDef = await getWorkflowForm(formId);
    currentFormDef.value = formDef ?? null;
  } finally {
    formLoading.value = false;
  }
};

const handleStart = async () => {
  const def = currentDefinition.value;
  const formDef = currentFormDef.value;
  if (!def || !formDef) {
    ElMessage.warning("请选择流程并确保已加载表单");
    return;
  }

  const tenantId = Number(query.tenantId) || 0;
  const startUserId = userStore.userInfo?.id;

  // 1. 先保存业务数据，得到 businessKey
  const businessKey = await saveWorkflowBusiness({
    tenantId,
    definitionId: def.id!,
    definitionKey: def.defKey!,
    formData: formModel.value,
    createdBy: startUserId,
  });

  await createWorkflowInstance({
    tenantId,
    definitionId: def.id!,
    definitionKey: def.defKey!,
    businessKey,
    title:
      (def.defName || "流程实例") +
      (formModel.value?.title ? `-${formModel.value.title}` : ""),
    startUserId,
    startUserName: userStore.userInfo?.username || "",
    status: 0,
  });

  ElMessage.success("流程已发起");
};

const handleSubmitClick = () => {
  if (!formRef.value) return;
  formRef.value.validate((valid) => {
    if (valid) {
      handleStart();
    }
  });
};
</script>

<template>
  <el-container class="h-full">
    <el-aside width="280px" class="h-full pr-2">
      <el-card
        class="h-full bg-card border-border rounded-[.5vw] dark:border-slate-750"
        body-class="flex flex-col h-full "
        shadow="never"
      >
        <template #header> 选择流程定义 </template>
        <el-form :inline="false" label-width="68px" class="mb-2">
          <el-form-item label="关键字">
            <el-input
              v-model="query.keyword"
              placeholder="编码 / 名称"
              clearable
              style="width: 180px"
              @keyup.enter="loadDefinitions"
            />
          </el-form-item>
          <el-form-item>
            <el-button type="primary" @click="loadDefinitions">
              查询
            </el-button>
          </el-form-item>
        </el-form>
        <el-scrollbar class="flex-1 mt-1">
          <el-skeleton :loading="loadingDefs" animated :rows="6">
            <el-empty
              v-if="!definitions.length"
              description="暂无可发起的流程"
              :image-size="80"
            />
            <el-menu
              v-else
              :default-active="String(selectedDefId ?? '')"
              class="w-full menu-flow"
            >
              <el-menu-item
                v-for="item in definitions"
                :key="item.id"
                :index="String(item.id)"
                @click="handleSelectDefinition(item)"
              >
                <div class="flex flex-col relative z-10">
                  <span class="text-sm font-medium pl-2 pt-2">
                    {{ item.defName }}
                  </span>
                  <span class="text-[11px] text-gray-500 pl-2 pb-2">
                    {{ item.defKey }}
                  </span>
                </div>
              </el-menu-item>
            </el-menu>
          </el-skeleton>
        </el-scrollbar>
      </el-card>
    </el-aside>

    <el-main class="bg-card border-border rounded-[.5vw] dark:border-slate-750">
      <el-card shadow="never" class="h-full" body-class="h-full flex flex-col">
        <template #header>
          <div class="flex items-center justify-between">
            <div class="space-y-1">
              <div class="text-base font-medium">
                {{
                  currentDefinition?.defName
                    ? `发起：${currentDefinition.defName}`
                    : "请选择要发起的流程"
                }}
              </div>
              <div
                v-if="currentDefinition"
                class="text-xs text-gray-500 dark:text-gray-400"
              >
                编码：{{ currentDefinition.defKey }}， 版本：{{
                  currentDefinition.version
                }}， 绑定表单：
                {{ currentFormDef?.name || "未绑定" }}
              </div>
            </div>
            <el-button
              type="primary"
              :disabled="!currentDefinition || !currentFormDef"
              @click="handleSubmitClick"
            >
              提交并发起流程
            </el-button>
          </div>
        </template>

        <div class="flex-1 overflow-auto p-2">
          <el-skeleton v-if="!currentDefinition" animated :rows="4" />
          <template v-else>
            <el-alert
              v-if="!currentFormDef"
              type="warning"
              show-icon
              title="当前流程尚未绑定表单或表单未加载"
              class="mb-3"
            />
            <SoaForm
              v-else-if="parsedFormConfig"
              ref="formRef"
              v-model="formModel"
              :config="parsedFormConfig"
            >
              <el-button type="primary" @click="handleSubmitClick">
                提交并发起流程
              </el-button>
              <el-button @click="formRef?.resetFields()">重置</el-button>
            </SoaForm>
          </template>
        </div>
      </el-card>
    </el-main>
  </el-container>
</template>
<style scoped>
:deep(.menu-flow .el-menu-item:after) {
  left: 0px;
  height: 100%;
  width: 100%;
}
</style>
