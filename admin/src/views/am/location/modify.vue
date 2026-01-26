<script setup lang="ts">
import { useSoaModal } from "@/component/soaModal/index.vue";
import {
  addAmLocation,
  updateAmLocation,
  fetchAmLocationById,
  fetchAmLocationList,
} from "@/api/am/location";
import { changeTree, generateCode } from "@/utils/tools";

const formRef = ref();
const saving = ref(false);
const emit = defineEmits(["complete"]);

const parentOptions = ref<any[]>([]);
// 地点类型：用于标识地点层级/用途（例如园区/楼栋/楼层/房间），便于后续筛选、联动、规则配置等。
// 后端目前是 string，前端做成下拉 + 允许自定义，避免写错也保留扩展性。
const typeOptions = [
  { label: "园区 (park)", value: "park" },
  { label: "楼栋 (building)", value: "building" },
  { label: "楼层 (floor)", value: "floor" },
  { label: "房间 (room)", value: "room" },
];

// 地点编码前缀（可统一调整）
const LOCATION_CODE_PREFIX = "DD-";

const formData = reactive<any>({
  id: "0",
  tenantId: "0",
  parentId: "0",
  code: "",
  name: "",
  type: "",
  sort: 0,
  status: true,
  summary: "",
});

const rules = {
  name: [{ required: true, message: "请输入地点名称", trigger: "blur" }],
};

const title = ref("新增地点");
const [Modal, modalApi] = useSoaModal({
  onConfirm: () => handleSubmit(),
  onCancel: () => resetForm(),
});

const loadParents = async () => {
  const list = await fetchAmLocationList({ status: "1" });
  const treeSrc = (list || []).map((m: any) => ({
    id: String(m.id),
    value: String(m.id),
    label: m.name,
    parentId: String(m.parentId ?? "0"),
  }));
  const tree = changeTree(treeSrc);
  parentOptions.value = [
    { id: "0", value: "0", label: "根节点", children: tree },
  ];
};

onMounted(() => {
  //loadParents();
});

const openModal = async (row?: any) => {
  resetForm();
  await loadParents();
  const id = row?.id ? String(row.id) : "0";
  title.value = id !== "0" ? "编辑地点" : "新增地点";
  if (id !== "0") {
    const res = await fetchAmLocationById(id);
    Object.assign(formData, res || {});
  } else if (row) {
    Object.assign(formData, row);
  }
  modalApi.open();
};

const generateLocationCode = () => {
  formData.code = generateCode(LOCATION_CODE_PREFIX, 8);
};

const handleSubmit = () => {
  if (saving.value) return;
  formRef.value?.validate(async (valid: any) => {
    if (!valid) return;
    saving.value = true;
    try {
      if (!formData.id || formData.id === "0") {
        await addAmLocation(formData);
        ElMessage.success("新增成功");
      } else {
        await updateAmLocation(formData);
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
    parentId: "0",
    code: "",
    name: "",
    type: "",
    sort: 0,
    status: true,
    summary: "",
  });
};

defineExpose({ openModal });
</script>

<template>
  <Modal :title="title" class="w-[850px]" draggable :closeOnClickModal="false">
    <el-form ref="formRef" :model="formData" :rules="rules" label-width="110px">
      <el-row :gutter="12">
        <el-col :span="24">
          <el-form-item label="上级地点">
            <el-tree-select
              v-model="formData.parentId"
              :data="parentOptions"
              placeholder="请选择上级"
              default-expand-all
              check-strictly
              :style="{ width: '100%' }"
            />
          </el-form-item>
        </el-col>
        <el-col :span="12">
          <el-form-item label="编码">
            <el-input
              v-model="formData.code"
              placeholder="请输入编码"
              clearable
              maxlength="32"
            >
              <template #append>
                <el-button @click="generateLocationCode">自动生成</el-button>
              </template>
            </el-input>
          </el-form-item>
        </el-col>
        <el-col :span="12">
          <el-form-item label="名称" prop="name">
            <el-input
              v-model="formData.name"
              placeholder="请输入地点名称"
              clearable
              maxlength="100"
              show-word-limit
            />
          </el-form-item>
        </el-col>
        <el-col :span="12">
          <el-form-item label="类型">
            <el-select
              v-model="formData.type"
              placeholder="请选择类型"
              clearable
              filterable
              allow-create
              default-first-option
              style="width: 100%"
            >
              <el-option
                v-for="it in typeOptions"
                :key="it.value"
                :label="it.label"
                :value="it.value"
              />
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
          <el-form-item label="状态">
            <el-switch v-model="formData.status" />
          </el-form-item>
        </el-col>
        <el-col :span="24">
          <el-form-item label="备注">
            <el-input
              v-model="formData.summary"
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
  </Modal>
</template>
