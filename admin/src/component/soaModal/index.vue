<template>
  <!-- 根组件本身不渲染内容，所有能力通过 useSoaModal 暴露 -->
</template>

<script lang="ts">
import { defineComponent, h, reactive, ref, type Component } from "vue";
import { ElDialog, ElButton } from "element-plus";

export interface SoaModalOptions {
  /**
   * 点击“确定”按钮时的回调
   */
  onConfirm?: () => void | Promise<void>;
  /**
   * 点击“取消”或关闭时的回调
   */
  onCancel?: () => void | Promise<void>;
  /**
   * 初始是否全屏
   */
  fullscreen?: boolean;
}

/**
 * Soa 弹窗封装
 *
 * 使用方式：
 *   import { useSoaModal } from "@/component/soaModal/index.vue";
 *
 *   const [Modal, modalApi] = useSoaModal({
 *     onConfirm: () => {
 *       modalApi.close();
 *     },
 *   });
 *
 *   <Modal class="w-[600px]" title="基础示例">
 *     modal content
 *   </Modal>
 *
 * 说明：
 *  - Modal 会渲染一个 Element Plus 的 ElDialog
 *  - 接收并透传所有原生 ElDialog 的 props / 事件 / class 等属性（通过 attrs）
 *  - 默认 footer 为“取消 / 确定”按钮，用户可通过具名插槽 footer 自定义
 */
export function useSoaModal(options: SoaModalOptions = {}): [
  Component,
  {
    open: () => void;
    close: () => void;
    setOptions: (opts: Partial<SoaModalOptions>) => void;
  }
] {
  const visible = ref(false);
  const currentOptions = reactive<SoaModalOptions>({ ...options });
  const isFullscreen = ref(!!options.fullscreen);

  const open = () => {
    visible.value = true;
  };

  const close = () => {
    visible.value = false;
  };

  const setOptions = (opts: Partial<SoaModalOptions>) => {
    Object.assign(currentOptions, opts);
    if (typeof opts.fullscreen !== "undefined") {
      isFullscreen.value = !!opts.fullscreen;
    }
  };

  // 使用 defineComponent 动态创建 Modal 组件
  const Modal: Component = defineComponent({
    name: "SoaModal",
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

      return () =>
        h(
          ElDialog,
          {
            modelValue: visible.value,
            "onUpdate:modelValue": (val: boolean) => {
              visible.value = val;
              if (!val && currentOptions.onCancel) {
                // 直接关闭弹窗（例如点击遮罩）时也触发 onCancel
                currentOptions.onCancel();
              }
            },
            // 默认配置：挂载到 body、支持拖拽、可全屏
            appendToBody: true,
            draggable: true,
            fullscreen: isFullscreen.value,
            // 让外部可以传入 title / width / class / 以及覆盖默认配置
            ...attrs,
            onClose: handleCancel,
          },
          {
            // 主体内容
            default: () => slots.default?.(),
            header: slots.header,
            // footer：如果用户自定义了 footer 插槽，则完全使用用户的
            // 否则渲染一个默认的“取消 / 确定”按钮区域
            footer: () =>
              slots.footer
                ? slots.footer()
                : h(
                    "div",
                    { class: "soa-modal__footer flex justify-end gap-2" },
                    [
                      h(
                        ElButton,
                        {
                          onClick: handleCancel,
                        },
                        { default: () => "取消" }
                      ),
                      h(
                        ElButton,
                        {
                          type: "primary",
                          onClick: handleConfirm,
                        },
                        { default: () => "确定" }
                      ),
                    ]
                  ),
          }
        );
    },
  });

  const modalApi = {
    open,
    close,
    setOptions,
  };

  return [Modal, modalApi];
}

// 默认导出一个空壳组件
export default defineComponent({
  name: "SoaModalRoot",
});
</script>
