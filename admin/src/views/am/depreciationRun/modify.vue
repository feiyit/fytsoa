<script setup lang="ts">
import { useSoaDrawer } from "@/component/soaDrawer/index.vue";
import SoaSelectAdmin from "@/component/soaSelectAdmin/index.vue";
import soaFormTable from "@/component/soaFormTable/index.vue";
import SoaAssetPicker from "@/component/soaAssetPicker/index.vue";
import {
  addAmDepreciationRun,
  updateAmDepreciationRun,
  fetchAmDepreciationRunById,
} from "@/api/am/depreciationRun";
import { fetchAdminById } from "@/api/sys/admin";
import { fetchAmAssetById } from "@/api/am/asset";

const formRef = ref();
const saving = ref(false);
const emit = defineEmits(["complete"]);

const activeTab = ref("basic");
const title = ref("新增计提");

const runUserObj = ref<any | null>(null);

const assetPickerRef = ref<any>(null);
const pickingTargetRow = ref<any>(null);
const itemsTableRef = ref<any>(null);

const formData = reactive<any>({
  id: "0",
  tenantId: "0",
  period: "",
  status: 0,
  runUserId: "0",
  runTime: undefined as any,
  remark: "",
  items: [] as any[],
});

const itemTemplate = {
  id: "0",
  assetId: "0",
  assetNo: "",
  assetName: "",
  amount: 0,
  accumAmount: 0,
  netBookValue: 0,
  remark: "",
};

const rules = {
  period: [{ required: true, message: "请输入期间(YYYY-MM)", trigger: "blur" }],
  runUserId: [{ required: true, message: "请选择执行人", trigger: "change" }],
};

const [Drawer, drawerApi] = useSoaDrawer({
  onConfirm: () => handleSubmit(),
  onCancel: () => resetForm(),
});

watch(
  runUserObj,
  (v) => {
    formData.runUserId = v?.id ? String(v.id) : "0";
    nextTick(() => formRef.value?.validateField?.("runUserId"));
  },
  { deep: true },
);

const rowAssetDisplay = (row: any) => {
  const left = row?.assetNo ? String(row.assetNo) : "";
  const right = row?.assetName ? String(row.assetName) : "";
  if (left && right) return `${left} / ${right}`;
  return (
    left ||
    right ||
    (row?.assetId && String(row.assetId) !== "0" ? `ID:${row.assetId}` : "")
  );
};

const openModal = async (row?: any) => {
  resetForm();
  const id = row?.id ? String(row.id) : "0";
  title.value = id !== "0" ? "编辑计提" : "新增计提";
  activeTab.value = "basic";
  if (id !== "0") {
    const res = await fetchAmDepreciationRunById(id);
    Object.assign(formData, res || {});
    if (!Array.isArray(formData.items)) formData.items = [];
    // 回显执行人
    if (res?.runUserObj) {
      runUserObj.value = res.runUserObj;
    } else if (res?.runUserId && String(res.runUserId) !== "0") {
      try {
        runUserObj.value = await fetchAdminById(res.runUserId);
      } catch {
        runUserObj.value = null;
      }
    }
    // 回显资产（如果后端只返回 assetId，这里补拉资产编号/名称用于展示）
    await Promise.all(
      (formData.items || [])
        .filter(
          (it: any) =>
            it?.assetId &&
            String(it.assetId) !== "0" &&
            (!it.assetNo || !it.assetName),
        )
        .map(async (it: any) => {
          try {
            const a: any = await fetchAmAssetById(String(it.assetId));
            it.assetNo = it.assetNo || a?.assetNo || "";
            it.assetName = it.assetName || a?.name || "";
          } catch {
            // ignore
          }
        }),
    );
  } else if (row) {
    Object.assign(formData, row);
    // 新增时传入默认值也尝试回显
    if (row?.runUserObj) runUserObj.value = row.runUserObj;
  }
  drawerApi.open();
};

const addEmptyItemRow = () => {
  itemsTableRef.value?.rowAdd?.();
};

const openAssetPicker = (opts: { multiple: boolean; row?: any }) => {
  pickingTargetRow.value = opts.row || null;
  assetPickerRef.value?.openModal({
    multiple: opts.multiple,
    title: opts.multiple ? "选择资产（多选）" : "选择资产",
    picked: opts.multiple
      ? (formData.items || []).map((it: any) => ({ id: it.assetId }))
      : opts.row?.assetId && String(opts.row.assetId) !== "0"
        ? { id: String(opts.row.assetId) }
        : null,
  });
};

const applyAssetToRow = (row: any, asset: any) => {
  row.assetId = String(asset.id);
  row.assetNo = asset.assetNo || "";
  row.assetName = asset.name || "";
};

const onAssetPicked = (payload: { rows: any[]; multiple: boolean }) => {
  const rows = payload?.rows || [];
  if (!rows.length) return;

  // 单选：回填到目标行
  if (!payload.multiple && pickingTargetRow.value) {
    applyAssetToRow(pickingTargetRow.value, rows[0]);
    pickingTargetRow.value = null;
    return;
  }

  // 多选：批量追加明细（按 assetId 去重）
  const exists = new Set(
    (formData.items || []).map((it: any) => String(it.assetId || "")),
  );
  rows.forEach((a: any) => {
    const assetId = String(a.id);
    if (!assetId || exists.has(assetId)) return;
    const item =
      itemsTableRef.value?.rowAdd?.() ??
      JSON.parse(JSON.stringify(itemTemplate));
    applyAssetToRow(item, a);
    exists.add(assetId);
  });
};

const calcTotal = () => {
  const total = (formData.items || []).reduce(
    (sum: number, it: any) => sum + Number(it.amount || 0),
    0,
  );
  return Number(total.toFixed(2));
};

const handleSubmit = () => {
  if (saving.value) return;
  formRef.value?.validate(async (valid: any) => {
    if (!valid) return;
    saving.value = true;
    try {
      // 提交时只保留后端需要的字段，避免把 UI 扩展字段(assetNo/assetName) 一并提交
      const payload = {
        ...formData,
        runUserId: formData.runUserId,
        totalAmount: calcTotal(),
        items: (formData.items || []).map((it: any) => ({
          id: it.id,
          assetId: it.assetId,
          amount: it.amount,
          accumAmount: it.accumAmount,
          netBookValue: it.netBookValue,
          remark: it.remark,
        })),
      };
      if (!formData.id || formData.id === "0") {
        await addAmDepreciationRun(payload);
        ElMessage.success("新增成功");
      } else {
        await updateAmDepreciationRun(payload);
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
    period: "",
    status: 0,
    runUserId: "0",
    runTime: undefined,
    remark: "",
    items: [],
  });
  runUserObj.value = null;
};

defineExpose({ openModal });
</script>

<template>
  <Drawer :title="title" size="65%" :closeOnClickModal="false">
    <el-tabs v-model="activeTab" class="mt-1">
      <el-tab-pane label="基础信息" name="basic">
        <el-form
          ref="formRef"
          :model="formData"
          :rules="rules"
          label-width="110px"
        >
          <el-row :gutter="12">
            <el-col :span="8">
              <el-form-item label="期间" prop="period">
                <el-date-picker
                  v-model="formData.period"
                  type="month"
                  value-format="YYYY-MM"
                  format="YYYY年MM月"
                  placeholder="选择期间"
                  clearable
                  style="width: 100%"
                />
              </el-form-item>
            </el-col>
            <el-col :span="8">
              <el-form-item label="状态">
                <el-select v-model="formData.status" style="width: 100%">
                  <el-option label="草稿" :value="0" />
                  <el-option label="已确认" :value="1" />
                </el-select>
              </el-form-item>
            </el-col>
            <el-col :span="8">
              <el-form-item label="执行人" prop="runUserId">
                <SoaSelectAdmin
                  v-model="runUserObj"
                  :multiple="false"
                  width="100%"
                />
              </el-form-item>
            </el-col>
            <el-col :span="8">
              <el-form-item label="执行时间">
                <el-date-picker
                  v-model="formData.runTime"
                  type="datetime"
                  value-format="YYYY-MM-DD HH:mm:ss"
                  format="YYYY年MM月DD日 HH:mm:ss"
                  style="width: 100%"
                />
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
      </el-tab-pane>

      <el-tab-pane label="明细" name="items">
        <div class="mb-2 flex justify-between">
          <div class="flex items-center gap-2">
            <el-button
              type="primary"
              @click="openAssetPicker({ multiple: true })"
              >选择资产</el-button
            >
            <el-button @click="addEmptyItemRow">新增行</el-button>
          </div>
          <div class="text-sm text-gray-500">
            本期合计：{{ calcTotal().toFixed(2) }}
          </div>
        </div>
        <soaFormTable
          ref="itemsTableRef"
          v-model="formData.items"
          :addTemplate="itemTemplate"
          :isAdd="false"
          height="520"
        >
          <el-table-column label="资产" min-width="260">
            <template #default="{ row }">
              <el-input
                :model-value="rowAssetDisplay(row)"
                placeholder="请选择资产"
                readonly
              >
                <template #append>
                  <el-button @click="openAssetPicker({ multiple: false, row })"
                    >选择</el-button
                  >
                </template>
              </el-input>
            </template>
          </el-table-column>

          <el-table-column label="本期折旧" width="160">
            <template #default="{ row }">
              <el-input-number
                v-model="row.amount"
                :min="0"
                :precision="2"
                controls-position="right"
                style="width: 100%"
              />
            </template>
          </el-table-column>
          <el-table-column label="累计折旧" width="160">
            <template #default="{ row }">
              <el-input-number
                v-model="row.accumAmount"
                :min="0"
                :precision="2"
                controls-position="right"
                style="width: 100%"
              />
            </template>
          </el-table-column>
          <el-table-column label="净值" width="160">
            <template #default="{ row }">
              <el-input-number
                v-model="row.netBookValue"
                :min="0"
                :precision="2"
                controls-position="right"
                style="width: 100%"
              />
            </template>
          </el-table-column>
          <el-table-column label="备注" min-width="200">
            <template #default="{ row }">
              <el-input v-model="row.remark" clearable />
            </template>
          </el-table-column>
        </soaFormTable>
      </el-tab-pane>
    </el-tabs>
  </Drawer>

  <SoaAssetPicker ref="assetPickerRef" @select="onAssetPicked" />
</template>
