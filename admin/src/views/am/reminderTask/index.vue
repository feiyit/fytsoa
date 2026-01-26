<template>
  <el-container class="h-full">
    <el-header
      class="bg-card border-border flex flex-wrap items-center justify-between gap-3 rounded-[.5vw]"
      style="height: auto; padding: 10px 20px"
    >
      <el-form
        :inline="true"
        :model="query"
        class="mb-0 flex flex-wrap items-center gap-3"
      >
        <el-form-item label="关键词">
          <el-input v-model="query.key" placeholder="标题/内容" clearable />
        </el-form-item>
        <el-form-item label="状态">
          <el-select
            v-model="query.taskStatus"
            placeholder="全部"
            style="width: 140px"
            clearable
          >
            <el-option label="待发送" :value="0" />
            <el-option label="已发送" :value="1" />
            <el-option label="已读" :value="2" />
            <el-option label="已关闭" :value="3" />
          </el-select>
        </el-form-item>
        <el-form-item label="提醒规则">
          <el-select
            v-model="query.ruleId"
            placeholder="全部"
            clearable
            filterable
            style="width: 280px"
            @clear="query.ruleId = '0'"
          >
            <el-option
              v-for="it in ruleOptions"
              :key="String(it.id)"
              :label="`${ruleTypeLabel(it.ruleType)} / 提前${it.daysBefore ?? 0}天 (ID:${it.id})`"
              :value="String(it.id)"
            />
          </el-select>
        </el-form-item>
        <el-form-item>
          <el-button type="primary" @click="handleSearch">查询</el-button>
          <el-button @click="handleReset">重置</el-button>
        </el-form-item>
      </el-form>
      <div>
        <el-button type="primary" @click="openDialog()">
          <el-icon><Plus /></el-icon>
          新增任务
        </el-button>
      </div>
    </el-header>

    <el-main class="bg-card border-border mt-2 rounded-[.5vw]">
      <soaTable
        ref="tableRef"
        :columns="columns"
        :apiObj="fetchAmReminderTaskPage"
        :params="query"
        row-key="id"
        row-serial-number
        @loaded="onLoaded"
      >
        <template #bodyCell="{ column, record }">
          <template v-if="column.key === 'ruleId'">
            <span>{{ ruleLabel(record) }}</span>
          </template>
          <template v-if="column.key === 'receiverUserId'">
            <span>{{ receiverLabel(record) }}</span>
          </template>
          <template v-if="column.key === 'status'">
            <el-tag :type="tagType(record.status)">
              {{ statusText(record.status) }}
            </el-tag>
          </template>
          <template v-if="column.key === 'action'">
            <div class="flex items-center gap-1">
              <el-button link type="primary" @click="openDialog(record)"
                >编辑</el-button
              >
              <el-button
                link
                type="success"
                :disabled="record.status >= 2"
                @click="handleRead(record.id)"
                >已读</el-button
              >
              <el-button
                link
                type="warning"
                :disabled="record.status === 3"
                @click="handleClose(record.id)"
                >关闭</el-button
              >
              <el-popconfirm
                title="确认删除该任务？"
                @confirm="handleDelete(record.id)"
              >
                <template #reference>
                  <el-button link type="danger">删除</el-button>
                </template>
              </el-popconfirm>
            </div>
          </template>
        </template>
      </soaTable>
    </el-main>

    <modify ref="modifyRef" @complete="handleSearch" />
  </el-container>
</template>

<script setup lang="ts">
import {
  fetchAmReminderTaskPage,
  deleteAmReminderTask,
  readAmReminderTask,
  closeAmReminderTask,
} from "@/api/am/reminderTask";
import { fetchAmReminderRuleList } from "@/api/am/reminderRule";
import { fetchAdminById } from "@/api/sys/admin";
const modify = defineAsyncComponent(() => import("./modify.vue"));
const tableRef = ref<any>(null);
const modifyRef = ref<any>(null);

const query = reactive({
  page: 1,
  tenantId: "0",
  key: "",
  ruleId: undefined,
  taskStatus: undefined as number | undefined,
});

const ruleTypeOptions = [
  { value: "BORROW_DUE", label: "借用到期（BORROW_DUE）" },
  { value: "WARRANTY_EXPIRE", label: "质保到期（WARRANTY_EXPIRE）" },
  { value: "INVENTORY_DUE", label: "盘点到期（INVENTORY_DUE）" },
  { value: "MAINTENANCE_DUE", label: "保养到期（MAINTENANCE_DUE）" },
  { value: "TRANSFER_SIGN", label: "调拨签收（TRANSFER_SIGN）" },
];

const ruleTypeLabel = (v: any) => {
  const key = String(v ?? "");
  const hit = ruleTypeOptions.find((x) => x.value === key);
  return hit ? hit.label : key || "-";
};

const ruleOptions = ref<any[]>([]);
const ruleMap = reactive<Record<string, any>>({});

const loadRules = async () => {
  try {
    const res: any = await fetchAmReminderRuleList({});
    const items = Array.isArray(res) ? res : res?.items || [];
    ruleOptions.value = items;
    Object.keys(ruleMap).forEach((k) => delete ruleMap[k]);
    items.forEach((it: any) => {
      if (it?.id != null) ruleMap[String(it.id)] = it;
    });
  } catch {
    ruleOptions.value = [];
    Object.keys(ruleMap).forEach((k) => delete ruleMap[k]);
  }
};

const columns = [
  { title: "标题", dataIndex: "title", key: "title", minWidth: 240 },
  { title: "提醒规则", dataIndex: "ruleId", key: "ruleId", minWidth: 300 },
  {
    title: "接收人",
    dataIndex: "receiverUserId",
    key: "receiverUserId",
    width: 180,
  },
  { title: "业务类型", dataIndex: "bizType", key: "bizType", width: 140 },
  { title: "业务", dataIndex: "bizId", key: "bizId", width: 120 },
  { title: "到期时间", dataIndex: "dueTime", key: "dueTime", width: 170 },
  {
    title: "状态",
    dataIndex: "status",
    key: "status",
    width: 110,
    align: "center",
  },
  {
    title: "操作",
    dataIndex: "action",
    key: "action",
    width: 260,
    fixed: "right",
  },
];

const adminCache = reactive<Record<string, any>>({});
const adminLoading = ref<Set<string>>(new Set());

const ruleLabel = (record: any) => {
  const id = record?.ruleId != null ? String(record.ruleId) : "";
  const r = ruleMap[id];
  if (r) return `${ruleTypeLabel(r.ruleType)} / 提前${r.daysBefore ?? 0}天`;
  return id && id !== "0" ? `ID:${id}` : "-";
};

const getAdminLabel = (u: any) =>
  u?.fullName ||
  u?.displayName ||
  u?.userName ||
  u?.loginAccount ||
  u?.mobile ||
  u?.id;

const receiverLabel = (record: any) => {
  const id =
    record?.receiverUserId != null ? String(record.receiverUserId) : "";
  const u = adminCache[id];
  const name = getAdminLabel(u);
  if (name && id && id !== "0") return `${name}`;
  if (name) return `${name}`;
  return id && id !== "0" ? `ID:${id}` : "-";
};

const ensureAdmin = async (id: string) => {
  if (!id || id === "0") return;
  if (adminCache[id]) return;
  if (adminLoading.value.has(id)) return;
  adminLoading.value.add(id);
  try {
    adminCache[id] = await fetchAdminById(id);
  } catch {
    // ignore
  } finally {
    adminLoading.value.delete(id);
  }
};

const onLoaded = async (payload: { items: any[] }) => {
  if (!ruleOptions.value.length) await loadRules();
  const items = payload?.items || [];
  const ids = Array.from(
    new Set(
      items
        .filter((it) => it?.receiverUserId && String(it.receiverUserId) !== "0")
        .map((it) => String(it.receiverUserId)),
    ),
  );
  await Promise.all(ids.map((id) => ensureAdmin(id)));
};

const tagType = (v: number) => {
  switch (v) {
    case 0:
      return "info";
    case 1:
      return "success";
    case 2:
      return "warning";
    case 3:
      return "danger";
    default:
      return "info";
  }
};

const statusText = (v: number) => {
  switch (v) {
    case 0:
      return "待发送";
    case 1:
      return "已发送";
    case 2:
      return "已读";
    case 3:
      return "已关闭";
    default:
      return "未知";
  }
};

const handleSearch = () => {
  tableRef.value?.upData(query, 1);
};

const handleReset = () => {
  Object.assign(query, {
    page: 1,
    tenantId: "0",
    key: "",
    ruleId: "0",
    taskStatus: undefined,
  });
  handleSearch();
};

const openDialog = (row?: any) => {
  modifyRef.value?.openModal(row || {});
};

const handleRead = async (id: string) => {
  await readAmReminderTask(id);
  ElMessage.success("已标记为已读");
  handleSearch();
};

const handleClose = async (id: string) => {
  await closeAmReminderTask(id);
  ElMessage.success("已关闭");
  handleSearch();
};

const handleDelete = async (id: string) => {
  await deleteAmReminderTask([id]);
  ElMessage.success("删除成功");
  handleSearch();
};

onMounted(() => {
  loadRules();
});
</script>
