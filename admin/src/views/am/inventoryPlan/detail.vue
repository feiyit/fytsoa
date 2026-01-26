<script setup lang="ts">
import { useSoaDrawer } from "@/component/soaDrawer/index.vue";
import { fetchAmInventoryPlanById } from "@/api/am/inventoryPlan";
import { fetchAdminList } from "@/api";
import { fetchAmLocationList } from "@/api/am/location";

const title = ref("盘点详情");
const loading = ref(false);

const adminOptions = ref<any[]>([]);
const locationOptions = ref<any[]>([]);

const formData = reactive<any>({
  id: "0",
  tenantId: "0",
  planNo: "",
  name: "",
  status: 0,
  startTime: undefined as any,
  endTime: undefined as any,
  scopeJson: "",
  assigneeIdsJson: "",
  remark: "",
  items: [] as any[],
});

const [Drawer, drawerApi] = useSoaDrawer({
  onCancel: () => reset(),
});

const reset = () => {
  Object.assign(formData, {
    id: "0",
    tenantId: "0",
    planNo: "",
    name: "",
    status: 0,
    startTime: undefined,
    endTime: undefined,
    scopeJson: "",
    assigneeIdsJson: "",
    remark: "",
    items: [],
  });
};

const loadOptions = async () => {
  const [adminsRes, locationsRes] = await Promise.all([
    fetchAdminList({ page: 1, limit: 1000 }) as any,
    fetchAmLocationList({ status: "1" }) as any,
  ]);

  adminOptions.value = Array.isArray(adminsRes)
    ? adminsRes
    : adminsRes?.items || [];
  locationOptions.value = Array.isArray(locationsRes)
    ? locationsRes
    : locationsRes?.items || [];
};

const statusText = (v: number) => {
  switch (v) {
    case 0:
      return "草稿";
    case 1:
      return "进行中";
    case 2:
      return "已完成";
    case 3:
      return "已取消";
    default:
      return "未知";
  }
};

const userLabel = (id: any) => {
  const sid = String(id || "");
  if (!sid || sid === "0") return "-";
  const u = adminOptions.value.find((x: any) => String(x.id) === sid);
  return (
    u?.fullName ||
    u?.displayName ||
    u?.loginAccount ||
    u?.mobile ||
    u?.id ||
    sid
  );
};

const locationLabel = (id: any) => {
  const sid = String(id || "");
  if (!sid || sid === "0") return "-";
  const l = locationOptions.value.find((x: any) => String(x.id) === sid);
  return l?.name || l?.code || l?.id || sid;
};

const openModal = async (row?: any) => {
  reset();
  const id = row?.id ? String(row.id) : "0";
  if (id === "0") return;

  loading.value = true;
  try {
    await loadOptions();
    const res = await fetchAmInventoryPlanById(id);
    Object.assign(formData, res || {});
    if (!Array.isArray(formData.items)) formData.items = [];

    // 详情接口的资产信息通常在 item.assetObj 内；这里同步到常用字段便于表格展示
    (formData.items || []).forEach((it: any) => {
      const a = it.assetObj;
      if (!a) return;
      it.assetNo = it.assetNo || a.assetNo || "";
      it.name = it.name || a.name || "";
      it.tagCode = it.tagCode || a.tagCode || "";
      it.model = it.model || a.model || "";
    });
  } finally {
    loading.value = false;
  }

  drawerApi.open();
};

defineExpose({ openModal });
</script>

<template>
  <Drawer :title="title" size="65%" :closeOnClickModal="false" :footer="false">
    <div v-loading="loading" class="space-y-3">
      <el-descriptions :column="2" border>
        <el-descriptions-item label="盘点编号">
          {{ formData.planNo || "-" }}
        </el-descriptions-item>
        <el-descriptions-item label="盘点名称">
          {{ formData.name || "-" }}
        </el-descriptions-item>
        <el-descriptions-item label="状态">
          {{ statusText(Number(formData.status || 0)) }}
        </el-descriptions-item>
        <el-descriptions-item label="开始时间">
          {{ formData.startTime || "-" }}
        </el-descriptions-item>
        <el-descriptions-item label="结束时间">
          {{ formData.endTime || "-" }}
        </el-descriptions-item>
        <el-descriptions-item label="范围(JSON)">
          {{ formData.scopeJson || "[]" }}
        </el-descriptions-item>
        <el-descriptions-item label="执行人(JSON)">
          {{ formData.assigneeIdsJson || "[]" }}
        </el-descriptions-item>
        <el-descriptions-item label="备注">
          {{ formData.remark || "-" }}
        </el-descriptions-item>
      </el-descriptions>

      <el-card>
        <template #header>
          <div class="flex items-center justify-between">
            <div>盘点明细</div>
            <div class="text-xs text-gray-500">
              共 {{ (formData.items || []).length }} 条
            </div>
          </div>
        </template>

        <el-table :data="formData.items" border height="520">
          <el-table-column label="资产编号" min-width="180">
            <template #default="{ row }">
              {{ row.assetNo || row.assetObj?.assetNo || "-" }}
            </template>
          </el-table-column>
          <el-table-column label="资产名称" min-width="200">
            <template #default="{ row }">
              {{ row.name || row.assetObj?.name || "-" }}
            </template>
          </el-table-column>
          <el-table-column label="标签码" prop="tagCode" width="160" />
          <el-table-column label="型号" prop="model" width="160" />

          <el-table-column label="系统地点" min-width="160">
            <template #default="{ row }">
              {{ locationLabel(row.expectedLocationId) }}
            </template>
          </el-table-column>
          <el-table-column label="盘点地点" min-width="160">
            <template #default="{ row }">
              {{ locationLabel(row.actualLocationId) }}
            </template>
          </el-table-column>

          <el-table-column label="系统责任人" min-width="160">
            <template #default="{ row }">
              {{ userLabel(row.expectedCustodianId) }}
            </template>
          </el-table-column>
          <el-table-column label="盘点人员" min-width="160">
            <template #default="{ row }">
              {{ userLabel(row.actualCustodianId || row.scanUserId) }}
            </template>
          </el-table-column>

          <el-table-column label="结果" prop="result" width="120" />
          <el-table-column label="盘点时间" prop="scanTime" width="180" />
          <el-table-column label="备注" prop="remark" min-width="180" />
        </el-table>
      </el-card>
    </div>
  </Drawer>
</template>
