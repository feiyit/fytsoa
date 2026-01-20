<script setup lang="ts">
import soaTable from "@/component/soaTable/index.vue";
import SoaForm from "@/component/soaForm/index.vue";
import SoaWorkflow from "@/component/soaWorkflow/index.vue";
import type { WorkflowNode } from "@/component/soaWorkflow/types";
import { useUserStore } from "@/stores/user";
import {
  getMyHandledWorkflowTaskHistory,
  getWorkflowTaskHistory,
  type WorkflowTaskHistoryDto,
} from "@/api";
import {
  getWorkflowInstance,
  getWorkflowInstanceHistory,
  getWorkflowDefinition,
  getWorkflowForm,
  getLatestWorkflowDefinitionModel,
  getWorkflowBusiness,
  type WorkflowInstanceDto,
  type WorkflowInstanceHistoryDto,
  type WorkflowFormDto,
  type WorkflowDefinitionModelDto,
  type WorkflowDefinitionDto,
} from "@/api";

interface MyHandledQueryState {
  tenantId: number | null;
  userId: number | null;
}

const userStore = useUserStore();

const query = reactive<MyHandledQueryState>({
  tenantId: 0,
  userId: (userStore.userInfo?.id as number | undefined) ?? null,
});

const tableRef = ref<InstanceType<typeof soaTable> | null>(null);

const columns = [
  {
    title: "实例 Id",
    dataIndex: "instanceId",
    key: "instanceId",
    width: 180,
    align: "center",
  },
  {
    title: "节点名称",
    dataIndex: "nodeName",
    key: "nodeName",
    minWidth: 180,
  },
  {
    title: "动作",
    dataIndex: "action",
    key: "action",
    width: 100,
  },
  {
    title: "审批意见",
    dataIndex: "comment",
    key: "comment",
    minWidth: 200,
  },
  {
    title: "开始时间",
    dataIndex: "createdAt",
    key: "createdAt",
    width: 180,
  },
  {
    title: "完成时间",
    dataIndex: "completedAt",
    key: "completedAt",
    width: 180,
  },
  {
    title: "操作",
    key: "actionCol",
    width: 160,
    fixed: "right",
  },
];

const loadMyHandled = () => {
  if (query.userId == null) {
    ElMessage.warning("当前用户信息缺失，无法加载列表");
    return;
  }
  tableRef.value?.upData(
    {
      tenantId: query.tenantId ?? 0,
      userId: query.userId,
    },
    1
  );
};

onMounted(() => {
  if (!query.userId && userStore.userInfo?.id) {
    query.userId = userStore.userInfo.id as number;
  }
  loadMyHandled();
});

// 明细：实例 + 历史（只读查看）
const detailVisible = ref(false);
const detailLoading = ref(false);
const currentInstance = ref<WorkflowInstanceDto | null>(null);
const instanceHistory = ref<WorkflowInstanceHistoryDto[]>([]);
const taskHistory = ref<WorkflowTaskHistoryDto[]>([]);

// 流程定义 / 表单 / 模型
const currentDefinition = ref<WorkflowDefinitionDto | null>(null);
const currentFormDef = ref<WorkflowFormDto | null>(null);
const latestModel = ref<WorkflowDefinitionModelDto | null>(null);
const workflowModel = ref<WorkflowNode | null>(null);
const workflowPreview = ref<WorkflowNode | null>(null);
const formModel = ref<Record<string, any>>({});
// 流程预览缩放比例
const workflowZoom = ref(1);

const parsedFormConfig = computed(() => {
  const json = currentFormDef.value?.schemaJson;
  if (!json) return null;
  try {
    const payload = JSON.parse(json);
    return payload.formConfig ?? null;
  } catch {
    console.warn("[wf/my-handled] 解析表单配置失败");
    return null;
  }
});

const statusText = (status?: number) => {
  switch (status) {
    case 0:
      return "运行中";
    case 1:
      return "已完成";
    case 2:
      return "已终止";
    case 3:
      return "已撤回";
    default:
      return "-";
  }
};

const openDetail = async (row: WorkflowTaskHistoryDto) => {
  if (!row.instanceId || query.tenantId == null) return;
  detailVisible.value = true;
  detailLoading.value = true;
  try {
    const tenantId = query.tenantId ?? 0;
    const [inst, instHis, taskHis] = await Promise.all([
      getWorkflowInstance(row.instanceId),
      getWorkflowInstanceHistory({
        tenantId,
        instanceId: row.instanceId,
      }),
      getWorkflowTaskHistory({
        tenantId,
        instanceId: row.instanceId,
      }),
    ]);
    currentInstance.value = inst ?? null;
    instanceHistory.value = instHis ?? [];
    taskHistory.value = taskHis ?? [];

    // 重置明细相关状态
    currentDefinition.value = null;
    currentFormDef.value = null;
    latestModel.value = null;
    workflowModel.value = null;
    workflowPreview.value = null;
    formModel.value = {};

    // 加载流程定义、最新流程模型 & 绑定表单定义
    if (inst?.definitionId) {
      const [def, model] = await Promise.all([
        getWorkflowDefinition(inst.definitionId),
        getLatestWorkflowDefinitionModel({
          tenantId,
          definitionId: inst.definitionId,
        }),
      ]);
      currentDefinition.value = def ?? null;
      latestModel.value = model ?? null;

      if (model?.modelJson) {
        try {
          workflowModel.value = JSON.parse(model.modelJson) as WorkflowNode;
        } catch {
          workflowModel.value = null;
          console.warn("[wf/my-handled] 解析流程模型 JSON 失败");
        }
      }

      const formKey = def?.formSchemaId;
      if (formKey) {
        const formDef = await getWorkflowForm(formKey);
        currentFormDef.value = formDef ?? null;
      }
    }

    // 根据业务主键从后端加载业务数据，填充表单模型
    if (inst?.definitionKey && inst.businessKey) {
      try {
        const biz = (await getWorkflowBusiness({
          tenantId,
          definitionKey: inst.definitionKey,
          businessKey: inst.businessKey,
        } as any)) as any;

        let payload: Record<string, any> | null = null;
        if (biz) {
          if (typeof biz.formDataJson === "string" && biz.formDataJson) {
            try {
              payload = JSON.parse(biz.formDataJson) ?? {};
            } catch (e) {
              console.warn("[wf/my-handled] 解析 formDataJson 失败", e);
            }
          }
          if (!payload && biz.formData) {
            if (typeof biz.formData === "string") {
              try {
                payload = JSON.parse(biz.formData) ?? {};
              } catch (e) {
                console.warn("[wf/my-handled] 解析 formData 失败", e);
              }
            } else if (typeof biz.formData === "object") {
              payload = biz.formData as Record<string, any>;
            }
          }
        }

        formModel.value = payload ?? {};
      } catch (err) {
        console.warn("[wf/my-handled] 加载业务表单数据失败", err);
        formModel.value = {};
      }
    }

    // 构建简单的流程预览
    if (workflowModel.value) {
      const doneNames = new Set(
        taskHistory.value
          .filter((x) => x.action || x.completedAt)
          .map((x) => x.nodeName)
          .filter(Boolean) as string[]
      );

      const markNode = (node: WorkflowNode | null): WorkflowNode | null => {
        if (!node) return null;
        const cloned: any = JSON.parse(JSON.stringify(node));
        const name: string | undefined = cloned.nodeName;

        if (name && doneNames.has(name)) {
          cloned._status = "done";
        } else {
          cloned._status = "future";
        }

        if (cloned.childNode) {
          cloned.childNode = markNode(cloned.childNode);
        }
        if (cloned.type === 4 && Array.isArray(cloned.conditionNodes)) {
          cloned.conditionNodes = cloned.conditionNodes.map((cond: any) =>
            markNode(cond)
          );
        }

        return cloned as WorkflowNode;
      };

      workflowPreview.value = markNode(workflowModel.value);
    }
  } finally {
    detailLoading.value = false;
  }
};
</script>

<template>
  <el-container class="h-full">
    <el-header
      class="bg-card border-border flex items-center justify-between gap-3 rounded-[.5vw]"
    >
      <div class="text-base font-medium">我的审批记录</div>
      <el-button type="primary" @click="loadMyHandled">刷新</el-button>
    </el-header>

    <el-main class="bg-card border-border mt-2 rounded-[.5vw]">
      <soaTable
        ref="tableRef"
        :columns="columns"
        :apiObj="getMyHandledWorkflowTaskHistory"
        :params="{ tenantId: query.tenantId ?? 0, userId: query.userId }"
        row-key="id"
        row-serial-number
        :showSelection="false"
      >
        <template #bodyCell="{ text, column, record }">
          <template v-if="column.key === 'actionCol'">
            <el-button
              type="primary"
              link
              size="small"
              @click="openDetail(record)"
            >
              查看流程
            </el-button>
          </template>
          <template v-else>
            {{ text }}
          </template>
        </template>
      </soaTable>
    </el-main>

    <el-drawer
      v-model="detailVisible"
      title="流程明细"
      size="60%"
      :destroy-on-close="true"
    >
      <div v-if="currentInstance" v-loading="detailLoading" class="space-y-4">
        <el-card shadow="never">
          <template #header> 实例信息 </template>
          <el-descriptions :column="2" size="small" border>
            <el-descriptions-item label="标题">
              {{ currentInstance.title }}
            </el-descriptions-item>
            <el-descriptions-item label="业务主键">
              {{ currentInstance.businessKey }}
            </el-descriptions-item>
            <el-descriptions-item label="流程编码">
              {{ currentInstance.definitionKey }}
            </el-descriptions-item>
            <el-descriptions-item label="状态">
              {{ statusText(currentInstance.status as number) }}
            </el-descriptions-item>
            <el-descriptions-item label="发起人">
              {{ currentInstance.startUserName }}
            </el-descriptions-item>
            <el-descriptions-item label="开始时间">
              {{ currentInstance.startTime }}
            </el-descriptions-item>
            <el-descriptions-item label="结束时间">
              {{ currentInstance.endTime || "-" }}
            </el-descriptions-item>
          </el-descriptions>
        </el-card>

        <el-card shadow="never">
          <template #header> 实例历史 </template>
          <el-table
            :data="instanceHistory"
            border
            stripe
            size="small"
            height="180"
          >
            <el-table-column
              prop="eventType"
              label="事件类型"
              width="120"
              show-overflow-tooltip
            />
            <el-table-column prop="operatorName" label="操作人" width="120" />
            <el-table-column
              prop="remark"
              label="备注"
              min-width="160"
              show-overflow-tooltip
            />
            <el-table-column
              prop="createdAt"
              label="时间"
              width="180"
              show-overflow-tooltip
            />
          </el-table>
        </el-card>

        <el-card shadow="never">
          <template #header> 任务历史 </template>
          <el-table :data="taskHistory" border stripe size="small" height="220">
            <el-table-column
              prop="nodeName"
              label="节点"
              min-width="140"
              show-overflow-tooltip
            />
            <el-table-column prop="assigneeName" label="处理人" width="120" />
            <el-table-column prop="action" label="动作" width="120" />
            <el-table-column
              prop="comment"
              label="意见"
              min-width="160"
              show-overflow-tooltip
            />
            <el-table-column
              prop="createdAt"
              label="开始时间"
              width="180"
              show-overflow-tooltip
            />
            <el-table-column
              prop="completedAt"
              label="完成时间"
              width="180"
              show-overflow-tooltip
            />
          </el-table>
        </el-card>
        <el-card v-if="currentFormDef" shadow="never">
          <template #header>
            业务表单（{{ currentFormDef.name || currentFormDef.code }}）
          </template>

          <SoaForm
            v-if="parsedFormConfig"
            v-model="formModel"
            :config="parsedFormConfig"
            readonly
          >
            <div class="mb-2 text-xs text-gray-500 dark:text-gray-400">
              展示的是该流程实例发起时填写的业务表单数据。
            </div>
          </SoaForm>
          <div
            v-else
            class="text-xs text-gray-400 dark:text-gray-500 mt-2 italic"
          >
            当前表单定义尚未配置 schemaJson。
          </div>
        </el-card>

        <el-card v-if="workflowPreview" shadow="never">
          <template #header>
            <div class="flex items-center justify-between">
              <div>整体流程</div>
              <div class="flex items-center gap-1 text-xs text-gray-500">
                <span>缩放：</span>
                <el-button-group>
                  <el-button
                    size="small"
                    text
                    @click="workflowZoom = Math.max(0.5, workflowZoom - 0.1)"
                    >-</el-button
                  >
                  <el-button size="small" text>
                    {{ Math.round(workflowZoom * 100) }}%
                  </el-button>
                  <el-button
                    size="small"
                    text
                    @click="workflowZoom = Math.min(2, workflowZoom + 0.1)"
                    >+</el-button
                  >
                </el-button-group>
              </div>
            </div>
          </template>
          <div class="workflow-preview-readonly">
            <el-scrollbar
              :wrap-style="{ overflowX: 'auto', overflowY: 'auto' }"
            >
              <SoaWorkflow
                v-model="workflowPreview"
                :scale="workflowZoom"
                :readonly="true"
              />
            </el-scrollbar>
          </div>
          <div class="mt-2 text-xs text-gray-500 dark:text-gray-400">
            已完成的节点会标记为“done”，其他节点为“future”，供查看参考。
          </div>
        </el-card>
      </div>
    </el-drawer>
  </el-container>
</template>

<style scoped>
.workflow-preview-readonly {
  padding: 8px 4px;
}

.workflow-preview-readonly :deep(.sc-workflow-design) {
  pointer-events: none;
}
</style>
