<script setup lang="ts">
import { nextTick, reactive, ref } from "vue";

import { useSoaModal } from "@/component/soaModal/index.vue";
import soaTable from "@/component/soaTable/index.vue";
import { fetchAmAssetPage } from "@/api/am/asset";
import { fetchSysCodeList } from "@/api/sys/code";

type Row = Record<string, any>;

const emit = defineEmits<{
  (e: "select", payload: { rows: Row[]; multiple: boolean }): void;
}>();

// sys_codetype 中 AM_ASSET_CATEGORY 对应的 Id（你给定的常量）
const AM_ASSET_CATEGORY_TYPE_ID = "2013975685776936960";

const title = ref("选择资产");
const multiple = ref(false);

const categoryOptions = ref<{ label: string; value: string }[]>([]);
const query = reactive({
  tenantId: "0",
  key: "",
  categoryId: undefined as string | undefined,
  assetStatus: undefined as number | undefined,
});

const tableRef = ref<any>(null);

// 多选：跨页保留
const pickedMap = ref<Map<string, Row>>(new Map());
// 单选：当前选中行
const pickedSingle = ref<Row | null>(null);

const buildPickedRows = () => Array.from(pickedMap.value.values());

const loadCategoryOptions = async () => {
  const codes = await fetchSysCodeList({
    id: AM_ASSET_CATEGORY_TYPE_ID,
    status: "1",
  });
  categoryOptions.value = (codes || []).map((m: any) => ({
    label: m.name,
    value: String(m.id),
  }));
};

const statusText = (v: number) => {
  switch (v) {
    case 1:
      return "在用";
    case 2:
      return "闲置";
    case 3:
      return "维修";
    case 4:
      return "报废";
    default:
      return v ? String(v) : "-";
  }
};

const applySelectionToTable = () => {
  if (!tableRef.value?.setSelectedRowKeys) return;
  if (multiple.value) {
    tableRef.value.setSelectedRowKeys(Array.from(pickedMap.value.keys()));
  } else {
    const id = pickedSingle.value ? String(pickedSingle.value.id) : "";
    tableRef.value.setSelectedRowKeys(id ? [id] : []);
  }
};

const onSelectionChange = (payload: { selectedRowKeys: any[]; selectedRows: Row[] }) => {
  const keys = (payload?.selectedRowKeys || []).map((x: any) => String(x));
  const rows = payload?.selectedRows || [];

  if (!multiple.value) {
    pickedSingle.value = rows?.[0] || null;
    return;
  }

  // 删除已取消的（selectedRowKeys 是当前“全量选中 key”的快照）
  for (const k of Array.from(pickedMap.value.keys())) {
    if (!keys.includes(k)) pickedMap.value.delete(k);
  }
  // 补齐当前页选中的行对象
  rows.forEach((r) => {
    const id = String(r?.id ?? "");
    if (id) pickedMap.value.set(id, r);
  });
};

const handleSearch = () => {
  tableRef.value?.upData?.(query, 1);
};

const handleReset = () => {
  Object.assign(query, {
    key: "",
    categoryId: undefined,
    assetStatus: undefined,
  });
  handleSearch();
};

const clearPicked = async () => {
  pickedMap.value = new Map();
  pickedSingle.value = null;
  await nextTick();
  tableRef.value?.setSelectedRowKeys?.([]);
};

const commit = () => {
  const rows = multiple.value
    ? buildPickedRows()
    : pickedSingle.value
      ? [pickedSingle.value]
      : [];
  emit("select", { rows, multiple: multiple.value });
  modalApi.close();
};

const [Modal, modalApi] = useSoaModal({
  onCancel: () => {
    // 关闭时不自动清空已选，便于重复打开时继续操作
  },
});

type OpenOptions = {
  multiple?: boolean;
  title?: string;
  // 预选（建议传 row 或 row 数组；内部以 id 去重）
  picked?: Row | Row[] | null;
};

const openModal = async (options: OpenOptions = {}) => {
  multiple.value = !!options.multiple;
  title.value = options.title || (multiple.value ? "选择资产（多选）" : "选择资产");

  // 重置筛选
  Object.assign(query, {
    tenantId: "0",
    key: "",
    categoryId: undefined,
    assetStatus: undefined,
  });

  // 初始化预选
  pickedMap.value = new Map();
  pickedSingle.value = null;
  const picked = options.picked;
  if (picked) {
    const arr = Array.isArray(picked) ? picked : [picked];
    if (multiple.value) {
      arr.forEach((r) => {
        const id = String((r as any)?.id ?? "");
        if (id) pickedMap.value.set(id, r as any);
      });
    } else {
      pickedSingle.value = arr[0] as any;
    }
  }

  await loadCategoryOptions();
  modalApi.open();
  await nextTick();
  // 触发表格重新加载
  handleSearch();
  applySelectionToTable();
};

defineExpose({ openModal });
</script>

<template>
  <Modal :title="title" class="w-[1000px]" :closeOnClickModal="false">
    <div class="mb-3 flex items-center gap-2">
      <el-input
        v-model="query.key"
        placeholder="编号/名称/标签"
        clearable
        maxlength="50"
        show-word-limit
        class="w-[320px]"
        @keyup.enter="handleSearch"
      />

      <el-select
        v-model="query.categoryId"
        placeholder="分类(全部)"
        clearable
        filterable
        class="w-[240px]"
      >
        <el-option
          v-for="it in categoryOptions"
          :key="it.value"
          :label="it.label"
          :value="it.value"
        />
      </el-select>

      <el-select
        v-model="query.assetStatus"
        placeholder="状态(全部)"
        clearable
        class="w-[160px]"
      >
        <el-option label="在用" :value="1" />
        <el-option label="闲置" :value="2" />
        <el-option label="维修" :value="3" />
        <el-option label="报废" :value="4" />
      </el-select>

      <el-button type="primary" @click="handleSearch">搜索</el-button>
      <el-button @click="handleReset">重置</el-button>
    </div>

    <soaTable
      ref="tableRef"
      :columns="[
        { title: '资产编号', dataIndex: 'assetNo', key: 'assetNo', width: 160 },
        { title: '资产名称', dataIndex: 'name', key: 'name', minWidth: 200 },
        { title: '分类', dataIndex: 'categoryId', key: 'categoryId', width: 160 },
        { title: '标签码', dataIndex: 'tagCode', key: 'tagCode', width: 150 },
        { title: '品牌', dataIndex: 'brand', key: 'brand', width: 120 },
        { title: '型号', dataIndex: 'model', key: 'model', width: 150 },
        { title: '状态', dataIndex: 'status', key: 'status', width: 110, align: 'center' },
        { title: '创建时间', dataIndex: 'createTime', key: 'createTime', width: 170 },
      ]"
      :apiObj="fetchAmAssetPage"
      :params="query"
      row-key="id"
      :singleSelect="!multiple"
      :toolbarShow="false"
      :pageSizes="[10, 20, 50, 100]"
      :initialPageInfo="{ page: 1, limit: 10 }"
      height="520px"
      @selection-change="onSelectionChange"
      @loaded="applySelectionToTable"
    >
      <template #bodyCell="{ column, record }">
        <template v-if="column.key === 'categoryId'">
          {{ record.categoryObj?.name || record.categoryId || "-" }}
        </template>
        <template v-if="column.key === 'status'">
          <el-tag :type="record.status === 1 ? 'success' : 'info'">
            {{ statusText(record.status) }}
          </el-tag>
        </template>
      </template>
    </soaTable>

    <template #footer>
      <div class="flex justify-end gap-2">
        <div class="mr-auto text-sm text-gray-500">
          <template v-if="multiple">已选：{{ buildPickedRows().length }} 条</template>
          <template v-else>已选：{{ pickedSingle ? "1" : "0" }} 条</template>
        </div>
        <el-button @click="clearPicked">清空已选</el-button>
        <el-button @click="modalApi.close()">取消</el-button>
        <el-button
          type="primary"
          :disabled="multiple ? buildPickedRows().length === 0 : !pickedSingle"
          @click="commit"
          >确定</el-button
        >
      </div>
    </template>
  </Modal>
</template>

