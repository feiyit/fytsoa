import type { App, Directive, DirectiveBinding } from "vue";
import { useUserStore } from "@/stores/user";

type AuthValue = string | string[];

/**
 * 权限校验工具：
 * - binding.value 期望是 string 或 string[]（权限编码）
 * - 当用户权限列表中不包含任何一个要求的编码时，移除元素
 */
function applyAuth(el: HTMLElement, binding: DirectiveBinding<AuthValue>) {
  const required = binding.value;

  // 未传权限编码，直接显示（等同于不加指令）
  if (!required) return;

  const userStore = useUserStore();
  const userPerms: string[] = (userStore as any).permissions || [];

  const requiredList = Array.isArray(required) ? required : [required];

  // 如果既没有用户权限列表，又配置了指令，默认按“无权限”处理，直接移除元素
  if (!userPerms.length) {
    el.parentNode && el.parentNode.removeChild(el);
    return;
  }

  const hasAny = requiredList.some((code) => userPerms.includes(code));

  if (!hasAny) {
    // 无权限：从 DOM 中移除元素
    el.parentNode && el.parentNode.removeChild(el);
  }
}

const authDirective: Directive<HTMLElement, AuthValue> = {
  mounted(el, binding) {
    applyAuth(el, binding);
  },
  updated(el, binding) {
    applyAuth(el, binding);
  },
};

export function setupAuthDirective(app: App) {
  app.directive("auth", authDirective);
}

