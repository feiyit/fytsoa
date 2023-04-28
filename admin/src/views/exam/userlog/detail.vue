<template>
	<el-container class="bg">
		<el-header class="exam-h-detail">
			<el-row :gutter="5">
				<el-col :lg="6">
					<el-card shadow="never">
						<sc-statistic
							title="总分"
							:value="
								model.questionItem
									.map((item) => item.score)
									.reduce((prev, curr) => prev + curr, 0)
							"
							groupSeparator
						></sc-statistic>
					</el-card>
				</el-col>
				<el-col :lg="6">
					<el-card shadow="never">
						<sc-statistic
							title="参考人数"
							:value="total"
							suffix="人"
							groupSeparator
						></sc-statistic>
					</el-card>
				</el-col>
				<el-col :lg="6">
					<el-card shadow="never">
						<sc-statistic
							title="题目总数"
							:value="model.questionItem.length"
							suffix="个"
							groupSeparator
						></sc-statistic>
					</el-card>
				</el-col>
				<el-col :lg="6">
					<el-card shadow="never">
						<sc-statistic
							title="考试时长"
							:value="model.minutesLength"
							suffix="分钟"
							groupSeparator
						></sc-statistic>
					</el-card>
				</el-col>
			</el-row>
		</el-header>
		<el-descriptions title="试卷信息" style="padding: 20px">
			<el-descriptions-item label="试卷名称：">{{
				model.title
			}}</el-descriptions-item>
			<el-descriptions-item label="试卷类型：">{{
				model.typeCode.name
			}}</el-descriptions-item>
			<el-descriptions-item label="创建人：">{{
				model.createUser
			}}</el-descriptions-item>
			<el-descriptions-item label="年级：">
				{{ model.grandCode.name }}
			</el-descriptions-item>
			<el-descriptions-item label="学科：">
				{{ model.subjectCode.name }}
			</el-descriptions-item>
			<el-descriptions-item label="创建时间：">{{
				model.createTime
			}}</el-descriptions-item>
		</el-descriptions>
		<el-header>
			<div class="left-panel"></div>
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
				:params="defaultParams"
				:hide-context-menu="false"
			>
				<template #nickName="{ data }">
					{{ data.userMember.nickName }}
				</template>
				<template #loginName="{ data }">
					{{ data.userMember.loginName }}
				</template>
				<template #ratio="{ data }">
					{{ data.questionItem.filter((m) => m.judge).length }}/{{
						data.questionItem.length
					}}
				</template>
				<template #status="{ data }">
					<el-tag
						disable-transitions
						:type="data.status ? 'success' : 'danger'"
					>
						{{ data.status ? "完成" : "待批改" }}
					</el-tag>
				</template>
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
							@click="look(scope.row, 1)"
						>
							查看
						</el-button>
						<el-divider direction="vertical" />
						<el-button
							size="small"
							text
							type="primary"
							@click="look(scope.row, 2)"
						>
							批改
						</el-button>
					</template>
				</el-table-column>
			</scTable>
		</el-main>
	</el-container>
</template>
<script>
import scStatistic from "@/components/scStatistic";
export default {
	components: {
		scStatistic,
	},
	data() {
		return {
			apiObj: this.$API.examuserlog.page,
			model: {
				typeCode: {},
				subjectCode: {},
				grandCode: {},
				questionItem: [],
			},
			total: 0,
			defaultParams: { id: 0 },
			param: { key: "" },
			column: [
				{
					label: "id",
					prop: "id",
					width: "200",
					sortable: true,
					hide: true,
				},
				{ prop: "loginName", label: "用户名", width: 100 },
				{ prop: "nickName", label: "真实姓名", width: 100 },
				{ prop: "ratio", label: "正确比", width: 100 },
				{ prop: "useMinutes", label: "耗时", width: 100 },
				{ prop: "status", label: "完成状态", width: 120 },
				{ prop: "createTime", label: "创建时间", width: 180 },
			],
		};
	},
	mounted() {
		this.defaultParams.id = this.$route.query.id;
		this.info(this.$route.query.id);
	},
	methods: {
		async info(id) {
			const res = await this.$API.exampaper.info.get(id);
			if (res.code == 200) {
				this.model = res.data;
			} else {
				this.$alert(res.message, "提示", { type: "error" });
			}
			this.total = this.$refs.table.total;
		},
		search() {
			this.$refs.table.upData(this.param);
		},
		look(item, type) {
			this.$router.push("/exam/userlog/correct");
			this.$route.id = item.id;
			this.$route.type = type;
		},
	},
};
</script>
<style lang="scss" scoped>
.bg {
	background-color: #ffffff;
}
.exam-h-detail {
	padding: 15px 15px 0px 15px;
	display: block;
	height: auto;
}
</style>
