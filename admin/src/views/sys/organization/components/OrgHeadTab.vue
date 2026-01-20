<template>
  <div class="h-full">
    <div class="flex flex-wrap justify-between gap-3">
      <el-form :inline="true" class="flex flex-wrap items-center gap-3">
        <el-form-item label="组织">
          <el-tree-select
            v-model="query.orgId"
            :data="orgTreeOptions"
            :props="treeSelectProps"
            filterable
            clearable
            check-strictly
            default-expand-all
            placeholder="选择组织"
            class="w-64"
          />
        </el-form-item>
        <el-form-item label="类型">
          <el-select
            v-model="query.headType"
            placeholder="全部"
            clearable
            style="width: 200px"
          >
            <el-option
              v-for="option in headTypeOptions"
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
      <el-button type="primary" @click="openCreateOrgHead">
        <el-icon><Plus /></el-icon>
        新增负责人
      </el-button>
    </div>

    <soaTable
      ref="innerTableRef"
      :columns="orgHeadColumns"
      :apiObj="fetchOrgHeadList"
      :showSelection="false"
      row-key="id"
      row-serial-number
    >
      <template #bodyCell="{ column, record }">
        <template v-if="column.key === 'orgId'">
          {{ formatOrg(record.orgId) }}
        </template>
        <template v-else-if="column.key === 'employmentId'">
          {{ record.employment.fullName }}
        </template>
        <template v-else-if="column.key === 'action'">
          <div class="flex items-center gap-1">
            <el-button link type="primary" @click="openEditOrgHead(record)"
              >编辑</el-button
            >
            <el-popconfirm
              title="确认删除该负责人？"
              @confirm="handleDeleteOrgHead(record.id)"
            >
              <template #reference>
                <el-button link type="danger">删除</el-button>
              </template>
            </el-popconfirm>
          </div>
        </template>
      </template>
    </soaTable>

    <!-- 负责人维护弹窗：使用 soaModal 封装 -->
    <OrgHeadModal
      title="负责人维护"
      class="w-[520px]"
      :close-on-click-modal="false"
    >
      <el-form
        ref="orgHeadFormRef"
        :model="orgHeadForm"
        :rules="orgHeadRules"
        label-width="112px"
      >
        <el-form-item label="组织" prop="orgId">
          <el-tree-select
            v-model="orgHeadForm.orgId"
            :data="orgTreeOptions"
            :props="treeSelectProps"
            filterable
            default-expand-all
            check-strictly
            placeholder="选择组织"
            class="w-full"
          />
        </el-form-item>
        <el-form-item label="任用" prop="headUser">
          <soaSelectAdmin
            v-model="orgHeadForm.headUser"
            placeholder="请选择负责人"
            @change="onHeadUserChange"
          />
        </el-form-item>
        <el-form-item label="负责人类型" prop="headType">
          <el-select v-model="orgHeadForm.headType" placeholder="选择类型">
            <el-option
              v-for="option in headTypeOptions"
              :key="option.value"
              :label="option.label"
              :value="option.value"
            />
          </el-select>
        </el-form-item>
        <el-form-item label="生效时间" prop="validFrom">
          <el-date-picker
            v-model="orgHeadForm.validFrom"
            type="datetime"
            value-format="YYYY-MM-DDTHH:mm:ss"
            placeholder="选择生效时间"
            class="w-full"
          />
        </el-form-item>
        <el-form-item label="失效时间" prop="validTo">
          <el-date-picker
            v-model="orgHeadForm.validTo"
            type="datetime"
            value-format="YYYY-MM-DDTHH:mm:ss"
            placeholder="选择失效时间"
            class="w-full"
            clearable
          />
        </el-form-item>
        <el-form-item label="备注" prop="note">
          <el-input
            v-model="orgHeadForm.note"
            type="textarea"
            :rows="3"
            placeholder="备注信息"
          />
        </el-form-item>
      </el-form>
    </OrgHeadModal>
  </div>
</template>

<script setup lang="ts">
import { ref } from "vue";
import { Plus } from "@element-plus/icons-vue";
import { ElMessage, type FormInstance, type FormRules } from "element-plus";
import soaTable from "@/component/soaTable/index.vue";
import { useSoaModal } from "@/component/soaModal/index.vue";
import {
  fetchOrgHeadList,
  createOrgHead,
  updateOrgHead,
  deleteOrgHead,
  fetchOrgUnitList,
} from "@/api";
import soaSelectAdmin from "@/component/soaSelectAdmin/index.vue";
import { changeTree } from "@/utils/tools";
import type {
  OrgUnitHead,
  CreateOrgUnitHeadInput,
  UpdateOrgUnitHeadInput,
} from "@/types/identity";

const props = defineProps<{
  // 当前租户，用于过滤负责人列表
  tenantId: number;
}>();

const emit = defineEmits<{
  /**
   * 负责人数据发生变更时通知父组件刷新依赖
   */
  (e: "org-head-changed"): void;
}>();

const innerTableRef = ref<InstanceType<typeof soaTable> | null>(null);

// 子组件内部维护查询条件
const query = ref<{
  tenantId: number;
  orgId?: number;
  headType?: string;
}>({
  tenantId: 0,
  orgId: undefined,
  headType: undefined,
});

// 负责人列表列配置
const orgHeadColumns = [
  {
    title: "组织",
    dataIndex: "orgId",
    key: "orgId",
    width: 220,
    resizable: true,
    fixed: true,
  },
  {
    title: "任用",
    dataIndex: "employmentId",
    key: "employmentId",
    width: 240,
  },
  {
    title: "类型",
    dataIndex: "headType",
    key: "headType",
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

// 组织树数据与 TreeSelect 配置在子组件内部维护
const orgTreeOptions = ref<any[]>([]);
const treeSelectProps = {
  value: "id",
  label: "name",
  children: "children",
};

const formatOrg = (orgId?: number) => {
  if (!orgId) return "-";
  // 这里只根据当前树简单查找名称（树数据较小，性能可接受）
  const loop = (nodes: any[]): string | undefined => {
    for (const n of nodes) {
      if (n.id === orgId) return n.name;
      if (n.children?.length) {
        const r = loop(n.children);
        if (r) return r;
      }
    }
  };
  return loop(orgTreeOptions.value) ?? `组织 ${orgId}`;
};

const getNowString = () => new Date().toISOString().slice(0, 19);

const orgHeadFormRef = ref<FormInstance>();
const orgHeadForm = ref<{
  id?: string;
  tenantId: number;
  orgId?: number;
  employmentId?: string;
  headType: string;
  validFrom: string;
  validTo?: string | null;
  note?: string | null;
  // soaSelectAdmin 选中的负责人对象
  headUser?: Record<string, any> | null;
  employment: any;
}>({
  id: undefined,
  tenantId: 0,
  orgId: undefined,
  employmentId: undefined,
  headType: "PRIMARY",
  validFrom: "",
  validTo: undefined,
  note: undefined,
  employment: null,
});

const orgHeadRules: FormRules = {
  orgId: [{ required: true, message: "请选择组织", trigger: "change" }],
  headUser: [{ required: true, message: "请选择负责人", trigger: "change" }],
  headType: [
    { required: true, message: "请选择负责人类型", trigger: "change" },
  ],
  validFrom: [{ required: true, message: "请选择生效时间", trigger: "change" }],
};

type DialogMode = "create" | "edit";
const dialogMode = ref<DialogMode>("create");
const saving = ref(false);

// 负责人类型字典在子组件内部维护
const headTypeOptions = [
  { label: "主要负责人 (PRIMARY)", value: "PRIMARY" },
  { label: "副负责人 (DEPUTY)", value: "DEPUTY" },
  { label: "代理负责人 (ACTING)", value: "ACTING" },
];

// 使用 soaModal 管理负责人弹窗
const [OrgHeadModal, orgHeadModalApi] = useSoaModal({
  onConfirm: () => {
    handleSubmit();
  },
  onCancel: () => {
    resetForm();
    orgHeadModalApi.close();
  },
});

const buildParams = () => {
  const params: { tenantId: number; orgId?: number; headType?: string } = {
    tenantId: query.value.tenantId,
  };
  if (query.value.orgId) params.orgId = query.value.orgId;
  if (query.value.headType) params.headType = query.value.headType;
  return params;
};

// 构建组织树
const buildTree = (data: any[]) => {
  if (!Array.isArray(data)) return [];
  // 使用全局工具 changeTree 按 parentId 构建树
  return changeTree(data);
};

const loadOrgTree = async () => {
  const list = await fetchOrgUnitList({ page: 1, limit: 1000 });
  orgTreeOptions.value = buildTree(list);
};

const resetForm = () => {
  orgHeadForm.value.id = undefined;
  orgHeadForm.value.tenantId = 0;
  orgHeadForm.value.orgId = query.orgId ?? undefined;
  orgHeadForm.value.employmentId = undefined;
  orgHeadForm.value.headType = "PRIMARY";
  orgHeadForm.value.validFrom = getNowString();
  orgHeadForm.value.validTo = null;
  orgHeadForm.value.note = undefined;
  orgHeadForm.value.headUser = null;
};

const openCreateOrgHead = () => {
  dialogMode.value = "create";
  resetForm();
  orgHeadModalApi.open();
};

const openEditOrgHead = (row: OrgUnitHead) => {
  dialogMode.value = "edit";
  orgHeadForm.value.id = row.id;
  orgHeadForm.value.tenantId = row.tenantId as any;
  orgHeadForm.value.orgId = row.orgId as any;
  orgHeadForm.value.employmentId = String(row.employmentId) as any;
  orgHeadForm.value.headType = row.headType as any;
  orgHeadForm.value.validFrom = row.validFrom ?? getNowString();
  orgHeadForm.value.validTo = row.validTo ?? null;
  orgHeadForm.value.note = row.note ?? undefined;
  // 仅根据 Id 填充一个简单的负责人对象，便于 soaSelectAdmin 回显
  orgHeadForm.value.headUser = row.employment;
  orgHeadModalApi.open();
};

// soaSelectAdmin change：为表单 employmentId 赋值
const onHeadUserChange = (val: any) => {
  orgHeadForm.value.headUser = val;
  orgHeadForm.value.employmentId = val ? String(val.id) : undefined;
};

const handleSubmit = () => {
  orgHeadFormRef.value?.validate(async (valid) => {
    if (!valid) return;
    const form = orgHeadForm.value;
    if (!form.orgId || !form.employmentId) {
      ElMessage.warning("请完整填写组织与负责人");
      return;
    }

    saving.value = true;
    try {
      const payload: CreateOrgUnitHeadInput = {
        tenantId: String(form.tenantId) as any,
        orgId: String(form.orgId) as any,
        employmentId: String(form.employmentId) as any,
        headType: form.headType as any,
        validFrom: form.validFrom,
        validTo: form.validTo || null,
        note: form.note || null,
      };

      if (dialogMode.value === "create") {
        await createOrgHead(payload);
        ElMessage.success("新增负责人成功");
      } else {
        await updateOrgHead({
          ...payload,
          id: form.id!,
        } as UpdateOrgUnitHeadInput);
        ElMessage.success("更新负责人成功");
      }

      orgHeadModalApi.close();
      resetForm();
      innerTableRef.value?.upData(buildParams());
      emit("org-head-changed");
    } finally {
      saving.value = false;
    }
  });
};

const handleDeleteOrgHead = async (id: number) => {
  await deleteOrgHead(id);
  ElMessage.success("删除成功");
  innerTableRef.value?.upData(buildParams());
  emit("org-head-changed");
};

const handleSearch = async () => {
  innerTableRef.value?.upData(buildParams());
};

const handleReset = async () => {
  query.value.orgId = undefined;
  query.value.headType = undefined;
  innerTableRef.value?.upData(buildParams());
};

// 暴露刷新方法给父组件：用于租户切换时重查
const refresh = (tenantId?: number) => {
  if (tenantId != null) {
    query.value.tenantId = tenantId;
  }
  innerTableRef.value?.upData(buildParams());
};

defineExpose({ refresh });

// 初始化组织树
loadOrgTree();
</script>
