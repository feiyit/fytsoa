import config from "@/config"
import http from "@/utils/request"
export default {
	save: {
		url: `${config.API_URL}/syssafety`,
		name: "添加",
		post: async function(data){
			return await http.post(this.url, data);
		}
	},
	model: {
		url: `${config.API_URL}/syssafety`,
		name: "查询一条",
		get: async function(){
			return await http.get(this.url);
		}
	},
}


