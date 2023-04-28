<template>
	<el-container>
		<el-header class="header-tabs" style="display: flex">
			<div class="left-panel">
				<el-tabs
					type="card"
					v-model="param.typeId"
					@tab-change="tabChange"
				>
					<el-tab-pane
						v-for="it in typeOption"
						:key="it.value"
						:label="it.label"
						:name="it.value"
					></el-tab-pane>
				</el-tabs>
			</div>
			<div class="right-panel" style="padding-right: 10px">
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
		<el-header style="height: auto">
			<sc-select-filter
				:data="filterData"
				:label-width="80"
				@on-change="filterChange"
			></sc-select-filter>
		</el-header>
		<el-main class="nopadding">
			<scTable
				ref="table"
				:api-obj="apiObj"
				:column="column"
				row-key="id"
				:params="defaultParams"
				:hide-context-menu="false"
				@selection-change="selectionChange"
			>
				<!-- 固定列-选择列 -->
				<el-table-column fixed type="selection" width="60" />
				<el-table-column
					align="center"
					fixed="right"
					label="操作"
					width="140"
				>
					<template #default="scope">
						<el-button
							size="small"
							text
							type="primary"
							v-auth="'examuserlog:check'"
							@click="correcting(scope.row)"
						>
							批改
						</el-button>
					</template>
				</el-table-column>
				<template #examNumber="{ data }">
					<el-tag>
						{{ data.userNumber }}
					</el-tag>
				</template>
				<template #grandCode="{ data }">
					{{ data.grandCode.name }}
				</template>
				<template #subjectCode="{ data }">
					{{ data.subjectCode.name }}
				</template>
				<template #typeCode="{ data }">
					{{ data.typeCode.name }}
				</template>
				<template #totalSocre="{ data }">
					{{
						data.questionItem
							.map((item) => item.score)
							.reduce((prev, curr) => prev + curr, 0)
					}}
				</template>
				<template #questionSum="{ data }">
					{{ data.questionItem.length }}
				</template>
			</scTable>
		</el-main>
	</el-container>
</template>
<script>
export default {
	components: {},
	data() {
		return {
			apiObj: this.$API.exampaper.page,
			list: [],
			param: {
				key: "",
				subject: 0,
				typeId: 0,
				grand: "",
				status: "1",
				isUserLog: true,
			},
			defaultParams: {
				status: "1",
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
				{
					prop: "title",
					label: "试卷名称",
					width: 200,
					align: "left",
					fixed: "left",
				},
				{ prop: "grandCode", label: "年级", width: 100 },
				{ prop: "subjectCode", label: "学科", width: 100 },
				{ prop: "examNumber", label: "考试次数", width: 100 },
				{ prop: "typeCode", label: "试卷类型", width: 100 },
				{ prop: "totalSocre", label: "总分", width: 120 },
				{ prop: "questionSum", label: "题目数", width: 120 },
				{ prop: "minutesLength", label: "考试时长(分)", width: 120 },
				{ prop: "createTime", label: "创建时间", width: 180 },
			],
			filterData: [
				{
					title: "学科",
					key: "subject",
					options: [
						{
							label: "全部",
							value: "",
						},
					],
				},
				{
					title: "年级",
					key: "grand",
					multiple: true,
					options: [
						{
							label: "全部",
							value: "",
						},
					],
				},
			],
			typeOption: [{ label: "所有", value: 0 }],
		};
	},
	mounted() {
		this.initGrandOption();
		this.initSubjectOption();
		this.initTypeOption();
	},
	methods: {
		async initGrandOption() {
			const res = await this.$API.syscode.list.get({ typeCode: "grand" });
			if (res.code == 200) {
				res.data.forEach((e) => {
					this.filterData[1].options.push({
						label: e.name,
						value: e.id,
					});
				});
			}
		},
		async initSubjectOption() {
			const res = await this.$API.syscode.list.get({
				typeCode: "subject",
			});
			if (res.code == 200) {
				res.data.forEach((e) => {
					this.filterData[0].options.push({
						label: e.name,
						value: e.id,
					});
				});
			}
		},
		async initTypeOption() {
			const res = await this.$API.syscode.list.get({
				typeCode: "paperType",
			});
			if (res.code == 200) {
				res.data.forEach((e) => {
					this.typeOption.push({ label: e.name, value: e.id });
				});
			}
		},
		complete() {
			this.$refs.table.refresh();
		},
		search() {
			this.$refs.table.upData(this.param);
		},
		filterChange(data) {
			if (data.subject) {
				this.param.subject = data.subject;
			} else {
				this.param.subject = "0";
			}
			if (data.grand) {
				this.param.grand = data.grand;
			} else {
				this.param.grand = "";
			}
			this.$refs.table.upData(this.param);
		},
		tabChange() {
			this.$refs.table.upData(this.param);
		},
		correcting(row) {
			this.$router.push("/exam/userlog/detail/?id=" + row.id);
		},
		selectionChange(selection) {
			this.selection = selection;
		},
	},
};
</script>
