import API from "@/api";
import config from "@/config";

//上传配置

export default {
	apiObj: API.sysfile.upload,			//上传请求API对象
	filename: "file",					//form请求时文件的key
	successCode: 200,					//请求完成代码
	maxSize: 100,						//最大文件大小 默认100MB
	parseData: function (res) {
		return {
			code: res.code,				//分析状态字段结构
			fileName: res.data.name,//分析文件名称
			src:config.SERVER_URL + res.data.path,			//分析图片远程地址结构
			msg: res.message			//分析描述字段结构
		}
	},
	apiObjFile: API.sysfile.uploadFile,	//附件上传请求API对象
	maxSizeFile: 100						//最大文件大小 默认100MB
}
