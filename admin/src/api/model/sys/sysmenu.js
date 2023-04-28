import config from "@/config";
import http from "@/utils/request";
export default {
	list: {
		url: `${config.API_URL}/sysmenu/list`,
		name: "列表",
		get: async function (params) {
			return await http.get(this.url, params);
		},
	},
	add: {
		url: `${config.API_URL}/sysmenu`,
		name: "添加",
		post: async function (data) {
			return await http.post(this.url, data);
		},
	},
	temp: {
		url: `${config.API_URL}/sysmenu/temp`,
		name: "添加",
		post: async function (data) {
			return await http.post(this.url, data);
		},
	},
	model: {
		url: `${config.API_URL}/sysmenu/`,
		name: "查询一条",
		get: async function (params) {
			return await http.get(this.url + params);
		},
	},
	update: {
		url: `${config.API_URL}/sysmenu`,
		name: "修改",
		put: async function (data) {
			return await http.put(this.url, data);
		},
	},
	dragging: {
		url: `${config.API_URL}/sysmenu/dragging`,
		name: "修改",
		put: async function (data) {
			return await http.put(this.url, data);
		},
	},
	delete: {
		url: `${config.API_URL}/sysmenu`,
		name: "删除",
		delete: async function (params) {
			return await http.delete(this.url + "?ids=" + params);
		},
	},
	authority: {
		url: `${config.API_URL}/sysmenu/authoritymenu`,
		name: "获得权限",
		get: async function () {
			return await http.get(this.url);
		},
	},
};
