<template>
  <div class="soa-table" ref="targetEl" v-loading="loading">
    <s-table
      v-model:selectedRowKeys="selectedRowKeys"
      ref="stableRef"
      :columns="columns"
      :data-source="data"
      :pagination="false"
      :row-key="rowKey"
      :size="config.size"
      :stripe="config.stripe"
      :bordered="config.bordered"
      :hover="config.hover"
      :showHeader="config.showHeader"
      highlight-selected
      defaultExpandAllRows
      v-bind="$attrs"
      v-on="$attrs"
      :row-selection="showSelection ? rowSelection : null"
      :height="tableHeight"
    >
      <template #bodyCell="scope">
        <slot name="bodyCell" v-bind="scope"></slot>
      </template>
      <slot></slot>
    </s-table>
    <div
      class="soaTable-footer flex justify-between justify-items-center"
      v-if="footerShow"
    >
      <el-pagination
        v-model:current-page="pageInfo.page"
        v-model:page-size="pageInfo.limit"
        :page-sizes="pageSizes"
        :size="size"
        :layout="paginationLayout"
        :total="total"
        v-if="paginationShow"
        @size-change="handleSizeChange"
        @current-change="handleCurrentChange"
      />
      <div class="footer-tool" v-if="toolbarShow">
        <el-button circle @click="loadData"
          ><Icon icon="lucide:refresh-ccw-dot"
        /></el-button>
        <el-button circle><Icon icon="lucide:file-x" /></el-button>
        <el-button circle><Icon icon="lucide:columns-settings" /></el-button>
        <el-popover
          title="表格设置"
          :width="500"
          trigger="click"
          :hide-after="0"
        >
          <template #reference>
            <el-button circle><Icon icon="lucide:settings" /></el-button>
          </template>
          <el-space direction="vertical" alignment="flex-start">
            <el-radio-group v-model="config.size">
              <el-radio-button label="small">小尺寸</el-radio-button>
              <el-radio-button label="default">默认尺寸</el-radio-button>
              <el-radio-button label="large">大尺寸</el-radio-button>
            </el-radio-group>
            <el-space :size="20">
              <el-checkbox v-model="config.stripe" label="显示斑马纹" />
              <el-checkbox v-model="config.bordered" label="显示表格边框" />
              <el-checkbox v-model="config.hover" label="显示悬浮效果" />
              <el-checkbox v-model="config.showHeader" label="显示表头" />
            </el-space>
          </el-space>
        </el-popover>
      </div>
    </div>
  </div>
</template>
<script setup lang="ts">
import {
  ref,
  reactive,
  computed,
  watch,
  onMounted,
  onUnmounted,
  defineProps,
  defineEmits,
  defineExpose,
} from "vue";
import { Icon } from "@iconify/vue";
import { SELECTION_ALL, SELECTION_INVERT, SELECTION_NONE } from "@shene/table";
import type { STableRowSelection } from "@shene/table";
import { changeTree } from "@/utils/tools";
interface PageInfo {
  page: number; // 当前页码
  limit: number; // 每页条数
}
const loading = ref(false);
const emit = defineEmits(["selection-change"]);
const targetEl = ref<HTMLDivElement | null>(null);
const props = defineProps({
  /** 表格数据源 */
  tableData: {
    type: Array as () => any[],
    default: () => [],
  },
  /** 表格列配置 */
  columns: {
    type: Array as () => any[],
    required: true,
  },
  rowKey: { type: String, default: "" },
  /** 加载状态 */
  loading: {
    type: Boolean,
    default: false,
  },
  /** 加载提示文本 */
  loadingText: {
    type: String,
    default: "加载中...",
  },
  /** 组件尺寸（同Element Plus尺寸） */
  size: {
    type: String as () => "default" | "small" | "large",
    default: "default",
  },
  /** 是否属性结构 */
  tree: {
    type: Boolean,
    default: false,
  },
  /** 是否隐藏底部工具栏 */
  footerShow: {
    type: Boolean,
    default: true,
  },
  /** 是否显示工具栏 */
  toolbarShow: {
    type: Boolean,
    default: true,
  },
  /** 是否显示分页 */
  paginationShow: {
    type: Boolean,
    default: true,
  },
  paginationLayout: {
    type: String,
    default: "total, sizes, prev, pager, next, jumper",
  },
  /** 分页尺寸选项 */
  pageSizes: {
    type: Array as () => number[],
    default: () => [20, 50, 100, 500, 1000, 3000],
  },
  /** 初始分页信息 */
  initialPageInfo: {
    type: Object as () => PageInfo,
    default: () => ({ page: 1, limit: 20 }),
  },
  /** 是否显示复选框列 */
  showSelection: {
    type: Boolean,
    default: true,
  },
  /** 是否单选 */
  singleSelect: {
    type: Boolean,
    default: false,
  },
  apiObj: { type: Function, default: () => {} },
  params: { type: Object, default: () => ({}) },
  tableProps: {
    type: Object as () => Partial<any>,
    default: () => ({}),
  },
  height: { type: String, default: "500px" },
});
const pageInfo = reactive<PageInfo>({ ...props.initialPageInfo });
const stableRef = ref(null);
const total = ref(0);
const data = ref<any>([]);
const config = ref({
  size: "default",
  stripe: false,
  bordered: true,
  hover: false,
  showHeader: true,
});
const isApiObjEmpty = () => {
  const funcStr = props.apiObj.toString().replace(/\s/g, "");
  return funcStr === "()=>{}" || funcStr === "function(){}";
};
const handleResize = () => {
  resizeTrigger.value++;
};
onMounted(() => {
  if (!isApiObjEmpty()) {
    loadData();
  } else if (props.tableData) {
    total.value = props.tableData.length;
    data.value = props.tableData;
  }
  window.addEventListener("resize", handleResize);
});
onUnmounted(() => {
  window.removeEventListener("resize", handleResize);
});
const resizeTrigger = ref(0);
const tableHeight = computed<any>(() => {
  resizeTrigger.value;
  if (targetEl.value && targetEl.value.parentElement) {
    let parentHeight = targetEl.value.parentElement.offsetHeight;
    if (parentHeight < 100) parentHeight = 500;
    if (!props.footerShow) {
      return `${Math.max(parentHeight - 20, 0)}px`;
    }
    const calculatedHeight = parentHeight - 60;
    return `${Math.max(calculatedHeight, 0)}px`;
  }
});

watch(
  () => props.initialPageInfo,
  (newVal) => {
    Object.assign(pageInfo, newVal);
  },
  { deep: true }
);
const loadData = async () => {
  loading.value = true;
  const reqData = {
    page: pageInfo.page,
    limit: pageInfo.limit,
  };
  Object.assign(reqData, props.params);
  reqData.page = pageInfo.page;
  const res = await props.apiObj(reqData);
  if (!props.tree) {
    data.value = res.items;
    total.value = parseInt(res.totalItems);
  } else {
    data.value = changeTree(res);
  }
  loading.value = false;
};

const refresh = () => {
  loadData();
};

const upData = (params: any, page: number = 1) => {
  pageInfo.page = page;
  Object.assign(props.params, params || {});
  loadData();
};

/** 每页条数变化 */
const handleSizeChange = (size: number) => {
  pageInfo.limit = size;
  loadData();
};

/** 当前页码变化 */
const handleCurrentChange = (num: number) => {
  pageInfo.page = num;
  loadData();
};
const selectedRowKeys = ref<(string | number)[]>([]);
const rowSelection = ref<STableRowSelection>({
  onChange: (selectedRowKeys, selectedRows) => {
    emit("selection-change", { selectedRowKeys, selectedRows });
  },
  type: props.singleSelect ? "radio" : "checkbox",
  allowCancelRadio: false,
  selections: [
    SELECTION_ALL,
    SELECTION_INVERT,
    SELECTION_NONE,
    {
      key: "odd",
      text: "选中奇数行",
      onSelect: (changableRowKeys) => {
        let newSelectedRowKeys = [];
        newSelectedRowKeys = changableRowKeys.filter((_key, index) => {
          if (index % 2 !== 0) {
            return false;
          }
          return true;
        });
        selectedRowKeys.value = newSelectedRowKeys;
      },
    },
    {
      key: "even",
      text: "选中偶数行",
      onSelect: (changableRowKeys) => {
        let newSelectedRowKeys = [];
        newSelectedRowKeys = changableRowKeys.filter((_key, index) => {
          if (index % 2 !== 0) {
            return true;
          }
          return false;
        });
        selectedRowKeys.value = newSelectedRowKeys;
      },
    },
  ],
});
defineExpose({
  stableRef,
  refresh,
  upData,
});
</script>
<style scoped>
.soa-table {
  height: 100%;
}
.soaTable-footer {
  padding-top: 8px;
}
</style>
