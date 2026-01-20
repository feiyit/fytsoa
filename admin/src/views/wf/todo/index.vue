<script setup lang="ts">
import soaTable from "@/component/soaTable/index.vue";
import SoaForm from "@/component/soaForm/index.vue";
import soaSelectAdmin from "@/component/soaSelectAdmin/index.vue";
import SoaWorkflow from "@/component/soaWorkflow/index.vue";
import type { WorkflowNode } from "@/component/soaWorkflow/types";
import {
  getWorkflowTodoTasks,
  getWorkflowInstance,
  getWorkflowInstanceHistory,
  getWorkflowTaskHistory,
  getWorkflowDefinition,
  getWorkflowForm,
  getLatestWorkflowDefinitionModel,
  type WorkflowTaskDto,
  type WorkflowInstanceDto,
  type WorkflowInstanceHistoryDto,
  type WorkflowTaskHistoryDto,
  type WorkflowFormDto,
  type WorkflowDefinitionModelDto,
} from "@/api";
import { getWorkflowBusiness, handleWorkflowTask } from "@/api";
import { useUserStore } from "@/stores/user";

interface TodoQueryState {
  tenantId: number | null;
  userId: number | null;
}

const userStore = useUserStore();

const query = reactive<TodoQueryState>({
  tenantId: 0,
  userId: (userStore.userInfo?.id as number | undefined) ?? null,
});

// 当前选中的用户（使用 soaSelectAdmin 组件选择）
const selectedAdmin = ref<Record<string, any> | null>(null);

const tableRef = ref<InstanceType<typeof soaTable> | null>(null);

const columns = [
  {
    title: "实例 Id",
    dataIndex: "instanceId",
    key: "instanceId",
    width: 190,
    align: "center",
  },
  {
    title: "节点名称",
    dataIndex: "nodeName",
    key: "nodeName",
    minWidth: 160,
  },
  {
    title: "当前处理人",
    dataIndex: "assigneeName",
    key: "assigneeName",
    minWidth: 120,
  },
  {
    title: "创建时间",
    dataIndex: "createdAt",
    key: "createdAt",
    width: 180,
  },
  {
    title: "操作",
    key: "action",
    width: 160,
    fixed: "right",
  },
];

const loadTodoTasks = () => {
  if (query.userId == null) {
    ElMessage.warning("请选择待办用户");
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
  // 默认选中当前登录用户
  if (!query.userId && userStore.userInfo?.id) {
    const id = userStore.userInfo.id as number;
    query.userId = id;
    selectedAdmin.value = {
      id,
      fullName: userStore.userInfo.username,
    };
  }
  loadTodoTasks();
});

// 用户选择变更时，更新查询条件并自动刷新列表
watch(
  () => selectedAdmin.value,
  (val) => {
    query.userId = val?.id ?? null;
    if (val?.id) {
      loadTodoTasks();
    }
  }
);

// 明细：实例 + 历史
const detailVisible = ref(false);
const detailLoading = ref(false);
const currentTask = ref<WorkflowTaskDto | null>(null);
const currentInstance = ref<WorkflowInstanceDto | null>(null);
const instanceHistory = ref<WorkflowInstanceHistoryDto[]>([]);
const taskHistory = ref<WorkflowTaskHistoryDto[]>([]);
const currentDefinition = ref<any | null>(null);
const currentFormDef = ref<WorkflowFormDto | null>(null);
const latestModel = ref<WorkflowDefinitionModelDto | null>(null);
const workflowModel = ref<WorkflowNode | null>(null);
const workflowPreview = ref<WorkflowNode | null>(null);
// 流程预览缩放比例（用于整体流程放大/缩小查看）
const workflowZoom = ref(1);
const formModel = ref<Record<string, any>>({});
const parsedFormConfig = computed(() => {
  const json = currentFormDef.value?.schemaJson;
  if (!json) return null;
  try {
    const payload = JSON.parse(json);
    return payload.formConfig ?? null;
  } catch {
    console.warn("[wf/todo] 解析表单配置失败");
    return null;
  }
});

// 审批操作状态
const approveAction = ref<"Agree" | "Reject">("Agree");
const approveComment = ref("");
const approveLoading = ref(false);

// 驳回目标设置：发起人 / 上一节点 / 指定节点
const rejectMode = ref<"Start" | "Previous" | "Specific">("Previous");
const rejectTargetNodeId = ref<string | null>(null);

// 可供选择的节点列表（来自流程配置，仅包含审批节点）
const rejectNodeOptions = computed(() => {
  const options: { label: string; value: string }[] = [];
  const visit = (node: WorkflowNode | null) => {
    if (!node) return;
    const anyNode: any = node;
    if (anyNode.type === 1 && typeof anyNode.nodeName === "string") {
      options.push({
        label: anyNode.nodeName,
        value: anyNode.nodeName,
      });
    }
    if (anyNode.type === 4 && Array.isArray(anyNode.conditionNodes)) {
      anyNode.conditionNodes.forEach((cond: any) => {
        if (cond.childNode) visit(cond.childNode);
      });
      if (anyNode.childNode) visit(anyNode.childNode);
    } else if (anyNode.childNode) {
      visit(anyNode.childNode);
    }
  };
  visit(workflowModel.value);
  return options;
});

const resetApproveState = () => {
  approveAction.value = "Agree";
  approveComment.value = "";
  rejectMode.value = "Previous";
  rejectTargetNodeId.value = null;
};

// 构建带有状态标记（已完成 / 当前 / 未开始）的流程预览模型
const buildWorkflowPreview = () => {
  if (!workflowModel.value) {
    workflowPreview.value = null;
    return;
  }

  // 根据任务历史 & 当前待办，按节点名称粗略推断节点状态
  const doneNames = new Set(
    taskHistory.value
      .filter((x) => x.action || x.completedAt)
      .map((x) => x.nodeName)
      .filter(Boolean) as string[]
  );
  const currentName = currentTask.value?.nodeName ?? null;

  const markNode = (node: WorkflowNode | null): WorkflowNode | null => {
    if (!node) return null;
    // 简单深拷贝，避免直接修改原始模型
    const cloned: any = JSON.parse(JSON.stringify(node));
    const name: string | undefined = cloned.nodeName;

    if (name && currentName && name === currentName) {
      cloned._status = "current";
    } else if (name && doneNames.has(name)) {
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
};

const openDetail = async (row: WorkflowTaskDto) => {
  if (!row.instanceId || query.tenantId == null) {
    return;
  }
  currentTask.value = row;
  detailVisible.value = true;
  detailLoading.value = true;
  try {
    const tenantId = query.tenantId ?? 0;

    const [instance, instHis, taskHis] = await Promise.all([
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
    currentInstance.value = instance ?? null;
    instanceHistory.value = instHis ?? [];
    taskHistory.value = taskHis ?? [];

    // 重置明细相关状态
    currentDefinition.value = null;
    currentFormDef.value = null;
    latestModel.value = null;
    workflowModel.value = null;
    formModel.value = {};
    resetApproveState();

    // 加载流程定义、最新流程模型 & 绑定表单定义
    if (instance?.definitionId) {
      const [def, model] = await Promise.all([
        getWorkflowDefinition(instance.definitionId),
        getLatestWorkflowDefinitionModel({
          tenantId,
          definitionId: instance.definitionId,
        }),
      ]);
      currentDefinition.value = def ?? null;
      latestModel.value = model ?? null;

      if (model?.modelJson) {
        try {
          workflowModel.value = JSON.parse(model.modelJson) as WorkflowNode;
        } catch {
          workflowModel.value = null;
          console.warn("[wf/todo] 解析流程模型 JSON 失败");
        }
      }
      console.log("def", def);
      const formKey = def?.formSchemaId;
      if (formKey) {
        const formDef = await getWorkflowForm(formKey);
        currentFormDef.value = formDef ?? null;
      }
    }

    // 根据业务主键从后端加载业务数据，填充表单模型
    if (instance?.definitionKey && instance.businessKey) {
      try {
        const biz = (await getWorkflowBusiness({
          tenantId,
          definitionKey: instance.definitionKey,
          businessKey: instance.businessKey,
        } as any)) as any;

        let payload: Record<string, any> | null = null;
        console.log("biz", biz);
        if (biz) {
          // 优先使用 formDataJson（后端 WorkflowBusinessData.FormDataJson）
          if (typeof biz.formDataJson === "string" && biz.formDataJson) {
            try {
              payload = JSON.parse(biz.formDataJson) ?? {};
              console.log("payload", payload);
            } catch (e) {
              console.warn("[wf/todo] 解析 formDataJson 失败", e);
            }
          }

          // 兼容后端直接返回反序列化后的对象（formData）
          if (!payload && biz.formData) {
            if (typeof biz.formData === "string") {
              try {
                payload = JSON.parse(biz.formData) ?? {};
                console.log("payload-string", payload);
              } catch (e) {
                console.warn("[wf/todo] 解析 formData 失败", e);
              }
            } else if (typeof biz.formData === "object") {
              payload = biz.formData as Record<string, any>;
              console.log("payload-object", payload);
            }
          }
        }

        formModel.value = payload ?? {};
        console.log("formModel.value", formModel.value);
      } catch (err) {
        console.warn("[wf/todo] 加载业务表单数据失败", err);
        formModel.value = {};
      }
    }

    // 根据最新的任务历史 & 流程模型，构建带状态的流程预览
    buildWorkflowPreview();
  } finally {
    detailLoading.value = false;
  }
};

// 抄送任务：仅标记已阅
const handleCcRead = async () => {
  if (!currentTask.value || !currentInstance.value) return;
  const tenantId = query.tenantId ?? 0;
  const userId = userStore.userInfo?.id as number | undefined;
  const userName = userStore.userInfo?.username || "";

  if (!userId) {
    ElMessage.error("当前用户信息缺失，无法提交");
    return;
  }

  approveLoading.value = true;
  try {
    await handleWorkflowTask({
      tenantId,
      taskId: currentTask.value.id!,
      action: "Read",
      comment: approveComment.value || "",
      operatorId: userId,
      operatorName: userName,
    } as any);

    ElMessage.success("已标记为已阅");
    detailVisible.value = false;
    loadTodoTasks();
  } finally {
    approveLoading.value = false;
  }
};

// 提交审批（同意 / 驳回）
const handleApprove = async () => {
  if (!currentTask.value || !currentInstance.value) return;
  const tenantId = query.tenantId ?? 0;
  const userId = userStore.userInfo?.id as number | undefined;
  const userName = userStore.userInfo?.username || "";

  if (!userId) {
    ElMessage.error("当前用户信息缺失，无法提交");
    return;
  }

  if (approveAction.value === "Reject" && !approveComment.value.trim()) {
    ElMessage.warning("驳回时必须填写审批意见");
    return;
  }

  approveLoading.value = true;
  try {
    await handleWorkflowTask({
      tenantId,
      taskId: currentTask.value.id!,
      action: approveAction.value,
      comment: approveComment.value || "",
      operatorId: userId,
      operatorName: userName,
      rejectMode:
        approveAction.value === "Reject" ? rejectMode.value : undefined,
      rejectToNodeId:
        approveAction.value === "Reject" && rejectMode.value === "Specific"
          ? rejectTargetNodeId.value || undefined
          : undefined,
      // 将当前表单模型作为最新业务数据传回后端（如需要可在后端更新 wf_business_data）
      formData: formModel.value,
    } as any);

    ElMessage.success(
      approveAction.value === "Agree" ? "已同意" : "已驳回流程"
    );
    detailVisible.value = false;
    loadTodoTasks();
  } finally {
    approveLoading.value = false;
  }
};
</script>

<template>
  <el-container class="h-full">
    <el-header
      class="bg-card border-border flex flex-wrap items-center justify-between gap-3 rounded-[.5vw]"
    >
      <el-form
        :inline="true"
        label-width="80px"
        class="mb-0 flex flex-wrap items-center gap-3"
      >
        <el-form-item label="用户">
          <soaSelectAdmin
            v-model="selectedAdmin"
            placeholder="请选择待办用户"
            :width="220"
          />
        </el-form-item>
        <el-form-item>
          <el-button type="primary" @click="loadTodoTasks"> 查询 </el-button>
        </el-form-item>
      </el-form>
    </el-header>

    <el-main class="bg-card border-border mt-2 rounded-[.5vw]">
      <soaTable
        ref="tableRef"
        :columns="columns"
        :apiObj="getWorkflowTodoTasks"
        :params="{ tenantId: query.tenantId ?? 0, userId: query.userId }"
        row-key="id"
        row-serial-number
      >
        <template #bodyCell="{ text, column, record }">
          <template v-if="column.key === 'action'">
            <el-button
              type="primary"
              link
              size="small"
              @click="openDetail(record)"
            >
              查看明细
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
      title="任务明细"
      size="50%"
      :destroy-on-close="true"
    >
      <div v-if="currentTask" class="space-y-4" v-loading="detailLoading">
        <el-card shadow="never">
          <template #header> 基本信息 </template>
          <el-descriptions :column="2" size="small" border>
            <el-descriptions-item label="实例 Id">
              {{ currentTask.instanceId }}
            </el-descriptions-item>
            <el-descriptions-item label="节点名称">
              {{ currentTask.nodeName }}
            </el-descriptions-item>
            <el-descriptions-item label="当前处理人">
              {{ currentTask.assigneeName }}
            </el-descriptions-item>
            <el-descriptions-item label="创建时间">
              {{ currentTask.createdAt }}
            </el-descriptions-item>
          </el-descriptions>
        </el-card>

        <el-card
          v-if="currentTask && currentTask.nodeName !== '抄送人'"
          shadow="never"
        >
          <template #header> 审批操作 </template>
          <el-form label-width="80px" size="small">
            <el-form-item label="操作">
              <el-radio-group v-model="approveAction">
                <el-radio label="Agree">同意</el-radio>
                <el-radio label="Reject">驳回</el-radio>
              </el-radio-group>
            </el-form-item>
            <el-form-item v-if="approveAction === 'Reject'" label="驳回到">
              <el-radio-group v-model="rejectMode">
                <el-radio label="Start">发起人</el-radio>
                <el-radio label="Previous">上一节点</el-radio>
                <el-radio label="Specific">指定节点</el-radio>
              </el-radio-group>
            </el-form-item>
            <el-form-item
              v-if="approveAction === 'Reject' && rejectMode === 'Specific'"
              label="目标节点"
            >
              <el-select
                v-model="rejectTargetNodeId"
                placeholder="请选择要驳回到的节点"
                style="width: 260px"
              >
                <el-option
                  v-for="opt in rejectNodeOptions"
                  :key="opt.value"
                  :label="opt.label"
                  :value="opt.value"
                />
              </el-select>
            </el-form-item>
            <el-form-item label="审批意见">
              <el-input
                v-model="approveComment"
                type="textarea"
                :autosize="{ minRows: 3, maxRows: 5 }"
                placeholder="请输入审批意见"
              />
            </el-form-item>
            <el-form-item>
              <el-button
                type="primary"
                :loading="approveLoading"
                @click="handleApprove"
              >
                提交
              </el-button>
              <el-button @click="detailVisible = false">关闭</el-button>
            </el-form-item>
          </el-form>
        </el-card>

        <el-card
          v-else-if="currentTask && currentTask.nodeName === '抄送人'"
          shadow="never"
        >
          <template #header> 抄送处理 </template>
          <div class="text-xs text-gray-500 dark:text-gray-400 mb-2">
            当前为抄送任务，仅需确认已阅，不会影响流程状态。
          </div>
          <el-form label-width="80px" size="small">
            <el-form-item label="备注">
              <el-input
                v-model="approveComment"
                type="textarea"
                :autosize="{ minRows: 2, maxRows: 4 }"
                placeholder="可填写阅读说明（选填）"
              />
            </el-form-item>
            <el-form-item>
              <el-button
                type="primary"
                :loading="approveLoading"
                @click="handleCcRead"
              >
                已阅
              </el-button>
              <el-button @click="detailVisible = false">关闭</el-button>
            </el-form-item>
          </el-form>
        </el-card>

        <el-card v-if="currentInstance" shadow="never">
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

        <el-card v-if="currentFormDef" shadow="never">
          <template #header>
            业务表单（{{ currentFormDef.name || currentFormDef.code }}）
          </template>

          <SoaForm
            v-if="parsedFormConfig"
            v-model="formModel"
            :config="parsedFormConfig"
            ><div class="mb-2 text-xs text-gray-500 dark:text-gray-400">
              展示的是该流程实例发起时填写的业务表单数据。
            </div></SoaForm
          >
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
              <div>
                整体流程
                <span
                  v-if="currentInstance?.currentNodeIds"
                  class="ml-2 text-xs text-gray-500 dark:text-gray-400"
                >
                  当前节点 Id：{{ currentInstance.currentNodeIds }}
                </span>
              </div>
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
            当前待办节点：{{
              currentTask?.nodeName || "-"
            }}；已完成步骤请查看下方“任务历史”列表。
          </div>
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
