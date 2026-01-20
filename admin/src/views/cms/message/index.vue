<template>
  <el-container class="h-full">
    <!-- 顶部筛选与批量操作 -->
    <el-header
      class="bg-card border-border flex flex-wrap items-center justify-between gap-3 rounded-[.5vw]"
    >
      <el-form
        :inline="true"
        :model="query"
        class="mb-0 flex flex-wrap items-center gap-3"
      >
        <el-form-item label="关键词">
          <el-input
            v-model="query.key"
            placeholder="标题/内容关键词"
            clearable
            style="width: 220px"
          />
        </el-form-item>
        <el-form-item label="是否已读">
          <el-select
            v-model="query.status"
            placeholder="全部"
            style="width: 140px"
            clearable
          >
            <el-option label="未读" value="2" />
            <el-option label="已读" value="1" />
          </el-select>
        </el-form-item>

        <!-- <el-form-item label="时间范围">
          <el-date-picker
            v-model="query.range"
            type="datetimerange"
            start-placeholder="开始时间"
            end-placeholder="结束时间"
            :shortcuts="rangeShortcuts"
            value-format="YYYY-MM-DD HH:mm:ss"
            style="width: 300px"
            clearable
          />
        </el-form-item> -->
        <el-form-item>
          <el-button type="primary" @click="handleSearch">查询</el-button>
          <el-button @click="handleReset">重置</el-button>
        </el-form-item>
      </el-form>
      <div class="flex items-center gap-2">
        <el-button
          type="success"
          :disabled="selectedRows.length === 0"
          @click="handleBatchRead"
        >
          <el-icon class="mr-1"><View /></el-icon>
          标记已读
        </el-button>
        <el-popconfirm title="确认删除所选消息？" @confirm="handleBatchDelete">
          <template #reference>
            <el-button type="danger" :disabled="selectedRows.length === 0">
              <el-icon class="mr-1"><Delete /></el-icon>
              批量删除
            </el-button>
          </template>
        </el-popconfirm>
      </div>
    </el-header>

    <!-- 列表主体（soaTable） -->
    <el-main class="bg-card border-border mt-2 rounded-[.5vw]">
      <soaTable
        ref="tableRef"
        :columns="columns"
        :apiObj="fetchCmsMessagePage"
        :params="{ id: columnId }"
        row-key="id"
        @selection-change="selectionChange"
      >
        <template #bodyCell="{ column, record }">
          <!-- 未读点 -->
          <template v-if="column.key === 'dot'">
            <span
              :class="[
                'inline-block h-2.5 w-2.5 rounded-full',
                isRead(record) ? 'bg-muted/70' : 'bg-primary',
              ]"
            ></span>
          </template>
          <!-- 内容与摘要 -->
          <template v-else-if="column.key === 'title'">
            <div class="flex items-start gap-3">
              <div class="min-w-0 flex-1">
                <div
                  :class="[
                    'truncate',
                    isRead(record)
                      ? 'text-foreground'
                      : 'text-foreground font-semibold',
                  ]"
                >
                  {{ record.title || "（无标题）" }}
                </div>
                <div
                  v-if="record.content"
                  class="text-muted-foreground mt-0.5 line-clamp-2 text-xs"
                >
                  {{ record.summary }}
                </div>
              </div>
            </div>
          </template>
          <!-- 时间 -->
          <template v-else-if="column.key === 'createTime'">
            {{ record.createTime || record.time || record.createdAt || "-" }}
          </template>
          <!-- 状态 -->
          <template v-else-if="column.key === 'status'">
            <el-tag :type="isRead(record) ? 'success' : 'danger'" size="small">
              {{ isRead(record) ? "已读" : "未读" }}
            </el-tag>
          </template>
          <!-- 操作 -->
          <template v-else-if="column.key === 'action'">
            <div class="flex items-center gap-1">
              <el-button
                link
                type="primary"
                size="small"
                @click.stop="openDetail(record)"
                >查看</el-button
              >
              <el-popconfirm
                title="确认删除该条消息？"
                @confirm="handleDelete(record)"
              >
                <template #reference>
                  <el-button link type="danger" size="small">删除</el-button>
                </template>
              </el-popconfirm>
            </div>
          </template>
        </template>
      </soaTable>
    </el-main>

    <!-- 详情抽屉 -->
    <el-drawer
      v-model="detailVisible"
      direction="rtl"
      size="520px"
      :title="current?.title || current?.subject || '消息详情'"
    >
      <div class="text-muted-foreground space-y-1 text-xs">
        <div>时间：{{ current?.createTime || "-" }}</div>
        <div>
          状态：<span
            :class="current?.read ? 'text-foreground' : 'text-primary'"
            >{{ current?.read ? "已读" : "未读" }}</span
          >
        </div>
      </div>
      <div
        class="border-border bg-card text-foreground mt-3 whitespace-pre-wrap rounded border p-3 text-sm leading-relaxed"
      >
        {{ current?.summary || "(无内容)" }}
      </div>
      <template #footer>
        <div class="flex w-full items-center justify-end gap-2">
          <el-button @click="detailVisible = false">关闭</el-button>
          <el-button
            type="primary"
            @click="markRead([current!.id])"
            :disabled="!current || current.read"
            >标记已读</el-button
          >
          <el-popconfirm
            title="确认删除该条消息？"
            @confirm="handleDelete(current!)"
          >
            <template #reference>
              <el-button type="danger">删除</el-button>
            </template>
          </el-popconfirm>
        </div>
      </template>
    </el-drawer>
  </el-container>
}</template>

<script setup lang="ts">
import {
  fetchCmsMessagePage,
  updateCmsMessageRead,
  deleteCmsMessage,
} from "@/api/cms/message";
const props = defineProps({
  columnId: { type: String, default: "0" },
});
type Msg = {
  id: string | number;
  title?: string;
  subject?: string;
  content?: string;
  createTime?: string;
  summary?: string;
  read?: boolean;
};

const query = reactive({
  id: "0" as string,
  key: "" as string,
  status: undefined as boolean | undefined,
  range: [] as string[] | [],
});

const rangeShortcuts = [
  {
    text: "最近7天",
    value: () => {
      const end = new Date();
      const start = new Date();
      start.setDate(end.getDate() - 7);
      return [start, end];
    },
  },
  {
    text: "最近30天",
    value: () => {
      const end = new Date();
      const start = new Date();
      start.setDate(end.getDate() - 30);
      return [start, end];
    },
  },
];

const tableRef = ref<any>(null);
const selectedRows = ref<Msg[]>([]);
const current = ref<Msg | null>(null);
const detailVisible = ref(false);

const columns = [
  { title: "", key: "dot", width: 42, align: "center" },
  {
    title: "姓名",
    dataIndex: "userName",
    key: "userName",
    resizable: true,
    minWidth: 120,
  },
  {
    title: "标题",
    dataIndex: "title",
    key: "title",
    resizable: true,
    ellipsis: true,
    minWidth: 150,
  },
  {
    title: "内容",
    dataIndex: "summary",
    key: "summary",
    resizable: true,
    ellipsis: true,
    minWidth: 260,
  },
  {
    title: "邮箱",
    dataIndex: "email",
    key: "email",
    width: 180,
    align: "center",
  },
  {
    title: "手机号码",
    dataIndex: "mobile",
    key: "mobile",
    width: 180,
    align: "center",
  },
  {
    title: "时间",
    dataIndex: "createTime",
    key: "createTime",
    width: 180,
    align: "center",
  },
  {
    title: "状态",
    dataIndex: "status",
    key: "status",
    width: 100,
    align: "center",
  },
  { title: "操作", key: "action", width: 120, fixed: "right" },
];

function getQueryParams() {
  const p: any = {};
  if (query.key && query.key.trim()) p.key = query.key.trim();
  p.status = query.status;
  if (Array.isArray(query.range) && query.range.length === 2) {
    p.startTime = query.range[0];
    p.endTime = query.range[1];
  }
  return p;
}

function handleSearch() {
  tableRef?.value.upData(getQueryParams(), 1);
}

function handleReset() {
  query.key = "";
  query.status = undefined;
  query.range = [];
  handleSearch();
}

function selectionChange(params: any) {
  selectedRows.value = params.selectedRows as Msg[];
}

function openDetail(row: Msg) {
  current.value = row;
  detailVisible.value = true;
  if (!isRead(row)) markRead([row.id], false);
}

async function markRead(ids: Array<string | number>, showMsg = true) {
  try {
    await updateCmsMessageRead(ids);
    if (current.value && ids.includes(current.value.id))
      current.value.read = true as any;
    if (showMsg) ElMessage.success("已标记为已读");
    handleSearch();
  } catch (e) {
    ElMessage.error("标记失败");
  }
}

async function handleBatchRead() {
  const ids = selectedRows.value.map((m) => m.id);
  if (ids.length === 0) return;
  await markRead(ids);
}

async function handleDelete(row: Msg) {
  try {
    await deleteCmsMessage([row.id]);
    ElMessage.success("删除成功");
    handleSearch();
    current.value = null;
    detailVisible.value = false;
  } catch (e) {
    ElMessage.error("删除失败");
  }
}

async function handleBatchDelete() {
  const ids = selectedRows.value.map((m) => m.id);
  if (ids.length === 0) return;
  try {
    await deleteCmsMessage(ids);
    selectedRows.value = [];
    ElMessage.success("删除成功");
    handleSearch();
  } catch (e) {
    ElMessage.error("删除失败");
  }
}

function isRead(row: any) {
  return !!(row.read ?? row.isRead ?? row.status === 1);
}
watch(
  () => props.columnId,
  (newId) => {
    query.id = newId;
    setTimeout(() => {
      handleSearch();
    }, 50);
  }
);
</script>
<style scoped>
:deep(.el-header) {
  height: auto;
  padding: 10px;
}
</style>
