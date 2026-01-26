<script setup lang="ts">
import { useSoaModal } from "@/component/soaModal/index.vue";
import SoaSelectAdmin from "@/component/soaSelectAdmin/index.vue";
import SoaAssetPicker from "@/component/soaAssetPicker/index.vue";
import {
  addAmReminderTask,
  updateAmReminderTask,
  fetchAmReminderTaskById,
} from "@/api/am/reminderTask";
import { fetchAmReminderRuleList } from "@/api/am/reminderRule";
import { fetchAdminById } from "@/api/sys/admin";
import { fetchAmAssetById } from "@/api/am/asset";

const formRef = ref();
const saving = ref(false);
const emit = defineEmits(["complete"]);

const ruleTypeOptions = [
  { value: "BORROW_DUE", label: "借用到期（BORROW_DUE）" },
  { value: "WARRANTY_EXPIRE", label: "质保到期（WARRANTY_EXPIRE）" },
  { value: "INVENTORY_DUE", label: "盘点到期（INVENTORY_DUE）" },
  { value: "MAINTENANCE_DUE", label: "保养到期（MAINTENANCE_DUE）" },
  { value: "TRANSFER_SIGN", label: "调拨签收（TRANSFER_SIGN）" },
];

const ruleTypeLabel = (v: any) => {
  const key = String(v ?? "");
  const hit = ruleTypeOptions.find((x) => x.value === key);
  return hit ? hit.label : key || "-";
};

const bizTypeOptions = [
  { value: "ASSET", label: "资产（ASSET）" },
  { value: "DOC", label: "单据（DOC）" },
  { value: "INVENTORY", label: "盘点（INVENTORY）" },
  { value: "MAINTENANCE", label: "维修/保养（MAINTENANCE）" },
];

const ruleOptions = ref<any[]>([]);
const receiverUserObj = ref<any | null>(null);

// bizType=ASSET 时支持选择资产
const assetPickerRef = ref<any>(null);
const pickedBizAsset = ref<any | null>(null);

const formData = reactive<any>({
  id: "0",
  tenantId: "0",
  ruleId: undefined,
  bizType: "ASSET",
  bizId: "0",
  receiverUserId: "0",
  title: "",
  content: "",
  dueTime: undefined as any,
  status: 0,
});

const rules = {
  ruleId: [{ required: true, message: "请选择提醒规则", trigger: "change" }],
  bizType: [{ required: true, message: "请选择业务类型", trigger: "change" }],
  bizId: [{ required: true, message: "请选择/填写业务Id", trigger: "change" }],
  receiverUserId: [
    { required: true, message: "请选择接收人", trigger: "change" },
  ],
  title: [{ required: true, message: "请输入标题", trigger: "blur" }],
};

const title = ref("新增任务");
const [Modal, modalApi] = useSoaModal({
  onConfirm: () => handleSubmit(),
  onCancel: () => resetForm(),
});

const loadRules = async () => {
  // 提醒规则列表（用于 ruleId 下拉选择）
  const res: any = await fetchAmReminderRuleList({});
  ruleOptions.value = Array.isArray(res) ? res : res?.items || [];
};

watch(
  receiverUserObj,
  (v) => {
    formData.receiverUserId = v?.id ? String(v.id) : "0";
    nextTick(() => formRef.value?.validateField?.("receiverUserId"));
  },
  { deep: true },
);

watch(
  () => formData.bizType,
  (v) => {
    // 切换业务类型时，清理业务Id/已选资产，避免脏数据
    if (v !== "ASSET") {
      pickedBizAsset.value = null;
    }
    formData.bizId = "0";
    nextTick(() => formRef.value?.validateField?.("bizId"));
  },
);

watch(
  pickedBizAsset,
  (a) => {
    if (formData.bizType !== "ASSET") return;
    formData.bizId = a?.id ? String(a.id) : "0";
    nextTick(() => formRef.value?.validateField?.("bizId"));
  },
  { deep: true },
);

const bizAssetDisplay = computed(() => {
  const a = pickedBizAsset.value;
  if (a?.assetNo || a?.name) {
    const left = a.assetNo ? String(a.assetNo) : "";
    const right = a.name ? String(a.name) : "";
    return left && right ? `${left} / ${right}` : left || right;
  }
  return formData.bizId && String(formData.bizId) !== "0"
    ? `ID:${formData.bizId}`
    : "";
});

const openAssetPicker = () => {
  assetPickerRef.value?.openModal({
    multiple: false,
    title: "选择资产",
    picked: pickedBizAsset.value || null,
  });
};

const onAssetPicked = (payload: { rows: any[]; multiple: boolean }) => {
  pickedBizAsset.value = payload?.rows?.[0] || null;
};

const clearBizAsset = () => {
  pickedBizAsset.value = null;
  formData.bizId = "0";
  nextTick(() => formRef.value?.validateField?.("bizId"));
};

const bizIdHint = computed(() => {
  switch (String(formData.bizType || "")) {
    case "ASSET":
      return "业务Id = 资产Id（建议直接选择资产，不要手填）";
    case "DOC":
      return "业务Id = 单据Id（来自“单据”模块列表）";
    case "INVENTORY":
      return "业务Id = 盘点单/盘点计划Id（来自“盘点”模块列表）";
    case "MAINTENANCE":
      return "业务Id = 维修/保养工单Id（来自“维修工单/保养工单”列表）";
    default:
      return "业务Id = 业务对象的主键Id（取决于业务类型）";
  }
});

const hydrateEditRelated = async (res: any) => {
  // 回显接收人
  receiverUserObj.value = null;
  if (res?.receiverUserObj) {
    receiverUserObj.value = res.receiverUserObj;
  } else if (res?.receiverUserId && String(res.receiverUserId) !== "0") {
    try {
      receiverUserObj.value = await fetchAdminById(res.receiverUserId);
    } catch {
      receiverUserObj.value = null;
    }
  }

  // bizType=ASSET 时回显资产
  pickedBizAsset.value = null;
  if (
    String(res?.bizType || "") === "ASSET" &&
    res?.bizId &&
    String(res.bizId) !== "0"
  ) {
    try {
      pickedBizAsset.value = await fetchAmAssetById(String(res.bizId));
    } catch {
      pickedBizAsset.value = null;
    }
  }
};

const openModal = async (row?: any) => {
  resetForm();
  await loadRules();
  const id = row?.id ? String(row.id) : "0";
  title.value = id !== "0" ? "编辑任务" : "新增任务";
  if (id !== "0") {
    const res = await fetchAmReminderTaskById(id);
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
        await addAmReminderTask(formData);
        ElMessage.success("新增成功");
      } else {
        await updateAmReminderTask(formData);
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
    ruleId: undefined,
    bizType: "ASSET",
    bizId: "0",
    receiverUserId: "0",
    title: "",
    content: "",
    dueTime: undefined,
    status: 0,
  });
  receiverUserObj.value = null;
  pickedBizAsset.value = null;
};

defineExpose({ openModal });
</script>

<template>
  <Modal :title="title" class="w-[900px]" draggable :closeOnClickModal="false">
    <el-form ref="formRef" :model="formData" :rules="rules" label-width="110px">
      <el-row :gutter="12">
        <el-col :span="12">
          <el-form-item label="提醒规则" prop="ruleId">
            <el-select
              v-model="formData.ruleId"
              placeholder="请选择提醒规则"
              clearable
              filterable
              style="width: 100%"
            >
              <el-option
                v-for="it in ruleOptions"
                :key="String(it.id)"
                :label="`${ruleTypeLabel(it.ruleType)} / 提前${it.daysBefore ?? 0}天 (ID:${it.id})`"
                :value="String(it.id)"
              />
            </el-select>
          </el-form-item>
        </el-col>
        <el-col :span="12">
          <el-form-item label="状态">
            <el-select v-model="formData.status" style="width: 100%">
              <el-option label="待发送" :value="0" />
              <el-option label="已发送" :value="1" />
              <el-option label="已读" :value="2" />
              <el-option label="已关闭" :value="3" />
            </el-select>
          </el-form-item>
        </el-col>

        <el-col :span="12">
          <el-form-item label="业务类型" prop="bizType">
            <el-select v-model="formData.bizType" style="width: 100%">
              <el-option
                v-for="it in bizTypeOptions"
                :key="it.value"
                :label="it.label"
                :value="it.value"
              />
            </el-select>
          </el-form-item>
        </el-col>
        <el-col :span="12">
          <el-form-item label="业务" prop="bizId">
            <div class="w-full">
              <template v-if="formData.bizType === 'ASSET'">
                <el-input
                  :model-value="bizAssetDisplay"
                  placeholder="请选择资产"
                  readonly
                  @click="openAssetPicker"
                >
                  <template #append>
                    <div class="flex items-center gap-1">
                      <el-button type="primary" @click.stop="openAssetPicker"
                        >选择</el-button
                      >
                      <el-button
                        v-if="pickedBizAsset"
                        @click.stop="clearBizAsset"
                        >清空</el-button
                      >
                    </div>
                  </template>
                </el-input>
              </template>
              <template v-else>
                <el-input
                  v-model="formData.bizId"
                  placeholder="请输入业务Id"
                  clearable
                  maxlength="32"
                  show-word-limit
                />
              </template>
              <el-text type="info" size="small" class="mt-1 block">{{
                bizIdHint
              }}</el-text>
            </div>
          </el-form-item>
        </el-col>

        <el-col :span="12">
          <el-form-item label="接收人" prop="receiverUserId">
            <SoaSelectAdmin
              v-model="receiverUserObj"
              :multiple="false"
              width="100%"
            />
          </el-form-item>
        </el-col>
        <el-col :span="12">
          <el-form-item label="到期时间">
            <el-date-picker
              v-model="formData.dueTime"
              type="datetime"
              value-format="YYYY-MM-DD HH:mm:ss"
              format="YYYY年MM月DD日 HH:mm:ss"
              style="width: 100%"
            />
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
          <el-form-item label="内容">
            <el-input
              v-model="formData.content"
              placeholder="请输入内容"
              type="textarea"
              :rows="4"
              maxlength="1000"
              show-word-limit
            />
          </el-form-item>
        </el-col>
      </el-row>
    </el-form>
  </Modal>

  <SoaAssetPicker ref="assetPickerRef" @select="onAssetPicked" />
</template>
