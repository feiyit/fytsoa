<script setup lang="ts">
import { useSoaModal } from "@/component/soaModal/index.vue";
import {
  addAmAssetCategoryProfile,
  updateAmAssetCategoryProfile,
  fetchAmAssetCategoryProfileById,
} from "@/api/am/assetCategoryProfile";
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
  depMethod: 0,
  depLifeMonths: 0,
  salvageRate: 0,
  extJson: "{}",
});

const rules = {
  categoryId: [{ required: true, message: "请选择分类", trigger: "change" }],
};

const title = ref("新增配置");
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
  title.value = id !== "0" ? "编辑配置" : "新增配置";
  if (id !== "0") {
    const res = await fetchAmAssetCategoryProfileById(id);
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
        await addAmAssetCategoryProfile(formData);
        ElMessage.success("新增成功");
      } else {
        await updateAmAssetCategoryProfile(formData);
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
    depMethod: 0,
    depLifeMonths: 0,
    salvageRate: 0,
    extJson: "{}",
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
          <el-form-item label="折旧方法">
            <el-select
              v-model="formData.depMethod"
              placeholder="请选择折旧方法"
              style="width: 100%"
            >
              <el-option label="不折旧" :value="0" />
              <el-option label="直线法" :value="1" />
              <el-option label="双倍余额" :value="2" />
              <el-option label="年数总和" :value="3" />
            </el-select>
          </el-form-item>
        </el-col>
        <el-col :span="12">
          <el-form-item label="折旧期(月)">
            <el-input-number
              v-model="formData.depLifeMonths"
              :min="0"
              style="width: 100%"
            />
          </el-form-item>
        </el-col>
        <el-col :span="12">
          <el-form-item label="残值率(%)">
            <el-input-number
              v-model="formData.salvageRate"
              :min="0"
              :precision="2"
              style="width: 100%"
            />
          </el-form-item>
        </el-col>
        <el-col :span="24">
          <el-form-item label="扩展(JSON)">
            <el-input
              v-model="formData.extJson"
              placeholder="请输入扩展JSON"
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
