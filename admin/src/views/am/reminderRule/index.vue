<template>
  <el-container class="h-full">
    <el-header
      class="bg-card border-border flex flex-wrap items-center justify-between gap-3 rounded-[.5vw]"
    >
      <el-form
        :inline="true"
        :model="query"
        class="mb-0 flex flex-wrap items-center gap-3"
      >
        <el-form-item label="规则类型">
          <el-select
            v-model="query.ruleType"
            placeholder="全部"
            clearable
            filterable
            allow-create
            default-first-option
            style="width: 220px"
          >
            <el-option
              v-for="it in ruleTypeOptions"
              :key="it.value"
              :label="it.label"
              :value="it.value"
            />
          </el-select>
        </el-form-item>
        <el-form-item label="启用">
          <el-select
            v-model="query.enabled"
            placeholder="全部"
            style="width: 120px"
            clearable
          >
            <el-option label="启用" :value="1" />
            <el-option label="停用" :value="2" />
          </el-select>
        </el-form-item>
        <el-form-item>
          <el-button type="primary" @click="handleSearch">查询</el-button>
          <el-button @click="handleReset">重置</el-button>
        </el-form-item>
      </el-form>
      <div>
        <el-popconfirm
          title="确认执行批量删除操作？"
          @confirm="handleBatchDelete"
        >
          <template #reference>
            <el-button type="danger" v-if="selectedRows.length > 0">
              <el-icon><Delete /></el-icon>
              批量删除
            </el-button>
          </template>
        </el-popconfirm>
        <el-button type="primary" @click="openDialog()">
          <el-icon><Plus /></el-icon>
          新增规则
        </el-button>
      </div>
    </el-header>

    <el-main class="bg-card border-border mt-2 rounded-[.5vw]">
      <soaTable
        ref="tableRef"
        :columns="columns"
        :apiObj="fetchAmReminderRulePage"
        row-key="id"
        row-serial-number
        @selection-change="selectionChange"
      >
        <template #bodyCell="{ column, record }">
          <template v-if="column.key === 'ruleType'">
            {{ ruleTypeLabel(record.ruleType) }}
          </template>
          <template v-if="column.key === 'isEnabled'">
            <el-tag :type="record.isEnabled ? 'success' : 'danger'">
              {{ record.isEnabled ? "启用" : "停用" }}
            </el-tag>
          </template>
          <template v-if="column.key === 'action'">
            <div class="flex items-center gap-1">
              <el-button link type="primary" @click="openDialog(record)"
                >编辑</el-button
              >
              <el-popconfirm
                title="确认删除该规则？"
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
  fetchAmReminderRulePage,
  deleteAmReminderRule,
} from "@/api/am/reminderRule";
const modify = defineAsyncComponent(() => import("./modify.vue"));
const tableRef = ref<any>(null);
const modifyRef = ref<any>(null);
const selectedRows = ref<any[]>([]);

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

const query = reactive({
  page: 1,
  tenantId: "0",
  key: "",
  ruleType: "",
  enabled: undefined as number | undefined,
});

const columns = [
  { title: "规则类型", dataIndex: "ruleType", key: "ruleType", minWidth: 220 },
  {
    title: "启用",
    dataIndex: "isEnabled",
    key: "isEnabled",
    width: 90,
    align: "center",
  },
  {
    title: "提前天数",
    dataIndex: "daysBefore",
    key: "daysBefore",
    width: 110,
    align: "center",
  },
  { title: "创建时间", dataIndex: "createTime", key: "createTime", width: 170 },
  {
    title: "操作",
    dataIndex: "action",
    key: "action",
    width: 110,
    fixed: "right",
  },
];

const selectionChange = (params: any) => {
  selectedRows.value = params.selectedRows || [];
};

const handleSearch = () => {
  tableRef.value?.upData(query, 1);
};

const handleReset = () => {
  Object.assign(query, {
    page: 1,
    tenantId: "0",
    key: "",
    ruleType: "",
    enabled: undefined,
  });
  handleSearch();
};

const openDialog = (row?: any) => {
  modifyRef.value?.openModal(row || {});
};

const handleDelete = async (id: string) => {
  await deleteAmReminderRule([id]);
  ElMessage.success("删除成功");
  handleSearch();
};

const handleBatchDelete = async () => {
  if (selectedRows.value.length === 0) return;
  await deleteAmReminderRule(selectedRows.value.map((x: any) => x.id));
  ElMessage.success("删除成功");
  handleSearch();
};
</script>
