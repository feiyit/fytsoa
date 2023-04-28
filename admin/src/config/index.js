const DEFAULT_CONFIG = {
	//标题
	APP_NAME: process.env.VUE_APP_TITLE,

	//应用ID
	APP_KEY: "870cb6cfa286ac4cac462bc8e33b1bb5",

	//参数加密签名Key
	SIGN_KEY: "ab517f95fab7d57",

	//首页地址
	DASHBOARD_URL: "/dashboard",

	//版本号
	APP_VER: "1.6.7",

	//内核版本号
	CORE_VER: "1.6.7",

	//接口地址
	API_URL: process.env.VUE_APP_API_BASEURL + "/api",

	//服务器地址
	SERVER_URL: process.env.VUE_APP_API_BASEURL,

	//SignalR地址
	SignalR_URL: process.env.VUE_APP_API_BASEURL + "/chathub",

	//请求超时
	TIMEOUT: 10000,

	//TokenName
	TOKEN_NAME: "accessToken",

	//Token前缀，注意最后有个空格，如不需要需设置空字符串
	TOKEN_PREFIX: "",

	//追加其他头
	HEADERS: {},

	//请求是否开启缓存
	REQUEST_CACHE: false,

	//布局 默认：default | 通栏：header | 经典：menu | 功能坞：dock
	//dock将关闭标签和面包屑栏
	LAYOUT: "default",

	//菜单是否折叠
	MENU_IS_COLLAPSE: false,

	//菜单是否启用手风琴效果
	MENU_UNIQUE_OPENED: false,

	//是否开启多标签
	LAYOUT_TAGS: true,

	//语言
	LANG: "zh-cn",

	//主题颜色
	COLOR: "",

	//控制台首页默认布局
	DEFAULT_GRID: {
		//默认分栏数量和宽度 例如 [24] [18,6] [8,8,8] [6,12,6]
		layout: [12, 6, 6],
		//小组件分布，com取值:views/home/components 文件名
		copmsList: [["welcome", "ver"], ["time", "progress"], ["about"]],
	},
};
//合并业务配置
import MY_CONFIG from "./myConfig";
Object.assign(DEFAULT_CONFIG, MY_CONFIG);

// 如果生产模式，就合并动态的APP_CONFIG
// public/config.js
if (process.env.NODE_ENV === "production") {
	Object.assign(DEFAULT_CONFIG, APP_CONFIG);
}

export default DEFAULT_CONFIG;
