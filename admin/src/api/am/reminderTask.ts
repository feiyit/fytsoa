import requestClient from "../http";

// 提醒任务
export const fetchAmReminderTaskPage = (data?: any) =>
  requestClient.post("/amremindertask/pages", data);

export const fetchAmReminderTaskById = (id: string) =>
  requestClient.get(`/amremindertask/${id}`);

export async function addAmReminderTask(data: any) {
  return requestClient.post("/amremindertask", data);
}

export async function updateAmReminderTask(data: any) {
  return requestClient.put("/amremindertask", data);
}

// 标记已读（后端：ReadAsync）
export async function readAmReminderTask(id: string) {
  return requestClient.post("/amremindertask/read", null, {
    params: { id },
  });
}

// 关闭（后端：CloseAsync）
export async function closeAmReminderTask(id: string) {
  return requestClient.post("/amremindertask/close", null, {
    params: { id },
  });
}

export async function deleteAmReminderTask(data: any) {
  return requestClient.request("/amremindertask", {
    data,
    method: "DELETE",
  });
}

