<script lang="ts" setup>
import { useSoaDrawer } from "@/component/soaDrawer/index.vue";
import { addCmsProduct, updateCmsProduct } from "@/api/cms/product";
import { fetchCmsColumnList } from "@/api/cms/column";
import { changeTree } from "@/utils/tools";
import soaUpload from "@/component/soaUpload/index.vue";
import soaUploadMultiple from "@/component/soaUpload/multiple.vue";
import soaEditor from "@/component/soaEditor/index.vue";

const columnOptions = ref<any[]>([]);
const activeTab = ref("basic");

const formData = reactive({
  id: "0",
  columnId: "",
  columnArr: [] as any[],
  title: undefined as string | undefined,
  subTitle: undefined as string | undefined,
  productNo: undefined as string | undefined,
  unit: undefined as string | undefined,
  price: 0,
  marketPrice: 0,
  costPrice: 0,
  intro: undefined as string | undefined,
  content: "",
  imgUrl: undefined as string | undefined,
  album: [] as string[],
  tags: [] as string[],
  status: true,
  sort: 0,
  hits: 0,
  dayHits: 0,
  weedHits: 0,
  monthHits: 0,
  publishTime: new Date(),
  extend: [] as any[],
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
      message: "请输入产品名称",
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

const [Drawer, drawerApi] = useSoaDrawer({
  onCancel: () => {
    resetForm();
  },
});

const initColumnTree = async () => {
  const tree = await fetchCmsColumnList();
  const _tree: any[] = [];
  tree.some((m: any) => {
    _tree.push({
      id: m.id,
      value: m.id,
      label: m.title,
      parentId: m.parentId,
    });
  });
  columnOptions.value = changeTree(_tree);
};

const openModal = async (row?: any, cid?: string) => {
  if (row) {
    Object.assign(formData, row);
  }
  if (row && cid && cid !== "0") {
    formData.columnId = cid;
  }
  activeTab.value = "basic";
  drawerApi.open();
};

const handleSubmit = () => {
  if (saving.value) return;
  formRef.value?.validate(async (valid: any) => {
    if (!valid) return;
    saving.value = true;
    try {
      if (formData.id === "0" || formData.id === 0) {
        await addCmsProduct(formData);
        ElMessage.success("新增成功");
      } else {
        await updateCmsProduct(formData);
        ElMessage.success("更新成功");
      }
      emit("complete");
      drawerApi.close();
    } finally {
      resetForm();
      saving.value = false;
    }
  });
};

const resetForm = () => {
  Object.assign(formData, {
    id: "0",
    columnId: "",
    columnArr: [] as any[],
    title: undefined,
    subTitle: undefined,
    productNo: undefined,
    unit: undefined,
    price: 0,
    marketPrice: 0,
    costPrice: 0,
    intro: undefined,
    content: "",
    imgUrl: undefined,
    album: [] as string[],
    tags: [] as string[],
    status: true,
    sort: 0,
    hits: 0,
    dayHits: 0,
    weedHits: 0,
    monthHits: 0,
    publishTime: new Date(),
    extend: [] as any[],
  });
  activeTab.value = "basic";
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
  <Drawer
    title="产品管理"
    :close-on-click-modal="false"
    :footer="false"
    size="80%"
  >
    <el-form
      ref="formRef"
      label-position="top"
      label-width="100px"
      :model="formData"
      :rules="rules"
    >
      <el-tabs v-model="activeTab">
        <el-tab-pane label="基本信息" name="basic">
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
              <el-form-item label="产品名称" prop="title">
                <el-input
                  v-model="formData.title"
                  placeholder="请输入产品名称"
                  :maxlength="50"
                  show-word-limit
                  clearable
                  :style="{ width: '100%' }"
                ></el-input>
              </el-form-item>
              <el-form-item label="产品副标题" prop="subTitle">
                <el-input
                  v-model="formData.subTitle"
                  placeholder="请输入产品副标题"
                  :maxlength="100"
                  show-word-limit
                  clearable
                  :style="{ width: '100%' }"
                ></el-input>
              </el-form-item>
              <el-row :gutter="15">
                <el-col :span="12">
                  <el-form-item label="产品编号" prop="productNo">
                    <el-input
                      v-model="formData.productNo"
                      placeholder="请输入产品编号"
                      :maxlength="64"
                      clearable
                      :style="{ width: '100%' }"
                    ></el-input>
                  </el-form-item>
                </el-col>
                <el-col :span="12">
                  <el-form-item label="单位" prop="unit">
                    <el-input
                      v-model="formData.unit"
                      placeholder="件 / 台 / 套"
                      :maxlength="32"
                      clearable
                      :style="{ width: '100%' }"
                    ></el-input>
                  </el-form-item>
                </el-col>
              </el-row>
              <el-row :gutter="15">
                <el-col :span="8">
                  <el-form-item label="销售价" prop="price">
                    <el-input-number
                      v-model="formData.price"
                      :min="0"
                      :step="1"
                      :precision="2"
                      :controls="false"
                      :style="{ width: '100%' }"
                    />
                  </el-form-item>
                </el-col>
                <el-col :span="8">
                  <el-form-item label="市场价" prop="marketPrice">
                    <el-input-number
                      v-model="formData.marketPrice"
                      :min="0"
                      :step="1"
                      :precision="2"
                      :controls="false"
                      :style="{ width: '100%' }"
                    />
                  </el-form-item>
                </el-col>
                <el-col :span="8">
                  <el-form-item label="成本价" prop="costPrice">
                    <el-input-number
                      v-model="formData.costPrice"
                      :min="0"
                      :step="1"
                      :precision="2"
                      :controls="false"
                      :style="{ width: '100%' }"
                    />
                  </el-form-item>
                </el-col>
              </el-row>
              <el-form-item label="产品简介" prop="intro">
                <el-input
                  v-model="formData.intro"
                  type="textarea"
                  placeholder="请输入产品简介"
                  :maxlength="300"
                  show-word-limit
                  :autosize="{ minRows: 2, maxRows: 4 }"
                  :style="{ width: '100%' }"
                ></el-input>
              </el-form-item>
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
              <el-form-item label="销售状态" prop="status" required>
                <el-switch
                  v-model="formData.status"
                  active-text="上架"
                  inactive-text="下架"
                ></el-switch>
              </el-form-item>
              <el-form-item label="产品标签" prop="tags">
                <el-select
                  v-model="formData.tags"
                  multiple
                  filterable
                  allow-create
                  clearable
                  placeholder="手动输入产品标签"
                  :style="{ width: '100%' }"
                ></el-select>
              </el-form-item>
              <el-form-item label="权重" prop="sort" class="pl-3">
                <el-slider
                  v-model="formData.sort"
                  :max="500"
                  show-input
                  :style="{ width: '100%' }"
                ></el-slider>
              </el-form-item>
              <el-row :gutter="20">
                <el-col :span="6">
                  <el-form-item label="总点击量" label-width="80px" prop="hits">
                    <el-input
                      v-model="formData.hits"
                      placeholder="总点击量"
                      :style="{ width: '100%' }"
                    ></el-input>
                  </el-form-item>
                </el-col>
                <el-col :span="6">
                  <el-form-item
                    label="日点击量"
                    label-width="80px"
                    prop="dayHits"
                  >
                    <el-input
                      v-model="formData.dayHits"
                      placeholder="日点击量"
                      :style="{ width: '100%' }"
                    ></el-input>
                  </el-form-item>
                </el-col>
                <el-col :span="6">
                  <el-form-item
                    label="周点击量"
                    label-width="80px"
                    prop="weedHits"
                  >
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
          </el-row>
          <div class="pl-2">
            <el-form-item label="封面图片" prop="imgUrl">
              <soa-upload
                v-model="formData.imgUrl"
                :width="240"
                :height="140"
                directory="product"
              />
            </el-form-item>
            <el-form-item label="产品相册" prop="album">
              <soa-upload-multiple
                v-model="formData.album"
                :width="240"
                :height="140"
                :max-count="8"
                directory="product"
              />
            </el-form-item>
          </div>
        </el-tab-pane>
        <el-tab-pane label="产品内容" name="content">
          <div class="p-2">
            <soaEditor v-model="formData.content" :height="400" />
          </div>
        </el-tab-pane>
        <el-tab-pane label="自定义参数" name="extend">
          <el-form-item label="自定义参数" prop="extend">
            <soa-form-table
              v-model="formData.extend"
              :add-template="extendTemplate"
              placeholder="暂未设置自定义参数"
              class="w-full"
            >
              <el-table-column label="名称" width="200">
                <template #default="{ row }">
                  <el-input v-model="row.key" placeholder="示例：规格" />
                </template>
              </el-table-column>
              <el-table-column label="值">
                <template #default="{ row }">
                  <el-input
                    v-model="row.value"
                    placeholder="示例：10cm × 20cm"
                  />
                </template>
              </el-table-column>
            </soa-form-table>
          </el-form-item>
        </el-tab-pane>
      </el-tabs>
      <div class="mt-4 flex justify-end gap-2">
        <el-button @click="drawerApi.close()">取消</el-button>
        <el-button type="primary" :loading="saving" @click="handleSubmit">
          保存
        </el-button>
      </div>
    </el-form>
  </Drawer>
</template>
