import config from "@/config";
import http from "@/utils/request";
export default {
	page: {
		url: `${config.API_URL}/examquestion/pages`,
		name: "分页-列表",
		get: async function (params) {
			return await http.get(this.url, params);
		},
	},
	list: {
		url: `${config.API_URL}/examquestion/list`,
		name: "列表",
		get: async function (params) {
			return await http.get(this.url, params);
		},
	},
	smart: {
		url: `${config.API_URL}/examquestion/smart`,
		name: "智能组卷",
		post: async function (data) {
			return await http.post(this.url, data);
		},
	},
	add: {
		url: `${config.API_URL}/examquestion`,
		name: "添加",
		post: async function (data) {
			return await http.post(this.url, data);
		},
	},
	model: {
		url: `${config.API_URL}/examquestion/`,
		name: "查询一条",
		get: async function (params) {
			return await http.get(this.url + params);
		},
	},
	update: {
		url: `${config.API_URL}/examquestion`,
		name: "修改",
		put: async function (data) {
			return await http.put(this.url, data);
		},
	},
	delete: {
		url: `${config.API_URL}/examquestion`,
		name: "删除",
		delete: async function (data) {
			return await http.delete(this.url, data);
		},
	},
};
