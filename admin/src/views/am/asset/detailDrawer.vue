<script setup lang="ts">
import { useSoaDrawer } from "@/component/soaDrawer/index.vue";
import { fetchAmAssetById } from "@/api/am/asset";

const title = ref("资产详情");
const loading = ref(false);
const formData = ref<any>({});

const [Drawer, drawerApi] = useSoaDrawer({
  onCancel: () => drawerApi.close(),
});

const statusText = (v: number) => {
  switch (v) {
    case 1:
      return "在库";
    case 2:
      return "在用";
    case 3:
      return "借出";
    case 4:
      return "维修中";
    case 5:
      return "闲置";
    case 6:
      return "在途";
    case 7:
      return "处置中";
    case 8:
      return "已处置";
    default:
      return v != null ? String(v) : "-";
  }
};

const statusTagType = (v: number) => {
  switch (v) {
    case 2:
      return "success";
    case 4:
      return "danger";
    case 7:
      return "warning";
    case 8:
      return "danger";
    default:
      return "info";
  }
};

const openModal = async (rowOrId: any) => {
  const id = typeof rowOrId === "object" ? rowOrId?.assetId || rowOrId?.id : rowOrId;
  if (!id) return;

  const t = typeof rowOrId === "object" ? rowOrId?.assetNo || rowOrId?.name || "" : "";
  title.value = t ? `资产详情：${t}` : "资产详情";

  loading.value = true;
  try {
    formData.value = (await fetchAmAssetById(String(id))) || {};
  } finally {
    loading.value = false;
  }

  drawerApi.open();
};

defineExpose({ openModal });
</script>

<template>
  <Drawer :title="title" size="65%" :closeOnClickModal="false" :footer="false">
    <div v-loading="loading">
      <el-descriptions :column="2" border>
        <el-descriptions-item label="资产Id">
          {{ formData.id || "-" }}
        </el-descriptions-item>
        <el-descriptions-item label="状态">
          <el-tag :type="statusTagType(formData.status)">
            {{ statusText(formData.status) }}
          </el-tag>
        </el-descriptions-item>

        <el-descriptions-item label="资产编号">
          {{ formData.assetNo || "-" }}
        </el-descriptions-item>
        <el-descriptions-item label="资产名称">
          {{ formData.name || "-" }}
        </el-descriptions-item>

        <el-descriptions-item label="分类">
          {{ formData.categoryObj?.name || formData.categoryName || formData.categoryId || "-" }}
        </el-descriptions-item>
        <el-descriptions-item label="供应商">
          {{ formData.vendorObj?.name || formData.vendorName || formData.vendorId || "-" }}
        </el-descriptions-item>

        <el-descriptions-item label="所属部门">
          {{ formData.orgUnitObj?.name || formData.orgUnitName || formData.orgUnitId || "-" }}
        </el-descriptions-item>
        <el-descriptions-item label="地点">
          {{ formData.locationObj?.name || formData.locationName || formData.locationId || "-" }}
        </el-descriptions-item>

        <el-descriptions-item label="仓库">
          {{ formData.warehouseObj?.name || formData.warehouseName || formData.warehouseId || "-" }}
        </el-descriptions-item>
        <el-descriptions-item label="标签码">
          {{ formData.tagCode || "-" }}
        </el-descriptions-item>

        <el-descriptions-item label="品牌">
          {{ formData.brand || "-" }}
        </el-descriptions-item>
        <el-descriptions-item label="型号">
          {{ formData.model || "-" }}
        </el-descriptions-item>

        <el-descriptions-item label="序列号">
          {{ formData.serialNo || "-" }}
        </el-descriptions-item>
        <el-descriptions-item label="备注">
          {{ formData.remark || "-" }}
        </el-descriptions-item>

        <el-descriptions-item label="创建时间">
          {{ formData.createTime || "-" }}
        </el-descriptions-item>
        <el-descriptions-item label="更新时间">
          {{ formData.updateTime || "-" }}
        </el-descriptions-item>
      </el-descriptions>
    </div>

    <template #footer>
      <div class="flex justify-end">
        <el-button @click="drawerApi.close()">关闭</el-button>
      </div>
    </template>
  </Drawer>
</template>
