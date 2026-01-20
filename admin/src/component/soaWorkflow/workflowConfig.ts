import type { WorkflowConfig } from './types';

// TODO: Replace the mock implementations below with real API calls.
// The structure mirrors the original SCUI workflow configuration so that
// the selector can be wired to existing back-end services without further changes.

const mockTree = [
  {
    id: 'dept-1',
    label: '示例部门',
    children: [
      {
        id: 'dept-1-1',
        label: '技术部',
      },
      {
        id: 'dept-1-2',
        label: '市场部',
      },
    ],
  },
];

const mockUsers = Array.from({ length: 8 }).map((_, index) => ({
  id: `user-${index + 1}`,
  user: `示例用户${index + 1}`,
}));

const mockRoles = [
  {
    id: 'role-1',
    label: '管理员',
  },
  {
    id: 'role-2',
    label: '业务员',
  },
];

const workflowConfig: WorkflowConfig = {
  successCode: 200,
  group: {
    apiObj: {
      async get() {
        return Promise.resolve({
          code: 200,
          data: mockTree,
          message: 'mock',
        });
      },
    },
    parseData: res => ({
      rows: res.data || [],
      msg: res.message,
      code: res.code,
    }),
    props: {
      key: 'id',
      label: 'label',
      children: 'children',
    },
  },
  user: {
    apiObj: {
      async get() {
        return Promise.resolve({
          code: 200,
          data: {
            rows: mockUsers,
            total: mockUsers.length,
          },
          message: 'mock',
        });
      },
    },
    pageSize: 20,
    parseData: res => ({
      rows: res.data?.rows || [],
      total: res.data?.total || 0,
      msg: res.message,
      code: res.code,
    }),
    props: {
      key: 'id',
      label: 'user',
    },
    request: {
      page: 'page',
      pageSize: 'pageSize',
      groupId: 'groupId',
      keyword: 'keyword',
    },
  },
  role: {
    apiObj: {
      async get() {
        return Promise.resolve({
          code: 200,
          data: mockRoles,
          message: 'mock',
        });
      },
    },
    parseData: res => ({
      rows: res.data || [],
      msg: res.message,
      code: res.code,
    }),
    props: {
      key: 'id',
      label: 'label',
      children: 'children',
    },
  },
};

export default workflowConfig;
