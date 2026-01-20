<template>
  <div class="table-select w-full">
    <el-select
      ref="selectRef"
      class="w-full"
      :multiple="multiple"
      :model-value="displayModel"
      value-key="value"
      :placeholder="placeholder"
      :teleported="teleported"
      :collapse-tags="collapseTags"
      :disabled="disabled"
      clearable
      @visible-change="onSelectVisible"
      @clear="onClearClick"
      @remove-tag="onRemoveTag"
    >
      <!-- 多选 tag（item 可能是字符串或对象，做兼容） -->
      <template #tag="{ item }" v-if="multiple">
        <el-tag
          v-if="item"
          closable
          @close="onRemoveByValue(typeof item === 'object' ? item.value : item)"
        >
          {{ typeof item === 'object' ? item.label : item }}
        </el-tag>
      </template>

      <!-- 下拉面板：搜索 + 表格 + 分页 + 操作 -->
      <template #empty>
        <div class="w-[760px] p-3" @mousedown.stop>
          <div class="mb-3 flex items-center gap-2">
            <el-input
              v-model="keyword"
              clearable
              :placeholder="searchPlaceholder"
              class="flex-1"
              @input="onSearchInput"
              @clear="onSearchClear"
            >
              <template #prefix
                ><el-icon><Search /></el-icon
              ></template>
            </el-input>
            <el-button :loading="loading" type="primary" @click="loadData(1)"
              >搜索</el-button
            >
          </div>

          <el-table
            :data="tableData"
            height="360"
            border
            v-loading="loading"
            :highlight-current-row="!multiple"
            ref="tableRef"
            @row-click="onRowClick"
          >
            <el-table-column
              v-if="multiple"
              type="selection"
              width="48"
              align="center"
              :selectable="rowSelectable"
              @select="onSelectionChange"
              @select-all="onSelectionChange"
            />
            <el-table-column v-else label="" width="48" align="center">
              <template #default="{ row }">
                <el-radio
                  :model-value="isPicked(row)"
                  :label="true"
                  @change="() => toggleSingle(row)"
                  ><span style="display: none"></span
                ></el-radio>
              </template>
            </el-table-column>

            <!-- 业务列交由父组件 -->
            <slot />
          </el-table>

          <div class="mt-3 flex items-center justify-between">
            <el-pagination
              v-model:current-page="page"
              v-model:page-size="pageSize"
              :total="total"
              :page-sizes="[10, 20, 50, 100]"
              layout="total, sizes, prev, pager, next, jumper"
              @size-change="onPageChange"
              @current-change="onPageChange"
            />
          </div>

          <div class="mt-3 flex justify-end gap-2">
            <el-button @click="clearSelection">清空</el-button>
            <el-button
              type="primary"
              :disabled="!pickedRows.length"
              @click="commitSelection"
              >确定</el-button
            >
          </div>
        </div>
      </template>
    </el-select>
  </div>
</template>

<script lang="ts" setup>
import { ref, shallowRef, nextTick, watch } from 'vue';
import type { TableInstance } from 'element-plus';
import { Search } from '@element-plus/icons-vue';

type Align = 'left' | 'center' | 'right';
type Row = Record<string, any>;
interface ColumnDef<T = any> {
  label: string;
  prop: keyof T & string;
  width?: string | number;
  minWidth?: string | number;
  align?: Align;
  fixed?: 'left' | 'right';
  slot?: string;
  formatter?: (row: T, column: any, cellValue: any, index: number) => any;
}
// FetchParams / FetchResult 仅作为参考类型，暂不在组件内部直接使用
// interface FetchParams {
//   page: number;
//   pageSize: number;
//   keyword: string;
// }
// interface FetchResult<T = any> {
//   items: T[];
//   total: number;
// }
interface LabeledValue {
  label: string;
  value: any;
}
interface ApiObj {
  get: (params: Record<string, any>) => Promise<{
    code: number;
    message?: string;
    data: { items: Row[]; totalItems: number | string };
  }>;
}

const props = withDefaults(
  defineProps<{
    /** v-model：外部传入/接收的就是对象或对象数组（不再支持纯 id 数组） */
    modelValue: Row | Row[] | null;

    /** 数据源：二选一；有 apiObj 优先远程，没有则用静态 data */
    apiObj?: ApiObj;
    data?: Row[];
    params?: Record<string, any>;
    columns?: ColumnDef[];

    /** 主键/展示字段/分页字段 */
    valueKey?: string; // 比如 'id'
    labelKey?: string; // 比如 'name' 或 'label'
    pageField?: string; // 默认 'page'
    pageSizeField?: string; // 默认 'limit'
    keywordField?: string; // 默认 'key'

    /** 展示名拼接（优先级高于 labelKey） */
    displayNames?: string[];

    /** 本地搜索字段 */
    filterFields?: string[];

    placeholder?: string;
    searchPlaceholder?: string;
    disabled?: boolean;
    pageSize?: number;
    teleported?: boolean;
    collapseTags?: boolean;
    multiple?: boolean;
    autoCloseOnSelect?: boolean;
    rowSelectable?: (row: Row, index: number) => boolean;
  }>(),
  {
    valueKey: 'id',
    labelKey: 'label',
    pageField: 'page',
    pageSizeField: 'limit',
    keywordField: 'key',
    placeholder: '请选择',
    searchPlaceholder: '输入关键字搜索',
    disabled: false,
    pageSize: 10,
    teleported: true,
    collapseTags: true,
    multiple: false,
    autoCloseOnSelect: false,
    rowSelectable: () => true,
    filterFields: () => ['label', 'id', 'name', 'title', 'text'],
  },
);

const emit = defineEmits<{
  (e: 'update:modelValue', v: Row | Row[] | null): void;
  (e: 'change', v: Row | Row[] | null): void;
  (e: 'select', payload: { rows: Row[] }): void;
  (e: 'visible-change', v: boolean): void;
  (e: 'search', keyword: string): void;
  (e: 'clear'): void;
}>();

/* refs / 状态 */
const selectRef = ref();
const tableRef = ref<TableInstance>();
const loading = ref(false);
const keyword = ref('');
const page = ref(1);
const pageSize = ref(props.pageSize);
const total = ref(0);
const tableData = ref<Row[]>([]);

const pickedRows = ref<Row[]>([]); // 下拉面板中“待提交”的勾选
const selectedRows = ref<Row[]>([]); // 已提交生效（就是父组件的 v-model）
const displayModel = shallowRef<LabeledValue[] | LabeledValue | ''>(''); // el-select 的可视数据

const rowSelectable = (row: Row, i: number) => props.rowSelectable!(row, i);

/* 生成展示名：displayNames 优先，其次 labelKey */
function buildLabel(row: Row) {
  if (props.displayNames?.length) {
    return props.displayNames
      .map((n) => row?.[n])
      .filter(Boolean)
      .join(' / ');
  }
  return row?.[props.labelKey!] ?? '';
}

/* 重建展示模型（避免 computed 带来的循环） */
function rebuildDisplayModel() {
  const arr = selectedRows.value.map((r) => ({
    label: buildLabel(r),
    value: r[props.valueKey!],
  }));
  // 多选空数组时返回 []，el-select 不会显示 tag
  displayModel.value = props.multiple ? arr : (arr[0] ?? '');
}

/* —— 可见性 —— */
async function onSelectVisible(v: boolean) {
  emit('visible-change', v);
  if (v) {
    await loadData(1);
    pickedRows.value = [...selectedRows.value];
    nextTick(() => syncTableCheckedFromPicked());
  }
}

/* 清空：顶部清空按钮 */
function onClearClick() {
  clearSelection(true);
  emit('clear');
}

/* 下拉内“清空”按钮（不关闭下拉） */
function clearSelection(fromTop = false) {
  pickedRows.value = [];
  selectedRows.value = [];
  // 取消当前页所有勾选
  tableData.value.forEach((row) =>
    tableRef.value?.toggleRowSelection(row, false),
  );
  syncOut(); // 对外同步（对象/对象数组）
  rebuildDisplayModel();
  if (!fromTop) {
    // 保持下拉打开，方便继续选择
  }
}

/* 移除单个 tag */
function onRemoveTag(item: any) {
  onRemoveByValue(typeof item === 'object' ? item.value : item);
}
function onRemoveByValue(val: any) {
  if (!props.multiple) return;
  const key = props.valueKey!;
  pickedRows.value = pickedRows.value.filter((r) => r[key] !== val);
  selectedRows.value = selectedRows.value.filter((r) => r[key] !== val);
  // 取消表格勾选（若在当前页）
  const inPage = tableData.value.find((r) => r[key] === val);
  if (inPage) tableRef.value?.toggleRowSelection(inPage, false);
  syncOut();
  rebuildDisplayModel();
}

/* —— 表格交互 —— */
function isPicked(row: Row) {
  return pickedRows.value.some(
    (r) => r[props.valueKey!] === row[props.valueKey!],
  );
}
function toggleSingle(row: Row) {
  pickedRows.value = [row];
  if (props.autoCloseOnSelect) commitSelection();
}
function onRowClick(row: Row) {
  if (props.multiple) {
    const key = props.valueKey!;
    const i = pickedRows.value.findIndex((r) => r[key] === row[key]);
    if (i >= 0) pickedRows.value.splice(i, 1);
    else pickedRows.value.push(row);
    tableRef.value?.toggleRowSelection(row, i < 0);
    if (props.autoCloseOnSelect) commitSelection();
  } else {
    toggleSingle(row);
  }
}
function onSelectionChange() {
  const sel = (tableRef.value as any)?.getSelectionRows?.() ?? [];
  pickedRows.value = sel;
}
function syncTableCheckedFromPicked() {
  const set = new Set(pickedRows.value.map((r) => r[props.valueKey!]));
  tableData.value.forEach((r) =>
    tableRef.value?.toggleRowSelection(r, set.has(r[props.valueKey!])),
  );
}

/* —— 提交 —— */
function commitSelection() {
  selectedRows.value = [...pickedRows.value]; // ✅ 确认后把对象作为最终值
  syncOut();
  rebuildDisplayModel();
  (selectRef.value as any)?.blur?.(); // 关闭下拉
}

/* —— 对外同步：始终回传“对象/对象数组/空” —— */
function syncOut() {
  const payload = props.multiple
    ? selectedRows.value.length
      ? [...selectedRows.value]
      : []
    : (selectedRows.value[0] ?? null);
  queueMicrotask(() => {
    emit('update:modelValue', payload);
    emit('change', payload);
    emit('select', { rows: [...selectedRows.value] });
  });
}

/* —— 反显：父传对象/对象数组时，直接用作 selectedRows —— */
function syncSelectedFromModel() {
  const mv = props.modelValue;
  if (mv == null) {
    selectedRows.value = [];
  } else if (Array.isArray(mv)) {
    selectedRows.value = mv.slice(); // 克隆对象数组
  } else if (typeof mv === 'object') {
    selectedRows.value = [mv];
  } else {
    // 如果父组件误传了纯 id，这里不处理；保持空，避免不一致
    selectedRows.value = [];
  }
  rebuildDisplayModel();
}

/* —— 搜索（防抖） —— */
let timer: any = null;
function onSearchInput() {
  emit('search', keyword.value);
  if (timer) clearTimeout(timer);
  timer = setTimeout(() => loadData(1), 300);
}
function onSearchClear() {
  loadData(1);
}

/* —— 加载数据：远程优先，否则静态数据（本地过滤+分页） —— */
async function loadData(toPage?: number) {
  loading.value = true;
  try {
    if (typeof toPage === 'number') page.value = toPage;

    if (props.apiObj?.get) {
      const res = await props.apiObj.get({
        [props.pageField!]: page.value,
        [props.pageSizeField!]: pageSize.value,
        [props.keywordField!]: keyword.value.trim() || null,
        ...(props.params || {}),
      });
      if (res.code !== 200) throw new Error(res.message || '加载失败');
      const list = (res.data?.items || []) as Row[];
      tableData.value = list;
      total.value = Number(res.data?.totalItems || list.length);
    } else {
      const all = (props.data || []) as Row[];
      const kw = keyword.value.trim().toLowerCase();
      const searchable = new Set([
        ...(props.filterFields || []),
        ...(props.columns?.map((c) => c.prop as string) || []),
      ]);
      const filtered = kw
        ? all.filter((row) =>
            Array.from(searchable).some((k) =>
              String(row?.[k] ?? '')
                .toLowerCase()
                .includes(kw),
            ),
          )
        : all;
      total.value = filtered.length;
      const start = (page.value - 1) * pageSize.value;
      tableData.value = filtered.slice(start, start + pageSize.value);
    }

    nextTick(() => {
      // 分页加载后：仅同步勾选态（反显不依赖当前页）
      syncTableCheckedFromPicked();
    });
  } catch (e: any) {
    ElMessage.error(e?.message || '加载失败');
    tableData.value = [];
    total.value = 0;
  } finally {
    loading.value = false;
  }
}
function onPageChange() {
  loadData();
}

/* 监听外部 v-model（受控）—— 父传对象/对象数组即可反显 */
watch(
  () => props.modelValue,
  () => nextTick(syncSelectedFromModel),
  { immediate: true },
);
</script>

<style scoped>
.table-select :deep(.el-select-dropdown__empty) {
  padding: 0;
}
</style>
