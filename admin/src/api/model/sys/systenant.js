import config from "@/config";
import http from "@/utils/request";
export default {
	page: {
		url: `${config.API_URL}/systenant/pages`,
		name: "列表",
		get: async function (params) {
			return await http.get(this.url, params);
		},
	},
	add: {
		url: `${config.API_URL}/systenant`,
		name: "添加",
		post: async function (data) {
			return await http.post(this.url, data);
		},
	},
	model: {
		url: `${config.API_URL}/systenant/`,
		name: "查询一条",
		get: async function (params) {
			return await http.get(this.url + params);
		},
	},
	update: {
		url: `${config.API_URL}/systenant`,
		name: "修改",
		put: async function (data) {
			return await http.put(this.url, data);
		},
	},
	resetPwd: {
		url: `${config.API_URL}/systenant/passreset`,
		name: "重置密码",
		put: async function (data) {
			return await http.put(this.url, data);
		},
	},
	delete: {
		url: `${config.API_URL}/systenant`,
		name: "删除",
		delete: async function (data) {
			return await http.delete(this.url, data);
		},
	},
};
