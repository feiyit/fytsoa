/**
 * @description 自动import导入所有 api 模块
 */

const files = require.context("./model", true, /\.js$/);
const modules = {};
files.keys().forEach((key) => {
	let k = key.replace(/(\.\/|\.js)/g, "");
	if (k.indexOf("/") > -1) {
		k = k.split("/").pop();
	}
	modules[k] = files(key).default;
});

export default modules;
