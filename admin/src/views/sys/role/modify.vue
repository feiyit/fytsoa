<script lang="ts" setup>
import { fetchRoleList, createRole, updateRole } from "@/api";
import { useSoaModal } from "@/component/soaModal/index.vue";
import { changeTree } from "@/utils/tools";
const formData = reactive({
  id: "0",
  parentId: undefined,
  name: undefined,
  isSystem: false,
  summary: undefined,
  status: true,
  maxLength: 0,
  sort: 1,
});
const rules = {
  parentId: [
    {
      required: true,
      message: "请选择所属角色",
      trigger: "change",
    },
  ],
  name: [
    {
      required: true,
      message: "请输入角色名称",
      trigger: "blur",
    },
  ],
  status: [
    {
      required: true,
      message: "状态不能为空",
      trigger: "change",
    },
  ],
  summary: [],
};
const statusOptions = [
  {
    label: "正常",
    value: true,
  },
  {
    label: "停用",
    value: false,
  },
];
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
const openModal = (row: any) => {
  if (row) {
    Object.assign(formData, row);
  }
  modalApi.open();
};
const loadRoleList = async () => {
  const list = await fetchRoleList({});
  let _tree = [{ id: "1", value: "0", label: "角色组", parentId: "0" }];
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
  formRef.value?.validate(async (valid) => {
    if (!valid) return;
    saving.value = true;
    try {
      if (formData.id === "0") {
        await createRole(formData);
        ElMessage.success("新增成功");
      } else {
        await updateRole(formData);
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
    parentId: undefined,
    name: undefined,
    isSystem: false,
    summary: undefined,
    status: true,
    maxLength: 0,
    sort: 1,
  });
};
defineExpose({
  openModal,
});
onMounted(async () => {
  await loadRoleList();
});
</script>
<template>
  <Modal
    title="角色"
    class="w-[700px]"
    draggable
    :closeOnClickModal="false"
    :confirmLoading="saving"
  >
    <el-form ref="formRef" label-width="100px" :model="formData" :rules="rules">
      <el-form-item label="所属角色" prop="parentId">
        <el-tree-select
          v-model="formData.parentId"
          placeholder="请选择所属角色"
          :data="parentOptions"
          collapse-tags
          check-strictly
          default-expand-all
          :style="{ width: '100%' }"
        />
      </el-form-item>
      <el-form-item label="角色名称" prop="name">
        <el-input
          v-model="formData.name"
          clearable
          :maxlength="30"
          placeholder="请输入角色名称"
          show-word-limit
          :style="{ width: '100%' }"
        />
      </el-form-item>
      <el-row>
        <el-col :span="12">
          <el-form-item label="角色最大数" prop="maxLength" required>
            <el-input-number
              v-model="formData.maxLength"
              :min="0"
              :max="100"
              controls-position="right"
            />
            <span class="ml-2">0为不限制</span>
          </el-form-item>
        </el-col>
        <el-col :span="12">
          <el-form-item label="状态" prop="status">
            <el-radio-group v-model="formData.status">
              <el-radio
                v-for="(item, index) in statusOptions"
                :key="index"
                :label="item.value"
              >
                {{ item.label }}
              </el-radio>
            </el-radio-group>
          </el-form-item>
        </el-col>
      </el-row>
      <el-form-item label="是否超管" prop="isSystem" required>
        <el-switch
          v-model="formData.isSystem"
          active-text="如果是超管，则不允许删除"
        />
      </el-form-item>
      <el-form-item label="备注" prop="summary">
        <el-input
          v-model="formData.summary"
          :autosize="{ minRows: 2, maxRows: 4 }"
          :maxlength="500"
          placeholder="请输入备注"
          show-word-limit
          :style="{ width: '100%' }"
          type="textarea"
        />
      </el-form-item>
    </el-form>
  </Modal>
</template>
