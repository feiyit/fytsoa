<template>
  <div class="h-full">
    <div class="mb-3 flex items-center justify-between">
      <div class="flex items-center gap-3">
        <el-input
          v-model="query.key"
          placeholder="岗位名称/编码"
          clearable
          style="width: 220px"
        />
        <el-button type="primary" @click="handleSearch">查询</el-button>
        <el-button @click="handleReset">重置</el-button>
      </div>
      <el-button type="primary" @click="openCreatePosition">
        <el-icon><Plus /></el-icon>
        新增岗位
      </el-button>
    </div>
    <soaTable
      ref="tablePosiionRef"
      :columns="positionColumns"
      :apiObj="fetchPositionPage"
      :params="tableParams"
      :showSelection="false"
      row-key="id"
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
            <el-button
              link
              type="primary"
              @click="openEditPosition(record)"
              >编辑</el-button
            >
            <el-popconfirm
              title="确认删除该岗位？"
              @confirm="handleDeletePosition(record.id)"
            >
              <template #reference>
                <el-button link type="danger">删除</el-button>
              </template>
            </el-popconfirm>
          </div>
        </template>
      </template>
    </soaTable>

    <!-- 岗位维护弹窗：使用 soaModal 封装 -->
    <PositionModal
      title="岗位维护"
      class="w-[480px]"
      :close-on-click-modal="false"
    >
      <el-form
        ref="positionFormRef"
        :model="positionForm"
        :rules="positionRules"
        label-width="96px"
      >
        <el-form-item label="岗位编码" prop="code">
          <el-input
            v-model="positionForm.code"
            placeholder="请输入岗位编码"
            maxlength="64"
          />
        </el-form-item>
        <el-form-item label="岗位名称" prop="name">
          <el-input
            v-model="positionForm.name"
            placeholder="请输入岗位名称"
            maxlength="200"
          />
        </el-form-item>
        <el-form-item label="启用状态" prop="isActive">
          <el-switch v-model="positionForm.isActive" />
        </el-form-item>
      </el-form>
    </PositionModal>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, computed } from "vue";
import { Plus } from "@element-plus/icons-vue";
import { ElMessage, type FormInstance, type FormRules } from "element-plus";
import soaTable from "@/component/soaTable/index.vue";
import { useSoaModal } from "@/component/soaModal/index.vue";
import type {
  Position,
  CreatePositionInput,
  UpdatePositionInput,
} from "@/types/identity";
import {
  fetchPositionPage,
  createPosition,
  updatePosition,
  deletePosition,
} from "@/api";

type DialogMode = "create" | "edit";

const props = defineProps<{
  // 当前租户，用于创建 / 编辑岗位时注入 tenantId
  tenantId: number | string;
}>();

const emit = defineEmits<{
  /**
   * 岗位数据发生变化时通知父组件刷新下拉选项
   */
  (e: "positions-changed"): void;
}>();

const tablePosiionRef = ref<InstanceType<typeof soaTable> | null>(null);

// 岗位列表列配置放在子组件内部，减少父组件干预
const positionColumns = [
  {
    title: "岗位编码",
    dataIndex: "code",
    key: "code",
    width: 150,
    align: "center",
  },
  {
    title: "岗位名称",
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

// 查询条件（当前仅岗位关键字）
const query = reactive<{ key: string }>({
  key: "",
});

// 传递给 soaTable 的查询参数
const tableParams = computed(() => ({
  key: query.key || undefined,
}));

const positionFormRef = ref<FormInstance>();
const positionForm = reactive<
  CreatePositionInput & Partial<UpdatePositionInput>
>({
  id: "" as any,
  tenantId: "",
  code: "",
  name: "",
  isActive: true,
});

const positionRules: FormRules = {
  code: [{ required: true, message: "请输入岗位编码", trigger: "blur" }],
  name: [{ required: true, message: "请输入岗位名称", trigger: "blur" }],
};

const dialogMode = ref<DialogMode>("create");
const saving = ref(false);

// 使用 soaModal 封装岗位弹窗
const [PositionModal, positionModalApi] = useSoaModal({
  onConfirm: () => {
    handleSubmit();
  },
  onCancel: () => {
    resetForm();
    positionModalApi.close();
  },
});

const resetForm = () => {
  positionForm.id = "" as any;
  positionForm.tenantId = String(props.tenantId ?? "");
  positionForm.code = "";
  positionForm.name = "";
  positionForm.isActive = true;
};

const openCreatePosition = () => {
  dialogMode.value = "create";
  resetForm();
  positionModalApi.open();
};

const openEditPosition = (row: Position) => {
  dialogMode.value = "edit";
  positionForm.id = row.id as any;
  positionForm.tenantId = row.tenantId ?? String(props.tenantId ?? "");
  positionForm.code = row.code;
  positionForm.name = row.name;
  positionForm.isActive = row.isActive;
  positionModalApi.open();
};

const handleSubmit = () => {
  positionFormRef.value?.validate(async (valid) => {
    if (!valid) return;
    saving.value = true;
    try {
      const payload: CreatePositionInput = {
        tenantId: String(positionForm.tenantId || props.tenantId || ""),
        code: positionForm.code,
        name: positionForm.name,
        isActive: positionForm.isActive,
      };

      if (dialogMode.value === "create") {
        await createPosition(payload);
        ElMessage.success("新增岗位成功");
      } else {
        await updatePosition({
          ...(payload as any),
          id: positionForm.id as any,
        } as UpdatePositionInput);
        ElMessage.success("更新岗位成功");
      }

      positionModalApi.close();
      resetForm();
      tablePosiionRef.value?.refresh();
      emit("positions-changed");
    } finally {
      saving.value = false;
    }
  });
};

const handleDeletePosition = async (id: string) => {
  await deletePosition(id);
  ElMessage.success("删除成功");
  tablePosiionRef.value?.refresh();
  emit("positions-changed");
};

const handleSearch = () => {
  tablePosiionRef.value?.upData(tableParams.value, 1);
};

const handleReset = () => {
  query.key = "";
  handleSearch();
};
</script>
