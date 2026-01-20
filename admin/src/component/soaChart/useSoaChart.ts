import type { EChartsOption } from "echarts";

/** 根据当前主题（亮色 / 暗黑）返回一组颜色配置 */
function getThemeColors() {
  let isDark = false;
  if (typeof document !== "undefined") {
    isDark = document.documentElement.classList.contains("dark");
  }

  const lightPalette = [
    "#006ce6",
    "#12d092",
    "#f97316",
    "#f43f5e",
    "#6366f1",
    "#10b981",
  ];

  const darkPalette = [
    "#60a5fa",
    "#34d399",
    "#fb923c",
    "#fb7185",
    "#a5b4fc",
    "#4ade80",
  ];

  return {
    isDark,
    palette: isDark ? darkPalette : lightPalette,
    text: isDark ? "#e5e7eb" : "#374151",
    subText: isDark ? "#9ca3af" : "#6b7280",
    axisLine: isDark ? "#4b5563" : "#d1d5db",
    splitLine: isDark ? "#1f2937" : "#e5e7eb",
  };
}

/** 饼图数据项 */
export interface PieDataItem {
  name: string;
  value: number;
}

export interface PieChartOptionsConfig {
  title?: string;
  subtitle?: string;
  seriesName?: string;
  data: PieDataItem[];
  /** 图例位置 */
  legendPosition?: "top" | "bottom" | "left" | "right";
}

/** 简化配置：生成饼图 options */
export function createPieChartOptions(config: PieChartOptionsConfig): EChartsOption {
  const {
    title,
    subtitle,
    seriesName = "",
    data,
    legendPosition = "bottom",
  } = config;

  const theme = getThemeColors();

  const legend: EChartsOption["legend"] = {
    type: "scroll",
    bottom: legendPosition === "bottom" ? 0 : undefined,
    top: legendPosition === "top" ? 0 : undefined,
    left: legendPosition === "left" ? 0 : "center",
    right: legendPosition === "right" ? 0 : undefined,
    orient:
      legendPosition === "left" || legendPosition === "right"
        ? "vertical"
        : "horizontal",
    textStyle: {
      color: theme.text,
    },
  };

  const option: EChartsOption = {
    color: theme.palette,
    title: title
      ? {
          text: title,
          subtext: subtitle,
          left: "center",
          textStyle: {
            color: theme.text,
          },
          subtextStyle: {
            color: theme.subText,
          },
        }
      : undefined,
    tooltip: {
      trigger: "item",
      formatter: "{b}: {c} ({d}%)",
    },
    legend,
    series: [
      {
        name: seriesName,
        type: "pie",
        radius: "50%",
        data,
        emphasis: {
          itemStyle: {
            shadowBlur: 10,
            shadowOffsetX: 0,
            shadowColor: "rgba(0, 0, 0, 0.4)",
          },
        },
      },
    ],
  };

  return option;
}

/** 柱状图系列数据 */
export interface BarSeriesItem {
  name: string;
  data: number[];
}

export interface BarChartOptionsConfig {
  title?: string;
  xAxis: string[];
  series: BarSeriesItem[];
  stacked?: boolean;
}

/** 简化配置：生成柱状图 options */
export function createBarChartOptions(config: BarChartOptionsConfig): EChartsOption {
  const { title, xAxis, series, stacked = false } = config;

  const theme = getThemeColors();

  const option: EChartsOption = {
    color: theme.palette,
    title: title
      ? {
          text: title,
          left: "center",
          textStyle: {
            color: theme.text,
          },
        }
      : undefined,
    tooltip: {
      trigger: "axis",
      axisPointer: {
        type: "shadow",
      },
    },
    legend: {
      bottom: 0,
      textStyle: {
        color: theme.text,
      },
    },
    grid: {
      left: "3%",
      right: "3%",
      bottom: "12%",
      containLabel: true,
    },
    xAxis: {
      type: "category",
      data: xAxis,
      axisLine: {
        lineStyle: {
          color: theme.axisLine,
        },
      },
      axisLabel: {
        color: theme.text,
      },
    },
    yAxis: {
      type: "value",
      splitNumber: 4,
      axisLine: {
        lineStyle: {
          color: theme.axisLine,
        },
      },
      axisLabel: {
        color: theme.text,
      },
      splitLine: {
        lineStyle: {
          color: theme.splitLine,
        },
      },
    },
    series: series.map((s) => ({
      name: s.name,
      type: "bar",
      barMaxWidth: 30,
      data: s.data,
      stack: stacked ? "total" : undefined,
      emphasis: {
        focus: "series",
      },
    })),
  };

  return option;
}

/** 折线图系列数据 */
export interface LineSeriesItem {
  name: string;
  data: (number | null)[];
  smooth?: boolean;
}

export interface LineChartOptionsConfig {
  title?: string;
  xAxis: string[];
  series: LineSeriesItem[];
}

/** 简化配置：生成折线图 options */
export function createLineChartOptions(config: LineChartOptionsConfig): EChartsOption {
  const { title, xAxis, series } = config;

  const theme = getThemeColors();

  const option: EChartsOption = {
    color: theme.palette,
    title: title
      ? {
          text: title,
          left: "center",
          textStyle: {
            color: theme.text,
          },
        }
      : undefined,
    tooltip: {
      trigger: "axis",
    },
    legend: {
      bottom: 0,
      textStyle: {
        color: theme.text,
      },
    },
    grid: {
      left: "3%",
      right: "3%",
      bottom: "12%",
      containLabel: true,
    },
    xAxis: {
      type: "category",
      data: xAxis,
      axisLine: {
        lineStyle: {
          color: theme.axisLine,
        },
      },
      axisLabel: {
        color: theme.text,
      },
    },
    yAxis: {
      type: "value",
      splitNumber: 4,
      axisLine: {
        lineStyle: {
          color: theme.axisLine,
        },
      },
      axisLabel: {
        color: theme.text,
      },
      splitLine: {
        lineStyle: {
          color: theme.splitLine,
        },
      },
    },
    series: series.map((s) => ({
      name: s.name,
      type: "line",
      data: s.data,
      smooth: s.smooth ?? true,
      showSymbol: false,
      emphasis: {
        focus: "series",
      },
    })),
  };

  return option;
}
