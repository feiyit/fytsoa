<script setup lang="ts">
import { ElMessage } from "element-plus";
import { useSoaModal } from "@/component/soaModal/index.vue";
import {
  addQuartzTask,
  updateQuartzTask,
  type QuartzTask,
} from "@/api/sys/quartz";

defineOptions({ name: "ConfigSchedulerModify" });

const emit = defineEmits(["complete"]);
const formRef = ref<any>(null);
const saving = ref(false);
const title = ref("新建任务");

const formData = reactive<QuartzTask>({
  id: undefined,
  taskName: "",
  groupName: "default",
  interval: "0 0 * * * ?",
  describe: "",
  taskType: 2,
  apiUrl: "",
  apiRequestType: "GET",
  apiAuthKey: "",
  apiAuthValue: "",
  apiParameter: "",
  dllClassName: "",
  dllActionName: "",
});

const resetForm = () => {
  Object.assign(formData, {
    id: undefined,
    taskName: "",
    groupName: "default",
    interval: "0 0 * * * ?",
    describe: "",
    taskType: 2,
    apiUrl: "",
    apiRequestType: "GET",
    apiAuthKey: "",
    apiAuthValue: "",
    apiParameter: "",
    dllClassName: "",
    dllActionName: "",
  });
  nextTick(() => formRef.value?.clearValidate?.());
};

const rules = computed<any>(() => {
  const base: any = {
    taskName: [{ required: true, message: "请输入任务名称", trigger: "blur" }],
    groupName: [{ required: true, message: "请输入任务分组", trigger: "blur" }],
    interval: [
      { required: true, message: "请输入 Cron 表达式", trigger: "blur" },
    ],
    taskType: [
      { required: true, message: "请选择任务类型", trigger: "change" },
    ],
  };
  if (formData.taskType === 2) {
    base.apiUrl = [
      { required: true, message: "请输入请求地址", trigger: "blur" },
    ];
  } else if (formData.taskType === 1) {
    base.dllClassName = [
      { required: true, message: "请输入类名（含命名空间）", trigger: "blur" },
    ];
    base.dllActionName = [
      { required: true, message: "请输入方法名", trigger: "blur" },
    ];
  }
  return base;
});

const handleSubmit = async () => {
  if (saving.value) return;
  await formRef.value?.validate?.();

  saving.value = true;
  try {
    const payload: QuartzTask = {
      id: formData.id,
      taskName: String(formData.taskName || "").trim(),
      groupName: String(formData.groupName || "").trim(),
      interval: String(formData.interval || "").trim(),
      describe: String(formData.describe || "").trim(),
      taskType: Number(formData.taskType),
      apiUrl:
        formData.taskType === 2 ? String(formData.apiUrl || "").trim() : "",
      apiRequestType: formData.taskType === 2 ? formData.apiRequestType : "",
      apiAuthKey: formData.taskType === 2 ? formData.apiAuthKey : "",
      apiAuthValue: formData.taskType === 2 ? formData.apiAuthValue : "",
      apiParameter: formData.taskType === 2 ? formData.apiParameter : "",
      dllClassName:
        formData.taskType === 1
          ? String(formData.dllClassName || "").trim()
          : "",
      dllActionName:
        formData.taskType === 1
          ? String(formData.dllActionName || "").trim()
          : "",
    };

    if (!payload.id) {
      const res = await addQuartzTask(payload);
      ElMessage.success(res?.message || "新增成功");
    } else {
      const res = await updateQuartzTask(payload);
      ElMessage.success(res?.message || "更新成功");
    }

    emit("complete");
    modalApi.close();
  } finally {
    saving.value = false;
    resetForm();
  }
};

const [Modal, modalApi] = useSoaModal({
  onConfirm: handleSubmit,
  onCancel: resetForm,
});

const openModal = (row?: QuartzTask) => {
  resetForm();
  if (row?.id) {
    title.value = "编辑任务";
    Object.assign(formData, row);
    // 兼容后端可能返回 null
    formData.apiUrl = formData.apiUrl || "";
    formData.apiRequestType = formData.apiRequestType || "GET";
    formData.apiAuthKey = formData.apiAuthKey || "";
    formData.apiAuthValue = formData.apiAuthValue || "";
    formData.apiParameter = formData.apiParameter || "";
    formData.dllClassName = formData.dllClassName || "";
    formData.dllActionName = formData.dllActionName || "";
  } else {
    title.value = "新建任务";
  }
  modalApi.open();
};

defineExpose({ openModal });
</script>

<template>
  <Modal :title="title" width="760px" :close-on-click-modal="false">
    <el-form ref="formRef" :model="formData" :rules="rules" label-width="110px">
      <el-divider content-position="left">基础信息</el-divider>

      <el-form-item label="任务名称" prop="taskName">
        <el-input
          v-model="formData.taskName"
          placeholder="例如：DepreciationRunJob"
        />
      </el-form-item>
      <el-form-item label="任务分组" prop="groupName">
        <el-input v-model="formData.groupName" placeholder="例如：am" />
      </el-form-item>
      <el-form-item label="描述" prop="describe">
        <el-input v-model="formData.describe" placeholder="任务用途说明" />
      </el-form-item>
      <el-form-item label="Cron" prop="interval">
        <soaCron v-model="formData.interval" placeholder="Cron 表达式" />
      </el-form-item>
      <el-form-item label="任务类型" prop="taskType">
        <el-radio-group v-model="formData.taskType">
          <el-radio-button :label="2">HTTP</el-radio-button>
          <el-radio-button :label="1">业务处理器</el-radio-button>
        </el-radio-group>
      </el-form-item>

      <el-divider content-position="left" v-if="formData.taskType === 2"
        >HTTP 配置</el-divider
      >
      <template v-if="formData.taskType === 2">
        <el-form-item label="请求地址" prop="apiUrl">
          <el-input
            v-model="formData.apiUrl"
            placeholder="例如：https://example.com/api/run"
          />
        </el-form-item>
        <el-form-item label="请求方式" prop="apiRequestType">
          <el-select v-model="formData.apiRequestType" style="width: 180px">
            <el-option label="GET" value="GET" />
            <el-option label="POST" value="POST" />
            <el-option label="PUT" value="PUT" />
            <el-option label="DELETE" value="DELETE" />
          </el-select>
        </el-form-item>
        <el-form-item label="认证Header" prop="apiAuthKey">
          <div class="flex w-full gap-2">
            <el-input
              v-model="formData.apiAuthKey"
              placeholder="Header Key（可选）"
            />
            <el-input
              v-model="formData.apiAuthValue"
              placeholder="Header Value（可选）"
            />
          </div>
        </el-form-item>
        <el-form-item label="请求参数" prop="apiParameter">
          <el-input
            v-model="formData.apiParameter"
            type="textarea"
            :rows="4"
            placeholder="可填写 JSON 字符串 / QueryString（按后端实现约定）"
          />
        </el-form-item>
      </template>

      <el-divider content-position="left" v-else>业务处理器配置</el-divider>
      <template v-if="formData.taskType === 1">
        <el-form-item label="类名" prop="dllClassName">
          <el-input
            v-model="formData.dllClassName"
            placeholder="例如：FytSoa.Application.Am.AmDepreciationService"
          />
        </el-form-item>
        <el-form-item label="方法名" prop="dllActionName">
          <el-input
            v-model="formData.dllActionName"
            placeholder="例如：RunAsync"
          />
        </el-form-item>
      </template>
    </el-form>

    <template #footer>
      <div class="flex items-center justify-between">
        <div class="text-xs text-slate-500">
          提示：任务创建后默认是“暂停”，需要手动开启才会按 Cron 自动执行。
        </div>
        <div class="flex items-center gap-2">
          <el-button @click="modalApi.close()">取消</el-button>
          <el-button type="primary" :loading="saving" @click="handleSubmit"
            >保存</el-button
          >
        </div>
      </div>
    </template>
  </Modal>
</template>
