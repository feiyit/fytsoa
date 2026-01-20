# soaChart 图表组件使用说明

封装基于 `echarts` + `vue-echarts` 的通用图表组件，提供：

- 通用渲染组件：`soaChart/index.vue`
- 简化配置工具：`soaChart/useSoaChart.ts`
  - `createPieChartOptions` 饼图
  - `createBarChartOptions` 柱状图
  - `createLineChartOptions` 折线图

组件自动根据当前主题（亮色 / 暗黑）调整配色。

> 说明：主题依赖于 `html` 根元素是否包含 `dark` class（和现有主题系统一致）。

---

## 1. 基础用法

### 1.1 引入组件

```ts
import SoaChart from "@/component/soaChart/index.vue";
import type { EChartsOption } from "echarts";

const options: EChartsOption = {
  // 标准 ECharts 配置
};
```

```vue
<SoaChart :options="options" :height="300" />
```

### 1.2 Props

- `options: EChartsOption`  
  必填，完整的 echarts 配置。

- `height?: number | string`  
  图表高度，默认 `300`（会转成 `300px`）。

- `width?: number | string`  
  图表宽度，默认 `"100%"`。

### 1.3 Events

- `@click="(params) => {}"`  
  图表点击事件，对应 ECharts 的 `click` 事件。

### 1.4 获取 ECharts 实例

```ts
const chartRef = ref<InstanceType<typeof SoaChart> | null>(null);

const resizeChart = () => {
  const chart = chartRef.value?.getInstance();
  chart?.resize();
};
```

```vue
<SoaChart ref="chartRef" :options="options" :height="280" />
<el-button @click="resizeChart">手动 resize</el-button>
```

---

## 2. 简化配置工具：useSoaChart

文件：`src/component/soaChart/useSoaChart.ts`

已封装三种常用图表：

- 饼图：`createPieChartOptions`
- 柱状图：`createBarChartOptions`
- 折线图：`createLineChartOptions`

内部会根据当前主题自动设置：

- 颜色组 `color`
- 标题 / 图例 / 坐标轴文字颜色
- 坐标轴线 / 分割线颜色

### 2.1 饼图：createPieChartOptions

```ts
import SoaChart from "@/component/soaChart/index.vue";
import { createPieChartOptions } from "@/component/soaChart/useSoaChart";

const pieOptions = computed(() =>
  createPieChartOptions({
    title: "访问来源",
    subtitle: "最近一周",
    seriesName: "访问量",
    legendPosition: "bottom", // top | bottom | left | right
    data: [
      { name: "搜索引擎", value: 1048 },
      { name: "直接访问", value: 735 },
      { name: "邮件营销", value: 580 },
      { name: "联盟广告", value: 484 },
      { name: "视频广告", value: 300 },
    ],
  }),
);
```

```vue
<SoaChart :options="pieOptions" :height="320" />
```

配置类型：

```ts
interface PieDataItem {
  name: string;
  value: number;
}

interface PieChartOptionsConfig {
  title?: string;
  subtitle?: string;
  seriesName?: string;
  data: PieDataItem[];
  legendPosition?: "top" | "bottom" | "left" | "right";
}

function createPieChartOptions(config: PieChartOptionsConfig): EChartsOption;
```

---

### 2.2 柱状图：createBarChartOptions

```ts
import SoaChart from "@/component/soaChart/index.vue";
import { createBarChartOptions } from "@/component/soaChart/useSoaChart";

const barOptions = computed(() =>
  createBarChartOptions({
    title: "月度销量",
    xAxis: ["一月", "二月", "三月", "四月", "五月", "六月"],
    series: [
      { name: "苹果", data: [120, 132, 101, 134, 90, 230] },
      { name: "香蕉", data: [220, 182, 191, 234, 290, 330] },
    ],
    stacked: false, // 是否堆叠
  }),
);
```

```vue
<SoaChart :options="barOptions" :height="300" />
```

配置类型：

```ts
interface BarSeriesItem {
  name: string;
  data: number[];
}

interface BarChartOptionsConfig {
  title?: string;
  xAxis: string[];
  series: BarSeriesItem[];
  stacked?: boolean;
}

function createBarChartOptions(config: BarChartOptionsConfig): EChartsOption;
```

---

### 2.3 折线图：createLineChartOptions

```ts
import SoaChart from "@/component/soaChart/index.vue";
import { createLineChartOptions } from "@/component/soaChart/useSoaChart";

const lineOptions = computed(() =>
  createLineChartOptions({
    title: "一周访问趋势",
    xAxis: ["周一", "周二", "周三", "周四", "周五", "周六", "周日"],
    series: [
      {
        name: "PV",
        data: [120, 132, 101, 134, 90, 230, 210],
      },
      {
        name: "UV",
        data: [80, 92, 91, 94, 60, 130, 150],
        smooth: false, // 默认 true，可按需关闭
      },
    ],
  }),
);
```

```vue
<SoaChart :options="lineOptions" :height="280" />
```

配置类型：

```ts
interface LineSeriesItem {
  name: string;
  data: (number | null)[];
  smooth?: boolean;
}

interface LineChartOptionsConfig {
  title?: string;
  xAxis: string[];
  series: LineSeriesItem[];
}

function createLineChartOptions(config: LineChartOptionsConfig): EChartsOption;
```

---

## 3. 主题联动

`useSoaChart` 内部通过 `document.documentElement.classList.contains("dark")` 判断当前是否为暗黑模式，并自动调整：

- 系列颜色 `color`
- 标题 / 图例文字颜色
- 坐标轴线 / 分割线颜色

当你使用已有的主题切换（设置 `html` 上的 `dark` class）时，新创建的图表会自动使用对应配色。  
如果需要在主题切换时实时刷新图表样式，建议在页面中监听主题变化，重新生成 options 并传给 `<SoaChart>`。

