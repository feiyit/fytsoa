import config from "@/config";
import http from "@/utils/request";
export default {
	page: {
		url: `${config.API_URL}/examcomment/pages`,
		name: "列表",
		get: async function (params) {
			return await http.get(this.url, params);
		},
	},
	add: {
		url: `${config.API_URL}/examcomment`,
		name: "添加",
		post: async function (data) {
			return await http.post(this.url, data);
		},
	},
	model: {
		url: `${config.API_URL}/examcomment/`,
		name: "查询一条",
		get: async function (params) {
			return await http.get(this.url + params);
		},
	},
	update: {
		url: `${config.API_URL}/examcomment`,
		name: "修改",
		put: async function (data) {
			return await http.put(this.url, data);
		},
	},
	audit: {
		url: `${config.API_URL}/examcomment/audit`,
		name: "审核",
		put: async function (data) {
			return await http.put(this.url, data);
		},
	},
	delete: {
		url: `${config.API_URL}/examcomment`,
		name: "删除",
		delete: async function (data) {
			return await http.delete(this.url, data);
		},
	},
};
