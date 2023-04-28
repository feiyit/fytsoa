<template>
	<sc-dialog
		v-model="visible"
		show-fullscreen
		:title="titleMap[mode]"
		width="1000px"
		heigit="700px"
		destroy-on-close
		@close="close"
	>
		<scTable
			ref="table"
			:api-obj="apiObj"
			:column="column"
			row-key="dbColumnName"
			:height="400"
			:hidePagination="true"
		>
			<el-table-column
				label="#"
				type="index"
				width="50"
			></el-table-column>
			<template #isPrimarykey="{ data }">
				<el-tag
					disable-transitions
					:type="data.isPrimarykey ? 'success' : 'danger'"
				>
					{{ data.isPrimarykey ? "是" : "否" }}
				</el-tag>
			</template>
			<template #isNullable="{ data }">
				<el-tag
					disable-transitions
					:type="data.isNullable ? 'success' : 'danger'"
				>
					{{ data.isNullable ? "是" : "否" }}
				</el-tag>
			</template>
		</scTable>

		<template #footer>
			<el-button @click="close">关 闭</el-button>
		</template>
	</sc-dialog>
</template>
<script>
export default {
	data() {
		return {
			apiObj: this.$API.modulesgenerate.column,
			mode: "add",
			titleMap: {
				add: "表详情",
			},
			visible: false,
			column: [
				{
					label: "id",
					prop: "id",
					width: "200",
					sortable: true,
					hide: true,
				},
				{
					label: "列名",
					prop: "dbColumnName",
					width: 200,
				},
				{
					label: "数据类型",
					prop: "dataType",
					width: 100,
				},
				{
					label: "是否主键",
					prop: "isPrimarykey",
					width: 100,
				},
				{
					label: "是否允许为空",
					prop: "isNullable",
					width: 120,
				},
				{
					label: "描述",
					prop: "columnDescription",
					align: "left",
				},
			],
		};
	},
	methods: {
		async init(param) {
			this.apiObj = this.$API.modulesgenerate.column;
			const that = this;
			setTimeout(function () {
				that.$refs.table.upData({ tableName: param });
			}, 100);
		},
		async open(row) {
			this.init(row.name);
			this.visible = true;
		},
		close() {
			this.visible = false;
		},
	},
};
</script>
