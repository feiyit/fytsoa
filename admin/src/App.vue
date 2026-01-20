<script setup lang="ts">
import { useAppStore } from "./stores/app";
import { ElConfigProvider } from "element-plus";

defineOptions({
  name: "App",
});

const appStore = useAppStore();
const isLoading = computed(() => appStore.loading);
</script>

<template>
  <div class="app-root">
    <Transition name="app-fade">
      <div v-if="isLoading" class="app-loading">
        <div class="app-loading__wrap">
          <h1 class="app-loading__title">FytAdmin 管理系统</h1>

          <div class="flex w-full flex-col items-center">
            <div class="app-loading__orbital">
              <div class="app-loading__outer-orbit">
                <div class="app-loading__outer-orbit-dot"></div>
              </div>
              <div class="app-loading__ring app-loading__ring--outer"></div>
              <div class="app-loading__ring app-loading__ring--middle"></div>
              <div class="app-loading__ring app-loading__ring--inner"></div>
              <div class="app-loading__dot"></div>
            </div>

            <p class="app-loading__text">初始化系统 · 加载资源中...</p>
          </div>
        </div>
      </div>
    </Transition>

    <ElConfigProvider>
      <RouterView v-if="!isLoading" />
    </ElConfigProvider>
  </div>
</template>
