import type { FormRules } from 'element-plus';

type Dict<T = any> = Record<string, T>;

export interface FormRemoteOption {
  api: string;
  data?: Dict;
}

export interface FormOptionItem extends Dict {
  label: string;
  value?: any;
  name?: string;
}

export interface FormItemOptions extends Dict {
  placeholder?: string;
  maxlength?: number;
  multiple?: boolean;
  type?: string;
  shortcuts?: any;
  defaultTime?: any;
  valueFormat?: string;
  marks?: Record<string | number, any>;
  items?: FormOptionItem[];
  remote?: FormRemoteOption;
  columns?: Dict[];
  componentProps?: Dict;
}

export interface FormItemConfig {
  name?: string;
  label?: string;
  span?: number;
  component: string;
  value?: any;
  tips?: string;
  message?: string;
  rules?: FormRules[string];
  hideHandle?: string | ((form: Dict) => boolean);
  requiredHandle?: string | ((form: Dict) => boolean);
  options?: FormItemOptions;
}

export interface FormConfig {
  labelWidth?: string | number;
  labelPosition?: 'left' | 'right' | 'top';
  gutter?: number;
  submitText?: string;
  resetText?: string;
  showReset?: boolean;
  formItems: FormItemConfig[];
}

export interface RemoteHandler {
  (payload?: Dict): Promise<any[]>;
}

export interface RemoteHandlerMap {
  [api: string]: RemoteHandler;
}

export type FormRecord = Dict;

export type FormExpose = {
  validate: FormValidateFn;
  resetFields: () => void;
  scrollToField: (prop: string) => void;
  submit: () => void;
  form: FormRecord;
};

type FormValidateCallback = (isValid: boolean, invalidFields?: Dict) => void;
export type FormValidateFn = (callback?: FormValidateCallback) => Promise<boolean> | void;

export const DEFAULT_FORM_GUTTER = 15;
