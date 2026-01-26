<script setup lang="ts">
import { useSoaModal } from "@/component/soaModal/index.vue";
import {
  addAmWarehouseBin,
  updateAmWarehouseBin,
  fetchAmWarehouseBinById,
} from "@/api/am/warehouseBin";
import { generateCode } from "@/utils/tools";

const props = defineProps({
  warehouseOptions: { type: Array as any, default: () => [] },
});

const formRef = ref();
const saving = ref(false);
const emit = defineEmits(["complete"]);

// 仓位编码前缀（可统一调整）
const WAREHOUSE_BIN_CODE_PREFIX = "CW-";

const formData = reactive<any>({
  id: "0",
  tenantId: "0",
  warehouseId: undefined,
  code: "",
  name: "",
  sort: 0,
  status: true,
});

const rules = {
  warehouseId: [{ required: true, message: "请选择仓库", trigger: "change" }],
  name: [{ required: true, message: "请输入仓位名称", trigger: "blur" }],
};

const title = ref("新增仓位");
const [Modal, modalApi] = useSoaModal({
  onConfirm: () => handleSubmit(),
  onCancel: () => resetForm(),
});

const openModal = async (row?: any) => {
  resetForm();
  const id = row?.id ? String(row.id) : "0";
  title.value = id !== "0" ? "编辑仓位" : "新增仓位";
  if (id !== "0") {
    const res = await fetchAmWarehouseBinById(id);
    Object.assign(formData, res || {});
  } else if (row) {
    Object.assign(formData, row);
  }
  modalApi.open();
};

const generateWarehouseBinCode = () => {
  formData.code = generateCode(WAREHOUSE_BIN_CODE_PREFIX, 8);
};

const handleSubmit = () => {
  if (saving.value) return;
  formRef.value?.validate(async (valid: any) => {
    if (!valid) return;
    saving.value = true;
    try {
      if (!formData.id || formData.id === "0") {
        await addAmWarehouseBin(formData);
        ElMessage.success("新增成功");
      } else {
        await updateAmWarehouseBin(formData);
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
    warehouseId: undefined,
    code: "",
    name: "",
    sort: 0,
    status: true,
  });
};

defineExpose({ openModal });
</script>

<template>
  <Modal :title="title" class="w-[800px]" draggable :closeOnClickModal="false">
    <el-form ref="formRef" :model="formData" :rules="rules" label-width="110px">
      <el-row :gutter="12">
        <el-col :span="24">
          <el-form-item label="所属仓库" prop="warehouseId">
            <el-select
              v-model="formData.warehouseId"
              placeholder="请选择仓库"
              filterable
              clearable
              style="width: 100%"
            >
              <el-option
                v-for="it in props.warehouseOptions"
                :key="it.id"
                :label="it.name"
                :value="it.id"
              />
            </el-select>
          </el-form-item>
        </el-col>

        <el-col :span="12">
          <el-form-item label="编码">
            <el-input
              v-model="formData.code"
              placeholder="请输入仓位编码"
              clearable
              maxlength="32"
            >
              <template #append>
                <el-button @click="generateWarehouseBinCode"
                  >自动生成</el-button
                >
              </template>
            </el-input>
          </el-form-item>
        </el-col>

        <el-col :span="12">
          <el-form-item label="名称" prop="name">
            <el-input
              v-model="formData.name"
              placeholder="请输入仓位名称"
              clearable
              maxlength="100"
              show-word-limit
            />
          </el-form-item>
        </el-col>

        <el-col :span="12">
          <el-form-item label="排序">
            <el-input-number
              v-model="formData.sort"
              :min="0"
              style="width: 100%"
            />
          </el-form-item>
        </el-col>

        <el-col :span="12">
          <el-form-item label="状态">
            <el-switch v-model="formData.status" />
          </el-form-item>
        </el-col>
      </el-row>
    </el-form>
  </Modal>
</template>
