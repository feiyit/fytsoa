<script setup lang="ts">
import { useSoaModal } from "@/component/soaModal/index.vue";
import {
  addAmVendor,
  updateAmVendor,
  fetchAmVendorById,
} from "@/api/am/vendor";
import { generateCode } from "@/utils/tools";

const formRef = ref();
const saving = ref(false);
const emit = defineEmits(["complete"]);

// 供应商编码前缀（可统一调整）
const VENDOR_CODE_PREFIX = "GYS-";

const formData = reactive<any>({
  id: "0",
  tenantId: "0",
  code: "",
  name: "",
  contactName: "",
  contactPhone: "",
  email: "",
  address: "",
  taxNo: "",
  bankName: "",
  bankAccount: "",
  status: true,
  summary: "",
});

const rules = {
  name: [{ required: true, message: "请输入供应商名称", trigger: "blur" }],
};

const title = ref("新增供应商");
const [Modal, modalApi] = useSoaModal({
  onConfirm: () => handleSubmit(),
  onCancel: () => resetForm(),
});

const generateVendorCode = () => {
  formData.code = generateCode(VENDOR_CODE_PREFIX, 8);
};

const openModal = async (row?: any) => {
  resetForm();
  const id = row?.id ? String(row.id) : "0";
  title.value = id !== "0" ? "编辑供应商" : "新增供应商";
  if (id !== "0") {
    const res = await fetchAmVendorById(id);
    Object.assign(formData, res || {});
  } else if (row) {
    Object.assign(formData, row);
  }
  modalApi.open();
};

const handleSubmit = () => {
  if (saving.value) return;
  formRef.value?.validate(async (valid: any) => {
    if (!valid) return;
    saving.value = true;
    try {
      if (!formData.id || formData.id === "0") {
        await addAmVendor(formData);
        ElMessage.success("新增成功");
      } else {
        await updateAmVendor(formData);
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
    contactName: "",
    contactPhone: "",
    email: "",
    address: "",
    taxNo: "",
    bankName: "",
    bankAccount: "",
    status: true,
    summary: "",
  });
};

defineExpose({ openModal });
</script>

<template>
  <Modal :title="title" class="w-[900px]" draggable :closeOnClickModal="false">
    <el-form ref="formRef" :model="formData" :rules="rules" label-width="110px">
      <el-row :gutter="12">
        <el-col :span="12">
          <el-form-item label="编码">
            <el-input
              v-model="formData.code"
              placeholder="请输入供应商编码"
              clearable
              maxlength="32"
            >
              <template #append>
                <el-button @click="generateVendorCode">自动生成</el-button>
              </template>
            </el-input>
          </el-form-item>
        </el-col>
        <el-col :span="12">
          <el-form-item label="名称" prop="name">
            <el-input
              v-model="formData.name"
              placeholder="请输入供应商名称"
              clearable
              maxlength="100"
              show-word-limit
            />
          </el-form-item>
        </el-col>

        <el-col :span="12">
          <el-form-item label="联系人">
            <el-input
              v-model="formData.contactName"
              placeholder="请输入联系人"
              clearable
              maxlength="50"
              show-word-limit
            />
          </el-form-item>
        </el-col>
        <el-col :span="12">
          <el-form-item label="联系电话">
            <el-input
              v-model="formData.contactPhone"
              placeholder="请输入联系电话"
              clearable
              maxlength="30"
              show-word-limit
            />
          </el-form-item>
        </el-col>

        <el-col :span="12">
          <el-form-item label="邮箱">
            <el-input
              v-model="formData.email"
              placeholder="请输入邮箱"
              clearable
              maxlength="100"
              show-word-limit
            />
          </el-form-item>
        </el-col>
        <el-col :span="12">
          <el-form-item label="税号">
            <el-input
              v-model="formData.taxNo"
              placeholder="请输入税号"
              clearable
              maxlength="50"
              show-word-limit
            />
          </el-form-item>
        </el-col>

        <el-col :span="12">
          <el-form-item label="开户行">
            <el-input
              v-model="formData.bankName"
              placeholder="请输入开户行"
              clearable
              maxlength="100"
              show-word-limit
            />
          </el-form-item>
        </el-col>
        <el-col :span="12">
          <el-form-item label="银行账号">
            <el-input
              v-model="formData.bankAccount"
              placeholder="请输入银行账号"
              clearable
              maxlength="50"
              show-word-limit
            />
          </el-form-item>
        </el-col>

        <el-col :span="24">
          <el-form-item label="地址">
            <el-input
              v-model="formData.address"
              placeholder="请输入地址"
              clearable
              maxlength="200"
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
