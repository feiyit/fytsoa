import config from "@/config";
import http from "@/utils/request";
export default {
	page: {
		url: `${config.API_URL}/examuserlog/pages`,
		name: "列表",
		get: async function (params) {
			return await http.get(this.url, params);
		},
	},
	add: {
		url: `${config.API_URL}/examuserlog`,
		name: "添加",
		post: async function (data) {
			return await http.post(this.url, data);
		},
	},
	modelCorrect: {
		url: `${config.API_URL}/examuserlog/usercorrect/`,
		name: "提供批改查询用户提交信息",
		get: async function (params) {
			return await http.get(this.url + params);
		},
	},
	model: {
		url: `${config.API_URL}/examuserlog/`,
		name: "查询一条",
		get: async function (params) {
			return await http.get(this.url + params);
		},
	},
	update: {
		url: `${config.API_URL}/examuserlog`,
		name: "修改",
		put: async function (data) {
			return await http.put(this.url, data);
		},
	},
	correct: {
		url: `${config.API_URL}/examuserlog/correct`,
		name: "提交批改",
		put: async function (data) {
			return await http.put(this.url, data);
		},
	},
	delete: {
		url: `${config.API_URL}/examuserlog`,
		name: "删除",
		delete: async function (params) {
			return await http.delete(this.url + "?ids=" + params);
		},
	},
};
