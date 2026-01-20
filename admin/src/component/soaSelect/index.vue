<template>
  <el-select
    v-model="internalValue"
    :placeholder="placeholder"
    :loading="loading"
    :filterable="filterable"
    :clearable="clearable"
    :collapse-tags="collapseTags"
    :multiple="multiple"
    :multiple-limit="multipleLimit"
    :remote="remote"
    :remote-method="fetchOptions"
    @visible-change="visibleChange"
    :teleported="false"
    :value-on-clear="valueClear"
    @change="handleChange"
  >
    <el-option
      v-for="item in options"
      :key="item[valueKey]"
      :label="item[labelKey]"
      :value="item[valueKey]"
    />
    <template #footer v-if="isAdd">
      <el-button
        type="primary"
        v-if="!isAdding"
        size="small"
        @click="onAddOption"
      >
        添加选项
      </el-button>
      <template v-else>
        <el-input
          v-model="optionName"
          class="option-input"
          placeholder="请输入内容"
          size="small"
        />
        <el-button type="primary" size="small" @click="onConfirm">
          确定
        </el-button>
        <el-button size="small" @click="clear">取消</el-button>
      </template>
    </template>
  </el-select>
</template>
<script lang="ts" setup>
import { ref, watch, computed, defineProps, defineEmits, onMounted } from "vue";
import { addSysCode } from "@/api";
const props = defineProps({
  modelValue: {
    type: [String, Number, Array],
    default: "",
  },
  placeholder: {
    type: String,
    default: "请选择",
  },
  data: {
    type: Array,
    default: [],
  },
  labelKey: {
    type: String,
    default: "label",
  },
  valueKey: {
    type: String,
    default: "value",
  },
  filterable: {
    type: Boolean,
    default: false,
  },
  clearable: {
    type: Boolean,
    default: true,
  },
  remote: {
    type: Boolean,
    default: false,
  },
  multiple: {
    type: Boolean,
    default: false,
  },
  multipleLimit: {
    type: Number,
    default: 0,
  },
  collapseTags: {
    type: Boolean,
    default: false,
  },
  isAdd: {
    type: Boolean,
    default: false,
  },
  params: { type: Object, default: () => ({}) },
  apiObj: { type: Object, default: () => {} },
  defaultOptions: { type: Array, default: () => [] },
  valueClear: {
    type: [String, Number, Array],
    default: "",
  },
  firstOption: { type: Array, default: () => [] },
});
const isAdding = ref(false);
const optionName = ref("");
const emits = defineEmits(["update:modelValue", "change"]);

// 定义选项列表和加载状态
const options = ref<Array<any>>([]);
const loading = ref(false);

// 监听modelValue的变化
watch(
  () => props.modelValue,
  (newValue) => {
    emits("update:modelValue", newValue);
  }
);

const internalValue = computed({
  get() {
    if (options.value.length == 0 && !props.multiple) {
      return "";
    } else if (options.value.length == 0 && props.multiple) {
      return [];
    } else {
      return props.modelValue;
    }
  },
  set(value) {
    emits("update:modelValue", value);
  },
});

onMounted(() => {
  if (props.modelValue && !props.data) {
    getRemoteData();
  }
  if (props.data) {
    options.value = props.data;
  }
});
const onAddOption = () => {
  isAdding.value = true;
};

const onConfirm = async () => {
  if (!optionName.value) {
    ElMessage({
      message: "请输入内容",
      type: "warning",
    });
    return;
  }
  const res: any = await addSysCode({
    typeCode: props.params.typeCode,
    tag: props.params.type,
    name: optionName.value,
  });
  if (res.code === 200) {
    getRemoteData(false);
    clear();
  } else {
    ElMessage({
      message: res.message,
      type: "error",
    });
  }
};

const clear = () => {
  optionName.value = "";
  isAdding.value = false;
};
// 处理选择变化
const handleChange = (value: any) => {
  emits("update:modelValue", value);
  emits("change", value);
};
const visibleChange = (ispoen: boolean) => {
  if (ispoen && options.value.length == 0) {
    getRemoteData();
  }
};
const getRemoteData = async (isLoading: Boolean = true) => {
  if (isLoading) {
    loading.value = true;
  }
  if (props.apiObj) {
    const res: any = await (props.apiObj as any)(props.params);
    if (res.code === 200) {
      options.value = res.data;
    }
  } else if (props.defaultOptions.length > 0) {
    options.value = props.defaultOptions;
  }
  if (props.firstOption.length > 0) {
    options.value.unshift(...props.firstOption);
  }
  loading.value = false;
};

const fetchOptions = async (query: string) => {
  loading.value = true;
  try {
    if (!props.apiObj) return;
    const res: any = await (props.apiObj as any).get?.(query);
    if (res && res.code === 200) {
      options.value = res.data;
    }
  } finally {
    loading.value = false;
  }
};
</script>
<style scoped>
.option-input {
  width: 100%;
  margin-bottom: 8px;
}
</style>
