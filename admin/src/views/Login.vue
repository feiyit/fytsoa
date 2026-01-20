<script setup lang="ts">
import FytSliderVerify from "@/component/FytSliderVerify.vue";
import { loginApi, getAllMenusApi, loginSliderToken } from "@/api/auth";
import { useAppStore } from "@/stores/app";
import { useUserStore } from "@/stores/user";
import { appStorage, STORAGE_KEYS } from "@/utils/storage";
import { md5Encrypt } from "@/utils/crypto";
import { getServerDynamicRoutes } from "@/utils/routeHandler";
import type { RouteRecordRaw } from "vue-router";

defineOptions({
  name: "LoginView",
});

const appStore = useAppStore();
const userStore = useUserStore();
const router = useRouter();
const route = useRoute();
const isDark = computed(() => appStore.theme === "dark");
const toggleTheme = () => appStore.toggleTheme();

// 当前 Element Plus 主色，用于高亮选中的圆点
const primaryColor = computed(() => appStore.primaryColor);

// 顶部右侧彩色圆点，可点击切换 Element Plus 主色
const colorDots = [
  "#3b82f6",
  "#a855f7",
  "#f97316",
  "#facc15",
  "#22c55e",
  "#06b6d4",
  "#4b5563",
];
const sliderRef = ref<InstanceType<typeof FytSliderVerify> | null>(null);

const handleSelectColor = (color: string) => {
  appStore.setPrimaryColor(color);
};

const account = ref("");
const password = ref("");
const rememberAccount = ref(false);

const ACCOUNT_KEY = STORAGE_KEYS.REMEMBER_ACCOUNT;

onMounted(() => {
  // 登录页默认使用暗色主题，但不要覆盖用户已保存的主题选择
  const savedTheme = appStorage.get<"light" | "dark">(STORAGE_KEYS.THEME);
  if (!savedTheme && appStore.theme !== "dark") {
    appStore.setTheme("dark", false);
  }

  // 初始化时从本地读取上次记住的账号
  const saved = appStorage.get<string>(ACCOUNT_KEY);
  if (saved) {
    account.value = saved;
    rememberAccount.value = true;
  }
});

const loading = ref(false);
const sliderPassed = ref(false);
const sliderToken = ref<string | null>(null);
const sliderVerifying = ref(false);

// 将后端菜单转换的动态路由挂载到 Layout 之下
const mountDynamicRoutes = (records: RouteRecordRaw[], parentPath = "") => {
  records.forEach((record) => {
    const rawPath = String(record.path || "");
    const fullPath = rawPath.startsWith("/")
      ? rawPath
      : `${parentPath}/${rawPath}`.replace(/\/+/g, "/");

    if (record.component) {
      const name =
        (record.name as string | undefined) ||
        fullPath ||
        `dynamic-${fullPath}`;

      // 如果已存在同名路由则跳过，避免重复注册
      if (router.hasRoute(name)) return;

      router.addRoute("Layout", {
        path: fullPath,
        name,
        component: record.component as any,
        meta: record.meta,
      });
    }

    if (record.children && record.children.length) {
      mountDynamicRoutes(record.children as RouteRecordRaw[], fullPath);
    }
  });
};

// 从本地存储读取菜单并刷新动态路由
const refreshDynamicRoutes = () => {
  const dynamicRecords = getServerDynamicRoutes();
  if (!dynamicRecords.length) return;
  mountDynamicRoutes(dynamicRecords);
};

// 当账号被清空时，重置滑动验证状态
watch(
  () => account.value,
  (val) => {
    if (!val) {
      sliderPassed.value = false;
      sliderToken.value = null;
    }
  },
);

// 监听滑块通过，与后台交互获取一次性 sliderToken
watch(
  () => sliderPassed.value,
  async (val) => {
    // 滑块被重置
    if (!val) {
      sliderToken.value = null;
      return;
    }

    sliderVerifying.value = true;
    try {
      const res = await loginSliderToken({
        account: account.value.trim() || undefined,
      });
      sliderToken.value = res.sliderToken;
    } catch (error) {
      console.error("slider verify failed:", error);
      sliderToken.value = null;
      sliderPassed.value = false;
    } finally {
      sliderVerifying.value = false;
    }
  },
);

const handleSubmit = async () => {
  if (!canSubmit.value) return;
  loading.value = true;

  try {
    const res = await loginApi({
      account: account.value.trim(),
      password: md5Encrypt(password.value),
      codeKey: sliderToken.value || "",
    });

    // 登录成功：写入用户 store（自动持久化 token / 用户信息）
    userStore.setUser(res);

    // 记住账号：仅在登录成功且勾选时保存
    if (rememberAccount.value && account.value) {
      appStorage.set(ACCOUNT_KEY, account.value.trim());
    } else {
      appStorage.remove(ACCOUNT_KEY);
    }

    const menuRes = await getAllMenusApi();
    appStorage.set(STORAGE_KEYS.ROUTER_MENU, menuRes.menu);
    userStore.setPermissions(menuRes.directive);

    // 登录成功后，根据最新菜单刷新一次动态路由
    refreshDynamicRoutes();

    // 跳转到重定向地址或首页
    const redirect = (route.query.redirect as string) || "/";
    router.push(redirect);
  } catch (error) {
    console.error("login failed:", error);
    sliderRef.value?.resetSlider();
  } finally {
    loading.value = false;
  }
};

const canSubmit = computed(() => {
  return (
    !!account.value &&
    !!password.value &&
    sliderPassed.value &&
    !!sliderToken.value &&
    !loading.value &&
    !sliderVerifying.value
  );
});
</script>

<template>
  <div
    class="login-page relative min-h-screen w-full text-slate-900 dark:text-slate-100"
  >
    <div class="login-bg"></div>
    <!-- 顶部工具条浮动在页面之上，左右工具区域上下贯穿整个页面视觉 -->
    <header
      class="pointer-events-none absolute inset-x-0 top-0 z-20 flex flex-wrap items-center justify-between gap-3 px-4 py-3 sm:px-8 sm:py-4"
    >
      <div class="pointer-events-auto inline-flex items-center gap-2">
        <span
          class="rounded-full bg-primary-500 px-2.5 py-1 text-xs font-semibold text-white shadow-sm"
        >
          FytAdmin
        </span>
        <span class="text-sm text-slate-600 dark:text-slate-300">
          企业管理系统
        </span>
      </div>

      <div
        class="pointer-events-auto flex flex-wrap items-center justify-end gap-3"
      >
        <!-- 顶部功能条（参考 Vben Login） -->
        <div
          class="flex items-center gap-2 rounded-full bg-black/40 px-3 py-1.5 text-xs text-slate-200 shadow-lg shadow-black/30 backdrop-blur-sm"
        >
          <!-- 选中主题指示圆点，仅装饰 -->
          <span
            class="flex h-5 w-5 items-center justify-center rounded-full border border-slate-500/70 bg-slate-900/70"
          >
            <span
              class="h-2.5 w-2.5 rounded-full"
              :style="{ backgroundColor: primaryColor }"
            />
          </span>

          <!-- 彩色圆点列表：点击切换 Element Plus 主题色 -->
          <div class="flex items-center gap-1.5">
            <button
              v-for="(c, idx) in colorDots"
              :key="idx"
              type="button"
              class="h-3.5 w-3.5 rounded-full border border-transparent transition hover:scale-110"
              :style="{
                backgroundColor: c,
                boxShadow:
                  primaryColor === c
                    ? '0 0 0 2px rgba(255,255,255,0.75)'
                    : 'none',
              }"
              @click="handleSelectColor(c)"
            />
          </div>

          <span class="mx-1 h-4 w-px bg-slate-600/60" />

          <!-- 主题切换按钮 -->
          <button
            type="button"
            class="flex h-6 w-6 items-center justify-center rounded-full text-[0] text-slate-200 transition hover:bg-slate-700/80"
            @click="toggleTheme"
          >
            <el-icon :size="14">
              <Moon v-if="isDark" />
              <Sunny v-else />
            </el-icon>
          </button>
        </div>
      </div>
    </header>

    <main
      class="relative flex min-h-screen flex-col overflow-hidden pt-16 sm:pt-20"
    >
      <!-- 居中布局：仅显示登录区域 -->
      <div
        class="relative z-10 flex h-full w-full items-center justify-center px-4 py-8 sm:px-0 sm:py-10"
      >
        <div
          class="login-card w-full max-w-[500px] rounded-3xl p-6 shadow-[0_0_40px_rgba(15,23,42,0.85)] sm:p-7"
        >
          <header class="mb-6 space-y-1">
            <h1 class="text-xl font-semibold">欢迎登录</h1>
            <p class="text-xs drak:text-slate-300/90">
              请输入账号与密码，完成验证后进入智能企业管理中台。
            </p>
          </header>

          <el-form label-position="top" class="space-y-4" @submit.prevent>
            <el-form-item label="账号">
              <el-input
                v-model="account"
                autocomplete="username"
                placeholder="请输入账号"
                size="large"
                clearable
              />
            </el-form-item>

            <el-form-item label="密码">
              <el-input
                v-model="password"
                type="password"
                autocomplete="current-password"
                placeholder="请输入密码"
                size="large"
                show-password
                clearable
              />
            </el-form-item>

            <!-- 账号未填写时禁用滑动验证 -->
            <FytSliderVerify
              ref="sliderRef"
              v-model="sliderPassed"
              :disabled="!account"
            />

            <div class="mt-1 flex items-center justify-between text-xs">
              <el-checkbox v-model="rememberAccount"> 记住账号 </el-checkbox>
            </div>

            <el-button
              type="primary"
              class="mt-4 w-full"
              size="large"
              :loading="loading"
              :disabled="!canSubmit"
              @click="handleSubmit"
            >
              登录
            </el-button>
          </el-form>
          <div
            class="text-center mt-5 text-xs drak:text-slate-400/90 sm:mt-6 sm:text-[11px]"
          >
            Copyright © 2024
            <a
              href="https://www.feiyit.com"
              class="hover:text-primary-300 mx-1"
              target="_blank"
              >FytAdmin</a
            >
          </div>
        </div>
      </div>
    </main>
  </div>
</template>

<style scoped>
.login-page {
  background: linear-gradient(to right, #f5f5f5, #dfe5ee 30%, #f5f5f5);
}
.dark .login-page {
  background: linear-gradient(to right, #1c1e22, #1a2534 50%, #1c1e22);
}
.login-bg {
  position: absolute;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  /* 企业级渐变底色（深蓝+浅灰，低饱和度） */
  background: linear-gradient(135deg, #1e3c72 0%, #2a5298 50%, #f5f7fa 100%);
  /* 叠加轻微的网格纹理，增加质感但不突兀 */
  background-image:
    linear-gradient(rgba(255, 255, 255, 0.05) 1px, transparent 1px),
    linear-gradient(90deg, rgba(255, 255, 255, 0.05) 1px, transparent 1px);
  background-size: 40px 40px;
  /* 动态光效动画 */
  animation: bgLightMove 15s ease-in-out infinite alternate;
}

/* 光效动画：模拟柔和的光线流动 */
@keyframes bgLightMove {
  0% {
    background-position: 0 0;
    filter: brightness(0.95);
  }
  100% {
    background-position: 40px 40px;
    filter: brightness(1.05);
  }
}
.dark .login-card {
  background: linear-gradient(
    145deg,
    rgba(15, 23, 42, 0.98),
    rgba(15, 23, 42, 0.96),
    rgba(15, 23, 42, 0.94)
  );
  border-color: rgba(148, 163, 184, 0.35);
}

@media (max-width: 640px) {
  .login-card {
    border-radius: 1.25rem;
    margin-inline: auto;
  }
}
</style>
