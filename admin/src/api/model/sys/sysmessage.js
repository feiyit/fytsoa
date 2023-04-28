import config from "@/config"
import http from "@/utils/request"
export default {
	page: {
		url: `${config.API_URL}/sysmessage/pages`,
		name: "列表",
		get: async function(params){
			return await http.get(this.url,params);
		}
	},
	add: {
		url: `${config.API_URL}/sysmessage`,
		name: "添加",
		post: async function(data){
			return await http.post(this.url, data);
		}
	},
	model: {
		url: `${config.API_URL}/sysmessage/`,
		name: "查询一条",
		get: async function(params){
			return await http.get(this.url + params);
		}
	},
	total: {
		url: `${config.API_URL}/sysmessage/total`,
		name: "汇总消息数量",
		get: async function(){
			return await http.get(this.url);
		}
	},
	update: {
		url: `${config.API_URL}/sysmessage`,
		name: "修改",
		put: async function(data){
			return await http.put(this.url,data);
		}
	},
	read: {
		url: `${config.API_URL}/sysmessage/read`,
		name: "设置可读",
		put: async function(data){
			return await http.put(this.url,data);
		}
	},
	delete: {
		url: `${config.API_URL}/sysmessage`,
		name: "删除",
		delete: async function(params){
			return await http.delete(this.url+'?ids='+params);
		}
	},
	recycle: {
		url: `${config.API_URL}/sysmessage/recycle`,
		name: "删除到回收站",
		delete: async function(params){
			return await http.delete(this.url+'?ids='+params);
		}
	}
}


