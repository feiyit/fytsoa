<template>
  <div class="tinymce-editor">
    <tiny-editor
      v-model="editorContent"
      :disabled="disabled"
      :init="editorInit"
      license-key="gpl"
      tinymce-script-src="/tinymce/tinymce.min.js"
      :key="themeKey"
      v-bind="$attrs"
      @init="onInit"
    />
  </div>
</template>

<script setup lang="ts">
import TinyEditor from "@tinymce/tinymce-vue";
import { uploadServiceFile } from "@/api/cms/file";

// import tinymce from 'tinymce/tinymce';
// import 'tinymce/plugins/template';
import { customTemplates } from "./templates/custom-templates.js";

// 组件 props 定义
const props = defineProps<{
  modelValue: string; // 双向绑定的内容
  disabled?: boolean; // 是否禁用
  height?: number; // 编辑器高度（默认300）
  uploadUrl?: string; // 图片上传接口地址
}>();

// 组件 emits 定义
const emit = defineEmits(["update:modelValue", "initialized"]);
// 编辑器内容（内部维护，与 props 双向绑定）
const editorContent = ref(props.modelValue);
const editorInstance = ref(null);

const isDarkMode = () => {
  return document.documentElement.classList.contains("dark");
};

// 监听 props 变化，同步到内部值
watch(
  () => props.modelValue,
  (newVal) => {
    if (newVal !== editorContent.value) {
      editorContent.value = newVal;
    }
  },
  { immediate: true }
);
watch(
  () => editorContent.value,
  (val) => {
    emit("update:modelValue", val);
  }
);
function onInit(e: any) {
  emit("initialized", e.target);
  editorInstance.value = e;
}
const apiURL = import.meta.env.BASE_URL;
const baseUrl = apiURL.replace("/api", "");
// 编辑器初始化配置
// const editorInit = ref({
//   language: 'zh_CN',
//   height: 480,
//   branding: false,
//   resize: false,
//   promotion: false,
//   highlight_on_focus: false,
//   skin: 'oxide-dark',
//   content_css: 'dark',
//   // toolbar: [
//   //   'undo redo |  forecolor backcolor bold italic underline strikethrough link | blocks fontfamily fontsize | \
//   // 				alignleft aligncenter alignright alignjustify | outdent indent | numlist bullist | pagebreak | \
//   // 				image media table preview | code selectall',
//   // ],
//   plugins: 'advlist image link code table lists fullscreen preview wordcount',
// });
const getEditorConfig = (dark: boolean) => ({
  language: "zh_CN",
  height: 480,
  branding: false,
  resize: false,
  promotion: false,
  highlight_on_focus: false,
  templates: customTemplates,
  skin: dark ? "oxide-dark" : "oxide",
  content_css: dark ? "dark" : "default",
  plugins:
    "advlist image media link code table lists fullscreen preview wordcount template",
  toolbar:
    "undo redo forecolor backcolor bold italic link blocks fontfamily fontsize image template",
  images_upload_handler: function (blobInfo: any) {
    return new Promise(async (resolve, reject) => {
      try {
        const data = new FormData();
        data.append("file", blobInfo.blob(), blobInfo.filename());
        data.append("timestamp", Date.now().toString());
        const res: any = await uploadServiceFile("article", data);
        resolve(baseUrl + res?.path);
      } catch (err) {
        const errorMsg = `上传出错：${(err as Error).message}`;
        reject(new Error(errorMsg));
      }
    });
  },
  paste_data_images: true,
});

const editorInit = ref(getEditorConfig(isDarkMode()));
const themeKey = ref(0);

onMounted(() => {});
</script>

<style scoped>
.tinymce-editor {
  width: 100%;
}
</style>
<style>
.tox-tinymce-aux {
  z-index: 10000 !important;
}
</style>
