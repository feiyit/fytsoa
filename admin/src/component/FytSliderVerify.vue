<script setup lang="ts">
const props = withDefaults(
  defineProps<{
    modelValue?: boolean;
    disabled?: boolean;
  }>(),
  {
    modelValue: false,
    disabled: false,
  }
);

const emit = defineEmits<{
  (e: "update:modelValue", value: boolean): void;
}>();

const trackRef = ref<HTMLElement | null>(null);
const handleRef = ref<HTMLElement | null>(null);

const percent = ref(0);
const passed = ref(props.modelValue);
const dragging = ref(false);

let trackRect: DOMRect | null = null;
let handleWidth = 32; // 默认宽度，与样式中的 w-8 保持一致

watch(
  () => props.modelValue,
  (val) => {
    passed.value = val;
    if (val) {
      percent.value = 100;
    } else if (!dragging.value) {
      percent.value = 0;
    }
  },
  { immediate: true }
);

const syncHandleWidth = () => {
  if (!handleRef.value) return;
  const rect = handleRef.value.getBoundingClientRect();
  if (rect.width > 0) {
    handleWidth = rect.width;
  }
};

const updateByClientX = (clientX: number) => {
  if (!trackRect) return;
  const { left, width } = trackRect;
  const maxOffset = Math.max(width - handleWidth, 0);
  const rawOffset = clientX - left - handleWidth / 2;
  const clampedOffset = Math.min(Math.max(rawOffset, 0), maxOffset);
  const ratio = maxOffset === 0 ? 0 : clampedOffset / maxOffset;
  percent.value = ratio * 100;

  if (ratio >= 0.96) {
    passed.value = true;
    dragging.value = false;
    percent.value = 100;
    emit("update:modelValue", true);
    removeListeners();
  }
};

const handleMove = (event: MouseEvent | TouchEvent) => {
  if (!dragging.value || !trackRect) return;
  const clientX =
    "touches" in event ? event.touches[0]?.clientX ?? 0 : event.clientX;
  updateByClientX(clientX);
};

const handleUp = () => {
  if (!passed.value) {
    percent.value = 0;
  }
  dragging.value = false;
  removeListeners();
};

const removeListeners = () => {
  if (typeof window === "undefined") return;
  window.removeEventListener("mousemove", handleMove as any);
  window.removeEventListener("touchmove", handleMove as any);
  window.removeEventListener("mouseup", handleUp);
  window.removeEventListener("touchend", handleUp);
};

// 对外暴露的重置方法：恢复到初始未通过状态
const resetSlider = () => {
  dragging.value = false;
  passed.value = false;
  percent.value = 0;
  removeListeners();
  emit("update:modelValue", false);
};

defineExpose({
  resetSlider,
});

const handleDown = (event: MouseEvent | TouchEvent) => {
  if (props.disabled || passed.value) return;
  const track = trackRef.value;
  if (!track) return;

  dragging.value = true;
  trackRect = track.getBoundingClientRect();
  syncHandleWidth();

  const clientX =
    "touches" in event ? event.touches[0]?.clientX ?? 0 : event.clientX;
  updateByClientX(clientX);

  if (typeof window !== "undefined") {
    window.addEventListener("mousemove", handleMove as any);
    window.addEventListener("touchmove", handleMove as any);
    window.addEventListener("mouseup", handleUp);
    window.addEventListener("touchend", handleUp);
  }
};

onMounted(() => {
  syncHandleWidth();
});

onUnmounted(() => {
  removeListeners();
});
</script>

<template>
  <div class="space-y-2">
    <p class="text-xs text-slate-500 dark:text-slate-400">滑动验证</p>
    <div
      ref="trackRef"
      class="relative flex min-h-[44px] items-center overflow-hidden rounded-xl bg-slate-100/80 px-3 py-2.5 dark:bg-slate-800/80 cursor-pointer select-none"
      @mousedown.prevent="handleDown"
      @touchstart.prevent="handleDown"
    >
      <div
        class="pointer-events-none absolute left-0 top-0 h-full bg-emerald-500/15 transition-all"
        :style="{ width: `${percent}%` }"
      />

      <div
        class="pointer-events-none relative z-10 text-xs font-medium text-center w-full"
        :class="
          passed ? 'text-emerald-500' : 'text-slate-400 dark:text-slate-500'
        "
      >
        {{
          passed
            ? "验证通过"
            : dragging
            ? "继续拖动完成验证"
            : "按住滑块拖动完成验证"
        }}
      </div>

      <span
        ref="handleRef"
        class="pointer-events-none absolute top-1/2 flex h-8 w-8 -translate-y-1/2 items-center justify-center rounded-full bg-white text-slate-500 shadow-md shadow-slate-300/80 ring-1 ring-slate-200 transition-[left,background-color] dark:bg-slate-900 dark:text-slate-200 dark:shadow-slate-900/60 dark:ring-slate-700"
        :class="passed ? 'bg-emerald-500 text-white ring-emerald-500' : ''"
        :style="{ left: `${percent + 1}%` }"
      >
        <el-icon v-if="passed">
          <CircleCheckFilled />
        </el-icon>
        <el-icon v-else>
          <DArrowRight />
        </el-icon>
      </span>
    </div>
  </div>
</template>
