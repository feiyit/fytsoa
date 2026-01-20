<script setup lang="ts">
import { useUserStore } from "@/stores/user";
import { setNewsPassword, fetchAdminById, updateBasicUser } from "@/api";

defineOptions({ name: "UserCenterView" });

const userStore = useUserStore();

const activeMenu = ref<"account" | "password">("account");

const formRef = ref();
const formModel = reactive({
  id: 0,
  // 登录账号，仅展示
  account: userStore.userInfo?.username || "",
  // fullName
  realName: "",
  // sex
  gender: "",
  // 头像地址
  avatar: "",
  // summary
  signature: "",
});

const formRules = {
  realName: [
    {
      required: true,
      message: "请输入姓名",
      trigger: "blur",
    },
  ],
  gender: [
    {
      required: true,
      message: "请选择性别",
      trigger: "change",
    },
  ],
};

const saving = ref(false);

const handleSave = () => {
  formRef.value?.validate(async (valid: boolean) => {
    if (!valid) return;
    if (!formModel.id) {
      ElMessage.error("未获取到当前用户信息，无法保存");
      return;
    }
    saving.value = true;
    try {
      // 更新个人基础信息
      await updateBasicUser({
        id: formModel.id,
        fullName: formModel.realName,
        sex: formModel.gender,
        avatar: formModel.avatar,
        summary: formModel.signature,
      });
      ElMessage.success("保存成功");
    } finally {
      saving.value = false;
    }
  });
};

// 修改密码表单（示例）
const passwordFormRef = ref<any>();
const passwordForm = reactive({
  currentPassword: "",
  newPassword: "",
  confirmPassword: "",
});

const passwordRules = {
  currentPassword: [
    { required: true, message: "请输入当前密码", trigger: "blur" },
  ],
  newPassword: [
    { required: true, message: "请输入新密码", trigger: "blur" },
    {
      min: 8,
      message: "新密码长度至少 8 位",
      trigger: "blur",
    },
  ],
  confirmPassword: [
    { required: true, message: "请再次输入新密码", trigger: "blur" },
    {
      validator: (
        _rule: unknown,
        value: string,
        callback: (err?: Error) => void
      ) => {
        if (!value) {
          callback(new Error("请再次输入新密码"));
          return;
        }
        if (value !== passwordForm.newPassword) {
          callback(new Error("两次输入的密码不一致"));
          return;
        }
        callback();
      },
      trigger: "blur",
    },
  ],
};

const passwordSaving = ref(false);

// 简单密码强度评估：0-4
const passwordStrength = computed(() => {
  const v = passwordForm.newPassword || "";
  let score = 0;
  if (v.length >= 8) score++;
  if (/[a-zA-Z]/.test(v)) score++;
  if (/[0-9]/.test(v)) score++;
  if (/[^a-zA-Z0-9]/.test(v)) score++;
  return score;
});

const handleSavePassword = () => {
  passwordFormRef.value?.validate(async (valid: boolean) => {
    if (!valid) return;
    if (!userStore.userInfo?.id) {
      ElMessage.error("未获取到当前用户信息，无法修改密码");
      return;
    }
    passwordSaving.value = true;
    try {
      // 调用修改密码接口
      await setNewsPassword({
        id: userStore.userInfo.id,
        sourcePwd: passwordForm.currentPassword,
        passWord: passwordForm.newPassword,
      });
      ElMessage.success("密码修改成功");

      await ElMessageBox.alert(
        "密码更新成功后，请使用新密码重新登录系统。",
        "提示",
        { type: "success" }
      );

      // 示例中不强制跳转，如需真实跳转可解除下行注释
      // const router = useRouter();
      // router.push('/login');
    } finally {
      passwordSaving.value = false;
    }
  });
};

const menuItems = [
  {
    key: "account",
    label: "账号信息",
    icon: "User",
    group: "基本设置",
  },
  {
    key: "password",
    label: "修改密码",
    icon: "Lock",
    group: "基本设置",
  },
] as const;

const groupedMenu = computed(() => {
  const groups: Record<string, typeof menuItems> = {};
  menuItems.forEach((item) => {
    if (!groups[item.group]) groups[item.group] = [] as any;
    groups[item.group].push(item as any);
  });
  return groups;
});

const handleMenuClick = (key: (typeof menuItems)[number]["key"]) => {
  activeMenu.value = key;
};

// 加载当前登录用户的基础信息
onMounted(async () => {
  if (!userStore.userInfo?.id) return;
  try {
    const user: any = await fetchAdminById(userStore.userInfo.id as any);
    formModel.id = user.id ?? 0;
    formModel.account = user.loginAccount || userStore.userInfo?.username || "";
    formModel.realName = user.fullName || "";
    formModel.gender = user.sex || "";
    formModel.avatar = user.avatar || "";
    formModel.signature = user.summary || "";
  } catch (error) {
    console.error("加载用户信息失败", error);
  }
});
</script>

<template>
  <div class="min-h-full rounded-[.5vw]">
    <el-row :gutter="12" class="h-full">
      <!-- 左侧：个人信息卡片 + 设置菜单 -->
      <el-col :xs="24" :sm="24" :md="7" :lg="5" :xl="5">
        <div
          class="flex h-full flex-col bg-card border-border rounded-[.5vw] dark:!border-slate-750"
        >
          <!-- 头像与基本信息 -->
          <section
            class="flex flex-col items-center border-b border-slate-100 dark:border-slate-750 pt-4"
            style="padding-bottom: 10px"
          >
            <el-avatar :size="80" class="mb-3">
              {{ userStore.displayName.slice(0, 1).toUpperCase() || "U" }}
            </el-avatar>
            <div class="mb-2 text-base font-semibold">
              {{ userStore.displayName || "未登录用户" }}
            </div>
            <el-tag type="primary" effect="light" round> 系统管理员 </el-tag>
          </section>

          <!-- 菜单 -->
          <section class="flex-1 overflow-y-auto px-4 py-4 text-[13px] pt-4">
            <div
              v-for="(items, group) in groupedMenu"
              :key="group"
              class="mb-4 last:mb-0"
            >
              <div
                class="mb-2 text-[13px] font-medium text-slate-400 dark:text-slate-500"
              >
                {{ group }}
              </div>
              <div class="space-y-1">
                <button
                  v-for="item in items"
                  :key="item.key"
                  type="button"
                  class="flex w-full items-center gap-2 rounded-[8px] px-2 py-2 text-[13px] transition"
                  :class="
                    activeMenu === item.key
                      ? 'bg-primary-50 text-primary-600 dark:bg-sky-900/60 dark:text-sky-100'
                      : 'text-slate-600 hover:bg-slate-100 dark:text-slate-300 dark:hover:bg-slate-800'
                  "
                  @click="handleMenuClick(item.key)"
                >
                  <el-icon :size="16">
                    <component :is="item.icon || 'Menu'" />
                  </el-icon>
                  <span class="truncate">{{ item.label }}</span>
                </button>
              </div>
            </div>
          </section>
        </div>
      </el-col>

      <!-- 右侧：内容区域 -->
      <el-col :xs="24" :sm="24" :md="17" :lg="19" :xl="19">
        <el-card
          shadow="never"
          class="h-full bg-card border-border rounded-[.5vw] dark:!border-slate-750"
        >
          <!-- 账号信息 -->
          <template v-if="activeMenu === 'account'">
            <h2
              class="mb-5 text-base font-semibold text-slate-800 dark:text-slate-100"
            >
              个人信息
            </h2>

            <el-form
              ref="formRef"
              :model="formModel"
              :rules="formRules"
              label-width="80px"
              label-position="left"
              class="max-w-3xl"
            >
              <el-form-item label="账号">
                <el-input v-model="formModel.account" disabled />
                <span
                  class="mt-1 block text-[13px] text-slate-400 dark:text-slate-500"
                >
                  账号信息用于登录，系统不允许修改
                </span>
              </el-form-item>

              <el-form-item label="姓名" prop="realName">
                <el-input
                  v-model="formModel.realName"
                  placeholder="请输入姓名"
                  clearable
                />
              </el-form-item>

              <el-form-item label="性别" prop="gender">
                <el-select
                  v-model="formModel.gender"
                  placeholder="请选择性别"
                  style="width: 200px"
                >
                  <el-option label="男" value="男" />
                  <el-option label="女" value="女" />
                  <el-option label="保密" value="保密" />
                </el-select>
              </el-form-item>

              <el-form-item label="个性签名">
                <el-input
                  v-model="formModel.signature"
                  type="textarea"
                  :autosize="{ minRows: 3, maxRows: 5 }"
                  placeholder="写一句简单的介绍，展示给其他人"
                  maxlength="80"
                  show-word-limit
                />
              </el-form-item>

              <el-form-item>
                <el-button type="primary" :loading="saving" @click="handleSave">
                  保存
                </el-button>
              </el-form-item>
            </el-form>
          </template>

          <!-- 修改密码 -->
          <template v-else-if="activeMenu === 'password'">
            <h2
              class="mb-4 text-base font-semibold text-slate-800 dark:text-slate-100"
            >
              修改密码
            </h2>

            <el-alert
              type="info"
              show-icon
              :closable="false"
              class="mb-4 text-[13px]"
              title="密码更新成功后，您将被重定向到登录页面，您可以使用新密码重新登录。"
            />

            <el-form
              ref="passwordFormRef"
              :model="passwordForm"
              :rules="passwordRules"
              label-width="100px"
              label-position="left"
              class="max-w-3xl"
            >
              <el-form-item label="当前密码" prop="currentPassword">
                <el-input
                  v-model="passwordForm.currentPassword"
                  type="password"
                  show-password
                  placeholder="请输入当前密码"
                  autocomplete="current-password"
                />
                <span
                  class="mt-1 block text-[13px] text-slate-400 dark:text-slate-500"
                >
                  必须提供当前登录用户密码才能进行更改
                </span>
              </el-form-item>

              <el-form-item label="新密码" prop="newPassword">
                <el-input
                  v-model="passwordForm.newPassword"
                  type="password"
                  show-password
                  placeholder="请输入新密码"
                  autocomplete="new-password"
                />
                <!-- 简易密码强度条 -->
                <div
                  class="mt-2 flex h-1.5 overflow-hidden rounded-full bg-slate-200/70 dark:bg-slate-800"
                >
                  <div
                    v-for="n in 4"
                    :key="n"
                    class="mr-0.5 flex-1 last:mr-0"
                    :class="
                      passwordStrength >= n
                        ? 'bg-emerald-500'
                        : 'bg-slate-400/40 dark:bg-slate-600'
                    "
                  />
                </div>
                <span
                  class="mt-1 block text-[13px] text-slate-400 dark:text-slate-500"
                >
                  请输入包含英文、数字的 8 位以上密码
                </span>
              </el-form-item>

              <el-form-item label="确认新密码" prop="confirmPassword">
                <el-input
                  v-model="passwordForm.confirmPassword"
                  type="password"
                  show-password
                  placeholder="请再次输入新密码"
                  autocomplete="new-password"
                />
              </el-form-item>

              <el-form-item>
                <el-button
                  type="primary"
                  :loading="passwordSaving"
                  @click="handleSavePassword"
                >
                  保存密码
                </el-button>
              </el-form-item>
            </el-form>
          </template>
        </el-card>
      </el-col>
    </el-row>
  </div>
</template>
