import requestClient from "../http";

export interface SysNoticeUser {
  id: number;
  fullName?: string;
  loginAccount?: string;
}

export interface SysNoticeItem {
  id: number;
  tenantId?: number;
  sendUserId: number;
  sendUser?: SysNoticeUser;
  acceptUserIds?: number[];
  title: string;
  content?: string;
  status?: number;
  isSend?: boolean;
  isRead?: boolean;
  createTime?: string;
}

export interface SysNoticePage<T> {
  totalPages: number;
  totalItems: number;
  items: T[];
}

export interface SysNoticeTotal {
  unread: number;
  draft: number;
  archive: number;
  delete: number;
}

/** 通知分页（收件箱/已发送等） */
export function fetchSysNoticePage(params: any) {
  return requestClient.get<SysNoticePage<SysNoticeItem>>("/sysnotice/pages", {
    params,
  });
}

/** 统计（未读/草稿/存档/删除） */
export function fetchSysNoticeTotal() {
  return requestClient.get<SysNoticeTotal>("/sysnotice/total");
}

/** 通知详情；type=1 表示“收件箱点击查看并标记已读” */
export function fetchSysNoticeById(id: string | number, type: 0 | 1 = 0) {
  return requestClient.get<SysNoticeItem>(`/sysnotice/${id}/${type}`);
}

/** 设置已读；空数组表示全部已读 */
export function setSysNoticeRead(ids: Array<string | number>) {
  return requestClient.put("/sysnotice/read", ids);
}

/** 取消已读 */
export function clearSysNoticeRead(ids: Array<string | number>) {
  return requestClient.put("/sysnotice/clearRead", ids);
}

/** 删除（物理删除） */
export function deleteSysNotice(ids: Array<string | number>) {
  return requestClient.request("/sysnotice", {
    method: "DELETE",
    data: ids,
  });
}

