<template>
	<div class="sc-tableFilter">
		<slot :filterLength="filterObjLength" :openFilter="openFilter">
			<el-badge
				:value="filterObjLength"
				type="danger"
				:hidden="filterObjLength <= 0"
			>
				<el-button
					icon="el-icon-filter"
					@click="openFilter"
				></el-button>
			</el-badge>
		</slot>
		<el-dialog
			v-model="dialog"
			title="高级查询"
			width="800px"
			destroy-on-close
			draggable
		>
			<el-scrollbar>
				<div class="sc-filter-main">
					<h2>设置过滤条件</h2>
					<div v-if="filter.length <= 0" class="nodata">
						没有默认过滤条件，请点击增加过滤项
					</div>
					<table v-else>
						<colgroup>
							<col width="50" />
							<col width="160" />
							<col v-if="showOperator" width="150" />
							<col />
							<col width="40" />
						</colgroup>
						<tr v-for="(item, index) in filter" :key="index">
							<td>
								<el-tag>{{ index + 1 }}</el-tag>
							</td>
							<td>
								<el-select
									v-model="item.field"
									placeholder=""
									@change="fieldChange(item)"
								>
									<el-option
										v-for="it in filterArr"
										:key="it.prop"
										:label="it.label"
										:value="it.prop"
									></el-option>
								</el-select>
							</td>
							<td v-if="showOperator">
								<el-select
									v-model="item.operator"
									placeholder="运算符"
								>
									<el-option
										v-for="ope in operators"
										:key="ope.value"
										:label="ope.label"
										:value="ope.value"
									></el-option>
								</el-select>
							</td>
							<td>
								<!-- 输入框 -->
								<el-input
									v-if="item.type == 'text'"
									v-model="item.value"
									:placeholder="item.placeholder || '请输入'"
								></el-input>
								<!-- 日期 -->
								<el-date-picker
									v-if="item.type == 'date'"
									v-model="item.value"
									type="date"
									value-format="YYYY-MM-DD"
									:placeholder="
										item.placeholder || '请选择日期'
									"
									style="width: 100%"
								></el-date-picker>
								<!-- 日期范围 -->
								<el-date-picker
									v-if="item.type == 'daterange'"
									v-model="item.value"
									type="daterange"
									value-format="YYYY-MM-DD"
									start-placeholder="开始日期"
									end-placeholder="结束日期"
									style="width: 100%"
								></el-date-picker>
								<!-- 日期时间 -->
								<el-date-picker
									v-if="item.type == 'datetime'"
									v-model="item.value"
									type="datetime"
									value-format="YYYY-MM-DD HH:mm:ss"
									:placeholder="
										item.placeholder || '请选择日期'
									"
									style="width: 100%"
								></el-date-picker>
								<!-- 日期时间范围 -->
								<el-date-picker
									v-if="item.type == 'datetimerange'"
									v-model="item.value"
									type="datetimerange"
									value-format="YYYY-MM-DD HH:mm:ss"
									start-placeholder="开始日期"
									end-placeholder="结束日期"
									style="width: 100%"
								></el-date-picker>
								<!-- 开关 -->
								<el-switch
									v-if="item.type == 'switch'"
									v-model="item.value"
									active-value="1"
									inactive-value="0"
								></el-switch>
								<!-- 标签 -->
								<el-select
									v-if="item.type == 'tags'"
									v-model="item.value"
									multiple
									filterable
									allow-create
									default-first-option
									no-data-text="输入关键词后按回车确认"
									:placeholder="item.placeholder || '请输入'"
								></el-select>
								<!-- 下拉框 -->
								<el-select
									v-if="item.type == 'select'"
									v-model="item.value"
									:placeholder="item.placeholder || '请选择'"
									filterable
									:multiple="item.extend.multiple"
									:loading="item.selectLoading"
									@visible-change="
										visibleChange($event, item)
									"
									:remote="item.extend.remote"
									:remote-method="
										(query) => {
											remoteMethod(query, item);
										}
									"
								>
									<el-option
										v-for="field in item.extend.data"
										:key="field.value"
										:label="field.label"
										:value="field.value"
									></el-option>
								</el-select>
							</td>
							<td>
								<el-icon class="del" @click="delFilter(index)"
									><el-icon-delete
								/></el-icon>
							</td>
						</tr>
					</table>
					<el-button
						type="text"
						icon="el-icon-plus"
						@click="addFilter"
						>增加过滤项</el-button
					>
				</div>
			</el-scrollbar>

			<template #footer>
				<span class="dialog-footer">
					<el-button type="primary" @click="Ok">确定</el-button>
					<el-button @click="dialog = false">取消</el-button>
				</span>
			</template>
		</el-dialog>
	</div>
</template>
<script>
export default {
	name: "tableFilter",
	components: {},
	props: {
		column: { type: Object, default: () => {} },
		filterName: { type: String, default: "" },
		showOperator: { type: Boolean, default: true },
		options: { type: Object, default: () => {} },
	},
	watch: {},
	computed: {
		filterArr() {
			let arr = [];
			this.column.forEach((item) => {
				if (item.filter) {
					arr.push(item);
				}
			});
			return arr;
		},
	},
	data() {
		return {
			dialog: false,
			filter: [],
			filterObjLength: 0,
			operators: [
				{ label: "等于", value: 0 },
				{ label: "不等于", value: 10 },
				{ label: "包含", value: 1 },
				{ label: "不包含", value: 13 },
				{ label: "大于", value: 2 },
				{ label: "大于等于", value: 3 },
				{ label: "小于", value: 4 },
				{ label: "小于等于", value: 5 },
				{ label: "为空", value: 11 },
				{ label: "不为空", value: 12 },
			],
		};
	},
	methods: {
		Ok() {
			let json = [];
			this.filter.forEach((item) => {
				json.push({
					fieldName: item.field,
					conditionalType: item.operator,
					fieldValue: item.value.toString(),
				});
			});
			this.dialog = false;
			this.$emit("filterSubmit", json);
		},
		openFilter() {
			this.dialog = true;
		},
		addFilter() {
			if (this.filterArr.length == 0) {
				this.$message.warning("无过滤项");
				return;
			}
			this.filter.push({
				field: undefined,
				operator: 0,
				type: "text",
				value: "",
			});
		},
		delFilter(index) {
			this.filter.splice(index, 1);
		},
		fieldChange(it) {
			let _item = undefined;
			this.column.forEach((item) => {
				if (item.prop == it.field) {
					_item = item;
				}
			});
			it.type = _item.type;
			it.value = undefined;
			it.extend = _item.extend;
		},
		//下拉框显示事件处理异步
		async visibleChange(isopen, item) {
			if (isopen && item.extend.request && !item.extend.remote) {
				item.selectLoading = true;
				try {
					var data = await item.extend.request();
				} catch (error) {
					console.log(error);
				}
				item.extend.data = data;
				item.selectLoading = false;
			}
		},
		//下拉框显示事件处理异步搜索
		async remoteMethod(query, item) {
			if (query !== "") {
				item.selectLoading = true;
				try {
					var data = await item.extend.request(query);
				} catch (error) {
					console.log(error);
				}
				item.extend.data = data;
				item.selectLoading = false;
			} else {
				item.extend.data = [];
			}
		},
	},
};
</script>

<style scoped>
.sc-tableFilter {
	margin-left: 12px;
}

.nodata {
	height: 46px;
	line-height: 46px;
	margin: 15px 0;
	border: 1px dashed #e6e6e6;
	color: #999;
	text-align: center;
	border-radius: 3px;
}

.sc-filter-main {
	padding: 0px 20px;
	border-bottom: 1px solid #e6e6e6;
	background: #fff;
}
.sc-filter-main h2 {
	font-size: 12px;
	color: #999;
	font-weight: normal;
}
.sc-filter-main table {
	width: 100%;
	margin: 15px 0;
}
.sc-filter-main table tr {
}
.sc-filter-main table td {
	padding: 5px 10px 5px 0;
}
.sc-filter-main table td:deep(.el-input .el-input__inner) {
	vertical-align: top;
}
.sc-filter-main table td .el-select {
	display: block;
}
.sc-filter-main table td .el-date-editor.el-input {
	display: block;
	width: 100%;
}
.sc-filter-main table td .del {
	background: #fff;
	color: #999;
	width: 32px;
	height: 32px;
	line-height: 32px;
	text-align: center;
	border-radius: 50%;
	font-size: 12px;
	cursor: pointer;
}
.sc-filter-main table td .del:hover {
	background: #f56c6c;
	color: #fff;
}
</style>
