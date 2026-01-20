<template>
  <el-splitter>
    <el-splitter-panel size="18%">
      <div class="bg-card border-border h-full rounded-[.5vw] p-2">
        <div
          class="mb-3 mt-1 flex items-center justify-between text-[14.5px] font-medium"
        >
          <div class="flex items-center">
            <el-icon class="mr-2" size="20px"><Menu /></el-icon> 栏目列表
          </div>
        </div>
        <div class="org-tree space-y-2">
          <el-scrollbar height="80vh">
            <el-tree
              ref="groupRef"
              node-key="id"
              default-expand-all
              :data="columnTree"
              :current-node-key="''"
              :highlight-current="true"
              :expand-on-click-node="false"
              @node-click="groupClick"
            ></el-tree>
          </el-scrollbar>
        </div>
      </div>
    </el-splitter-panel>
    <el-splitter-panel :min="200" class="pl-2">
      <div class="h-full">
        <component
          :is="pageComponent"
          v-bind="currentParams"
          :columnId="selectColumnId"
          @complete="initTree"
        />
      </div>
    </el-splitter-panel>
  </el-splitter>
</template>
<script setup lang="ts">
import { fetchCmsColumnList } from "@/api/cms/column";
import { changeTree } from "@/utils/tools";
const defaults = defineAsyncComponent(() => import("./default.vue"));
const articles = defineAsyncComponent(() => import("../article/index.vue"));
const columnUpdate = defineAsyncComponent(() => import("../column/update.vue"));
const messages = defineAsyncComponent(() => import("../message/index.vue"));
const products = defineAsyncComponent(() => import("../product/index.vue"));
const columnTree = ref([]);
const componentMap = {
  default: defaults,
  articles: articles,
  columnUpdate: columnUpdate,
  message: messages,
  product: products,
};
const selectColumnId = ref("0");
const page = ref<keyof typeof componentMap>("default");
const pageComponent = computed(() => componentMap[page.value]);
const currentParams = ref<Record<string, any>>({});
const initTree = async () => {
  const tree = await fetchCmsColumnList();
  let _tree: any = [];
  tree.some((m) => {
    _tree.push({
      id: m.id,
      value: m.id,
      label: m.title,
      parentId: m.parentId,
      templateId: m.templateId,
    });
  });
  columnTree.value = changeTree(_tree);
};
const groupClick = (data: any) => {
  if (data.templateId == "1365210485116506112") {
    page.value = "columnUpdate";
  }
  if (data.templateId == "1390623120041316352") {
    page.value = "articles";
  }
  if (data.templateId == "1991082190145982464") {
    page.value = "message";
  }
  if (data.templateId == "2011348881002074113") {
    page.value = "product";
  }
  // currentParams.value = {
  //   id: data.id,
  // };
  currentParams.value = {}; // 先清空
  // 下一帧再赋值（避免组件未销毁导致的参数未更新）
  setTimeout(() => {
    currentParams.value = { id: data.id };
    selectColumnId.value = data.id;
  }, 0);
};
onMounted(() => {
  initTree();
});
</script>
<style scoped>
:deep(.org-tree .el-tree-node__content) {
  height: 36px;
  border-radius: 8px;
}
:deep(.el-scrollbar__view) {
  height: 100%;
}
</style>
