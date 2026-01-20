<template>
  <!-- 根组件本身不渲染内容，所有能力通过 useSoaDrawer 暴露 -->
</template>

<script lang="ts">
import { defineComponent, h, reactive, ref, type Component } from "vue";
import { ElDrawer, ElButton } from "element-plus";

export interface SoaDrawerOptions {
  /**
   * 点击“确定”按钮时的回调
   */
  onConfirm?: () => void | Promise<void>;
  /**
   * 点击“取消”或关闭时的回调
   */
  onCancel?: () => void | Promise<void>;
}

/**
 * Soa 抽屉封装
 *
 * 使用方式：
 *   import { useSoaDrawer } from "@/component/soaDrawer/index.vue";
 *
 *   const [Drawer, drawerApi] = useSoaDrawer({
 *     onConfirm: () => {
 *       drawerApi.close();
 *     },
 *   });
 *
 *   <Drawer class="w-[500px]" title="抽屉示例">
 *     drawer content
 *   </Drawer>
 *
 * 说明：
 *  - Drawer 会渲染一个 Element Plus 的 ElDrawer
 *  - 接收并透传所有原生 ElDrawer 的 props / 事件 / class 等属性（通过 attrs）
 *  - 默认 footer 为“取消 / 确定”按钮，用户可通过具名插槽 footer 或 :footer="false" 控制
 */
export function useSoaDrawer(options: SoaDrawerOptions = {}): [
  Component,
  {
    open: () => void;
    close: () => void;
    setOptions: (opts: Partial<SoaDrawerOptions>) => void;
  }
] {
  const visible = ref(false);
  const currentOptions = reactive<SoaDrawerOptions>({ ...options });

  const open = () => {
    visible.value = true;
  };

  const close = () => {
    visible.value = false;
  };

  const setOptions = (opts: Partial<SoaDrawerOptions>) => {
    Object.assign(currentOptions, opts);
  };

  // 使用 defineComponent 动态创建 Drawer 组件
  const Drawer: Component = defineComponent({
    name: "SoaDrawer",
    inheritAttrs: false,
    setup(_props, { slots, attrs }) {
      const handleConfirm = async () => {
        if (currentOptions.onConfirm) {
          await currentOptions.onConfirm();
        } else {
          close();
        }
      };

      const handleCancel = async () => {
        if (currentOptions.onCancel) {
          await currentOptions.onCancel();
        }
        close();
      };

      return () => {
        const rawAttrs = attrs as Record<string, unknown>;
        const { footer, ...restAttrs } = rawAttrs;
        const hideDefaultFooter = footer === false;

        return h(
          ElDrawer,
          {
            modelValue: visible.value,
            "onUpdate:modelValue": (val: boolean) => {
              visible.value = val;
              if (!val && currentOptions.onCancel) {
                // 直接关闭抽屉（例如点击遮罩）时也触发 onCancel
                currentOptions.onCancel();
              }
            },
            // 默认配置：挂载到 body，关闭时销毁内容
            appendToBody: true,
            destroyOnClose: true,
            // 让外部可以传入 title / size / direction / class 等，并覆盖默认配置
            ...restAttrs,
            onClose: handleCancel,
          },
          {
            // 主体内容
            default: () => slots.default?.(),
            header: slots.header,
            // footer：如果用户自定义了 footer 插槽，则完全使用用户的
            // 如果 :footer="false"，则不渲染默认 footer
            // 否则渲染一个默认的“取消 / 确定”按钮区域
            footer: () => {
              if (slots.footer) {
                return slots.footer();
              }
              if (hideDefaultFooter) {
                return null;
              }

              return h(
                "div",
                { class: "soa-drawer__footer flex justify-end gap-2" },
                [
                  h(
                    ElButton,
                    {
                      onClick: handleCancel,
                    },
                    { default: () => "取消" },
                  ),
                  h(
                    ElButton,
                    {
                      type: "primary",
                      onClick: handleConfirm,
                    },
                    { default: () => "确定" },
                  ),
                ],
              );
            },
          },
        );
      };
    },
  });

  const drawerApi = {
    open,
    close,
    setOptions,
  };

  return [Drawer, drawerApi];
}

// 默认导出一个空壳组件
export default defineComponent({
  name: "SoaDrawerRoot",
});
</script>

