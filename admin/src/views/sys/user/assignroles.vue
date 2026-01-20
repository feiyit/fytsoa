<script lang="ts" setup>
import { useSoaModal } from "@/component/soaModal/index.vue";
import { assignAdminRoles, fetchRoleList } from "@/api";
import { ElMessage } from "element-plus";
import { changeTree } from "@/utils/tools";
const adminArr = ref([]);
const treeData = ref([]);
const treeRef = ref();
const [Modal, modalApi] = useSoaModal({
  onConfirm: () => {
    handleSubmit();
  },
  onCancel: () => {
    modalApi.close();
  },
});
onMounted(async () => {});
const openModal = async (row: any) => {
  adminArr.value = row;
  const roles = await fetchRoleList();
  roles.forEach((m) => {
    m.label = m.name;
    m.value = m.id;
  });
  treeData.value = changeTree(roles);
  modalApi.open();
};
const handleSubmit = async () => {
  const selectNodes = treeRef.value!.getCheckedKeys(false);
  await assignAdminRoles({ adminArr: adminArr.value, roleArr: selectNodes });
  ElMessage.success("分配成功");
  modalApi.close();
};
defineExpose({
  openModal,
});
</script>
<template>
  <Modal
    title="分配角色"
    class="w-[600px]"
    draggable
    :closeOnClickModal="false"
  >
    <el-scrollbar max-height="400px">
      <el-tree
        ref="treeRef"
        :data="treeData"
        node-key="id"
        default-expand-all
        show-checkbox
      /> </el-scrollbar
  ></Modal>
</template>
