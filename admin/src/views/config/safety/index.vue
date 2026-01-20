<template>
  <div>
    <div class="bg-card border-border safety rounded-[.5vw] p-2 pl-5 pr-5">
      <el-tabs @tab-click="tabClick">
        <el-tab-pane label="上传文件白名单">
          <el-alert
            title="设置只允许上传文件的后缀名，格式为：”jpg|gif"
            type="info"
            :closable="false"
            show-icon
          ></el-alert>
          <el-input
            v-model="formData.uploadWhitelist"
            type="textarea"
            placeholder="格式为：”jpg|gig“，以竖线分隔"
          ></el-input>
        </el-tab-pane>
        <el-tab-pane label="IP访问黑名单">
          <el-alert
            title="设置IP黑名单，不设置全部可访问，如设置IP后，设置的IP不可访问（注：设置IP方式为一行一个IP地址）"
            type="info"
            :closable="false"
            show-icon
          ></el-alert>
          <el-input
            v-model="formData.ipBlacklist"
            type="textarea"
            placeholder="设置IP方式为一行一个IP地址"
          ></el-input>
        </el-tab-pane>
        <el-tab-pane label="敏感信息设置">
          <el-alert
            title="敏感关键字（使用英文逗号分隔）"
            type="info"
            :closable="false"
            show-icon
          ></el-alert>
          <el-input
            v-model="formData.sensitivity"
            type="textarea"
            placeholder="请输入内容"
          ></el-input>
        </el-tab-pane>
      </el-tabs>
      <el-button
        type="primary"
        style="margin-top: 10px"
        :loading="isSaveing"
        @click="saveSetting"
      >
        确定保存
      </el-button>
    </div>
  </div>
</template>
<script setup>
import { fetchSafetyFind, addSafety } from "@/api/sys/safety";

const tabIndex = ref(0);
const isSaveing = ref(false);
const formData = ref({
  sensitivity: undefined,
  ipBlacklist: undefined,
  uploadWhitelist: undefined,
});

const tabClick = (index) => {
  tabIndex.value = index;
};
const saveSetting = async () => {
  await addSafety(formData.value);
  ElMessage.success("保存成功！");
};

onMounted(async () => {
  const data = await fetchSafetyFind();
  if (data != null) {
    formData.value = data;
  }
});
</script>
<style scoped>
.safety .el-textarea {
  margin-top: 10px;
  width: 100%;
  min-height: 200px;
}
:deep(.safety .el-textarea__inner) {
  min-height: 200px !important;
}
:deep(.safety .el-tabs--top) {
  width: 100%;
}
</style>
