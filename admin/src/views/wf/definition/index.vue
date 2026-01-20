<script setup lang="ts">
import soaTable from "@/component/soaTable/index.vue";
import {
  getWorkflowDefinitionList,
  createWorkflowDefinition,
  updateWorkflowDefinition,
  deleteWorkflowDefinition,
  type WorkflowDefinitionDto,
} from "@/api";
import { getWorkflowFormList, type WorkflowFormDto } from "@/api";

interface QueryState {
  tenantId: number | null;
  keyword: string;
  status: number | "";
}

const router = useRouter();
const tableRef = ref<InstanceType<typeof soaTable> | null>(null);
const formOptions = ref<WorkflowFormDto[]>([]);
const formMap = computed(() => {
  const map = new Map<string, WorkflowFormDto>();
  for (const f of formOptions.value) {
    const idKey = f.id != null ? String(f.id) : "";
    if (idKey) {
      map.set(idKey, f);
    }
    if (f.code) {
      map.set(f.code, f);
    }
  }
  return map;
});

// 查询条件，tenantId 默认 0（单租户项目可统一使用 0）
const query = reactive<QueryState>({
  tenantId: 0,
  keyword: "",
  status: "",
});

const columns = [
  {
    title: "流程编码",
    dataIndex: "defKey",
    key: "defKey",
    minWidth: 140,
    resizable: true,
  },
  {
    title: "流程名称",
    dataIndex: "defName",
    key: "defName",
    minWidth: 160,
    resizable: true,
  },
  {
    title: "版本",
    dataIndex: "version",
    key: "version",
    width: 80,
    align: "center",
  },
  {
    title: "状态",
    dataIndex: "status",
    key: "status",
    width: 100,
    align: "center",
  },
  {
    title: "分类",
    dataIndex: "category",
    key: "category",
    minWidth: 120,
  },
  {
    title: "绑定表单",
    dataIndex: "formSchemaId",
    key: "formSchemaId",
    minWidth: 150,
  },
  {
    title: "备注",
    dataIndex: "remark",
    key: "remark",
    minWidth: 160,
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

const statusOptions = [
  { label: "全部", value: "" },
  { label: "草稿", value: 0 },
  { label: "已发布", value: 1 },
  { label: "停用", value: 2 },
];

const statusText = (status?: number) => {
  if (status === 0) return "草稿";
  if (status === 1) return "已发布";
  if (status === 2) return "停用";
  return "-";
};

const statusTagType = (status?: number) => {
  if (status === 0) return "info";
  if (status === 1) return "success";
  if (status === 2) return "warning";
  return "default";
};

// soaTable 数据源：在前端做简单分页包装
const fetchDefinitionPage = async (req: {
  page: number;
  limit: number;
  tenantId?: number;
  keyword?: string;
  status?: number;
}) => {
  const list = await getWorkflowDefinitionList({
    tenantId: req.tenantId ?? Number(query.tenantId ?? 0),
    keyword: req.keyword || query.keyword || undefined,
    status:
      typeof req.status === "number"
        ? req.status
        : query.status === "" || query.status === null
        ? undefined
        : Number(query.status),
  });

  const start = (req.page - 1) * req.limit;
  const end = start + req.limit;

  return {
    items: list.slice(start, end),
    totalItems: list.length,
  };
};

const handleSearch = () => {
  if (query.tenantId == null) {
    ElMessage.warning("请先填写租户编号");
    return;
  }
  const params = {
    tenantId: Number(query.tenantId) || 0,
    keyword: query.keyword || undefined,
    status:
      query.status === "" || query.status === null
        ? undefined
        : Number(query.status),
  };
  tableRef.value?.upData(params);
};

const loadForms = async () => {
  const tid = Number(query.tenantId ?? 0);
  if (Number.isNaN(tid)) return;
  formOptions.value = await getWorkflowFormList({
    tenantId: tid,
    keyword: "",
  });
};

onMounted(() => {
  loadForms();
  // 可以按需自动触发一次查询
  // handleSearch();
});

const resetQuery = () => {
  query.keyword = "";
  query.status = "";
  handleSearch();
};

// 编辑弹窗
const editDialogVisible = ref(false);
const editing = ref<WorkflowDefinitionDto | null>(null);
const isEdit = computed(() => !!editing.value?.id);

const openCreateDialog = () => {
  editing.value = {
    tenantId: query.tenantId ?? 0,
    version: 1,
    status: 0,
  };
  editDialogVisible.value = true;
};

const openEditDialog = (row: WorkflowDefinitionDto) => {
  editing.value = { ...row };
  editDialogVisible.value = true;
};

const handleSave = async () => {
  if (!editing.value) return;
  const payload: WorkflowDefinitionDto = {
    ...editing.value,
    tenantId: editing.value.tenantId ?? query.tenantId ?? 0,
  };

  if (!payload.defKey || !payload.defName) {
    ElMessage.warning("请填写流程编码和流程名称");
    return;
  }

  if (!payload.id) {
    await createWorkflowDefinition(payload);
    ElMessage.success("创建成功");
  } else {
    await updateWorkflowDefinition(payload);
    ElMessage.success("保存成功");
  }

  editDialogVisible.value = false;
  await handleSearch();
};

const handleDelete = async (row: WorkflowDefinitionDto) => {
  if (!row.id) return;
  try {
    await ElMessageBox.confirm(
      `确定要删除流程定义「${row.defName || row.defKey || row.id}」吗？`,
      "提示",
      { type: "warning" }
    );
  } catch {
    return;
  }

  await deleteWorkflowDefinition(row.id);
  ElMessage.success("删除成功");
  await handleSearch();
};

const openDesigner = (row: WorkflowDefinitionDto) => {
  if (!row.id) return;
  router.push({
    path: "/wf/designer",
    query: {
      definitionId: row.id,
      tenantId: row.tenantId ?? query.tenantId ?? 0,
    },
  });
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
        <el-form-item label="关键字">
          <el-input
            v-model="query.keyword"
            placeholder="流程编码 / 名称"
            clearable
            style="width: 220px"
          />
        </el-form-item>
        <el-form-item label="状态">
          <el-select
            v-model="query.status"
            placeholder="全部"
            clearable
            style="width: 140px"
          >
            <el-option
              v-for="item in statusOptions"
              :key="item.label"
              :label="item.label"
              :value="item.value"
            />
          </el-select>
        </el-form-item>
        <el-form-item>
          <el-button type="primary" @click="loadDefinitions"> 查询 </el-button>
          <el-button @click="resetQuery"> 重置 </el-button>
        </el-form-item>
        <el-form-item>
          <el-button type="success" @click="openCreateDialog">
            新建流程定义
          </el-button>
        </el-form-item>
      </el-form>
    </el-header>

    <el-main class="bg-card border-border mt-2 rounded-[.5vw]">
      <soaTable
        ref="tableRef"
        :columns="columns"
        :apiObj="fetchDefinitionPage"
        row-key="id"
        row-serial-number
      >
        <template #bodyCell="{ text, column, record }">
          <template v-if="column.key === 'status'">
            <el-tag :type="statusTagType(record.status)">
              {{ statusText(record.status) }}
            </el-tag>
          </template>
          <template v-else-if="column.key === 'formSchemaId'">
            <span>
              {{
                formMap.get(String(record.formSchemaId || ""))?.name ||
                formMap.get(String(record.formSchemaId || ""))?.code ||
                record.formSchemaId ||
                "-"
              }}
            </span>
          </template>
          <template v-else-if="column.key === 'action'">
            <el-space>
              <el-button
                type="primary"
                link
                size="small"
                @click="openDesigner(record)"
              >
                设计流程
              </el-button>
              <el-button
                type="primary"
                link
                size="small"
                @click="openEditDialog(record)"
              >
                编辑
              </el-button>
              <el-button
                type="danger"
                link
                size="small"
                @click="handleDelete(record)"
              >
                删除
              </el-button>
            </el-space>
          </template>
          <template v-else>
            {{ text }}
          </template>
        </template>
      </soaTable>
    </el-main>

    <el-dialog
      v-model="editDialogVisible"
      :title="isEdit ? '编辑流程定义' : '新建流程定义'"
      width="520px"
      destroy-on-close
    >
      <el-form
        v-if="editing"
        label-width="90px"
        label-position="right"
        class="pt-2"
      >
        <el-form-item label="流程编码" required>
          <el-input v-model="editing.defKey" placeholder="例如：LEAVE_APPLY" />
        </el-form-item>
        <el-form-item label="流程名称" required>
          <el-input v-model="editing.defName" placeholder="请输入流程名称" />
        </el-form-item>
        <el-form-item label="版本号">
          <el-input-number
            v-model="editing.version"
            :min="1"
            :step="1"
            style="width: 160px"
          />
        </el-form-item>
        <el-form-item label="状态">
          <el-select v-model="editing.status" style="width: 160px">
            <el-option label="草稿" :value="0" />
            <el-option label="已发布" :value="1" />
            <el-option label="停用" :value="2" />
          </el-select>
        </el-form-item>
        <el-form-item label="分类">
          <el-input v-model="editing.category" placeholder="例如：人事审批" />
        </el-form-item>
        <el-form-item label="绑定表单">
          <el-select
            v-model="editing.formSchemaId"
            placeholder="请选择绑定的表单"
            clearable
            filterable
            style="width: 100%"
          >
            <el-option
              v-for="item in formOptions"
              :key="item.id"
              :label="item.name || item.code || item.id"
              :value="String(item.id)"
            />
          </el-select>
        </el-form-item>
        <el-form-item label="备注">
          <el-input
            v-model="editing.remark"
            type="textarea"
            :rows="3"
            placeholder="备注说明"
          />
        </el-form-item>
      </el-form>

      <template #footer>
        <el-button @click="editDialogVisible = false"> 取消 </el-button>
        <el-button type="primary" @click="handleSave"> 保存 </el-button>
      </template>
    </el-dialog>
  </el-container>
</template>
