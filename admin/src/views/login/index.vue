<template>
  <div
    class="min-h-screen w-full flex flex-col items-center justify-center relative font-sans transition-colors duration-700"
    :class="isDarkMode ? 'text-slate-100' : 'text-slate-900'"
  >
    <!-- 背景组件 -->
    <Background :primaryColor="primaryColor" :isDarkMode="isDarkMode" />

    <!-- 顶部栏组件 -->
    <TopBar
      :currentTheme="primaryColor"
      :isDarkMode="isDarkMode"
      @themeChange="setThemeId"
      @toggleDarkMode="toggleTheme"
    />

    <!-- 主内容区 - 登录表单 -->
    <main class="z-10 w-full flex justify-center px-6">
      <LoginForm :primaryColor="primaryColor" :isDarkMode="isDarkMode" />
    </main>

    <div
      class="fixed top-20 right-[15%] w-64 h-64 rounded-full blur-3xl pointer-events-none -z-0 transition-opacity duration-1000"
      :class="
        isDarkMode ? 'opacity-20 bg-blue-900/40' : 'opacity-10 bg-blue-400/10'
      "
    ></div>
    <div
      class="fixed bottom-20 left-[15%] w-64 h-64 rounded-full blur-3xl pointer-events-none -z-0 transition-opacity duration-1000"
      :class="
        isDarkMode
          ? 'opacity-20 bg-emerald-900/40'
          : 'opacity-10 bg-emerald-400/10'
      "
    ></div>
  </div>
</template>

<script setup lang="ts">
import { useAppStore } from "@/stores/app";
const appStore = useAppStore();
import Background from "./components/background.vue";
import TopBar from "./components/topbar.vue";
import LoginForm from "./components/loginform.vue";
const toggleTheme = () => appStore.toggleTheme();
const isDarkMode = computed(() => appStore.theme === "dark");
const primaryColor = computed(() => appStore.primaryColor);

const setThemeId = (id: string) => {
  appStore.setPrimaryColor(id);
};
</script>
