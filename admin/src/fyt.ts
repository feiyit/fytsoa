import type { App } from "vue";
import pinia from "./stores";
import { useAppStore } from "./stores/app";
import { setupAuthDirective } from "@/directives/auth";

/** 初始化全局主题 / 颜色 / 启动 Loading 等 */
export function setupFytBootstrap() {
  const appStore = useAppStore(pinia);

  // 初始化主题（跟随系统偏好）
  appStore.initTheme();
  // 初始化全局主色（Element Plus 主题色）
  appStore.initPrimaryColor();

  const handleWindowLoad = () => {
    // 所有静态资源（HTML、CSS、JS、图片等）加载完成后，关闭启动 loading
    appStore.setLoading(false);

    if (typeof window !== "undefined") {
      window.removeEventListener("load", handleWindowLoad);
    }
  };

  if (typeof window === "undefined") {
    handleWindowLoad();
    return;
  }

  if (window.document.readyState === "complete") {
    // 如果此时页面已经加载完成，直接关闭 loading
    handleWindowLoad();
  } else {
    // 等待浏览器触发 load 事件（包括 CSS/JS/图片 等资源）
    window.addEventListener("load", handleWindowLoad);
  }
}

/** 注册自定义指令等全局能力 */
export function setupFytDirectives(app: App) {
  setupAuthDirective(app);
}

/** 自动全局注册 src/component 目录下的所有组件 */
export function setupFytGlobalComponents(app: App) {
  // 通过 Vite 的 import.meta.glob 扫描并一次性引入所有组件
  const modules = import.meta.glob("./component/**/*.vue", { eager: true });

  Object.entries(modules).forEach(([path, mod]) => {
    const component = (mod as any).default;
    if (!component) return;

    // 优先使用组件本身的 name
    let name: string | undefined = component.name;

    // 如果组件没有显式 name，则根据路径推导一个
    if (!name) {
      // 示例路径：./component/soaTable/index.vue
      const segments = path.split("/");
      let file = segments.pop() || "";
      if (file.endsWith(".vue")) {
        file = file.replace(/\.vue$/, "");
      }
      if (file.toLowerCase() === "index" && segments.length) {
        // 使用上一级目录名作为组件名：soaTable / soaForm 等
        name = segments.pop();
      } else {
        name = file;
      }
    }

    if (!name) return;

    app.component(name, component);
  });
}
