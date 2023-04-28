import config from "@/config";
import http from "@/utils/request";
export default {
	page: {
		url: `${config.API_URL}/cmsarticle/pages`,
		name: "列表",
		get: async function (data) {
			return await http.post(this.url, data);
		},
	},
	add: {
		url: `${config.API_URL}/cmsarticle`,
		name: "添加",
		post: async function (data) {
			return await http.post(this.url, data);
		},
	},
	model: {
		url: `${config.API_URL}/cmsarticle/`,
		name: "查询一条",
		get: async function (params) {
			return await http.get(this.url + params);
		},
	},
	update: {
		url: `${config.API_URL}/cmsarticle`,
		name: "修改",
		put: async function (data) {
			return await http.put(this.url, data);
		},
	},
	recycle: {
		url: `${config.API_URL}/cmsarticle/recycle`,
		name: "删除到回收站",
		put: async function (data) {
			return await http.put(this.url, data);
		},
	},
	recovery: {
		url: `${config.API_URL}/cmsarticle/recovery`,
		name: "回收站-恢复",
		put: async function (data) {
			return await http.put(this.url, data);
		},
	},
	delete: {
		url: `${config.API_URL}/cmsarticle`,
		name: "删除",
		delete: async function (params) {
			return await http.delete(this.url + "?ids=" + params);
		},
	},
};
