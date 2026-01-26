<script setup lang="ts">
import { useSoaDrawer } from "@/component/soaDrawer/index.vue";
import { fetchAmDocById } from "@/api/am/doc";

const title = ref("单据详情");
const loading = ref(false);

const formData = ref<any>({});

const [Drawer, drawerApi] = useSoaDrawer({
  onCancel: () => drawerApi.close(),
});

const openModal = async (rowOrId: any) => {
  const id = typeof rowOrId === "object" ? rowOrId?.id : rowOrId;
  if (!id) return;
  title.value = `单据详情：${rowOrId?.docNo || ""}`.trim();
  loading.value = true;
  try {
    const res = await fetchAmDocById(String(id));
    formData.value = res || {};
    if (!Array.isArray(formData.value.items)) formData.value.items = [];
  } finally {
    loading.value = false;
  }
  drawerApi.open();
};

const statusText = (v: number) => {
  switch (v) {
    case 0:
      return "草稿";
    case 1:
      return "待审批";
    case 2:
      return "已通过";
    case 3:
      return "已驳回";
    case 4:
      return "执行中";
    case 5:
      return "已完成";
    case 6:
      return "已取消";
    default:
      return "未知";
  }
};

defineExpose({ openModal });
</script>

<template>
  <Drawer :title="title" size="65%" :closeOnClickModal="false" :footer="false">
    <div v-loading="loading">
      <el-descriptions :column="3" border class="mb-3">
        <el-descriptions-item label="单据号">{{
          formData.docNo
        }}</el-descriptions-item>
        <el-descriptions-item label="类型">{{
          formData.docType
        }}</el-descriptions-item>
        <el-descriptions-item label="子类型">{{
          formData.subType
        }}</el-descriptions-item>
        <el-descriptions-item label="状态">{{
          statusText(formData.status)
        }}</el-descriptions-item>
        <el-descriptions-item label="业务时间">{{
          formData.bizTime
        }}</el-descriptions-item>
        <el-descriptions-item label="到期时间">{{
          formData.dueTime
        }}</el-descriptions-item>
        <el-descriptions-item label="备注" :span="3">{{
          formData.remark
        }}</el-descriptions-item>
      </el-descriptions>

      <div class="mb-2 flex items-center justify-between">
        <div class="text-sm text-gray-500">
          明细：{{ (formData.items || []).length }} 条
        </div>
      </div>

      <el-table :data="formData.items || []" border height="520">
        <el-table-column type="index" width="50" />
        <el-table-column label="资产编号" prop="assetNo" width="160" />
        <el-table-column
          label="标签码"
          prop="tagCode"
          width="150"
          show-overflow-tooltip
        />
        <el-table-column
          label="名称"
          prop="name"
          min-width="200"
          show-overflow-tooltip
        />
        <el-table-column
          label="型号"
          prop="model"
          width="150"
          show-overflow-tooltip
        />
        <el-table-column label="数量" prop="qty" width="110" align="right" />
        <el-table-column label="单价" prop="price" width="120" align="right" />
        <el-table-column label="金额" prop="amount" width="120" align="right" />
        <el-table-column
          label="备注"
          prop="remark"
          min-width="180"
          show-overflow-tooltip
        />
      </el-table>
    </div>

    <template #footer>
      <div class="flex justify-end">
        <el-button @click="drawerApi.close()">关闭</el-button>
      </div>
    </template>
  </Drawer>
</template>
