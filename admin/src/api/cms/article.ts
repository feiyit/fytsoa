import requestClient from "../http";

export const fetchCmsArticlePage = (data?: any) =>
  requestClient.post('/cmsarticle/pages', data);

export const fetchCmsArticleById = (id: string) =>
  requestClient.get(`/cmsarticle/${id}`);

export async function addCmsArticle(data: any) {
  return requestClient.post('/cmsarticle', data);
}

export async function updateCmsArticle(data: any) {
  return requestClient.put('/cmsarticle', data);
}

export async function deleteCmsArticle(data: any) {
  return requestClient.request('/cmsarticle', {
    data: data,
    method: 'DELETE',
  });
}

export async function recycleCmsArticle(data: any) {
  return requestClient.request('/cmsarticle/recycle', {
    data: data,
    method: 'PUT',
  });
}

export async function recoveryCmsArticle(data: any) {
  return requestClient.request('/cmsarticle/recovery', {
    data: data,
    method: 'PUT',
  });
}
