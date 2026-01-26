import requestClient from "../http";

// 提醒规则
export const fetchAmReminderRulePage = (data?: any) =>
  requestClient.post("/amreminderrule/pages", data);

export const fetchAmReminderRuleList = (params?: any) =>
  requestClient.get("/amreminderrule/list", { params });

export const fetchAmReminderRuleById = (id: string) =>
  requestClient.get(`/amreminderrule/${id}`);

export async function addAmReminderRule(data: any) {
  return requestClient.post("/amreminderrule", data);
}

export async function updateAmReminderRule(data: any) {
  return requestClient.put("/amreminderrule", data);
}

export async function deleteAmReminderRule(data: any) {
  return requestClient.request("/amreminderrule", {
    data,
    method: "DELETE",
  });
}

