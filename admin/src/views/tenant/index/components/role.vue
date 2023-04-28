<template>
	<scTable
		ref="table"
		:api-obj="apiObj"
		:column="column"
		:params="{ tenantId: tenant }"
		row-key="id"
		height="400"
		hide-pagination
		:hideContextMenu="false"
		is-tree
	>
		<el-table-column
			label="#"
			type="index"
			width="50"
			fixed="left"
		></el-table-column>
		<template #status="{ data }">
			<el-tag
				disable-transitions
				:type="data.status ? 'success' : 'danger'"
			>
				{{ data.status ? "正常" : "停用" }}
			</el-tag>
		</template>
		<template #isSystem="{ data }">
			<el-tag :type="data.isSystem ? 'success' : 'danger'">
				{{ data.isSystem ? "是" : "否" }}
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
			apiObj: this.$API.sysrole.list,
			column: [
				{
					label: "id",
					prop: "id",
					width: "100",
					sortable: true,
					hide: true,
				},
				{
					label: "角色名称",
					prop: "name",
					align: "left",
				},
				{
					label: "是否超管",
					prop: "isSystem",
					width: "100",
					filters: [
						{ text: "是", value: "1" },
						{ text: "否", value: "0" },
					],
				},
				{
					label: "状态",
					prop: "status",
					width: "100",
					filters: [
						{ text: "正常", value: "1" },
						{ text: "异常", value: "0" },
					],
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
