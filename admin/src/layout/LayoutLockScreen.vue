<script setup lang="ts">
import { appStorage, STORAGE_KEYS } from "@/utils/storage";
import { md5Encrypt } from "@/utils/crypto";
import { useUserStore } from "@/stores/user";

interface LockInfo {
  password: string; // MD5 后的密码
  lockedAt: number;
}

defineOptions({ name: "LayoutLockScreen" });

const userStore = useUserStore();
const router = useRouter();

const showSetDialog = ref(false);
const lockPassword = ref("");
const lockLoading = ref(false);

const isLocked = ref(false);
const showUnlockPanel = ref(false);
const unlockPassword = ref("");
const unlockLoading = ref(false);

const now = ref(new Date());

const LOCK_KEY = STORAGE_KEYS.SCREEN_LOCK;

const displayName = computed(() => userStore.displayName || "未登录用户");

const avatarText = computed(() =>
  (userStore.displayName || "U").slice(0, 1).toUpperCase()
);

const timeText = computed(() => {
  const d = now.value;
  const hours = d.getHours();
  const minutes = d.getMinutes().toString().padStart(2, "0");
  const ampm = hours >= 12 ? "PM" : "AM";
  const h12 = hours % 12 || 12;
  return { ampm, hours: h12.toString().padStart(2, "0"), minutes };
});

const dateText = computed(() => {
  const d = now.value;
  const yyyy = d.getFullYear();
  const mm = (d.getMonth() + 1).toString().padStart(2, "0");
  const dd = d.getDate().toString().padStart(2, "0");
  const weekdays = [
    "星期日",
    "星期一",
    "星期二",
    "星期三",
    "星期四",
    "星期五",
    "星期六",
  ];
  const w = weekdays[d.getDay()];
  return `${yyyy}-${mm}-${dd} ${w}`;
});

let timer: number | null = null;

const startClock = () => {
  if (timer !== null) return;
  timer = window.setInterval(() => {
    now.value = new Date();
  }, 1000);
};

const stopClock = () => {
  if (timer !== null) {
    window.clearInterval(timer);
    timer = null;
  }
};

const loadLockInfo = () => {
  const info = appStorage.get<LockInfo>(LOCK_KEY);
  if (info && info.password) {
    isLocked.value = true;
    showUnlockPanel.value = false;
    startClock();
  }
};

onMounted(() => {
  loadLockInfo();
});

onBeforeUnmount(() => {
  stopClock();
});

const openSetDialog = () => {
  // 如果已经锁定，直接展示锁屏
  if (isLocked.value) {
    showUnlockPanel.value = false;
    startClock();
    return;
  }
  lockPassword.value = "";
  showSetDialog.value = true;
};

const handleConfirmLock = async () => {
  if (!lockPassword.value) {
    ElMessage.warning("请输入锁屏密码");
    return;
  }
  lockLoading.value = true;
  try {
    const info: LockInfo = {
      password: md5Encrypt(lockPassword.value),
      lockedAt: Date.now(),
    };
    appStorage.set(LOCK_KEY, info);
    isLocked.value = true;
    showSetDialog.value = false;
    lockPassword.value = "";
    unlockPassword.value = "";
    showUnlockPanel.value = false;
    startClock();
  } finally {
    lockLoading.value = false;
  }
};

const handleClickUnlock = () => {
  showUnlockPanel.value = true;
};

const handleBackToClock = () => {
  showUnlockPanel.value = false;
  unlockPassword.value = "";
};

const handleUnlock = async () => {
  if (!unlockPassword.value) {
    ElMessage.warning("请输入锁屏密码");
    return;
  }
  const info = appStorage.get<LockInfo>(LOCK_KEY);
  if (!info || !info.password) {
    ElMessage.error("当前未处于锁定状态");
    isLocked.value = false;
    stopClock();
    return;
  }

  const inputHash = md5Encrypt(unlockPassword.value);
  if (inputHash !== info.password) {
    ElMessage.error("锁屏密码错误");
    return;
  }

  unlockLoading.value = true;
  try {
    appStorage.remove(LOCK_KEY);
    isLocked.value = false;
    unlockPassword.value = "";
    showUnlockPanel.value = false;
    stopClock();
    ElMessage.success("解锁成功");
  } finally {
    unlockLoading.value = false;
  }
};

const handleToLogin = () => {
  appStorage.remove(LOCK_KEY);
  isLocked.value = false;
  stopClock();
  router.push("/login");
};

defineExpose({
  openSetDialog,
});
</script>

<template>
  <!-- 步骤1：弹窗设置锁屏密码 -->
  <el-dialog
    v-model="showSetDialog"
    title="锁定屏幕"
    width="420px"
    :close-on-click-modal="false"
    :close-on-press-escape="false"
    append-to-body
  >
    <div class="flex flex-col items-center py-2">
      <el-avatar :size="80" class="mb-3">
        {{ avatarText }}
      </el-avatar>
      <div class="mb-6 text-base font-semibold">{{ displayName }}</div>

      <el-input
        v-model="lockPassword"
        type="password"
        show-password
        placeholder="请输入锁屏密码"
        class="w-full mb-4"
      />

      <el-button
        type="primary"
        class="w-full"
        :loading="lockLoading"
        @click="handleConfirmLock"
      >
        锁定
      </el-button>
    </div>
  </el-dialog>

  <!-- 步骤2/3：全屏锁定层 -->
  <transition name="fade">
    <div
      v-if="isLocked"
      class="fixed inset-0 z-[1000] flex flex-col items-center bg-white dark:bg-slate-950"
    >
      <!-- 解锁面板 -->
      <div
        v-if="showUnlockPanel"
        class="mt-32 flex w-full max-w-md flex-col items-center px-4"
      >
        <el-avatar :size="80" class="mb-3">
          {{ avatarText }}
        </el-avatar>
        <div class="mb-6 text-base font-semibold">{{ displayName }}</div>

        <el-input
          v-model="unlockPassword"
          type="password"
          show-password
          placeholder="请输入锁屏密码"
          class="w-full mb-4"
        />

        <el-button
          type="primary"
          class="w-full"
          :loading="unlockLoading"
          @click="handleUnlock"
        >
          进入系统
        </el-button>

        <div class="mt-4 flex flex-col items-center text-xs text-slate-500">
          <button class="mb-1 hover:text-primary-500" @click="handleToLogin">
            返回登录
          </button>
          <button class="hover:text-primary-500" @click="handleBackToClock">
            返回
          </button>
        </div>
      </div>

      <!-- 时钟视图 -->
      <div v-else class="mt-24 flex flex-col items-center">
        <div class="mb-4 flex flex-col items-center text-sm text-slate-500">
          <el-icon class="mb-1 text-slate-400">
            <Lock />
          </el-icon>
          <button
            type="button"
            class="text-slate-600 hover:text-primary-500"
            @click="handleClickUnlock"
          >
            点击解锁
          </button>
        </div>

        <div class="flex gap-4">
          <div
            class="flex h-24 w-28 flex-col items-center justify-center rounded-2xl bg-slate-100 text-slate-800 shadow-sm"
          >
            <span class="mb-1 text-xs text-slate-500">
              {{ timeText.ampm }}
            </span>
            <span class="text-4xl font-semibold">
              {{ timeText.hours }}
            </span>
          </div>
          <div
            class="flex h-24 w-28 flex-col items-center justify-center rounded-2xl bg-slate-100 text-slate-800 shadow-sm"
          >
            <span class="mb-1 text-xs text-slate-500">MIN</span>
            <span class="text-4xl font-semibold">
              {{ timeText.minutes }}
            </span>
          </div>
        </div>
      </div>

      <!-- 底部日期和时间 -->
      <div class="mt-auto mb-10 text-center text-sm text-slate-500">
        <div class="mb-2">
          {{ `${timeText.hours}:${timeText.minutes} ${timeText.ampm}` }}
        </div>
        <div>{{ dateText }}</div>
      </div>
    </div>
  </transition>
</template>

<style scoped>
.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.25s ease;
}
.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}
</style>
