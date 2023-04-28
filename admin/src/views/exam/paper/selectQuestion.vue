<template>
	<sc-dialog
		v-model="drawer"
		title="选择题目项"
		:append-to-body="true"
		width="1000px"
		top="10vh"
		custom-class="questiondialog"
		:destroy-on-close="true"
	>
		<el-header>
			<div class="left-panel"></div>
			<div class="right-panel">
				<el-input v-model="param.key" clearable placeholder="关键字" />
				<el-button
					icon="el-icon-search"
					type="primary"
					@click="search"
				/>
				<el-button icon="el-icon-check" type="primary" @click="save"
					>确定选择</el-button
				>
			</div>
		</el-header>
		<el-main class="nopadding">
			<scTable
				ref="tableRef"
				:api-obj="apiObj"
				:column="column"
				:params="defaultParams"
				row-key="id"
				@selection-change="selectionChange"
			>
				<!-- 固定列-选择列 -->
				<el-table-column fixed type="selection" width="60" />
				<template #status="{ data }">
					<el-tag
						disable-transitions
						:type="data.status ? 'success' : 'danger'"
					>
						{{ data.status ? "正常" : "停用" }}
					</el-tag>
				</template>
				<template #grandCode="{ data }">
					{{ data.grandCode.name }}
				</template>
				<template #subjectCode="{ data }">
					{{ data.subjectCode.name }}
				</template>
				<template #type="{ data }">
					{{ resQuestionType(data.type) }}
				</template>
			</scTable>
		</el-main>
	</sc-dialog>
</template>
<script>
export default {
	emits: ["completeSelect"],
	data() {
		return {
			apiObj: this.$API.examquestion.page,
			param: {
				key: "",
			},
			defaultParams: {
				grand: "",
				subject: "",
			},
			selection: [],
			drawer: false,
			column: [
				{
					label: "id",
					prop: "id",
					width: "200",
					sortable: true,
					hide: true,
				},
				{
					prop: "title",
					label: "题目",
					width: 300,
					align: "left",
					fixed: "left",
				},
				{ prop: "grandCode", label: "年级编号", width: 100 },
				{ prop: "subjectCode", label: "学科编号", width: 100 },
				{ prop: "type", label: "类型", width: 100 },
				{ prop: "score", label: "分数", width: 80 },
				{ prop: "difficulty", label: "难度", width: 80 },
				{ prop: "createTime", label: "创建时间", width: 180 },
			],
		};
	},
	mounted() {},
	methods: {
		search() {
			this.$refs.tableRef.upData(this.param);
		},
		selectionChange(selection) {
			this.selection = selection;
		},
		save() {
			this.$emit("completeSelect", this.selection);
			this.drawer = false;
		},
		resQuestionType(number) {
			let str = "解答题";
			switch (number) {
				case 1:
					str = "单选题";
					break;
				case 2:
					str = "多选题";
					break;
				case 3:
					str = "判断题";
					break;
				case 4:
					str = "填空题";
					break;
			}
			return str;
		},
		open(row) {
			this.defaultParams.grand = row.grand;
			this.defaultParams.subject = row.subject;
			this.drawer = true;
		},
	},
};
</script>
<style>
.questiondialog .el-dialog__body {
	padding-top: 0px;
}
</style>
