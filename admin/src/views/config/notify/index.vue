<template>
  <el-container class="h-full">
    <el-header
      class="bg-card border-border flex flex-wrap items-center justify-between gap-3 rounded-[.5vw]"
    >
      <div class="flex flex-wrap items-center gap-3">
        <el-tabs
          v-model="activeBox"
          class="min-w-[240px]"
        >
          <el-tab-pane name="inbox">
            <template #label>
              <span class="flex items-center gap-2">
                <span>收件箱</span>
                <el-badge
                  v-if="total.unread > 0"
                  :value="total.unread"
                  type="danger"
                />
              </span>
            </template>
          </el-tab-pane>
          <el-tab-pane name="sent" label="已发送" />
        </el-tabs>

        <el-input
          v-model="query.key"
          placeholder="标题/内容关键词"
          clearable
          style="width: 220px"
        />

        <el-select v-model="query.readStatus" style="width: 140px" clearable>
          <el-option label="全部" :value="0" />
          <el-option label="未读" :value="1" />
          <el-option label="已读" :value="2" />
        </el-select>

        <el-button type="primary" @click="handleSearch">查询</el-button>
        <el-button @click="handleReset">重置</el-button>
      </div>

      <div class="flex items-center gap-2">
        <el-popconfirm title="确认将全部通知标记为已读？" @confirm="handleMarkAllRead">
          <template #reference>
            <el-button :disabled="activeBox !== 'inbox'">全部已读</el-button>
          </template>
        </el-popconfirm>

        <el-button
          type="primary"
          :disabled="activeBox !== 'inbox' || selectedRows.length === 0"
          @click="handleMarkSelectedRead"
        >
          标记已读
        </el-button>

        <el-button
          :disabled="activeBox !== 'inbox' || selectedRows.length === 0"
          @click="handleMarkSelectedUnread"
        >
          标记未读
        </el-button>

        <el-popconfirm title="确认删除选中的通知？" @confirm="handleDeleteSelected">
          <template #reference>
            <el-button type="danger" :disabled="selectedRows.length === 0">
              删除
            </el-button>
          </template>
        </el-popconfirm>
      </div>
    </el-header>

    <el-main class="bg-card border-border mt-2 rounded-[.5vw]">
      <soaTable
        ref="tableRef"
        :columns="columns"
        :apiObj="fetchSysNoticePage"
        :params="query"
        row-key="id"
        row-serial-number
        @selection-change="selectionChange"
      >
        <template #bodyCell="{ column, record }">
          <template v-if="column.key === 'title'">
            <el-link type="primary" :underline="false" @click="openDetail(record)">
              <span class="truncate">{{ record.title || "-" }}</span>
            </el-link>
          </template>

          <template v-if="column.key === 'sendUser'">
            {{ record?.sendUser?.fullName || record?.sendUser?.loginAccount || "-" }}
          </template>

          <template v-if="column.key === 'isRead'">
            <el-tag
              :type="record.isRead ? 'success' : 'danger'"
              v-if="activeBox === 'inbox'"
            >
              {{ record.isRead ? "已读" : "未读" }}
            </el-tag>
            <span v-else>-</span>
          </template>

          <template v-if="column.key === 'createTime'">
            {{ formatDateTime(record.createTime) }}
          </template>

          <template v-if="column.key === 'action'">
            <div class="flex items-center gap-2">
              <el-button link type="primary" @click="openDetail(record)">查看</el-button>
              <el-button
                v-if="activeBox === 'inbox'"
                link
                type="primary"
                :disabled="record.isRead"
                @click="handleMarkRowRead(record)"
              >
                已读
              </el-button>
              <el-button
                v-if="activeBox === 'inbox'"
                link
                type="warning"
                :disabled="!record.isRead"
                @click="handleMarkRowUnread(record)"
              >
                未读
              </el-button>
              <el-popconfirm title="确认删除该通知？" @confirm="handleDeleteRow(record)">
                <template #reference>
                  <el-button link type="danger">删除</el-button>
                </template>
              </el-popconfirm>
            </div>
          </template>
        </template>
      </soaTable>
    </el-main>

    <Drawer class="w-[780px]" destroyOnClose :footer="false" :title="drawerTitle">
      <el-main style="padding: 0 20px">
        <el-descriptions :column="1" border size="small" label-width="110px">
          <el-descriptions-item label="标题">{{ detail.title || "-" }}</el-descriptions-item>
          <el-descriptions-item label="发送人">{{
            detail?.sendUser?.fullName || detail?.sendUser?.loginAccount || "-"
          }}</el-descriptions-item>
          <el-descriptions-item label="时间">{{ formatDateTime(detail.createTime) }}</el-descriptions-item>
          <el-descriptions-item label="状态" v-if="activeBox === 'inbox'">
            <el-tag :type="detail.isRead ? 'success' : 'danger'">
              {{ detail.isRead ? "已读" : "未读" }}
            </el-tag>
          </el-descriptions-item>
        </el-descriptions>

        <div class="mt-4">
          <div class="mb-2 text-sm text-slate-500">内容</div>
          <el-card shadow="never">
            <pre class="whitespace-pre-wrap text-sm">{{ detail.content || "-" }}</pre>
          </el-card>
        </div>
      </el-main>
    </Drawer>
  </el-container>
</template>

<script setup lang="ts">
import dayjs from "dayjs";
import { useSoaDrawer } from "@/component/soaDrawer/index.vue";
import {
  fetchSysNoticePage,
  fetchSysNoticeTotal,
  fetchSysNoticeById,
  setSysNoticeRead,
  clearSysNoticeRead,
  deleteSysNotice,
} from "@/api";

const tableRef = ref<any>(null);
const selectedRows = ref<any[]>([]);
const total = reactive({
  unread: 0,
  draft: 0,
  archive: 0,
  delete: 0,
});

type BoxType = "inbox" | "sent";
const activeBox = ref<BoxType>("inbox");

const query = reactive({
  page: 1,
  limit: 20,
  key: "",
  type: 2, // 2=收件箱, 1=已发送
  readStatus: 0, // 0=全部, 1=未读, 2=已读
});

const columns = [
  { title: "标题", dataIndex: "title", key: "title", minWidth: 240 },
  { title: "发送人", dataIndex: "sendUser", key: "sendUser", width: 140 },
  { title: "已读", dataIndex: "isRead", key: "isRead", width: 90, align: "center" },
  { title: "时间", dataIndex: "createTime", key: "createTime", width: 180 },
  { title: "操作", dataIndex: "action", key: "action", width: 220, fixed: "right" },
];

const selectionChange = (params: any) => {
  selectedRows.value = params.selectedRows || [];
};

const formatDateTime = (v?: string | null) => {
  if (!v) return "-";
  const d = dayjs(v);
  return d.isValid() ? d.format("YYYY-MM-DD HH:mm") : String(v);
};

const loadTotal = async () => {
  try {
    const res: any = await fetchSysNoticeTotal();
    Object.assign(total, res || {});
  } catch {
    Object.assign(total, { unread: 0, draft: 0, archive: 0, delete: 0 });
  }
};

const handleTabChange = () => {
  query.type = activeBox.value === "sent" ? 1 : 2;
  query.readStatus = 0;
  selectedRows.value = [];
  handleSearch();
  loadTotal();
};

const handleSearch = () => {
  tableRef.value?.upData(query, 1);
};

const handleReset = () => {
  Object.assign(query, {
    page: 1,
    limit: 20,
    key: "",
    type: activeBox.value === "sent" ? 1 : 2,
    readStatus: 0,
  });
  selectedRows.value = [];
  handleSearch();
};

const idsFromSelected = () =>
  (selectedRows.value || [])
    .map((x: any) => x?.id)
    .filter((x: any) => x != null && String(x) !== "");

const handleMarkAllRead = async () => {
  await setSysNoticeRead([]);
  await loadTotal();
  handleSearch();
};

const handleMarkSelectedRead = async () => {
  const ids = idsFromSelected();
  if (!ids.length) return;
  await setSysNoticeRead(ids);
  await loadTotal();
  handleSearch();
};

const handleMarkSelectedUnread = async () => {
  const ids = idsFromSelected();
  if (!ids.length) return;
  await clearSysNoticeRead(ids);
  await loadTotal();
  handleSearch();
};

const handleMarkRowRead = async (row: any) => {
  await setSysNoticeRead([row.id]);
  await loadTotal();
  handleSearch();
};

const handleMarkRowUnread = async (row: any) => {
  await clearSysNoticeRead([row.id]);
  await loadTotal();
  handleSearch();
};

const handleDeleteSelected = async () => {
  const ids = idsFromSelected();
  if (!ids.length) return;
  await deleteSysNotice(ids);
  await loadTotal();
  handleSearch();
};

const handleDeleteRow = async (row: any) => {
  if (!row?.id) return;
  await deleteSysNotice([row.id]);
  await loadTotal();
  handleSearch();
};

const detail = ref<any>({});
const drawerTitle = computed(() => {
  const t = detail.value?.title ? String(detail.value.title) : "";
  return t ? `通知详情：${t}` : "通知详情";
});

const [Drawer, drawerApi] = useSoaDrawer({
  onCancel: () => {
    drawerApi.close();
  },
});

const openDetail = async (row: any) => {
  if (!row?.id) return;
  const markRead = activeBox.value === "inbox" ? 1 : 0;
  detail.value = (await fetchSysNoticeById(row.id, markRead as 0 | 1)) || {};
  drawerApi.open();
  await loadTotal();
  handleSearch();
};

onMounted(() => {
  loadTotal();
});

watch(
  () => activeBox.value,
  () => {
    handleTabChange();
  },
);
</script>
