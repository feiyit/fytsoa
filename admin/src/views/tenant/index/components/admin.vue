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
		<template #headPic="{ data }">
			<el-avatar
				:src="$CONFIG.SERVER_URL + data.headPic"
				size="small"
			></el-avatar>
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
			apiObj: this.$API.sysadmin.page,
			column: [
				{
					label: "id",
					prop: "id",
					width: "200",
					sortable: true,
					hide: true,
				},
				{
					label: "头像",
					prop: "headPic",
					width: "100",
					align: "left",
				},
				{
					label: "姓名",
					prop: "fullName",
					width: "120",
					align: "left",
				},
				{
					label: "所属角色",
					prop: "roleGroupName",
					align: "left",
					width: "120",
				},
				{
					label: "性别",
					prop: "sex",
					width: "80",
				},
				{
					label: "手机号码",
					prop: "mobile",
					width: "120",
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
	created() {},
	methods: {},
};
</script>
