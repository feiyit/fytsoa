<template>
  <div class="soa_upload-container">
    <!-- 已上传图片列表 -->
    <div class="mb-2 flex flex-wrap gap-4">
      <div
        v-for="(file, index) in fileList"
        :key="file.path || index"
        class="group relative overflow-hidden rounded-md shadow-sm"
        :style="{ width: `${width}px`, height: `${height}px` }"
      >
        <!-- 图片预览 -->
        <img
          :src="file.url"
          class="h-full w-full object-cover transition-transform duration-300 group-hover:scale-105"
          @click="handlePreview(index)"
          alt="上传图片"
          @error="handleImageError(index)"
        />

        <!-- 加载错误占位 -->
        <div
          v-if="file.error"
          class="absolute inset-0 flex items-center justify-center"
        >
          <el-icon class="text-gray-400" size="40"><Picture /></el-icon>
        </div>

        <!-- 上传中状态 -->
        <div
          v-if="file.uploading"
          class="absolute inset-0 flex items-center justify-center bg-black/30"
        >
          <div
            class="z-1000 absolute flex h-full w-full items-center justify-center bg-black/30"
          >
            <el-progress
              :percentage="file.percentage"
              :text-inside="true"
              :stroke-width="18"
              class="w-3/4"
            />
          </div>
          <!-- 上传中预览图 -->
          <el-image
            :src="file.path"
            class="h-full w-full object-cover"
            fit="cover"
          />
        </div>

        <!-- 删除按钮 -->
        <div
          class="absolute right-1 top-1 z-10 opacity-0 transition-opacity group-hover:opacity-100"
        >
          <el-icon
            size="22"
            class="cursor-pointer rounded-full bg-red-500 p-1 text-white shadow-md hover:bg-red-600"
            @click.stop="handleRemove(index)"
          >
            <Delete />
          </el-icon>
        </div>
      </div>

      <!-- 上传按钮占位符 -->
      <div
        v-if="fileList.length < maxCount"
        :style="{ width: `${width}px`, height: `${height}px` }"
        class=""
      >
        <el-upload
          class="h-full w-full"
          action=""
          :show-file-list="false"
          :on-change="handleFileChange"
          accept="image/*"
          multiple
          :auto-upload="false"
        >
          <div
            class="hover:border-primary flex h-full w-full flex-col items-center justify-center rounded-md border-2 border-dashed border-gray-300 transition-all"
          >
            <el-icon class="soa-upload-icon mb-2">
              <UploadFilled />
            </el-icon>
            <p class="text-sm text-gray-500">点击上传图片</p>
          </div>
        </el-upload>
      </div>
    </div>
    <div class="pl-0">
      <el-popover width="350px">
        <template #reference>
          <el-button type="primary" size="small"
            >文本框输入地址</el-button
          ></template
        >
        <p class="mb-2">请输入图片地址</p>
        <el-input
          v-model="inputUrl"
          clearable
          style="width: 320px"
          placeholder="地址"
        />
      </el-popover>
    </div>
    <!-- 图片预览弹窗 -->
    <el-image-viewer
      v-if="previewVisible"
      :url-list="previewSrcList"
      :initial-index="previewIndex"
      @close="previewVisible = false"
    />
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onUnmounted, watch } from "vue";
import { uploadServiceFile } from "@/api/cms/file";
import { Delete, Picture } from "@element-plus/icons-vue";
import { ElImageViewer } from "element-plus";
import { resolveUrl } from "@/utils/tools";

// 组件参数
const props = defineProps({
  // 图片宽度
  width: {
    type: Number,
    default: 120,
  },
  // 图片高度
  height: {
    type: Number,
    default: 120,
  },
  // 最大上传数量
  maxCount: {
    type: Number,
    default: 5,
  },
  // 最大文件大小(MB)
  maxSize: {
    type: Number,
    default: 5,
  },
  // 初始图片列表
  modelValue: {
    type: Array as () => Array<{
      url: string;
      path: string;
      error?: boolean;
      uploading?: boolean;
    }>,
    default: () => [],
  },
  directory: {
    type: String,
    default: "images",
    required: false,
  },
});

// 组件事件
const emit = defineEmits([
  "update:modelValue",
  "change",
  "upload-success",
  "upload-error",
]);

// 状态管理
const fileList = ref<
  Array<{
    url: string;
    path: string;
    error?: boolean;
    uploading?: boolean;
    percentage: number;
    file?: File; // 临时存储原始文件
  }>
>([
  ...(props.modelValue || []).map((path) => ({
    url: resolveUrl(path),
    path,
    percentage: 0,
  })),
]);

const previewVisible = ref(false);
const previewIndex = ref(0);
const previewSrcList = computed(() => fileList.value.map((item) => item.url));
const objectUrls = ref<string[]>([]); // 管理blob url用于清理
const inputUrl = ref<string>("");
// 监听初始值变化
watch(
  () => props.modelValue,
  (newVal) => {
    fileList.value = (newVal || []).map((path) => ({
      path,
      url: resolveUrl(path),
      percentage: 0,
    }));
  },
  { deep: true, immediate: true }
);

watch(
  () => inputUrl.value,
  (newVal) => {
    if (isCompleteImageUrl(newVal)) {
      fileList.value.push({
        url: resolveUrl(newVal),
        path: resolveUrl(newVal),
        uploading: false,
        percentage: 0,
        file: undefined,
      });
      emit(
        "update:modelValue",
        fileList.value.map((item) => item.path)
      );
    }
  }
);

/**
 * 判断输入内容是否为完整的图片地址（支持绝对URL和相对路径）
 * @param {string} url - 用户输入的地址
 * @param {boolean} [checkValidity=false] - 是否验证地址是否真的可访问（异步）
 * @returns {boolean|Promise<boolean>} 同步返回格式是否合法，异步返回是否有效
 */
function isCompleteImageUrl(url: string, checkValidity = false) {
  // 优化后的正则：支持 http/https 绝对URL 或 以/开头的相对路径
  // 支持的图片后缀：jpg/jpeg/png/gif/webp/svg/bmp
  const imageUrlRegex =
    /^(https?:\/\/.+?|\/.+?)\.(jpg|jpeg|png|gif|webp|svg|bmp)$/i;

  // 去除首尾空格后验证格式
  const trimmedUrl = url.trim();
  const isFormatValid = imageUrlRegex.test(trimmedUrl);
  if (!isFormatValid || !checkValidity) {
    return isFormatValid;
  }

  // 异步验证地址有效性（相对路径会基于当前域名拼接）
  return new Promise((resolve) => {
    const img = new Image();
    img.crossOrigin = "anonymous";
    img.onload = () => resolve(true);
    img.onerror = () => resolve(false);
    setTimeout(() => resolve(false), 5000); // 5秒超时
    // 处理相对路径：拼接当前页面的域名
    img.src = trimmedUrl.startsWith("http")
      ? trimmedUrl
      : window.location.origin + trimmedUrl;
  });
}

// 处理文件选择
const handleFileChange = async (uploadFile: any) => {
  if (fileList.value.length >= props.maxCount) {
    ElMessage.warning(`最多只能上传${props.maxCount}张图片`);
    return;
  }

  const file = uploadFile.raw;
  if (!file) return;

  // 验证文件类型
  if (!file.type.startsWith("image/")) {
    ElMessage.error("请选择图片文件");
    return;
  }

  // 验证文件大小
  if (file.size > props.maxSize * 1024 * 1024) {
    ElMessage.error(`图片大小不能超过${props.maxSize}MB`);
    return;
  }

  try {
    // 显示上传中状态
    const previewUrl = URL.createObjectURL(file);
    objectUrls.value.push(previewUrl);

    // 添加临时文件到列表（显示上传中）
    const tempIndex = fileList.value.length;
    fileList.value.push({
      url: previewUrl,
      path: "",
      uploading: true,
      percentage: 0,
      file,
    });

    // 执行上传
    await uploadFileToServer(tempIndex, uploadFile);
  } catch (error) {
    ElMessage.error("处理图片失败，请重试");
    console.error("文件处理错误:", error);
  }
};

// 上传到服务器
const uploadFileToServer = async (index: number, _uploadFile: any) => {
  const fileItem = fileList.value[index];
  if (!fileItem || !fileItem.file) return;

  try {
    // 构建表单数据
    const formData = new FormData();
    formData.append("file", fileItem.file);

    // 调用上传接口
    const res = await uploadServiceFile(props.directory, formData, {
      onUploadProgress: (progressEvent: any) => {
        const percent = Math.round(
          (progressEvent.loaded / (progressEvent.total || 1)) * 100
        );
        fileList.value[index].percentage = percent;
        //uploadFile.onProgress({ percent });
      },
    });

    if (res?.path) {
      // 更新文件信息（清除上传中状态）
      fileList.value[index] = {
        ...fileItem,
        path: res.path,
        uploading: false,
        file: undefined,
      };
      // 触发事件
      emit(
        "update:modelValue",
        fileList.value.map((item) => item.path)
      );
      emit("change", [...fileList.value]);
      emit("upload-success", res.path, index);
      ElMessage.success("上传成功");
    } else {
      throw new Error("未获取到图片路径");
    }
  } catch (error) {
    // 上传失败处理
    fileList.value[index] = {
      ...fileItem,
      uploading: false,
      error: true,
      file: undefined,
    };

    emit("upload-error", error, index);
    ElMessage.error("上传失败，请重试");
    console.error("上传错误:", error);
  }
};

// 预览图片
const handlePreview = (index: number) => {
  previewIndex.value = index;
  previewVisible.value = true;
};

// 删除图片
const handleRemove = (index: number) => {
  const removedFile = fileList.value[index];
  // 清理blob url
  if (removedFile.url.startsWith("blob:")) {
    const urlIndex = objectUrls.value.indexOf(removedFile.url);
    if (urlIndex > -1) {
      URL.revokeObjectURL(removedFile.url);
      objectUrls.value.splice(urlIndex, 1);
    }
  }

  // 移除图片
  const newList = [...fileList.value];
  newList.splice(index, 1);
  fileList.value = newList;

  // 触发事件
  emit(
    "update:modelValue",
    fileList.value.map((item) => item.path)
  );
  emit("change", [...fileList.value]);
  ElMessage.success("文件已删除");
};

// 图片加载错误处理
const handleImageError = (index: number) => {
  fileList.value = fileList.value.map((item, i) =>
    i === index ? { ...item, error: true } : item
  );
};

// 清理资源
onUnmounted(() => {
  // 清理所有blob url，防止内存泄漏
  objectUrls.value.forEach((url) => {
    URL.revokeObjectURL(url);
  });
  objectUrls.value = [];
});
</script>

<style scoped>
.soa_upload-container {
  @apply w-full;
}

:deep(.el-upload) {
  @apply cursor-pointer;
}

:deep(.el-image-viewer__wrapper) {
  @apply z-50;
}

:deep(.el-image-viewer__btn) {
  @apply bg-black/50 transition-colors hover:bg-black/70;
}

:deep(.el-image__error) {
  @apply flex items-center justify-center;
}
.soa-upload-icon {
  @apply transition-colors duration-300;
  font-size: 30px;
}
:deep(.el-upload--text) {
  width: 100%;
  height: 100%;
}
</style>
