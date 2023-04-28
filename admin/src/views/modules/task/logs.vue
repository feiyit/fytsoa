<template>
	<el-drawer
		title="计划任务日志"
		v-model="dialog.logsVisible"
		:size="600"
		direction="rtl"
		destroy-on-close
	>
		<el-main style="padding: 0 20px">
			<el-table
				ref="table"
				:data="list"
				row-key="beginDate"
				height="500"
				stripe
			>
				<el-table-column
					label="开始时间"
					prop="beginDate"
					width="180"
				></el-table-column>
				<el-table-column
					label="结束时间"
					prop="endDate"
					width="180"
				></el-table-column>
				<el-table-column
					label="执行结果"
					prop="msg"
					min-width="300"
				></el-table-column>
			</el-table>
			<el-pagination
				background
				layout="prev, pager, next, jumper"
				style="margin-top:10px;"
				:small="true"
				:total="total"
				@current-change="handleCurrentChange"
			/>
		</el-main>
	</el-drawer>
</template>

<script>
export default {
	data() {
		return {
			dialog: {
				logsVisible: false,
			},
			list: [],
			params: {
				taskName: undefined,
				groupName: undefined,
				current: 1,
				size: 10,
			},
			total:0,
			loading: true,
		};
	},
	methods: {
		async init() {
			const res = await this.$API.sysquartz.logs.get(this.params);
			if (res.code == 200) {
				this.total=res.data.total
				this.list = res.data.data;
			}
		},
		handleCurrentChange (number){
			this.params.current=number
			this.init()
		},
		opens(m) {
			this.params.taskName = m.taskName;
			this.params.groupName = m.groupName;
			this.init();
			this.dialog.logsVisible = true;
		},
		close() {
			this.dialog.logsVisible = false;
		},
	},
};
</script>

<style>
.quartz-log-wall .el-dialog__body {
	background: #171717;
	color: #2cc730;
}
.quartz-log-wall .log-main {
	height: 400px;
	overflow: auto;
}
</style>
