<!--
 * @Descripttion: 数据表格组件
 * @version: 1.7
 * @Author: sakuya
 * @Date: 2021年11月29日21:51:15
 * @LastEditors: sakuya
 * @LastEditTime: 2022年2月9日09:59:37
-->

<template>
	<div
		class="scTable"
		:style="{ height: _height }"
		ref="scTableMain"
		v-loading="loading"
	>
		<div class="scTable-table" :style="{ height: _table_height }">
			<el-table
				v-bind="$attrs"
				:data="tableData"
				:row-key="rowKey"
				:key="toggleIndex"
				default-expand-all
				ref="scTable"
				:height="height == 'auto' ? null : '100%'"
				:size="config.size"
				:border="config.border"
				:stripe="config.stripe"
				:summary-method="
					remoteSummary ? remoteSummaryMethod : summaryMethod
				"
				@row-contextmenu="rowContextmenu"
				@sort-change="sortChange"
				@filter-change="filterChange"
			>
				<slot></slot>
				<template v-for="(item, index) in userColumn" :key="index">
					<el-table-column
						v-if="!item.hide"
						highlight-current-row
						:align="item.align == null ? 'center' : item.align"
						:column-key="item.prop"
						:label="item.label"
						:prop="item.prop"
						:min-width="item.width"
						:sortable="item.sortable"
						:fixed="item.fixed"
						:filters="item.filters"
						:filter-method="
							remoteFilter || !item.filters ? null : filterHandler
						"
						:show-overflow-tooltip="item.showOverflowTooltip"
					>
						<template #default="scope">
							<slot
								:name="item.prop"
								v-bind="scope"
								:data="scope.row"
							>
								{{ scope.row[item.prop] }}
							</slot>
						</template>
					</el-table-column>
				</template>
				<template #empty>
					<el-empty
						:description="emptyText"
						:image-size="100"
					></el-empty>
				</template>
			</el-table>
		</div>
		<div class="scTable-page">
			<div class="scTable-pagination">
				<el-pagination
					v-if="!hidePagination"
					:currentPage="currentPage"
					:page-size="pageSize"
					:page-sizes="scPageSize"
					:layout="paginationLayout"
					:total="total"
					background
					:small="true"
					@size-change="paginationSizeChange"
					@current-change="paginationChange"
				/>
			</div>
			<div class="scTable-do" v-if="!hideDo">
				<el-button
					v-if="!hideRefresh"
					@click="refresh"
					icon="el-icon-refresh"
					circle
					style="margin-left: 15px"
				></el-button>
				<el-popover
					v-if="!hidePrint"
					placement="top"
					ref="printPover"
					title="打印列设置"
					:width="400"
					trigger="click"
					:hide-after="0"
				>
					<template #reference>
						<el-button
							icon="el-icon-printer"
							circle
							style="margin-left: 15px"
						></el-button>
					</template>
					<columnPrint
						:column="column"
						:data="tableData"
						@printCancel="printCancel"
					></columnPrint>
				</el-popover>
				<el-popover
					v-if="!hideExcel"
					placement="top"
					ref="excelPover"
					title="导出Excel设置"
					:width="400"
					trigger="click"
					:hide-after="0"
				>
					<template #reference>
						<el-button
							icon="sc-icon-file-excel"
							circle
							style="margin-left: 15px"
						></el-button>
					</template>
					<columnExcel
						:column="column"
						:data="tableData"
						@excelCancel="excelCancel"
					></columnExcel>
				</el-popover>
				<el-popover
					v-if="column"
					placement="top"
					title="列设置"
					:width="500"
					trigger="click"
					:hide-after="0"
					@show="customColumnShow = true"
					@after-leave="customColumnShow = false"
				>
					<template #reference>
						<el-button
							icon="el-icon-set-up"
							circle
							style="margin-left: 15px"
						></el-button>
					</template>
					<columnSetting
						v-if="customColumnShow"
						ref="columnSetting"
						@userChange="columnSettingChange"
						@save="columnSettingSave"
						@back="columnSettingBack"
						:column="userColumn"
					></columnSetting>
				</el-popover>
				<el-popover
					v-if="!hideSetting"
					placement="top"
					title="表格设置"
					:width="400"
					trigger="click"
					:hide-after="0"
				>
					<template #reference>
						<el-button
							icon="el-icon-setting"
							circle
							style="margin-left: 15px"
						></el-button>
					</template>
					<el-form label-width="80px" label-position="left">
						<el-form-item label="表格尺寸">
							<el-radio-group
								v-model="config.size"
								size="small"
								@change="configSizeChange"
							>
								<el-radio-button label="large"
									>大</el-radio-button
								>
								<el-radio-button label="default"
									>正常</el-radio-button
								>
								<el-radio-button label="small"
									>小</el-radio-button
								>
							</el-radio-group>
						</el-form-item>
						<el-form-item label="样式">
							<el-checkbox
								v-model="config.border"
								label="纵向边框"
							></el-checkbox>
							<el-checkbox
								v-model="config.stripe"
								label="斑马纹"
							></el-checkbox>
						</el-form-item>
					</el-form>
				</el-popover>
			</div>
		</div>
		<sc-contextmenu
			ref="contextmenu"
			@command="handleCommand"
			@visible-change="visibleChange"
		>
			<sc-contextmenu-item
				v-if="menuDefault.includes('add')"
				command="add"
				title="添加"
				icon="el-icon-plus"
				suffix="Add"
			></sc-contextmenu-item>
			<sc-contextmenu-item
				v-if="menuDefault.includes('edit')"
				command="edit"
				title="编辑"
				icon="el-icon-edit"
				suffix="Uelete"
			></sc-contextmenu-item>
			<sc-contextmenu-item
				v-if="menuDefault.includes('delete')"
				command="delete"
				title="删除"
				icon="el-icon-delete"
				suffix="Delete"
			></sc-contextmenu-item>
			<sc-contextmenu-item
				v-for="(item, index) in menuColumn"
				:key="index"
				:command="item.command"
				:title="item.title"
				:icon="item.icon"
				:suffix="item.suffix"
			></sc-contextmenu-item>
			<sc-contextmenu-item
				command="refresh"
				title="重新加载(R)"
				icon="el-icon-refresh"
				divided
			></sc-contextmenu-item>
			<sc-contextmenu-item
				command="close"
				title="关闭"
				divided
			></sc-contextmenu-item>
		</sc-contextmenu>
	</div>
</template>

<script>
import config from "@/config/table";
import columnSetting from "./columnSetting";
import columnPrint from "./columnPrint";
import columnExcel from "./columnExcel";
import scContextmenu from "@/components/scContextmenu";
import scContextmenuItem from "@/components/scContextmenu/item";

export default {
	name: "scTable",
	components: {
		columnSetting,
		columnPrint,
		columnExcel,
		scContextmenu,
		scContextmenuItem,
	},
	props: {
		tableName: { type: String, default: "" },
		apiObj: { type: Object, default: () => {} },
		params: { type: Object, default: () => ({}) },
		data: { type: Object, default: () => {} },
		height: { type: [String, Number], default: "100%" },
		size: { type: String, default: "default" },
		pageSizes: { type: Array, default: config.pageSizes },
		border: { type: Boolean, default: false },
		stripe: { type: Boolean, default: false },
		isTree: { type: Boolean, default: false },
		limit: { type: Number, default: config.limit },
		rowKey: { type: String, default: "" },
		summaryMethod: { type: Function, default: null },
		column: { type: Object, default: () => {} },
		remoteSort: { type: Boolean, default: false },
		remoteFilter: { type: Boolean, default: false },
		remoteSummary: { type: Boolean, default: false },
		hidePagination: { type: Boolean, default: false },
		hideDo: { type: Boolean, default: false },
		hideRefresh: { type: Boolean, default: false },
		hidePrint: { type: Boolean, default: false },
		hideExcel: { type: Boolean, default: false },
		hideSetting: { type: Boolean, default: false },
		hideContextMenu: { type: Boolean, default: true },
		menuColumn: { type: Object, default: () => {} },
		menuDefault: { type: Array, default: () => ["add", "edit", "delete"] },
		paginationLayout: {
			type: String,
			default: config.paginationLayout,
		},
	},
	watch: {
		//监听从props里拿到值了
		data() {
			this.tableData = this.data;
			this.total = this.tableData.length;
		},
		apiObj() {
			this.tableParams = this.params;
			this.refresh();
		},
		column() {
			this.userColumn = this.column;
		},
	},
	computed: {
		_height() {
			return Number(this.height)
				? Number(this.height) + "px"
				: this.height;
		},
		_table_height() {
			return this.hidePagination && this.hideDo
				? "100%"
				: "calc(100% - 50px)";
		},
	},
	data() {
		return {
			scPageSize: this.pageSize,
			isActivat: true,
			emptyText: "暂无数据",
			toggleIndex: 0,
			tableData: [],
			total: 0,
			currentPage: 1,
			pageSize: this.limit,
			prop: null,
			order: null,
			loading: false,
			tableHeight: "100%",
			tableParams: this.params,
			userColumn: [],
			customColumnShow: false,
			summary: {},
			tableRow: {},
			config: {
				size: this.size,
				border: this.border,
				stripe: this.stripe,
			},
		};
	},
	mounted() {
		//判断是否开启自定义列
		if (this.column) {
			this.getCustomColumn();
		} else {
			this.userColumn = this.column;
		}
		//判断是否静态数据
		if (this.apiObj) {
			this.getData();
		} else if (this.data) {
			this.tableData = this.data;
			this.total = this.tableData.length;
		}
	},
	activated() {
		if (!this.isActivat) {
			this.$refs.scTable.doLayout();
		}
	},
	deactivated() {
		this.isActivat = false;
	},
	methods: {
		//获取列
		async getCustomColumn() {
			const userColumn = await config.columnSettingGet(
				this.tableName,
				this.column
			);
			this.userColumn = userColumn;
		},
		//获取数据
		async getData() {
			this.loading = true;
			var reqData = {
				[config.request.page]: this.currentPage,
				[config.request.limit]: this.pageSize,
				[config.request.prop]: this.prop,
				[config.request.order]: this.order,
			};
			if (this.hidePagination) {
				delete reqData[config.request.page];
				delete reqData[config.request.limit];
			}
			Object.assign(reqData, this.tableParams);
			var res = await this.apiObj.get(reqData);
			this.loading = false;
			if (res.code != 200) {
				this.$message.error(res.message);
				return;
			}
			if (this.isTree) {
				this.tableData = config.changeTree(res.data);
				return;
			}
			if (res.data.items) {
				this.tableData = res.data.items;
				this.total = parseInt(res.data.totalItems);
			} else {
				this.tableData = res.data;
			}
			this.$refs.scTable.setScrollTop(0);
			this.$emit("dataChange", res, this.tableData);
		},
		//分页点击
		paginationChange(number) {
			this.currentPage = number;
			this.getData();
		},
		paginationSizeChange(number) {
			this.pageSize = number;
			this.currentPage = 1;
			this.getData();
		},
		getTotal() {
			return this.total;
		},
		//刷新数据
		refresh() {
			this.$refs.scTable.clearSelection();
			this.getData();
		},
		//更新数据 合并上一次params
		upData(params, page = 1) {
			this.currentPage = page;
			this.$refs.scTable.clearSelection();
			Object.assign(this.tableParams, params || {});
			this.getData();
		},
		//重载数据 替换params
		reload(params, page = 1) {
			this.currentPage = page;
			this.tableParams = params || {};
			this.$refs.scTable.clearSelection();
			this.$refs.scTable.clearSort();
			this.$refs.scTable.clearFilter();
			this.getData();
		},
		//自定义变化事件
		columnSettingChange(userColumn) {
			this.userColumn = userColumn;
			this.toggleIndex += 1;
		},
		//自定义列保存
		async columnSettingSave(userColumn) {
			this.$refs.columnSetting.isSave = true;
			try {
				await config.columnSettingSave(this.tableName, userColumn);
			} catch (error) {
				this.$message.error("保存失败");
				this.$refs.columnSetting.isSave = false;
			}
			this.$message.success("保存成功");
			this.$refs.columnSetting.isSave = false;
		},
		//自定义列重置
		async columnSettingBack() {
			this.$refs.columnSetting.isSave = true;
			try {
				const column = await config.columnSettingReset(
					this.tableName,
					this.column
				);
				this.userColumn = column;
				this.$refs.columnSetting.usercolumn = JSON.parse(
					JSON.stringify(this.userColumn || [])
				);
			} catch (error) {
				this.$message.error("重置失败");
				this.$refs.columnSetting.isSave = false;
			}
			this.$refs.columnSetting.isSave = false;
		},
		//排序事件
		sortChange(obj) {
			if (!this.remoteSort) {
				return false;
			}
			if (obj.column && obj.prop) {
				this.prop = obj.prop;
				this.order = obj.order;
			} else {
				this.prop = null;
				this.order = null;
			}
			this.getData();
		},
		//本地过滤
		filterHandler(value, row, column) {
			const property = column.property;
			return row[property] === value;
		},
		//过滤事件
		filterChange(filters) {
			if (!this.remoteFilter) {
				return false;
			}
			Object.keys(filters).forEach((key) => {
				filters[key] = filters[key].join(",");
			});
			this.upData(filters);
		},
		//远程合计行处理
		remoteSummaryMethod(param) {
			const { columns } = param;
			const sums = [];
			columns.forEach((column, index) => {
				if (index === 0) {
					sums[index] = "合计";
					return;
				}
				const values = this.summary[column.property];
				if (values) {
					sums[index] = values;
				} else {
					sums[index] = "";
				}
			});
			return sums;
		},
		configSizeChange() {
			this.$refs.scTable.doLayout();
		},
		//原生方法转发
		clearSelection() {
			this.$refs.scTable.clearSelection();
		},
		toggleRowSelection(row, selected) {
			this.$refs.scTable.toggleRowSelection(row, selected);
		},
		toggleAllSelection() {
			this.$refs.scTable.toggleAllSelection();
		},
		toggleRowExpansion(row, expanded) {
			this.$refs.scTable.toggleRowExpansion(row, expanded);
		},
		setCurrentRow(row) {
			this.$refs.scTable.setCurrentRow(row);
		},
		clearSort() {
			this.$refs.scTable.clearSort();
		},
		clearFilter(columnKey) {
			this.$refs.scTable.clearFilter(columnKey);
		},
		doLayout() {
			this.$refs.scTable.doLayout();
		},
		sort(prop, order) {
			this.$refs.scTable.sort(prop, order);
		},
		handleCommand(command) {
			if (command == "refresh") {
				this.refresh();
				return;
			}
			if (command == "close") {
				this.$refs.contextmenu.closeMenu();
				return;
			}
			this.$emit("menu-handle", { command, row: this.tableRow });
		},
		rowContextmenu(row, column, event) {
			if (!this.hideContextMenu) {
				return;
			}
			this.tableRow = row;
			this.$refs.scTable.setCurrentRow(row);
			this.$refs.contextmenu.openMenu(event);
		},
		visibleChange(visible) {
			if (!visible) {
				this.$refs.scTable.setCurrentRow();
			}
		},
		printCancel() {
			this.$refs.printPover.hide();
		},
		excelCancel() {
			this.$refs.excelPover.hide();
		},
	},
};
</script>

<style scoped>
.scTable {
}
.scTable-table {
	height: calc(100% - 50px);
}
.scTable-page {
	height: 50px;
	display: flex;
	align-items: center;
	justify-content: space-between;
	padding: 0 15px;
}
.scTable-do {
	white-space: nowrap;
}
.scTable:deep(.el-table__footer) .cell {
	font-weight: bold;
}
</style>
