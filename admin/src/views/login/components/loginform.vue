<template>
  <div
    class="glass-card w-full max-w-[460px] p-10 rounded-[40px] flex flex-col items-center transition-all duration-500"
    :class="
      isDarkMode
        ? 'bg-slate-900/60 border-white/5 shadow-2xl'
        : 'bg-white/70 border-white/30'
    "
  >
    <div class="w-full text-left mb-8">
      <h2
        class="text-3xl font-bold mb-2 transition-colors duration-500"
        :class="textColor"
      >
        欢迎登录
      </h2>
      <p
        class="text-sm transition-all duration-500"
        :class="[subTextColor, isLoadingAi ? 'opacity-50' : 'opacity-100']"
      >
        {{ aiGreeting }}
      </p>
    </div>

    <div class="w-full space-y-6">
      <!-- Username -->
      <div class="group">
        <label
          class="block text-sm font-medium mb-2 ml-1 transition-colors duration-500"
          :class="isDarkMode ? 'text-slate-300' : 'text-slate-700'"
          >账号</label
        >
        <div class="relative">
          <input
            type="text"
            v-model="account"
            placeholder="请输入账号"
            class="w-full px-4 py-3 rounded-xl border focus:outline-none transition-all duration-300 focus:ring-2"
            :class="[inputBg, inputBorder, textColor]"
            :style="{
              borderColor:
                sliderVerifying || account ? primaryColor : undefined,
              '--tw-ring-color': primaryColor,
            }"
          />
          <div
            class="absolute right-4 top-1/2 -translate-y-1/2 transition-colors duration-500"
            :class="isDarkMode ? 'text-slate-600' : 'text-slate-300'"
          >
            <el-icon><UserFilled /></el-icon>
          </div>
        </div>
      </div>

      <div class="group">
        <label
          class="block text-sm font-medium mb-2 ml-1 transition-colors duration-500"
          :class="isDarkMode ? 'text-slate-300' : 'text-slate-700'"
          >密码</label
        >
        <div class="relative">
          <input
            type="password"
            v-model="password"
            placeholder="请输入密码"
            class="w-full px-4 py-3 rounded-xl border focus:outline-none transition-all duration-300 focus:ring-2"
            :class="[inputBg, inputBorder, textColor]"
            :style="{
              borderColor:
                sliderVerifying || password ? primaryColor : undefined,
              '--tw-ring-color': primaryColor,
            }"
          />
          <div
            class="absolute right-4 top-1/2 -translate-y-1/2 transition-colors duration-500"
            :class="isDarkMode ? 'text-slate-600' : 'text-slate-300'"
          >
            <el-icon><Lock /></el-icon>
          </div>
        </div>
      </div>

      <div class="space-y-2">
        <label
          class="block text-sm font-medium mb-2 ml-1 transition-colors duration-500"
          :class="isDarkMode ? 'text-slate-300' : 'text-slate-700'"
          >滑动验证</label
        >
        <FytSliderVerify
          ref="sliderRef"
          v-model="sliderPassed"
          :disabled="!account"
        />
      </div>

      <div class="flex items-center space-x-2">
        <input
          type="checkbox"
          id="remember"
          v-model="rememberAccount"
          class="w-4 h-4 rounded text-blue-600 focus:ring-blue-500 border-slate-300 transition-colors cursor-pointer"
        />
        <label
          for="remember"
          class="text-sm cursor-pointer transition-colors duration-500"
          :class="subTextColor"
        >
          记住账号
        </label>
      </div>

      <button
        :loading="loading"
        :disabled="!canSubmit"
        @click="handleSubmit"
        class="w-full py-3 rounded-xl text-white font-semibold text-lg transition-all transform active:scale-[0.98]"
        :class="
          !sliderVerifying
            ? 'bg-slate-300/50 cursor-not-allowed text-slate-500'
            : 'shadow-lg hover:shadow-xl'
        "
        :style="{ backgroundColor: isVerified ? primaryColor : undefined }"
      >
        登录
      </button>
    </div>

    <div
      class="mt-10 text-xs text-center transition-colors duration-500"
      :class="isDarkMode ? 'text-slate-600' : 'text-slate-400'"
    >
      Copyright © 2026
      <a
        href="https://www.feiyit.com"
        class="hover:text-primary-300 mx-1"
        target="_blank"
        >{{ appName }}</a
      >
    </div>
  </div>
</template>

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

const primaryColor = computed(() => appStore.primaryColor);

const sliderRef = ref<InstanceType<typeof FytSliderVerify> | null>(null);

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
const isVerified = ref(false);

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
      isVerified.value = false;
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
      isVerified.value = true;
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
  isLoadingAi.value = true;
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
    isLoadingAi.value = false;
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

const appName = import.meta.env.VITE_APP_NAME;
interface Props {
  primaryColor: string;
  isDarkMode: boolean;
}

// 声明 Props
const props = defineProps<Props>();

const aiGreeting = ref("欢迎登录，请输入账号与密码进入智能企业管理中台。");
const isLoadingAi = ref(false);

// 计算属性（封装样式类名，简化模板）
const textColor = computed(() =>
  props.isDarkMode ? "text-slate-100" : "text-slate-800",
);
const subTextColor = computed(() =>
  props.isDarkMode ? "text-slate-400" : "text-slate-500",
);
const inputBg = computed(() =>
  props.isDarkMode ? "bg-slate-900/50" : "bg-white",
);
const inputBorder = computed(() =>
  props.isDarkMode ? "border-slate-700" : "border-slate-200",
);
</script>
