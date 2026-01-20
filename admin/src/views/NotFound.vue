<script setup lang="ts">
const router = useRouter();

const goHome = () => {
  router.push("/");
};
</script>

<template>
  <div
    class="flex min-h-screen flex-col items-center justify-center bg-slate-50 text-slate-800 dark:bg-slate-900 dark:text-slate-100 px-4"
  >
    <div class="max-w-md text-center">
      <p class="text-sm font-semibold tracking-widest text-sky-500 mb-2">
        404
      </p>
      <h1 class="mb-3 text-3xl font-bold tracking-tight sm:text-4xl">
        页面未找到
      </h1>
      <p class="mb-6 text-sm text-slate-500 dark:text-slate-400">
        很抱歉，您访问的页面不存在、已被移动或暂时不可用。
      </p>

      <div class="flex flex-wrap items-center justify-center gap-3">
        <el-button type="primary" @click="goHome">
          返回首页
        </el-button>
        <el-button text @click="router.back()">
          返回上一页
        </el-button>
      </div>
    </div>
  </div>
</template>

