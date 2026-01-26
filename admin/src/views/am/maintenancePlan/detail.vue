<script setup lang="ts">
import { useSoaModal } from "@/component/soaModal/index.vue";
import { fetchAmMaintenancePlanById } from "@/api/am/maintenancePlan";
import { fetchAmAssetById } from "@/api/am/asset";
import { fetchSysCodeList } from "@/api/sys/code";
import { fetchAmLocationList } from "@/api/am/location";
import { fetchAmWarehouseList } from "@/api/am/warehouse";
import { fetchAmVendorList } from "@/api/am/vendor";
import { fetchOrgUnitList } from "@/api";

const title = ref("计划详情");
const loading = ref(false);
const formData = ref<any>({});

const [Modal, modalApi] = useSoaModal({
  onConfirm: () => modalApi.close(),
  onCancel: () => modalApi.close(),
  // 详情查看不需要确认按钮；这里仍保留 Confirm 行为为“关闭”
});

// sys_codetype 中 AM_ASSET_CATEGORY 对应的 Id（和资产/盘点模块一致）
const AM_ASSET_CATEGORY_TYPE_ID = "2013975685776936960";

const cycleText = (v: string) => {
  switch (v) {
    case "DAY":
      return "天";
    case "WEEK":
      return "周";
    case "MONTH":
      return "月";
    case "YEAR":
      return "年";
    default:
      return v || "-";
  }
};

const safeParse = (v: any) => {
  try {
    if (!v) return {};
    return typeof v === "string" ? JSON.parse(v) : v;
  } catch {
    return {};
  }
};

const scopeModeText = computed(() => {
  const s = safeParse(formData.value?.scopeJson);
  const assetIds = Array.isArray(s?.assetIds) ? s.assetIds : [];
  return assetIds.length ? "指定资产" : "按条件";
});

const scopeView = reactive({
  assets: [] as any[],
  categories: [] as string[],
  vendors: [] as string[],
  locations: [] as string[],
  warehouses: [] as string[],
  orgUnits: [] as string[],
});

const optionMapsLoaded = ref(false);
const categoryMap = ref<Record<string, string>>({});
const vendorMap = ref<Record<string, string>>({});
const locationMap = ref<Record<string, string>>({});
const warehouseMap = ref<Record<string, string>>({});
const orgUnitMap = ref<Record<string, string>>({});

const toIdArray = (arr: any) =>
  (Array.isArray(arr) ? arr : [])
    .map((x: any) => String(x))
    .filter((x: string) => x && x !== "0");

const buildNameMap = (list: any[], idKey: string, nameKeys: string[]) => {
  const map: Record<string, string> = {};
  (list || []).forEach((it: any) => {
    const id = String(it?.[idKey] ?? it?.id ?? "");
    if (!id) return;
    const name =
      nameKeys.map((k) => it?.[k]).find((x) => x != null && String(x).trim()) ??
      it?.name ??
      it?.vendorName ??
      it?.locationName ??
      it?.warehouseName;
    map[id] = name != null && String(name).trim() ? String(name) : id;
  });
  return map;
};

const ensureOptionMaps = async () => {
  if (optionMapsLoaded.value) return;

  const [codes, vendors, locations, warehouses, orgUnits] = await Promise.all([
    fetchSysCodeList({ id: AM_ASSET_CATEGORY_TYPE_ID, status: "1" }),
    fetchAmVendorList({ status: "1" }),
    fetchAmLocationList({ status: "1" }),
    fetchAmWarehouseList({ status: "1" }),
    fetchOrgUnitList({ page: 1, limit: 1000 }),
  ]);

  categoryMap.value = buildNameMap(codes || [], "id", ["name"]);
  vendorMap.value = buildNameMap(vendors || [], "id", ["name", "vendorName"]);
  locationMap.value = buildNameMap(locations || [], "id", ["name", "locationName"]);
  warehouseMap.value = buildNameMap(warehouses || [], "id", ["name", "warehouseName"]);
  orgUnitMap.value = buildNameMap(orgUnits || [], "id", ["name"]);

  optionMapsLoaded.value = true;
};

const resolveScope = async () => {
  Object.assign(scopeView, {
    assets: [],
    categories: [],
    vendors: [],
    locations: [],
    warehouses: [],
    orgUnits: [],
  });

  const s = safeParse(formData.value?.scopeJson);

  const assetIds = toIdArray(s?.assetIds);
  if (assetIds.length) {
    // 指定资产：按 id 拉取资产对象，显示编号/名称
    const assets = await Promise.all(
      assetIds.map(async (id: string) => {
        try {
          return await fetchAmAssetById(String(id));
        } catch {
          return { id };
        }
      }),
    );
    scopeView.assets = (assets || []).filter(Boolean);
    return;
  }

  // 条件筛选：加载字典/列表，把 id 显示为名称
  await ensureOptionMaps();

  const categoryIds = toIdArray(s?.categoryIds);
  const vendorIds = toIdArray(s?.vendorIds);
  const locationIds = toIdArray(s?.locationIds);
  const warehouseIds = toIdArray(s?.warehouseIds);
  const orgUnitIds = toIdArray(s?.orgUnitIds);

  scopeView.categories = categoryIds.map((id) => categoryMap.value[id] || id);
  scopeView.vendors = vendorIds.map((id) => vendorMap.value[id] || id);
  scopeView.locations = locationIds.map((id) => locationMap.value[id] || id);
  scopeView.warehouses = warehouseIds.map((id) => warehouseMap.value[id] || id);
  scopeView.orgUnits = orgUnitIds.map((id) => orgUnitMap.value[id] || id);
};

const openModal = async (rowOrId: any) => {
  const id = typeof rowOrId === "object" ? rowOrId?.id : rowOrId;
  if (!id) return;

  title.value = `计划详情：${rowOrId?.planNo || ""}`.trim() || "计划详情";
  loading.value = true;
  try {
    formData.value = (await fetchAmMaintenancePlanById(String(id))) || {};
    await resolveScope();
  } finally {
    loading.value = false;
  }
  modalApi.open();
};

defineExpose({ openModal });
</script>

<template>
  <Modal :title="title" class="w-[900px]" draggable :closeOnClickModal="false">
    <div v-loading="loading">
      <el-descriptions :column="2" border class="mb-3">
        <el-descriptions-item label="计划编号">{{ formData.planNo || "-" }}</el-descriptions-item>
        <el-descriptions-item label="启用">
          <el-tag :type="formData.isEnabled ? 'success' : 'danger'">
            {{ formData.isEnabled ? "是" : "否" }}
          </el-tag>
        </el-descriptions-item>

        <el-descriptions-item label="计划名称" :span="2">
          {{ formData.name || "-" }}
        </el-descriptions-item>

        <el-descriptions-item label="周期类型">{{ cycleText(formData.cycleType) }}</el-descriptions-item>
        <el-descriptions-item label="周期值">{{ formData.cycleValue ?? "-" }}</el-descriptions-item>

        <el-descriptions-item label="下次执行">{{ formData.nextRunTime || "-" }}</el-descriptions-item>
        <el-descriptions-item label="创建时间">{{ formData.createTime || "-" }}</el-descriptions-item>

        <el-descriptions-item label="更新时间">{{ formData.updateTime || "-" }}</el-descriptions-item>
        <el-descriptions-item label="备注">{{ formData.remark || "-" }}</el-descriptions-item>

        <el-descriptions-item label="范围" :span="2">
          <div class="w-full">
            <div class="mb-2 text-sm text-slate-600">模式：{{ scopeModeText }}</div>

            <template v-if="scopeModeText === '指定资产'">
              <div v-if="scopeView.assets.length" class="flex flex-wrap gap-2">
                <el-tag
                  v-for="a in scopeView.assets"
                  :key="String(a.id)"
                  type="info"
                >
                  {{
                    a.assetNo && a.name
                      ? `${a.assetNo} / ${a.name}`
                      : a.assetNo || a.name || a.id
                  }}
                </el-tag>
              </div>
              <div v-else class="text-sm text-slate-400">未选择资产</div>
            </template>

            <template v-else>
              <div
                v-if="!scopeView.categories.length && !scopeView.vendors.length && !scopeView.locations.length && !scopeView.warehouses.length && !scopeView.orgUnits.length"
                class="text-sm text-slate-400"
              >
                未设置范围条件
              </div>

              <div v-if="scopeView.categories.length" class="mb-2">
                <span class="mr-2 text-sm text-slate-600">分类：</span>
                <el-tag v-for="t in scopeView.categories" :key="`c-${t}`" type="info" class="mr-1">
                  {{ t }}
                </el-tag>
              </div>
              <div v-if="scopeView.vendors.length" class="mb-2">
                <span class="mr-2 text-sm text-slate-600">供应商：</span>
                <el-tag v-for="t in scopeView.vendors" :key="`v-${t}`" type="info" class="mr-1">
                  {{ t }}
                </el-tag>
              </div>
              <div v-if="scopeView.locations.length" class="mb-2">
                <span class="mr-2 text-sm text-slate-600">地点：</span>
                <el-tag v-for="t in scopeView.locations" :key="`l-${t}`" type="info" class="mr-1">
                  {{ t }}
                </el-tag>
              </div>
              <div v-if="scopeView.warehouses.length" class="mb-2">
                <span class="mr-2 text-sm text-slate-600">仓库：</span>
                <el-tag v-for="t in scopeView.warehouses" :key="`w-${t}`" type="info" class="mr-1">
                  {{ t }}
                </el-tag>
              </div>
              <div v-if="scopeView.orgUnits.length">
                <span class="mr-2 text-sm text-slate-600">部门：</span>
                <el-tag v-for="t in scopeView.orgUnits" :key="`o-${t}`" type="info" class="mr-1">
                  {{ t }}
                </el-tag>
              </div>
            </template>
          </div>
        </el-descriptions-item>
      </el-descriptions>
    </div>
  </Modal>
</template>
