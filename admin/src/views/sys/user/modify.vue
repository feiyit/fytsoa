<script lang="ts" setup>
import { useSoaModal } from "@/component/soaModal/index.vue";
import {
  createAdmin,
  updateUser,
  fetchOrgUnitList,
  fetchPositionList,
  fetchRoleList,
  fetchAdminById,
} from "@/api";
import { changeTree } from "@/utils/tools";
import { md5Encrypt } from "@/utils/index";
const formData = reactive({
  id: 0,
  organizeId: undefined,
  organizeIdList: [],
  loginAccount: "",
  roleGroup: undefined,
  loginPassWord: "",
  postGroup: [],
  avatar: undefined,
  fullName: "",
  mobile: "",
  sex: "男",
  email: "",
  status: true,
  summary: "",
  loginCount: 0,
});
const rules = {
  organizeId: [
    {
      required: true,
      message: "请选择所属部门",
      trigger: "change",
    },
  ],
  loginAccount: [
    {
      required: true,
      message: "请输入登录账号",
      trigger: "blur",
    },
  ],
  roleGroup: [
    {
      required: true,
      message: "请择选所属角色",
      trigger: "change",
    },
  ],
  loginPassWord: [
    {
      required: true,
      message: "请输入登录密码",
      trigger: "blur",
    },
  ],
  postGroup: [
    {
      required: true,
      message: "请选择所属岗位",
      trigger: "change",
    },
  ],
  fullName: [
    {
      required: true,
      message: "请输入姓名",
      trigger: "blur",
    },
  ],
  mobile: [
    {
      required: true,
      message: "请输入手机号码",
      trigger: "blur",
    },
  ],
  sex: [
    {
      required: true,
      message: "性别不能为空",
      trigger: "change",
    },
  ],
  email: [],
  summary: [],
};
const [Modal, modalApi] = useSoaModal({
  onConfirm: () => {
    handleSubmit();
  },
  onCancel: () => {
    resetForm();
    modalApi.close();
  },
});
const title = ref("添加");
const imageFiles = ref("");
const formRef = ref();
const saving = ref(false);
const emit = defineEmits(["complete"]);
const organizeOptions = ref([]);
const roleOptions = ref([]);
const postOptions = ref([]);
const openModal = async (row: any) => {
  title.value = row.id ? "修改" : "添加";
  if (row.id) {
    const user = await fetchAdminById(row.id);
    Object.assign(formData, user);
  }

  modalApi.open();
};
const sexOptions = [
  {
    label: "男",
    value: "男",
  },
  {
    label: "女",
    value: "女",
  },
];
onMounted(async () => {
  const org = await fetchOrgUnitList({
    page: 1,
    limit: 1000,
  });
  org.forEach((m) => {
    m.label = m.name;
    m.value = m.id;
  });
  organizeOptions.value = changeTree(org);
  const role = await fetchRoleList({});
  role.forEach((m) => {
    m.label = m.name;
    m.value = m.id;
  });
  roleOptions.value = changeTree(role);
  const postRes = await fetchPositionList();
  postRes.forEach((m) => {
    m.label = m.name;
    m.value = m.id;
  });
  postOptions.value = postRes;
});
const handleSubmit = () => {
  formRef.value?.validate(async (valid: any) => {
    if (!valid) return;
    saving.value = true;
    try {
      formData.loginPassWord = md5Encrypt(formData.loginPassWord);
      if (formData.id === 0) {
        await createAdmin(formData);
        ElMessage.success("新增成功");
      } else {
        await updateUser(formData);
        ElMessage.success("更新成功");
      }
      emit("complete");
      resetForm();
      modalApi.close();
    } finally {
      saving.value = false;
    }
  });
};
const resetForm = () => {
  Object.assign(formData, {
    id: 0,
    organizeId: undefined,
    organizeIdList: [],
    loginAccount: "",
    roleGroup: undefined,
    loginPassWord: "",
    postGroup: [],
    avatar: undefined,
    fullName: "",
    mobile: "",
    sex: "男",
    email: "",
    status: true,
    summary: "",
    loginCount: 0,
  });
};
defineExpose({
  openModal,
});
</script>
<template>
  <Modal :title="title" class="w-[900px]" draggable :closeOnClickModal="false">
    <el-container>
      <el-aside width="180px" class="pl-3 pt-2">
        <soa-upload
          v-model="formData.avatar"
          directory="avatar"
          type="image"
          :file-size="2"
          :limit="1"
          :width="150"
          :height="150"
        />
        <p class="m-2 text-xs">请选择头像文件上传</p>
        <!-- <p class="last-login">上次登录：{{ formData.upLoginTime }}</p> -->
      </el-aside>
      <el-container>
        <el-form
          ref="formRef"
          label-width="100px"
          :model="formData"
          :rules="rules"
        >
          <el-row>
            <el-col :span="12">
              <el-form-item label="所属部门" prop="organizeId">
                <el-tree-select
                  v-model="formData.organizeId"
                  placeholder="请选择所属部门"
                  :data="organizeOptions"
                  default-expand-all
                  check-strictly
                  :style="{ width: '100%' }"
                />
              </el-form-item>
            </el-col>
            <el-col :span="12">
              <el-form-item label="所属角色" prop="roleGroup">
                <el-tree-select
                  v-model="formData.roleGroup"
                  placeholder="请选择所属角色"
                  :data="roleOptions"
                  multiple
                  collapse-tags
                  check-strictly
                  default-expand-all
                  :style="{ width: '100%' }"
                />
              </el-form-item>
            </el-col>
            <el-col :span="12">
              <el-form-item label="登录账号" prop="loginAccount">
                <el-input
                  v-model="formData.loginAccount"
                  placeholder="请输入登录账号"
                  :maxlength="30"
                  show-word-limit
                  clearable
                  :style="{ width: '100%' }"
                ></el-input>
              </el-form-item>
            </el-col>

            <el-col :span="12">
              <el-form-item label="登录密码" prop="loginPassWord">
                <el-input
                  v-model="formData.loginPassWord"
                  placeholder="请输入登录密码"
                  :maxlength="30"
                  :disabled="formData.id != 0"
                  show-word-limit
                  clearable
                  show-password
                  :style="{ width: '100%' }"
                ></el-input>
              </el-form-item>
            </el-col>

            <el-col :span="12">
              <el-form-item label="姓名" prop="fullName">
                <el-input
                  v-model="formData.fullName"
                  placeholder="请输入姓名"
                  :maxlength="20"
                  clearable
                  :style="{ width: '100%' }"
                ></el-input>
              </el-form-item>
            </el-col>
            <el-col :span="12">
              <el-form-item label="手机号码" prop="mobile">
                <el-input
                  v-model="formData.mobile"
                  placeholder="请输入手机号码"
                  :maxlength="11"
                  clearable
                  :style="{ width: '100%' }"
                ></el-input>
              </el-form-item>
            </el-col>
            <el-col :span="12">
              <el-form-item label="所属岗位" prop="postGroup">
                <soa-select
                  v-model="formData.postGroup"
                  :data="postOptions"
                  placeholder="请选择所属岗位"
                  clearable
                  filterable
                  multiple
                  :multipleLimit="1"
                  style="width: 100%"
                ></soa-select>
              </el-form-item>
            </el-col>
            <el-col :span="12">
              <el-form-item label="性别" prop="sex">
                <el-radio-group v-model="formData.sex">
                  <el-radio
                    v-for="(item, index) in sexOptions"
                    :key="index"
                    :label="item.value"
                    :disabled="item.disabled"
                  >
                    {{ item.label }}
                  </el-radio>
                </el-radio-group>
              </el-form-item>
            </el-col>
            <el-col :span="12">
              <el-form-item label="邮箱" prop="email">
                <el-input
                  v-model="formData.email"
                  placeholder="请输入邮箱"
                  :maxlength="50"
                  show-word-limit
                  clearable
                  :style="{ width: '100%' }"
                ></el-input>
              </el-form-item>
            </el-col>
            <el-col :span="12">
              <el-form-item label="状态" prop="status" required>
                <el-switch
                  v-model="formData.status"
                  active-text="冻结用户，无法登录"
                ></el-switch>
              </el-form-item>
            </el-col>
          </el-row>

          <el-form-item label="描述" prop="summary">
            <el-input
              v-model="formData.summary"
              type="textarea"
              placeholder="请输入描述"
              :maxlength="200"
              show-word-limit
              :autosize="{ minRows: 2, maxRows: 3 }"
              :style="{ width: '100%' }"
            ></el-input>
          </el-form-item>
        </el-form>
      </el-container>
    </el-container>
  </Modal>
</template>
