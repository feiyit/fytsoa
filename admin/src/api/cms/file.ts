import requestClient from "../http";

// export async function uploadFile(path: string, data: any) {
//   return requestClient.post('/sysfile/uploading?path=/upload/' + path, data);
// }

export async function uploadServiceFile(path: string, data: any, options?: {}) {
  return requestClient.request('/sysfile/Upload?path=/upload/' + path + '/', {
    data: data,
    method: 'POST',
    headers: {
      'Content-Type': 'multipart/form-data',
    },
    ...options,
  });
}

export const fetchFileTree = (params?: any) =>
  requestClient.get('/sysfile/directory', { params });

export const fetchFileList = (params: any) =>
  requestClient.get('/sysfile/files', { params });

export const deletePathFile = (params: string) =>
  requestClient.delete('/sysfile/file?path=' + params);

export const deleteDirectory = (params: string) =>
  requestClient.delete('/sysfile/directory?path=' + params);
