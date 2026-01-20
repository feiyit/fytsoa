<template>
  <!-- 引导页：提示先在左侧选择栏目，再在右侧维护内容 -->
  <div class="h-full">
    <div
      class="bg-card ring-border relative flex h-full min-h-[360px] items-center justify-center rounded-[.5vw]"
    >
      <div class="max-w-xl text-center">
        <div
          class="bg-primary/10 text-primary mx-auto mb-4 flex h-14 w-14 items-center justify-center rounded-full"
        >
          <el-icon class="text-2xl"><EditPen /></el-icon>
        </div>
        <h2 class="text-foreground text-2xl font-semibold">请选择左侧的栏目</h2>
        <p class="text-muted-foreground mt-2 text-sm">
          通过右侧区域来维护相关的内容
        </p>

        <div class="mt-6 space-y-2 text-left">
          <div class="flex items-start gap-2">
            <span
              class="bg-primary mt-0.5 inline-flex h-5 w-5 items-center justify-center rounded-full text-xs font-semibold text-white"
              >1</span
            >
            <p class="text-foreground text-sm">在左侧选择或新建一个栏目。</p>
          </div>
          <div class="flex items-start gap-2">
            <span
              class="bg-primary mt-0.5 inline-flex h-5 w-5 items-center justify-center rounded-full text-xs font-semibold text-white"
              >2</span
            >
            <p class="text-foreground text-sm">在右侧编辑、上传素材并保存。</p>
          </div>
          <div class="flex items-start gap-2">
            <span
              class="bg-primary mt-0.5 inline-flex h-5 w-5 items-center justify-center rounded-full text-xs font-semibold text-white"
              >3</span
            >
            <p class="text-foreground text-sm">支持草稿/发布，随时预览效果。</p>
          </div>
        </div>

        <div
          class="text-muted-foreground mt-6 flex items-center justify-center gap-3 text-xs"
        >
          <span class="inline-flex items-center gap-1">
            <span class="i-heroicons-cursor-arrow-ripple h-4 w-4"></span>
            点击栏目
          </span>
          <span>→</span>
          <span class="inline-flex items-center gap-1">
            <span class="i-heroicons-pencil-square h-4 w-4"></span>
            在右侧维护内容
          </span>
        </div>
      </div>

      <!-- 左侧引导标注（中等以上屏幕显示） -->
      <div
        class="text-muted-foreground pointer-events-none absolute left-3 top-1/2 hidden -translate-y-1/2 items-center gap-2 md:flex"
      >
        <span class="i-heroicons-arrow-long-left h-5 w-5"></span>
        <span class="text-xs">栏目在这里</span>
      </div>
      <!-- 右侧引导标注（中等以上屏幕显示） -->
      <div
        class="text-muted-foreground pointer-events-none absolute right-3 top-1/2 hidden -translate-y-1/2 items-center gap-2 md:flex"
      >
        <span class="text-xs">在此维护内容</span>
        <span class="i-heroicons-arrow-long-right h-5 w-5"></span>
      </div>
    </div>
  </div>
</template>
