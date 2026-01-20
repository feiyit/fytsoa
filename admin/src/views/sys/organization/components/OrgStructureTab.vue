<template>
  <div class="h-full">
    <div class="mb-3 flex justify-end">
      <el-button type="primary" @click="openCreateOrg">
        <el-icon><Plus /></el-icon>
        新增组织
      </el-button>
    </div>
    <soaTable
      ref="tableOrgRef"
      :columns="orgColumns"
      :apiObj="fetchOrgUnitList"
      :paginationShow="false"
      :showSelection="false"
      row-key="id"
      tree
      row-serial-number
    >
      <template #bodyCell="{ column, record }">
        <template v-if="column.key === 'isActive'">
          <el-tag :type="record.isActive ? 'success' : 'danger'">
            {{ record.isActive ? "启用" : "禁用" }}
          </el-tag>
        </template>
        <template v-else-if="column.key === 'action'">
          <div class="flex items-center gap-1">
            <el-button link type="primary" @click="openEditOrg(record)">
              编辑
            </el-button>
            <el-popconfirm
              title="确认删除该机构？"
              @confirm="handleDeleteOrg(record.id)"
            >
              <template #reference>
                <el-button link type="danger">删除</el-button>
              </template>
            </el-popconfirm>
          </div>
        </template>
      </template>
    </soaTable>

    <!-- 组织维护弹窗：使用 soaModal 封装，内部自管理表单和提交逻辑 -->
    <OrgModal
      title="组织维护"
      class="w-[520px]"
      :close-on-click-modal="false"
    >
      <el-form
        ref="orgFormRef"
        :model="orgForm"
        :rules="orgRules"
        label-width="100px"
      >
        <el-form-item label="上级组织" prop="parentId">
          <el-tree-select
            v-model="orgForm.parentId"
            :data="orgTreeOptions"
            :props="treeSelectProps"
            check-strictly
            default-expand-all
            placeholder="选择上级组织"
            clearable
            class="w-full"
          />
        </el-form-item>
        <el-form-item label="组织编码" prop="code">
          <el-input
            v-model="orgForm.code"
            placeholder="请输入组织编码"
            maxlength="64"
          />
        </el-form-item>
        <el-form-item label="组织名称" prop="name">
          <el-input
            v-model="orgForm.name"
            placeholder="请输入组织名称"
            maxlength="200"
          />
        </el-form-item>
        <el-form-item label="启用状态" prop="isActive">
          <el-switch v-model="orgForm.isActive" />
        </el-form-item>
      </el-form>
    </OrgModal>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted } from "vue";
import { Plus } from "@element-plus/icons-vue";
import { ElMessage, type FormInstance, type FormRules } from "element-plus";
import soaTable from "@/component/soaTable/index.vue";
import { useSoaModal } from "@/component/soaModal/index.vue";
import type {
  OrgUnit,
  CreateOrgUnitInput,
  UpdateOrgUnitInput,
} from "@/types/identity";
import { fetchOrgUnitList, createOrgUnit, updateOrgUnit, deleteOrgUnit } from "@/api";

type DialogMode = "create" | "edit";

const props = defineProps<{
  // 当前租户，用于创建 / 编辑组织时注入 tenantId
  tenantId: number | string;
}>();

const emit = defineEmits<{
  /**
   * 组织数据发生变化时通知父组件刷新缓存树
   */
  (e: "refresh-tree"): void;
}>();

const tableOrgRef = ref<InstanceType<typeof soaTable> | null>(null);

// 组织列表列配置：只在本组件中使用，放在子组件内部更清晰
const orgColumns = [
  {
    title: "机构编码",
    dataIndex: "code",
    key: "code",
    width: 150,
    align: "center",
  },
  {
    title: "机构名称",
    dataIndex: "name",
    key: "name",
    resizable: true,
    fixed: true,
  },
  {
    title: "状态",
    dataIndex: "isActive",
    key: "isActive",
    width: 80,
    align: "center",
  },
  {
    title: "创建时间",
    dataIndex: "createdAt",
    key: "createdAt",
    width: 175,
  },
  {
    title: "操作",
    key: "action",
    width: 100,
    fixed: "right",
  },
];

// 弹窗里用到的组织树与 tree-select 配置也放到子组件内部
const orgTreeOptions = ref<any[]>([]);
const treeSelectProps = {
  value: "id",
  label: "name",
  children: "children",
};

const orgFormRef = ref<FormInstance>();
const orgForm = reactive<CreateOrgUnitInput & Partial<UpdateOrgUnitInput>>({
  id: "" as any,
  tenantId: "",
  code: "",
  name: "",
  parentId: undefined,
  isActive: true,
});

const orgRules: FormRules = {
  code: [{ required: true, message: "请输入组织编码", trigger: "blur" }],
  name: [{ required: true, message: "请输入组织名称", trigger: "blur" }],
};

const dialogMode = ref<DialogMode>("create");
const saving = ref(false);

// 使用 soaModal 封装弹窗行为：确认 = 提交表单，取消 = 重置表单
const [OrgModal, orgModalApi] = useSoaModal({
  onConfirm: () => {
    handleSubmit();
  },
  onCancel: () => {
    resetForm();
    orgModalApi.close();
  },
});

// 根据平铺的组织列表构建树结构（与父组件中逻辑保持一致）
const buildTree = (data: OrgUnit[]) => {
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
  orgTreeOptions.value = buildTree(list);
};

const resetForm = () => {
  orgForm.id = "" as any;
  orgForm.tenantId = String(props.tenantId ?? "");
  orgForm.code = "";
  orgForm.name = "";
  orgForm.parentId = undefined;
  orgForm.isActive = true;
};

const openCreateOrg = () => {
  dialogMode.value = "create";
  resetForm();
  orgModalApi.open();
};

const openEditOrg = (row: OrgUnit) => {
  dialogMode.value = "edit";
  orgForm.id = row.id as any;
  orgForm.tenantId = row.tenantId ?? String(props.tenantId ?? "");
  orgForm.code = row.code;
  orgForm.name = row.name;
  orgForm.parentId = row.parentId;
  orgForm.isActive = row.isActive;
  orgModalApi.open();
};

const handleSubmit = () => {
  orgFormRef.value?.validate(async (valid) => {
    if (!valid) return;
    saving.value = true;
    try {
      const payload: CreateOrgUnitInput = {
        tenantId: String(orgForm.tenantId || props.tenantId || ""),
        code: orgForm.code,
        name: orgForm.name,
        parentId: orgForm.parentId,
        isActive: orgForm.isActive,
      };

      if (dialogMode.value === "create") {
        await createOrgUnit(payload);
        ElMessage.success("新增组织成功");
      } else {
        await updateOrgUnit({
          ...(payload as any),
          id: orgForm.id as any,
        } as UpdateOrgUnitInput);
        ElMessage.success("更新组织成功");
      }

      orgModalApi.close();
      resetForm();
      tableOrgRef.value?.refresh();
      // 本地和父级缓存的组织树都刷新
      await loadOrgTree();
      emit("refresh-tree");
    } finally {
      saving.value = false;
    }
  });
};

const handleDeleteOrg = async (id: string) => {
  await deleteOrgUnit(id);
  ElMessage.success("删除成功");
  tableOrgRef.value?.refresh();
  await loadOrgTree();
  emit("refresh-tree");
};

onMounted(async () => {
  await loadOrgTree();
});
</script>
