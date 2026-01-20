<script lang="ts" setup>
import { useSoaModal } from "@/component/soaModal/index.vue";
import {
  fetchSysColumnList,
  addSysColumn,
  updateSysColumn,
} from "@/api/sys/code";
import { changeTree } from "@/utils/tools";
import { ElMessage } from "element-plus";
const formData = reactive({
  id: "0",
  parentId: "0",
  parentIdList: [],
  name: undefined,
  code: undefined,
  isSystem: false,
  sort: 1,
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
      message: "请输入栏位名称",
      trigger: "blur",
    },
  ],
  code: [
    {
      required: true,
      message: "请输入栏位标识",
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
    Object.assign(formData, row);
  }
  await loadRoleList();
  modalApi.open();
};
const loadRoleList = async () => {
  const list = await fetchSysColumnList({ type: 0 });
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
        await addSysColumn(formData);
        ElMessage.success("新增成功");
      } else {
        await updateSysColumn(formData);
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
    code: undefined,
    isSystem: false,
    sort: 1,
  });
};
defineExpose({
  openModal,
});
onMounted(async () => {});
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
          v-model="formData.code"
          placeholder="请输入栏目标识"
          :maxlength="30"
          show-word-limit
          clearable
          :style="{ width: '100%' }"
        ></el-input>
      </el-form-item>

      <el-form-item label="状态" prop="isSystem" required>
        <el-switch
          v-model="formData.isSystem"
          active-text="是否为系统内置集成，如果为是，则不允许删除"
          active-color="#EB5B02"
        ></el-switch>
      </el-form-item>
    </el-form>
  </Modal>
</template>
