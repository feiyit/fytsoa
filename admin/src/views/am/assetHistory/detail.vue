<script setup lang="ts">
import { useSoaModal } from "@/component/soaModal/index.vue";

const title = ref("留痕详情");
const formData = ref<any>({});

const [Modal, modalApi] = useSoaModal({
  onConfirm: () => modalApi.close(),
  onCancel: () => modalApi.close(),
  fullscreen: true,
});

const openModal = (row: any) => {
  formData.value = row || {};
  modalApi.open();
};

const pretty = (v: any) => {
  try {
    if (!v) return "";
    const obj = typeof v === "string" ? JSON.parse(v) : v;
    return JSON.stringify(obj, null, 2);
  } catch {
    return String(v ?? "");
  }
};

defineExpose({ openModal });
</script>

<template>
  <Modal :title="title" :closeOnClickModal="false">
    <el-descriptions :column="2" border class="mb-3">
      <el-descriptions-item label="资产Id">{{
        formData.assetId
      }}</el-descriptions-item>
      <el-descriptions-item label="业务类型">{{
        formData.bizType
      }}</el-descriptions-item>
      <el-descriptions-item label="业务Id">{{
        formData.bizId
      }}</el-descriptions-item>
      <el-descriptions-item label="操作">{{
        formData.operation
      }}</el-descriptions-item>
      <el-descriptions-item label="操作人Id">{{
        formData.operatorId
      }}</el-descriptions-item>
      <el-descriptions-item label="操作时间">{{
        formData.operateTime
      }}</el-descriptions-item>
      <el-descriptions-item label="备注" :span="2">{{
        formData.remark
      }}</el-descriptions-item>
    </el-descriptions>

    <el-tabs>
      <el-tab-pane label="变更前(BeforeJson)">
        <el-input
          type="textarea"
          :rows="18"
          :model-value="pretty(formData.beforeJson)"
          readonly
        />
      </el-tab-pane>
      <el-tab-pane label="变更后(AfterJson)">
        <el-input
          type="textarea"
          :rows="18"
          :model-value="pretty(formData.afterJson)"
          readonly
        />
      </el-tab-pane>
    </el-tabs>
  </Modal>
</template>
