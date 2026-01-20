<template>
  <VChart
    ref="chartRef"
    :option="options"
    :autoresize="true"
    :style="chartStyle"
    v-bind="$attrs"
    @click="handleClick"
  />
</template>

<script setup lang="ts">
import { use } from "echarts/core";
import { CanvasRenderer } from "echarts/renderers";
import { BarChart, LineChart, PieChart } from "echarts/charts";
import {
  GridComponent,
  TooltipComponent,
  LegendComponent,
  TitleComponent,
  DatasetComponent,
} from "echarts/components";
import VChart from "vue-echarts";
import type { EChartsOption } from "echarts";

// 注册常用图表类型和组件（只需执行一次）
use([
  CanvasRenderer,
  BarChart,
  LineChart,
  PieChart,
  GridComponent,
  TooltipComponent,
  LegendComponent,
  TitleComponent,
  DatasetComponent,
]);

interface SoaChartProps {
  /**
   * ECharts 配置项
   */
  options: EChartsOption;
  /**
   * 高度，支持 number（px）或任意 CSS 高度字符串
   * @default 300
   */
  height?: number | string;
  /**
   * 宽度，支持 number（px）或任意 CSS 宽度字符串
   * @default "100%"
   */
  width?: number | string;
}

const props = withDefaults(defineProps<SoaChartProps>(), {
  height: 300,
  width: "100%",
});

const emit = defineEmits<{
  (e: "click", params: any): void;
}>();

const chartRef = ref<InstanceType<typeof VChart> | null>(null);

const chartStyle = computed(() => {
  const normalize = (v: number | string) =>
    typeof v === "number" ? `${v}px` : v || "auto";

  return {
    height: normalize(props.height),
    width: normalize(props.width),
  };
});

const handleClick = (params: any) => {
  emit("click", params);
};

// 对外暴露获取 ECharts 实例的方法（可选）
const getInstance = () => {
  // vue-echarts 将 ECharts 实例暴露在组件实例的 chart 属性上
  return (chartRef.value as any)?.chart || null;
};

defineExpose({
  getInstance,
});
</script>

