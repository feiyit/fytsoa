import config from "@/config";
import http from "@/utils/request";
export default {
	page: {
		url: `${config.API_URL}/exampaper/pages`,
		name: "列表",
		get: async function (params) {
			return await http.get(this.url, params);
		},
	},
	add: {
		url: `${config.API_URL}/exampaper`,
		name: "添加",
		post: async function (data) {
			return await http.post(this.url, data);
		},
	},
	model: {
		url: `${config.API_URL}/exampaper/`,
		name: "查询一条",
		get: async function (params) {
			return await http.get(this.url + params);
		},
	},
	info: {
		url: `${config.API_URL}/exampaper/paper/`,
		name: "查询一条,包括用户考试次数",
		get: async function (params) {
			return await http.get(this.url + params);
		},
	},
	update: {
		url: `${config.API_URL}/exampaper`,
		name: "修改",
		put: async function (data) {
			return await http.put(this.url, data);
		},
	},
	release: {
		url: `${config.API_URL}/exampaper/release`,
		name: "发布",
		put: async function (data) {
			return await http.put(this.url, data);
		},
	},
	undo: {
		url: `${config.API_URL}/exampaper/undo`,
		name: "撤销",
		put: async function (data) {
			return await http.put(this.url, data);
		},
	},
	delete: {
		url: `${config.API_URL}/exampaper`,
		name: "删除",
		delete: async function (data) {
			return await http.delete(this.url, data);
		},
	},
};
