import { createApp } from "vue";
import ElementPlus from "element-plus";
import "element-plus/dist/index.css";
import "element-plus/theme-chalk/display.css";
import scui from "./scui";
import i18n from "./locales";
import router from "./router";
import store from "./store";
import App from "./App.vue";

import vue3TreeOrg from "vue3-tree-org";
import "vue3-tree-org/lib/vue3-tree-org.css";

const app = createApp(App);

app.use(vue3TreeOrg);
app.use(store);
app.use(router);
app.use(ElementPlus, { size: "default" });
app.use(i18n);
app.use(scui);

//挂载app
app.mount("#app");
