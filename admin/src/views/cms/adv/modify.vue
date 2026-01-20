<script lang="ts" setup>
import { useSoaModal } from "@/component/soaModal/index.vue";
import { addAdvInfo, updateAdvInfo } from "@/api/cms/adv";
import soaUpload from "@/component/soaUpload/index.vue";
const formData = reactive({
  id: "0",
  columnId: 0,
  title: undefined,
  target: "_blank",
  status: true,
  isTimeLimit: false,
  beginTime: null,
  endTime: null,
  linkUrl: undefined,
  sort: 1,
  types: 1,
  hits: 0,
  imgUrl: null,
  summary: undefined,
  codes: undefined,
});
const rules = {
  title: [
    {
      required: true,
      message: "请输入广告位名称",
      trigger: "blur",
    },
  ],
  target: [
    {
      required: true,
      message: "请选择跳转方式",
      trigger: "change",
    },
  ],
  beginTime: [],
  endTime: [],
  linkUrl: [],
  summary: [],
  codes: [],
};
const column = ref({});
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
const targetOptions = [
  {
    label: "新窗口",
    value: "_blank",
  },
  {
    label: "原窗口",
    value: "_self",
  },
];
const openModal = async (row: any, columnParam: any) => {
  if (row) {
    Object.assign(formData, row);
  }
  if (columnParam) {
    column.value = column;
    formData.columnId = columnParam.id;
  }
  modalApi.open();
};
const handleSubmit = () => {
  formRef.value?.validate(async (valid: any) => {
    if (!valid) return;
    saving.value = true;
    try {
      if (formData.id === "0") {
        await addAdvInfo(formData);
        ElMessage.success("新增成功");
      } else {
        await updateAdvInfo(formData);
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
    columnId: 0,
    title: undefined,
    target: "_blank",
    status: true,
    isTimeLimit: false,
    beginTime: null,
    endTime: null,
    linkUrl: undefined,
    sort: 1,
    types: 1,
    hits: 0,
    imgUrl: null,
    summary: undefined,
    codes: undefined,
  });
};
defineExpose({
  openModal,
});
</script>
<template>
  <Modal
    title="广告位管理"
    class="w-[900px]"
    draggable
    :closeOnClickModal="false"
    :confirmLoading="saving"
  >
    <el-form ref="formRef" label-width="100px" :model="formData" :rules="rules">
      <el-container>
        <el-aside width="240px" class="no-right-border">
          <div class="select-img">
            <div class="bg-gray">
              <div class="up-wall pl-5 pt-3">
                <soa-upload
                  v-model="formData.imgUrl"
                  action="/sysfile/upload?path=/upload/adv/"
                  type="image"
                  :file-size="5"
                  :limit="1"
                />
              </div>
            </div>
          </div>
        </el-aside>
        <el-container style="display: block">
          <el-form
            ref="formRef"
            label-width="100px"
            :model="formData"
            :rules="rules"
          >
            <el-row>
              <el-col :span="12">
                <el-form-item label="广告位名称" prop="title">
                  <el-input
                    v-model="formData.title"
                    placeholder="请输入广告位名称"
                    :maxlength="100"
                    show-word-limit
                    clearable
                    :style="{ width: '100%' }"
                  ></el-input>
                </el-form-item>
              </el-col>
              <el-col :span="12">
                <el-form-item label="跳转方式" prop="target">
                  <el-select
                    v-model="formData.target"
                    placeholder="请选择跳转方式"
                    clearable
                    :style="{ width: '100%' }"
                  >
                    <el-option
                      v-for="(item, index) in targetOptions"
                      :key="index"
                      :label="item.label"
                      :value="item.value"
                      :disabled="item.disabled"
                    ></el-option>
                  </el-select>
                </el-form-item>
              </el-col>
              <el-col :span="12">
                <el-form-item label="状态" prop="status" required>
                  <el-switch v-model="formData.status"></el-switch>
                </el-form-item>
              </el-col>
              <el-col :span="12">
                <el-form-item label="时间限制" prop="isTimeLimit" required>
                  <el-switch v-model="formData.isTimeLimit"></el-switch>
                </el-form-item>
              </el-col>
              <el-col v-show="formData.isTimeLimit" :span="12">
                <el-form-item label="开始时间" prop="beginTime">
                  <el-date-picker
                    v-model="formData.beginTime"
                    format="yyyy-MM-dd"
                    value-format="yyyy-MM-dd"
                    :style="{ width: '100%' }"
                    placeholder="请选择开始时间"
                    clearable
                  ></el-date-picker>
                </el-form-item>
              </el-col>
              <el-col v-show="formData.isTimeLimit" :span="12">
                <el-form-item label="结束时间" prop="endTime">
                  <el-date-picker
                    v-model="formData.endTime"
                    format="yyyy-MM-dd"
                    value-format="yyyy-MM-dd"
                    :style="{ width: '100%' }"
                    placeholder="请选择结束时间"
                    clearable
                  ></el-date-picker>
                </el-form-item>
              </el-col>
            </el-row>
            <el-form-item label="链接地址" prop="linkUrl">
              <el-input
                v-model="formData.linkUrl"
                placeholder="请输入链接地址"
                :maxlength="128"
                show-word-limit
                clearable
                :style="{ width: '100%' }"
              ></el-input>
            </el-form-item>
            <el-form-item label="权重" prop="sort" required>
              <el-slider
                v-model="formData.sort"
                show-input
                :max="100"
                :step="1"
              ></el-slider>
            </el-form-item>
            <el-form-item label="广告描述" prop="summary">
              <el-input
                v-model="formData.summary"
                type="textarea"
                placeholder="请输入广告描述"
                :maxlength="500"
                show-word-limit
                :autosize="{ minRows: 3, maxRows: 3 }"
                :style="{ width: '100%' }"
              ></el-input>
            </el-form-item>
            <el-form-item label="广告代码" prop="codes">
              <el-input
                v-model="formData.codes"
                type="textarea"
                placeholder="请输入广告代码"
                :maxlength="500"
                show-word-limit
                :autosize="{ minRows: 3, maxRows: 3 }"
                :style="{ width: '100%' }"
              ></el-input>
            </el-form-item>
          </el-form>
        </el-container>
      </el-container>
    </el-form>
  </Modal>
</template>
