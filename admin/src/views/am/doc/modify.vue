<script setup lang="ts">
import { useSoaDrawer } from "@/component/soaDrawer/index.vue";
import { addAmDoc, updateAmDoc, fetchAmDocById } from "@/api/am/doc";
import { generateCode } from "@/utils/tools";
import soaFormTable from "@/component/soaFormTable/index.vue";
import SoaAssetPicker from "@/component/soaAssetPicker/index.vue";

const formRef = ref();
const saving = ref(false);
const emit = defineEmits(["complete"]);

const activeTab = ref("basic");
const title = ref("新增单据");

// 单据号前缀（可统一调整）
const DOC_NO_PREFIX = "DJ-";

const assetPickerRef = ref<any>(null);
const pickingTargetRow = ref<any>(null);
const itemsTableRef = ref<any>(null);

const formData = reactive<any>({
  id: "0",
  tenantId: "0",
  docType: "INBOUND",
  subType: "",
  docNo: "",
  status: 0,
  vendorId: "0",
  fromWarehouseId: "0",
  toWarehouseId: "0",
  fromLocationId: "0",
  toLocationId: "0",
  fromOrgUnitId: "0",
  toOrgUnitId: "0",
  fromCustodianId: "0",
  toCustodianId: "0",
  fromUserId: "0",
  toUserId: "0",
  bizTime: undefined as any,
  dueTime: undefined as any,
  remark: "",
  items: [] as any[],
});

const itemTemplate = {
  id: "0",
  assetId: "0",
  assetNo: "",
  tagCode: "",
  name: "",
  model: "",
  warrantyExpireDate: undefined,
  qty: 1,
  price: 0,
  amount: 0,
  remark: "",
};

const rules = {
  docType: [{ required: true, message: "请选择单据类型", trigger: "change" }],
  docNo: [{ required: true, message: "请输入单据号", trigger: "blur" }],
};

const [Drawer, drawerApi] = useSoaDrawer({
  onConfirm: () => handleSubmit(),
  onCancel: () => resetForm(),
});

const openModal = async (row?: any) => {
  resetForm();
  const id = row?.id ? String(row.id) : "0";
  title.value = id !== "0" ? "编辑单据" : "新增单据";
  activeTab.value = "basic";
  if (id !== "0") {
    const res = await fetchAmDocById(id);
    Object.assign(formData, res || {});
    if (!Array.isArray(formData.items)) formData.items = [];
  } else if (row) {
    Object.assign(formData, row);
  }
  drawerApi.open();
};

const generateDocNo = () => {
  formData.docNo = generateCode(DOC_NO_PREFIX, 8);
};

const openAssetPicker = (opts: { multiple: boolean; row?: any }) => {
  pickingTargetRow.value = opts.row || null;
  assetPickerRef.value?.openModal({
    multiple: opts.multiple,
    title: opts.multiple ? "选择资产（多选）" : "选择资产",
  });
};

const applyAssetToRow = (row: any, asset: any) => {
  row.assetId = String(asset.id);
  row.assetNo = asset.assetNo || "";
  row.tagCode = asset.tagCode || "";
  row.name = asset.name || "";
  row.model = asset.model || "";
  row.warrantyExpireDate = asset.warrantyExpireDate || "";
};

const onAssetPicked = (payload: { rows: any[]; multiple: boolean }) => {
  const rows = payload.rows || [];
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
    // 通过 soaFormTable 的 rowAdd 添加行，保持行为一致（也支持拖拽/内置逻辑扩展）
    const item =
      itemsTableRef.value?.rowAdd?.() ??
      JSON.parse(JSON.stringify(itemTemplate));
    applyAssetToRow(item, a);
    calcAmount(item);
    exists.add(assetId);
  });
};

const calcAmount = (row: any) => {
  const qty = Number(row.qty || 0);
  const price = Number(row.price || 0);
  row.amount = Number((qty * price).toFixed(2));
};

const handleSubmit = () => {
  if (saving.value) return;
  formRef.value?.validate(async (valid: any) => {
    if (!valid) return;
    saving.value = true;
    try {
      // 兜底计算金额
      (formData.items || []).forEach((it: any) => calcAmount(it));

      if (!formData.id || formData.id === "0") {
        await addAmDoc(formData);
        ElMessage.success("新增成功");
      } else {
        await updateAmDoc(formData);
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
    docType: "INBOUND",
    subType: "",
    docNo: "",
    status: 0,
    vendorId: "0",
    fromWarehouseId: "0",
    toWarehouseId: "0",
    fromLocationId: "0",
    toLocationId: "0",
    fromOrgUnitId: "0",
    toOrgUnitId: "0",
    fromCustodianId: "0",
    toCustodianId: "0",
    fromUserId: "0",
    toUserId: "0",
    bizTime: undefined,
    dueTime: undefined,
    remark: "",
    items: [],
  });
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
            <el-col :span="12">
              <el-form-item label="单据类型" prop="docType">
                <el-select
                  v-model="formData.docType"
                  placeholder="请选择单据类型"
                  style="width: 100%"
                >
                  <el-option label="入库" value="INBOUND" />
                  <el-option label="出库" value="OUTBOUND" />
                  <el-option label="归还" value="RETURN" />
                  <el-option label="调拨" value="TRANSFER" />
                  <el-option label="变更" value="CHANGE" />
                  <el-option label="处置" value="DISPOSE" />
                  <el-option label="盘盈盘亏" value="INV_ADJUST" />
                </el-select>
              </el-form-item>
            </el-col>
            <el-col :span="12">
              <el-form-item label="子类型">
                <el-input
                  v-model="formData.subType"
                  placeholder="请输入子类型"
                  clearable
                  maxlength="50"
                  show-word-limit
                />
              </el-form-item>
            </el-col>
            <el-col :span="12">
              <el-form-item label="单据号" prop="docNo">
                <el-input
                  v-model="formData.docNo"
                  placeholder="请输入单据号"
                  clearable
                  maxlength="32"
                  show-word-limit
                >
                  <template #append>
                    <el-button @click="generateDocNo">自动生成</el-button>
                  </template>
                </el-input>
              </el-form-item>
            </el-col>

            <el-col :span="12">
              <el-form-item label="状态">
                <el-select v-model="formData.status" style="width: 100%">
                  <el-option label="草稿" :value="0" />
                  <el-option label="待审批" :value="1" />
                  <el-option label="已通过" :value="2" />
                  <el-option label="已驳回" :value="3" />
                  <el-option label="执行中" :value="4" />
                  <el-option label="已完成" :value="5" />
                  <el-option label="已取消" :value="6" />
                </el-select>
              </el-form-item>
            </el-col>

            <el-col :span="12">
              <el-form-item label="业务时间">
                <el-date-picker
                  v-model="formData.bizTime"
                  type="datetime"
                  value-format="YYYY-MM-DD HH:mm:ss"
                  format="YYYY年MM月DD日 HH:mm:ss"
                  style="width: 100%"
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
          </div>
          <div class="text-sm text-gray-500">
            提示：Name 为必填（后端校验），Qty/Price 会计算 Amount。
          </div>
        </div>
        <soaFormTable
          ref="itemsTableRef"
          v-model="formData.items"
          :addTemplate="itemTemplate"
          height="520"
        >
          <el-table-column label="资产编号" width="190">
            <template #default="{ row }">
              <el-input
                v-model="row.assetNo"
                placeholder="请选择资产"
                clearable
                maxlength="64"
              >
                <template #append>
                  <el-button @click="openAssetPicker({ multiple: false, row })"
                    >选择</el-button
                  >
                </template>
              </el-input>
            </template>
          </el-table-column>
          <el-table-column label="资产名称" min-width="200">
            <template #default="{ row }">
              <el-input
                v-model="row.name"
                placeholder="自动带出"
                clearable
                maxlength="200"
                show-word-limit
              />
            </template>
          </el-table-column>
          <el-table-column label="标签码" width="160">
            <template #default="{ row }">
              <el-input
                v-model="row.tagCode"
                placeholder="自动带出"
                clearable
                maxlength="64"
                show-word-limit
              />
            </template>
          </el-table-column>
          <el-table-column label="型号" width="160">
            <template #default="{ row }">
              <el-input
                v-model="row.model"
                placeholder="自动带出"
                clearable
                maxlength="200"
                show-word-limit
              />
            </template>
          </el-table-column>
          <el-table-column label="质保到期日" width="160">
            <template #default="{ row }">
              <el-date-picker
                v-model="row.warrantyExpireDate"
                type="date"
                value-format="YYYY-MM-DD"
                format="YYYY年MM月DD日"
                style="width: 100%"
              />
            </template>
          </el-table-column>
          <el-table-column label="数量" width="120">
            <template #default="{ row }">
              <el-input-number
                v-model="row.qty"
                :min="0"
                controls-position="right"
                style="width: 100%"
                @change="() => calcAmount(row)"
              />
            </template>
          </el-table-column>
          <el-table-column label="单价" width="140">
            <template #default="{ row }">
              <el-input-number
                v-model="row.price"
                :min="0"
                :precision="2"
                controls-position="right"
                style="width: 100%"
                @change="() => calcAmount(row)"
              />
            </template>
          </el-table-column>
          <el-table-column label="金额" width="120" align="right">
            <template #default="{ row }">
              {{ Number(row.amount || 0).toFixed(2) }}
            </template>
          </el-table-column>
          <el-table-column label="备注" min-width="180">
            <template #default="{ row }">
              <el-input
                v-model="row.remark"
                placeholder="请输入备注"
                clearable
                maxlength="200"
                show-word-limit
              />
            </template>
          </el-table-column>
        </soaFormTable>
      </el-tab-pane>
    </el-tabs>
  </Drawer>

  <SoaAssetPicker ref="assetPickerRef" @select="onAssetPicked" />
</template>
