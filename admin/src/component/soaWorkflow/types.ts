export type WorkflowSelectType = 1 | 2;

export interface WorkflowUser {
  id: string | number;
  name: string;
}

export interface WorkflowRole {
  id: string | number;
  name: string;
}

export type ApproverSetType = 1 | 2 | 3 | 4 | 5 | 7;
export type ApproverSelectMode = 1 | 2;
export type ApproverDirectorMode = 0 | 1;
export type ApproverTermMode = 0 | 1;
export type ApproverExamineMode = 1 | 2 | 3;

export interface WorkflowBaseNode {
  nodeName: string;
  type: number;
  childNode?: WorkflowNode | null;
}

export interface PromoterNode extends WorkflowBaseNode {
  type: 0;
  nodeRoleList: WorkflowRole[];
}

export interface ApproverNode extends WorkflowBaseNode {
  type: 1;
  setType: ApproverSetType;
  nodeUserList: WorkflowUser[];
  nodeRoleList: WorkflowRole[];
  examineLevel: number;
  directorLevel: number;
  selectMode: ApproverSelectMode;
  termAuto: boolean;
  term: number;
  termMode: ApproverTermMode;
  examineMode: ApproverExamineMode;
  directorMode: ApproverDirectorMode;
}

export interface SendNode extends WorkflowBaseNode {
  type: 2;
  userSelectFlag: boolean;
  nodeUserList: WorkflowUser[];
}

export interface ConditionItem {
  nodeName: string;
  type: 3;
  priorityLevel: number;
  conditionMode: 1 | 2;
  conditionList: Array<{
    label: string;
    field: string;
    operator: string;
    value: string;
  }>;
  childNode?: WorkflowNode | null;
}

export interface BranchNode extends WorkflowBaseNode {
  type: 4;
  conditionNodes: ConditionItem[];
}

export type WorkflowNode = PromoterNode | ApproverNode | SendNode | BranchNode | ConditionItem;

export interface SelectValueItem {
  id: string | number;
  name: string;
}

export interface WorkflowConfig {
  successCode: number;
  group: {
    apiObj: Record<string, any>;
    parseData: (res: any) => {
      rows: any[];
      msg?: string;
      code?: number;
    };
    props: {
      key: string;
      label: string;
      children?: string;
    };
  };
  user: {
    apiObj: Record<string, any>;
    pageSize: number;
    parseData: (res: any) => {
      rows: any[];
      total?: number;
      msg?: string;
      code?: number;
    };
    props: {
      key: string;
      label: string;
    };
    request: {
      page: string;
      pageSize: string;
      groupId: string;
      keyword: string;
    };
  };
  role: {
    apiObj: Record<string, any>;
    parseData: (res: any) => {
      rows: any[];
      msg?: string;
      code?: number;
    };
    props: {
      key: string;
      label: string;
      children?: string;
    };
  };
}
