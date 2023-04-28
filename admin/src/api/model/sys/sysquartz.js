import config from "@/config";
import http from "@/utils/request";
export default {
	list: {
		url: `${config.API_URL}/sysquartz`,
		name: "所有",
		get: async function () {
			return await http.get(this.url);
		},
	},
	logs: {
		url: `${config.API_URL}/sysquartz/jobrecord`,
		name: "查询任务记录",
		get: async function (params) {
			return await http.get(this.url, params);
		},
	},
	add: {
		url: `${config.API_URL}/sysquartz`,
		name: "添加",
		post: async function (data) {
			return await http.post(this.url, data);
		},
	},
	model: {
		url: `${config.API_URL}/sysquartz/`,
		name: "查询一条",
		get: async function (params) {
			return await http.get(this.url + params);
		},
	},
	update: {
		url: `${config.API_URL}/sysquartz`,
		name: "修改",
		put: async function (data) {
			return await http.put(this.url, data);
		},
	},
	start: {
		url: `${config.API_URL}/sysquartz/startjob`,
		name: "开启运行",
		put: async function (data) {
			return await http.put(this.url, data);
		},
	},
	run: {
		url: `${config.API_URL}/sysquartz/runjob`,
		name: "执行一次",
		put: async function (data) {
			return await http.put(this.url, data);
		},
	},
	pause: {
		url: `${config.API_URL}/sysquartz/pausejob`,
		name: "暂停",
		put: async function (data) {
			return await http.put(this.url, data);
		},
	},
	delete: {
		url: `${config.API_URL}/sysquartz`,
		name: "删除",
		delete: async function (data) {
			return await http.delete(this.url, data);
		},
	},
};
