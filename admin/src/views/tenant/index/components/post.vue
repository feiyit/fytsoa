<template>
	<scTable
		ref="table"
		:api-obj="apiObj"
		:column="column"
		:params="{ tenantId: tenant }"
		row-key="id"
		:hideContextMenu="false"
		height="400"
	>
		<el-table-column label="#" type="index" width="50"></el-table-column>
		<template #status="{ data }">
			<el-tag
				disable-transitions
				:type="data.status ? 'success' : 'danger'"
			>
				{{ data.status ? "正常" : "停用" }}
			</el-tag>
		</template>
	</scTable>
</template>
<script>
export default {
	components: {},
	props: {
		tenant: { type: String, default: "0" },
	},
	data() {
		return {
			apiObj: this.$API.syspost.page,
			column: [
				{
					label: "id",
					prop: "id",
					width: "200",
					sortable: true,
					hide: true,
				},
				{
					label: "岗位编号",
					prop: "number",
					width: 120,
				},
				{
					label: "岗位名称",
					prop: "name",
					align: "left",
				},
				{
					label: "排序",
					prop: "sort",
					width: "100",
				},
				{
					label: "状态",
					prop: "status",
					width: "100",
				},
				{
					label: "创建时间",
					prop: "createTime",
					width: "180",
					sortable: true,
					fixed: "right",
				},
			],
		};
	},
	mounted() {},
	methods: {},
};
</script>
