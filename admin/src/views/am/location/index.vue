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
        <el-form-item label="关键词">
          <el-input v-model="query.key" placeholder="名称/编码" clearable />
        </el-form-item>
        <el-form-item label="状态">
          <el-select
            v-model="query.status"
            placeholder="全部"
            style="width: 120px"
            clearable
          >
            <el-option label="启用" value="1" />
            <el-option label="停用" value="2" />
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
          新增地点
        </el-button>
      </div>
    </el-header>

    <el-main class="bg-card border-border mt-2 rounded-[.5vw]">
      <soaTable
        ref="tableRef"
        :columns="columns"
        :apiObj="fetchAmLocationPage"
        row-key="id"
        row-serial-number
        @selection-change="selectionChange"
      >
        <template #bodyCell="{ column, record }">
          <template v-if="column.key === 'type'">
            {{ typeText(record.type) }}
          </template>
          <template v-if="column.key === 'status'">
            <el-tag :type="record.status ? 'success' : 'danger'">
              {{ record.status ? "启用" : "停用" }}
            </el-tag>
          </template>
          <template v-if="column.key === 'action'">
            <div class="flex items-center gap-1">
              <el-button link type="primary" @click="openDialog(record)"
                >编辑</el-button
              >
              <el-popconfirm
                title="确认删除该地点？"
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
import { fetchAmLocationPage, deleteAmLocation } from "@/api/am/location";
const modify = defineAsyncComponent(() => import("./modify.vue"));
const tableRef = ref<any>(null);
const modifyRef = ref<any>(null);
const selectedRows = ref<any[]>([]);

const query = reactive({
  page: 1,
  tenantId: "0",
  key: "",
  status: undefined as string | undefined,
});

// 与表单保持一致的显示（park/building/floor/room）
const typeText = (v: string) => {
  switch (v) {
    case "park":
      return "园区 (park)";
    case "building":
      return "楼栋 (building)";
    case "floor":
      return "楼层 (floor)";
    case "room":
      return "房间 (room)";
    default:
      return v || "-";
  }
};

const columns = [
  { title: "编码", dataIndex: "code", key: "code", width: 140 },
  { title: "名称", dataIndex: "name", key: "name", minWidth: 200 },
  { title: "类型", dataIndex: "type", key: "type", width: 140 },
  {
    title: "层级",
    dataIndex: "layer",
    key: "layer",
    width: 80,
    align: "center",
  },
  {
    title: "状态",
    dataIndex: "status",
    key: "status",
    width: 80,
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
    status: undefined,
  });
  handleSearch();
};

const openDialog = (row?: any) => {
  modifyRef.value?.openModal(row || {});
};

const handleDelete = async (id: string) => {
  await deleteAmLocation([id]);
  ElMessage.success("删除成功");
  handleSearch();
};

const handleBatchDelete = async () => {
  if (selectedRows.value.length === 0) return;
  await deleteAmLocation(selectedRows.value.map((x: any) => x.id));
  ElMessage.success("删除成功");
  handleSearch();
};
</script>
