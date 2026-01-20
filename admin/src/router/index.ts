import { createRouter, createWebHashHistory, type RouteRecordRaw } from 'vue-router'
import NProgress from 'nprogress'
import Layout from '@/layout/Layout.vue'
import LoginView2 from '@/views/Login.vue'
import LoginView from '@/views/login/index.vue'
// import { dynamicRoutes } from './dynamic-routes'
import { appStorage, STORAGE_KEYS } from '@/utils'
import { getServerDynamicRoutes } from "@/utils/routeHandler";

NProgress.configure({ showSpinner: false })

const routes: RouteRecordRaw[] = [
  {
    path: '/login2',
    name: 'Login2',
    component: LoginView2,
    meta: {
      title: '登录2',
    },
  },
  {
    path: '/login',
    name: 'Login',
    component: LoginView,
    meta: {
      title: '登录',
    },
  },
  {
    path: '/',
    name: 'Layout',
    component: Layout,
    children: [
      {
        path: '',
        redirect: '/workspace',
      },
    ],
  },
  {
    path: '/:pathMatch(.*)*',
    name: 'NotFound',
    component: () => import('@/views/NotFound.vue'),
    meta: {
      title: '页面未找到',
    },
  },
]

const WHITE_LIST: string[] = ['/login','/login2']

const router = createRouter({
  // history: createWebHistory(import.meta.env.BASE_URL),
  history: createWebHashHistory(import.meta.env.BASE_URL),
  routes,
})

// 动态挂载业务路由到 Layout 下面
// 要求：不在 views 目录下再嵌套 <RouterView />，所以这里将动态路由“拍平”，
// 让所有业务路由都作为 Layout 的直接子路由，在同一个内容区域渲染。
const addDynamicRoutes = (records: RouteRecordRaw[], parentPath = '') => {
  records.forEach((record) => {
    const rawPath = String(record.path || '')
    const fullPath = rawPath.startsWith('/')
      ? rawPath
      : `${parentPath}/${rawPath}`.replace(/\/+/g, '/')

    // 只要这个记录本身有 component，就单独注册成一个子路由
    if (record.component) {
      router.addRoute('Layout', {
        path: fullPath,
        name: record.name,
        component: record.component,
        meta: record.meta,
      })
    }

    // 继续递归处理 children，但不再保持嵌套结构
    if (record.children && record.children.length) {
      addDynamicRoutes(record.children as RouteRecordRaw[], fullPath)
    }
  })
}

// 使用本地定义的 dynamicRoutes 作为业务路由来源
const res=getServerDynamicRoutes();
//addDynamicRoutes(dynamicRoutes)
addDynamicRoutes(res)

router.beforeEach((to, _from, next) => {
  if (to.meta?.title) {
    document.title = String(to.meta.title)
  }
  NProgress.start()

  const token = appStorage.get<string>(STORAGE_KEYS.ACCESS_TOKEN)

  // 已登录
  if (token) {
    // 已登录用户访问登录页时，跳转到首页或重定向地址
    if (to.path === '/login') {
      const redirect = (to.query.redirect as string) || '/'
      next(redirect)
      return
    }
    next()
    return
  }

  // 未登录：白名单直接放行
  if (WHITE_LIST.includes(to.path)) {
    next()
    return
  }

  // 未登录且访问受保护路由：跳转登录，并带上返回地址
  next({
    path: '/login',
    query: { redirect: to.fullPath },
  })
})

router.afterEach(() => {
  NProgress.done()
})

export default router
