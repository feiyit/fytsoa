import requestClient from "./http";
export interface WorkspaceDto {
  articleCountAll: Number;
  articleCountDay: Number;
  articleCountCheck: Number;
  articleCountDraft: Number;
  articleCountComment: Number;
}
export const fetchWorkspace = () =>
  requestClient.get<WorkspaceDto>('/workspace/workspace');

export const fetchArticleView = (params: any) =>
  requestClient.get('/workspace/articleview', {
    params,
  });