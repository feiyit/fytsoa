<template>
  <div class="h-full">
    <div class="flex flex-wrap justify-between gap-2">
      <el-form :inline="true" class="flex flex-wrap items-center gap-2">
        <el-form-item label="下属任用">
          <el-select
            v-model="query.subordinateEmploymentId"
            placeholder="选择下属任用"
            clearable
            filterable
            style="width: 220px"
          >
            <el-option
              v-for="item in employmentSelectOptions"
              :key="item.value"
              :label="item.label"
              :value="item.value"
            />
          </el-select>
        </el-form-item>
        <el-form-item label="上级任用">
          <el-select
            v-model="query.managerEmploymentId"
            placeholder="选择上级任用"
            clearable
            filterable
            style="width: 220px"
          >
            <el-option
              v-for="item in employmentSelectOptions"
              :key="item.value"
              :label="item.label"
              :value="item.value"
            />
          </el-select>
        </el-form-item>
        <el-form-item label="关系">
          <el-select
            v-model="query.relation"
            placeholder="全部"
            clearable
            style="width: 180px"
          >
            <el-option
              v-for="option in reportingRelationOptions"
              :key="option.value"
              :label="option.label"
              :value="option.value"
            />
          </el-select>
        </el-form-item>
        <el-form-item>
          <el-button type="primary" @click="handleSearch">查询</el-button>
          <el-button @click="handleReset">重置</el-button>
        </el-form-item>
      </el-form>
      <el-button type="primary" @click="openCreateReporting">
        <el-icon><Plus /></el-icon>
        新增汇报关系
      </el-button>
    </div>

    <soaTable
      ref="innerTableRef"
      :columns="reportingColumns"
      :apiObj="fetchReportingList"
      :showSelection="false"
      row-key="id"
      row-serial-number
    >
      <template #bodyCell="{ column, record }">
        <template v-if="column.key === 'subordinateEmploymentId'">
          {{ formatEmployment(record.subordinateEmploymentId) }}
        </template>
        <template v-else-if="column.key === 'managerEmploymentId'">
          {{ formatEmployment(record.managerEmploymentId) }}
        </template>
        <template v-else-if="column.key === 'relation'">
          {{ record.relation }}
        </template>
        <template v-else-if="column.key === 'action'">
          <div class="flex items-center gap-1">
            <el-button link type="primary" @click="openEditReporting(record)"
              >编辑</el-button
            >
            <el-popconfirm
              title="确认删除该汇报关系？"
              @confirm="handleDeleteReporting(record.id)"
            >
              <template #reference>
                <el-button link type="danger">删除</el-button>
              </template>
            </el-popconfirm>
          </div>
        </template>
      </template>
    </soaTable>

    <!-- 汇报关系维护弹窗：使用 soaModal 封装 -->
    <ReportingModal
      title="汇报关系维护"
      class="w-[520px]"
      :close-on-click-modal="false"
    >
      <el-form
        ref="reportingFormRef"
        :model="reportingForm"
        :rules="reportingRules"
        label-width="112px"
      >
        <el-form-item label="下属任用" prop="subordinateEmploymentId">
          <el-select
            v-model="reportingForm.subordinateEmploymentId"
            placeholder="选择下属任用"
            filterable
            class="w-full"
          >
            <el-option
              v-for="item in employmentSelectOptions"
              :key="item.value"
              :label="item.label"
              :value="item.value"
            />
          </el-select>
        </el-form-item>
        <el-form-item label="上级任用" prop="managerEmploymentId">
          <el-select
            v-model="reportingForm.managerEmploymentId"
            placeholder="选择上级任用"
            filterable
            class="w-full"
          >
            <el-option
              v-for="item in employmentSelectOptions"
              :key="item.value"
              :label="item.label"
              :value="item.value"
            />
          </el-select>
        </el-form-item>
        <el-form-item label="关系类型" prop="relation">
          <el-select v-model="reportingForm.relation" placeholder="选择关系">
            <el-option
              v-for="option in reportingRelationOptions"
              :key="option.value"
              :label="option.label"
              :value="option.value"
            />
          </el-select>
        </el-form-item>
        <el-form-item label="生效区间" prop="validRange">
          <el-date-picker
            v-model="reportingForm.validRange"
            type="datetimerange"
            value-format="YYYY-MM-DDTHH:mm:ss"
            range-separator="至"
            start-placeholder="开始时间"
            end-placeholder="结束时间"
            class="w-full"
          />
        </el-form-item>
        <el-form-item label="备注" prop="note">
          <el-input
            v-model="reportingForm.note"
            type="textarea"
            :rows="3"
            placeholder="备注信息"
          />
        </el-form-item>
      </el-form>
    </ReportingModal>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from "vue";
import { Plus } from "@element-plus/icons-vue";
import { ElMessage, type FormInstance, type FormRules } from "element-plus";
import soaTable from "@/component/soaTable/index.vue";
import { useSoaModal } from "@/component/soaModal/index.vue";
import {
  fetchReportingList,
  createReporting,
  updateReporting,
  deleteReporting,
  fetchEmploymentList,
} from "@/api";
import type {
  EmploymentReporting,
  CreateEmploymentReportingInput,
  UpdateEmploymentReportingInput,
  Employment,
} from "@/types/identity";

const props = defineProps<{
  tenantId: number;
}>();

const emit = defineEmits<{
  /**
   * 汇报关系变更时通知父组件（当前仅作为扩展点，父组件可选择监听）
   */
  (e: "reporting-changed"): void;
}>();

// 任用列表缓存：用于构建下拉和格式化显示
const employmentList = ref<Employment[]>([]);

// 从后端加载所有任用记录，并按当前租户进行过滤
const loadEmploymentOptions = async () => {
  // 约定：id = "0" 时不按用户过滤，返回当前租户的所有任用记录
  const list = await fetchEmploymentList({ id: "0" });
  employmentList.value = (list || []).filter(
    (e: any) => String(e.tenantId) === String(props.tenantId)
  );
};

// 为任用构建下拉选项，label 中尽量包含用户 / 组织 / 岗位信息，便于区分多任用场景
const employmentSelectOptions = computed(() =>
  employmentList.value.map((e: any) => {
    const user = e.user || e.User;
    const org = e.org || e.Org || e.orgUnit || e.OrgUnit;
    const position = e.position || e.Position;

    const userText =
      user?.fullName || user?.userName || (e.userId ? `用户${e.userId}` : "");
    const orgText = org?.name || (e.orgId ? `组织${e.orgId}` : "");
    const posText =
      position?.name || (e.positionId ? `岗位${e.positionId}` : "");

    const parts = [userText, orgText, posText].filter(Boolean);
    const label = parts.length > 0 ? parts.join(" / ") : `任用ID=${e.id}`;

    return {
      value: String(e.id),
      label,
    };
  })
);

const innerTableRef = ref<InstanceType<typeof soaTable> | null>(null);

// 对外暴露刷新方法：可选更新租户，同时重新加载任用选项并刷新汇报关系列表
const refresh = async (tenantId?: number) => {
  if (tenantId != null) {
    query.value.tenantId = tenantId;
  }
  await loadEmploymentOptions();
  innerTableRef.value?.upData(buildParams());
};

defineExpose({ refresh });

// 查询条件由子组件自己维护：下属任用 / 上级任用 / 关系
const query = ref<{
  tenantId: number;
  subordinateEmploymentId?: string;
  managerEmploymentId?: string;
  relation?: string;
}>({
  tenantId: props.tenantId,
  subordinateEmploymentId: undefined,
  managerEmploymentId: undefined,
  relation: undefined,
});

// 根据任用Id格式化显示（表格中使用）
const formatEmployment = (employmentId?: string) => {
  if (!employmentId) return "-";
  const target = employmentList.value.find(
    (e) => String(e.id) === String(employmentId)
  ) as any;
  if (!target) {
    return employmentId;
  }
  const user = target.user || target.User;
  const org = target.org || target.Org || target.orgUnit || target.OrgUnit;
  const position = target.position || target.Position;

  const userText =
    user?.fullName ||
    user?.userName ||
    (target.userId ? `用户${target.userId}` : "");
  const orgText =
    org?.name || (target.orgId ? `组织${target.orgId}` : "");
  const posText =
    position?.name || (target.positionId ? `岗位${target.positionId}` : "");

  const parts = [userText, orgText, posText].filter(Boolean);
  return parts.length > 0 ? parts.join(" / ") : `任用ID=${target.id}`;
};

// 关系字典在子组件内部维护，父组件只管租户和数据源
const reportingRelationOptions = [
  { label: "直属 (LINE)", value: "LINE" },
  { label: "职能 (FUNCTIONAL)", value: "FUNCTIONAL" },
];

// 列配置下沉到子组件内部
const reportingColumns = [
  {
    title: "下属任用",
    dataIndex: "subordinateEmploymentId",
    key: "subordinateEmploymentId",
    width: 240,
    resizable: true,
    fixed: true,
  },
  {
    title: "上级任用",
    dataIndex: "managerEmploymentId",
    key: "managerEmploymentId",
    width: 240,
  },
  {
    title: "关系",
    dataIndex: "relation",
    key: "relation",
    width: 160,
  },
  {
    title: "生效时间",
    dataIndex: "validFrom",
    key: "validFrom",
    width: 180,
  },
  {
    title: "失效时间",
    dataIndex: "validTo",
    key: "validTo",
    width: 180,
    align: "center",
  },

  {
    title: "备注",
    dataIndex: "note",
    key: "note",
    minWidth: 200,
  },
  {
    title: "操作",
    key: "action",
    width: 100,
    fixed: "right",
  },
];

const getNowString = () => new Date().toISOString().slice(0, 19);

const buildParams = () => {
  const params: {
    tenantId: number;
    subordinateEmploymentId?: number;
    managerEmploymentId?: number;
    relation?: string;
  } = {
    tenantId: query.value.tenantId,
  };
  if (query.value.subordinateEmploymentId) {
    params.subordinateEmploymentId = Number(
      query.value.subordinateEmploymentId
    );
  }
  if (query.value.managerEmploymentId) {
    params.managerEmploymentId = Number(query.value.managerEmploymentId);
  }
  if (query.value.relation) params.relation = query.value.relation;
  return params;
};

const reportingFormRef = ref<FormInstance>();
const reportingForm = ref<{
  id?: string;
  tenantId: number;
  subordinateEmploymentId?: string;
  managerEmploymentId?: string;
  relation: string;
  validRange: [string, string] | [];
  note?: string | null;
}>({
  id: undefined,
  tenantId: query.value.tenantId,
  subordinateEmploymentId: undefined,
  managerEmploymentId: undefined,
  relation: "LINE",
  validRange: [] as [string, string] | [],
  note: undefined,
});

const reportingRules: FormRules = {
  subordinateEmploymentId: [
    { required: true, message: "请选择下属任用", trigger: "change" },
  ],
  managerEmploymentId: [
    { required: true, message: "请选择上级任用", trigger: "change" },
  ],
  relation: [{ required: true, message: "请选择关系类型", trigger: "change" }],
  validRange: [
    { required: true, message: "请选择生效区间", trigger: "change" },
  ],
};

type DialogMode = "create" | "edit";
const dialogMode = ref<DialogMode>("create");
const saving = ref(false);

// 使用 soaModal 管理汇报关系弹窗
const [ReportingModal, reportingModalApi] = useSoaModal({
  onConfirm: () => {
    handleSubmit();
  },
  onCancel: () => {
    resetForm();
    reportingModalApi.close();
  },
});

const resetForm = () => {
  reportingForm.value.id = undefined;
  reportingForm.value.tenantId = query.value.tenantId;
  reportingForm.value.subordinateEmploymentId = undefined;
  reportingForm.value.managerEmploymentId = undefined;
  reportingForm.value.relation = "LINE";
  reportingForm.value.validRange = [getNowString(), getNowString()] as [
    string,
    string
  ];
  reportingForm.value.note = undefined;
};

const openCreateReporting = () => {
  dialogMode.value = "create";
  resetForm();
  reportingModalApi.open();
};

const openEditReporting = (row: EmploymentReporting) => {
  dialogMode.value = "edit";
  reportingForm.value.id = row.id;
  reportingForm.value.tenantId = row.tenantId as any;
  reportingForm.value.subordinateEmploymentId =
    row.subordinateEmploymentId as any;
  reportingForm.value.managerEmploymentId = row.managerEmploymentId as any;
  reportingForm.value.relation = row.relation;
  reportingForm.value.validRange = [
    row.validFrom,
    row.validTo ?? row.validFrom,
  ] as [string, string];
  reportingForm.value.note = row.note ?? undefined;
  reportingModalApi.open();
};

const handleSubmit = () => {
  reportingFormRef.value?.validate(async (valid) => {
    if (!valid) return;
    const form = reportingForm.value;

    if (!form.subordinateEmploymentId || !form.managerEmploymentId) {
      ElMessage.warning("请选择上下级任用");
      return;
    }
    if (form.subordinateEmploymentId === form.managerEmploymentId) {
      ElMessage.warning("上下级任用不能相同");
      return;
    }
    if (!form.validRange || form.validRange.length !== 2) {
      ElMessage.warning("请选择生效区间");
      return;
    }

    saving.value = true;
    try {
      const payload: CreateEmploymentReportingInput = {
        tenantId: String(form.tenantId),
        subordinateEmploymentId: String(form.subordinateEmploymentId!),
        managerEmploymentId: String(form.managerEmploymentId!),
        relation: form.relation,
        validFrom: form.validRange[0],
        validTo: form.validRange[1] || null,
        note: form.note || null,
      };

      if (dialogMode.value === "create") {
        await createReporting(payload);
        ElMessage.success("新增汇报关系成功");
      } else {
        await updateReporting({
          ...payload,
          id: form.id!,
        } as UpdateEmploymentReportingInput);
        ElMessage.success("更新汇报关系成功");
      }

      reportingModalApi.close();
      resetForm();
      innerTableRef.value?.upData(buildParams());
      emit("reporting-changed");
    } finally {
      saving.value = false;
    }
  });
};

const handleDeleteReporting = async (id: number) => {
  await deleteReporting(id);
  ElMessage.success("删除成功");
  innerTableRef.value?.upData(buildParams());
  emit("reporting-changed");
};

const handleSearch = () => {
  innerTableRef.value?.upData(buildParams());
};

const handleReset = () => {
  query.value.subordinateEmploymentId = undefined;
  query.value.managerEmploymentId = undefined;
  query.value.relation = undefined;
  innerTableRef.value?.upData(buildParams());
};

onMounted(async () => {
  // 初始化加载任用选项，支持一人多任用 / 多岗位 / 多组织的配置场景
  await loadEmploymentOptions();
  innerTableRef.value?.upData(buildParams());
});
</script>
