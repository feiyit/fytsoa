<template>
  <el-form ref="formRef" label-width="100px" :model="formData" :rules="rules">
    <el-tabs v-model="activeName">
      <el-tab-pane label="基本信息" name="first">
        <el-form-item label="网站名称" prop="name">
          <el-input
            v-model="formData.name"
            placeholder="请输入网站名称"
            :maxlength="32"
            show-word-limit
            clearable
          ></el-input>
        </el-form-item>
        <el-form-item label="网站Logo" prop="logo">
          <soa-upload
            v-model="formData.logo"
            action="/sysfile/upload?path=/upload/site/"
            type="image"
            :file-size="5"
            :limit="1"
          />
        </el-form-item>
        <el-form-item label="网站网址" prop="siteUrl">
          <el-input
            v-model="formData.siteUrl"
            placeholder="请输入网站网址"
            :maxlength="128"
            show-word-limit
            clearable
          ></el-input>
        </el-form-item>
        <el-form-item label="SEO标题" prop="seoTitle">
          <el-input
            v-model="formData.seoTitle"
            placeholder="请输入SEO标题"
            :maxlength="128"
            show-word-limit
            clearable
          ></el-input>
        </el-form-item>
        <el-form-item label="SEO关键字" prop="seoKey">
          <el-input
            v-model="formData.seoKey"
            placeholder="请输入SEO关键字"
            type="textarea"
            :maxlength="512"
            show-word-limit
            clearable
          ></el-input>
        </el-form-item>
        <el-form-item label="SEO描述" prop="seoDescribe">
          <el-input
            v-model="formData.seoDescribe"
            placeholder="请输入SEO描述"
            type="textarea"
            :maxlength="512"
            show-word-limit
            clearable
          ></el-input>
        </el-form-item>
        <el-form-item label="网站版权信息" prop="copyright">
          <el-input
            v-model="formData.copyright"
            placeholder="请输入网站版权信息"
            type="textarea"
            :maxlength="1024"
            show-word-limit
            clearable
          ></el-input>
        </el-form-item>
        <el-form-item label="网站开启状态" prop="status">
          <el-switch v-model="formData.status"></el-switch>
        </el-form-item>
        <el-form-item label="网站关闭原因" prop="closeInfo">
          <el-input
            v-model="formData.closeInfo"
            placeholder="请输入网站关闭原因"
            type="textarea"
            :maxlength="1024"
            show-word-limit
            clearable
          ></el-input>
        </el-form-item>
      </el-tab-pane>
      <el-tab-pane label="公司信息" name="second">
        <el-form-item label="公司电话" prop="companyTel">
          <el-input
            v-model="formData.companyTel"
            placeholder="请输入公司电话"
            :maxlength="32"
            show-word-limit
            clearable
          ></el-input>
        </el-form-item>
        <el-form-item label="公司传真" prop="companyFax">
          <el-input
            v-model="formData.companyFax"
            placeholder="请输入公司传真"
            :maxlength="32"
            show-word-limit
            clearable
          ></el-input>
        </el-form-item>
        <el-form-item label="公司邮箱" prop="companyEmail">
          <el-input
            v-model="formData.companyEmail"
            placeholder="请输入公司邮箱"
            :maxlength="32"
            show-word-limit
            clearable
          ></el-input>
        </el-form-item>
        <el-form-item label="公司地址" prop="companyAddress">
          <el-input
            v-model="formData.companyAddress"
            placeholder="请输入公司地址"
            :maxlength="64"
            show-word-limit
            clearable
          ></el-input>
        </el-form-item>
        <el-form-item label="客服信息" prop="customerService">
          <el-input
            v-model="formData.customerService"
            placeholder="请输入客服信息"
            :maxlength="128"
            show-word-limit
            clearable
          ></el-input>
        </el-form-item>
      </el-tab-pane>
      <el-tab-pane label="二维码" name="third">
        <el-form-item label="二维码" prop="codes">
          <soa-upload
            v-model="formData.codes"
            action="/sysfile/upload?path=/upload/site/"
            type="image"
            :file-size="5"
            :limit="1"
          />
        </el-form-item>
      </el-tab-pane>
      <el-tab-pane label="扩展" name="extend">
        <soaFormTable v-model="formData.extend" :addTemplate="addTemplate">
          <el-table-column label="前置" width="180">
            <template #default="scope">
              <el-input
                v-model="scope.row.key"
                style="width: 150px"
                placeholder="例如：姓名"
              />
            </template>
          </el-table-column>
          <el-table-column label="值">
            <template #default="scope">
              <el-input
                v-model="scope.row.value"
                style="width: 100%"
                placeholder="例如：张三"
              />
            </template>
          </el-table-column>
        </soaFormTable>
      </el-tab-pane>
    </el-tabs>
  </el-form>
  <div>
    <el-button @click="resetForm">重 置</el-button>
    <el-button :loading="saving" type="primary" @click="handleSubmit">
      确 定
    </el-button>
  </div>
</template>
<script lang="ts" setup>
import soaUpload from "@/component/soaUpload/index.vue";
import { addCmsSite, fetchCmsSiteById, updateCmsSite } from "@/api/cms/site";
const addTemplate = {
  key: undefined,
  values: undefined,
};
const activeName = ref("first");
const formData = reactive({
  id: "0",
  name: "",
  logo: "",
  siteUrl: "",
  seoTitle: "",
  seoKey: "",
  seoDescribe: "",
  copyright: "",
  status: false,
  closeInfo: "",
  companyTel: "",
  companyFax: "",
  companyEmail: "",
  companyAddress: "",
  customerService: "",
  codes: "",
  extend: [],
});
const rules = {
  name: [
    {
      required: true,
      message: "请输入站点名称",
      trigger: "blur",
    },
  ],
};
const formRef = ref();
const saving = ref(false);
const emit = defineEmits(["complete"]);

const openModal = async (row: any) => {
  if (row) {
    const site = await fetchCmsSiteById(row);
    Object.assign(formData, site);
  } else {
    resetForm();
  }
};
const handleSubmit = () => {
  formRef.value?.validate(async (valid: any) => {
    if (!valid) return;
    saving.value = true;
    try {
      if (formData.id === "0") {
        await addCmsSite(formData);
        ElMessage.success("新增成功");
      } else {
        await updateCmsSite(formData);
        ElMessage.success("更新成功");
      }
      emit("complete");
    } finally {
      //resetForm();
      saving.value = false;
    }
  });
};
const resetForm = () => {
  Object.assign(formData, {
    id: "0",
    name: "",
    logo: "",
    siteUrl: "",
    seoTitle: "",
    seoKey: "",
    seoDescribe: "",
    copyright: "",
    status: false,
    closeInfo: "",
    companyTel: "",
    companyFax: "",
    companyEmail: "",
    companyAddress: "",
    customerService: "",
    codes: "",
    extend: [],
  });
};
defineExpose({
  openModal,
});
</script>
