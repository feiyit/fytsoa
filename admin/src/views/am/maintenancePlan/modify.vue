<script setup lang="ts">
import { useSoaModal } from "@/component/soaModal/index.vue";
import SoaAssetPicker from "@/component/soaAssetPicker/index.vue";
import {
  addAmMaintenancePlan,
  updateAmMaintenancePlan,
  fetchAmMaintenancePlanById,
} from "@/api/am/maintenancePlan";
import { fetchAmAssetById } from "@/api/am/asset";
import { fetchSysCodeList } from "@/api/sys/code";
import { fetchAmLocationList } from "@/api/am/location";
import { fetchAmWarehouseList } from "@/api/am/warehouse";
import { fetchAmVendorList } from "@/api/am/vendor";
import { fetchOrgUnitList } from "@/api";
import { fetchAdminList } from "@/api";
import { changeTree, generateCode } from "@/utils/tools";

const formRef = ref();
const saving = ref(false);
const emit = defineEmits(["complete"]);

// 保养计划编号前缀（可统一调整）
const MAINTENANCE_PLAN_NO_PREFIX = "BY-";

// sys_codetype 中 AM_ASSET_CATEGORY 对应的 Id（和资产/盘点模块一致）
const AM_ASSET_CATEGORY_TYPE_ID = "2013975685776936960";

type ScopeMode = "FILTER" | "ASSET";
const scopeMode = ref<ScopeMode>("FILTER");

// 指定资产（多选）
const assetPickerRef = ref<any>(null);
const pickedAssets = ref<any[]>([]);

// 条件筛选（多选）
const categoryOptions = ref<{ label: string; value: string }[]>([]);
const locationOptions = ref<any[]>([]);
const warehouseOptions = ref<any[]>([]);
const vendorOptions = ref<any[]>([]);
const orgOptions = ref<any[]>([]);
const adminOptions = ref<any[]>([]);

const scopeCategoryIds = ref<string[]>([]);
const scopeLocationIds = ref<string[]>([]);
const scopeWarehouseIds = ref<string[]>([]);
const scopeVendorIds = ref<string[]>([]);
const scopeOrgUnitIds = ref<string[]>([]);

const formData = reactive<any>({
  id: "0",
  tenantId: "0",
  planNo: "",
  name: "",
  cycleType: "MONTH",
  cycleValue: 1,
  managerId: undefined as any,
  nextRunTime: undefined as any,
  isEnabled: true,
  scopeJson: "{}",
  remark: "",
});

const safeParse = (s: any) => {
  try {
    if (!s) return {};
    return typeof s === "string" ? JSON.parse(s) : s;
  } catch {
    return {};
  }
};

const buildScopeJson = () => {
  const isAssetMode = scopeMode.value === "ASSET";
  return {
    v: 1,
    assetIds: isAssetMode
      ? Array.from(
          new Set(
            (pickedAssets.value || [])
              .map((a: any) => String(a?.id ?? ""))
              .filter((x) => x && x !== "0"),
          ),
        )
      : [],
    categoryIds: scopeCategoryIds.value || [],
    locationIds: scopeLocationIds.value || [],
    warehouseIds: scopeWarehouseIds.value || [],
    vendorIds: scopeVendorIds.value || [],
    orgUnitIds: scopeOrgUnitIds.value || [],
  };
};

const triggerScopeValidate = () =>
  nextTick(() => formRef.value?.validateField?.("scopeJson"));

const validateScope = (_rule: any, _value: any, callback: any) => {
  if (scopeMode.value === "ASSET") {
    if ((pickedAssets.value || []).length === 0) {
      callback(new Error("请选择资产"));
      return;
    }
    callback();
    return;
  }

  const hasAny =
    (scopeCategoryIds.value || []).length ||
    (scopeLocationIds.value || []).length ||
    (scopeWarehouseIds.value || []).length ||
    (scopeVendorIds.value || []).length ||
    (scopeOrgUnitIds.value || []).length;

  if (!hasAny) {
    callback(new Error("请选择范围条件（或切换到“指定资产”）"));
    return;
  }
  callback();
};

const rules = {
  planNo: [{ required: true, message: "请输入计划编号", trigger: "blur" }],
  name: [{ required: true, message: "请输入计划名称", trigger: "blur" }],
  managerId: [{ required: true, message: "请选择保养管理员", trigger: "change" }],
  scopeJson: [{ validator: validateScope, trigger: "change" }],
};

const title = ref("新增保养计划");
const [Modal, modalApi] = useSoaModal({
  onConfirm: () => handleSubmit(),
  onCancel: () => resetForm(),
});

const loadOptions = async () => {
  // 分类（sys_code），TypeId=AM_ASSET_CATEGORY
  const codes = await fetchSysCodeList({
    id: AM_ASSET_CATEGORY_TYPE_ID,
    status: "1",
  });
  categoryOptions.value = (codes || []).map((m: any) => ({
    label: m.name,
    value: String(m.id),
  }));

  // 地点/仓库/供应商
  locationOptions.value = (await fetchAmLocationList({ status: "1" })) || [];
  warehouseOptions.value = (await fetchAmWarehouseList({ status: "1" })) || [];
  vendorOptions.value = (await fetchAmVendorList({ status: "1" })) || [];
  adminOptions.value = (await fetchAdminList({ page: 1, limit: 1000 })) || [];

  // 组织机构树
  const org = await fetchOrgUnitList({ page: 1, limit: 1000 });
  const orgTree = (org || []).map((m: any) => ({
    id: String(m.id),
    value: String(m.id),
    label: m.name,
    parentId: String(m.parentId ?? "0"),
  }));
  orgOptions.value = changeTree(orgTree);
};

const hydrateScope = async (scopeJson: any) => {
  const s = safeParse(scopeJson);
  const assetIds = Array.isArray(s?.assetIds) ? s.assetIds.map((x: any) => String(x)) : [];
  const categoryIds = Array.isArray(s?.categoryIds) ? s.categoryIds.map((x: any) => String(x)) : [];
  const locationIds = Array.isArray(s?.locationIds) ? s.locationIds.map((x: any) => String(x)) : [];
  const warehouseIds = Array.isArray(s?.warehouseIds) ? s.warehouseIds.map((x: any) => String(x)) : [];
  const vendorIds = Array.isArray(s?.vendorIds) ? s.vendorIds.map((x: any) => String(x)) : [];
  const orgUnitIds = Array.isArray(s?.orgUnitIds) ? s.orgUnitIds.map((x: any) => String(x)) : [];

  scopeMode.value = assetIds.length ? "ASSET" : "FILTER";
  scopeCategoryIds.value = categoryIds;
  scopeLocationIds.value = locationIds;
  scopeWarehouseIds.value = warehouseIds;
  scopeVendorIds.value = vendorIds;
  scopeOrgUnitIds.value = orgUnitIds;

  // 资产回显：按 id 拉取资产对象，用于 tag 展示/预选
  pickedAssets.value = [];
  if (assetIds.length) {
    try {
      const assets = await Promise.all(assetIds.map((id: string) => fetchAmAssetById(String(id))));
      pickedAssets.value = (assets || []).filter(Boolean);
    } catch {
      pickedAssets.value = [];
    }
  }
};

const openAssetPicker = () => {
  assetPickerRef.value?.openModal({
    multiple: true,
    title: "选择资产（多选）",
    picked: pickedAssets.value || [],
  });
};

const onAssetPicked = (payload: { rows: any[]; multiple: boolean }) => {
  pickedAssets.value = payload?.rows || [];
  triggerScopeValidate();
};

const removePickedAsset = (id: string) => {
  pickedAssets.value = (pickedAssets.value || []).filter((x: any) => String(x?.id) !== String(id));
  triggerScopeValidate();
};

const openModal = async (row?: any) => {
  resetForm();
   await loadOptions();
  const id = row?.id ? String(row.id) : "0";
  title.value = id !== "0" ? "编辑保养计划" : "新增保养计划";
  if (id !== "0") {
    const res = await fetchAmMaintenancePlanById(id);
    Object.assign(formData, res || {});
    await hydrateScope(formData.scopeJson);
  } else if (row) {
    Object.assign(formData, row);
    await hydrateScope(formData.scopeJson);
  }
  modalApi.open();
};

const generateMaintenancePlanNo = () => {
  formData.planNo = generateCode(MAINTENANCE_PLAN_NO_PREFIX, 8);
};

const handleSubmit = () => {
  if (saving.value) return;
  // 先把可视化选择转换为 scopeJson，再执行表单校验/提交
  formData.scopeJson = JSON.stringify(buildScopeJson());
  formRef.value?.validate(async (valid: any) => {
    if (!valid) return;
    saving.value = true;
    try {
      if (!formData.id || formData.id === "0") {
        await addAmMaintenancePlan(formData);
        ElMessage.success("新增成功");
      } else {
        await updateAmMaintenancePlan(formData);
        ElMessage.success("更新成功");
      }
      emit("complete");
      modalApi.close();
    } finally {
      saving.value = false;
      resetForm();
    }
  });
};

const resetForm = () => {
  Object.assign(formData, {
    id: "0",
    tenantId: "0",
    planNo: "",
    name: "",
    cycleType: "MONTH",
    cycleValue: 1,
    managerId: undefined,
    nextRunTime: undefined,
    isEnabled: true,
    scopeJson: "{}",
    remark: "",
  });
  scopeMode.value = "FILTER";
  pickedAssets.value = [];
  scopeCategoryIds.value = [];
  scopeLocationIds.value = [];
  scopeWarehouseIds.value = [];
  scopeVendorIds.value = [];
  scopeOrgUnitIds.value = [];
};

defineExpose({ openModal });
</script>

<template>
  <Modal :title="title" class="w-[900px]" draggable :closeOnClickModal="false">
    <el-form ref="formRef" :model="formData" :rules="rules" label-width="110px">
      <el-row :gutter="12">
        <el-col :span="12">
          <el-form-item label="计划编号" prop="planNo">
            <el-input
              v-model="formData.planNo"
              placeholder="请输入计划编号"
              clearable
              maxlength="32"
            >
              <template #append>
                <el-button @click="generateMaintenancePlanNo"
                  >自动生成</el-button
                >
              </template>
            </el-input>
          </el-form-item>
        </el-col>
        <el-col :span="12">
          <el-form-item label="计划名称" prop="name">
            <el-input
              v-model="formData.name"
              placeholder="请输入计划名称"
              clearable
              maxlength="100"
              show-word-limit
            />
          </el-form-item>
        </el-col>

        <el-col :span="12">
          <el-form-item label="周期类型">
            <el-select v-model="formData.cycleType" style="width: 100%">
              <el-option label="天" value="DAY" />
              <el-option label="周" value="WEEK" />
              <el-option label="月" value="MONTH" />
              <el-option label="年" value="YEAR" />
            </el-select>
          </el-form-item>
        </el-col>
        <el-col :span="12">
          <el-form-item label="周期值">
            <el-input-number
              v-model="formData.cycleValue"
              :min="1"
              style="width: 100%"
            />
          </el-form-item>
        </el-col>

        <el-col :span="12">
          <el-form-item label="下次执行">
            <el-date-picker
              v-model="formData.nextRunTime"
              type="datetime"
              value-format="YYYY-MM-DD HH:mm:ss"
              format="YYYY年MM月DD日 HH:mm:ss"
              style="width: 100%"
            />
          </el-form-item>
        </el-col>
        <el-col :span="12">
          <el-form-item label="保养管理员" prop="managerId">
            <el-select
              v-model="formData.managerId"
              placeholder="请选择保养管理员"
              filterable
              clearable
              style="width: 100%"
            >
              <el-option
                v-for="u in adminOptions"
                :key="String(u.id)"
                :label="u.fullName || u.loginAccount || String(u.id)"
                :value="u.id"
              />
            </el-select>
          </el-form-item>
        </el-col>

        <el-col :span="12">
          <el-form-item label="启用">
            <el-switch
              v-model="formData.isEnabled"
              active-text="是"
              inactive-text="否"
            />
          </el-form-item>
        </el-col>

        <el-col :span="24">
          <el-form-item label="范围" prop="scopeJson">
            <div class="w-full">
              <el-radio-group v-model="scopeMode" @change="triggerScopeValidate">
                <el-radio-button label="FILTER">按条件</el-radio-button>
                <el-radio-button label="ASSET">指定资产</el-radio-button>
              </el-radio-group>

              <div v-if="scopeMode === 'ASSET'" class="mt-3">
                <div class="mb-2 flex items-center gap-2">
                  <el-button type="primary" @click="openAssetPicker">选择资产</el-button>
                  <span class="text-xs text-slate-500">已选 {{ (pickedAssets || []).length }} 条</span>
                </div>
                <div class="flex flex-wrap gap-2">
                  <el-tag
                    v-for="a in pickedAssets"
                    :key="String(a.id)"
                    closable
                    @close="removePickedAsset(String(a.id))"
                  >
                    {{ a.assetNo || a.name || a.id }}
                    <template v-if="a.assetNo && a.name"> / {{ a.name }}</template>
                  </el-tag>
                </div>
              </div>

              <div v-else class="mt-3">
                <el-row :gutter="12">
                  <el-col :span="12">
                    <div class="mb-1 text-xs text-slate-500">资产分类</div>
                    <el-select
                      v-model="scopeCategoryIds"
                      multiple
                      filterable
                      clearable
                      collapse-tags
                      collapse-tags-tooltip
                      placeholder="分类(可多选)"
                      style="width: 100%"
                      @change="triggerScopeValidate"
                    >
                      <el-option
                        v-for="it in categoryOptions"
                        :key="it.value"
                        :label="it.label"
                        :value="it.value"
                      />
                    </el-select>
                  </el-col>
                  <el-col :span="12">
                    <div class="mb-1 text-xs text-slate-500">供应商</div>
                    <el-select
                      v-model="scopeVendorIds"
                      multiple
                      filterable
                      clearable
                      collapse-tags
                      collapse-tags-tooltip
                      placeholder="供应商(可多选)"
                      style="width: 100%"
                      @change="triggerScopeValidate"
                    >
                      <el-option
                        v-for="it in vendorOptions"
                        :key="String(it.id)"
                        :label="it.name || it.vendorName || String(it.id)"
                        :value="String(it.id)"
                      />
                    </el-select>
                  </el-col>

                  <el-col :span="12" class="mt-3">
                    <div class="mb-1 text-xs text-slate-500">地点</div>
                    <el-select
                      v-model="scopeLocationIds"
                      multiple
                      filterable
                      clearable
                      collapse-tags
                      collapse-tags-tooltip
                      placeholder="地点(可多选)"
                      style="width: 100%"
                      @change="triggerScopeValidate"
                    >
                      <el-option
                        v-for="it in locationOptions"
                        :key="String(it.id)"
                        :label="it.name || it.locationName || String(it.id)"
                        :value="String(it.id)"
                      />
                    </el-select>
                  </el-col>
                  <el-col :span="12" class="mt-3">
                    <div class="mb-1 text-xs text-slate-500">仓库</div>
                    <el-select
                      v-model="scopeWarehouseIds"
                      multiple
                      filterable
                      clearable
                      collapse-tags
                      collapse-tags-tooltip
                      placeholder="仓库(可多选)"
                      style="width: 100%"
                      @change="triggerScopeValidate"
                    >
                      <el-option
                        v-for="it in warehouseOptions"
                        :key="String(it.id)"
                        :label="it.name || it.warehouseName || String(it.id)"
                        :value="String(it.id)"
                      />
                    </el-select>
                  </el-col>

                  <el-col :span="24" class="mt-3">
                    <div class="mb-1 text-xs text-slate-500">所属部门</div>
                    <el-tree-select
                      v-model="scopeOrgUnitIds"
                      :data="orgOptions"
                      multiple
                      show-checkbox
                      check-strictly
                      default-expand-all
                      placeholder="部门(可多选)"
                      :style="{ width: '100%' }"
                      @change="triggerScopeValidate"
                    />
                  </el-col>
                </el-row>
              </div>
            </div>
          </el-form-item>
        </el-col>
        <el-col :span="24">
          <el-form-item label="备注">
            <el-input
              v-model="formData.remark"
              placeholder="请输入备注"
              type="textarea"
              :rows="3"
              maxlength="500"
              show-word-limit
            />
          </el-form-item>
        </el-col>
      </el-row>
    </el-form>
  </Modal>

  <SoaAssetPicker ref="assetPickerRef" @select="onAssetPicked" />
</template>
