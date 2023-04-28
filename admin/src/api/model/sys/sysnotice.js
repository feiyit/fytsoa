import config from "@/config";
import http from "@/utils/request";
export default {
	page: {
		url: `${config.API_URL}/sysnotice/pages`,
		name: "列表",
		get: async function (params) {
			return await http.get(this.url, params);
		},
	},
	total: {
		url: `${config.API_URL}/sysnotice/total`,
		name: "统计数量",
		get: async function () {
			return await http.get(this.url);
		},
	},
	add: {
		url: `${config.API_URL}/sysnotice`,
		name: "添加",
		post: async function (data) {
			return await http.post(this.url, data);
		},
	},
	model: {
		url: `${config.API_URL}/sysnotice/`,
		name: "查询一条",
		get: async function (params) {
			return await http.get(this.url + params);
		},
	},
	update: {
		url: `${config.API_URL}/sysnotice`,
		name: "修改",
		put: async function (data) {
			return await http.put(this.url, data);
		},
	},
	status: {
		url: `${config.API_URL}/sysnotice/status`,
		name: "修改状态",
		put: async function (data) {
			return await http.put(this.url, data);
		},
	},
	read: {
		url: `${config.API_URL}/sysnotice/read`,
		name: "已读",
		put: async function (data) {
			return await http.put(this.url, data);
		},
	},
	clearRead: {
		url: `${config.API_URL}/sysnotice/clearread`,
		name: "取消已读",
		put: async function (data) {
			return await http.put(this.url, data);
		},
	},
	delete: {
		url: `${config.API_URL}/sysnotice`,
		name: "删除",
		delete: async function (data) {
			return await http.delete(this.url, data);
		},
	},
};
