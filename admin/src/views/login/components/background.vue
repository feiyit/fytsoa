<template>
  <div
    class="fixed inset-0 overflow-hidden transition-colors duration-700"
    :class="isDarkMode ? 'bg-slate-950' : 'bg-slate-50'"
  >
    <div
      class="mesh-gradient-item absolute top-[-10%] left-[-10%] w-[60%] h-[60%] rounded-full opacity-30 blur-[100px] transition-all duration-1000"
      :style="{ backgroundColor: primaryColor, animationDelay: '0s' }"
    ></div>
    <div
      class="mesh-gradient-item absolute bottom-[-10%] right-[-10%] w-[50%] h-[50%] rounded-full opacity-30 blur-[100px] transition-all duration-1000"
      :style="{
        backgroundColor: isDarkMode ? '#4c1d95' : '#c084fc',
        animationDelay: '-5s',
      }"
    ></div>
    <div
      class="mesh-gradient-item absolute top-[20%] right-[10%] w-[40%] h-[40%] rounded-full opacity-20 blur-[100px] transition-all duration-1000"
      :style="{
        backgroundColor: isDarkMode ? '#1e3a8a' : '#818cf8',
        animationDelay: '-10s',
      }"
    ></div>
    <div
      class="mesh-gradient-item absolute bottom-[10%] left-[20%] w-[45%] h-[45%] rounded-full opacity-25 blur-[100px] transition-all duration-1000"
      :style="{
        backgroundColor: isDarkMode ? '#134e4a' : '#2dd4bf',
        animationDelay: '-15s',
      }"
    ></div>

    <div
      class="absolute inset-0 transition-opacity duration-700"
      :class="isDarkMode ? 'opacity-[0.05]' : 'opacity-[0.03]'"
      :style="{
        backgroundImage: `radial-gradient(${isDarkMode ? '#94a3b8' : '#475569'} 1px, transparent 1px)`,
        backgroundSize: '40px 40px',
      }"
    ></div>

    <div
      class="absolute top-1/4 left-10 w-32 h-32 border backdrop-blur-md rounded-2xl rotate-12 transition-all duration-1000 hover:rotate-45"
      :class="
        isDarkMode
          ? 'bg-white/5 border-white/10'
          : 'bg-white/10 border-white/20'
      "
    ></div>
    <div
      class="absolute bottom-1/4 right-20 w-24 h-24 border backdrop-blur-md rounded-full -rotate-12 transition-all duration-1000 hover:-rotate-45"
      :class="
        isDarkMode
          ? 'bg-white/5 border-white/10'
          : 'bg-white/10 border-white/20'
      "
    ></div>
  </div>
</template>

<script setup lang="ts">
import { toRefs, watch } from "vue";

// 定义组件 Props 类型
interface Props {
  primaryColor: string;
  isDarkMode: boolean;
}

const props = defineProps<Props>();

const { primaryColor, isDarkMode } = toRefs(props);
</script>

<style scoped>
.mesh-gradient-item {
  animation: float 20s ease-in-out infinite;
}

@keyframes float {
  0%,
  100% {
    transform: translate(0, 0) scale(1);
  }
  25% {
    transform: translate(10px, -10px) scale(1.05);
  }
  50% {
    transform: translate(-10px, 10px) scale(0.95);
  }
  75% {
    transform: translate(5px, 5px) scale(1.02);
  }
}
</style>
