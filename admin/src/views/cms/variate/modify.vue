<script lang="ts" setup>
import { useSoaModal } from "@/component/soaModal/index.vue";
import { addCmsVariate, updateCmsVariate } from "@/api/cms/variate";
const formData = reactive({
  id: "0",
  group: undefined,
  title: undefined,
  field: undefined,
  fieldType: 1,
  value: undefined,
});
const rules = {
  group: [
    {
      required: true,
      message: "请输入组",
      trigger: "blur",
    },
  ],
  title: [
    {
      required: true,
      message: "请输入标题",
      trigger: "blur",
    },
  ],
  field: [
    {
      required: true,
      message: "请输入字典",
      trigger: "blur",
    },
  ],
  value: [
    {
      required: true,
      message: "请输入值",
      trigger: "blur",
    },
  ],
};
const formRef = ref();
const saving = ref(false);
const emit = defineEmits(["complete"]);
const typeArray = [
  { name: "单行文本", value: 1 },
  { name: "多行文本", value: 2 },
  { name: "单选项", value: 3 },
  { name: "多选项", value: 4 },
  { name: "开关", value: 5 },
  { name: "上传", value: 6 },
  { name: "下拉框", value: 7 },
  { name: "日期", value: 8 },
  { name: "媒体", value: 9 },
  { name: "HTML文本", value: 10 },
  { name: "其他", value: 11 },
];
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
        await addCmsVariate(formData);
        ElMessage.success("新增成功");
      } else {
        await updateCmsVariate(formData);
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
    group: undefined,
    title: undefined,
    field: undefined,
    fieldType: 1,
    value: undefined,
  });
};
defineExpose({
  openModal,
});
</script>
<template>
  <Modal
    title="自定义管理"
    class="w-[650px]"
    draggable
    :closeOnClickModal="false"
    :confirmLoading="saving"
  >
    <el-form ref="formRef" label-width="100px" :model="formData" :rules="rules">
      <el-form-item label="组" prop="group">
        <el-input
          v-model="formData.group"
          placeholder="请输入组"
          :maxlength="30"
          show-word-limit
          clearable
          :style="{ width: '100%' }"
        ></el-input>
      </el-form-item>
      <el-form-item label="字典标题" prop="title">
        <el-input
          v-model="formData.title"
          placeholder="请输入字典标题"
          :maxlength="50"
          show-word-limit
          clearable
          :style="{ width: '100%' }"
        ></el-input>
      </el-form-item>
      <el-form-item label="字典名称" prop="field">
        <el-input
          v-model="formData.field"
          placeholder="请输入字典名称"
          :maxlength="50"
          show-word-limit
          clearable
          :style="{ width: '100%' }"
        ></el-input>
      </el-form-item>
      <el-form-item label="字典类型" prop="fieldType">
        <el-radio-group v-model="formData.fieldType" fill="#409eff">
          <el-radio
            v-for="it in typeArray"
            :label="it.name"
            :value="it.value"
            border
            class="mb-2"
          />
        </el-radio-group>
      </el-form-item>
      <el-form-item label="值" prop="value">
        <el-input
          v-model="formData.value"
          placeholder="请输入值"
          :maxlength="1000"
          show-word-limit
          clearable
          :rows="2"
          type="textarea"
          :style="{ width: '100%' }"
        ></el-input>
      </el-form-item>
      <el-form-item>
        <el-alert
          type="primary"
          show-icon
          :closable="false"
          title="如果字段类型为单择项、多选项、下拉框时，此处填写被选择的项目用英文“,”分隔。如“男,女”"
        ></el-alert>
      </el-form-item>
    </el-form>
  </Modal>
</template>
<style scoped>
:deep(.el-alert__title) {
  font-size: 12px;
}
</style>
