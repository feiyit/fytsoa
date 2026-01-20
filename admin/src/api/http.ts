import axios, {
  type AxiosError,
  type AxiosInstance,
  type AxiosResponse,
  type InternalAxiosRequestConfig,
  type AxiosRequestConfig,
} from "axios";
import CryptoJS from "crypto-js";
import router from "@/router";
import { appStorage, STORAGE_KEYS } from "@/utils/storage";

/**
 * 后端统一返回格式约定
 * 例如：{ code: 200, data: T, message: string, status?: number }
 * 也兼容仅返回 { code, message } 的场景（data 可选）
 */
export interface ApiResponse<T = any> {
  code: number;
  data?: T;
  message?: string;
  status?: number;
}

// 请求头 token 配置，可在这里统一修改
const authHeaderName = "accessToken";
const authScheme = ""; // 例如：Bearer / Token / 自定义前缀

// 安全签名配置（从 Vite 环境变量中读取）
const SECURITY_APP_KEY = import.meta.env.VITE_SECURITY_APP_KEY as string | undefined;
const SECURITY_SIGN_KEY = import.meta.env.VITE_SECURITY_SIGN_KEY as string | undefined;

const getAccessToken = (): string | null => {
  return appStorage.get<string>(STORAGE_KEYS.ACCESS_TOKEN) ?? null;
};

const setAccessToken = (token: string | null) => {
  if (!token) {
    appStorage.remove(STORAGE_KEYS.ACCESS_TOKEN);
    return;
  }
  appStorage.set(STORAGE_KEYS.ACCESS_TOKEN, token);
};

const setRefreshToken = (token: string | null) => {
  if (!token) {
    appStorage.remove(STORAGE_KEYS.REFRESH_TOKEN);
    return;
  }
  appStorage.set(STORAGE_KEYS.REFRESH_TOKEN, token);
};

const clearAuthTokens = () => {
  setAccessToken(null);
  setRefreshToken(null);
};

const showErrorMessage = (message: string) => {
  if (!message) return;
  ElMessage.closeAll();
  ElMessage.error(message);
};

// 运行时配置：优先读取 public/runtime-config.js 中的 API_BASE_URL
const runtimeConfig =
  (typeof window !== "undefined" && (window as any).__APP_CONFIG__) || {};
const runtimeApiBase = runtimeConfig.API_BASE_URL as string | undefined;

// 内部 axios 实例：优先使用运行时配置，其次使用构建时环境变量
const http: AxiosInstance = axios.create({
  baseURL: runtimeApiBase || import.meta.env.VITE_API_BASE_URL,
  timeout: 10000,
});


/**
 * 内容区域：统一处理业务响应（code / data / message）
 */
const transformResponseData = <T = any>(
  response: AxiosResponse<unknown>,
): T | Promise<T> => {
  const raw = response.data as any;

  // 优先识别标准业务响应格式：只要包含 code 字段即可
  if (raw && typeof raw === "object" && "code" in raw) {
    const res = raw as ApiResponse<T>;
    if (res.code === 200) {
      // 成功：返回 data（可能为 undefined，由调用方自行约定）
      return res.data as T;
    }

    // 非 200 业务状态码，统一错误处理（例如：{ code: 302, message: "密码错误~" }）
    const msg =
      res.message ||
      (response.status ? `请求失败（HTTP ${response.status}）` : "请求失败");
    showErrorMessage(msg);
    return Promise.reject(res);
  }

  // 没有使用统一业务包装时，直接返回原始 data（兼容现有 mock 登录等场景）
  return raw as T;
};

/**
 * 构建用于签名的 data 字符串
 * - GET：对 params 按 key 排序后拼接 key+value
 * - 其它方法：使用请求体原文（字符串），或对对象 JSON.stringify
 */
const buildSignData = (config: InternalAxiosRequestConfig): string => {
  const method = (config.method || "get").toUpperCase();

  if (method === "GET") {
    const params = (config.params || {}) as Record<string, any>;
    const keys = Object.keys(params).sort();
    return keys
      .filter((k) => k)
      .map((k) => `${k}${params[k] ?? ""}`)
      .join("");
  }

  const data = config.data;
  if (!data) return "";
  if (typeof data === "string") return data;
  // 对象/数组等，统一转 JSON 字符串（需与后端签名拼接规则保持一致）
  return JSON.stringify(data);
};

// 请求拦截：统一挂载 token、其它自定义头
http.interceptors.request.use(
  (config: InternalAxiosRequestConfig) => {
    // 1. 统一挂载 Token
    const token = getAccessToken();
    if (token) {
      const headers = config.headers || {};
      const finalToken = authScheme ? `${authScheme} ${token}` : token;
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      (headers as any)[authHeaderName] = finalToken;
      config.headers = headers;
    }

    // 2. 安全签名头（appkey / timestamp / signature）
    if (SECURITY_APP_KEY && SECURITY_SIGN_KEY) {
      const appkey = SECURITY_APP_KEY;
      const timestamp = Date.now().toString(); // 13 位毫秒时间戳
      const dataStr = buildSignData(config);
      const signStr = appkey + SECURITY_SIGN_KEY + timestamp + dataStr;
      const signature = CryptoJS.MD5(signStr).toString();

      const headers = config.headers || {};
      (headers as any).appkey = appkey;
      (headers as any).timestamp = timestamp;
      (headers as any).signature = signature;
      config.headers = headers;
    }

    return config;
  },
  (error) => Promise.reject(error),
);

// 响应拦截：处理响应头、新 token、统一业务错误
http.interceptors.response.use(
  (response: AxiosResponse<unknown>) => {
    // 响应头中如果携带新的 token，自动更新（优先使用 x-refresh-token）
    const refreshHeader =
      (response.headers?.["x-refresh-token"] as string | undefined)

    if (refreshHeader) {
      const token = refreshHeader.trim();
      if (token) {
        setAccessToken(token);
      }
    } 
    return transformResponseData(response);
  },
  async (error: AxiosError<ApiResponse>) => {
    const { response } = error;

    // 没有响应（网络错误 / 超时等）
    if (!response) {
      showErrorMessage("网络异常，请检查网络连接");
      return Promise.reject(error);
    }

    const status = response.status;
    // 401：提示重新登录并通过路由跳转到登录页
    if (status === 401) {
      clearAuthTokens();
      showErrorMessage("登录已过期，请重新登录");
      const currentPath = router.currentRoute.value?.fullPath || "/";
      if (router.currentRoute.value?.path !== "/login") {
        router.push({
          path: "/login",
          query: { redirect: currentPath },
        });
      }
      return Promise.reject(error);
    }

    // 其它 HTTP 状态码的通用错误处理
    let msg =
      response.data?.message ||
      `请求失败（HTTP ${status}）`;

    if (!response.data?.message) {
      switch (status) {
        case 400:
          msg = "请求参数错误";
          break;
        case 403:
          msg = "没有访问权限";
          break;
        case 404:
          msg = "请求的资源不存在";
          break;
        case 500:
          msg = "服务器异常，请稍后重试";
          break;
        default:
          msg = `请求失败（HTTP ${status}）`;
      }
    }

    showErrorMessage(msg);
    return Promise.reject(error);
  },
);

// 对外暴露的请求客户端，提供常用方法：get / post / put / delete / request
export interface RequestOptions<D = any> extends AxiosRequestConfig<D> {}

const requestClient = {
  get<T = any, D = any>(url: string, config?: RequestOptions<D>) {
    return http.get<T>(url, config);
  },
  post<T = any, D = any>(url: string, data?: D, config?: RequestOptions<D>) {
    return http.post<T>(url, data, config);
  },
  put<T = any, D = any>(url: string, data?: D, config?: RequestOptions<D>) {
    return http.put<T>(url, data, config);
  },
  delete<T = any, D = any>(url: string, config?: RequestOptions<D>) {
    return http.delete<T>(url, config);
  },
  /**
   * 通用 request 方法，支持 url + config 的形式
   * 例如：
   *   requestClient.request('/sysfile/Upload', {
   *     method: 'POST',
   *     data,
   *     headers: { 'Content-Type': 'multipart/form-data' },
   *   })
   */
  request<T = any, D = any>(url: string, config?: RequestOptions<D>) {
    const finalConfig: AxiosRequestConfig<D> = {
      url,
      ...(config || {}),
    };
    return http.request<T, T, D>(finalConfig);
  },
};

export default requestClient;
