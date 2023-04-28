import config from "@/config"
import http from "@/utils/request"
export default {
	page: {
		url: `${config.API_URL}/syspost/pages`,
		name: "列表",
		get: async function(params){
			return await http.get(this.url,params);
		}
	},
	list: {
		url: `${config.API_URL}/syspost/all`,
		name: "所有",
		get: async function(){
			return await http.get(this.url);
		}
	},
	add: {
		url: `${config.API_URL}/syspost`,
		name: "添加",
		post: async function(data){
			return await http.post(this.url, data);
		}
	},
	model: {
		url: `${config.API_URL}/syspost/`,
		name: "查询一条",
		get: async function(params){
			return await http.get(this.url + params);
		}
	},
	update: {
		url: `${config.API_URL}/syspost`,
		name: "修改",
		put: async function(data){
			return await http.put(this.url,data);
		}
	},
	delete: {
		url: `${config.API_URL}/syspost`,
		name: "删除",
		delete: async function(params){
			return await http.delete(this.url+'?ids='+params);
		}
	}
}
