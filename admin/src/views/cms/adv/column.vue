<script lang="ts" setup>
import { useSoaModal } from "@/component/soaModal/index.vue";
import {
  fetchAdvColumnList,
  addAdvColumn,
  updateAdvColumn,
  fetchAdvColumnById,
} from "@/api/cms/adv";
import { changeTree } from "@/utils/tools";
const formData = reactive({
  id: "0",
  parentId: "0",
  parentIdList: [],
  name: undefined,
  flag: undefined,
  width: undefined,
  height: undefined,
  status: true,
  summary: undefined,
});
const rules = {
  parentId: [
    {
      required: true,
      message: "请选择上级栏目",
      trigger: "blur",
    },
  ],
  name: [
    {
      required: true,
      message: "请输入栏目名称",
      trigger: "blur",
    },
  ],
  flag: [
    {
      required: true,
      message: "请选择广告类型",
      trigger: "change",
    },
  ],
  width: [
    {
      required: true,
      message: "请输入宽度",
      trigger: "blur",
    },
    {
      pattern: /^\d+$/,
      message: "必须为数字",
      trigger: "blur",
    },
  ],
  height: [
    {
      required: true,
      message: "请输入高度",
      trigger: "blur",
    },
    {
      pattern: /^\d+$/,
      message: "必须为数字",
      trigger: "blur",
    },
  ],
};
const parentOptions = ref([]);
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
    const resData = await fetchAdvColumnById(row.id);
    Object.assign(formData, resData);
  }
  await loadRoleList();
  modalApi.open();
};
const loadRoleList = async () => {
  const list = await fetchAdvColumnList({ type: 0 });
  let _tree = [{ id: "1", value: "0", label: "一级栏目", parentId: "0" }];
  list.some((m) => {
    _tree.push({
      id: m.id,
      value: m.id,
      label: m.name,
      parentId: m.parentId,
    });
  });

  parentOptions.value = changeTree(_tree);
};
const handleSubmit = () => {
  formRef.value?.validate(async (valid: any) => {
    if (!valid) return;
    saving.value = true;
    try {
      if (formData.id === "0") {
        await addAdvColumn(formData);
        ElMessage.success("新增成功");
      } else {
        await updateAdvColumn(formData);
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
    parentId: "0",
    parentIdList: [],
    name: undefined,
    flag: undefined,
    width: undefined,
    height: undefined,
    status: true,
    summary: undefined,
  });
};
defineExpose({
  openModal,
});
onMounted(async () => {});
</script>
<template>
  <Modal
    title="广告栏目"
    class="w-[700px]"
    draggable
    :closeOnClickModal="false"
    :confirmLoading="saving"
  >
    <el-form ref="formRef" label-width="100px" :model="formData" :rules="rules">
      <el-form-item label="上级栏目" prop="parentId">
        <el-tree-select
          placeholder="请选择上级栏目"
          clearable
          filterable
          default-expand-all
          :check-strictly="true"
          highlight-current
          :indent="24"
          v-model="formData.parentId"
          :data="parentOptions"
          :style="{ width: '100%' }"
        />
      </el-form-item>
      <el-form-item label="栏目名称" prop="name">
        <el-input
          v-model="formData.name"
          placeholder="请输入栏目名称"
          :maxlength="30"
          show-word-limit
          clearable
          :style="{ width: '100%' }"
        ></el-input>
      </el-form-item>
      <el-form-item label="栏目标识" prop="code">
        <el-input
          v-model="formData.flag"
          placeholder="请输入栏目标识"
          :maxlength="30"
          show-word-limit
          clearable
          :style="{ width: '100%' }"
        ></el-input>
      </el-form-item>
      <el-row>
        <el-col :span="12">
          <el-form-item label="广告位宽度" prop="width">
            <el-input
              v-model="formData.width"
              placeholder="请输入宽度，例如100px或100"
              :maxlength="10"
              show-word-limit
              clearable
              :style="{ width: '100%' }"
            ></el-input>
          </el-form-item>
        </el-col>
        <el-col :span="12">
          <el-form-item label="广告位高度" prop="height">
            <el-input
              v-model="formData.height"
              placeholder="请输入高度，例如100px或100"
              :maxlength="10"
              show-word-limit
              clearable
              :style="{ width: '100%' }"
            ></el-input>
          </el-form-item>
        </el-col>
      </el-row>
      <el-form-item label="状态" prop="status" required>
        <el-switch v-model="formData.status"></el-switch>
      </el-form-item>
      <el-form-item label="广告位说明" prop="summary">
        <el-input
          v-model="formData.summary"
          type="textarea"
          placeholder="简要说明一下，广告的位置，作用等。"
          :maxlength="500"
          show-word-limit
          :autosize="{ minRows: 3, maxRows: 3 }"
          :style="{ width: '100%' }"
        ></el-input>
      </el-form-item>
    </el-form>
  </Modal>
</template>
