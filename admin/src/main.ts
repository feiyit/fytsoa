import App from './App.vue'
import router from './router'
import pinia from './stores'
import ElementPlus from 'element-plus'
import * as ElementPlusIconsVue from '@element-plus/icons-vue'
import { setupFytBootstrap, setupFytDirectives, setupFytGlobalComponents } from './fyt'
import STable from '@shene/table';
import '@shene/table/dist/index.css';
import 'element-plus/dist/index.css'
import 'element-plus/theme-chalk/dark/css-vars.css'
import './styles/index.css'
import './styles/dark.css'
import 'nprogress/nprogress.css'


const app = createApp(App)

app.use(pinia)
app.use(router)
app.use(ElementPlus)
app.use(STable);

setupFytGlobalComponents(app)

// 注册自定义指令
setupFytDirectives(app)

// 初始化主题及全局 loading 逻辑
setupFytBootstrap()

for (const [key, component] of Object.entries(ElementPlusIconsVue)) {
  app.component(key, component)
}

app.mount('#app')
