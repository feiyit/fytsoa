import type { RemoteHandlerMap } from './types';

const remoteHandlers: RemoteHandlerMap = {
  '/mock/form/departments': async () => [
    { label: '技术部', value: 'tech' },
    { label: '市场部', value: 'marketing' },
    { label: '运营部', value: 'operation' },
  ],
  '/mock/form/roles': async () => [
    { label: '管理员', value: 'admin' },
    { label: '审核员', value: 'auditor' },
    { label: '访客', value: 'guest' },
  ],
  '/mock/form/cascader': async () => [
    {
      label: '华北大区',
      value: 'north',
      children: [
        { label: '北京', value: 'beijing' },
        { label: '天津', value: 'tianjin' },
      ],
    },
    {
      label: '华南大区',
      value: 'south',
      children: [
        { label: '广州', value: 'guangzhou' },
        { label: '深圳', value: 'shenzhen' },
      ],
    },
  ],
};

export async function requestFormRemote(
  api: string,
  payload?: Record<string, any>,
): Promise<any[]> {
  const handler = remoteHandlers[api];
  if (!handler) {
    if (import.meta.env.DEV) {
      // eslint-disable-next-line no-console
      console.warn(`[soaForm] 未找到远程数据处理函数: ${api}`);
    }
    return [];
  }

  const result = await handler(payload);
  if (Array.isArray(result)) {
    return result;
  }
  if (result && typeof result === 'object' && 'data' in result) {
    const data = (result as { data?: any }).data;
    if (Array.isArray(data)) {
      return data;
    }
  }
  return [];
}

export function registerFormRemote(api: string, handler: RemoteHandlerMap[string]) {
  remoteHandlers[api] = handler;
}
