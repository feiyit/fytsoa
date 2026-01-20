<script setup lang="ts">
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
