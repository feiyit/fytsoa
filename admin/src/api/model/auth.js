import config from "@/config"
import http from "@/utils/request"

export default {
	token: {
		url: `${config.API_URL}/operator/login`,
		name: "登录获取TOKEN",
		post: async function(data={}){
			return await http.post(this.url, data);
		}
	},
	user: {
		url: `${config.API_URL}/operator/userword`,
		name: "查询登录人信息",
		get: async function(data={}){
			return await http.get(this.url, data);
		}
	},

}
