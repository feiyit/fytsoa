<script lang="ts" setup>
import { useSoaModal } from "@/component/soaModal/index.vue";
import { addSysCode, updateSysCode } from "@/api/sys/code";
const formData = reactive({
  id: "0",
  tag: 1,
  typeId: 0,
  name: undefined,
  codeValues: undefined,
  status: true,
  sort: 1,
  summary: undefined,
});
const rules = {
  name: [
    {
      required: true,
      message: "请输入字典值名称",
      trigger: "blur",
    },
  ],
  codeValues: [
    {
      required: true,
      message: "请输入字典值阈值",
      trigger: "blur",
    },
  ],
};
const column = ref({});
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
const openModal = async (row: any, columnParam: any) => {
  if (row) {
    Object.assign(formData, row);
  }
  if (columnParam) {
    column.value = column;
    formData.typeId = columnParam.id;
  }
  modalApi.open();
};
const handleSubmit = () => {
  formRef.value?.validate(async (valid: any) => {
    if (!valid) return;
    saving.value = true;
    try {
      if (formData.id === "0") {
        await addSysCode(formData);
        ElMessage.success("新增成功");
      } else {
        await updateSysCode(formData);
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
    tag: 1,
    typeId: 0,
    name: undefined,
    codeValues: undefined,
    status: true,
    sort: 1,
    summary: undefined,
  });
};
defineExpose({
  openModal,
});
</script>
<template>
  <Modal
    title="字典栏目"
    class="w-[600px]"
    draggable
    :closeOnClickModal="false"
    :confirmLoading="saving"
  >
    <el-form ref="formRef" label-width="100px" :model="formData" :rules="rules">
      <el-form-item label="字典名称" prop="name">
        <el-input
          v-model="formData.name"
          placeholder="请输入字典名称"
          :maxlength="30"
          show-word-limit
          clearable
          :style="{ width: '100%' }"
        ></el-input>
      </el-form-item>
      <el-form-item label="字典阈值" prop="codeValues">
        <el-input
          v-model="formData.codeValues"
          placeholder="请输入字典阈值，如A、B"
          :maxlength="30"
          show-word-limit
          clearable
          :style="{ width: '100%' }"
        ></el-input>
      </el-form-item>
      <el-form-item label="状态" prop="status" required>
        <el-switch v-model="formData.status" active-text="是否启用"></el-switch>
      </el-form-item>
      <el-form-item label="排序" prop="sort" required>
        <el-slider
          v-model="formData.sort"
          :max="100"
          :step="1"
          show-input
        ></el-slider>
      </el-form-item>
      <el-form-item label="备注">
        <el-input
          v-model="formData.summary"
          type="textarea"
          placeholder="请输入备注"
          :maxlength="100"
          show-word-limit
          :autosize="{ minRows: 2, maxRows: 4 }"
          :style="{ width: '100%' }"
        ></el-input>
      </el-form-item>
    </el-form>
  </Modal>
</template>
