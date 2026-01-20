import type { RouteRecordRaw } from 'vue-router'

/**
 * 动态业务路由（作为 Layout 的子路由挂载）
 * 这里至少包含 8 个一级菜单（/dashboard、/analysis、/monitor、/system、/form、/list、/report、/profile）
 * 后续可以替换为从后端返回的路由配置，再做映射。
 */
export const dynamicRoutes: RouteRecordRaw[] = [
  {
    path: '/dashboard',
    name: 'Dashboard',
    component: () => import('@/views/dashboard/index.vue'),
    meta: {
      title: '仪表盘',
      icon: 'Menu',
      menuKey: 'dashboard',
      fixed: true,
    },
  },
  {
    path: '/analysis',
    name: 'Analysis',
    component: () => import('@/views/analysis/index.vue'),
    meta: {
      title: '数据分析',
      icon: 'Menu',
      menuKey: 'analysis',
    },
  },
  {
    path: '/monitor',
    name: 'Monitor',
    component: () => import('@/views/monitor/index.vue'),
    meta: {
      title: '实时监控',
      icon: 'Menu',
      menuKey: 'monitor',
    },
  },
  {
    path: '/system',
    name: 'System',
    component: () => import('@/views/system/index.vue'),
    meta: {
      title: '系统管理',
      icon: 'Setting',
      menuKey: 'system',
    },
    children: [
      {
        path: 'user',
        name: 'SystemUser',
        component: () => import('@/views/system/user/index.vue'),
        meta: {
          title: '用户管理',
          icon: 'User',
          menuKey: 'system-user',
        },
        children: [
          {
            path: 'list',
            name: 'SystemUserList',
            component: () => import('@/views/system/user/list.vue'),
            meta: {
              title: '用户列表',
              icon: 'List',
              menuKey: 'system-user-list',
            },
          },
          {
            path: 'detail',
            name: 'SystemUserDetail',
            component: () => import('@/views/system/user/detail.vue'),
            meta: {
              title: '用户详情',
              icon: 'Document',
              menuKey: 'system-user-detail',
            },
          },
          {
            path: 'permission',
            name: 'SystemUserPermission',
            component: () => import('@/views/system/user/permission/index.vue'),
            meta: {
              title: '权限配置',
              icon: 'Lock',
              menuKey: 'system-user-permission',
            },
            children: [
              {
                path: 'role-bind',
                name: 'SystemUserRoleBind',
                component: () => import('@/views/system/user/permission/role-bind.vue'),
                meta: {
                  title: '角色绑定',
                  icon: 'Key',
                  menuKey: 'system-user-role-bind',
                },
              },
            ],
          },
        ],
      },
      {
        path: 'role',
        name: 'SystemRole',
        component: () => import('@/views/sys/role/index.vue'),
        meta: {
          title: '角色管理',
          icon: 'UserFilled',
          menuKey: 'system-role',
        },
      },
      {
        path: 'menu',
        name: 'SystemMenu',
        component: () => import('@/views/system/menu/index.vue'),
        meta: {
          title: '菜单管理',
          icon: 'Menu',
          menuKey: 'system-menu',
        },
      },
    ],
  },
  {
    path: '/form',
    name: 'Form',
    component: () => import('@/views/form/index.vue'),
    meta: {
      title: '表单页',
      icon: 'Document',
      menuKey: 'form',
    },
    children: [
      {
        path: 'basic',
        name: 'FormBasic',
        component: () => import('@/views/form/basic/index.vue'),
        meta: {
          title: '基础表单',
          icon: 'Edit',
          menuKey: 'form-basic',
        },
        children: [
          {
            path: 'create',
            name: 'FormBasicCreate',
            component: () => import('@/views/form/basic/create.vue'),
            meta: {
              title: '新建表单',
              icon: 'CirclePlus',
              menuKey: 'form-basic-create',
            },
          },
          {
            path: 'edit',
            name: 'FormBasicEdit',
            component: () => import('@/views/form/basic/edit.vue'),
            meta: {
              title: '编辑表单',
              icon: 'EditPen',
              menuKey: 'form-basic-edit',
            },
          },
        ],
      },
      {
        path: 'step',
        name: 'FormStep',
        component: () => import('@/views/form/step/index.vue'),
        meta: {
          title: '分步表单',
          icon: 'MoreFilled',
          menuKey: 'form-step',
        },
      },
    ],
  },
  {
    path: '/list',
    name: 'List',
    component: () => import('@/views/list/index.vue'),
    meta: {
      title: '列表页',
      icon: 'List',
      menuKey: 'list',
    },
    children: [
      {
        path: 'table',
        name: 'ListTable',
        component: () => import('@/views/list/table.vue'),
        meta: {
          title: '查询表格',
          icon: 'Tickets',
          menuKey: 'list-table',
        },
      },
      {
        path: 'card',
        name: 'ListCard',
        component: () => import('@/views/list/card.vue'),
        meta: {
          title: '卡片列表',
          icon: 'Collection',
          menuKey: 'list-card',
        },
      },
    ],
  },
  {
    path: '/report',
    name: 'Report',
    component: () => import('@/views/report/index.vue'),
    meta: {
      title: '报表中心',
      icon: 'DataAnalysis',
      menuKey: 'report',
    },
    children: [
      {
        path: 'sales',
        name: 'ReportSales',
        component: () => import('@/views/report/sales/index.vue'),
        meta: {
          title: '销售报表',
          icon: 'TrendCharts',
          menuKey: 'report-sales',
        },
        children: [
          {
            path: 'region',
            name: 'ReportSalesRegion',
            component: () => import('@/views/report/sales/region/index.vue'),
            meta: {
              title: '区域销售',
              icon: 'LocationInformation',
              menuKey: 'report-sales-region',
            },
            children: [
              {
                path: 'east',
                name: 'ReportSalesRegionEast',
                component: () => import('@/views/report/sales/region/east.vue'),
                meta: {
                  title: '华东大区',
                  icon: 'OfficeBuilding',
                  menuKey: 'report-sales-region-east',
                },
              },
              {
                path: 'south',
                name: 'ReportSalesRegionSouth',
                component: () => import('@/views/report/sales/region/south.vue'),
                meta: {
                  title: '华南大区',
                  icon: 'School',
                  menuKey: 'report-sales-region-south',
                },
              },
            ],
          },
        ],
      },
      {
        path: 'traffic',
        name: 'ReportTraffic',
        component: () => import('@/views/report/traffic/index.vue'),
        meta: {
          title: '流量分析',
          icon: 'PieChart',
          menuKey: 'report-traffic',
        },
      },
      {
        path: 'demo',
        name: 'demo',
        component: () => import('@/views/demo.vue'),
        meta: {
          title: 'Demo',
          icon: 'PieChart',
          menuKey: 'report-demo',
        },
      },
    ],
  },
  {
    path: '/profile',
    name: 'Profile',
    component: () => import('@/views/profile/index.vue'),
    meta: {
      title: '个人中心',
      icon: 'User',
      menuKey: 'profile',
    },
  },
]
