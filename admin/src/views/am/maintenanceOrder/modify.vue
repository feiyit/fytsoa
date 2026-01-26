<script setup lang="ts">
import { useSoaModal } from "@/component/soaModal/index.vue";
import SoaAssetPicker from "@/component/soaAssetPicker/index.vue";
import SoaSelectAdmin from "@/component/soaSelectAdmin/index.vue";
import {
  addAmMaintenanceOrder,
  updateAmMaintenanceOrder,
  fetchAmMaintenanceOrderById,
} from "@/api/am/maintenanceOrder";
import { fetchAmAssetById } from "@/api/am/asset";
import { fetchAmVendorList } from "@/api/am/vendor";
import { fetchAdminById } from "@/api/sys/admin";
import { generateCode } from "@/utils/tools";

const formRef = ref();
const saving = ref(false);
const emit = defineEmits(["complete"]);

// 工单号前缀（可统一调整）
const MAINTENANCE_ORDER_NO_PREFIX = "GD-";

const assetPickerRef = ref<any>(null);
const pickedAsset = ref<any | null>(null);
const vendorOptions = ref<any[]>([]);

const reportUserObj = ref<any | null>(null);
const assignUserObj = ref<any | null>(null);

const formData = reactive<any>({
  id: "0",
  tenantId: "0",
  orderNo: "",
  type: 1,
  status: 0,
  priority: 2,
  assetId: "0",
  title: "",
  description: "",
  reportUserId: "0",
  reportTime: undefined as any,
  assignUserId: "0",
  assignTime: undefined as any,
  vendorId: undefined as any,
  startTime: undefined as any,
  finishTime: undefined as any,
  cost: 0,
  result: "",
});

const rules = {
  orderNo: [{ required: true, message: "请输入工单号", trigger: "blur" }],
  assetId: [{ required: true, message: "请选择资产", trigger: "change" }],
  title: [{ required: true, message: "请输入标题", trigger: "blur" }],
  reportUserId: [
    { required: true, message: "请选择报修人", trigger: "change" },
  ],
  assignUserId: [
    { required: true, message: "请选择指派人", trigger: "change" },
  ],
};

const title = ref("新增工单");
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
  reportUserObj,
  (v) => {
    formData.reportUserId = v?.id ? String(v.id) : "0";
    nextTick(() => formRef.value?.validateField?.("reportUserId"));
  },
  { deep: true },
);

watch(
  assignUserObj,
  (v) => {
    formData.assignUserId = v?.id ? String(v.id) : "0";
    nextTick(() => formRef.value?.validateField?.("assignUserId"));
  },
  { deep: true },
);

watch(
  pickedAsset,
  (a) => {
    formData.assetId = a?.id ? String(a.id) : "0";
    // 选择资产后自动带出供应商
    if (a?.vendorId != null && String(a.vendorId) !== "0") {
      formData.vendorId = String(a.vendorId);
    } else {
      formData.vendorId = undefined;
    }
  },
  { deep: true },
);

const openAssetPicker = () => {
  assetPickerRef.value?.openModal({
    multiple: false,
    title: "选择资产",
    picked: pickedAsset.value || null,
  });
};

const onAssetPicked = (payload: { rows: any[]; multiple: boolean }) => {
  const row = payload?.rows?.[0];
  pickedAsset.value = row || null;
  // 资产变更后，触发表单校验（assetId 的 required）
  nextTick(() => formRef.value?.validateField?.("assetId"));
};

const clearAsset = () => {
  pickedAsset.value = null;
  formData.assetId = "0";
  formData.vendorId = undefined;
  nextTick(() => formRef.value?.validateField?.("assetId"));
};

const loadVendors = async () => {
  vendorOptions.value = (await fetchAmVendorList({ status: "1" })) || [];
};

const hydrateEditRelated = async (res: any) => {
  // 资产回显：优先后端返回的 assetObj，其次按 assetId 拉取
  if (res?.assetObj) {
    pickedAsset.value = res.assetObj;
  } else if (res?.assetId && String(res.assetId) !== "0") {
    try {
      pickedAsset.value = await fetchAmAssetById(String(res.assetId));
    } catch {
      pickedAsset.value = null;
    }
  }

  // 报修人/指派人回显：优先后端返回对象，其次按 id 拉取
  if (res?.reportUserObj) {
    reportUserObj.value = res.reportUserObj;
  } else if (res?.reportUserId && String(res.reportUserId) !== "0") {
    try {
      reportUserObj.value = await fetchAdminById(res.reportUserId);
    } catch {
      reportUserObj.value = null;
    }
  }

  if (res?.assignUserObj) {
    assignUserObj.value = res.assignUserObj;
  } else if (res?.assignUserId && String(res.assignUserId) !== "0") {
    try {
      assignUserObj.value = await fetchAdminById(res.assignUserId);
    } catch {
      assignUserObj.value = null;
    }
  }
};

const openModal = async (row?: any) => {
  resetForm();
  await loadVendors();
  const id = row?.id ? String(row.id) : "0";
  title.value = id !== "0" ? "编辑工单" : "新增工单";
  if (id !== "0") {
    const res = await fetchAmMaintenanceOrderById(id);
    Object.assign(formData, res || {});
    await hydrateEditRelated(res);
  } else if (row) {
    Object.assign(formData, row);
    await hydrateEditRelated(row);
  }
  modalApi.open();
};

const generateMaintenanceOrderNo = () => {
  formData.orderNo = generateCode(MAINTENANCE_ORDER_NO_PREFIX, 8);
};

const handleSubmit = () => {
  if (saving.value) return;
  formRef.value?.validate(async (valid: any) => {
    if (!valid) return;
    saving.value = true;
    try {
      if (!formData.id || formData.id === "0") {
        await addAmMaintenanceOrder(formData);
        ElMessage.success("新增成功");
      } else {
        await updateAmMaintenanceOrder(formData);
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
    orderNo: "",
    type: 1,
    status: 0,
    priority: 2,
    assetId: "0",
    title: "",
    description: "",
    reportUserId: "0",
    reportTime: undefined,
    assignUserId: "0",
    assignTime: undefined,
    vendorId: undefined,
    startTime: undefined,
    finishTime: undefined,
    cost: 0,
    result: "",
  });
  pickedAsset.value = null;
  reportUserObj.value = null;
  assignUserObj.value = null;
};

defineExpose({ openModal });
</script>

<template>
  <Modal :title="title" class="w-[900px]" draggable :closeOnClickModal="false">
    <el-form ref="formRef" :model="formData" :rules="rules" label-width="110px">
      <el-row :gutter="12">
        <el-col :span="12">
          <el-form-item label="工单号" prop="orderNo">
            <el-input
              v-model="formData.orderNo"
              placeholder="请输入工单号"
              clearable
              maxlength="32"
            >
              <template #append>
                <el-button @click="generateMaintenanceOrderNo"
                  >自动生成</el-button
                >
              </template>
            </el-input>
          </el-form-item>
        </el-col>
        <el-col :span="12">
          <el-form-item label="类型">
            <el-select v-model="formData.type" style="width: 100%">
              <el-option label="报修" :value="1" />
              <el-option label="保养" :value="2" />
            </el-select>
          </el-form-item>
        </el-col>

        <el-col :span="12">
          <el-form-item label="状态">
            <el-select v-model="formData.status" style="width: 100%">
              <el-option label="草稿" :value="0" />
              <el-option label="待受理" :value="1" />
              <el-option label="已指派" :value="2" />
              <el-option label="处理中" :value="3" />
              <el-option label="已完成" :value="4" />
              <el-option label="已关闭" :value="5" />
              <el-option label="已取消" :value="6" />
            </el-select>
          </el-form-item>
        </el-col>
        <el-col :span="12">
          <el-form-item label="优先级">
            <el-select v-model="formData.priority" style="width: 100%">
              <el-option label="高" :value="1" />
              <el-option label="中" :value="2" />
              <el-option label="低" :value="3" />
            </el-select>
          </el-form-item>
        </el-col>

        <el-col :span="12">
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
        <el-col :span="12">
          <el-form-item label="供应商">
            <el-select
              v-model="formData.vendorId"
              placeholder="自动带出/可调整"
              clearable
              filterable
              style="width: 100%"
            >
              <el-option
                v-for="it in vendorOptions"
                :key="String(it.id)"
                :label="it.name || it.vendorName || String(it.id)"
                :value="String(it.id)"
              />
            </el-select>
          </el-form-item>
        </el-col>

        <el-col :span="24">
          <el-form-item label="标题" prop="title">
            <el-input
              v-model="formData.title"
              placeholder="请输入标题"
              clearable
              maxlength="100"
              show-word-limit
            />
          </el-form-item>
        </el-col>
        <el-col :span="24">
          <el-form-item label="描述">
            <el-input
              v-model="formData.description"
              placeholder="请输入描述"
              type="textarea"
              :rows="3"
              maxlength="1000"
              show-word-limit
            />
          </el-form-item>
        </el-col>

        <el-col :span="12">
          <el-form-item label="报修人" prop="reportUserId">
            <SoaSelectAdmin
              v-model="reportUserObj"
              :multiple="false"
              width="100%"
            />
          </el-form-item>
        </el-col>
        <el-col :span="12">
          <el-form-item label="报修时间">
            <el-date-picker
              v-model="formData.reportTime"
              type="datetime"
              value-format="YYYY-MM-DD HH:mm:ss"
              format="YYYY年MM月DD日 HH:mm:ss"
              style="width: 100%"
            />
          </el-form-item>
        </el-col>

        <el-col :span="12">
          <el-form-item label="指派人" prop="assignUserId">
            <SoaSelectAdmin
              v-model="assignUserObj"
              :multiple="false"
              width="100%"
            />
          </el-form-item>
        </el-col>
        <el-col :span="12">
          <el-form-item label="指派时间">
            <el-date-picker
              v-model="formData.assignTime"
              type="datetime"
              value-format="YYYY-MM-DD HH:mm:ss"
              format="YYYY年MM月DD日 HH:mm:ss"
              style="width: 100%"
            />
          </el-form-item>
        </el-col>

        <el-col :span="12">
          <el-form-item label="费用">
            <el-input-number
              v-model="formData.cost"
              :min="0"
              :precision="2"
              style="width: 100%"
            />
          </el-form-item>
        </el-col>
        <el-col :span="24">
          <el-form-item label="处理结果">
            <el-input
              v-model="formData.result"
              placeholder="请输入处理结果"
              type="textarea"
              :rows="2"
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
