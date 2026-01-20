<template>
  <div class="h-full">
    <div class="flex flex-wrap justify-between gap-3">
      <el-form
        :inline="true"
        :model="query"
        class="mb-0 flex flex-wrap items-center gap-3"
      >
        <el-form-item label="用户">
          <el-select
            v-model="query.userId"
            placeholder="请选择用户"
            clearable
            filterable
            style="width: 240px"
          >
            <el-option
              v-for="user in userOptions"
              :key="user.id"
              :label="`${user.fullName}`"
              :value="user.id"
            />
          </el-select>
        </el-form-item>
        <el-form-item>
          <el-button type="primary" @click="handleSearch">查询</el-button>
          <el-button @click="handleReset">重置</el-button>
        </el-form-item>
      </el-form>
      <el-button type="primary" @click="openCreateEmployment">
        <el-icon><Plus /></el-icon>
        新增任用
      </el-button>
    </div>
    <soaTable
      ref="tableEmploymentRef"
      :columns="employmentColumns"
      :apiObj="fetchEmploymentPage"
      :params="tableParams"
      :showSelection="false"
      row-key="id"
      row-serial-number
    >
      <template #bodyCell="{ column, record }">
        <template v-if="column.key === 'userId'">
          {{ formatUser(record.userId) }}
        </template>
        <template v-else-if="column.key === 'orgId'">
          {{ formatOrg(record.orgId) }}
        </template>
        <template v-else-if="column.key === 'positionId'">
          {{ formatPosition(record.positionId) }}
        </template>
        <template v-else-if="column.key === 'isPrimary'">
          <el-tag :type="record.isPrimary ? 'success' : 'info'">
            {{ record.isPrimary ? "主" : "辅" }}
          </el-tag>
        </template>
        <template v-else-if="column.key === 'action'">
          <div class="flex items-center gap-1">
            <el-button link type="primary" @click="openEditEmployment(record)"
              >编辑</el-button
            >
            <el-popconfirm
              title="确认删除该任用？"
              @confirm="handleDeleteEmployment(record.id)"
            >
              <template #reference>
                <el-button link type="danger">删除</el-button>
              </template>
            </el-popconfirm>
          </div>
        </template>
      </template>
    </soaTable>

    <!-- 任用维护弹窗：使用 soaModal 封装 -->
    <EmploymentModal
      title="任用维护"
      class="w-[520px]"
      :close-on-click-modal="false"
    >
      <el-form
        ref="employmentFormRef"
        :model="employmentForm"
        :rules="employmentRules"
        label-width="112px"
      >
        <el-form-item label="用户" prop="userId">
          <el-select
            v-model="employmentForm.userId"
            placeholder="请选择用户"
            filterable
            class="w-full"
          >
            <el-option
              v-for="user in userOptions"
              :key="user.id"
              :label="`${user.fullName}`"
              :value="user.id"
            />
          </el-select>
        </el-form-item>
        <el-form-item label="组织" prop="orgId">
          <el-tree-select
            v-model="employmentForm.orgId"
            :data="orgTreeOptions"
            :props="treeSelectProps"
            filterable
            default-expand-all
            check-strictly
            placeholder="请选择组织"
            class="w-full"
          />
        </el-form-item>
        <el-form-item label="岗位" prop="positionId">
          <el-select
            v-model="employmentForm.positionId"
            placeholder="请选择岗位"
            filterable
          >
            <el-option
              v-for="item in positionOptions"
              :key="item.id"
              :label="`${item.name} (${item.code})`"
              :value="item.id"
            />
          </el-select>
        </el-form-item>
        <el-form-item label="主任用" prop="isPrimary">
          <el-switch v-model="employmentForm.isPrimary" />
        </el-form-item>
        <el-form-item label="生效时间" prop="validFrom">
          <el-date-picker
            v-model="employmentForm.validFrom"
            type="datetime"
            value-format="YYYY-MM-DDTHH:mm:ss"
            placeholder="选择生效时间"
            class="w-full"
          />
        </el-form-item>
        <el-form-item label="失效时间" prop="validTo">
          <el-date-picker
            v-model="employmentForm.validTo"
            type="datetime"
            value-format="YYYY-MM-DDTHH:mm:ss"
            placeholder="选择失效时间"
            class="w-full"
            clearable
          />
        </el-form-item>
        <el-form-item label="备注" prop="note">
          <el-input
            v-model="employmentForm.note"
            type="textarea"
            :rows="3"
            placeholder="备注信息"
          />
        </el-form-item>
      </el-form>
    </EmploymentModal>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, computed, onMounted } from "vue";
import { Plus } from "@element-plus/icons-vue";
import { ElMessage, type FormInstance, type FormRules } from "element-plus";
import soaTable from "@/component/soaTable/index.vue";
import { useSoaModal } from "@/component/soaModal/index.vue";
import {
  fetchEmploymentPage,
  createEmployment,
  updateEmployment,
  deleteEmployment,
  fetchOrgUnitList,
} from "@/api";
import type {
  UserAccount,
  OrgUnit,
  Position,
  Employment,
  CreateEmploymentInput,
  UpdateEmploymentInput,
} from "@/types/identity";

type DialogMode = "create" | "edit";

const props = defineProps<{
  userOptions: UserAccount[];
  // 当前租户，用于保存任用记录
  tenantId: number | string;
  // 岗位选项，用于表单选择
  positionOptions: Position[];
}>();

const emit = defineEmits<{
  /**
   * 任用数据发生变更时通知父组件刷新其它依赖（汇报关系、负责人等）
   */
  (e: "employment-changed"): void;
}>();

const tableEmploymentRef = ref<InstanceType<typeof soaTable> | null>(null);

// 任用列表的列配置定义在子组件内部
const employmentColumns = [
  {
    title: "用户",
    dataIndex: "userId",
    key: "userId",
    width: 120,
    resizable: true,
    fixed: true,
  },
  {
    title: "组织",
    dataIndex: "orgId",
    key: "orgId",
    width: 120,
  },
  {
    title: "岗位",
    dataIndex: "positionId",
    key: "positionId",
    width: 180,
  },
  {
    title: "主任用",
    dataIndex: "isPrimary",
    key: "isPrimary",
    width: 80,
    align: "center",
  },
  {
    title: "生效时间",
    dataIndex: "validFrom",
    key: "validFrom",
    width: 175,
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

// 本组件内部根据 props.userOptions / orgTreeOptions / positionOptions 进行格式化
const formatUser = (userId?: string) => {
  if (!userId) return "-";
  const map = new Map<string, UserAccount>();
  props.userOptions.forEach((user) => map.set(user.id, user));
  const user = map.get(userId);
  return user ? user.fullName : `用户 ${userId}`;
};

const formatOrg = (orgId?: string) => {
  if (!orgId) return "-";
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

const formatPosition = (positionId?: string) => {
  if (!positionId) return "-";
  const map = new Map<string, Position>();
  props.positionOptions.forEach((p) => map.set(p.id as any, p));
  const pos = map.get(positionId);
  return pos?.name ?? `岗位 ${positionId}`;
};

// 查询条件（当前仅支持按用户过滤）
const query = reactive<{ userId?: string }>({
  userId: undefined,
});

const tableParams = computed(() => ({
  id: query.userId || undefined,
}));

const getNowString = () => new Date().toISOString().slice(0, 19);

const employmentFormRef = ref<FormInstance>();
const employmentForm = reactive<{
  id?: string;
  tenantId: string;
  userId?: string;
  orgId?: string;
  positionId?: string;
  isPrimary: boolean;
  validFrom: string;
  validTo?: string | null;
  note?: string | null;
}>({
  id: undefined,
  tenantId: "0",
  userId: undefined,
  orgId: undefined,
  positionId: undefined,
  isPrimary: true,
  validFrom: "",
  validTo: undefined,
  note: undefined,
});

const employmentRules: FormRules = {
  userId: [{ required: true, message: "请选择用户", trigger: "change" }],
  orgId: [{ required: true, message: "请选择组织", trigger: "change" }],
  positionId: [{ required: true, message: "请选择岗位", trigger: "change" }],
  validFrom: [{ required: true, message: "请选择生效时间", trigger: "change" }],
};

const dialogMode = ref<DialogMode>("create");
const saving = ref(false);

// 使用 soaModal 管理弹窗：确认 = 提交表单，取消 = 重置表单
const [EmploymentModal, employmentModalApi] = useSoaModal({
  onConfirm: () => {
    handleSubmit();
  },
  onCancel: () => {
    resetForm();
    employmentModalApi.close();
  },
});

const resetForm = () => {
  employmentForm.id = undefined;
  employmentForm.tenantId = String(props.tenantId ?? "0");
  employmentForm.userId = query.userId;
  employmentForm.orgId = undefined;
  employmentForm.positionId = undefined;
  employmentForm.isPrimary = true;
  employmentForm.validFrom = getNowString();
  employmentForm.validTo = null;
  employmentForm.note = undefined;
};

const openCreateEmployment = () => {
  dialogMode.value = "create";
  resetForm();
  employmentModalApi.open();
};

const openEditEmployment = (row: Employment) => {
  dialogMode.value = "edit";
  employmentForm.id = row.id;
  employmentForm.tenantId = row.tenantId ?? String(props.tenantId ?? "0");
  employmentForm.userId = row.userId;
  employmentForm.orgId = row.orgId;
  employmentForm.positionId = row.positionId;
  employmentForm.isPrimary = row.isPrimary;
  employmentForm.validFrom = row.validFrom ?? getNowString();
  employmentForm.validTo = row.validTo ?? null;
  employmentForm.note = row.note ?? undefined;
  employmentModalApi.open();
};

const handleSubmit = () => {
  employmentFormRef.value?.validate(async (valid) => {
    if (!valid) return;
    saving.value = true;
    try {
      const payload: CreateEmploymentInput = {
        tenantId: employmentForm.tenantId,
        userId: employmentForm.userId!,
        orgId: employmentForm.orgId!,
        positionId: employmentForm.positionId!,
        isPrimary: employmentForm.isPrimary,
        validFrom: employmentForm.validFrom,
        validTo: employmentForm.validTo || null,
        note: employmentForm.note || null,
      };

      if (dialogMode.value === "create") {
        await createEmployment(payload);
        ElMessage.success("新增任用成功");
      } else {
        await updateEmployment({
          ...(payload as CreateEmploymentInput),
          id: employmentForm.id!,
        } as UpdateEmploymentInput);
        ElMessage.success("更新任用成功");
      }

      // 同步查询条件中的用户，方便保存后继续查看该用户任用
      if (query.userId !== employmentForm.userId) {
        query.userId = employmentForm.userId;
      }

      employmentModalApi.close();
      resetForm();
      query.userId = undefined;
      tableEmploymentRef.value?.upData(tableParams.value, 1);
      // 任用数据发生变更后，通知父组件刷新依赖（汇报关系、负责人等）
      emit("employment-changed");
    } finally {
      saving.value = false;
    }
  });
};

const handleDeleteEmployment = async (id: string) => {
  await deleteEmployment(id);
  ElMessage.success("删除成功");
  tableEmploymentRef.value?.upData(tableParams.value, 1);
  emit("employment-changed");
};

const handleSearch = () => {
  tableEmploymentRef.value?.upData(tableParams.value, 1);
};

const handleReset = () => {
  query.userId = undefined;
  handleSearch();
};

// 组织树数据与 TreeSelect 配置在子组件内部管理
const orgTreeOptions = ref<any[]>([]);
const treeSelectProps = {
  value: "id",
  label: "name",
  children: "children",
};

const buildOrgTree = (data: OrgUnit[]) => {
  if (!data) return [];
  const map = new Map<string, any>();
  data.forEach((item) => {
    map.set(item.id as any, { ...item, children: [] as any[] });
  });
  const roots: any[] = [];
  data.forEach((item) => {
    const node = map.get(item.id as any);
    const parentId = item.parentId as any;
    if (parentId && map.has(parentId)) {
      map.get(parentId)!.children!.push(node);
    } else {
      roots.push(node);
    }
  });
  return roots;
};

const loadOrgTree = async () => {
  const list = await fetchOrgUnitList({ page: 1, limit: 1000 });
  orgTreeOptions.value = buildOrgTree(list as any);
};

onMounted(async () => {
  await loadOrgTree();
  // 初始时按空条件加载一次列表
  //handleSearch();
});
</script>
