<script setup lang="ts">
// import { dynamicRoutes } from "@/router/dynamic-routes";
import { getServerDynamicRoutes } from "@/utils/routeHandler";
import { useAppStore } from "@/stores/app";
import { useUserStore } from "@/stores/user";
import LayoutSidebar from "@/layout/LayoutSidebar.vue";
import LayoutHeader from "@/layout/LayoutHeader.vue";
import LayoutTabs from "@/layout/LayoutTabs.vue";
import LayoutLockScreen from "@/layout/LayoutLockScreen.vue";
import { appStorage, STORAGE_KEYS } from "@/utils/storage";

defineOptions({
  name: "BasicLayout",
});
const dynamicRoutes = getServerDynamicRoutes();
interface AppMenuItem {
  key: string;
  label: string;
  icon?: string;
  path?: string;
  children?: AppMenuItem[];
}

const router = useRouter();
const route = useRoute();
const appStore = useAppStore();
const userStore = useUserStore();

const appTitle = computed(() => {
  const base = appStore.title || "FytAdmin 管理系统";
  return `${base}`;
});

// 右上区域：主题切换 / 全屏 / 设置 / 用户信息
const isDark = computed(() => appStore.theme === "dark");
const toggleTheme = () => appStore.toggleTheme();

const isFullscreen = ref(false);
const settingsVisible = ref(false);

const username = computed(() => userStore.displayName);

// 二级菜单区域收起 / 展开状态
const secondaryCollapsed = ref(false);

// 内容区域重新加载 key，用于强制重建当前路由组件
const viewReloadKey = ref(0);

const SECONDARY_MENU_STATE_KEY = STORAGE_KEYS.SECONDARY_MENU_STATE;
const TAGS_STATE_KEY = STORAGE_KEYS.NAV_TAGS;

onMounted(() => {
  const raw = appStorage.get<string>(SECONDARY_MENU_STATE_KEY);
  if (!raw) return;
  try {
    const parsed = JSON.parse(raw) as {
      collapsed?: boolean;
    };
    secondaryCollapsed.value = !!parsed.collapsed;
  } catch {
    // ignore parse error
  }
});

// 监听全屏状态变化
const handleFullscreenChange = () => {
  if (typeof document === "undefined") return;
  // 任何元素进入 / 退出全屏都更新状态
  isFullscreen.value = !!document.fullscreenElement;
};

onMounted(() => {
  if (typeof document === "undefined") return;
  document.addEventListener("fullscreenchange", handleFullscreenChange);
});

onBeforeUnmount(() => {
  if (typeof document === "undefined") return;
  document.removeEventListener("fullscreenchange", handleFullscreenChange);
});

// 递归根据动态路由构建菜单（支持 2/3/4 级）
const buildMenuItemFromRoute = (
  routeRecord: any,
  parentPath = ""
): AppMenuItem => {
  const rawPath = String(routeRecord.path || "");
  const fullPath = rawPath.startsWith("/")
    ? rawPath
    : `${parentPath}/${rawPath}`.replace(/\/+/g, "/");

  const key =
    (routeRecord.meta?.menuKey as string | undefined) ||
    (routeRecord.name as string | undefined) ||
    fullPath;

  const label =
    (routeRecord.meta?.title as string | undefined) ||
    (routeRecord.name as string | undefined) ||
    rawPath ||
    fullPath;

  const icon = (routeRecord.meta?.icon as string | undefined) || "Menu";

  const children: AppMenuItem[] =
    (routeRecord.children || []).map((child: any) =>
      buildMenuItemFromRoute(child, fullPath)
    ) || [];

  return {
    key,
    label,
    icon,
    path: fullPath,
    children,
  };
};

const menus = computed<AppMenuItem[]>(() => {
  return dynamicRoutes.map((routeRecord) =>
    buildMenuItemFromRoute(routeRecord)
  );
});

const activePrimaryKey = ref<string>("");
const activeSecondaryKey = ref<string | null>(null);

const activePrimary = computed<AppMenuItem | null>(() => {
  const list = menus.value;
  if (!list.length) return null;
  return list.find((m) => m.key === activePrimaryKey.value) || list[0] || null;
});

const secondaryMenus = computed<AppMenuItem[]>(() => {
  return activePrimary.value?.children || [];
});

// 工具：在某个菜单树中根据路径查找叶子菜单 key
const findLeafKeyByPath = (
  items: AppMenuItem[],
  path: string
): string | null => {
  for (const item of items) {
    if (item.path === path) return item.key;
    if (item.children && item.children.length) {
      const nested = findLeafKeyByPath(item.children, path);
      if (nested) return nested;
    }
  }
  return null;
};

// 工具：根据当前 path 在菜单树中找到从顶级到叶子的完整路径，用于生成多级面包屑
const findMenuPathByPath = (
  items: AppMenuItem[],
  path: string,
  parents: AppMenuItem[] = []
): AppMenuItem[] | null => {
  for (const item of items) {
    const currentChain = [...parents, item];
    if (item.path === path) {
      return currentChain;
    }
    if (item.children && item.children.length) {
      const childChain = findMenuPathByPath(item.children, path, currentChain);
      if (childChain) return childChain;
    }
  }
  return null;
};

// 路由变化时同步选中菜单（按照 path 匹配，支持多级）
watch(
  () => route.path,
  (path) => {
    const list = menus.value;
    for (const primary of list) {
      if (primary.children && primary.children.length) {
        const leafKey = findLeafKeyByPath(primary.children, path);
        if (leafKey) {
          activePrimaryKey.value = primary.key;
          activeSecondaryKey.value = leafKey;
          return;
        }
      }
      if (primary.path === path) {
        activePrimaryKey.value = primary.key;
        activeSecondaryKey.value = primary.key;
        return;
      }
    }
  },
  { immediate: true }
);

// 监听二级菜单折叠状态变化，持久化到本地存储
watch(secondaryCollapsed, (collapsed) => {
  appStorage.set(SECONDARY_MENU_STATE_KEY, JSON.stringify({ collapsed }));
});

// 初始化默认选中的一级菜单
watch(
  menus,
  (list) => {
    const first = list[0];
    if (!activePrimaryKey.value && first) {
      activePrimaryKey.value = first.key;
    }
  },
  { immediate: true }
);

const handlePrimaryClick = (item: AppMenuItem) => {
  activePrimaryKey.value = item.key;

  // 找到第一个可跳转的叶子菜单
  const findFirstLeaf = (
    node: AppMenuItem | null | undefined
  ): AppMenuItem | null => {
    if (!node) return null;
    if (node.children && node.children.length) {
      return findFirstLeaf(node.children[0]);
    }
    return node.path ? node : null;
  };

  if (item.children && item.children.length > 0) {
    const leaf = findFirstLeaf(item.children[0]) || item;
    activeSecondaryKey.value = leaf.key;
    if (leaf.path) {
      router.push(leaf.path);
    }
  } else if (item.path) {
    activeSecondaryKey.value = item.key;
    router.push(item.path);
  }
};

// 面包屑：优先根据菜单结构生成，支持 2/3/4 级菜单路径
const breadcrumbs = computed(() => {
  const path = route.path;
  const menuTrail =
    menus.value.length > 0 ? findMenuPathByPath(menus.value, path) : null;

  if (menuTrail && menuTrail.length) {
    return menuTrail.map((item) => ({
      path: item.path || "",
      label: item.label,
    }));
  }

  // 回退：找不到对应菜单时，使用路由本身的信息
  const title =
    (route.meta?.title as string | undefined) ||
    (route.name as string | undefined) ||
    "当前页面";

  return [
    {
      path,
      label: title,
    },
  ];
});

const toggleSecondaryCollapse = () => {
  secondaryCollapsed.value = !secondaryCollapsed.value;
};

const handleSecondaryClick = (item: AppMenuItem) => {
  activeSecondaryKey.value = item.key;
  if (item.path) {
    router.push(item.path);
  }
};

const toggleFullscreen = async () => {
  if (typeof document === "undefined") return;
  try {
    if (!document.fullscreenElement) {
      await document.documentElement.requestFullscreen();
    } else if (document.exitFullscreen) {
      await document.exitFullscreen();
    }
  } catch (error) {
    console.error("切换全屏失败:", error);
  }
};

const openSettings = () => {
  settingsVisible.value = true;
};

const handleLogout = async () => {
  try {
    await ElMessageBox.confirm("确定要退出当前登录吗？", "提示", {
      confirmButtonText: "确定",
      cancelButtonText: "取消",
      type: "warning",
    });
  } catch {
    // 用户取消退出
    return;
  }

  // 清理登录状态（token + 用户信息）
  userStore.clearUser();
  // 清理与当前会话相关的本地状态，但保留记住账号等个人偏好
  appStorage.remove(STORAGE_KEYS.ROUTER_MENU);
  appStorage.remove(STORAGE_KEYS.NAV_TAGS);
  appStorage.remove(STORAGE_KEYS.SECONDARY_MENU_STATE);
  router.push("/login");
};

const handleGoUser = async () => {
  router.push("/usercenter");
};

// 右侧第二行：多标签导航
interface NavTag {
  path: string;
  title: string;
  fixed?: boolean;
}

const tags = ref<NavTag[]>([]);

type TagContextCommand =
  | "reload"
  | "close"
  | "closeLeft"
  | "closeRight"
  | "closeOthers"
  | "closeAll";

const addTagFromRoute = (to: typeof route) => {
  const path = to.fullPath;
  // 登录页等不加入标签
  if (path === "/login") return;

  const title =
    (to.meta?.title as string | undefined) ||
    (to.name as string | undefined) ||
    "未命名页面";

  const fixed = !!to.meta?.fixed;

  const exists = tags.value.some((t) => t.path === path);
  if (!exists) {
    tags.value.push({ path, title, fixed });
  }
};

// 初始化时从本地恢复标签状态，并加入当前路由标签
onMounted(() => {
  const raw = appStorage.get<string>(TAGS_STATE_KEY);
  if (raw) {
    try {
      const parsed = JSON.parse(raw) as NavTag[];
      if (Array.isArray(parsed)) {
        // 过滤掉无效数据
        tags.value = parsed.filter((t) => !!t && !!t.path && !!t.title);
      }
    } catch {
      // ignore parse error
    }
  }

  addTagFromRoute(route);
});

// 路由切换时同步标签
watch(
  () => route.fullPath,
  () => {
    addTagFromRoute(route);
  }
);

// 持久化标签状态（包括固定标记）
watch(
  tags,
  (list) => {
    const plain = list.map((t) => ({
      path: t.path,
      title: t.title,
      fixed: !!t.fixed,
    }));
    appStorage.set(TAGS_STATE_KEY, JSON.stringify(plain));
  },
  { deep: true }
);

const lockRef = ref<InstanceType<typeof LayoutLockScreen> | null>(null);

const handleLockScreen = () => {
  lockRef.value?.openSetDialog();
};

const handleTagClick = (tag: NavTag) => {
  if (tag.path !== route.fullPath) {
    router.push(tag.path);
  }
};

const handleTagClose = (tag: NavTag) => {
  // 固定标签不可关闭
  if (tag.fixed) return;

  const index = tags.value.findIndex((t) => t.path === tag.path);
  if (index === -1) return;

  const isActive = route.fullPath === tag.path;
  tags.value.splice(index, 1);

  if (!isActive) return;

  const nextTags = tags.value;
  if (!nextTags.length) {
    router.push("/");
    return;
  }

  const next = nextTags[index] || nextTags[index - 1] || nextTags[0];
  if (next) {
    router.push(next.path);
  } else {
    router.push("/");
  }
};

const closeLeftTags = (tag: NavTag) => {
  const index = tags.value.findIndex((t) => t.path === tag.path);
  if (index <= 0) return;

  const original = [...tags.value];
  const kept: NavTag[] = [];
  original.forEach((t, i) => {
    if (i >= index || t.fixed) kept.push(t);
  });
  const removed = original.filter((t) => !kept.includes(t));
  tags.value = kept;

  const activeRemoved = removed.some((t) => t.path === route.fullPath);
  if (activeRemoved && route.fullPath !== tag.path) {
    router.push(tag.path);
  }
};

const closeRightTags = (tag: NavTag) => {
  const index = tags.value.findIndex((t) => t.path === tag.path);
  if (index === -1 || index >= tags.value.length - 1) return;

  const original = [...tags.value];
  const kept: NavTag[] = [];
  original.forEach((t, i) => {
    if (i <= index || t.fixed) kept.push(t);
  });
  const removed = original.filter((t) => !kept.includes(t));
  tags.value = kept;

  const activeRemoved = removed.some((t) => t.path === route.fullPath);
  if (activeRemoved && route.fullPath !== tag.path) {
    router.push(tag.path);
  }
};

const closeOtherTags = (tag: NavTag) => {
  tags.value = tags.value.filter((t) => t.fixed || t.path === tag.path);
  if (route.fullPath !== tag.path) {
    router.push(tag.path);
  }
};

const closeAllTags = () => {
  const fixedTags = tags.value.filter((t) => t.fixed);
  tags.value = fixedTags;

  if (fixedTags.length) {
    const target =
      fixedTags.find((t) => t.path === route.fullPath) || fixedTags[0];
    if (target && target.path !== route.fullPath) {
      router.push(target.path);
    } else if (!target) {
      router.push("/");
    }
  } else {
    router.push("/");
  }
};

const handleTagContextCommand = (command: TagContextCommand, tag: NavTag) => {
  switch (command) {
    case "reload":
      if (route.fullPath === tag.path) {
        // 只重新挂载当前内容区域组件，而不是整页刷新
        viewReloadKey.value++;
      } else {
        router.push(tag.path);
      }
      break;
    case "close":
      handleTagClose(tag);
      break;
    case "closeLeft":
      closeLeftTags(tag);
      break;
    case "closeRight":
      closeRightTags(tag);
      break;
    case "closeOthers":
      closeOtherTags(tag);
      break;
    case "closeAll":
      closeAllTags();
      break;
    default:
      break;
  }
};
</script>

<template>
  <div
    class="flex h-screen w-full overflow-x-hidden bg-slate-100 text-slate-900 dark:bg-slate-950 dark:text-slate-100"
  >
    <!-- 左侧整体菜单区域（组件化） -->
    <LayoutSidebar
      :app-title="appTitle"
      :menus="menus"
      :active-primary-key="activePrimaryKey"
      :secondary-menus="secondaryMenus"
      :secondary-collapsed="secondaryCollapsed"
      :active-primary="activePrimary"
      :active-secondary-key="activeSecondaryKey"
      @primary-click="handlePrimaryClick"
      @secondary-click="handleSecondaryClick"
      @toggle-secondary-collapse="toggleSecondaryCollapse"
    />

    <!-- 右侧：顶部栏 + 标签栏 + 内容区域 -->
    <section class="flex h-full flex-1 min-w-0 flex-col">
      <LayoutHeader
        :breadcrumbs="breadcrumbs"
        :is-dark="isDark"
        :is-fullscreen="isFullscreen"
        :username="username"
        @toggle-theme="toggleTheme"
        @open-settings="openSettings"
        @toggle-fullscreen="toggleFullscreen"
        @logout="handleLogout"
        @usercenter="handleGoUser"
        @lock-screen="handleLockScreen"
      />

      <LayoutTabs
        :tags="tags"
        :active-path="route.fullPath"
        @click-tag="handleTagClick"
        @close-tag="handleTagClose"
        @context-command="handleTagContextCommand"
      />

      <!-- 主内容区域 -->
      <main
        class="flex-1 overflow-auto bg-[#f2f3f6] p-4 dark:bg-[#14161a] sm:p-2"
      >
        <RouterView v-slot="{ Component, route }">
          <Transition name="fade" mode="out-in">
            <component
              :is="Component"
              :key="`${route.fullPath}-${viewReloadKey}`"
            />
          </Transition>
        </RouterView>
      </main>

      <!-- 偏好设置抽屉 -->
      <el-drawer
        v-model="settingsVisible"
        title="偏好设置"
        direction="rtl"
        size="320px"
      >
        <p class="mb-3 text-xs text-slate-500 dark:text-slate-400">
          这里可以放置主题、布局、导航等偏好设置内容。
        </p>
        <el-divider />
        <p class="text-xs text-slate-400">
          当前仅为占位示例，后续可根据需求增加具体配置项。
        </p>
      </el-drawer>
    </section>

    <!-- 锁屏组件 -->
    <LayoutLockScreen ref="lockRef" />
  </div>
</template>
