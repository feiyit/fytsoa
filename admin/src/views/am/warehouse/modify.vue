<script setup lang="ts">
import { useSoaModal } from "@/component/soaModal/index.vue";
import {
  addAmWarehouse,
  updateAmWarehouse,
  fetchAmWarehouseById,
} from "@/api/am/warehouse";
import { fetchAmLocationList } from "@/api/am/location";
import { fetchAdminList } from "@/api";
import { generateCode } from "@/utils/tools";

const formRef = ref();
const saving = ref(false);
const emit = defineEmits(["complete"]);

const locationOptions = ref<any[]>([]);
const adminOptions = ref<any[]>([]);

// 仓库编码前缀（支持后续统一调整）
const WAREHOUSE_CODE_PREFIX = "CK-";

const formData = reactive<any>({
  id: "0",
  tenantId: "0",
  code: "",
  name: "",
  locationId: undefined,
  managerId: undefined,
  phone: "",
  status: true,
  summary: "",
});

const rules = {
  code: [{ required: true, message: "请输入仓库编码", trigger: "blur" }],
  name: [{ required: true, message: "请输入仓库名称", trigger: "blur" }],
};

const title = ref("新增仓库");
const [Modal, modalApi] = useSoaModal({
  onConfirm: () => handleSubmit(),
  onCancel: () => resetForm(),
});

const loadOptions = async () => {
  locationOptions.value = await fetchAmLocationList({ status: "1" });
  adminOptions.value = await fetchAdminList({ page: 1, limit: 1000 });
};

onMounted(() => {
  loadOptions();
});

const openModal = async (row?: any) => {
  resetForm();
  await loadOptions();
  const id = row?.id ? String(row.id) : "0";
  title.value = id !== "0" ? "编辑仓库" : "新增仓库";
  if (id !== "0") {
    const res = await fetchAmWarehouseById(id);
    Object.assign(formData, res || {});
  } else if (row) {
    Object.assign(formData, row);
  }
  modalApi.open();
};

const generateWarehouseCode = () => {
  formData.code = generateCode(WAREHOUSE_CODE_PREFIX, 8);
};

const handleSubmit = () => {
  if (saving.value) return;
  formRef.value?.validate(async (valid: any) => {
    if (!valid) return;
    saving.value = true;
    try {
      if (!formData.id || formData.id === "0") {
        await addAmWarehouse(formData);
        ElMessage.success("新增成功");
      } else {
        await updateAmWarehouse(formData);
        ElMessage.success("更新成功");
      }
      emit("complete");
      modalApi.close();
    } finally {
      saving.value = false;
      resetForm();
    }
  });
};

const resetForm = () => {
  Object.assign(formData, {
    id: "0",
    tenantId: "0",
    code: "",
    name: "",
    locationId: undefined,
    managerId: undefined,
    phone: "",
    status: true,
    summary: "",
  });
};

defineExpose({ openModal });
</script>

<template>
  <Modal :title="title" class="w-[800px]" draggable :closeOnClickModal="false">
    <el-form ref="formRef" :model="formData" :rules="rules" label-width="110px">
      <el-row :gutter="12">
        <el-col :span="12">
          <el-form-item label="编码" prop="code">
            <el-input
              v-model="formData.code"
              placeholder="请输入仓库编码"
              clearable
              maxlength="32"
            >
              <template #append>
                <el-button @click="generateWarehouseCode">自动生成</el-button>
              </template>
            </el-input>
          </el-form-item>
        </el-col>
        <el-col :span="12">
          <el-form-item label="名称" prop="name">
            <el-input
              v-model="formData.name"
              placeholder="请输入仓库名称"
              clearable
              maxlength="100"
              show-word-limit
            />
          </el-form-item>
        </el-col>

        <el-col :span="12">
          <el-form-item label="地点">
            <el-select
              v-model="formData.locationId"
              placeholder="请选择地点"
              filterable
              clearable
              style="width: 100%"
            >
              <el-option
                v-for="it in locationOptions"
                :key="it.id"
                :label="it.name"
                :value="it.id"
              />
            </el-select>
          </el-form-item>
        </el-col>

        <el-col :span="12">
          <el-form-item label="管理员">
            <el-select
              v-model="formData.managerId"
              placeholder="请选择管理员"
              filterable
              clearable
              style="width: 100%"
            >
              <el-option
                v-for="it in adminOptions"
                :key="it.id"
                :label="it.fullName || it.loginAccount || it.id"
                :value="it.id"
              />
            </el-select>
          </el-form-item>
        </el-col>

        <el-col :span="12">
          <el-form-item label="电话">
            <el-input
              v-model="formData.phone"
              placeholder="请输入电话"
              clearable
              maxlength="30"
              show-word-limit
            />
          </el-form-item>
        </el-col>

        <el-col :span="12">
          <el-form-item label="状态">
            <el-switch v-model="formData.status" />
          </el-form-item>
        </el-col>

        <el-col :span="24">
          <el-form-item label="备注">
            <el-input
              v-model="formData.summary"
              placeholder="请输入备注"
              type="textarea"
              :rows="3"
              maxlength="500"
              show-word-limit
            />
          </el-form-item>
        </el-col>
      </el-row>
    </el-form>
  </Modal>
</template>
