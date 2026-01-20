import requestClient from "../http";

export const fetchCmsTemplatePage = (params?: any) =>
  requestClient.get('/cmstemplate/pages', {
    params,
  });

export const fetchCmsTemplateById = (id: string) =>
  requestClient.get(`/cmstemplate/${id}`);

export async function addCmsTemplate(data: any) {
  return requestClient.post('/cmstemplate', data);
}

export async function updateCmsTemplate(data: any) {
  return requestClient.put('/cmstemplate', data);
}

export async function deleteCmsTemplate(data: any) {
  return requestClient.request('/cmstemplate', {
    data: data,
    method: 'DELETE',
  });
}
