export type StorageType = "localStorage" | "sessionStorage";

export interface StorageManagerOptions {
  /**
   * 统一前缀，避免不同项目 / 模块间 key 冲突
   * 最终 key 形式为 `${prefix}:${key}`
   */
  prefix?: string;
  /**
   * 使用哪种浏览器存储，默认 localStorage
   */
  storageType?: StorageType;
}

/**
 * 封装对浏览器 Storage 的访问，统一处理：
 * - key 前缀
 * - JSON 序列化 / 反序列化
 * - SSR 场景 (window 未定义) 安全访问
 * - 仅清理当前应用前缀下的数据
 */
export class StorageManager {
  private prefix: string;
  private storage: Storage | null;

  constructor(options?: StorageManagerOptions) {
    this.prefix = options?.prefix ? `${options.prefix}:` : "fyt:";

    if (typeof window === "undefined") {
      // SSR / 非浏览器环境下，避免直接访问 window
      this.storage = null;
    } else {
      this.storage =
        options?.storageType === "sessionStorage"
          ? window.sessionStorage
          : window.localStorage;
    }
  }

  /**
   * 构造带前缀的 key
   */
  private buildKey(key: string): string {
    return `${this.prefix}${key}`;
  }

  /**
   * 设置值
   * - 自动 JSON 序列化
   * - value 为 null/undefined 时相当于 remove
   */
  set<T = unknown>(key: string, value: T | null | undefined): void {
    if (!this.storage) return;

    const fullKey = this.buildKey(key);

    if (value === null || value === undefined) {
      this.storage.removeItem(fullKey);
      return;
    }

    try {
      const payload = JSON.stringify(value);
      this.storage.setItem(fullKey, payload);
    } catch {
      // 如果序列化失败，降级为直接存 string
      // eslint-disable-next-line @typescript-eslint/no-base-to-string
      this.storage.setItem(fullKey, String(value));
    }
  }

  /**
   * 获取值
   * - 自动 JSON 反序列化
   * - 解析失败时返回 defaultValue
   */
  get<T = unknown>(key: string, defaultValue?: T): T | undefined {
    if (!this.storage) return defaultValue;

    const fullKey = this.buildKey(key);
    const raw = this.storage.getItem(fullKey);
    if (raw === null) return defaultValue;

    try {
      return JSON.parse(raw) as T;
    } catch {
      // 非 JSON 场景，直接返回原始字符串（如果类型声明为 string 更合适）
      return (raw as unknown) as T;
    }
  }

  /**
   * 删除单个 key
   */
  remove(key: string): void {
    if (!this.storage) return;
    const fullKey = this.buildKey(key);
    this.storage.removeItem(fullKey);
  }

  /**
   * 清空当前前缀下的所有 key，不影响其他业务的存储
   */
  clearAll(): void {
    if (!this.storage) return;
    const keysToRemove: string[] = [];
    const prefix = this.prefix;

    for (let i = 0; i < this.storage.length; i += 1) {
      const k = this.storage.key(i);
      if (k && k.startsWith(prefix)) {
        keysToRemove.push(k);
      }
    }

    keysToRemove.forEach((k) => this.storage!.removeItem(k));
  }
}

// 常用 key 统一定义，便于集中维护
export const STORAGE_KEYS = {
  ACCESS_TOKEN: "accessToken",
  REFRESH_TOKEN: "refreshToken",
  USER_INFO: "user-info",
  REMEMBER_ACCOUNT: "remember-account",
  THEME: "theme",
  PRIMARY_COLOR: "primary-color",
  SECONDARY_MENU_STATE: "secondary-menu-state",
  NAV_TAGS: "nav-tags",
  PERMISSIONS: "permissions",
  ROUTER_MENU: "router-menu",
  SCREEN_LOCK: "screen-lock",
} as const;

// 默认导出一个带应用前缀的 localStorage 管理器
export const appStorage = new StorageManager({
  prefix: "fyt-admin",
  storageType: "localStorage",
});
