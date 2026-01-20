<script setup lang="ts">
interface NavTag {
  path: string;
  title: string;
  fixed?: boolean;
}

type TagContextCommand =
  | "reload"
  | "close"
  | "closeLeft"
  | "closeRight"
  | "closeOthers"
  | "closeAll";

const props = defineProps<{
  tags: NavTag[];
  activePath: string;
}>();

const { tags, activePath } = toRefs(props);

const emit = defineEmits<{
  (e: "click-tag", tag: NavTag): void;
  (e: "close-tag", tag: NavTag): void;
  (e: "context-command", command: TagContextCommand, tag: NavTag): void;
}>();

const tagScrollRef = ref<HTMLElement | null>(null);
const canScrollLeft = ref(false);
const canScrollRight = ref(false);

const updateTagScrollState = () => {
  const el = tagScrollRef.value;
  if (!el) return;
  const maxScrollLeft = el.scrollWidth - el.clientWidth;
  canScrollLeft.value = el.scrollLeft > 0;
  canScrollRight.value = el.scrollLeft < maxScrollLeft - 1;
};

const scrollTags = (direction: "left" | "right") => {
  const el = tagScrollRef.value;
  if (!el) return;
  const delta = el.clientWidth * 0.6;
  const target =
    direction === "left" ? el.scrollLeft - delta : el.scrollLeft + delta;
  el.scrollTo({ left: target, behavior: "smooth" });
};

const handleTagWheel = (event: WheelEvent) => {
  const el = tagScrollRef.value;
  if (!el) return;
  if (!el.scrollWidth || el.scrollWidth <= el.clientWidth) return;

  // 使用垂直滚轮控制水平滚动
  event.preventDefault();
  el.scrollLeft += event.deltaY;
  updateTagScrollState();
};

onMounted(() => {
  updateTagScrollState();
});

watch(
  () => tags.value,
  () => {
    nextTick(() => {
      updateTagScrollState();
    });
  },
  { deep: true }
);
</script>

<template>
  <!-- 标签栏（第二行，高度 38px） -->
  <div
    class="flex h-[38px] items-center border-b border-slate-200/80 bg-white/80 px-2 dark:border-slate-750 dark:bg-[#1c1e22]"
  >
    <!-- 向左滚动箭头 -->
    <button
      class="mr-1 flex h-6 w-6 items-center justify-center rounded bg-slate-100 shadow-sm transition hover:bg-slate-200 dark:bg-slate-750 dark:text-slate-300 dark:hover:bg-slate-700"
      :class="!canScrollLeft ? 'opacity-40 cursor-default' : ''"
      type="button"
      :disabled="!canScrollLeft"
      @click="canScrollLeft && scrollTags('left')"
    >
      <el-icon :size="14">
        <ArrowLeft />
      </el-icon>
    </button>

    <!-- 中间标签滚动区域 -->
    <div
      ref="tagScrollRef"
      class="layout-tag-scroll flex-1 min-w-0 overflow-x-auto whitespace-nowrap"
      @wheel.prevent="handleTagWheel"
    >
      <div class="flex items-center gap-2">
        <el-dropdown
          v-for="tag in tags"
          :key="tag.path"
          trigger="contextmenu"
          @command="
            (cmd: TagContextCommand) =>
              emit('context-command', cmd, tag)
          "
        >
          <span>
            <el-tag
              type="info"
              size="small"
              effect="plain"
              :hit="activePath === tag.path"
              :closable="tags.length > 1 && !tag.fixed"
              class="layout-tab cursor-pointer select-none"
              :class="{ 'layout-tab--active': activePath === tag.path }"
              @click="emit('click-tag', tag)"
              @close.stop="emit('close-tag', tag)"
            >
              {{ tag.title }}
            </el-tag>
          </span>
          <template #dropdown>
            <el-dropdown-menu>
              <el-dropdown-item command="reload">
                <el-icon :size="14" class="mr-1">
                  <RefreshRight />
                </el-icon>
                重新加载
              </el-dropdown-item>
              <el-dropdown-item command="close">
                <el-icon :size="14" class="mr-1">
                  <Close />
                </el-icon>
                关闭当前
              </el-dropdown-item>
              <el-dropdown-item command="closeLeft">
                <el-icon :size="14" class="mr-1">
                  <Back />
                </el-icon>
                关闭左侧标签
              </el-dropdown-item>
              <el-dropdown-item command="closeRight">
                <el-icon :size="14" class="mr-1">
                  <Right />
                </el-icon>
                关闭右侧标签
              </el-dropdown-item>
              <el-dropdown-item command="closeOthers">
                <el-icon :size="14" class="mr-1">
                  <Switch />
                </el-icon>
                关闭其他
              </el-dropdown-item>
              <el-dropdown-item command="closeAll">
                <el-icon :size="14" class="mr-1">
                  <CircleClose />
                </el-icon>
                关闭全部
              </el-dropdown-item>
            </el-dropdown-menu>
          </template>
        </el-dropdown>
      </div>
    </div>

    <!-- 向右滚动箭头 -->
    <button
      class="ml-1 flex h-6 w-6 items-center justify-center rounded bg-slate-100 text-slate-500 shadow-sm transition hover:bg-slate-200 dark:bg-slate-750 dark:text-slate-300 dark:hover:bg-slate-700"
      :class="!canScrollRight ? 'opacity-40 cursor-default' : ''"
      type="button"
      :disabled="!canScrollRight"
      @click="canScrollRight && scrollTags('right')"
    >
      <el-icon :size="14">
        <ArrowRight />
      </el-icon>
    </button>
  </div>
</template>

<style scoped>
.layout-tag-scroll {
  scrollbar-width: none; /* Firefox */
  -ms-overflow-style: none; /* IE 10+ */
}

.layout-tag-scroll::-webkit-scrollbar {
  display: none; /* Chrome/Safari */
}

.layout-tab {
  border-radius: 9999px;
  border: 1px solid transparent !important;
  background: transparent !important;
  color: #4b5563 !important;
  font-size: 13px !important;
  padding: 0 10px !important;
  height: 26px !important;
  line-height: 24px !important;
  display: inline-flex !important;
  align-items: center;
  transition: background-color 0.15s ease, color 0.15s ease,
    border-color 0.15s ease, box-shadow 0.15s ease;
}

.layout-tab:hover {
  background: rgba(148, 163, 184, 0.18) !important;
}

.layout-tab--active {
  background: #dee9fc !important;
  border-color: #dee9fc !important;
  color: #0f172a !important;
}

.dark .layout-tab {
  color: #dee9fc !important;
}

.dark .layout-tab:hover {
  background: #2f3033 !important;
}

.dark .layout-tab--active {
  background: #2f3033 !important;
  border-color: #2f3033 !important;
  color: #e5e7eb !important;
}
</style>
