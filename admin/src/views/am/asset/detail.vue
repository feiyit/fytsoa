<script setup lang="ts">
import { fetchAmAssetById } from "@/api/am/asset";

const route = useRoute();
const router = useRouter();

const loading = ref(false);
const asset = ref<any>({});

const assetId = computed(() => {
  const q = route.query?.id;
  const p = (route.params as any)?.id;
  return q != null ? String(q) : p != null ? String(p) : "";
});

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
      return v != null ? String(v) : "-";
  }
};

const load = async () => {
  const id = assetId.value;
  if (!id || id === "0") {
    asset.value = {};
    return;
  }
  loading.value = true;
  try {
    asset.value = (await fetchAmAssetById(id)) || {};
  } finally {
    loading.value = false;
  }
};

watch(assetId, load, { immediate: true });
</script>

<template>
  <el-container class="h-full">
    <el-header
      class="bg-card border-border flex flex-wrap items-center justify-between gap-3 rounded-[.5vw]"
    >
      <div class="flex items-center gap-2">
        <el-button @click="router.back()">返回</el-button>
        <div class="text-base font-medium">
          资产详情：{{ asset.assetNo || "-" }}
          <span v-if="asset.name">/ {{ asset.name }}</span>
        </div>
      </div>
      <div class="flex items-center gap-2">
        <el-button :loading="loading" @click="load">刷新</el-button>
      </div>
    </el-header>

    <el-main class="bg-card border-border mt-2 rounded-[.5vw]">
      <el-card v-loading="loading" shadow="never">
        <el-descriptions :column="2" border>
          <el-descriptions-item label="资产Id">
            {{ asset.id || assetId || "-" }}
          </el-descriptions-item>
          <el-descriptions-item label="状态">
            <el-tag :type="asset.status === 1 ? 'success' : 'info'">
              {{ statusText(asset.status) }}
            </el-tag>
          </el-descriptions-item>

          <el-descriptions-item label="资产编号">
            {{ asset.assetNo || "-" }}
          </el-descriptions-item>
          <el-descriptions-item label="资产名称">
            {{ asset.name || "-" }}
          </el-descriptions-item>

          <el-descriptions-item label="分类">
            {{ asset.categoryObj?.name || asset.categoryName || asset.categoryId || "-" }}
          </el-descriptions-item>
          <el-descriptions-item label="供应商">
            {{ asset.vendorObj?.name || asset.vendorName || asset.vendorId || "-" }}
          </el-descriptions-item>

          <el-descriptions-item label="所属部门">
            {{ asset.orgUnitObj?.name || asset.orgUnitName || asset.orgUnitId || "-" }}
          </el-descriptions-item>
          <el-descriptions-item label="地点">
            {{ asset.locationObj?.name || asset.locationName || asset.locationId || "-" }}
          </el-descriptions-item>

          <el-descriptions-item label="仓库">
            {{ asset.warehouseObj?.name || asset.warehouseName || asset.warehouseId || "-" }}
          </el-descriptions-item>
          <el-descriptions-item label="标签码">
            {{ asset.tagCode || "-" }}
          </el-descriptions-item>

          <el-descriptions-item label="品牌">
            {{ asset.brand || "-" }}
          </el-descriptions-item>
          <el-descriptions-item label="型号">
            {{ asset.model || "-" }}
          </el-descriptions-item>

          <el-descriptions-item label="序列号">
            {{ asset.serialNo || "-" }}
          </el-descriptions-item>
          <el-descriptions-item label="备注">
            {{ asset.remark || "-" }}
          </el-descriptions-item>

          <el-descriptions-item label="创建时间">
            {{ asset.createTime || "-" }}
          </el-descriptions-item>
          <el-descriptions-item label="更新时间">
            {{ asset.updateTime || "-" }}
          </el-descriptions-item>
        </el-descriptions>
      </el-card>
    </el-main>
  </el-container>
</template>

