import requestClient from "../http";

export interface QuartzTask {
  id?: number;
  taskName: string;
  groupName: string;
  /** Cron 表达式（Quartz 格式） */
  interval: string;
  /** 描述 */
  describe?: string;
  /** 任务类型：约定 2=HTTP, 1=业务处理器(DLL) */
  taskType: number;

  /** HTTP 任务参数 */
  apiUrl?: string;
  apiRequestType?: string; // GET/POST/PUT/DELETE...
  apiAuthKey?: string;
  apiAuthValue?: string;
  apiParameter?: string;

  /** 业务处理器参数 */
  dllClassName?: string;
  dllActionName?: string;

  /** 任务状态（后端 JobState 枚举值） */
  status?: number;
  lastRunTime?: string | null;
  changeTime?: string | null;
}

export interface QuartzTaskLog {
  id: number;
  taskName: string;
  groupName: string;
  beginDate: string;
  endDate?: string | null;
  msg?: string;
}

export interface QuartzResult {
  status: boolean;
  message?: string;
}

export interface QuartzLogPage<T> {
  total: number;
  data: T[];
}

/** 任务列表 */
export async function fetchQuartzTaskList() {
  return requestClient.get<QuartzTask[]>("/sysquartz");
}

/** 新建任务 */
export async function addQuartzTask(data: QuartzTask) {
  return requestClient.post<QuartzResult>("/sysquartz", data);
}

/** 修改任务 */
export async function updateQuartzTask(data: QuartzTask) {
  return requestClient.put<QuartzResult>("/sysquartz", data);
}

/** 暂停任务 */
export async function pauseQuartzTask(data: QuartzTask) {
  return requestClient.put<QuartzResult>("/sysquartz/pauseJob", data);
}

/** 开启任务 */
export async function startQuartzTask(data: QuartzTask) {
  return requestClient.put<QuartzResult>("/sysquartz/startJob", data);
}

/** 立即执行 */
export async function runQuartzTask(data: QuartzTask) {
  return requestClient.put<QuartzResult>("/sysquartz/runJob", data);
}

/** 删除任务 */
export async function deleteQuartzTask(data: QuartzTask) {
  return requestClient.request<QuartzResult>("/sysquartz", {
    method: "DELETE",
    data,
  });
}

/** 任务执行日志 */
export async function fetchQuartzTaskLogs(params: {
  taskName: string;
  groupName: string;
  current: number;
  size: number;
}) {
  return requestClient.get<QuartzLogPage<QuartzTaskLog>>("/sysquartz/jobRecord", {
    params,
  });
}
