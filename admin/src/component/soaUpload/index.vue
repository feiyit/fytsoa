<template>
  <!-- 上传容器：支持父组件传入大小，默认200x200 -->
  <div>
    <div
      class="soa-upload-container relative"
      :style="{ width: `${defaultWidth}px`, height: `${defaultHeight}px` }"
    >
      <!-- 上传中状态：进度条 + 预览图 -->
      <div
        v-if="file && file.status === 'uploading'"
        class="soa-upload-loading absolute inset-0 z-10"
      >
        <!-- 进度条覆盖层 -->
        <div
          class="z-1000 absolute flex h-full w-full items-center justify-center bg-black/30"
        >
          <el-progress
            :percentage="file.percentage"
            :text-inside="true"
            :stroke-width="14"
            class="w-3/4"
          />
        </div>
        <!-- 上传中预览图 -->
        <el-image
          :src="file.tempUrl"
          class="h-full w-full object-cover"
          fit="cover"
        />
      </div>

      <!-- 上传成功状态：图片 + 操作按钮 -->
      <div
        v-else-if="file && file.status === 'success'"
        class="soa-upload-success relative h-full w-full"
      >
        <el-image
          :src="file.url"
          :preview-src-list="[file.url]"
          class="h-full w-full cursor-pointer object-cover"
          fit="cover"
          hide-on-click-modal
          append-to-body
          show-progress
          :z-index="9999"
        >
          <template #placeholder>
            <div class="flex h-full w-full items-center justify-center">
              加载中...
            </div>
          </template>
        </el-image>

        <!-- 鼠标滑入显示操作栏 -->
        <div class="soa-upload-actions absolute">
          <el-button type="text" icon="Delete" @click.stop="handleRemove">
          </el-button>
        </div>
      </div>

      <!-- 未上传/已删除状态：上传按钮 -->
      <el-upload
        v-else
        class="soa-upload-btn h-full w-full"
        ref="uploadRef"
        :auto-upload="true"
        :show-file-list="false"
        :accept="accept"
        :limit="1"
        :http-request="handleHttpRequest"
        :before-upload="handleBeforeUpload"
        :on-exceed="handleExceed"
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
    <div class="pt-2 pl-0">
      <el-popover width="350px">
        <template #reference>
          <el-button type="primary" size="small"
            >文本框输入地址</el-button
          ></template
        >
        <p class="mb-2">请输入图片地址</p>
        <el-input
          v-model="innerValue"
          style="width: 320px"
          placeholder="地址"
          clearable
        />
      </el-popover>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, watch, onMounted, nextTick } from "vue";
import {
  ElUpload,
  ElImage,
  ElProgress,
  ElButton,
  ElIcon,
  ElMessage,
} from "element-plus";
import { UploadFilled } from "@element-plus/icons-vue";
import { type UploadRequestOptions, type UploadFile } from "element-plus";
import { uploadServiceFile } from "@/api/cms/file";
import { resolveUrl } from "@/utils/tools";
const apiURL = (import.meta as any).env.BASE_URL as string;
const baseUrl: string = apiURL.replace("/api", "");

// 定义组件 Props
const props = defineProps<{
  /** 组件宽度，默认200px */
  width?: number;
  /** 组件高度，默认200px */
  height?: number;
  /** 支持的文件类型，默认图片格式 */
  accept?: string;
  /** v-model 绑定的图片地址 */
  modelValue?: string;
  /** 服务端保存地址 */
  directory?: string;
}>();

// 定义组件 Emits
const emit = defineEmits<{
  /** v-model 更新事件 */
  "update:modelValue": [value: string];
  /** 删除文件事件 */
  delete: [value: void];
  /** 上传成功事件 */
  success: [url: string];
  /** 上传失败事件 */
  error: [msg: string];
}>();

// 内部状态管理
const uploadRef = ref<InstanceType<typeof ElUpload> | null>(null);
const file = ref<
  (UploadFile & { tempUrl?: string; percentage?: number }) | null
>(null);
const innerValue = ref<string>("");

// 默认值处理
const defaultWidth: number = props.width ?? 200;
const defaultHeight: number = props.height ?? 200;
const defaultUpPath = props.directory || "images";
const accept = props.accept || "image/gif, image/jpeg, image/png, image/webp";

// 初始化 v-model 绑定
onMounted(() => {
  innerValue.value = props.modelValue || "";
  // 如果初始有值，创建成功状态的文件对象
  if (innerValue.value) {
    file.value = {
      status: "success",
      url: resolveUrl(innerValue.value),
      uid: Date.now().toString(),
      name: innerValue.value.split("/").pop() || "file",
      size: 0,
    };
  }
});

// 监听 v-model 外部变化
watch(
  () => props.modelValue,
  (newVal) => {
    innerValue.value = newVal || "";
    if (newVal) {
      file.value = {
        status: "success",
        url: resolveUrl(newVal),
        uid: Date.now().toString(),
        name: newVal.split("/").pop() || "file",
        size: 0,
      };
    } else {
      file.value = null;
    }
  },
  { immediate: true }
);

// 监听内部值变化，同步到 v-model
watch(
  () => innerValue.value,
  (newVal) => {
    if (isCompleteImageUrl(newVal)) {
      emit("update:modelValue", newVal);
      file.value = {
        status: "success",
        url: resolveUrl(newVal),
        uid: Date.now().toString(),
        name: newVal.split("/").pop() || "file",
        size: 0,
      };
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

/**
 * 上传前校验
 * @param rawFile 原始文件
 */
const handleBeforeUpload = (rawFile: File) => {
  // 校验文件类型
  const isAccept = accept
    .split(",")
    .some((type) => rawFile.type === type.trim());
  if (!isAccept) {
    ElMessage.warning(`仅支持上传 ${accept} 格式的文件`);
    return false;
  }

  // 生成临时预览地址
  file.value = {
    status: "uploading",
    raw: rawFile,
    uid: Date.now().toString(),
    name: rawFile.name,
    size: rawFile.size,
    tempUrl: URL.createObjectURL(rawFile),
    percentage: 0,
  };

  return true;
};

/**
 * 自定义上传请求（核心）
 * @param options 上传配置
 */
const handleHttpRequest = async (options: UploadRequestOptions) => {
  if (!file.value?.raw) {
    ElMessage.error("上传文件不存在");
    options.onError(new Error("文件不存在") as any);
    return;
  }

  try {
    // 构造 FormData（根据后端要求调整字段名）
    const formData = new FormData();
    formData.append("file", file.value.raw);

    // 调用后端上传接口，监听进度
    const res: any = await uploadServiceFile(defaultUpPath, formData, {
      onUploadProgress: (progressEvent: any) => {
        const percent = Math.round(
          (progressEvent.loaded / (progressEvent.total || 1)) * 100
        );
        if (file.value) {
          file.value.percentage = percent;
        }
        options.onProgress({ percent } as any);
      },
    });
    const simageUrl = baseUrl + res.path;
    if (res.path) {
      file.value = {
        status: "success",
        url: simageUrl,
        uid: file.value.uid,
        name: file.value.name,
        size: file.value.size,
      };
      innerValue.value = res.path;
      options.onSuccess(baseUrl + res.path);
      emit("success", res.path);
      ElMessage.success("上传成功");
    } else {
      throw new Error("上传失败：未返回有效文件地址");
    }
  } catch (err) {
    // 上传失败：清理状态
    const errorMsg = err instanceof Error ? err.message : "上传失败，请重试";
    file.value = null;
    innerValue.value = "";
    options.onError(err as any);
    emit("error", errorMsg);
    ElMessage.error(errorMsg);
  } finally {
    // 释放临时 URL 内存
    if (file.value?.tempUrl) {
      URL.revokeObjectURL(file.value.tempUrl);
    }
  }
};

/**
 * 处理文件超出限制
 */
const handleExceed = () => {
  ElMessage.warning("最多只能上传一个文件");
};

/**
 * 移除已上传文件
 */
const handleRemove = () => {
  if (file.value?.url) {
    innerValue.value = "";
    file.value = null;
    nextTick(() => {
      uploadRef.value?.clearFiles();
    });
    emit("delete");
    ElMessage.success("文件已删除");
  }
};

// 暴露组件内部方法（可选）
defineExpose({
  handleRemove,
  clearFiles: () => {
    innerValue.value = "";
    file.value = null;
    uploadRef.value?.clearFiles();
  },
});
</script>

<style scoped>
/* 自定义样式均以 soa_ 开头 */
.soa-upload-container {
  @apply overflow-hidden rounded-md;
}

.soa-upload-loading {
  @apply flex items-center justify-center;
}

.soa-upload-success {
  @apply overflow-hidden rounded-md;
}

.soa-upload-actions {
  @apply gap-4;
  background: #f56c6d;
  right: 0px;
  top: 0px;
  text-align: center;
  padding: 0px 6px;
  display: none;
}
.soa-upload-actions .el-button {
  color: #ffffff;
}
.soa-upload-success:hover .soa-upload-actions {
  display: block;
}

.soa-upload-btn {
  @apply rounded-md;
}

.soa-upload-icon {
  @apply transition-colors duration-300;
  font-size: 30px;
}

/* 覆盖 Element Plus 部分样式，确保与 Tailwind 兼容 */
:deep(.el-progress__text) {
  @apply font-medium text-white;
}

:deep(.el-progress__stroke) {
  @apply bg-primary;
}

:deep(.el-image-viewer__wrapper) {
  @apply z-50;
}
:deep(.el-upload--text) {
  width: 100%;
  height: 100%;
}
</style>
