<script setup lang="ts">
import { useSoaModal } from "@/component/soaModal/index.vue";
import SoaSelectAdmin from "@/component/soaSelectAdmin/index.vue";
import {
  addAmReminderRule,
  updateAmReminderRule,
  fetchAmReminderRuleById,
} from "@/api/am/reminderRule";
import { fetchAdminById } from "@/api/sys/admin";

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

const formData = reactive<any>({
  id: "0",
  tenantId: "0",
  ruleType: "",
  isEnabled: true,
  daysBefore: 0,
  configJson: "",
});

type ConfigMode = "VISUAL" | "JSON";
const configMode = ref<ConfigMode>("VISUAL");

// 可视化配置（v1）
const receiverUsers = ref<any[]>([]);
const channels = ref<string[]>(["IN_APP"]);
const template = ref<string>("");

const safeParse = (v: any) => {
  try {
    if (!v) return null;
    return typeof v === "string" ? JSON.parse(v) : v;
  } catch {
    return null;
  }
};

const buildConfig = () => {
  return {
    v: 1,
    receiverUserIds: (receiverUsers.value || [])
      .map((u: any) => String(u?.id ?? ""))
      .filter((x) => x && x !== "0"),
    channels: channels.value || [],
    template: template.value || "",
  };
};

const hydrateConfig = (cfgRaw: any) => {
  const cfg = safeParse(cfgRaw);
  if (!cfgRaw) {
    configMode.value = "VISUAL";
    receiverUsers.value = [];
    channels.value = ["IN_APP"];
    template.value = "";
    return;
  }
  if (!cfg || typeof cfg !== "object") {
    // 非法JSON：保留原文，切到 JSON 模式避免覆盖
    configMode.value = "JSON";
    receiverUsers.value = [];
    channels.value = ["IN_APP"];
    template.value = "";
    return;
  }

  // 仅当结构符合 v1 时才回显，否则还是走 JSON 模式
  const v = Number((cfg as any).v || 0);
  if (v !== 1) {
    configMode.value = "JSON";
    receiverUsers.value = [];
    channels.value = ["IN_APP"];
    template.value = "";
    return;
  }

  configMode.value = "VISUAL";
  // receiverUserIds -> 回显到 SoaSelectAdmin（需要对象）
  const ids = Array.isArray((cfg as any).receiverUserIds)
    ? (cfg as any).receiverUserIds.map((x: any) => String(x))
    : [];
  receiverUsers.value = [];
  if (ids.length) {
    // 不阻塞弹窗打开；异步回显
    Promise.all(
      ids.map(async (id: string) => {
        try {
          return await fetchAdminById(Number(id));
        } catch {
          return null;
        }
      }),
    ).then((rows) => {
      receiverUsers.value = (rows || []).filter(Boolean) as any[];
    });
  }
  channels.value = Array.isArray((cfg as any).channels)
    ? (cfg as any).channels
    : ["IN_APP"];
  template.value = String((cfg as any).template ?? "");
};

const configPreview = computed(() => {
  try {
    return JSON.stringify(buildConfig(), null, 2);
  } catch {
    return "";
  }
});

const rules = {
  ruleType: [{ required: true, message: "请选择规则类型", trigger: "change" }],
};

const title = ref("新增规则");
const [Modal, modalApi] = useSoaModal({
  onConfirm: () => handleSubmit(),
  onCancel: () => resetForm(),
});

const openModal = async (row?: any) => {
  resetForm();
  const id = row?.id ? String(row.id) : "0";
  title.value = id !== "0" ? "编辑规则" : "新增规则";
  if (id !== "0") {
    const res = await fetchAmReminderRuleById(id);
    Object.assign(formData, res || {});
  } else if (row) {
    Object.assign(formData, row);
  }
  hydrateConfig(formData.configJson);
  modalApi.open();
};

const handleSubmit = () => {
  if (saving.value) return;
  formRef.value?.validate(async (valid: any) => {
    if (!valid) return;
    saving.value = true;
    try {
      // 可视化模式：生成 configJson
      if (configMode.value === "VISUAL") {
        formData.configJson = JSON.stringify(buildConfig());
      }
      if (!formData.id || formData.id === "0") {
        await addAmReminderRule(formData);
        ElMessage.success("新增成功");
      } else {
        await updateAmReminderRule(formData);
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
    ruleType: "",
    isEnabled: true,
    daysBefore: 0,
    configJson: "",
  });
  configMode.value = "VISUAL";
  receiverUsers.value = [];
  channels.value = ["IN_APP"];
  template.value = "";
};

defineExpose({ openModal });
</script>

<template>
  <Modal :title="title" size="50%" draggable :closeOnClickModal="false">
    <el-form ref="formRef" :model="formData" :rules="rules" label-width="110px">
      <el-row :gutter="12">
        <el-col :span="24">
          <el-form-item label="规则类型" prop="ruleType">
            <el-select
              v-model="formData.ruleType"
              placeholder="请选择规则类型"
              clearable
              filterable
              allow-create
              default-first-option
              style="width: 100%"
            >
              <el-option
                v-for="it in ruleTypeOptions"
                :key="it.value"
                :label="it.label"
                :value="it.value"
              />
            </el-select>
          </el-form-item>
        </el-col>
        <el-col :span="12">
          <el-form-item label="启用">
            <el-switch
              v-model="formData.isEnabled"
              inline-prompt
              active-text="是"
              inactive-text="否"
            />
          </el-form-item>
        </el-col>
        <el-col :span="12">
          <el-form-item label="提前天数">
            <el-input-number
              v-model="formData.daysBefore"
              :min="0"
              style="width: 100%"
            />
          </el-form-item>
        </el-col>
        <el-col :span="24">
          <el-form-item label="配置">
            <div class="w-full">
              <el-radio-group v-model="configMode" class="mb-2">
                <el-radio-button label="VISUAL">可视化</el-radio-button>
                <el-radio-button label="JSON">高级(JSON)</el-radio-button>
              </el-radio-group>

              <template v-if="configMode === 'VISUAL'">
                <el-row :gutter="12">
                  <el-col :span="24">
                    <div class="mb-1 text-xs text-slate-500">
                      接收人（可选）：若不选择，后续可由后端按业务规则决定默认接收人（如资产责任人/管理员）。
                    </div>
                    <SoaSelectAdmin
                      v-model="receiverUsers"
                      :multiple="true"
                      width="100%"
                    />
                  </el-col>

                  <el-col :span="24" class="mt-3">
                    <div class="mb-1 text-xs text-slate-500">
                      通知渠道（示例）
                    </div>
                    <el-select
                      v-model="channels"
                      multiple
                      clearable
                      collapse-tags
                      collapse-tags-tooltip
                      placeholder="请选择通知渠道"
                      style="width: 100%"
                    >
                      <el-option label="站内通知(IN_APP)" value="IN_APP" />
                      <el-option label="邮件(EMAIL)" value="EMAIL" />
                      <el-option label="短信(SMS)" value="SMS" />
                    </el-select>
                  </el-col>

                  <el-col :span="24" class="mt-3">
                    <div class="mb-1 text-xs text-slate-500">
                      模板/补充说明（可选）
                    </div>
                    <el-input
                      v-model="template"
                      placeholder="例如：{assetNo} 将在 {daysBefore} 天后到期"
                      clearable
                      maxlength="200"
                      show-word-limit
                    />
                  </el-col>

                  <el-col :span="24" class="mt-3">
                    <div class="mb-1 text-xs text-slate-500">
                      配置预览（自动生成）
                    </div>
                    <el-input
                      type="textarea"
                      :rows="6"
                      :model-value="configPreview"
                      readonly
                    />
                  </el-col>
                </el-row>
              </template>

              <template v-else>
                <el-input
                  v-model="formData.configJson"
                  placeholder="请输入配置JSON"
                  type="textarea"
                  :rows="8"
                  maxlength="2000"
                  show-word-limit
                />
              </template>
            </div>
          </el-form-item>
        </el-col>
      </el-row>
    </el-form>
  </Modal>
</template>
