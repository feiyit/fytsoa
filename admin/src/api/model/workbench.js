import config from "@/config";
import http from "@/utils/request";
export default {
	resourceuse: {
		url: `${config.API_URL}/workbench/resourceuse`,
		name: "系统资源使用情况",
		get: async function (params) {
			return await http.get(this.url, params);
		},
	},
	systeminfo: {
		url: `${config.API_URL}/workbench/systeminfo`,
		name: "读取系统信息",
		get: async function (params) {
			return await http.get(this.url, params);
		},
	},
};
