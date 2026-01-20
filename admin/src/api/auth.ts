import requestClient from "./http";

export interface LoginParams {
  account: string;
  password: string;
  /** 后端用于校验滑动验证的一次性 token */
  codeKey: string;
}

// 服务端返回的用户信息结构
export interface UserInfo {
  id: number | string;
  username: string;
  avatar?: string;
}

export interface LoginResult {
  accessToken: string;
  userInfo: UserInfo;
}

/**
 * 登录
 */
export async function loginApi(data: LoginParams) {
  return requestClient.post<LoginResult>('/operator/login', data);
}

/**
 * 退出登录
 */
export async function logoutApi() {
  return requestClient.get('/operator/logout');
}

interface RouteMenu {
  workbench: string;
  directive: [];
  menu: [];
}

export async function getAllMenusApi() {
  return requestClient.get<RouteMenu[]>('/sysmenu/vbanmenu');
}

export interface SliderVerifyParams {
  account?: string;
}

export interface SliderTokenResult {
  sliderToken: string;
}

/**
 * 滑动请求token
 */
export async function loginSliderToken(data: SliderVerifyParams) {
  return requestClient.post<SliderTokenResult>('/slidervalidate/token', data);
}
