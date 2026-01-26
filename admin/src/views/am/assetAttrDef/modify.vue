<script setup lang="ts">
import { useSoaModal } from "@/component/soaModal/index.vue";
import {
  addAmAssetAttrDef,
  updateAmAssetAttrDef,
  fetchAmAssetAttrDefById,
} from "@/api/am/assetAttrDef";
import { fetchSysCodeList } from "@/api/sys/code";

const formRef = ref();
const saving = ref(false);
const emit = defineEmits(["complete"]);

const categoryOptions = ref<any[]>([]);

// sys_codetype 中 AM_ASSET_CATEGORY 对应的 Id（你给定的常量）
const AM_ASSET_CATEGORY_TYPE_ID = "2013975685776936960";

const formData = reactive<any>({
  id: "0",
  tenantId: "0",
  categoryId: undefined,
  fieldKey: "",
  fieldName: "",
  dataType: "string",
  optionsJson: "{}",
  isRequired: false,
  isEnabled: true,
  sort: 0,
});

const rules = {
  categoryId: [{ required: true, message: "请选择分类", trigger: "change" }],
  fieldKey: [{ required: true, message: "请输入字段Key", trigger: "blur" }],
  fieldName: [{ required: true, message: "请输入字段名称", trigger: "blur" }],
};

const title = ref("新增字段");
const [Modal, modalApi] = useSoaModal({
  onConfirm: () => handleSubmit(),
  onCancel: () => resetForm(),
});

const loadCategories = async () => {
  const codes = await fetchSysCodeList({
    id: AM_ASSET_CATEGORY_TYPE_ID,
    status: "1",
  });
  categoryOptions.value = (codes || []).map((m: any) => ({
    label: m.name,
    value: m.id,
  }));
};

onMounted(() => {
  loadCategories();
});

const openModal = async (row?: any) => {
  resetForm();
  await loadCategories();
  const id = row?.id ? String(row.id) : "0";
  title.value = id !== "0" ? "编辑字段" : "新增字段";
  if (id !== "0") {
    const res = await fetchAmAssetAttrDefById(id);
    Object.assign(formData, res || {});
  } else if (row) {
    Object.assign(formData, row);
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
        await addAmAssetAttrDef(formData);
        ElMessage.success("新增成功");
      } else {
        await updateAmAssetAttrDef(formData);
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
    categoryId: undefined,
    fieldKey: "",
    fieldName: "",
    dataType: "string",
    optionsJson: "{}",
    isRequired: false,
    isEnabled: true,
    sort: 0,
  });
};

defineExpose({ openModal });
</script>

<template>
  <Modal :title="title" class="w-[900px]" draggable :closeOnClickModal="false">
    <el-form ref="formRef" :model="formData" :rules="rules" label-width="110px">
      <el-row :gutter="12">
        <el-col :span="24">
          <el-form-item label="分类" prop="categoryId">
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
          <el-form-item label="字段Key" prop="fieldKey">
            <el-input
              v-model="formData.fieldKey"
              placeholder="请输入字段Key"
              clearable
              maxlength="64"
              show-word-limit
            />
          </el-form-item>
        </el-col>
        <el-col :span="12">
          <el-form-item label="字段名称" prop="fieldName">
            <el-input
              v-model="formData.fieldName"
              placeholder="请输入字段名称"
              clearable
              maxlength="100"
              show-word-limit
            />
          </el-form-item>
        </el-col>

        <el-col :span="12">
          <el-form-item label="数据类型">
            <el-select v-model="formData.dataType" style="width: 100%">
              <el-option label="字符串" value="string" />
              <el-option label="数字" value="number" />
              <el-option label="日期" value="date" />
              <el-option label="布尔" value="bool" />
              <el-option label="单选" value="select" />
              <el-option label="多选" value="multi-select" />
            </el-select>
          </el-form-item>
        </el-col>
        <el-col :span="12">
          <el-form-item label="排序">
            <el-input-number
              v-model="formData.sort"
              :min="0"
              style="width: 100%"
            />
          </el-form-item>
        </el-col>

        <el-col :span="12">
          <el-form-item label="必填">
            <el-switch v-model="formData.isRequired" />
          </el-form-item>
        </el-col>
        <el-col :span="12">
          <el-form-item label="启用">
            <el-switch v-model="formData.isEnabled" />
          </el-form-item>
        </el-col>

        <el-col :span="24">
          <el-form-item label="选项(JSON)">
            <el-input
              v-model="formData.optionsJson"
              placeholder="请输入选项JSON"
              type="textarea"
              :rows="3"
              maxlength="2000"
              show-word-limit
            />
          </el-form-item>
        </el-col>
      </el-row>
    </el-form>
  </Modal>
</template>
