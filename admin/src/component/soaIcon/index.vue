<script lang="ts" setup>
import { Icon } from "@iconify/vue";
import { useSoaModal } from "@/component/soaModal/index.vue";
import { iconNames } from "./lucide-icons";
const activeName = ref("first");
// 定义传出的事件
const emit = defineEmits(["update:modelValue"]);
const props = defineProps({
  selectedIcon: String,
  modelValue: String,
});
const [Modal, modalApi] = useSoaModal({
  onConfirm: () => {
    confirmSelection();
  },
});
const openIconPicker = () => {
  modalApi.open();
};
const selectedIcon = ref<any>(props.modelValue);

watch(
  () => props.modelValue,
  (newVal) => {
    selectedIcon.value = newVal;
  }
);
const filteredIcons = ref(iconNames);
const selectIcon = (icon: any) => {
  selectedIcon.value = icon;
  confirmSelection();
};
const searchQuery = ref("");
const filterIcons = () => {
  filteredIcons.value = iconNames.filter((icon) =>
    icon.toLowerCase().includes(searchQuery.value.toLowerCase())
  );
};
const confirmSelection = () => {
  emit("update:modelValue", selectedIcon.value);
  modalApi.close();
};
</script>
<template>
  <Modal title="图标选择器" draggable :closeOnClickModal="false">
    <el-tabs v-model="activeName">
      <el-tab-pane label="选择图标" name="first">
        <el-input
          v-model="searchQuery"
          placeholder="搜索图标..."
          class="mb-4"
          @input="filterIcons"
        />
        <div class="icon-list">
          <el-row :gutter="20">
            <el-col :span="4" v-for="icon in filteredIcons" :key="icon">
              <div
                class="icon-card mb-2 rounded-md border"
                @click="selectIcon(icon)"
              >
                <div class="icon-wrapper mb-2 flex justify-center p-5">
                  <icon :icon="icon" />
                </div>
                <!-- <p class="icon-name pb-3">{{ icon }}</p> -->
              </div>
            </el-col>
          </el-row>
        </div>
      </el-tab-pane>
      <el-tab-pane label="手动输入" name="second">
        <p class="text-sm text-gray-300">
          集成了
          <a
            href="https://iconify.design/"
            class="text-blue-500"
            target="_blank"
            >iconify</a
          >
          图标库
        </p>
        <el-input
          v-model="selectedIcon"
          style="width: 100%"
          class="mt-5"
          placeholder="请输入Icon名称"
          ><template #prepend><icon :icon="selectedIcon"></icon></template
        ></el-input>
      </el-tab-pane>
    </el-tabs>
  </Modal>
  <el-button @click="openIconPicker">
    <icon class="mr-2 text-lg" :icon="selectedIcon" v-if="selectedIcon" />
    选择图标
  </el-button>
</template>

<style scoped>
.icon-list {
  max-height: 400px;
  overflow-y: auto;
}
.icon-card {
  text-align: center;
  cursor: pointer;
}
.icon-wrapper {
  font-size: 20px;
}
.icon-name {
  font-size: 12px;
  margin-top: 8px;
}
.dialog-footer {
  text-align: center;
}
</style>
