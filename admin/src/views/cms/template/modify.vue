<script lang="ts" setup>
import { useSoaModal } from "@/component/soaModal/index.vue";
import { addCmsTemplate, updateCmsTemplate } from "@/api/cms/template";
const formData = reactive({
  id: "0",
  name: undefined,
  urls: undefined,
  status: true,
});
const rules = {
  name: [
    {
      required: true,
      message: "请输入模板名称",
      trigger: "blur",
    },
  ],
  urls: [
    {
      required: true,
      message: "请输入模板地址",
      trigger: "blur",
    },
  ],
};
const formRef = ref();
const saving = ref(false);
const emit = defineEmits(["complete"]);
const [Modal, modalApi] = useSoaModal({
  onConfirm: () => {
    handleSubmit();
  },
  onCancel: () => {
    resetForm();
    modalApi.close();
  },
});

const openModal = async (row: any) => {
  if (row) {
    Object.assign(formData, row);
  }
  modalApi.open();
};
const handleSubmit = () => {
  formRef.value?.validate(async (valid: any) => {
    if (!valid) return;
    saving.value = true;
    try {
      if (formData.id === "0") {
        await addCmsTemplate(formData);
        ElMessage.success("新增成功");
      } else {
        await updateCmsTemplate(formData);
        ElMessage.success("更新成功");
      }
      emit("complete");
      modalApi.close();
    } finally {
      resetForm();
      saving.value = false;
    }
  });
};
const resetForm = () => {
  Object.assign(formData, {
    id: "0",
    name: undefined,
    urls: undefined,
    status: true,
  });
};
defineExpose({
  openModal,
});
</script>
<template>
  <Modal
    title="模版管理"
    class="w-[650px]"
    draggable
    :closeOnClickModal="false"
    :confirmLoading="saving"
  >
    <el-form ref="formRef" label-width="100px" :model="formData" :rules="rules">
      <el-form-item label="模板名称" prop="name">
        <el-input
          v-model="formData.name"
          placeholder="请输入模板名称"
          :maxlength="30"
          show-word-limit
          clearable
          :style="{ width: '100%' }"
        ></el-input>
      </el-form-item>
      <el-form-item label="模板地址" prop="urls">
        <el-input
          v-model="formData.urls"
          placeholder="请输入模板地址"
          :maxlength="100"
          show-word-limit
          clearable
          :style="{ width: '100%' }"
        ></el-input>
      </el-form-item>
      <el-form-item label="可用状态" prop="status" required>
        <el-switch v-model="formData.status"></el-switch>
      </el-form-item>
    </el-form>
  </Modal>
</template>
