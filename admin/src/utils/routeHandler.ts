import type { RouteRecordRaw, RouteMeta } from "vue-router";
import { appStorage, STORAGE_KEYS } from "@/utils/storage";
import { uuid } from "@/utils/tools";

/**
 * 后端返回的菜单节点结构（字段名可以按实际接口调整）
 * 要求至少包含：path / name / component? / redirect? / meta? / children?
 */
export interface BackendMenu {
  path: string;
  name: string;
  type:string;
  component?: string;
  redirect?: string;
  meta?: {
    title?: string;
    icon?: string;
    affixTab?: boolean;
    link: string;
  };
  children?: BackendMenu[];
}

// 通过 Vite 动态加载 views 目录下的页面组件
const viewModules = import.meta.glob("../views/**/*.vue");

function resolveViewComponent(component?: string) {
  if (!component) return undefined;

  // 例如：传入 "sys/role/index" -> ../views/sys/role/index.vue
  const normalized = component
    .replace(/^\/+/, "") // 去掉开头的 /
    .replace(/^views\//, "") // 去掉多余的 views/ 前缀
    .replace(/^src\/views\//, ""); // 去掉 src/views/ 前缀

  const filePath = `../views/${normalized}`;
  const loader = viewModules[filePath];

  if (!loader) {
    console.warn(
      `[routeHandler] 未找到对应视图组件: ${component} (解析为 ${filePath})`,
    );
    return undefined;
  }

  return loader;
}

/**
 * 内部递归方法：
 * - 循环处理后端菜单树，生成前端可用的 RouteRecordRaw[]
 * - 规则：
 *   - meta.affixTab => meta.fixed
 *   - meta.menuKey = 上层菜单的 name（顶层为自身 name）
 */
function buildRoutesFromMenus(
  menus: BackendMenu[],
): RouteRecordRaw[] {
  const result: RouteRecordRaw[] = [];

  menus.forEach((menu) => {
    const children = menu.children || [];

    const rawMeta = menu.meta || {};
    const meta: RouteMeta = {
      ...rawMeta,
      title: rawMeta.title || menu.name,
      icon: rawMeta.icon,
      menuKey:uuid(15)
    };

    // affixTab -> fixed
    if (Object.prototype.hasOwnProperty.call(rawMeta, "affixTab")) {
      const affix = !!(rawMeta as any).affixTab;
      (meta as any).fixed = affix;
      delete (meta as any).affixTab;
    }

    const route: RouteRecordRaw = {
      path: menu.path,
      name: menu.name,
      meta,
    };

    if (menu.redirect) {
      route.redirect = menu.redirect;
    }

    const view = resolveViewComponent(menu.component);
    if (view) {
      route.component = view;
    }

    if (children.length) {
      route.children = buildRoutesFromMenus(children);
    }

    result.push(route);
  });

  return result;
}

/**
 * 对外方法：
 * - 从本地存储读取 STORAGE_KEYS.ROUTER_MENU（后端返回的菜单）
 * - 调用内部递归方法处理后返回 RouteRecordRaw[]
 * - 如果没有菜单，返回空数组
 */
export function getServerDynamicRoutes(): RouteRecordRaw[] {
  const backendMenus =
    appStorage.get<BackendMenu[]>(STORAGE_KEYS.ROUTER_MENU, []) ?? [];

  if (!backendMenus.length) {
    return [];
  }

  try {
    return buildRoutesFromMenus(backendMenus);
  } catch (error) {
    console.error("[routeHandler] 构建服务端动态路由失败:", error);
    return [];
  }
}
