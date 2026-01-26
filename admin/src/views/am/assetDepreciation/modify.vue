<script setup lang="ts">
import { useSoaModal } from "@/component/soaModal/index.vue";
import SoaAssetPicker from "@/component/soaAssetPicker/index.vue";
import {
  addAmAssetDepreciation,
  updateAmAssetDepreciation,
  fetchAmAssetDepreciationById,
} from "@/api/am/assetDepreciation";
import { fetchAmAssetById } from "@/api/am/asset";

const formRef = ref();
const saving = ref(false);
const emit = defineEmits(["complete"]);

const assetPickerRef = ref<any>(null);
const pickedAsset = ref<any | null>(null);

const formData = reactive<any>({
  id: "0",
  tenantId: "0",
  assetId: "0",
  method: 0,
  lifeMonths: 0,
  salvageRate: 0,
  startDate: undefined as any,
  accumAmount: 0,
  lastPeriod: "",
  status: 0,
});

const isNoDepreciation = computed(() => Number(formData.method || 0) === 0);

const validateLifeMonths = (_rule: any, _value: any, callback: any) => {
  if (isNoDepreciation.value) return callback();
  const v = Number(formData.lifeMonths || 0);
  if (!v || v <= 0) return callback(new Error("请输入折旧期(月)"));
  callback();
};

const methodHelpText = computed(() => {
  switch (Number(formData.method || 0)) {
    case 0:
      return "不折旧：该资产不计提折旧，不会按月生成折旧额；折旧期/残值率/起算日期将不生效。";
    case 1:
      return "直线法：在使用年限内平均分摊折旧，每月折旧额相同（常用于大多数固定资产）。";
    case 2:
      return "双倍余额递减法：前期多、后期少，按期初净值乘以固定折旧率（通常为 2/使用年限）计提。";
    case 3:
      return "年数总和法：前期多、后期少，按“剩余年数/年数总和”比例分摊折旧。";
    default:
      return "";
  }
});

const rules = {
  assetId: [{ required: true, message: "请选择资产", trigger: "change" }],
  lifeMonths: [{ validator: validateLifeMonths, trigger: "change" }],
};

const title = ref("新增折旧配置");
const [Modal, modalApi] = useSoaModal({
  onConfirm: () => handleSubmit(),
  onCancel: () => resetForm(),
});

const assetDisplay = computed(() => {
  const a = pickedAsset.value;
  if (a?.assetNo || a?.name) {
    const left = a.assetNo ? String(a.assetNo) : "";
    const right = a.name ? String(a.name) : "";
    return left && right ? `${left} / ${right}` : left || right;
  }
  return formData.assetId && String(formData.assetId) !== "0"
    ? `ID:${formData.assetId}`
    : "";
});

watch(
  pickedAsset,
  (a) => {
    formData.assetId = a?.id ? String(a.id) : "0";
    nextTick(() => formRef.value?.validateField?.("assetId"));
  },
  { deep: true },
);

watch(
  () => formData.method,
  (m) => {
    const method = Number(m || 0);
    if (method === 0) {
      // 不折旧：清空/禁用折旧参数
      formData.lifeMonths = 0;
      formData.salvageRate = 0;
      formData.startDate = undefined;
      formData.status = 0;
      nextTick(() => formRef.value?.clearValidate?.(["lifeMonths"]));
    } else {
      // 开启折旧时，给个更合理的默认值
      if (!formData.lifeMonths || Number(formData.lifeMonths) <= 0)
        formData.lifeMonths = 12;
    }
  },
  { immediate: true },
);

const openAssetPicker = () => {
  assetPickerRef.value?.openModal({
    multiple: false,
    title: "选择资产",
    picked: pickedAsset.value || null,
  });
};

const onAssetPicked = (payload: { rows: any[]; multiple: boolean }) => {
  pickedAsset.value = payload?.rows?.[0] || null;
};

const clearAsset = () => {
  pickedAsset.value = null;
  formData.assetId = "0";
  nextTick(() => formRef.value?.validateField?.("assetId"));
};

const hydrateEditRelated = async (res: any) => {
  if (res?.assetObj) {
    pickedAsset.value = res.assetObj;
    return;
  }
  if (res?.assetId && String(res.assetId) !== "0") {
    try {
      pickedAsset.value = await fetchAmAssetById(String(res.assetId));
    } catch {
      pickedAsset.value = null;
    }
  }
};

const openModal = async (row?: any) => {
  resetForm();
  const id = row?.id ? String(row.id) : "0";
  title.value = id !== "0" ? "编辑折旧配置" : "新增折旧配置";
  if (id !== "0") {
    const res = await fetchAmAssetDepreciationById(id);
    Object.assign(formData, res || {});
    await hydrateEditRelated(res);
  } else if (row) {
    Object.assign(formData, row);
    await hydrateEditRelated(row);
  }
  modalApi.open();
};

const handleSubmit = () => {
  if (saving.value) return;
  formRef.value?.validate(async (valid: any) => {
    if (!valid) return;
    saving.value = true;
    try {
      if (!formData.id || formData.id === "0") {
        await addAmAssetDepreciation(formData);
        ElMessage.success("新增成功");
      } else {
        await updateAmAssetDepreciation(formData);
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
    assetId: "0",
    method: 0,
    lifeMonths: 0,
    salvageRate: 0,
    startDate: undefined,
    accumAmount: 0,
    lastPeriod: "",
    status: 0,
  });
  pickedAsset.value = null;
};

defineExpose({ openModal });
</script>

<template>
  <Modal :title="title" class="w-[750px]" draggable :closeOnClickModal="false">
    <el-form ref="formRef" :model="formData" :rules="rules" label-width="110px">
      <el-row :gutter="12">
        <el-col :span="24">
          <el-form-item label="资产" prop="assetId">
            <el-input
              :model-value="assetDisplay"
              placeholder="请选择资产"
              readonly
              @click="openAssetPicker"
            >
              <template #append>
                <div class="flex items-center gap-1">
                  <el-button
                    type="primary"
                    icon="Check"
                    @click.stop="openAssetPicker"
                  ></el-button>
                  <el-button
                    v-if="pickedAsset"
                    icon="CircleClose"
                    style="margin-left: 2px"
                    @click.stop="clearAsset"
                  ></el-button>
                </div>
              </template>
            </el-input>
          </el-form-item>
        </el-col>
        <el-col :span="24">
          <el-form-item label="折旧方法">
            <div class="w-full">
              <el-select v-model="formData.method" style="width: 100%">
                <el-option label="不折旧" :value="0" />
                <el-option label="直线法" :value="1" />
                <el-option label="双倍余额" :value="2" />
                <el-option label="年数总和" :value="3" />
              </el-select>
              <div
                v-if="methodHelpText"
                class="mt-1 text-xs leading-5 text-slate-500"
              >
                {{ methodHelpText }}
              </div>
            </div>
          </el-form-item>
        </el-col>
        <el-col :span="24">
          <el-form-item label="折旧期(月)" prop="lifeMonths">
            <el-input-number
              v-model="formData.lifeMonths"
              :min="0"
              :disabled="isNoDepreciation"
              style="width: 100%"
            />
          </el-form-item>
        </el-col>
        <el-col :span="24">
          <el-form-item label="残值率(%)">
            <el-input-number
              v-model="formData.salvageRate"
              :min="0"
              :precision="2"
              :disabled="isNoDepreciation"
              style="width: 100%"
            />
          </el-form-item>
        </el-col>
        <el-col :span="24">
          <el-form-item label="起算日期">
            <el-date-picker
              v-model="formData.startDate"
              type="date"
              value-format="YYYY-MM-DD"
              format="YYYY年MM月DD日"
              :disabled="isNoDepreciation"
              style="width: 100%"
            />
          </el-form-item>
        </el-col>
        <el-col :span="24">
          <el-form-item label="状态">
            <el-select
              v-model="formData.status"
              :disabled="isNoDepreciation"
              style="width: 100%"
            >
              <el-option label="未启用" :value="0" />
              <el-option label="折旧中" :value="1" />
              <el-option label="已停用" :value="2" />
            </el-select>
          </el-form-item>
        </el-col>
      </el-row>
    </el-form>
  </Modal>

  <SoaAssetPicker ref="assetPickerRef" @select="onAssetPicked" />
</template>
