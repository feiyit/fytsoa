import config from "@/config";
import http from "@/utils/request";
export default {
	page: {
		url: `${config.API_URL}/exammaterial/pages`,
		name: "列表",
		get: async function (params) {
			return await http.get(this.url, params);
		},
	},
	add: {
		url: `${config.API_URL}/exammaterial`,
		name: "添加",
		post: async function (data) {
			return await http.post(this.url, data);
		},
	},
	model: {
		url: `${config.API_URL}/exammaterial/`,
		name: "查询一条",
		get: async function (params) {
			return await http.get(this.url + params);
		},
	},
	update: {
		url: `${config.API_URL}/exammaterial`,
		name: "修改",
		put: async function (data) {
			return await http.put(this.url, data);
		},
	},
	reName: {
		url: `${config.API_URL}/exammaterial/rename`,
		name: "文件重命名",
		put: async function (data) {
			return await http.put(this.url, data);
		},
	},
	delete: {
		url: `${config.API_URL}/exammaterial`,
		name: "删除",
		delete: async function (data) {
			return await http.delete(this.url, data);
		},
	},
	upload: {
		url: `${config.API_URL}/exammaterial/upload`,
		name: "上传知识库素材",
		post: async function (data) {
			return await http.post(this.url + "?path=/upload/knowledge/", data);
		},
	},
};
