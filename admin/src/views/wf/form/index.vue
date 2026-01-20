<script setup lang="ts">
import Draggable from "vuedraggable";
import {
  EditPen,
  Notebook,
  Select,
  List,
  SwitchButton,
  Tickets,
  Pointer,
  Collection,
  MessageBox,
  Operation,
  Grid,
  Minus,
  Plus,
  RefreshLeft,
  RefreshRight,
  UploadFilled,
  Sort,
} from "@element-plus/icons-vue";

import SoaForm from "@/component/soaForm/index.vue";
import type {
  FormConfig,
  FormItemConfig,
  FormRecord,
} from "@/component/soaForm/types";
import {
  getWorkflowForm,
  getWorkflowFormList,
  createWorkflowForm,
  updateWorkflowForm,
  deleteWorkflowForm,
  type WorkflowFormDto,
} from "@/api";

interface FormFieldTemplate {
  label: string;
  component: string;
  icon: any;
  config: Partial<FormItemConfig>;
}

interface RowConfig {
  gutter: number;
  justify: string;
  align: string;
}

interface ColConfig {
  span: number;
}

interface DesignerFieldNode {
  id: string;
  type: "field";
  component: string;
  label: string;
  config: FormItemConfig;
}

interface DesignerDividerNode {
  id: string;
  type: "field";
  component: "divider";
  label: string;
  config: FormItemConfig;
}

interface DesignerColumnNode {
  id: string;
  type: "layout";
  component: "col";
  label: string;
  config: ColConfig;
  children: DesignerNode[];
}

interface DesignerRowNode {
  id: string;
  type: "layout";
  component: "row";
  label: string;
  config: RowConfig;
  columns: DesignerColumnNode[];
}

type DesignerNode =
  | DesignerFieldNode
  | DesignerDividerNode
  | DesignerRowNode
  | DesignerColumnNode;

type DesignerCanvas = Array<
  DesignerFieldNode | DesignerDividerNode | DesignerRowNode
>;

const MAX_HISTORY = 50;

// -------- 表单定义与后台交互（wf_form） --------

const tenantId = ref<number>(0);
const formList = ref<WorkflowFormDto[]>([]);
const currentFormId = ref<number | null>(null);
const currentForm = ref<WorkflowFormDto | null>(null);

// 表单名称：用于输入 / 显示当前表单名称
const formName = computed({
  get: () => currentForm.value?.name ?? "",
  set: (val: string) => {
    if (!currentForm.value) {
      currentForm.value = {
        tenantId: tenantId.value,
        status: 0,
        name: val,
        code: "",
      };
    } else {
      currentForm.value.name = val;
    }
  },
});

const uidState = reactive({
  field: 1,
  row: 1,
  col: 1,
});

const generateId = (prefix: keyof typeof uidState) =>
  `${prefix}_${uidState[prefix]++}`;

const ensureOptions = (node: DesignerFieldNode) => {
  if (!node.config.options) node.config.options = {};
  if (!Array.isArray(node.config.options.items)) {
    node.config.options.items = [];
  }
};

const basicComponents: FormFieldTemplate[] = [
  {
    label: "输入框",
    component: "input",
    icon: markRaw(EditPen),
    config: {
      label: "输入框",
      name: "",
      component: "input",
      span: 24,
      value: "",
      options: {
        placeholder: "请输入",
      },
    },
  },
  {
    label: "多行文本",
    component: "input",
    icon: markRaw(Notebook),
    config: {
      label: "多行文本",
      name: "",
      component: "input",
      span: 24,
      value: "",
      options: {
        placeholder: "请输入",
        type: "textarea",
      },
    },
  },
  {
    label: "下拉选择",
    component: "select",
    icon: markRaw(Select),
    config: {
      label: "下拉选择",
      name: "",
      component: "select",
      span: 24,
      value: null,
      options: {
        placeholder: "请选择",
        items: [
          { label: "选项一", value: "1" },
          { label: "选项二", value: "2" },
        ],
      },
    },
  },
  {
    label: "单选框",
    component: "radio",
    icon: markRaw(Pointer),
    config: {
      label: "单选框",
      name: "",
      component: "radio",
      span: 24,
      value: "1",
      options: {
        items: [
          { label: "选项一", value: "1" },
          { label: "选项二", value: "2" },
        ],
      },
    },
  },
  {
    label: "多选组",
    component: "checkboxGroup",
    icon: markRaw(List),
    config: {
      label: "多选组",
      name: "",
      component: "checkboxGroup",
      span: 24,
      value: [],
      options: {
        items: [
          { label: "选项一", value: "1" },
          { label: "选项二", value: "2" },
        ],
      },
    },
  },
  {
    label: "开关",
    component: "switch",
    icon: markRaw(SwitchButton),
    config: {
      label: "开关",
      name: "",
      component: "switch",
      span: 12,
      value: false,
    },
  },
  {
    label: "数字输入",
    component: "number",
    icon: markRaw(Operation),
    config: {
      label: "数字输入",
      name: "",
      component: "number",
      span: 12,
      value: 0,
    },
  },
  {
    label: "日期时间",
    component: "date",
    icon: markRaw(Tickets),
    config: {
      label: "日期时间",
      name: "",
      component: "date",
      span: 12,
      value: "",
      options: {
        type: "datetime",
        placeholder: "选择日期时间",
        valueFormat: "YYYY-MM-DD HH:mm:ss",
      },
    },
  },
  {
    label: "评分",
    component: "rate",
    icon: markRaw(MessageBox),
    config: {
      label: "评分",
      name: "",
      component: "rate",
      span: 12,
      value: 0,
    },
  },
  {
    label: "上传",
    component: "upload",
    icon: markRaw(UploadFilled),
    config: {
      label: "上传",
      name: "",
      component: "upload",
      span: 24,
      value: [],
      options: {
        items: [
          {
            label: "附件",
            name: "file",
            componentProps: {
              type: "file",
            },
          },
        ],
      },
    },
  },
  {
    label: "分割标题",
    component: "divider",
    icon: markRaw(Minus),
    config: {
      label: "分割标题",
      component: "title",
      span: 24,
    },
  },
];

const layoutComponents = [
  {
    label: "栅格行",
    component: "row",
    icon: markRaw(Grid),
  },
  {
    label: "栅格列",
    component: "col",
    icon: markRaw(Grid),
  },
];

const canvas = ref<DesignerCanvas>([]);
const selectedId = ref<string>("");

const history = ref<DesignerCanvas[]>([]);
const future = ref<DesignerCanvas[]>([]);
const ignoreHistory = ref(false);
let historyInitialized = false;

const resetHistoryWithCurrentCanvas = () => {
  history.value = [cloneDeep(canvas.value)];
  future.value = [];
  historyInitialized = true;
};

const cloneDeep = <T>(value: T): T => {
  if (value === null || typeof value !== "object") return value;
  if (Array.isArray(value))
    return value.map((item) => cloneDeep(item)) as unknown as T;
  const output: Record<string, any> = {};
  Object.keys(value as Record<string, any>).forEach((key) => {
    output[key] = cloneDeep((value as Record<string, any>)[key]);
  });
  return output as T;
};

const pushHistory = () => {
  if (!historyInitialized) {
    resetHistoryWithCurrentCanvas();
    return;
  }
  history.value.push(cloneDeep(canvas.value));
  if (history.value.length > MAX_HISTORY) history.value.shift();
  future.value = [];
};

watch(
  canvas,
  () => {
    if (ignoreHistory.value) return;
    pushHistory();
  },
  { deep: true }
);

watch(
  canvas,
  () => {
    if (!selectedId.value) return;
    const meta = findNodeMeta(canvas.value, selectedId.value);
    if (!meta) selectedId.value = "";
  },
  { deep: true }
);

const undo = () => {
  if (history.value.length <= 1) return;
  const current = history.value.pop();
  if (!current) return;
  future.value.push(cloneDeep(current));
  const previous = history.value[history.value.length - 1] || [];
  ignoreHistory.value = true;
  canvas.value = cloneDeep(previous);
  nextTick(() => {
    ignoreHistory.value = false;
  });
};

const redo = () => {
  if (!future.value.length) return;
  const next = future.value.pop();
  if (!next) return;
  history.value.push(cloneDeep(next));
  ignoreHistory.value = true;
  canvas.value = cloneDeep(next);
  nextTick(() => {
    ignoreHistory.value = false;
  });
};

pushHistory();

const generateFieldName = (component: string) => {
  const key = component.replace(/[^a-zA-Z]/g, "") || "field";
  const count = (uidState as any)[key]
    ? ++(uidState as any)[key]
    : ((uidState as any)[key] = 1);
  return `${key}_${count}`;
};

const createFieldNode = (
  template: FormFieldTemplate
): DesignerFieldNode | DesignerDividerNode => {
  const id = generateId("field");
  const config = cloneDeep(template.config) as FormItemConfig;
  if (template.component !== "divider") {
    config.name = generateFieldName(template.component);
    config.label = template.label;
  } else {
    config.label = template.label;
  }
  if (template.component === "upload") {
    if (!config.options?.items?.length) {
      config.options = config.options || {};
      config.options.items = [
        {
          label: "附件",
          name: "file",
          componentProps: {
            type: "file",
          },
        },
      ];
    }
  }
  return {
    id,
    type: "field",
    component: template.component as DesignerFieldNode["component"],
    label: template.label,
    config,
  } as DesignerFieldNode | DesignerDividerNode;
};

const createColNode = (): DesignerColumnNode => {
  const colId = generateId("col");
  return {
    id: colId,
    type: "layout",
    component: "col",
    label: "列",
    config: {
      span: 24,
    },
    children: [],
  };
};

const rebalanceRowColumns = (row: DesignerRowNode) => {
  const size = row.columns.length;
  if (!size) return;
  const base = Math.max(1, Math.floor(24 / size));
  let remaining = 24;
  row.columns.forEach((column, index) => {
    let span = base;
    if (index === size - 1) {
      span = remaining;
    } else {
      remaining -= span;
    }
    column.config.span = span;
    if (index === size - 1) {
      remaining = Math.max(0, remaining - span);
    }
  });
};

const createRowNode = (columnCount = 2): DesignerRowNode => {
  const rowId = generateId("row");
  const columns = Array.from({ length: columnCount }).map(() =>
    createColNode()
  );
  const row: DesignerRowNode = {
    id: rowId,
    type: "layout",
    component: "row",
    label: "栅格行",
    config: {
      gutter: 16,
      justify: "start",
      align: "top",
    },
    columns,
  };
  rebalanceRowColumns(row);
  return row;
};

const cloneFromPalette = (item: any) => {
  if ("config" in item) {
    return createFieldNode(item as FormFieldTemplate);
  }
  if (item.component === "row") {
    return createRowNode();
  }
  if (item.component === "col") {
    return createColNode();
  }
  return null;
};

const setSelected = (id: string) => {
  selectedId.value = id;
};

interface NodeMeta {
  node: DesignerNode;
  container:
    | DesignerNode[]
    | DesignerColumnNode[]
    | DesignerColumnNode["children"];
  index: number;
  parentRow?: DesignerRowNode;
  parentColumn?: DesignerColumnNode;
}

const findNodeMeta = (
  nodes: DesignerCanvas,
  targetId: string
): NodeMeta | null => {
  const walk = (
    list: DesignerNode[] | DesignerCanvas,
    parentRow?: DesignerRowNode,
    parentColumn?: DesignerColumnNode
  ): NodeMeta | null => {
    for (let index = 0; index < list.length; index += 1) {
      const node = list[index];
      if (node.id === targetId) {
        return {
          node,
          container: list,
          index,
          parentRow,
          parentColumn,
        };
      }
      if (node.type === "layout" && node.component === "row") {
        const rowNode = node as DesignerRowNode;
        for (
          let colIndex = 0;
          colIndex < rowNode.columns.length;
          colIndex += 1
        ) {
          const column = rowNode.columns[colIndex];
          if (column.id === targetId) {
            return {
              node: column,
              container: rowNode.columns,
              index: colIndex,
              parentRow: rowNode,
            };
          }
          const childMeta = walk(column.children, rowNode, column);
          if (childMeta) return childMeta;
        }
      }
    }
    return null;
  };
  return walk(nodes);
};

const removeNodeById = (id: string) => {
  const meta = findNodeMeta(canvas.value, id);
  if (!meta) return;
  if (
    meta.parentRow &&
    meta.node.type === "layout" &&
    meta.node.component === "col"
  ) {
    if (meta.parentRow.columns.length <= 1) {
      ElMessage.warning("行至少保留一列");
      return;
    }
  }
  (meta.container as DesignerNode[]).splice(meta.index, 1);
  if (selectedId.value === id) selectedId.value = "";
};

const addColumn = (row: DesignerRowNode) => {
  row.columns.push(createColNode());
  rebalanceRowColumns(row);
};

const deleteColumn = (row: DesignerRowNode, column: DesignerColumnNode) => {
  if (row.columns.length <= 1) {
    ElMessage.warning("行至少保留一列");
    return;
  }
  row.columns = row.columns.filter((col) => col.id !== column.id);
  rebalanceRowColumns(row);
  if (selectedId.value === column.id) selectedId.value = "";
};

const onCanvasAdd = (evt: any) => {
  if (evt.from === evt.to) return;
  const node = canvas.value[evt.newIndex];
  if (!node) return;
  if (node.type === "layout" && node.component === "col") {
    // standalone columns should not be added to root
    (evt.to as HTMLElement).removeChild(evt.item);
    ElMessage.warning("请先拖入行组件，再添加列");
    canvas.value.splice(evt.newIndex, 1);
    return;
  }
  if (node.type === "field") {
    selectedId.value = node.id;
  }
};

const onColumnChildrenAdd = (evt: any, column: DesignerColumnNode) => {
  const node = column.children[evt.newIndex];
  if (!node) return;
  if (node.type === "layout" && node.component === "col") {
    ElMessage.warning("列不能嵌套在字段区域");
    column.children.splice(evt.newIndex, 1);
    return;
  }
  selectedId.value = node.id;
};

const onColumnsAdd = (evt: any, row: DesignerRowNode) => {
  const node = row.columns[evt.newIndex];
  if (!node) return;
  if (node.type === "layout" && node.component === "col") {
    rebalanceRowColumns(row);
    selectedId.value = node.id;
    return;
  }

  if (node.type === "field") {
    const fieldNode = node as DesignerFieldNode;
    row.columns.splice(evt.newIndex, 1);
    const column = createColNode();
    column.children.push(fieldNode);
    row.columns.splice(evt.newIndex, 0, column);
    rebalanceRowColumns(row);
    if (evt.item && evt.item.parentElement) {
      evt.item.parentElement.removeChild(evt.item);
    }
    selectedId.value = fieldNode.id;
    return;
  }

  row.columns.splice(evt.newIndex, 1);
  ElMessage.warning("行只能包含列组件");
};

const deleteNode = (node: DesignerNode) => {
  removeNodeById(node.id);
};

const ensureOptionsItems = (node: DesignerFieldNode) => {
  ensureOptions(node);
  if (!node.config.options) node.config.options = {};
  if (
    !Array.isArray(node.config.options.items) ||
    !node.config.options.items.length
  ) {
    node.config.options.items = [
      { label: "选项一", value: "1" },
      { label: "选项二", value: "2" },
    ];
  }
};

const addOption = (node: DesignerFieldNode) => {
  ensureOptionsItems(node);
  node.config.options!.items!.push({ label: "新选项", value: `${Date.now()}` });
};

const removeOption = (node: DesignerFieldNode, index: number) => {
  node.config.options!.items!.splice(index, 1);
};

const setRequired = (node: DesignerFieldNode, value: boolean) => {
  let rules = node.config.rules;
  if (!Array.isArray(rules) && rules) {
    rules = [rules];
  }
  if (!rules) rules = [];
  const idx = rules.findIndex((rule: any) =>
    Object.prototype.hasOwnProperty.call(rule, "required")
  );
  if (value) {
    if (idx > -1) {
      (rules[idx] as any).required = true;
      if (!(rules[idx] as any).message)
        (rules[idx] as any).message = `${
          node.config.label || "该字段"
        }不能为空`;
    } else {
      rules.push({
        required: true,
        message: `${node.config.label || "该字段"}不能为空`,
        trigger: "blur",
      });
    }
  } else if (idx > -1) {
    rules.splice(idx, 1);
  }
  node.config.rules = rules;
};

const isRequired = (node: DesignerFieldNode) => {
  let rules = node.config.rules;
  if (!rules) return false;
  if (!Array.isArray(rules)) rules = [rules];
  return rules.some(
    (rule: any) =>
      Object.prototype.hasOwnProperty.call(rule, "required") && rule.required
  );
};

const selectedMeta = computed(() => {
  if (!selectedId.value) return null;
  return findNodeMeta(canvas.value, selectedId.value);
});

const selectedNode = computed(() => selectedMeta.value?.node ?? null);

const generateFormSchema = (nodes: DesignerCanvas): FormConfig => {
  const formItems: FormItemConfig[] = [];

  const traverse = (list: DesignerCanvas | DesignerNode[], parentSpan = 24) => {
    list.forEach((node) => {
      if (node.type === "layout" && node.component === "row") {
        const row = node as DesignerRowNode;
        row.columns.forEach((col) => {
          const colSpan = col.config.span;
          traverse(col.children, colSpan);
        });
      } else if (node.type === "layout" && node.component === "col") {
        const colNode = node as DesignerColumnNode;
        traverse(colNode.children, colNode.config.span);
      } else if (node.type === "field") {
        if (node.component === "divider") {
          formItems.push({
            component: "title",
            label: node.config.label,
            span: (node.config.span as number) || parentSpan,
          });
        } else {
          const item: FormItemConfig = {
            ...cloneDeep(node.config),
            component: node.component,
            span: (node.config.span as number) || parentSpan,
          };
          formItems.push(item);
        }
      }
    });
  };

  traverse(nodes);

  return {
    labelWidth: 120,
    gutter: 16,
    formItems,
  };
};

const previewSchema = computed(() => {
  const schema = generateFormSchema(canvas.value);
  schema.labelWidth = formSettings.labelWidth;
  schema.labelPosition = formSettings.labelPosition;
  schema.gutter = formSettings.gutter;
  (schema as any).title = formSettings.title;
  return schema;
});

// 将当前设计转换为可持久化结构
const buildPersistPayload = () => {
  return {
    // 设计器画布 & 配置（用于后续编辑）
    canvas: cloneDeep(canvas.value),
    formSettings: cloneDeep(formSettings),
    // 运行时表单配置：直接给 SoaForm 使用
    formConfig: previewSchema.value,
  };
};

// 从持久化结构还原设计
const applyPersistPayload = (schemaJson?: string | null) => {
  ignoreHistory.value = true;
  try {
    if (!schemaJson) {
      canvas.value = [];
      formSettings.title = "动态表单";
      formSettings.labelPosition = "right";
      formSettings.labelWidth = 90;
      formSettings.gutter = 16;
      resetHistoryWithCurrentCanvas();
      selectedId.value = "";
      return;
    }
    const payload = JSON.parse(schemaJson);
    if (Array.isArray(payload)) {
      // 兼容旧格式：直接是 canvas
      canvas.value = payload as DesignerCanvas;
    } else {
      if (Array.isArray(payload.canvas)) {
        canvas.value = payload.canvas as DesignerCanvas;
      }
      if (payload.formSettings) {
        formSettings.title = payload.formSettings.title ?? formSettings.title;
        formSettings.labelPosition =
          payload.formSettings.labelPosition ?? formSettings.labelPosition;
        formSettings.labelWidth =
          payload.formSettings.labelWidth ?? formSettings.labelWidth;
        formSettings.gutter =
          payload.formSettings.gutter ?? formSettings.gutter;
      }
    }
    resetHistoryWithCurrentCanvas();
    selectedId.value = "";
  } catch {
    ElMessage.error("解析表单设计数据失败，已使用空白表单");
    canvas.value = [];
    resetHistoryWithCurrentCanvas();
    selectedId.value = "";
  } finally {
    ignoreHistory.value = false;
  }
};

const exportJson = async () => {
  const payload = JSON.stringify(canvas.value, null, 2);
  try {
    await navigator.clipboard.writeText(payload);
    ElMessage.success("已复制表单结构");
    console.log("表单结构：", payload);
  } catch (error) {
    await ElMessageBox.alert(
      `<pre style="text-align:left">${payload}</pre>`,
      "表单结构",
      {
        confirmButtonText: "关闭",
        dangerouslyUseHTMLString: true,
        customClass: "design-form-export-dialog",
      }
    );
  }
};

const resetDesigner = () => {
  ElMessageBox.confirm("确定清空设计区吗？", "提示", { type: "warning" })
    .then(() => {
      canvas.value = [];
      selectedId.value = "";
      resetHistoryWithCurrentCanvas();
    })
    .catch(() => undefined);
};

const optionEditableComponents = ["select", "radio", "checkboxGroup"];

const leftPaneSize = ref("24%");
const rightPaneSize = ref("35%");
const activeRightTab = ref<"property" | "form" | "preview">("property");

const formSettings = reactive({
  title: "动态表单",
  labelPosition: "right" as FormConfig["labelPosition"],
  labelWidth: 90,
  gutter: 16,
});

const handleNodeClick = (node: DesignerNode) => {
  setSelected(node.id);
};

const addRow = () => {
  canvas.value.push(createRowNode());
};

const addField = (componentKey: string) => {
  const template = basicComponents.find(
    (item) => item.component === componentKey
  );
  if (!template) return;
  canvas.value.push(createFieldNode(template));
};

const onInnerSplitUpdate = (percentages: number[]) => {
  if (Array.isArray(percentages) && percentages.length >= 2) {
    rightPaneSize.value = `${percentages[1]}%`;
  }
};

// -------- 表单定义列表 / 当前表单操作（wf_form） --------

const loadFormList = async () => {
  const list = await getWorkflowFormList({
    tenantId: tenantId.value,
    keyword: "",
  });
  formList.value = list ?? [];
};

const loadFormDetail = async (id: number) => {
  const form = await getWorkflowForm(id);
  if (!form) {
    ElMessage.warning("表单不存在或已被删除");
    return;
  }
  currentForm.value = form;
  currentFormId.value = form.id ?? null;
  applyPersistPayload(form.schemaJson);
};

const handleCreateForm = () => {
  currentForm.value = {
    tenantId: tenantId.value,
    status: 0,
    name: "",
    code: "",
  };
  currentFormId.value = null;
  applyPersistPayload(null);
};

const handleSaveForm = async () => {
  if (!currentForm.value) {
    currentForm.value = {
      tenantId: tenantId.value,
      status: 0,
      name: formSettings.title || "未命名表单",
      code: "",
    };
  }

  const form = currentForm.value;

  if (!form.name || !form.name.trim()) {
    ElMessage.warning("请填写表单名称");
    return;
  }
  if (!form.code || !form.code.trim()) {
    form.code = `FORM_${Date.now()}`;
  }

  const payload = buildPersistPayload();
  const dto: WorkflowFormDto = {
    ...form,
    tenantId: tenantId.value,
    schemaJson: JSON.stringify(payload),
  };

  if (!form.id) {
    const newId = await createWorkflowForm(dto);
    form.id = newId;
    currentFormId.value = newId;
    ElMessage.success("表单已创建");
  } else {
    await updateWorkflowForm(dto);
    ElMessage.success("表单已保存");
  }

  await loadFormList();
};

const handleDeleteForm = async () => {
  if (!currentFormId.value) return;
  try {
    await ElMessageBox.confirm("确定删除当前表单吗？", "提示", {
      type: "warning",
    });
  } catch {
    return;
  }
  await deleteWorkflowForm(currentFormId.value);
  ElMessage.success("删除成功");
  currentForm.value = null;
  currentFormId.value = null;
  applyPersistPayload(null);
  await loadFormList();
};

watch(currentFormId, (id) => {
  if (!id) {
    currentForm.value = null;
    applyPersistPayload(null);
    return;
  }
  loadFormDetail(id);
});

onMounted(() => {
  loadFormList();
});
</script>

<template>
  <div class="h-full design-form-pag">
    <el-header
      class="bg-card border-border flex flex-wrap items-center justify-between gap-3 rounded-[.5vw]"
    >
      <el-form
        :inline="true"
        label-width="80px"
        class="mb-0 flex flex-wrap items-center gap-3"
      >
        <el-form-item label="表单名称" style="margin-right: 0px">
          <el-input
            v-model="formName"
            placeholder="请输入表单名称"
            style="width: 180px"
          />
        </el-form-item>
        <el-form-item label="表单">
          <el-select
            v-model="currentFormId"
            placeholder="选择或新建表单"
            clearable
            filterable
            style="width: 160px"
          >
            <el-option
              v-for="item in formList"
              :key="item.id"
              :label="item.name || item.code || item.id"
              :value="item.id"
            />
          </el-select>
        </el-form-item>
        <el-form-item>
          <el-button type="primary" @click="handleCreateForm">
            新建表单
          </el-button>
          <el-button type="success" @click="handleSaveForm">
            保存当前设计
          </el-button>
          <el-button
            type="danger"
            :disabled="!currentFormId"
            @click="handleDeleteForm"
          >
            删除当前
          </el-button>
        </el-form-item>
      </el-form>
    </el-header>

    <div class="design-toolbar mt-2 mb-2">
      <el-space>
        <el-button :icon="Sort" type="primary" @click="addRow"
          >添加行</el-button
        >
        <el-button :icon="Plus" @click="() => addField('input')"
          >添加输入框</el-button
        >
        <el-button
          :icon="RefreshLeft"
          :disabled="history.length <= 1"
          @click="undo"
          >撤销</el-button
        >
        <el-button :icon="RefreshRight" :disabled="!future.length" @click="redo"
          >重做</el-button
        >
        <el-button @click="exportJson">导出 JSON</el-button>
        <el-button type="danger" @click="resetDesigner">清空</el-button>
      </el-space>
    </div>
    <div class="design-body">
      <el-splitter class="design-splitter" direction="horizontal">
        <el-splitter-panel :size="leftPaneSize" :min="200">
          <aside class="design-panel pr-1">
            <el-card shadow="hover" class="design-panel-card pb-10">
              <template #header>
                <span
                  ><el-icon><Collection /></el-icon> 基础组件</span
                >
              </template>
              <Draggable
                :list="basicComponents"
                :clone="cloneFromPalette"
                item-key="label"
                :sort="false"
                :group="{ name: 'form-components', pull: 'clone', put: false }"
              >
                <template #item="{ element }">
                  <div class="palette-item">
                    <el-icon :size="16">
                      <component :is="element.icon" />
                    </el-icon>
                    <span>{{ element.label }}</span>
                  </div>
                </template>
              </Draggable>
            </el-card>
            <el-card shadow="hover" class="design-panel-card">
              <template #header>
                <span
                  ><el-icon><Grid /></el-icon> 布局组件</span
                >
              </template>
              <Draggable
                :list="layoutComponents"
                :clone="cloneFromPalette"
                item-key="component"
                :sort="false"
                :group="{
                  name: 'layout-components',
                  pull: 'clone',
                  put: false,
                }"
              >
                <template #item="{ element }">
                  <div class="palette-item">
                    <el-icon :size="16">
                      <component :is="element.icon" />
                    </el-icon>
                    <span>{{ element.label }}</span>
                  </div>
                </template>
              </Draggable>
            </el-card>
          </aside>
        </el-splitter-panel>
        <el-splitter-panel>
          <el-splitter
            class="design-splitter__inner"
            direction="horizontal"
            @updated="onInnerSplitUpdate"
          >
            <el-splitter-panel class="pl-3 pr-1">
              <main class="design-canvas">
                <div class="design-canvas__placeholder" v-if="!canvas.length">
                  <el-icon><Plus /></el-icon>
                  拖拽组件到此处开始设计
                </div>
                <Draggable
                  v-model="canvas"
                  item-key="id"
                  class="design-canvas__list"
                  :group="{ name: 'form-components', pull: true, put: true }"
                  :clone="cloneFromPalette"
                  @add="onCanvasAdd"
                >
                  <template #item="{ element }">
                    <div
                      :class="[
                        'designer-node',
                        element.component === 'row'
                          ? 'designer-node--row'
                          : 'designer-node--field',
                        { 'is-selected': selectedId === element.id },
                      ]"
                      @click.stop="handleNodeClick(element as DesignerNode)"
                    >
                      <template v-if="element.component === 'row'">
                        <div class="designer-row__header">
                          <span
                            >行布局 (gutter: {{ element.config.gutter }})</span
                          >
                          <div class="designer-actions">
                            <el-button
                              type="text"
                              size="small"
                              @click.stop="
                                addColumn(element as DesignerRowNode)
                              "
                              >添加列</el-button
                            >
                            <el-button
                              type="text"
                              size="small"
                              @click.stop="deleteNode(element as DesignerNode)"
                              >删除</el-button
                            >
                          </div>
                        </div>
                        <Draggable
                          v-model="(element as DesignerRowNode).columns"
                          class="designer-row__columns"
                          item-key="id"
                          :group="{
                            name: 'layout-components',
                            pull: true,
                            put: true,
                          }"
                          @add="
                            (evt) =>
                              onColumnsAdd(evt, element as DesignerRowNode)
                          "
                        >
                          <template #item="{ element: column }">
                            <div
                              :class="[
                                'designer-column',
                                { 'is-selected': selectedId === column.id },
                              ]"
                              @click.stop="
                                handleNodeClick(column as DesignerNode)
                              "
                            >
                              <div class="designer-column__header">
                                <span>列 span: {{ column.config.span }}</span>
                                <div class="designer-actions">
                                  <el-button
                                    type="text"
                                    size="small"
                                    @click.stop="
                                      deleteColumn(
                                        element as DesignerRowNode,
                                        column as DesignerColumnNode
                                      )
                                    "
                                    >删除</el-button
                                  >
                                </div>
                              </div>
                              <Draggable
                                v-model="column.children"
                                item-key="id"
                                class="designer-column__body"
                                :group="{
                                  name: 'form-components',
                                  pull: true,
                                  put: true,
                                }"
                                :clone="cloneFromPalette"
                                @add="
                                  (evt) =>
                                    onColumnChildrenAdd(
                                      evt,
                                      column as DesignerColumnNode,
                                    )
                                "
                              >
                                <template #item="{ element: child }">
                                  <div
                                    :class="[
                                      'designer-child-node',
                                      {
                                        'is-selected': selectedId === child.id,
                                      },
                                    ]"
                                    @click.stop="
                                      handleNodeClick(child as DesignerNode)
                                    "
                                  >
                                    <div class="designer-child-node__title">
                                      <span>{{
                                        child.label || child.component
                                      }}</span>
                                      <el-button
                                        type="text"
                                        size="small"
                                        @click.stop="
                                          deleteNode(child as DesignerNode)
                                        "
                                        >删除</el-button
                                      >
                                    </div>
                                  </div>
                                </template>
                              </Draggable>
                            </div>
                          </template>
                        </Draggable>
                      </template>

                      <template v-else>
                        <div class="designer-field">
                          <div class="designer-field__title">
                            <span
                              >{{ element.config.label }} ({{
                                element.component
                              }})</span
                            >
                            <div class="designer-actions">
                              <el-button
                                type="text"
                                size="small"
                                @click.stop="
                                  deleteNode(element as DesignerNode)
                                "
                              >
                                删除
                              </el-button>
                            </div>
                          </div>
                        </div>
                      </template>
                    </div>
                  </template>
                </Draggable>
              </main>
            </el-splitter-panel>
            <el-splitter-panel :size="rightPaneSize" :min="200">
              <aside class="design-panel pl-3">
                <el-card shadow="hover" class="design-panel-card">
                  <el-tabs v-model="activeRightTab" class="design-tabs" stretch>
                    <el-tab-pane name="form" label="表单属性">
                      <div class="design-tab-pane">
                        <el-form
                          label-width="96px"
                          label-position="left"
                          class="property-form"
                        >
                          <el-form-item label="表单标题">
                            <el-input v-model="formSettings.title" />
                          </el-form-item>
                          <el-form-item label="标签位置">
                            <el-select v-model="formSettings.labelPosition">
                              <el-option label="左对齐" value="left" />
                              <el-option label="右对齐" value="right" />
                              <el-option label="顶部" value="top" />
                            </el-select>
                          </el-form-item>
                          <el-form-item label="标签宽度">
                            <el-input-number
                              v-model="formSettings.labelWidth"
                              :min="50"
                              :max="200"
                            />
                          </el-form-item>
                          <el-form-item label="表单栅格间距">
                            <el-input-number
                              v-model="formSettings.gutter"
                              :min="0"
                              :max="40"
                            />
                          </el-form-item>
                        </el-form>
                      </div>
                    </el-tab-pane>
                    <el-tab-pane name="property" label="组件属性">
                      <div class="design-tab-pane">
                        <template v-if="selectedNode">
                          <el-form
                            label-width="96px"
                            label-position="left"
                            class="property-form"
                          >
                            <template
                              v-if="
                                selectedNode.type === 'layout' &&
                                selectedNode.component === 'row'
                              "
                            >
                              <el-form-item label="行间距">
                                <el-slider
                                  v-model="
                                    (selectedNode as DesignerRowNode).config
                                      .gutter
                                  "
                                  :max="40"
                                  :min="0"
                                />
                              </el-form-item>
                              <el-form-item label="对齐方式">
                                <el-select
                                  v-model="
                                    (selectedNode as DesignerRowNode).config
                                      .justify
                                  "
                                >
                                  <el-option label="start" value="start" />
                                  <el-option label="center" value="center" />
                                  <el-option label="end" value="end" />
                                  <el-option
                                    label="space-around"
                                    value="space-around"
                                  />
                                  <el-option
                                    label="space-between"
                                    value="space-between"
                                  />
                                </el-select>
                              </el-form-item>
                              <el-form-item label="纵向对齐">
                                <el-select
                                  v-model="
                                    (selectedNode as DesignerRowNode).config
                                      .align
                                  "
                                >
                                  <el-option label="top" value="top" />
                                  <el-option label="middle" value="middle" />
                                  <el-option label="bottom" value="bottom" />
                                </el-select>
                              </el-form-item>
                            </template>

                            <template
                              v-else-if="
                                selectedNode.type === 'layout' &&
                                selectedNode.component === 'col'
                              "
                            >
                              <el-form-item label="列宽 span">
                                <el-input-number
                                  v-model="
                                    (selectedNode as DesignerColumnNode).config
                                      .span
                                  "
                                  :min="1"
                                  :max="24"
                                />
                              </el-form-item>
                            </template>

                            <template v-else-if="selectedNode.type === 'field'">
                              <el-form-item label="标签">
                                <el-input
                                  v-model="
                                    (selectedNode as DesignerFieldNode).config
                                      .label
                                  "
                                />
                              </el-form-item>
                              <el-form-item
                                v-if="selectedNode.component !== 'divider'"
                                label="字段名"
                              >
                                <el-input
                                  v-model="
                                    (selectedNode as DesignerFieldNode).config
                                      .name
                                  "
                                />
                              </el-form-item>
                              <el-form-item
                                v-if="selectedNode.component !== 'divider'"
                                label="栅格跨度"
                              >
                                <el-input-number
                                  v-model="
                                    (selectedNode as DesignerFieldNode).config
                                      .span
                                  "
                                  :min="1"
                                  :max="24"
                                />
                              </el-form-item>
                              <el-form-item
                                v-if="
                                  selectedNode.component === 'input' ||
                                  selectedNode.component === 'date' ||
                                  selectedNode.component === 'select'
                                "
                                label="占位提示"
                              >
                                <el-input
                                  v-model="
                                    (selectedNode as DesignerFieldNode).config
                                      .options!.placeholder
                                  "
                                />
                              </el-form-item>
                              <el-form-item
                                v-if="
                                  selectedNode.component !== 'divider' &&
                                  selectedNode.component !== 'upload'
                                "
                                label="默认值"
                              >
                                <el-input
                                  v-if="
                                    !Array.isArray(
                                      (selectedNode as DesignerFieldNode).config
                                        .value,
                                    )
                                  "
                                  v-model="
                                    (selectedNode as DesignerFieldNode).config
                                      .value as any
                                  "
                                />
                                <el-input
                                  v-else
                                  v-model="
                                    (selectedNode as DesignerFieldNode).config
                                      .value as any
                                  "
                                  placeholder="数组值请在 JSON 中修改"
                                  disabled
                                />
                              </el-form-item>
                              <el-form-item
                                v-if="selectedNode.component !== 'divider'"
                                label="必填"
                              >
                                <el-switch
                                  :model-value="
                                    isRequired(
                                      selectedNode as DesignerFieldNode,
                                    )
                                  "
                                  @change="
                                    (val: boolean) => {
                                      setRequired(
                                        selectedNode as DesignerFieldNode,
                                        val,
                                      );
                                    }
                                  "
                                />
                              </el-form-item>

                              <template
                                v-if="
                                  optionEditableComponents.includes(
                                    selectedNode.component
                                  )
                                "
                              >
                                <div class="property-options">
                                  <div class="property-options__header">
                                    <span>选项列表</span>
                                    <el-button
                                      type="primary"
                                      size="small"
                                      @click="
                                        () => {
                                          addOption(
                                            selectedNode as DesignerFieldNode,
                                          );
                                        }
                                      "
                                      >新增选项</el-button
                                    >
                                  </div>
                                  <el-table
                                    :data="
                                      (selectedNode as DesignerFieldNode).config
                                        .options!.items || []
                                    "
                                    border
                                    size="small"
                                  >
                                    <el-table-column
                                      label="标签"
                                      prop="label"
                                      min-width="120"
                                    >
                                      <template #default="{ row }">
                                        <el-input
                                          v-model="row.label"
                                          size="small"
                                        />
                                      </template>
                                    </el-table-column>
                                    <el-table-column
                                      label="值"
                                      prop="value"
                                      min-width="120"
                                    >
                                      <template #default="{ row }">
                                        <el-input
                                          v-model="row.value"
                                          size="small"
                                        />
                                      </template>
                                    </el-table-column>
                                    <el-table-column
                                      label="操作"
                                      width="80"
                                      align="center"
                                    >
                                      <template #default="{ $index }">
                                        <el-button
                                          type="text"
                                          size="small"
                                          @click="
                                            () => {
                                              removeOption(
                                                selectedNode as DesignerFieldNode,
                                                $index,
                                              );
                                            }
                                          "
                                          >删除</el-button
                                        >
                                      </template>
                                    </el-table-column>
                                  </el-table>
                                </div>
                              </template>
                            </template>
                          </el-form>
                        </template>
                        <template v-else>
                          <div class="property-empty">
                            选择设计区组件以编辑属性
                          </div>
                        </template>
                      </div>
                    </el-tab-pane>
                    <el-tab-pane name="preview" label="预览">
                      <div class="design-tab-pane preview-pane">
                        <SoaForm :config="previewSchema" :model-value="{}" />
                      </div>
                    </el-tab-pane>
                  </el-tabs>
                </el-card>
              </aside>
            </el-splitter-panel>
          </el-splitter>
        </el-splitter-panel>
      </el-splitter>
    </div>
  </div>
</template>

<style scoped>
.design-form-page {
  display: flex;
  flex-direction: column;
  height: 100%;
  gap: 12px;
}

.design-toolbar {
  padding: 10px 16px;
  border-radius: 8px;
  background: var(--el-bg-color);
  box-shadow: 0 1px 4px rgba(15, 23, 42, 0.08);
}

.design-body {
  flex: 1;
  min-height: 0;
}

.design-splitter,
.design-splitter__inner {
  height: 100%;
}

.design-splitter :deep(.el-splitter__panel),
.design-splitter__inner :deep(.el-splitter__panel) {
  overflow: hidden;
}

.design-splitter :deep(.el-splitter__trigger),
.design-splitter__inner :deep(.el-splitter__trigger) {
  background-color: var(--el-border-color-lighter);
}

.design-panel {
  display: flex;
  flex-direction: column;
  gap: 12px;
  overflow: hidden;
  height: 100%;
}

.design-panel--left,
.design-panel--right {
  padding: 0 8px;
}

.design-panel-card {
  flex: 1;
  overflow: hidden;
}

.design-panel-card :deep(.el-card__body) {
  height: 100%;
  overflow: auto;
}

.design-tabs {
  display: flex;
  flex-direction: column;
  height: 100%;
}

.design-tabs :deep(.el-tabs__header) {
  margin-bottom: 8px;
}

.design-tabs :deep(.el-tabs__content) {
  flex: 1;
  height: 100%;
}

.design-tabs :deep(.el-tab-pane) {
  height: 100%;
  display: flex;
}

.design-tab-pane {
  height: 100%;
  width: 100%;
  overflow: auto;
  padding: 12px;
  box-sizing: border-box;
}

.preview-pane {
  background: var(--el-fill-color-lighter);
  border-radius: 8px;
  padding: 12px;
}

.palette-item {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 8px 10px;
  border: 1px dashed transparent;
  border-radius: 6px;
  background: var(--el-fill-color-lighter);
  margin-bottom: 8px;
  cursor: grab;
  transition: border-color 0.2s ease, background 0.2s ease;
}

.palette-item:hover {
  border-color: var(--el-color-primary);
  background: rgba(var(--el-color-primary-rgb), 0.08);
}

.design-canvas {
  background: var(--el-bg-color);
  border-radius: 12px;
  padding: 12px;
  overflow: auto;
  box-shadow: inset 0 0 0 1px var(--el-border-color-lighter);
  height: 100vh;
  box-sizing: border-box;
}

.design-canvas__placeholder {
  height: 50px;
  border: 2px dashed var(--el-border-color);
  border-radius: 12px;
  color: var(--el-text-color-placeholder);
  display: flex;
  align-items: center;
  justify-content: center;
  flex-direction: column;
  gap: 6px;
  font-size: 13px;
}

.design-canvas__list {
  min-height: 100%;
}

.designer-node {
  border-radius: 8px;
  border: 1px solid var(--el-border-color);
  /* background: var(--el-color-white); */
  margin-bottom: 12px;
  transition: border-color 0.2s ease, box-shadow 0.2s ease;
  cursor: pointer;
}

.designer-node.is-selected {
  border-color: var(--el-color-primary);
  box-shadow: 0 0 0 1px rgba(var(--el-color-primary-rgb), 0.3);
}

.designer-node--field {
  padding: 12px;
}

.designer-field__title,
.designer-row__header,
.designer-column__header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  font-size: 13px;
  font-weight: 600;
  color: var(--el-text-color-primary);
}

.designer-row__header {
  padding: 10px 12px;
  border-bottom: 1px solid var(--el-border-color-lighter);
}

.designer-row__columns {
  display: flex;
  gap: 12px;
  padding: 12px;
}

.designer-column {
  flex: 1;
  min-width: 0;
  border: 1px dashed var(--el-border-color);
  border-radius: 8px;
  display: flex;
  flex-direction: column;
  background: var(--el-fill-color-lighter);
  transition: border-color 0.2s ease;
}

.designer-column.is-selected {
  border-color: var(--el-color-primary);
}

.designer-column__header {
  padding: 8px 10px;
  border-bottom: 1px dashed var(--el-border-color-lighter);
  font-size: 12px;
}

.designer-column__body {
  flex: 1;
  padding: 10px;
  display: flex;
  flex-direction: column;
  gap: 10px;
  min-height: 120px;
}

.designer-child-node {
  border: 1px dashed transparent;
  border-radius: 6px;
  padding: 8px;
  /* background: var(--el-color-white); */
  transition: border-color 0.2s ease, box-shadow 0.2s ease;
}

.designer-child-node.is-selected {
  border-color: var(--el-color-primary);
  box-shadow: 0 0 0 1px rgba(var(--el-color-primary-rgb), 0.3);
}

.designer-child-node__title {
  display: flex;
  justify-content: space-between;
  align-items: center;
  font-size: 13px;
}

.property-form {
  display: flex;
  flex-direction: column;
  gap: 6px;
}

.property-empty {
  height: 120px;
  display: flex;
  align-items: center;
  justify-content: center;
  color: var(--el-text-color-placeholder);
}

.property-options {
  margin-top: 12px;
}

.property-options__header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 6px;
}

.design-panel-card :deep(.el-table) {
  border-radius: 6px;
}

.design-panel-card :deep(.el-table th) {
  background: var(--el-fill-color-light);
}

.design-panel-card :deep(.el-table td) {
  /* background kept default */
}
</style>

<style>
.design-form-export-dialog pre {
  white-space: pre-wrap;
  word-break: break-word;
  font-size: 12px;
}
</style>
