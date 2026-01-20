<script lang="ts" setup>
import { useSoaModal } from "@/component/soaModal/index.vue";
import { addCmsArticle, updateCmsArticle } from "@/api/cms/article";
import { fetchCmsColumnList } from "@/api/cms/column";
import { changeTree } from "@/utils/tools";
import soaUpload from "@/component/soaUpload/index.vue";
import soaEditor from "@/component/soaEditor/index.vue";
const columnOptions = ref([]);
const attrOptions = [
  {
    label: "是否推荐",
    value: 1,
  },
  {
    label: "是否热点",
    value: 2,
  },
  {
    label: "是否滚动",
    value: 3,
  },
  {
    label: "是否评论",
    value: 4,
  },
  {
    label: "是否回收站",
    value: 5,
  },
];
const formData = reactive({
  id: "0",
  title: undefined,
  titleColor: undefined,
  columnId: "",
  columnArr: [],
  subTitle: undefined,
  updateTime: new Date(),
  publishTime: new Date(),
  keyWord: undefined,
  summary: undefined,
  linkUrl: undefined,
  content: "",
  imgUrl: undefined,
  status: true,
  tag: [],
  attr: [],
  author: undefined,
  source: undefined,
  extend: [],
  sort: 0,
  hits: 0,
  dayHits: 0,
  weedHits: 0,
  monthHits: 0,
});
const rules = {
  columnId: [
    {
      required: true,
      message: "请选择栏目",
      trigger: "change",
    },
  ],
  title: [
    {
      required: true,
      message: "请输入文章标题",
      trigger: "blur",
    },
  ],
  publishTime: [
    {
      required: true,
      message: "发布时间不能为空",
      trigger: "change",
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
  let _tree: any = [];
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
const openModal = async (row: any, columnId?: string) => {
  if (row) {
    Object.assign(formData, row);
  }
  if (row && columnId && columnId !== "0") {
    formData.columnId = columnId;
  }
  modalApi.open();
};
const handleSubmit = () => {
  formRef.value?.validate(async (valid: any) => {
    if (!valid) return;
    saving.value = true;
    try {
      if (formData.id === "0") {
        await addCmsArticle(formData);
        ElMessage.success("新增成功");
      } else {
        await updateCmsArticle(formData);
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
    title: undefined,
    titleColor: undefined,
    columnId: "",
    columnArr: [],
    subTitle: undefined,
    updateTime: new Date(),
    publishTime: new Date(),
    keyWord: undefined,
    summary: undefined,
    linkUrl: undefined,
    content: "",
    imgUrl: undefined,
    status: true,
    tag: [],
    attr: [],
    author: undefined,
    source: undefined,
    extend: [],
    sort: 0,
    hits: 0,
    dayHits: 0,
    weedHits: 0,
    monthHits: 0,
  });
};
const extendTemplate = {
  key: "",
  value: "",
};
onMounted(async () => {
  await initColumnTree();
});
defineExpose({
  openModal,
});
</script>
<template>
  <Modal
    title="文章管理"
    class="w-[1100px]"
    draggable
    :closeOnClickModal="false"
    :confirmLoading="saving"
  >
    <el-form ref="formRef" label-width="100px" :model="formData" :rules="rules">
      <el-row :gutter="20" style="margin: 0px">
        <el-col :span="14">
          <el-form-item label="归属栏目" prop="columnId">
            <el-tree-select
              v-model="formData.columnId"
              placeholder="请选择归属栏目"
              :data="columnOptions"
              collapse-tags
              check-strictly
              default-expand-all
              :style="{ width: '100%' }"
            />
          </el-form-item>
          <el-row>
            <el-col :span="22">
              <el-form-item label="文章标题" prop="title">
                <el-input
                  v-model="formData.title"
                  placeholder="请输入文章标题"
                  :maxlength="50"
                  show-word-limit
                  clearable
                  :style="{ width: '100%' }"
                ></el-input>
              </el-form-item>
            </el-col>
            <el-col :span="2" style="padding-left: 10px">
              <el-color-picker v-model="formData.titleColor"></el-color-picker>
            </el-col>
          </el-row>
          <el-form-item label="关键词" prop="keyWord">
            <el-input
              v-model="formData.keyWord"
              type="textarea"
              placeholder="请输入SEO关键词"
              :maxlength="200"
              show-word-limit
              :autosize="{ minRows: 2, maxRows: 4 }"
              :style="{ width: '100%' }"
            ></el-input>
          </el-form-item>

          <el-form-item label="描述" prop="summary">
            <el-input
              v-model="formData.summary"
              type="textarea"
              placeholder="请输入SEO描述"
              :maxlength="300"
              show-word-limit
              :autosize="{ minRows: 2, maxRows: 4 }"
              :style="{ width: '100%' }"
            ></el-input>
          </el-form-item>
          <el-form-item label="外链地址" prop="linkUrl">
            <el-input
              v-model="formData.linkUrl"
              placeholder="请输入外链地址"
              :maxlength="255"
              show-word-limit
              clearable
              :style="{ width: '100%' }"
              ><template #prepend> <el-button icon="Link" /> </template
            ></el-input>
          </el-form-item>
          <el-form-item label="文章标签" prop="tag">
            <el-select
              v-model="formData.tag"
              multiple
              filterable
              allow-create
              clearable
              :maxlength="100"
              placeholder="手动输入文章标签"
              :style="{ width: '100%' }"
            ></el-select>
          </el-form-item>
          <el-form-item label="权重" prop="sort">
            <el-slider
              v-model="formData.sort"
              placeholder="请输入作者"
              clearable
              :max="500"
              :style="{ width: '100%' }"
              show-input
            ></el-slider>
          </el-form-item>
          <el-row style="padding-left: 40px">
            <el-col :span="6">
              <el-form-item label="总点击量" label-width="80px" prop="hits">
                <el-input
                  v-model="formData.hits"
                  placeholder="请输入总点击量"
                  :style="{ width: '100%' }"
                ></el-input>
              </el-form-item>
            </el-col>
            <el-col :span="6">
              <el-form-item label="日点击量" label-width="80px" prop="dayHits">
                <el-input
                  v-model="formData.dayHits"
                  placeholder="日点击量"
                  :style="{ width: '100%' }"
                ></el-input>
              </el-form-item>
            </el-col>
            <el-col :span="6">
              <el-form-item label="周点击量" label-width="80px" prop="weedHits">
                <el-input
                  v-model="formData.weedHits"
                  placeholder="周点击量"
                  :style="{ width: '100%' }"
                ></el-input>
              </el-form-item>
            </el-col>
            <el-col :span="6">
              <el-form-item
                label="月点击量"
                label-width="80px"
                prop="monthHits"
              >
                <el-input
                  v-model="formData.monthHits"
                  placeholder="月点击量"
                  :style="{ width: '100%' }"
                ></el-input>
              </el-form-item>
            </el-col>
          </el-row>
        </el-col>
        <el-col :span="10">
          <el-form-item label="发布时间" prop="publishTime">
            <el-date-picker
              v-model="formData.publishTime"
              type="datetime"
              :style="{ width: '100%' }"
              clearable
            ></el-date-picker>
          </el-form-item>
          <el-form-item label="审核状态" prop="status" required>
            <el-switch v-model="formData.status"></el-switch>
          </el-form-item>
          <el-form-item label="图片" prop="imgUrl">
            <soa-upload v-model="formData.imgUrl" :width="240" :height="140" />
          </el-form-item>
          <el-form-item label="文章属性" prop="attr">
            <el-checkbox-group v-model="formData.attr">
              <el-checkbox
                v-for="(item, index) in attrOptions"
                :key="index"
                :label="item.value"
              >
                {{ item.label }}
              </el-checkbox>
            </el-checkbox-group>
          </el-form-item>
          <el-form-item label="作者" prop="author">
            <el-input
              v-model="formData.author"
              placeholder="请输入作者"
              clearable
              :style="{ width: '100%' }"
            ></el-input>
          </el-form-item>
          <el-form-item label="来源" prop="source">
            <el-input
              v-model="formData.source"
              placeholder="请输入来源"
              :maxlength="200"
              clearable
              :style="{ width: '100%' }"
            ></el-input>
          </el-form-item>
        </el-col>
      </el-row>
      <el-form-item label="扩展值" prop="extend">
        <soa-form-table
          v-model="formData.extend"
          :add-template="extendTemplate"
          placeholder="暂未设置扩展值"
          class="w-full"
        >
          <el-table-column label="名称" width="200">
            <template #default="{ row }">
              <el-input v-model="row.key" placeholder="示例：地址" />
            </template>
          </el-table-column>
          <el-table-column label="值">
            <template #default="{ row }">
              <el-input v-model="row.value" placeholder="示例：北京市朝阳区" />
            </template>
          </el-table-column>
        </soa-form-table>
      </el-form-item>
      <div class="p-2">
        <soaEditor v-model="formData.content" :height="400" />
      </div>
    </el-form>
  </Modal>
</template>
