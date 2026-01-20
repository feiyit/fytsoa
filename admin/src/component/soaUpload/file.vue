<template>
  <div class="soa_upload-container">
    <el-upload
      v-model:file-list="internalFileList"
      :auto-upload="true"
      :on-success="handleSuccess"
      :on-error="handleError"
      :on-remove="handleRemove"
      :before-upload="beforeUpload"
      :limit="limit"
      :multiple="limit > 0"
      :on-exceed="handleExceed"
      :disabled="disabled"
      list-type="text"
      class="soa_upload-wrapper"
    >
      <el-button type="primary" :disabled="disabled">
        <el-icon class="mr-2"><Upload /></el-icon>
        选择文件
      </el-button>
      <template #tip>
        <div class="mt-1 text-xs text-gray-500">
          支持上传 {{ fileType.join("、") }} 格式文件，单个文件不超过
          {{ fileSize }}MB，最多上传 {{ limit }} 个文件
        </div>
      </template>
    </el-upload>
  </div>
</template>

<script setup lang="ts">
import { ref, watch, computed } from "vue";
import { Upload } from "@element-plus/icons-vue";
import { uploadServiceFile } from "@/api/cms/file";

const props = defineProps<{
  modelValue: string[];
  directory: string;
  fileType?: string[];
  fileSize?: number;
  limit?: number;
  disabled?: boolean;
}>();

const emit = defineEmits<{
  (e: "update:modelValue", value: string[]): void;
  (e: "error", message: string): void;
}>();

// 默认参数
const defaultFileType = [
  "pdf",
  "doc",
  "docx",
  "xls",
  "xlsx",
  "ppt",
  "pptx",
  "rar",
  "zip",
  "7z",
  "txt",
];
const defaultFileSize = 10; // MB
const defaultLimit = 5;

// 内部状态管理
const internalFileList = ref<any[]>([]);
const localPaths = ref<string[]>([...props.modelValue]);

// 计算属性
const fileType = computed(() => props.fileType || defaultFileType);
const fileSize = computed(() => props.fileSize || defaultFileSize);
const limit = computed(() => props.limit || defaultLimit);

const arrayEqual = (a: string[], b: string[]): boolean => {
  if (a.length !== b.length) return false;
  return a.every((item, index) => item === b[index]);
};
// 监听v-model变化
watch(
  () => props.modelValue,
  (newVal) => {
    if (!arrayEqual(newVal, localPaths.value)) {
      localPaths.value = [...newVal];
    }
  },
  { immediate: true }
);

watch(
  () => localPaths.value,
  (newVal) => {
    internalFileList.value = newVal.map((path) => ({
      name: path,
      url: path,
      status: "success",
    }));
    emit("update:modelValue", [...newVal]);
  }
);

// 监听本地路径变化，同步到v-model
watch(
  () => localPaths.value,
  (newVal) => {
    emit("update:modelValue", [...newVal]);
  }
);

// 上传前校验
const beforeUpload = (file: File) => {
  // 校验文件类型
  const ext = file.name.split(".").pop()?.toLowerCase();
  if (!ext || !fileType.value.includes(ext)) {
    ElMessage.error(`不支持的文件类型，仅支持${fileType.value.join("、")}`);
    return false;
  }

  // 校验文件大小
  if (file.size / 1024 / 1024 > fileSize.value) {
    ElMessage.error(`文件大小不能超过${fileSize.value}MB`);
    return false;
  }

  // 手动处理上传
  uploadFile(file);
  return false; // 阻止自动上传
};

// 实际上传处理
const uploadFile = async (file: File) => {
  try {
    const formData = new FormData();
    formData.append("file", file);

    const res = await uploadServiceFile(props.directory, formData);

    // 上传成功后更新列表
    localPaths.value = [...localPaths.value, res.path];
    internalFileList.value.push({
      name: file.name,
      url: res.path,
      status: "success",
    });
  } catch (error) {
    const message = error instanceof Error ? error.message : "文件上传失败";
    ElMessage.error(message);
    emit("error", message);
  }
};

// 上传成功回调
const handleSuccess = () => {
  // 由于使用手动上传，这里可能不会触发
};

// 上传失败回调
const handleError = (err: any) => {
  ElMessage.error("文件上传失败");
  emit("error", err?.message || "文件上传失败");
};

// 移除文件处理
const handleRemove = (file: any) => {
  const index = localPaths.value.findIndex((path) => path === file.url);
  if (index > -1) {
    localPaths.value.splice(index, 1);
  }
};

// 超出文件数量限制
const handleExceed = () => {
  ElMessage.error(`最多只能上传${limit.value}个文件`);
};
</script>

<style scoped>
.soa_upload-container {
  @apply w-full;
}

.soa_upload-wrapper {
  @apply w-full;
}

:deep(.el-upload-list__item) {
  @apply mt-2;
}

:deep(.el-upload-list__item-name) {
  @apply max-w-[calc(100%-80px)] truncate;
}
</style>
