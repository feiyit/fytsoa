<script setup lang="ts">
import dayjs from "dayjs";
import { useRouter } from "vue-router";
import {
  fetchSysNoticePage,
  fetchSysNoticeTotal,
  setSysNoticeRead,
} from "@/api";

interface BreadcrumbItem {
  path: string;
  label: string;
}

const { breadcrumbs, isDark, isFullscreen, username } = defineProps<{
  breadcrumbs: BreadcrumbItem[];
  isDark: boolean;
  isFullscreen: boolean;
  username: string;
}>();

const emit = defineEmits<{
  (e: "toggle-theme"): void;
  (e: "open-settings"): void;
  (e: "toggle-fullscreen"): void;
  (e: "logout"): void;
  (e: "usercenter"): void;
  (e: "lock-screen"): void;
}>();

const router = useRouter();

const noticePopoverVisible = ref(false);
const noticeLoading = ref(false);
const noticeTotal = reactive({ unread: 0 });
const latestUnread = ref<any[]>([]);

const formatDateTime = (v?: string | null) => {
  if (!v) return "";
  const d = dayjs(v);
  return d.isValid() ? d.format("MM-DD HH:mm") : String(v);
};

const loadNoticeTotal = async () => {
  try {
    const res: any = await fetchSysNoticeTotal();
    noticeTotal.unread = Number(res?.unread ?? 0);
  } catch {
    noticeTotal.unread = 0;
  }
};

const loadLatestUnread = async () => {
  noticeLoading.value = true;
  try {
    const res: any = await fetchSysNoticePage({
      page: 1,
      limit: 5,
      type: 2, // 收件箱
      readStatus: 1, // 未读
    });
    latestUnread.value = res?.items || [];
  } catch {
    latestUnread.value = [];
  } finally {
    noticeLoading.value = false;
  }
};

const refreshNotice = async () => {
  await Promise.all([loadNoticeTotal(), loadLatestUnread()]);
};

const markAllRead = async () => {
  try {
    // 空数组：全部已读（后端约定）
    await setSysNoticeRead([]);
  } finally {
    await refreshNotice();
  }
};

const goNotifyList = () => {
  noticePopoverVisible.value = false;
  router.push("/config/notify");
};

watch(
  () => noticePopoverVisible.value,
  (v) => {
    if (v) {
      refreshNotice();
    }
  },
);

onMounted(() => {
  loadNoticeTotal();
});
</script>

<template>
  <!-- 顶部操作栏：左侧面包屑 / 右侧操作区域 -->
  <header
    class="flex h-[50px] items-center justify-between border-b border-slate-200/80 bg-white/90 px-4 dark:border-slate-750 dark:bg-[#1c1e22] backdrop-blur"
  >
    <!-- 左侧：面包屑 -->
    <div class="flex items-center gap-2">
      <el-breadcrumb separator="/">
        <el-breadcrumb-item
          v-for="(bc, index) in breadcrumbs"
          :key="bc.path || index"
          :to="index < breadcrumbs.length - 1 ? { path: bc.path } : undefined"
        >
          {{ bc.label }}
        </el-breadcrumb-item>
      </el-breadcrumb>
    </div>

    <!-- 右侧：主题、设置、全屏、用户 -->
    <div class="flex items-center gap-2">
      <!-- 主题切换 -->
      <el-tooltip content="切换主题" placement="bottom">
        <el-button circle text size="small" @click="emit('toggle-theme')">
          <el-icon :size="16">
            <Moon v-if="isDark" />
            <Sunny v-else />
          </el-icon>
        </el-button>
      </el-tooltip>

      <!-- 通知提醒：用 ElTooltip(click + content slot) 实现“弹层面板”，交互更稳定 -->
      <el-tooltip
        v-model:visible="noticePopoverVisible"
        trigger="click"
        placement="bottom"
        effect="light"
        :enterable="true"
        :teleported="true"
        :z-index="30000"
        :popper-style="{ width: '360px' }"
        popper-class="notify-popper"
      >
        <template #content>
          <div class="flex items-center justify-between pt-1">
            <div class="text-sm font-medium">未读通知</div>
            <div class="text-xs text-slate-400">最新 5 条</div>
          </div>

          <div class="mt-3" v-loading="noticeLoading">
            <div
              v-if="latestUnread.length === 0"
              class="py-6 text-center text-sm text-slate-400"
            >
              暂无未读通知
            </div>
            <div v-else class="space-y-2">
              <div
                v-for="it in latestUnread"
                :key="String(it.id)"
                class="border-border hover:bg-slate-50 dark:hover:bg-slate-800/40 cursor-pointer rounded-md border px-3 py-2"
                @click="goNotifyList"
              >
                <div class="flex items-center justify-between gap-2">
                  <div
                    class="truncate text-sm text-slate-800 dark:text-slate-200"
                  >
                    {{ it.title || "-" }}
                  </div>
                  <div class="shrink-0 text-xs text-slate-400">
                    {{ formatDateTime(it.createTime) }}
                  </div>
                </div>
              </div>
            </div>

            <div class="mt-4 flex items-center justify-between pb-2">
              <el-button
                size="small"
                :disabled="noticeTotal.unread <= 0"
                @click="markAllRead"
              >
                全部已读
              </el-button>
              <el-button size="small" type="primary" @click="goNotifyList">
                消息列表
              </el-button>
            </div>
          </div>
        </template>

        <!-- trigger -->
        <span class="inline-flex" title="通知">
          <el-button circle text size="small">
            <el-badge
              :value="noticeTotal.unread"
              :hidden="noticeTotal.unread <= 0"
              type="danger"
            >
              <el-icon :size="16">
                <Bell />
              </el-icon>
            </el-badge>
          </el-button>
        </span>
      </el-tooltip>

      <!-- 偏好设置（抽屉） -->
      <el-tooltip content="偏好设置" placement="bottom">
        <el-button circle text size="small" @click="emit('open-settings')">
          <el-icon :size="16">
            <Setting />
          </el-icon>
        </el-button>
      </el-tooltip>

      <!-- 全屏 -->
      <el-tooltip
        :content="isFullscreen ? '退出全屏' : '全屏'"
        placement="bottom"
      >
        <el-button circle text size="small" @click="emit('toggle-fullscreen')">
          <el-icon :size="16">
            <FullScreen />
          </el-icon>
        </el-button>
      </el-tooltip>

      <!-- 登录用户 -->
      <el-dropdown>
        <span class="flex cursor-pointer items-center gap-2 select-none">
          <el-avatar
            :size="26"
            class="border border-slate-200 dark:border-slate-700"
          >
            {{ username[0] }}
          </el-avatar>
          <span
            class="hidden text-xs text-slate-700 dark:text-slate-200 sm:inline"
          >
            {{ username }}
          </span>
          <el-icon class="hidden text-slate-400 sm:inline">
            <ArrowDown />
          </el-icon>
        </span>
        <template #dropdown>
          <el-dropdown-menu>
            <el-dropdown-item @click="emit('usercenter')"
              >个人中心</el-dropdown-item
            >
            <el-dropdown-item @click="emit('usercenter')"
              >修改密码</el-dropdown-item
            >
            <el-dropdown-item @click="emit('lock-screen')">
              锁定屏幕
            </el-dropdown-item>
            <el-dropdown-item divided @click="emit('logout')">
              退出登录
            </el-dropdown-item>
          </el-dropdown-menu>
        </template>
      </el-dropdown>
    </div>
  </header>
</template>
