//数据表格配置

import tool from "@/utils/tool";

export default {
	successCode: 200, //请求完成代码
	limit: 20, //表格每一页条数
	pageSizes: [10, 20, 50, 100, 500, 1000],
	paginationLayout: "total, sizes, prev, pager, next, jumper",
	parseData: function (res) {
		//数据分析
		return {
			data: res.data, //分析无分页的数据字段结构
			rows: res.data.rows, //分析行数据字段结构
			total: res.data.total, //分析总数字段结构
			summary: res.data.summary, //分析合计行字段结构
			msg: res.message, //分析描述字段结构
			code: res.code, //分析状态字段结构
		};
	},
	request: {
		//请求规定字段
		page: "page", //规定当前分页字段
		limit: "limit", //规定一页条数字段
		prop: "prop", //规定排序字段名字段
		order: "order", //规定排序规格字段
	},
	/**
	 * 自定义列保存处理
	 * @tableName scTable组件的props->tableName
	 * @column 用户配置好的列
	 */
	columnSettingSave: function (tableName, column) {
		return new Promise((resolve) => {
			setTimeout(() => {
				//这里为了演示使用了session和setTimeout演示，开发时应用数据请求
				tool.session.set(tableName, column);
				resolve(true);
			}, 1000);
		});
	},
	/**
	 * 获取自定义列
	 * @tableName scTable组件的props->tableName
	 * @column 组件接受到的props->column
	 */
	columnSettingGet: function (tableName, column) {
		return new Promise((resolve) => {
			//这里为了演示使用了session和setTimeout演示，开发时应用数据请求
			const userColumn = tool.session.get(tableName);
			if (userColumn) {
				resolve(userColumn);
			} else {
				resolve(column);
			}
		});
	},
	/**
	 * 重置自定义列
	 * @tableName scTable组件的props->tableName
	 * @column 组件接受到的props->column
	 */
	columnSettingReset: function (tableName, column) {
		return new Promise((resolve) => {
			//这里为了演示使用了session和setTimeout演示，开发时应用数据请求
			setTimeout(() => {
				tool.session.remove(tableName);
				resolve(column);
			}, 1000);
		});
	},
	changeTree: function (data) {
		if (data.length > 0) {
			data.forEach((item) => {
				const parentId = item.parentId;
				if (parentId) {
					data.forEach((ele) => {
						if (ele.id === parentId) {
							let childArray = ele.children;
							if (!childArray) {
								childArray = [];
							}

							childArray.push(item);
							ele.children = childArray;
						}
					});
				}
			});
		}
		return data.filter((item) => item.parentId == "0");
	},
};
