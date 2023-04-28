import config from "@/config"
import http from "@/utils/request"
export default {
	list: {
		url: `${config.API_URL}/generator/table`,
		name: "列表",
		get: async function(){
			return await http.get(this.url);
		}
	},
	code: {
		url: `${config.API_URL}/generator`,
		name: "添加",
		post: async function(data){
			return await http.post(this.url, data);
		}
	},
	column: {
		url: `${config.API_URL}/generator/column`,
		name: "列表",
		get: async function(params){
			return await http.get(this.url,params);
		}
	},
}


