<script setup lang="ts">
import { useSoaDrawer } from "@/component/soaDrawer/index.vue";
import { addAmAsset, updateAmAsset, fetchAmAssetById } from "@/api/am/asset";
import { fetchOrgUnitList } from "@/api";
import { fetchAmVendorList } from "@/api/am/vendor";
import { fetchAmLocationList } from "@/api/am/location";
import { fetchAmWarehouseList } from "@/api/am/warehouse";
import { changeTree, generateCode } from "@/utils/tools";
import { fetchSysCodeList } from "@/api/sys/code";

const formRef = ref();
const saving = ref(false);
const emit = defineEmits(["complete"]);

const categoryOptions = ref<{ label: string; value: string }[]>([]);
const orgOptions = ref<any[]>([]);
const vendorOptions = ref<any[]>([]);
const locationOptions = ref<any[]>([]);
const warehouseOptions = ref<any[]>([]);

// 资产编号前缀（可统一调整）
const ASSET_NO_PREFIX = "ZC-";
// sys_codetype 中 AM_ASSET_CATEGORY 对应的 Id（你给定的常量）
const AM_ASSET_CATEGORY_TYPE_ID = "2013975685776936960";

const formData = reactive<any>({
  id: "0",
  tenantId: "0",
  assetNo: "",
  name: "",
  categoryId: undefined,
  orgUnitId: undefined,
  locationId: undefined,
  warehouseId: undefined,
  vendorId: undefined,
  tagCode: "",
  brand: "",
  model: "",
  serialNo: "",
  originalValue: 0,
  netBookValue: 0,
  status: 1,
  remark: "",
});

const rules = {
  assetNo: [{ required: true, message: "请输入资产编号", trigger: "blur" }],
  name: [{ required: true, message: "请输入资产名称", trigger: "blur" }],
  categoryId: [{ required: true, message: "请选择分类", trigger: "change" }],
};

const [Drawer, drawerApi] = useSoaDrawer({
  onConfirm: () => handleSubmit(),
  onCancel: () => resetForm(),
});

const title = ref("新增资产");

const loadOptions = async () => {
  // 分类（sys_code），TypeId=AM_ASSET_CATEGORY
  const cats = await fetchSysCodeList({
    id: AM_ASSET_CATEGORY_TYPE_ID,
    status: "1",
  });
  categoryOptions.value = (cats || []).map((m: any) => ({
    label: m.name,
    value: String(m.id),
  }));

  // 部门（sys_org_unit）
  const org = await fetchOrgUnitList({ page: 1, limit: 1000 });
  const orgTree = (org || []).map((m: any) => ({
    id: String(m.id),
    value: String(m.id),
    label: m.name,
    parentId: String(m.parentId ?? "0"),
  }));
  orgOptions.value = changeTree(orgTree);

  // 供应商/地点/仓库（am）
  vendorOptions.value = await fetchAmVendorList({ status: "1" });
  locationOptions.value = await fetchAmLocationList({ status: "1" });
  warehouseOptions.value = await fetchAmWarehouseList({ status: "1" });
};

onMounted(() => {
  loadOptions();
});

const openModal = async (row?: any) => {
  resetForm();
  const id = row?.id ? String(row.id) : "0";
  title.value = id !== "0" ? "编辑资产" : "新增资产";
  if (id !== "0") {
    const res = await fetchAmAssetById(id);
    Object.assign(formData, res || {});
  } else if (row) {
    Object.assign(formData, row);
  }
  drawerApi.open();
};

const generateAssetNo = () => {
  formData.assetNo = generateCode(ASSET_NO_PREFIX, 8);
};

const handleSubmit = () => {
  if (saving.value) return;
  formRef.value?.validate(async (valid: any) => {
    if (!valid) return;
    saving.value = true;
    try {
      if (!formData.id || formData.id === "0") {
        await addAmAsset(formData);
        ElMessage.success("新增成功");
      } else {
        await updateAmAsset(formData);
        ElMessage.success("更新成功");
      }
      emit("complete");
      drawerApi.close();
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
    assetNo: "",
    name: "",
    categoryId: undefined,
    orgUnitId: undefined,
    locationId: undefined,
    warehouseId: undefined,
    vendorId: undefined,
    tagCode: "",
    brand: "",
    model: "",
    serialNo: "",
    originalValue: 0,
    netBookValue: 0,
    status: 1,
    remark: "",
  });
};

defineExpose({ openModal });
</script>

<template>
  <Drawer
    :title="title"
    size="60%"
    class="w-[900px]"
    :closeOnClickModal="false"
  >
    <el-form ref="formRef" :model="formData" :rules="rules" label-width="110px">
      <el-row :gutter="12">
        <el-col :span="12">
          <el-form-item label="资产编号" prop="assetNo">
            <el-input
              v-model="formData.assetNo"
              placeholder="请输入资产编号"
              clearable
              maxlength="32"
            >
              <template #append>
                <el-button @click="generateAssetNo">自动生成</el-button>
              </template>
            </el-input>
          </el-form-item>
        </el-col>
        <el-col :span="12">
          <el-form-item label="资产名称" prop="name">
            <el-input
              v-model="formData.name"
              placeholder="请输入资产名称"
              clearable
              maxlength="100"
              show-word-limit
            />
          </el-form-item>
        </el-col>

        <el-col :span="12">
          <el-form-item label="资产分类" prop="categoryId">
            <el-select
              v-model="formData.categoryId"
              placeholder="请选择分类"
              filterable
              clearable
              style="width: 100%"
            >
              <el-option
                v-for="it in categoryOptions"
                :key="it.value"
                :label="it.label"
                :value="it.value"
              />
            </el-select>
          </el-form-item>
        </el-col>
        <el-col :span="12">
          <el-form-item label="所属部门">
            <el-tree-select
              v-model="formData.orgUnitId"
              :data="orgOptions"
              placeholder="请选择部门"
              default-expand-all
              check-strictly
              :style="{ width: '100%' }"
            />
          </el-form-item>
        </el-col>

        <el-col :span="12">
          <el-form-item label="地点">
            <el-select
              v-model="formData.locationId"
              placeholder="请选择地点"
              filterable
              clearable
              style="width: 100%"
            >
              <el-option
                v-for="it in locationOptions"
                :key="it.id"
                :label="it.name"
                :value="String(it.id)"
              />
            </el-select>
          </el-form-item>
        </el-col>
        <el-col :span="12">
          <el-form-item label="仓库">
            <el-select
              v-model="formData.warehouseId"
              placeholder="请选择仓库"
              filterable
              clearable
              style="width: 100%"
            >
              <el-option
                v-for="it in warehouseOptions"
                :key="it.id"
                :label="it.name"
                :value="String(it.id)"
              />
            </el-select>
          </el-form-item>
        </el-col>

        <el-col :span="12">
          <el-form-item label="供应商">
            <el-select
              v-model="formData.vendorId"
              placeholder="请选择供应商"
              filterable
              clearable
              style="width: 100%"
            >
              <el-option
                v-for="it in vendorOptions"
                :key="it.id"
                :label="it.name"
                :value="String(it.id)"
              />
            </el-select>
          </el-form-item>
        </el-col>

        <el-col :span="12">
          <el-form-item label="标签码">
            <el-input
              v-model="formData.tagCode"
              placeholder="请输入标签码"
              clearable
              maxlength="64"
              show-word-limit
            />
          </el-form-item>
        </el-col>

        <el-col :span="12">
          <el-form-item label="品牌">
            <el-input
              v-model="formData.brand"
              placeholder="请输入品牌"
              clearable
              maxlength="50"
              show-word-limit
            />
          </el-form-item>
        </el-col>
        <el-col :span="12">
          <el-form-item label="型号">
            <el-input
              v-model="formData.model"
              placeholder="请输入型号"
              clearable
              maxlength="50"
              show-word-limit
            />
          </el-form-item>
        </el-col>
        <el-col :span="12">
          <el-form-item label="序列号">
            <el-input
              v-model="formData.serialNo"
              placeholder="请输入序列号"
              clearable
              maxlength="64"
              show-word-limit
            />
          </el-form-item>
        </el-col>

        <el-col :span="12">
          <el-form-item label="资产原值">
            <el-input-number
              v-model="formData.originalValue"
              :min="0"
              :precision="2"
              :step="0.01"
              controls-position="right"
              style="width: 100%"
              placeholder="请输入资产原值"
            />
          </el-form-item>
        </el-col>

        <el-col :span="12">
          <el-form-item label="资产净值">
            <el-input-number
              v-model="formData.netBookValue"
              :min="0"
              :precision="2"
              :step="0.01"
              controls-position="right"
              style="width: 100%"
              placeholder="请输入资产净值"
            />
          </el-form-item>
        </el-col>

        <el-col :span="12">
          <el-form-item label="状态">
            <el-select v-model="formData.status" style="width: 100%">
              <el-option label="在库" :value="1" />
              <el-option label="在用" :value="2" />
              <el-option label="借出" :value="3" />
              <el-option label="维修中" :value="4" />
              <el-option label="闲置" :value="5" />
              <el-option label="在途" :value="6" />
              <el-option label="处置中" :value="7" />
              <el-option label="已处置" :value="8" />
            </el-select>
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
  </Drawer>
</template>
