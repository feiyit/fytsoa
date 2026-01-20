<template>
  <el-skeleton v-if="showSkeleton" animated :rows="skeletonRows" />
  <el-form
    v-else
    ref="formRef"
    :model="form"
    :label-width="schema?.labelWidth ?? 'auto'"
    :label-position="schema?.labelPosition ?? 'right'"
    v-loading="formLoading"
    element-loading-text="Loading..."
  >
    <el-row :gutter="schema?.gutter ?? DEFAULT_FORM_GUTTER">
      <template v-for="(item, index) in schema?.formItems ?? []" :key="index">
        <el-col v-if="!shouldHide(item)" :span="item.span ?? 24">
          <div v-if="item.component === 'title'" class="soa-form__title">
            <span>{{ item.label }}</span>
          </div>
          <el-form-item v-else :prop="item.name" :rules="rulesHandle(item)">
            <template #label>
              <span>{{ item.label }}</span>
              <el-tooltip v-if="item.tips" :content="item.tips">
                <el-icon class="soa-form__tips-icon"
                  ><QuestionFilled
                /></el-icon>
              </el-tooltip>
            </template>

            <template v-if="item.component === 'input'">
              <el-input
                v-model="form[item.name as string]"
                :type="item.options?.type === 'textarea' ? 'textarea' : 'text'"
                :placeholder="item.options?.placeholder"
                :maxlength="item.options?.maxlength"
                :show-word-limit="Boolean(item.options?.maxlength)"
                clearable
              />
            </template>

            <template v-else-if="item.component === 'checkbox'">
              <template v-if="item.name">
                <el-checkbox
                  v-for="option in item.options?.items ?? []"
                  :key="option.name ?? option.value"
                  v-model="form[item.name!][option.name as string]"
                  :label="option.label"
                />
              </template>
              <template v-else>
                <el-checkbox
                  v-for="option in item.options?.items ?? []"
                  :key="option.name ?? option.value"
                  v-model="form[option.name as string]"
                  :label="option.label"
                />
              </template>
            </template>

            <template v-else-if="item.component === 'checkboxGroup'">
              <el-checkbox-group v-model="form[item.name as string]">
                <el-checkbox
                  v-for="option in item.options?.items ?? []"
                  :key="option.value ?? option.label"
                  :label="option.value"
                >
                  {{ option.label }}
                </el-checkbox>
              </el-checkbox-group>
            </template>

            <template v-else-if="item.component === 'upload'">
              <div class="soa-form__upload-group">
                <div
                  v-for="option in item.options?.items ?? []"
                  :key="option.name ?? option.label"
                  class="soa-form__upload-line"
                >
                  <div class="soa-form__upload-label">{{ option.label }}</div>
                  <SoaUpload
                    :model-value="getUploadModel(item, option)"
                    @update:modelValue="setUploadModel(item, option, $event)"
                    :action="resolveUploadAction(option)"
                    v-bind="getUploadProps(option)"
                  />
                </div>
              </div>
            </template>

            <template v-else-if="item.component === 'switch'">
              <el-switch v-model="form[item.name as string]" />
            </template>

            <template v-else-if="item.component === 'select'">
              <el-select
                v-model="form[item.name as string]"
                :multiple="item.options?.multiple"
                :placeholder="item.options?.placeholder"
                clearable
                filterable
                class="w-full"
              >
                <el-option
                  v-for="option in item.options?.items ?? []"
                  :key="option.value ?? option.label"
                  :label="option.label"
                  :value="option.value"
                />
              </el-select>
            </template>

            <template v-else-if="item.component === 'cascader'">
              <el-cascader
                v-model="form[item.name as string]"
                :options="item.options?.items ?? []"
                clearable
                class="w-full"
              />
            </template>

            <template v-else-if="item.component === 'date'">
              <el-date-picker
                v-model="form[item.name as string]"
                :type="(item.options?.type as any) || 'date'"
                :shortcuts="item.options?.shortcuts"
                :default-time="item.options?.defaultTime"
                :value-format="item.options?.valueFormat"
                :placeholder="item.options?.placeholder ?? '请选择'"
                class="w-full"
              />
            </template>

            <template v-else-if="item.component === 'number'">
              <el-input-number
                v-model="form[item.name as string]"
                controls-position="right"
              />
            </template>

            <template v-else-if="item.component === 'radio'">
              <el-radio-group v-model="form[item.name as string]">
                <el-radio
                  v-for="option in item.options?.items ?? []"
                  :key="option.value ?? option.label"
                  :label="option.value"
                >
                  {{ option.label }}
                </el-radio>
              </el-radio-group>
            </template>

            <template v-else-if="item.component === 'color'">
              <el-color-picker v-model="form[item.name as string]" />
            </template>

            <template v-else-if="item.component === 'rate'">
              <el-rate v-model="form[item.name as string]" />
            </template>

            <template v-else-if="item.component === 'slider'">
              <el-slider
                v-model="form[item.name as string]"
                :marks="item.options?.marks"
              />
            </template>

            <template v-else-if="item.component === 'tableselect'">
              <SoaTableSelect
                v-model="form[item.name as string]"
                :data="item.options?.items"
                v-bind="item.options?.componentProps"
              >
                <el-table-column
                  v-for="column in item.options?.columns ?? []"
                  :key="column.prop ?? column.label"
                  v-bind="column"
                />
              </SoaTableSelect>
            </template>

            <template v-else-if="item.component === 'editor'">
              <el-input
                v-model="form[item.name as string]"
                type="textarea"
                :rows="item.options?.rows ?? 6"
                :placeholder="item.options?.placeholder ?? '请输入内容'"
              />
            </template>

            <template v-else>
              <el-tag type="danger"
                >[{{ item.component }}] Component not found</el-tag
              >
            </template>

            <div v-if="item.message" class="el-form-item-msg">
              {{ item.message }}
            </div>
          </el-form-item>
        </el-col>
      </template>

      <el-col v-if="schema?.formItems?.length" :span="24">
        <el-form-item>
          <slot>
            <el-button type="primary" @click="handleSubmit">
              {{ schema?.submitText ?? "提交" }}
            </el-button>
            <el-button v-if="schema?.showReset !== false" @click="resetFields">
              {{ schema?.resetText ?? "重置" }}
            </el-button>
          </slot>
        </el-form-item>
      </el-col>
    </el-row>
  </el-form>
</template>

<script setup lang="ts">
import { ElMessage } from "element-plus";
import type { FormInstance } from "element-plus";
import { QuestionFilled } from "@element-plus/icons-vue";
import { computed, reactive, ref, watch } from "vue";

import SoaTableSelect from "@/component/soaTableSelect/index.vue";
import SoaUpload from "@/component/soaUpload/index.vue";

import { requestFormRemote } from "./formConfig";
import type { FormConfig, FormItemConfig, FormRecord } from "./types";
import { DEFAULT_FORM_GUTTER } from "./types";

const props = withDefaults(
  defineProps<{
    modelValue?: FormRecord;
    config?: FormConfig | null;
    loading?: boolean;
  }>(),
  {
    modelValue: () => ({}),
    config: null,
    loading: false,
  }
);

const emit = defineEmits<{
  (event: "update:modelValue", value: FormRecord): void;
  (event: "submit", value: FormRecord): void;
}>();

const formRef = ref<FormInstance>();
const schema = ref<FormConfig | null>(null);
const initializing = ref(true);
const remoteLoading = ref(false);
const suppressEmit = ref(false);

const form = reactive<FormRecord>({});

const cloneDeep = <T>(value: T): T => {
  if (value === null || typeof value !== "object") {
    return value;
  }
  if (Array.isArray(value)) {
    return value.map((item) => cloneDeep(item)) as unknown as T;
  }
  const result: Record<string, any> = {};
  Object.keys(value as Record<string, any>).forEach((key) => {
    result[key] = cloneDeep((value as Record<string, any>)[key]);
  });
  return result as T;
};

const deepMerge = (target: FormRecord, source: FormRecord): FormRecord => {
  Object.keys(source || {}).forEach((key) => {
    const sourceValue = source[key];
    if (
      sourceValue &&
      typeof sourceValue === "object" &&
      !Array.isArray(sourceValue)
    ) {
      const base = target[key];
      if (!base || typeof base !== "object" || Array.isArray(base)) {
        target[key] = {};
      }
      deepMerge(target[key] as FormRecord, sourceValue as FormRecord);
    } else {
      target[key] = sourceValue;
    }
  });
  return target;
};

const resetFormState = () => {
  Object.keys(form).forEach((key) => {
    delete form[key];
  });
};

const initializeItemValue = (item: FormItemConfig) => {
  if (!item || item.component === "title") {
    return;
  }
  if (
    !item.name &&
    item.component !== "checkbox" &&
    item.component !== "upload"
  ) {
    return;
  }

  const items = item.options?.items ?? [];

  if (item.component === "checkbox") {
    if (item.name) {
      const value: Record<string, any> = {};
      items.forEach((option) => {
        const optionName = option.name as string;
        if (!optionName) return;
        value[optionName] = option.value ?? false;
      });
      form[item.name] = value;
    } else {
      items.forEach((option) => {
        const optionName = option.name as string;
        if (!optionName) return;
        form[optionName] = option.value ?? false;
      });
    }
    return;
  }

  if (item.component === "upload") {
    if (item.name) {
      const value: Record<string, any> = {};
      items.forEach((option) => {
        const optionName = option.name as string;
        if (!optionName) return;
        value[optionName] = option.value ?? [];
      });
      form[item.name] = value;
    } else {
      items.forEach((option) => {
        const optionName = option.name as string;
        if (!optionName) return;
        form[optionName] = option.value ?? [];
      });
    }
    return;
  }

  if (item.name) {
    if (
      item.component === "checkboxGroup" ||
      (item.component === "select" && item.options?.multiple)
    ) {
      form[item.name] = item.value ?? [];
      return;
    }

    if (item.component === "tableselect" && item.options?.multiple) {
      form[item.name] = item.value ?? [];
      return;
    }

    if (item.component === "switch") {
      form[item.name] = item.value ?? false;
      return;
    }

    form[item.name] = item.value ?? null;
  }
};

const ensureCollectionKeys = (item: FormItemConfig) => {
  const items = item.options?.items ?? [];
  if (!items.length) return;

  if (item.component === "checkbox") {
    if (item.name) {
      const group = (form[item.name] ??= {});
      items.forEach((option) => {
        const optionName = option.name as string;
        if (!optionName || optionName in group) return;
        group[optionName] = option.value ?? false;
      });
    } else {
      items.forEach((option) => {
        const optionName = option.name as string;
        if (!optionName || optionName in form) return;
        form[optionName] = option.value ?? false;
      });
    }
  }

  if (item.component === "upload") {
    if (item.name) {
      const group = (form[item.name] ??= {});
      items.forEach((option) => {
        const optionName = option.name as string;
        if (!optionName || optionName in group) return;
        group[optionName] = option.value ?? [];
      });
    } else {
      items.forEach((option) => {
        const optionName = option.name as string;
        if (!optionName || optionName in form) return;
        form[optionName] = option.value ?? [];
      });
    }
  }
};

const applyRemoteDefaults = (item: FormItemConfig) => {
  if (!item || !item.name) return;
  if (
    (item.component === "select" || item.component === "radio") &&
    (form[item.name] === undefined || form[item.name] === null)
  ) {
    const first = item.options?.items?.[0];
    if (first && !item.options?.multiple) {
      form[item.name] = first.value ?? null;
    }
  }
};

const getUploadModel = (item: FormItemConfig, option: Record<string, any>) => {
  const optionName = option.name as string;
  if (!optionName) return undefined;
  if (item.name) {
    const group = (form[item.name] ??= {});
    return group[optionName];
  }
  return form[optionName];
};

const setUploadModel = (
  item: FormItemConfig,
  option: Record<string, any>,
  value: any
) => {
  const optionName = option.name as string;
  if (!optionName) return;
  if (item.name) {
    const group = (form[item.name] ??= {});
    group[optionName] = value;
  } else {
    form[optionName] = value;
  }
};

const resolveUploadAction = (option: Record<string, any>): string => {
  const props = option.componentProps ?? {};
  if (typeof props.action === "string" && props.action) {
    return props.action;
  }
  if (typeof option.action === "string" && option.action) {
    return option.action;
  }
  return "/sysfile/upload?path=/upload/temp/";
};

const getUploadProps = (option: Record<string, any>) => {
  const props = { ...(option.componentProps ?? {}) } as Record<string, any>;
  if ("action" in props) {
    delete props.action;
  }
  return props;
};

const fetchRemoteOptions = async (items: FormItemConfig[]) => {
  const tasks: Promise<void>[] = [];
  items.forEach((item) => {
    const remote = item.options?.remote;
    if (!remote) return;
    tasks.push(
      requestFormRemote(remote.api, remote.data)
        .then((options) => {
          item.options = item.options ?? {};
          item.options.items = options;
          ensureCollectionKeys(item);
          applyRemoteDefaults(item);
        })
        .catch((error) => {
          if (import.meta.env.DEV) {
            // eslint-disable-next-line no-console
            console.warn("[soaForm] 远程数据加载失败", error);
          }
          ElMessage.error("远程选项加载失败");
        })
    );
  });

  if (tasks.length) {
    remoteLoading.value = true;
    await Promise.all(tasks).finally(() => {
      remoteLoading.value = false;
    });
  }
};

const render = async () => {
  if (!schema.value) {
    initializing.value = false;
    return;
  }

  initializing.value = true;
  resetFormState();

  schema.value.formItems?.forEach((item) => {
    initializeItemValue(item);
  });

  if (props.modelValue && Object.keys(props.modelValue).length) {
    suppressEmit.value = true;
    deepMerge(form, cloneDeep(props.modelValue));
    suppressEmit.value = false;
  }

  await fetchRemoteOptions(schema.value.formItems ?? []);
  initializing.value = false;
};

watch(
  () => props.config,
  (config) => {
    if (!config) {
      schema.value = null;
      initializing.value = false;
      return;
    }
    schema.value = cloneDeep(config);
    render();
  },
  { deep: true, immediate: true }
);

watch(
  () => props.modelValue,
  (value) => {
    if (!value || suppressEmit.value) return;
    suppressEmit.value = true;
    deepMerge(form, cloneDeep(value));
    suppressEmit.value = false;
  },
  { deep: true }
);

watch(
  form,
  (value) => {
    if (suppressEmit.value) return;
    emit("update:modelValue", cloneDeep(value));
  },
  { deep: true }
);

const showSkeleton = computed(() => initializing.value);
const skeletonRows = computed(() => schema.value?.formItems?.length || 3);
const formLoading = computed(
  () => Boolean(props.loading) || remoteLoading.value
);

const evaluateHandle = (
  handle: FormItemConfig["hideHandle"] | FormItemConfig["requiredHandle"]
): boolean => {
  if (!handle) return false;
  const formSnapshot = form as FormRecord;
  try {
    if (typeof handle === "function") {
      return Boolean(handle(formSnapshot));
    }
    const expression = handle.replace(/\$/g, "form");
    // eslint-disable-next-line no-new-func
    return Boolean(
      new Function("form", `return (${expression})`)(formSnapshot)
    );
  } catch (error) {
    if (import.meta.env.DEV) {
      // eslint-disable-next-line no-console
      console.warn("[soaForm] 表达式解析失败", handle, error);
    }
    return false;
  }
};

const shouldHide = (item: FormItemConfig): boolean => {
  if (!item?.hideHandle) return false;
  return evaluateHandle(item.hideHandle);
};

const rulesHandle = (item: FormItemConfig) => {
  if (!item.rules) return item.rules;
  const rules = cloneDeep(item.rules);
  if (!item.requiredHandle) return rules;

  const required = evaluateHandle(item.requiredHandle);
  const ruleList = Array.isArray(rules) ? rules : [rules];
  const requiredRule = ruleList.find((rule: any) =>
    Object.prototype.hasOwnProperty.call(rule, "required")
  );
  if (requiredRule) {
    requiredRule.required = required;
  }
  return Array.isArray(rules) ? ruleList : ruleList[0];
};

const validate = (
  callback?: (isValid: boolean, invalidFields?: Record<string, any>) => void
) => formRef.value?.validate(callback);

const scrollToField = (prop: string) => formRef.value?.scrollToField(prop);

const resetFields = () => {
  formRef.value?.resetFields();
};

const handleSubmit = () => {
  emit("submit", cloneDeep(form));
};

defineExpose({
  validate,
  resetFields,
  scrollToField,
  submit: handleSubmit,
  form,
});
</script>

<style scoped>
.soa-form__title {
  font-weight: 600;
  font-size: 16px;
  color: var(--el-color-primary);
  margin: 4px 0 16px;
  border-left: 3px solid var(--el-color-primary);
  padding-left: 10px;
}

.soa-form__tips-icon {
  margin-left: 4px;
  cursor: pointer;
  color: var(--el-color-primary);
}

.el-form-item-msg {
  margin-top: 6px;
  font-size: 12px;
  color: var(--el-color-info);
}

.soa-form__upload-group {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.soa-form__upload-line {
  display: flex;
  gap: 16px;
  align-items: center;
}

.soa-form__upload-label {
  min-width: 88px;
  font-size: 13px;
  color: var(--el-text-color-secondary);
}
</style>
