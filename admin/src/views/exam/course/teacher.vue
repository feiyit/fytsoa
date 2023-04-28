<template>
	<sc-dialog
		v-model="visible"
		show-fullscreen
		:title="titleMap[mode]"
		width="1000px"
		top="8vh"
		class="cur-dialog"
		@close="close"
	>
		<el-header>
			<div class="right-panel">
				<div class="right-panel-search">
					<el-input
						v-model="param.key"
						clearable
						placeholder="关键字"
					/>
					<el-button
						icon="el-icon-search"
						type="primary"
						@click="search"
					/>
				</div>
			</div>
		</el-header>
		<el-main class="nopadding">
			<scTable
				ref="table"
				:api-obj="apiObj"
				:column="column"
				row-key="id"
				@menu-handle="menuHandle"
				@selection-change="selectionChange"
			>
				<!-- 固定列-选择列 -->
				<el-table-column
					align="center"
					fixed="right"
					label="操作"
					width="140"
				>
					<template #default="scope">
						<el-button
							text
							type="primary"
							size="small"
							@click="select(scope.row)"
						>
							确定选择
						</el-button>
					</template>
				</el-table-column>
				<template #professionCode="{ data }">
					{{ data.professionCode.name }}
				</template>
				<template #name="{ data }">
					<el-avatar class="user-avatar" :src="data.avatar" />{{
						data.name
					}}
				</template>
			</scTable>
		</el-main>
	</sc-dialog>
</template>
<script>
export default {
	data() {
		return {
			mode: "add",
			titleMap: {
				add: "选择讲师",
			},
			visible: false,
			apiObj: this.$API.examteacher.page,
			list: [],
			param: {
				key: "",
			},
			selection: [],
			column: [
				{
					label: "id",
					prop: "id",
					width: "200",
					sortable: true,
					hide: true,
				},
				{ prop: "name", label: "教师姓名", width: 200, align: "left" },
				{ prop: "professionCode", label: "专业", width: 100 },
				{ prop: "postName", label: "职称", width: 100 },
				{ prop: "age", label: "年龄", width: 100 },
				{ prop: "createTime", label: "创建时间", width: 180 },
			],
		};
	},
	mounted() {},
	methods: {
		search() {
			this.$refs.table.upData(this.param);
		},
		async open() {
			this.visible = true;
		},
		select(m) {
			this.$emit("complete", m);
			this.visible = false;
		},
		close() {
			this.visible = false;
		},
	},
};
</script>
<style scoped>
.user-avatar {
	vertical-align: middle;
	margin-right: 5px;
}
.cur-dialog >>> .el-dialog__body {
	padding: 10px 20px;
}
</style>
