<template>
  <div
    class="fixed top-0 left-0 right-0 p-6 flex justify-between items-center z-20"
  >
    <div class="flex items-center space-x-3">
      <div
        class="px-3 py-1 rounded-lg text-white font-bold text-sm shadow-md transition-colors duration-500"
        :style="{ backgroundColor: currentThemeColor }"
      >
        {{ SYSTEM_NAME }}
      </div>
      <span
        class="font-medium text-sm hidden md:inline transition-colors duration-500"
        :class="isDarkMode ? 'text-slate-400' : 'text-slate-600'"
      >
        {{ SYSTEM_SUBTITLE }}
      </span>
    </div>

    <div
      class="glass-card flex items-center space-x-3 px-4 py-2 rounded-full transition-all duration-500"
      :class="
        isDarkMode
          ? 'bg-slate-900/40 border-white/5'
          : 'bg-white/70 border-white/30'
      "
    >
      <div class="flex items-center space-x-2 mr-2">
        <button
          v-for="theme in THEME_OPTIONS"
          :key="theme.id"
          @click="handleThemeChange(theme.id)"
          class="w-5 h-5 rounded-full border-2 transition-all hover:scale-125"
          :class="
            currentTheme === theme.id
              ? 'border-white scale-125 ring-2 ring-slate-400/30 shadow-lg'
              : 'border-transparent opacity-60 hover:opacity-100'
          "
          :style="{ backgroundColor: theme.color }"
          :title="theme.name"
        />
      </div>

      <div
        class="w-[1px] h-4 mx-2 transition-colors duration-500"
        :class="isDarkMode ? 'bg-slate-700' : 'bg-slate-300'"
      ></div>

      <button
        @click="handleToggleDarkMode"
        class="flex items-center justify-center w-8 h-8 rounded-full transition-all duration-300"
        :class="
          isDarkMode
            ? 'text-amber-400 hover:bg-slate-800'
            : 'text-slate-500 hover:bg-slate-200'
        "
      >
        <Moon v-if="isDarkMode" style="width: 25px" />
        <Sunny v-else style="width: 25px" />
      </button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { SYSTEM_NAME, SYSTEM_SUBTITLE, THEME_OPTIONS } from "../constants.ts";
// 定义组件 Props 类型
interface Props {
  currentTheme: string;
  isDarkMode: boolean;
  onThemeChange: (id: string) => void;
  onToggleDarkMode: () => void;
}

// 声明 Props
const props = defineProps<Props>();

// 定义事件处理函数（转发父组件传入的回调）
const handleThemeChange = (id: string) => {
  props.onThemeChange(id);
};

const handleToggleDarkMode = () => {
  props.onToggleDarkMode();
};

// 计算属性：获取当前主题对应的颜色
const currentThemeColor = computed(() => {
  return THEME_OPTIONS.find((t) => t.id === props.currentTheme)?.color || "";
});
</script>

<style scoped>
/* 如果需要组件内样式可以在这里添加，原代码使用的是 Tailwind 类，无需额外样式 */
</style>
