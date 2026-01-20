<script lang="ts" setup>
import { useSoaModal } from "@/component/soaModal/index.vue";
import {
  fetchCmsColumnList,
  addCmsColumn,
  updateCmsColumn,
} from "@/api/cms/column";
import { fetchCmsTemplatePage } from "@/api/cms/template";
import { changeTree } from "@/utils/tools";
import soaUpload from "@/component/soaUpload/index.vue";
import soaEditor from "@/component/soaEditor/index.vue";
const columnOptions = ref([]);
const templateOptions = ref([]);
const formData = reactive({
  id: "0",
  parentIdList: [],
  parentId: "0",
  templateId: undefined,
  imgUrl: "",
  status: true,
  linkUrl: undefined,
  title: undefined,
  subTitle: undefined,
  number: undefined,
  enTitle: undefined,
  keyWord: undefined,
  summary: undefined,
  sort: 0,
  content: "",
});
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
  const tree = await fetchCmsColumnList();
  let _tree: any = [{ id: "1", value: "0", label: "顶级栏目", parentId: "0" }];
  tree.some((m) => {
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
  const template = await fetchCmsTemplatePage();
  template.items.some((m) => {
    templateOptions.value.push({ label: m.name, value: m.id });
  });
};
const openModal = async (row: any, subitem: boolean) => {
  if (row && !subitem) {
    Object.assign(formData, row);
  }
  initColumnTree();
  if (subitem) {
    formData.parentId = row.id;
  }
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
      modalApi.close();
    } finally {
      resetForm();
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
    imgUrl: null,
    status: true,
    linkUrl: undefined,
    title: undefined,
    subTitle: undefined,
    number: undefined,
    enTitle: undefined,
    keyWord: undefined,
    summary: undefined,
    sort: 0,
    content: "",
  });
};
onMounted(async () => {
  await initTemplate();
});
defineExpose({
  openModal,
});
</script>
<template>
  <Modal
    title="栏目管理"
    class="w-[1100px]"
    draggable
    :closeOnClickModal="false"
    :confirmLoading="saving"
  >
    <el-form ref="formRef" label-width="100px" :model="formData" :rules="rules">
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
          <el-form-item label="排序" prop="sort">
            <el-slider
              v-model="formData.sort"
              placeholder="请选择排序"
              clearable
              :max="500"
              :style="{ width: '100%' }"
              show-input
            ></el-slider>
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
    <div class="p-2">
      <soaEditor
        v-model="formData.content"
        placeholder="请输入"
        :height="400"
      />
    </div>
  </Modal>
</template>
