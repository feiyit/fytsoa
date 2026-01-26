<script setup lang="ts">
import { useSoaDrawer } from "@/component/soaDrawer/index.vue";
import {
  addAmInventoryPlan,
  updateAmInventoryPlan,
  fetchAmInventoryPlanById,
  scanAmInventoryItem,
} from "@/api/am/inventoryPlan";
import { generateCode } from "@/utils/tools";
import soaFormTable from "@/component/soaFormTable/index.vue";
import SoaAssetPicker from "@/component/soaAssetPicker/index.vue";
import { fetchAdminList } from "@/api";
import { fetchAmLocationList } from "@/api/am/location";

const formRef = ref();
const saving = ref(false);
const emit = defineEmits(["complete"]);

const activeTab = ref("basic");
const title = ref("新增盘点");

// 盘点编号前缀（可统一调整）
const INVENTORY_PLAN_NO_PREFIX = "PD-";

const assetPickerRef = ref<any>(null);
const pickingTargetRow = ref<any>(null);
const itemsTableRef = ref<any>(null);

const adminOptions = ref<any[]>([]);
const locationOptions = ref<any[]>([]);
// 基础信息：范围/执行人改为可视化选择（提交时转换为 JSON 字符串）
const scopeLocationIds = ref<string[]>([]);
const assigneeIds = ref<string[]>([]);

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

const itemTemplate = {
  id: "0",
  planId: "0",
  assetId: "0",
  assetNo: "",
  name: "",
  tagCode: "",
  model: "",
  expectedLocationId: undefined,
  actualLocationId: undefined,
  expectedCustodianId: undefined,
  actualCustodianId: undefined,
  result: 0,
  scanTime: undefined,
  scanUserId: undefined,
  remark: "",
  assetObj: undefined as any,
};

const rules = {
  planNo: [{ required: true, message: "请输入盘点编号", trigger: "blur" }],
  name: [{ required: true, message: "请输入盘点名称", trigger: "blur" }],
};

const [Drawer, drawerApi] = useSoaDrawer({
  onConfirm: () => handleSubmit(),
  onCancel: () => resetForm(),
});

const loadAdminOptions = async () => {
  // 仅用于下拉选择展示；如需更大数据量可改为远程搜索。
  const res: any = await fetchAdminList({ page: 1, limit: 1000 });
  adminOptions.value = Array.isArray(res) ? res : res?.items || [];
};

const loadLocationOptions = async () => {
  const res: any = await fetchAmLocationList({ status: "1" });
  locationOptions.value = Array.isArray(res) ? res : res?.items || [];
};

const openModal = async (row?: any) => {
  resetForm();
  await Promise.all([loadAdminOptions(), loadLocationOptions()]);
  const id = row?.id ? String(row.id) : "0";
  title.value = id !== "0" ? "编辑盘点" : "新增盘点";
  activeTab.value = "basic";
  if (id !== "0") {
    const res = await fetchAmInventoryPlanById(id);
    Object.assign(formData, res || {});

    // 基础信息回显：把 JSON 字符串转换为可视化选择的数据结构
    try {
      const v = formData.scopeJson ? JSON.parse(formData.scopeJson) : [];
      const arr = Array.isArray(v) ? v : Array.isArray(v?.locationIds) ? v.locationIds : [];
      scopeLocationIds.value = arr.map((x: any) => String(x));
    } catch {
      scopeLocationIds.value = [];
    }
    try {
      const v = formData.assigneeIdsJson ? JSON.parse(formData.assigneeIdsJson) : [];
      assigneeIds.value = Array.isArray(v) ? v.map((x: any) => String(x)) : [];
    } catch {
      assigneeIds.value = [];
    }

    if (!Array.isArray(formData.items)) formData.items = [];
    // 详情返回 assetObj 时，把常用字段同步到行上，便于前端显示/编辑
    (formData.items || []).forEach((it: any) => {
      const a = it.assetObj;
      // 明细中的各类 Id 字段统一转成 string，便于 el-select / el-input 绑定
      it.actualLocationId =
        it.actualLocationId && String(it.actualLocationId) !== "0"
          ? String(it.actualLocationId)
          : undefined;
      it.actualCustodianId =
        it.actualCustodianId && String(it.actualCustodianId) !== "0"
          ? String(it.actualCustodianId)
          : undefined;
      it.scanUserId =
        it.scanUserId && String(it.scanUserId) !== "0"
          ? String(it.scanUserId)
          : undefined;

      // 回显字段：优先明细字段，其次 assetObj 字段
      if (a) {
        it.assetNo = it.assetNo || a.assetNo || "";
        it.name = it.name || a.name || "";
        it.tagCode = it.tagCode || a.tagCode || "";
        it.model = it.model || a.model || "";

        it.expectedLocationId =
          it.expectedLocationId && String(it.expectedLocationId) !== "0"
            ? String(it.expectedLocationId)
            : a.locationId
              ? String(a.locationId)
              : undefined;
        it.expectedCustodianId =
          it.expectedCustodianId && String(it.expectedCustodianId) !== "0"
            ? String(it.expectedCustodianId)
            : a.custodianId
              ? String(a.custodianId)
              : undefined;
      } else {
        it.expectedLocationId = it.expectedLocationId
          ? String(it.expectedLocationId)
          : undefined;
        it.expectedCustodianId = it.expectedCustodianId
          ? String(it.expectedCustodianId)
          : undefined;
      }

      // 盘点人/盘点责任人保留一项：两者互相兜底（优先盘点责任人）
      if (!it.actualCustodianId) it.actualCustodianId = it.scanUserId;
      if (!it.scanUserId) it.scanUserId = it.actualCustodianId;

      // 盘点地点兜底：如未选择则默认等于系统地点
      if (!it.actualLocationId) it.actualLocationId = it.expectedLocationId;
    });
  } else if (row) {
    Object.assign(formData, row);
  }
  drawerApi.open();
};

const generateInventoryPlanNo = () => {
  formData.planNo = generateCode(INVENTORY_PLAN_NO_PREFIX, 8);
};

const addEmptyItemRow = () => {
  itemsTableRef.value?.rowAdd?.();
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
  row.assetNo = asset.assetNo || row.assetNo || "";
  row.name = asset.name || row.name || "";
  row.tagCode = asset.tagCode || row.tagCode || "";
  row.model = asset.model || row.model || "";

  // 默认带出系统地点/责任人（按资产台账）
  row.expectedLocationId =
    asset.locationId && String(asset.locationId) !== "0"
      ? String(asset.locationId)
      : undefined;
  row.expectedCustodianId =
    asset.custodianId && String(asset.custodianId) !== "0"
      ? String(asset.custodianId)
      : undefined;

  // 默认把盘点值先等于系统值（用户可再手工修改）
  if (!row.actualLocationId || row.actualLocationId === "0")
    row.actualLocationId = row.expectedLocationId;
  if (!row.actualCustodianId || row.actualCustodianId === "0")
    row.actualCustodianId = row.expectedCustodianId;
  // 简化明细：盘点人/盘点责任人保留一项时，scanUserId 与 actualCustodianId 统一取同一值
  if (!row.scanUserId || row.scanUserId === "0")
    row.scanUserId = row.actualCustodianId;

  // 回显用
  row.assetObj = asset;
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
    const item =
      itemsTableRef.value?.rowAdd?.() ??
      JSON.parse(JSON.stringify(itemTemplate));
    applyAssetToRow(item, a);
    exists.add(assetId);
  });
};

const handleScan = async (row: any) => {
  if (!row.id || row.id === "0") {
    ElMessage.warning("该明细尚未保存（没有 Id），请先保存盘点计划。");
    return;
  }
  const scanUserId =
    row.scanUserId || row.actualCustodianId || row.expectedCustodianId || "0";
  const actualCustodianId = row.actualCustodianId || scanUserId || "0";
  const expectedCustodianId = row.expectedCustodianId || "0";
  const expectedLocationId = row.expectedLocationId || "0";
  const actualLocationId =
    row.actualLocationId || row.expectedLocationId || "0";

  await scanAmInventoryItem({
    ...row,
    scanUserId,
    actualCustodianId,
    expectedCustodianId,
    expectedLocationId,
    actualLocationId,
    scanTime: row.scanTime || new Date(),
  });
  ElMessage.success("扫码结果已提交");
};

const handleSubmit = () => {
  if (saving.value) return;
  formRef.value?.validate(async (valid: any) => {
    if (!valid) return;
    saving.value = true;
    try {
      // 不直接改写表格行，避免 UI 出现默认 0；仅在提交 payload 时兜底补齐必填字段
      const submitData: any = {
        ...formData,
        // 未选择时提交 "[]"
        scopeJson: JSON.stringify(scopeLocationIds.value || []),
        assigneeIdsJson: JSON.stringify(assigneeIds.value || []),
        items: (formData.items || []).map((it: any) => {
          const scanUserId =
            it.scanUserId ||
            it.actualCustodianId ||
            it.expectedCustodianId ||
            "0";
          const actualCustodianId = it.actualCustodianId || scanUserId || "0";
          const expectedCustodianId = it.expectedCustodianId || "0";
          const expectedLocationId = it.expectedLocationId || "0";
          const actualLocationId = it.actualLocationId || expectedLocationId;

          return {
            ...it,
            scanUserId,
            actualCustodianId,
            expectedCustodianId,
            expectedLocationId,
            actualLocationId,
          };
        }),
      };

      if (!formData.id || formData.id === "0") {
        await addAmInventoryPlan(submitData);
        ElMessage.success("新增成功");
      } else {
        await updateAmInventoryPlan(submitData);
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
  scopeLocationIds.value = [];
  assigneeIds.value = [];
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
              <el-form-item label="盘点编号" prop="planNo">
                <el-input
                  v-model="formData.planNo"
                  placeholder="请输入盘点编号"
                  clearable
                  maxlength="32"
                >
                  <template #append>
                    <el-button @click="generateInventoryPlanNo"
                      >自动生成</el-button
                    >
                  </template>
                </el-input>
              </el-form-item>
            </el-col>
            <el-col :span="12">
              <el-form-item label="盘点名称" prop="name">
                <el-input
                  v-model="formData.name"
                  placeholder="请输入盘点名称"
                  clearable
                  maxlength="100"
                  show-word-limit
                />
              </el-form-item>
            </el-col>
            <el-col :span="12">
              <el-form-item label="状态">
                <el-select v-model="formData.status" style="width: 100%">
                  <el-option label="草稿" :value="0" />
                  <el-option label="进行中" :value="1" />
                  <el-option label="已完成" :value="2" />
                  <el-option label="已取消" :value="3" />
                </el-select>
              </el-form-item>
            </el-col>

            <el-col :span="12">
              <el-form-item label="开始时间">
                <el-date-picker
                  v-model="formData.startTime"
                  type="datetime"
                  value-format="YYYY-MM-DD HH:mm:ss"
                  format="YYYY年MM月DD日 HH:mm:ss"
                  style="width: 100%"
                />
              </el-form-item>
            </el-col>
            <el-col :span="12">
              <el-form-item label="结束时间">
                <el-date-picker
                  v-model="formData.endTime"
                  type="datetime"
                  value-format="YYYY-MM-DD HH:mm:ss"
                  format="YYYY年MM月DD日 HH:mm:ss"
                  style="width: 100%"
                />
              </el-form-item>
            </el-col>

            <el-col :span="24">
              <el-form-item label="范围(地点)">
                <el-select
                  v-model="scopeLocationIds"
                  placeholder="请选择地点范围（可多选），不选则为 []"
                  multiple
                  filterable
                  clearable
                  collapse-tags
                  collapse-tags-tooltip
                  style="width: 100%"
                >
                  <el-option
                    v-for="it in locationOptions"
                    :key="it.id"
                    :label="it.name || it.id"
                    :value="String(it.id)"
                  />
                </el-select>
              </el-form-item>
            </el-col>

            <el-col :span="24">
              <el-form-item label="执行人">
                <el-select
                  v-model="assigneeIds"
                  placeholder="请选择执行人（可多选），不选则为 []"
                  multiple
                  filterable
                  clearable
                  collapse-tags
                  collapse-tags-tooltip
                  style="width: 100%"
                >
                  <el-option
                    v-for="it in adminOptions"
                    :key="it.id"
                    :label="it.fullName || it.displayName || it.loginAccount || it.id"
                    :value="String(it.id)"
                  />
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
      </el-tab-pane>

      <el-tab-pane label="盘点明细" name="items">
        <div class="mb-2 flex justify-between">
          <div class="flex items-center gap-2">
            <el-button
              type="primary"
              @click="openAssetPicker({ multiple: true })"
              >选择资产</el-button
            >
            <el-button @click="addEmptyItemRow">新增空行</el-button>
          </div>
          <div class="text-sm text-gray-500">
            提示：保存后可对单条明细执行“提交扫码结果”。
          </div>
        </div>
        <soaFormTable
          ref="itemsTableRef"
          v-model="formData.items"
          :addTemplate="itemTemplate"
          height="520"
        >
          <!-- 明细Id：通常仅用于调试/排查问题；不影响业务操作（提交扫码/保存仍会携带 row.id） -->

          <el-table-column label="资产编号" width="220">
            <template #default="{ row }">
              <el-input
                v-model="row.assetNo"
                :placeholder="
                  row.assetObj?.assetNo ? row.assetObj.assetNo : '请选择资产'
                "
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

          <el-table-column label="资产名称" min-width="220">
            <template #default="{ row }">
              <el-input
                v-model="row.name"
                :placeholder="
                  row.assetObj?.name ? row.assetObj.name : '自动带出'
                "
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
                :placeholder="
                  row.assetObj?.tagCode ? row.assetObj.tagCode : '自动带出'
                "
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
                :placeholder="
                  row.assetObj?.model ? row.assetObj.model : '自动带出'
                "
                clearable
                maxlength="200"
                show-word-limit
              />
            </template>
          </el-table-column>

          <el-table-column label="系统地点" width="170">
            <template #default="{ row }">
              <el-select
                v-model="row.expectedLocationId"
                placeholder="自动带出"
                filterable
                clearable
                disabled
                style="width: 100%"
              >
                <el-option
                  v-for="it in locationOptions"
                  :key="it.id"
                  :label="it.name || it.id"
                  :value="String(it.id)"
                />
              </el-select>
            </template>
          </el-table-column>

          <el-table-column label="盘点地点" width="170">
            <template #default="{ row }">
              <el-select
                v-model="row.actualLocationId"
                placeholder="请选择盘点地点"
                filterable
                clearable
                style="width: 100%"
              >
                <el-option
                  v-for="it in locationOptions"
                  :key="it.id"
                  :label="it.name || it.id"
                  :value="String(it.id)"
                />
              </el-select>
            </template>
          </el-table-column>

          <el-table-column label="系统责任人" width="170">
            <template #default="{ row }">
              <el-select
                v-model="row.expectedCustodianId"
                placeholder="请选择"
                filterable
                clearable
                style="width: 100%"
              >
                <el-option
                  v-for="it in adminOptions"
                  :key="it.id"
                  :label="
                    it.fullName || it.displayName || it.loginAccount || it.id
                  "
                  :value="String(it.id)"
                />
              </el-select>
            </template>
          </el-table-column>

          <el-table-column label="盘点人员" width="170">
            <template #default="{ row }">
              <el-select
                v-model="row.actualCustodianId"
                placeholder="请选择人员"
                filterable
                clearable
                style="width: 100%"
                @change="(v) => (row.scanUserId = v ? String(v) : undefined)"
              >
                <el-option
                  v-for="it in adminOptions"
                  :key="it.id"
                  :label="
                    it.fullName || it.displayName || it.loginAccount || it.id
                  "
                  :value="String(it.id)"
                />
              </el-select>
            </template>
          </el-table-column>

          <el-table-column label="结果" width="140">
            <template #default="{ row }">
              <el-select v-model="row.result" style="width: 100%">
                <el-option label="正常" :value="0" />
                <el-option label="未盘到" :value="1" />
                <el-option label="盘盈" :value="2" />
                <el-option label="盘亏" :value="3" />
                <el-option label="地点不符" :value="4" />
                <el-option label="责任人不符" :value="5" />
              </el-select>
            </template>
          </el-table-column>

          <!-- 盘点人Id 与盘点责任人保留一项：已与盘点责任人合并（ScanUserId 自动同步） -->

          <el-table-column label="盘点时间" width="190">
            <template #default="{ row }">
              <el-date-picker
                v-model="row.scanTime"
                type="datetime"
                value-format="YYYY-MM-DD HH:mm:ss"
                format="YYYY年MM月DD日 HH:mm:ss"
                style="width: 100%"
              />
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

          <el-table-column label="操作" width="110" fixed="right">
            <template #default="{ row }">
              <el-button link type="primary" @click="handleScan(row)"
                >提交扫码</el-button
              >
            </template>
          </el-table-column>
        </soaFormTable>
      </el-tab-pane>
    </el-tabs>
  </Drawer>

  <SoaAssetPicker ref="assetPickerRef" @select="onAssetPicked" />
</template>
