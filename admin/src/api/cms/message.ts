import requestClient from "../http";

export const fetchCmsMessagePage = (params?: any) =>
  requestClient.get('/sysmessage/pages', { params });

export async function updateCmsMessageRead(data: any) {
  return requestClient.put('/sysmessage/read/', data);
}

export async function deleteCmsMessage(data: any) {
  return requestClient.request('/sysmessage', {
    data: data,
    method: 'DELETE',
  });
}
