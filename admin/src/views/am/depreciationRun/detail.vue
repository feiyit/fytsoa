<script setup lang="ts">
import { useSoaDrawer } from "@/component/soaDrawer/index.vue";
import { fetchAmDepreciationRunById } from "@/api/am/depreciationRun";
import { fetchAdminById } from "@/api/sys/admin";
import { fetchAmAssetById } from "@/api/am/asset";

const title = ref("计提详情");
const loading = ref(false);

const formData = ref<any>({});
const runUserObj = ref<any | null>(null);
const assetCache = reactive<Record<string, any>>({});

const [Drawer, drawerApi] = useSoaDrawer({
  onCancel: () => drawerApi.close(),
});

const statusText = (v: number) => (Number(v) === 1 ? "已确认" : "草稿");

const getAdminLabel = (u: any) =>
  u?.fullName ||
  u?.displayName ||
  u?.userName ||
  u?.loginAccount ||
  u?.mobile ||
  u?.id;

const runUserLabel = computed(() => {
  const id =
    formData.value?.runUserId != null ? String(formData.value.runUserId) : "";
  const name = getAdminLabel(runUserObj.value);
  if (name && id && id !== "0") return `${name} (${id})`;
  if (name) return String(name);
  return id && id !== "0" ? `ID:${id}` : "-";
});

const assetLabel = (assetId: any) => {
  const id = assetId != null ? String(assetId) : "";
  const a = assetCache[id];
  const no = a?.assetNo;
  const name = a?.name;
  if (no && name) return `${no} / ${name}`;
  return no || name || (id && id !== "0" ? `ID:${id}` : "-");
};

const ensureAssets = async (items: any[]) => {
  const ids = Array.from(
    new Set(
      (items || [])
        .map((it) => (it?.assetId != null ? String(it.assetId) : ""))
        .filter((x) => x && x !== "0" && !assetCache[x]),
    ),
  );
  await Promise.all(
    ids.map(async (id) => {
      try {
        assetCache[id] = await fetchAmAssetById(id);
      } catch {
        // ignore
      }
    }),
  );
};

const openModal = async (rowOrId: any) => {
  const id = typeof rowOrId === "object" ? rowOrId?.id : rowOrId;
  if (!id) return;

  title.value = `计提详情：${rowOrId?.period || ""}`.trim() || "计提详情";
  loading.value = true;
  try {
    const res: any = await fetchAmDepreciationRunById(String(id));
    formData.value = res || {};
    if (!Array.isArray(formData.value.items)) formData.value.items = [];

    // 执行人回显
    runUserObj.value = null;
    if (res?.runUserObj) {
      runUserObj.value = res.runUserObj;
    } else if (res?.runUserId && String(res.runUserId) !== "0") {
      try {
        runUserObj.value = await fetchAdminById(res.runUserId);
      } catch {
        runUserObj.value = null;
      }
    }

    await ensureAssets(formData.value.items || []);
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
      <el-descriptions :column="3" border class="mb-3">
        <el-descriptions-item label="期间">{{
          formData.period || "-"
        }}</el-descriptions-item>
        <el-descriptions-item label="状态">{{
          statusText(formData.status)
        }}</el-descriptions-item>
        <el-descriptions-item label="执行人">{{
          runUserLabel
        }}</el-descriptions-item>

        <el-descriptions-item label="执行时间">{{
          formData.runTime || "-"
        }}</el-descriptions-item>
        <el-descriptions-item label="本期合计">{{
          formData.totalAmount ?? "-"
        }}</el-descriptions-item>
        <el-descriptions-item label="备注">{{
          formData.remark || "-"
        }}</el-descriptions-item>
      </el-descriptions>

      <div class="mb-2 flex items-center justify-between">
        <div class="text-sm text-gray-500">
          明细：{{ (formData.items || []).length }} 条
        </div>
      </div>

      <el-table :data="formData.items || []" border height="520">
        <el-table-column type="index" width="50" />
        <el-table-column label="资产" min-width="240">
          <template #default="{ row }">
            <span>{{ assetLabel(row.assetId) }}</span>
          </template>
        </el-table-column>
        <el-table-column
          label="本期折旧"
          prop="amount"
          width="150"
          align="right"
        />
        <el-table-column
          label="累计折旧"
          prop="accumAmount"
          width="150"
          align="right"
        />
        <el-table-column
          label="净值"
          prop="netBookValue"
          width="150"
          align="right"
        />
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
