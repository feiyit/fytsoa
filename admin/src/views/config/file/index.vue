<template>
  <div>
    <el-splitter>
      <!-- 左侧：筛选 + 目录树 -->
      <el-splitter-panel size="20%" class="pr-1">
        <div class="bg-card border-border org-tree h-full rounded-[.5vw] p-2">
          <!-- 顶部筛选器 -->
          <div class="mb-2 flex items-center gap-2">
            <el-select v-model="filterKind" class="w-full">
              <el-option label="全部文件" value="all" />
              <el-option label="图片" value="image" />
              <el-option label="文件" value="doc" />
            </el-select>
          </div>
          <!-- 目录树 -->
          <el-scrollbar height="calc(100vh - 160px)">
            <el-tree
              ref="treeRef"
              node-key="id"
              :data="folderTree"
              :props="props"
              :highlight-current="true"
              :expand-on-click-node="false"
              default-expand-all
              @node-click="onFolderClick"
            >
              <template #default="{ node, data }">
                <span class="custom-tree-node">
                  <span class="label">
                    <el-icon
                      class="node-icon"
                      :class="{ 'is-leaf': node.isLeaf }"
                    >
                      <Folder v-if="node.isLeaf" />
                      <Folder v-else-if="!node.expanded" />
                      <FolderOpened v-else />
                    </el-icon>
                    {{ node.label }}</span
                  >
                  <span class="do" v-if="data.routes">
                    <el-icon @click.stop="removePath(node, data)"
                      ><Delete
                    /></el-icon>
                  </span>
                </span> </template
            ></el-tree>
          </el-scrollbar>
        </div>
      </el-splitter-panel>

      <!-- 右侧：工具栏 + 文件网格 -->
      <el-splitter-panel :min="300" class="pl-3">
        <div
          class="bg-card border-border flex h-full flex-col rounded-[.5vw] p-2"
        >
          <!-- 工具栏 -->
          <div class="flex flex-wrap items-center justify-between gap-3">
            <!-- 面包屑（当前目录） -->
            <el-breadcrumb separator-class="el-icon">
              <el-breadcrumb-item
                v-for="(seg, idx) in breadcrumbSegs"
                :key="idx"
                class="cursor-pointer"
                @click="navigateTo(idx)"
              >
                {{ seg.label }}
              </el-breadcrumb-item>
            </el-breadcrumb>

            <!-- 快捷按钮菜单 -->
            <div class="flex items-center gap-2">
              <el-button @click="copyCurrentPath">
                <el-icon class="mr-1"><DocumentCopy /></el-icon>复制路径
              </el-button>
              <el-button :disabled="!selectedFile" @click="previewSelected">
                <el-icon class="mr-1"><View /></el-icon>查看文件
              </el-button>
              <el-button
                type="danger"
                :disabled="!selectedFile"
                @click="deleteSelected"
              >
                <el-icon class="mr-1"><Delete /></el-icon>删除
              </el-button>
              <el-button :disabled="!selectedFile" @click="downloadSelected">
                <el-icon class="mr-1"><Download /></el-icon>下载
              </el-button>
              <el-upload
                :http-request="handleUpload"
                :show-file-list="false"
                :multiple="true"
              >
                <el-button type="primary">
                  <el-icon class="mr-1"><UploadFilled /></el-icon>本地上传
                </el-button>
              </el-upload>
            </div>
          </div>

          <!-- 文件网格（滚动容器，底部自动加载更多） -->
          <div ref="scrollRef" class="mt-2 grow overflow-auto">
            <div
              class="grid h-[calc(100vh-160px)] grid-cols-2 gap-2 p-1 md:grid-cols-3 lg:grid-cols-5 xl:grid-cols-7 2xl:grid-cols-8"
            >
              <div
                v-for="file in allFiles"
                :key="file.id"
                class="border-border bg-card/50 hover:border-primary/50 group relative h-[120px] cursor-pointer rounded-md border p-3 text-center"
                :class="
                  selectedFile?.id === file.id ? 'ring-primary ring-2' : ''
                "
                @click="selectFile(file)"
              >
                <div
                  v-if="guessKindByName(file.fileExt) != 'image'"
                  class="bg-muted text-muted-foreground m-auto mb-2 flex h-14 w-14 items-center justify-center rounded"
                >
                  <icon class="h-7 w-7" :icon="fileIconClass(file)" />
                </div>
                <el-image
                  v-if="guessKindByName(file.fileExt) == 'image'"
                  :src="resolveUrl(file.fileName)"
                  class="h-16"
                  fit="cover"
                />
                <div
                  class="text-foreground truncate text-xs"
                  :title="file.name"
                >
                  {{ file.name }}
                </div>
                <div
                  class="absolute right-2 top-2 hidden gap-1 group-hover:flex"
                >
                  <el-tooltip content="查看">
                    <el-button
                      size="small"
                      circle
                      @click.stop="previewFile(file)"
                    >
                      <el-icon><View /></el-icon>
                    </el-button>
                  </el-tooltip>
                  <el-tooltip content="删除">
                    <el-button
                      size="small"
                      circle
                      type="danger"
                      @click.stop="deleteFile(file)"
                    >
                      <el-icon><Delete /></el-icon>
                    </el-button>
                  </el-tooltip>
                </div>
              </div>
              <div v-if="allFiles.length == 0" class="text-center text-xs">
                无文件
              </div>
            </div>
          </div>
        </div>
      </el-splitter-panel>
    </el-splitter>

    <el-image-viewer
      v-if="showPreview"
      :url-list="srcList"
      show-progress
      @close="showPreview = false"
    />
  </div>
</template>

<script setup lang="ts">
import {
  uploadServiceFile,
  fetchFileTree,
  fetchFileList,
  deletePathFile,
  deleteDirectory,
} from "@/api/cms/file";
import { Icon } from "@iconify/vue";
import { resolveUrl } from "@/utils/tools";

type FileKind = "image" | "doc" | "video" | "audio" | "archive" | "other";

type FileItem = {
  id: string;
  fileName: string;
  fileSize: string;
  fileExt: FileKind;
  name: string;
  fullName: string;
  kind: string;
};

type TreeNode = {
  id: string; // 目录完整路径
  label: string;
  children?: TreeNode[];
};
const props = {
  label: "path",
};
// 左侧目录树（示例数据，可替换为接口）
const folderTree = ref<TreeNode[]>([]);

// 模拟文件源数据（可替换为接口返回）
const allFiles = ref<FileItem[]>([]);

const showPreview = ref(false);
const srcList = ref([]);

// 当前选择
const currentDir = ref<string>("/");
const filterKind = ref<"all" | "image" | "doc">("all");
const selectedFile = ref<FileItem | null>(null);

// 面包屑
const breadcrumbSegs = computed(() => {
  const parts = currentDir.value === "/" ? [""] : currentDir.value.split("/");
  const segs: { label: string; full: string }[] = [];
  let acc = "";
  for (const p of parts) {
    if (!p) {
      segs.push({ label: "根目录", full: "/" });
      acc = "/";
    } else {
      acc = acc.endsWith("/") ? `${acc}${p}` : `${acc}/${p}`;
      segs.push({ label: p, full: acc });
    }
  }
  return segs;
});

function navigateTo(index: number) {
  const target = breadcrumbSegs.value[index];
  if (target) {
    currentDir.value = target.full;
    resetAndLoad();
  }
}

function onFolderClick(node: TreeNode) {
  currentDir.value = node?.routes == undefined ? "" : "/upload" + node.routes;
  resetAndLoad();
}

function resetAndLoad() {
  selectedFile.value = null;
  seedFiles();
}

// 选择与操作
function selectFile(file: FileItem) {
  if (selectedFile.value != null && selectedFile.value.id == file.id) {
    selectedFile.value = null;
    return;
  }
  selectedFile.value = file;
}

function previewFile(file: FileItem) {
  selectedFile.value = file;
  if (file.kind === "image") {
    srcList.value = [resolveUrl(file.fileName)];
    showPreview.value = true;
  } else {
    openInNewTab(file);
  }
}

function deleteFile(file: FileItem) {
  ElMessageBox.confirm(`确认删除文件：${file.name}？`, "删除确认", {
    type: "warning",
  })
    .then(async () => {
      allFiles.value = allFiles.value.filter((f) => f.id !== file.id);
      await deletePathFile(file.fileName);
      if (selectedFile.value?.id === file.id) selectedFile.value = null;
      ElMessage.success("删除成功");
    })
    .catch(() => void 0);
}

function downloadFile(file: FileItem) {
  const a = document.createElement("a");
  a.href = resolveUrl(file.fileName);
  a.download = file.name;
  a.target = "_blank";
  a.click();
}

// 顶部快捷按钮封装
function copyCurrentPath() {
  console.log("selectedFile", selectedFile.value);
  const text = selectedFile.value?.fileName; //currentDir.value;
  navigator.clipboard
    .writeText(text)
    .then(() => ElMessage.success("路径已复制"))
    .catch(() => ElMessage.warning("复制失败，请手动复制"));
}

function previewSelected() {
  if (!selectedFile.value) return;
  previewFile(selectedFile.value);
}

function deleteSelected() {
  if (!selectedFile.value) return;
  deleteFile(selectedFile.value);
}

function downloadSelected() {
  if (!selectedFile.value) return;
  downloadFile(selectedFile.value);
}

function removePath(node: TreeNode, data: any) {
  ElMessageBox.confirm(`确认删除目录：${data.routes}？`, "删除确认", {
    type: "warning",
  })
    .then(async () => {
      await deleteDirectory("/upload" + data.routes);
      ElMessage.success("删除成功");
      initTree();
    })
    .catch(() => void 0);
}

// 上传
async function handleUpload(option: any) {
  const formData = new FormData();
  formData.append("file", option.file as File);
  try {
    let pathTemp = currentDir.value.replace(/^\/+/, "");
    pathTemp = pathTemp.replace("upload/", "");
    await uploadServiceFile(pathTemp, formData, {
      onUploadProgress: option.onProgress,
    });
    ElMessage.success("上传成功");
    const f = option.file as File;
    resetAndLoad();
    option.onSuccess({}, f);
  } catch (e) {
    ElMessage.error("上传失败");
    option.onError?.(e);
  }
}

function openInNewTab(file?: FileItem | null) {
  const f = file || selectedFile.value;
  if (!f) return;
  window.open(resolveUrl(f.fileName), "_blank");
}

// 图标映射
function fileIconClass(file: FileItem) {
  switch (file.kind) {
    case "image":
      return "lucide:image-minus";
    case "doc":
      return "lucide:dock";
    case "video":
      return "lucide:file-video-camera";
    case "audio":
      return "lucide:audio-lines";
    case "archive":
      return "lucide:folder-archive";
    default:
      return "lucide:file-box";
  }
}

function guessKindByName(name: string): FileKind {
  const ext = name.split(".").pop()?.toLowerCase() || "";
  if (["png", "jpg", "jpeg", "gif", "webp", "svg"].includes(ext))
    return "image";
  if (
    ["pdf", "doc", "docx", "xls", "xlsx", "ppt", "pptx", "txt", "md"].includes(
      ext
    )
  )
    return "doc";
  if (["mp4", "mov", "mkv", "webm"].includes(ext)) return "video";
  if (["mp3", "wav", "aac", "flac", "ogg"].includes(ext)) return "audio";
  if (["zip", "rar", "7z", "tar", "gz"].includes(ext)) return "archive";
  return "other";
}

// 模拟数据生成
async function seedFiles() {
  allFiles.value = await fetchFileList({ path: currentDir.value });
  allFiles.value.forEach((m) => {
    m.kind = guessKindByName(m.fileExt);
  });
}

const initTree = async () => {
  const res = await fetchFileTree();
  folderTree.value = res;
};

onMounted(() => {
  initTree();
  resetAndLoad();
});
</script>

<style scoped>
:deep(.el-breadcrumb__item) span {
  color: hsl(var(--foreground));
}
:deep(.org-tree .el-tree-node__content) {
  height: 36px;
  border-radius: 8px;
}
.custom-tree-node {
  display: flex;
  flex: 1;
  align-items: center;
  justify-content: space-between;
  font-size: 14px;
  padding-right: 24px;
  height: 100%;
}
.custom-tree-node .code {
  font-size: 12px;
  color: #999;
}
.custom-tree-node .do {
  display: none;
}
.custom-tree-node .do i {
  margin-left: 5px;
  color: #999;
}
.custom-tree-node:hover .code {
  display: none;
}
.custom-tree-node:hover .do {
  display: inline-block;
}
.node-icon {
  margin-right: 3px;
  color: var(--el-color-warning);
  position: relative;
  top: 2px;
}
</style>
