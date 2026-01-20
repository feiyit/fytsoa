<script lang="ts" setup>
import { useSoaModal } from "@/component/soaModal/index.vue";
import {
  fetchCmsColumnList,
  addCmsColumn,
  updateCmsColumn,
  fetchCmsColumnById,
} from "@/api/cms/column";
import { fetchCmsTemplatePage } from "@/api/cms/template";
import { changeTree } from "@/utils/tools";
import { ElMessage } from "element-plus";
import soaUpload from "@/component/soaUpload/index.vue";
import soaEditor from "@/component/soaEditor/index.vue";
interface ColumnOption {
  id: string;
  value: string;
  label: string;
  parentId: string;
  children?: ColumnOption[];
}

interface TemplateOption {
  label: string;
  value: string;
}

interface FormData {
  id: string;
  parentIdList: any[];
  parentId: string;
  templateId: undefined | string;
  imgUrl: undefined | string;
  status: boolean;
  linkUrl: undefined | string;
  title: undefined | string;
  subTitle: undefined | string;
  number: undefined | string;
  enTitle: undefined | string;
  keyWord: undefined | string;
  summary: undefined | string;
  content: string;
}

const columnOptions = ref<ColumnOption[]>([]);
const templateOptions = ref<TemplateOption[]>([]);
const props = defineProps({
  id: { type: String, default: "0" },
  columnId: { type: String, default: "0" },
});

const defaultFormData: FormData = {
  id: "0",
  parentIdList: [],
  parentId: "0",
  templateId: undefined,
  imgUrl: undefined,
  status: true,
  linkUrl: undefined,
  title: undefined,
  subTitle: undefined,
  number: undefined,
  enTitle: undefined,
  keyWord: undefined,
  summary: undefined,
  content: "",
};

const formData = reactive<FormData>({ ...defaultFormData });
const rules = {
  parentId: [
    {
      required: true,
      message: "请至少选择一个所属上级",
      trigger: "change",
    },
  ],
  templateId: [
    {
      required: true,
      message: "请选择栏目模板",
      trigger: "change",
    },
  ],
  title: [
    {
      required: true,
      message: "请输入栏目名称",
      trigger: "blur",
    },
  ],
};
const formRef = ref();
const saving = ref(false);
const emit = defineEmits(["complete"]);
const [Modal, modalApi] = useSoaModal({
  onConfirm: () => {
    handleSubmit();
  },
  onCancel: () => {
    resetForm();
    modalApi.close();
  },
});
const initColumnTree = async () => {
  const response = await fetchCmsColumnList();
  const tree = response as unknown as any[];
  let _tree: any = [{ id: "1", value: "0", label: "顶级栏目", parentId: "0" }];
  tree.forEach((m: any) => {
    _tree.push({
      id: m.id,
      value: m.id,
      label: m.title,
      parentId: m.parentId,
    });
  });
  columnOptions.value = changeTree(_tree);
};
const initTemplate = async () => {
  const response = await fetchCmsTemplatePage();
  const template = response as unknown as { items: any[] };
  template.items.forEach((m: any) => {
    templateOptions.value.push({ label: m.name, value: m.id });
  });
};
const openModal = async (row: any) => {
  if (row) {
    Object.assign(formData, row);
  }
  initColumnTree();
  modalApi.open();
};
const handleSubmit = () => {
  formRef.value?.validate(async (valid: any) => {
    if (!valid) return;
    saving.value = true;
    try {
      if (formData.id === "0") {
        await addCmsColumn(formData);
        ElMessage.success("新增成功");
      } else {
        await updateCmsColumn(formData);
        ElMessage.success("更新成功");
      }
      emit("complete");
    } finally {
      //resetForm();
      saving.value = false;
    }
  });
};
const resetForm = () => {
  Object.assign(formData, {
    id: "0",
    parentIdList: [],
    parentId: "0",
    templateId: undefined,
    imgUrl: undefined,
    status: true,
    linkUrl: undefined,
    title: undefined,
    subTitle: undefined,
    number: undefined,
    enTitle: undefined,
    keyWord: undefined,
    summary: undefined,
    content: "",
  });
};
const byColumn = async (id: string) => {
  if (id == "0") return;
  initColumnTree();
  const res = await fetchCmsColumnById(id);
  console.log("res", res);
  Object.assign(formData, res);

  // 重置缺失的字段为默认值
  const formKeys: Array<keyof FormData> = [
    "id",
    "parentIdList",
    "parentId",
    "templateId",
    "imgUrl",
    "status",
    "linkUrl",
    "title",
    "subTitle",
    "number",
    "enTitle",
    "keyWord",
    "summary",
    "content",
  ];

  formKeys.forEach((key) => {
    if (!(key in res)) {
      // 使用类型断言来解决TypeScript的类型推断问题
      (formData as any)[key] = defaultFormData[key];
    }
  });
};
watch(
  () => props.columnId,
  (newId) => {
    byColumn(newId);
  },
  { immediate: true }
);
onMounted(async () => {
  await initTemplate();
});
defineExpose({
  openModal,
});
</script>
<template>
  <div
    class="bg-card border-border flex flex-wrap items-center justify-between gap-3 rounded-[.5vw] p-2 pt-4"
  >
    <el-scrollbar height="84vh" style="width: 100%">
      <el-form
        ref="formRef"
        label-width="100px"
        :model="formData"
        :rules="rules"
        class="w-full"
      >
        <el-row :gutter="20" style="margin: 0px">
          <el-col :span="14">
            <el-form-item label="所属上级" prop="parentId">
              <el-tree-select
                v-model="formData.parentId"
                placeholder="请选择所属上级"
                :data="columnOptions"
                collapse-tags
                check-strictly
                default-expand-all
                :style="{ width: '100%' }"
              />
            </el-form-item>
            <el-form-item label="栏目名称" prop="title">
              <el-input
                v-model="formData.title"
                placeholder="请输入栏目名称"
                :maxlength="30"
                show-word-limit
                clearable
                :style="{ width: '100%' }"
              ></el-input>
            </el-form-item>
            <el-form-item label="英文标题" prop="enTitle">
              <el-input
                v-model="formData.enTitle"
                placeholder="请输入英文标题"
                :maxlength="50"
                show-word-limit
                clearable
                :style="{ width: '100%' }"
              ></el-input>
            </el-form-item>
            <el-form-item label="栏目副标题" prop="subTitle">
              <el-input
                v-model="formData.subTitle"
                placeholder="请输入栏目副标题"
                :maxlength="50"
                show-word-limit
                clearable
                :style="{ width: '100%' }"
              ></el-input>
            </el-form-item>
            <el-form-item label="Seo关键字" prop="keyWord">
              <el-input
                v-model="formData.keyWord"
                type="textarea"
                placeholder="请输入Seo关键字"
                :maxlength="100"
                :autosize="{ minRows: 2, maxRows: 3 }"
                :style="{ width: '100%' }"
              ></el-input>
            </el-form-item>
            <el-form-item label="Seo描述" prop="summary">
              <el-input
                v-model="formData.summary"
                type="textarea"
                placeholder="请输入Seo描述"
                :maxlength="100"
                :autosize="{ minRows: 2, maxRows: 3 }"
                :style="{ width: '100%' }"
              ></el-input>
            </el-form-item>
          </el-col>
          <el-col :span="10">
            <el-form-item label="栏目模板" prop="templateId">
              <el-select
                v-model="formData.templateId"
                placeholder="请选择栏目模板"
                clearable
                :style="{ width: '100%' }"
              >
                <el-option
                  v-for="(item, index) in templateOptions"
                  :key="index"
                  :label="item.label"
                  :value="item.value"
                ></el-option>
              </el-select>
            </el-form-item>
            <el-form-item label="上传" prop="imgUrl">
              <soa-upload
                v-model="formData.imgUrl"
                action="/sysfile/upload?path=/upload/column/"
                type="image"
                :file-size="5"
                :limit="1"
              />
            </el-form-item>
            <el-form-item label="栏目状态" prop="status">
              <el-switch v-model="formData.status"></el-switch>
            </el-form-item>
            <el-form-item label="外链地址" prop="linkUrl">
              <el-input
                v-model="formData.linkUrl"
                placeholder="请输入外链地址"
                clearable
                :style="{ width: '100%' }"
              >
                <template #prepend>Http://</template>
              </el-input>
            </el-form-item>
          </el-col>
        </el-row>
      </el-form>
      <div class="w-full p-2">
        <soaEditor
          v-model="formData.content"
          placeholder="请输入"
          :height="400"
        />
      </div>
      <div class="p-2 text-center">
        <el-button class="w-[100px]" type="primary" @click="handleSubmit"
          >保存</el-button
        >
      </div>
    </el-scrollbar>
  </div>
</template>
