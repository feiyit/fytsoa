const menu = {
    code: 200,
    data: {
      menu: [
        {
          name: "home",
          path: "/home",
          meta: {
            title: "首页",
            icon: "el-icon-eleme-filled",
            type: "menu",
          },
          children: [
            {
              name: "dashboard",
              path: "/dashboard",
              meta: {
                title: "控制台",
                icon: "el-icon-menu",
                affix: true,
              },
              component: "home",
            },
            {
              name: "userCenter",
              path: "/usercenter",
              meta: {
                title: "个人信息",
                icon: "el-icon-user",
              },
              component: "userCenter",
            },
          ],
        },
        {
          name: "vab",
          path: "/vab",
          meta: {
            title: "组件",
            icon: "el-icon-takeaway-box",
            type: "menu",
          },
          children: [
            {
              path: "/vab/mini",
              name: "minivab",
              meta: {
                title: "原子组件",
                icon: "el-icon-magic-stick",
                type: "menu",
              },
              component: "vab/mini",
            },
            {
              path: "/vab/iconfont",
              name: "iconfont",
              meta: {
                title: "扩展图标",
                icon: "el-icon-orange",
                type: "menu",
              },
              component: "vab/iconfont",
            },
            {
              path: "/vab/data",
              name: "vabdata",
              meta: {
                title: "Data 数据展示",
                icon: "el-icon-histogram",
                type: "menu",
              },
              children: [
                {
                  path: "/vab/chart",
                  name: "chart",
                  meta: {
                    title: "图表 Echarts",
                    type: "menu",
                  },
                  component: "vab/chart",
                },
                {
                  path: "/vab/statistic",
                  name: "statistic",
                  meta: {
                    title: "统计数值",
                    type: "menu",
                  },
                  component: "vab/statistic",
                },
                {
                  path: "/vab/video",
                  name: "scvideo",
                  meta: {
                    title: "视频播放器",
                    type: "menu",
                  },
                  component: "vab/video",
                },
                {
                  path: "/vab/qrcode",
                  name: "qrcode",
                  meta: {
                    title: "二维码",
                    type: "menu",
                  },
                  component: "vab/qrcode",
                },
              ],
            },
            {
              path: "/vab/form",
              name: "vabform",
              meta: {
                title: "Form 数据录入",
                icon: "el-icon-edit",
                type: "menu",
              },
              children: [
                {
                  path: "/vab/tableselect",
                  name: "tableselect",
                  meta: {
                    title: "表格选择器",
                    type: "menu",
                  },
                  component: "vab/tableselect",
                },
                {
                  path: "/vab/formtable",
                  name: "formtable",
                  meta: {
                    title: "表单表格",
                    type: "menu",
                  },
                  component: "vab/formtable",
                },
                {
                  path: "/vab/filterbar",
                  name: "filterBar",
                  meta: {
                    title: "过滤器v2",
                    type: "menu",
                  },
                  component: "vab/filterBar",
                },
                {
                  path: "/vab/upload",
                  name: "upload",
                  meta: {
                    title: "上传",
                    type: "menu",
                  },
                  component: "vab/upload",
                },
                {
                  path: "/vab/select",
                  name: "scselect",
                  meta: {
                    title: "异步选择器",
                    type: "menu",
                  },
                  component: "vab/select",
                },
                {
                  path: "/vab/iconselect",
                  name: "iconSelect",
                  meta: {
                    title: "图标选择器",
                    type: "menu",
                  },
                  component: "vab/iconselect",
                },
                {
                  path: "/vab/cron",
                  name: "cron",
                  meta: {
                    title: "Cron规则生成器",
                    type: "menu",
                  },
                  component: "vab/cron",
                },
                {
                  path: "/vab/editor",
                  name: "editor",
                  meta: {
                    title: "富文本编辑器",
                    type: "menu",
                  },
                  component: "vab/editor",
                },
              ],
            },
            {
              path: "/vab/feedback",
              name: "vabfeedback",
              meta: {
                title: "Feedback 反馈",
                icon: "el-icon-mouse",
                type: "menu",
              },
              children: [
                {
                  path: "/vab/drag",
                  name: "drag",
                  meta: {
                    title: "拖拽排序",
                    type: "menu",
                  },
                  component: "vab/drag",
                },
                {
                  path: "/vab/contextmenu",
                  name: "contextmenu",
                  meta: {
                    title: "右键菜单",
                    type: "menu",
                  },
                  component: "vab/contextmenu",
                },
                {
                  path: "/vab/cropper",
                  name: "cropper",
                  meta: {
                    title: "图像剪裁",
                    type: "menu",
                  },
                  component: "vab/cropper",
                },
                {
                  path: "/vab/fileselect",
                  name: "fileselect",
                  meta: {
                    title: "资源库选择器",
                    type: "menu",
                  },
                  component: "vab/fileselect",
                },
                {
                  path: "/vab/dialog",
                  name: "dialogExtend",
                  meta: {
                    title: "弹窗扩展",
                    type: "menu",
                  },
                  component: "vab/dialog",
                },
              ],
            },
            {
              path: "/vab/others",
              name: "vabothers",
              meta: {
                title: "Others 其他",
                icon: "el-icon-more-filled",
                type: "menu",
              },
              children: [
                {
                  path: "/vab/print",
                  name: "print",
                  meta: {
                    title: "打印",
                    type: "menu",
                  },
                  component: "vab/print",
                },
                {
                  path: "/vab/watermark",
                  name: "watermark",
                  meta: {
                    title: "水印",
                    type: "menu",
                  },
                  component: "vab/watermark",
                },
              ],
            },
            {
              path: "/vab/workflow",
              name: "workflow",
              meta: {
                title: "工作流设计器",
                icon: "el-icon-share",
                type: "menu",
              },
              component: "vab/workflow",
            },
            {
              path: "/vab/formrender",
              name: "formRender",
              meta: {
                title: "动态表单(Beta)",
                icon: "el-icon-message-box",
                type: "menu",
              },
              component: "vab/form",
            },
          ],
        },
        {
          name: "template",
          path: "/template",
          meta: {
            title: "模板",
            icon: "el-icon-files",
            type: "menu",
          },
          children: [
            {
              path: "/template/blank",
              name: "blank",
              meta: {
                title: "空白模板",
                icon: "el-icon-folder",
                type: "menu",
              },
              component: "template/blank",
            },
            {
              path: "/template/chartlist",
              name: "chartlist",
              meta: {
                title: "统计列表",
                icon: "el-icon-data-analysis",
                type: "menu",
              },
              component: "template/chartlist",
            },
            {
              path: "/template/calendar",
              name: "calendar",
              meta: {
                title: "日历计划",
                icon: "el-icon-calendar",
                type: "menu",
              },
              component: "template/calendar",
            },
            {
              path: "/template/list",
              name: "list",
              meta: {
                title: "详细列表",
                icon: "el-icon-fold",
                type: "menu",
              },
              component: "template/list",
            },
            {
              path: "/template/list/save/:id?",
              name: "list-save",
              meta: {
                title: "新标签",
                hidden: true,
                active: "/template/list",
                type: "menu",
              },
              component: "template/list/save",
            },
            {
              path: "/template/svgmap",
              name: "svgmap",
              meta: {
                title: "地理信息",
                icon: "el-icon-map-location",
                type: "menu",
              },
              component: "template/svgmap",
            },
            {
              path: "/template/tabinfo",
              name: "tabinfo",
              meta: {
                title: "分栏明细",
                icon: "el-icon-document",
                type: "menu",
              },
              component: "template/tabinfo",
            },
            {
              path: "/template/server",
              name: "server",
              meta: {
                title: "服务器监控",
                icon: "el-icon-cpu",
                type: "menu",
              },
              component: "template/server",
            },
            {
              path: "/template/stepform",
              name: "stepform",
              meta: {
                title: "分步表单",
                icon: "el-icon-switch",
                type: "menu",
              },
              component: "template/stepform",
            },
          ],
        },
        {
          name: "other",
          path: "/other",
          meta: {
            title: "其他",
            icon: "el-icon-more-filled",
            type: "menu",
          },
          children: [
            {
              path: "/other/directive",
              name: "directive",
              meta: {
                title: "指令",
                icon: "el-icon-price-tag",
                type: "menu",
              },
              component: "other/directive",
            },
            {
              path: "/other/viewTags",
              name: "viewTags",
              meta: {
                title: "标签操作",
                icon: "el-icon-files",
                type: "menu",
              },
              component: "other/viewTags",
              children: [
                {
                  path: "/other/fullpage",
                  name: "fullpage",
                  meta: {
                    title: "整页路由",
                    icon: "el-icon-monitor",
                    fullpage: true,
                    hidden: true,
                    type: "menu",
                  },
                  component: "other/fullpage",
                },
              ],
            },
            {
              path: "/link",
              name: "link",
              meta: {
                title: "外部链接",
                icon: "el-icon-link",
                type: "menu",
              },
              children: [
                {
                  path: "https://baidu.com",
                  name: "百度",
                  meta: {
                    title: "百度",
                    type: "link",
                  },
                },
                {
                  path: "https://www.google.cn",
                  name: "谷歌",
                  meta: {
                    title: "谷歌",
                    type: "link",
                  },
                },
              ],
            },
            {
              path: "/iframe",
              name: "Iframe",
              meta: {
                title: "Iframe",
                icon: "el-icon-position",
                type: "menu",
              },
              children: [
                {
                  path: "https://v3.cn.vuejs.org",
                  name: "vue3",
                  meta: {
                    title: "VUE 3",
                    type: "iframe",
                  },
                },
                {
                  path: "https://element-plus.gitee.io",
                  name: "elementplus",
                  meta: {
                    title: "Element Plus",
                    type: "iframe",
                  },
                },
                {
                  path: "https://lolicode.gitee.io/scui-doc",
                  name: "scuidoc",
                  meta: {
                    title: "SCUI文档",
                    type: "iframe",
                  },
                },
              ],
            },
          ],
        },
        {
          name: "test",
          path: "/test",
          meta: {
            title: "实验室",
            icon: "el-icon-mouse",
            type: "menu",
          },
          children: [
            {
              path: "/test/autocode",
              name: "autocode",
              meta: {
                title: "代码生成器",
                icon: "sc-icon-code",
                type: "menu",
              },
              redirect: "/test/autocode/index",
              children: [
                {
                  path: "/test/autocode/index",
                  name: "autocode-index",
                  meta: {
                    title: "代码生成器",
                    hidden: true,
                    hiddenBreadcrumb: true,
                    active: "/test/autocode",
                    type: "menu",
                  },
                  component: "test/autocode",
                },
                {
                  path: "/test/autocode/list",
                  name: "autocode-list",
                  meta: {
                    title: "列表生成器",
                    hidden: true,
                    active: "/test/autocode",
                    type: "menu",
                  },
                  component: "test/autocode/list",
                },
              ],
            },
            {
              path: "/test/codebug",
              name: "codebug",
              meta: {
                title: "异常处理",
                icon: "sc-icon-bug-line",
                type: "menu",
              },
              component: "test/codebug",
            },
          ],
        },
        {
          name: "setting",
          path: "/setting",
          meta: {
            title: "配置",
            icon: "el-icon-setting",
            type: "menu",
          },
          children: [
            {
              path: "/setting/system",
              name: "system",
              meta: {
                title: "系统设置",
                icon: "el-icon-tools",
                type: "menu",
              },
              component: "setting/system",
            },
            {
              path: "/setting/user",
              name: "user",
              meta: {
                title: "用户管理",
                icon: "el-icon-user-filled",
                type: "menu",
              },
              component: "setting/user",
            },
            {
              path: "/setting/role",
              name: "role",
              meta: {
                title: "角色管理",
                icon: "el-icon-notebook",
                type: "menu",
              },
              component: "setting/role",
            },
            {
              path: "/setting/dic",
              name: "dic",
              meta: {
                title: "字典管理",
                icon: "el-icon-document",
                type: "menu",
              },
              component: "setting/dic",
            },
            {
              path: "/setting/table",
              name: "tableSetting",
              meta: {
                title: "表格列管理",
                icon: "el-icon-scale-to-original",
                type: "menu",
              },
              component: "setting/table",
            },
            {
              path: "/setting/menu",
              name: "settingMenu",
              meta: {
                title: "菜单管理",
                icon: "el-icon-fold",
                type: "menu",
              },
              component: "setting/menu",
            },
            {
              path: "/setting/task",
              name: "task",
              meta: {
                title: "计划任务",
                icon: "el-icon-alarm-clock",
                type: "menu",
              },
              component: "setting/task",
            },
            {
              path: "/setting/client",
              name: "client",
              meta: {
                title: "应用管理",
                icon: "el-icon-help-filled",
                type: "menu",
              },
              component: "setting/client",
            },
            {
              path: "/setting/log",
              name: "log",
              meta: {
                title: "系统日志",
                icon: "el-icon-warning",
                type: "menu",
              },
              component: "setting/log",
            },
          ],
        },
        {
          path: "/other/about",
          name: "about",
          meta: {
            title: "关于",
            icon: "el-icon-info-filled",
            type: "menu",
          },
          component: "other/about",
        },
        {
          name: "sys",
          path: "/sys",
          meta: {
            title: "系统",
            icon: "el-icon-grid",
            type: "menu",
          },
          children: [
            {
              name: "sysorganize",
              path: "/sys/organize",
              meta: {
                title: "机构管理",
                icon: "el-icon-menu",
              },
              component: "sys/organize",
            },
            {
              name: "sysrole",
              path: "/sys/role",
              meta: {
                title: "角色管理",
                icon: "el-icon-user",
              },
              component: "sys/role",
            },
            {
              name: "syspost",
              path: "/sys/post",
              meta: {
                title: "岗位管理",
                icon: "el-icon-user",
              },
              component: "sys/post",
            },
            {
              name: "sysmenu",
              path: "/sys/menu",
              meta: {
                title: "资源管理",
                icon: "el-icon-user",
              },
              component: "sys/menu",
            },
            {
              name: "sysadmin",
              path: "/sys/admin",
              meta: {
                title: "用户管理",
                icon: "el-icon-user",
              },
              component: "sys/admin",
            },
            {
              name: "sysauthorize",
              path: "/sys/authorize",
              meta: {
                title: "权限管理",
                icon: "el-icon-user",
              },
              component: "sys/authorize",
            },
            {
              name: "syslog",
              path: "/sys/log",
              meta: {
                title: "系统日志",
                icon: "el-icon-user",
              },
              component: "sys/log",
            },
            {
              name: "syscity",
              path: "/sys/city",
              meta: {
                title: "城市管理",
                icon: "el-icon-user",
              },
              component: "sys/city",
            },
            {
              name: "syscode",
              path: "/sys/code",
              meta: {
                title: "数据字典",
                icon: "el-icon-user",
              },
              component: "sys/code",
            },
            {
              name: "syssafety",
              path: "/sys/safety",
              meta: {
                title: "安全设置",
                icon: "el-icon-user",
              },
              component: "sys/safety",
            },
            {
              name: "sysmessage",
              path: "/sys/message",
              meta: {
                title: "系统消息",
                icon: "el-icon-user",
              },
              component: "sys/message",
            },
            {
              name: "modulesgenerate",
              path: "/modules/generate",
              meta: {
                title: "代码生成",
                icon: "el-icon-user",
              },
              component: "modules/generate",
            },
            {
              name: "modulestask",
              path: "/modules/task",
              meta: {
                title: "任务调度",
                icon: "el-icon-user",
              },
              component: "modules/task",
            },
            {
              name: "sysfile",
              path: "/sys/file",
              meta: {
                title: "资源库管理",
                icon: "el-icon-user",
              },
              component: "sys/file",
            },
          ],
        },
      ],
      permissions: [
        "list.add",
        "list.edit",
        "list.delete",
        "user.add",
        "user.edit",
        "user.delete",
      ],
    },
    message: "",
  };
  
  export default menu;