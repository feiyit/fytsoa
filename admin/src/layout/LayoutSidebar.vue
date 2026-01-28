<script setup lang="ts">
import { Icon } from "@iconify/vue";
interface AppMenuItem {
  key: string;
  label: string;
  icon?: string;
  path?: string;
  children?: AppMenuItem[];
}

defineProps<{
  appTitle: string;
  menus: AppMenuItem[];
  activePrimaryKey: string;
  secondaryMenus: AppMenuItem[];
  secondaryCollapsed: boolean;
  activePrimary: AppMenuItem | null;
  activeSecondaryKey: string | null;
}>();

const emit = defineEmits<{
  (e: "primary-click", item: AppMenuItem): void;
  (e: "secondary-click", item: AppMenuItem): void;
  (e: "toggle-secondary-collapse"): void;
}>();

const handlePrimaryClick = (item: AppMenuItem) => {
  emit("primary-click", item);
};

const handleSecondaryClick = (item: AppMenuItem) => {
  emit("secondary-click", item);
};

const toggleSecondaryCollapse = () => {
  emit("toggle-secondary-collapse");
};
</script>

<template>
  <!-- 左侧整体菜单区域 -->
  <aside
    class="flex h-full flex-col border-b border-slate-200/80 dark:border-slate-750 dark:bg-[#1c1e22] backdrop-blur"
  >
    <!-- 顶部标题 -->
    <div
      class="flex h-[50px] items-center px-4 font-semibold tracking-wide border-b border-slate-200/80 px-4 text-base dark:border-slate-750 dark:bg-[#1c1e22] backdrop-blur"
    >
      <div class="truncate flex items-center">
        <img
          src="/src/assets/flogo.png"
          class="w-[35px] h-[35px] mr-2"
          alt=""
          srcset=""
        />
        <span v-if="secondaryMenus.length">{{ appTitle }}</span>
      </div>
    </div>

    <!-- 下方：左右菜单区域 -->
    <div class="flex flex-1 min-h-0">
      <!-- 最左侧：一级菜单（宽度 80px） -->
      <div
        class="flex w-[86px] flex-col items-center border-r pt-2 pl-2 pr-2 border-b border-slate-200/80 dark:border-slate-750 dark:bg-[#1c1e22] backdrop-blur"
      >
        <div
          v-for="item in menus"
          :key="item.key"
          class="group flex w-full flex-col rounded-[8px] items-center gap-1 py-3 cursor-pointer transition-colors mb-1"
          :class="
            activePrimaryKey === item.key
              ? 'bg-[#006ce6] text-slate-100 dark:bg-slate-800 dark:text-primary-300'
              : 'dark:text-slate-300 dark:hover:bg-slate-800/80 dark:hover:text-white'
          "
          @click="handlePrimaryClick(item)"
        >
          <!-- <el-icon :size="20" class="mb-1">
            <component :is="item.icon || 'Menu'" />
          </el-icon> -->
          <icon :icon="item.icon" class="mb-1 w-5 h-5" />
          <span class="px-1 text-[13px] truncate">
            {{ item.label }}
          </span>
        </div>
      </div>

      <!-- 右侧：对应一级菜单下的二级/三级/四级菜单区域（使用 Element Plus Menu） -->
      <div
        v-if="secondaryMenus.length"
        class="flex flex-col min-h-0 border-r border-slate-200/80 dark:bj-[#1c1e22] transition-[width] duration-200 dark:border-slate-750"
        :class="secondaryCollapsed ? 'w-16' : 'w-[200px]'"
      >
        <header
          class="flex h-12 items-center px-2 border-b border-slate-200/80 dark:border-slate-750 dark:bg-[#1c1e22]"
        >
          <!-- 收齐状态下隐藏标题，只保留操作按钮 -->
          <span v-if="!secondaryCollapsed" class="truncate flex-1 px-2">
            {{ activePrimary?.label || "菜单" }}
          </span>

          <div
            class="flex items-center gap-1"
            :class="secondaryCollapsed ? 'flex-1 justify-center' : ''"
          >
            <!-- 收齐 / 展开 -->
            <el-tooltip content="收起 / 展开" placement="bottom">
              <el-button
                text
                circle
                size="small"
                @click.stop="toggleSecondaryCollapse"
              >
                <el-icon :size="14">
                  <DArrowLeft v-if="!secondaryCollapsed" />
                  <DArrowRight v-else />
                </el-icon>
              </el-button>
            </el-tooltip>
          </div>
        </header>

        <div class="flex-1 min-h-0 overflow-y-auto">
          <el-menu
            :default-active="activeSecondaryKey || ''"
            :collapse="secondaryCollapsed"
            class="border-0 bg-transparent"
            :unique-opened="true"
          >
            <template v-for="item in secondaryMenus" :key="item.key">
              <!-- 有子菜单的二级：使用 el-sub-menu -->
              <el-sub-menu
                v-if="item.children && item.children.length"
                :index="item.key"
              >
                <template #title>
                  <div class="menu-item">
                    <!-- <el-icon :size="14">
                      <component :is="item.icon || 'Menu'" />
                    </el-icon> -->
                    <icon :icon="item.icon" class="w-4 h-4 mr-1" />
                    <span class="ml-1 truncate">{{ item.label }}</span>
                  </div>
                </template>

                <!-- 三级 / 四级 -->
                <template v-for="child in item.children" :key="child.key">
                  <!-- 还有子级：继续用 sub-menu（三级） -->
                  <el-sub-menu
                    v-if="child.children && child.children.length"
                    :index="child.key"
                  >
                    <template #title>
                      <div class="menu-item">
                        <!-- <el-icon :size="13">
                          <component :is="child.icon || 'Menu'" />
                        </el-icon> -->
                        <icon :icon="item.icon" class="w-4 h-4 mr-1" />
                        <span class="ml-1 truncate">{{ child.label }}</span>
                      </div>
                    </template>

                    <!-- 四级 -->
                    <el-menu-item
                      v-for="grand in child.children"
                      :key="grand.key"
                      :index="grand.key"
                      @click="handleSecondaryClick(grand)"
                    >
                      <div class="menu-item">
                        <!-- <el-icon :size="12">
                          <component :is="grand.icon || 'Menu'" />
                        </el-icon> -->
                        <icon :icon="item.icon" class="w-4 h-4 mr-1" />
                        <span class="ml-1 truncate">{{ grand.label }}</span>
                      </div>
                    </el-menu-item>
                  </el-sub-menu>

                  <!-- 没有子级：作为普通菜单项（三级） -->
                  <el-menu-item
                    v-else
                    :index="child.key"
                    @click="handleSecondaryClick(child)"
                  >
                    <div class="menu-item">
                      <!-- <el-icon :size="13">
                        <component :is="child.icon || 'Menu'" />
                      </el-icon> -->
                      <icon :icon="item.icon" class="w-4 h-4 mr-1" />
                      <span class="ml-1 truncate">{{ child.label }}</span>
                    </div>
                  </el-menu-item>
                </template>
              </el-sub-menu>

              <!-- 没有子菜单的二级：普通菜单项 -->
              <el-menu-item
                v-else
                :index="item.key"
                @click="handleSecondaryClick(item)"
              >
                <div class="menu-item">
                  <!-- <el-icon :size="14">
                    <component :is="item.icon || 'Menu'" />
                  </el-icon> -->
                  <icon :icon="item.icon" class="w-4 h-4 mr-1" />
                  <span class="ml-1 truncate">{{ item.label }}</span>
                </div>
              </el-menu-item>
            </template>
          </el-menu>
        </div>
      </div>
    </div>
  </aside>
</template>
