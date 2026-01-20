import requestClient from "../http";

export const fetchLogsPage = (params: any) =>
  requestClient.get('/syslog/pages', {
    params,
  });

export const fetchByLogs = (id: string) =>
  requestClient.delete<boolean>(`/syslog/${id}`);

export const fetchLogsChart = () => requestClient.get(`/syslog/chart`);

export async function deleteLogs(data: any) {
  return requestClient.request('/syslog', {
    data: data,
    method: 'DELETE',
  });
}

export const clearLogs = () => requestClient.delete<boolean>(`/syslog/clear`);
