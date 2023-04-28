<template>
	<el-container>
		<el-header>
			<el-page-header
				:content="type == 1 ? '查看试卷' : '批改试卷'"
				@back="goBack"
			/>
		</el-header>
		<el-container>
			<el-aside class="correct-left" width="260px">
				<div class="title">测试试卷</div>
				<div class="item">考生：{{ user.nickName }}</div>
				<div class="item">
					得分：{{
						model.questionItem
							.map((item) => item.score)
							.reduce((prev, curr) => prev + curr, 0)
					}}
				</div>
				<div class="item">考试时长：{{ paper.minutesLength }}分钟</div>
				<div class="item">耗时：{{ model.useMinutes }}秒</div>
				<el-divider />
				<div class="question-num">
					<el-tag
						:type="
							it.judge == null
								? 'info'
								: it.judge
								? 'success'
								: 'danger'
						"
						v-for="(it, index) in model.questionItem"
						:key="index"
						size="large"
						>{{ (index += 1) }}</el-tag
					>
				</div>
			</el-aside>
			<el-main class="bg">
				<el-scrollbar height="83vh">
					<div
						class="question-type"
						v-if="
							model.questionItem.filter(
								(m) => m.question.type == 1
							).length > 0
						"
					>
						单选题
					</div>
					<div
						class="paper-question"
						v-for="(it, index) in model.questionItem"
						:key="index"
						v-show="it.question.type == 1"
					>
						<div class="title">{{ (index += 1) }}</div>
						<div class="content">
							<div class="item">
								{{ it.question.title }}（{{
									it.sourceScore
								}}）分
							</div>
							<div
								v-for="o in it.question.subjectItem"
								:key="o"
								class="item"
							>
								{{ o.tag }}：{{ o.value }}
							</div>
							<div class="item" v-if="it.question.type != 4">
								标答：{{ it.question.answer }}
							</div>
							<div class="item" v-if="it.question.type == 4">
								<el-row :gutter="10">
									<el-col :span="2">标答：</el-col>
									<el-col :span="12">
										<p
											v-for="r in JSON.parse(
												it.question.answer
											)"
											:key="r.name"
										>
											{{ r.name }}
										</p>
									</el-col>
								</el-row>
							</div>
							<div class="item user">
								用户回答：{{ it.answer }}
							</div>
							<div class="judge" v-if="type == 2">
								<div class="judge-title user">批改：</div>
								<el-radio-group v-model="it.judge">
									<el-radio
										v-for="(item, index) in judgeOptions"
										:key="index"
										:label="item.value"
										@change="judgeChange(it)"
										>{{ item.label }}</el-radio
									>
								</el-radio-group>
							</div>
							<div class="judge" v-if="type == 2">
								<div class="judge-title user">得分：</div>
								<el-input-number
									v-model="it.score"
									placeholder=""
									:step="1"
								></el-input-number>
							</div>
							<div class="item">
								难度：<el-rate
									v-model="it.question.difficulty"
									:max="5"
									disabled
									style="position: relative; top: 5px"
								></el-rate>
							</div>
							<div class="item">
								解析：{{ it.question.parsing }}
							</div>
						</div>
					</div>
					<div
						class="question-type"
						v-if="
							model.questionItem.filter(
								(m) => m.question.type == 2
							).length > 0
						"
					>
						多选题
					</div>
					<div
						class="paper-question"
						v-for="(it, index) in model.questionItem"
						:key="index"
						v-show="it.question.type == 2"
					>
						<div class="title">{{ (index += 1) }}</div>
						<div class="content">
							<div class="item">
								{{ it.question.title }}（{{
									it.sourceScore
								}}）分
							</div>
							<div
								v-for="o in it.question.subjectItem"
								:key="o"
								class="item"
							>
								{{ o.tag }}：{{ o.value }}
							</div>
							<div class="item" v-if="it.question.type != 4">
								标答：{{ it.question.answer }}
							</div>
							<div class="item" v-if="it.question.type == 4">
								<el-row :gutter="10">
									<el-col :span="2">标答：</el-col>
									<el-col :span="12">
										<p
											v-for="r in JSON.parse(
												it.question.answer
											)"
											:key="r.name"
										>
											{{ r.name }}
										</p>
									</el-col>
								</el-row>
							</div>
							<div class="item user">
								用户回答：{{ it.answer }}
							</div>
							<div class="judge" v-if="type == 2">
								<div class="judge-title user">批改：</div>
								<el-radio-group v-model="it.judge">
									<el-radio
										v-for="(item, index) in judgeOptions"
										:key="index"
										:label="item.value"
										@change="judgeChange(it)"
										>{{ item.label }}</el-radio
									>
								</el-radio-group>
							</div>
							<div class="judge" v-if="type == 2">
								<div class="judge-title user">得分：</div>
								<el-input-number
									v-model="it.score"
									placeholder=""
									:step="1"
								></el-input-number>
							</div>
							<div class="item">
								难度：<el-rate
									v-model="it.question.difficulty"
									:max="5"
									disabled
									style="position: relative; top: 5px"
								></el-rate>
							</div>
							<div class="item">
								解析：{{ it.question.parsing }}
							</div>
						</div>
					</div>
					<div
						class="question-type"
						v-if="
							model.questionItem.filter(
								(m) => m.question.type == 3
							).length > 0
						"
					>
						判断题
					</div>
					<div
						class="paper-question"
						v-for="(it, index) in model.questionItem"
						:key="index"
						v-show="it.question.type == 3"
					>
						<div class="title">{{ (index += 1) }}</div>
						<div class="content">
							<div class="item">
								{{ it.question.title }}（{{
									it.sourceScore
								}}）分
							</div>
							<div
								v-for="o in it.question.subjectItem"
								:key="o"
								class="item"
							>
								{{ o.tag }}：{{ o.value }}
							</div>
							<div class="item" v-if="it.question.type != 4">
								标答：{{ it.question.answer }}
							</div>
							<div class="item" v-if="it.question.type == 4">
								<el-row :gutter="10">
									<el-col :span="2">标答：</el-col>
									<el-col :span="12">
										<p
											v-for="r in JSON.parse(
												it.question.answer
											)"
											:key="r.name"
										>
											{{ r.name }}
										</p>
									</el-col>
								</el-row>
							</div>
							<div class="item user">
								用户回答：{{ it.answer }}
							</div>
							<div class="judge" v-if="type == 2">
								<div class="judge-title user">批改：</div>
								<el-radio-group v-model="it.judge">
									<el-radio
										v-for="(item, index) in judgeOptions"
										:key="index"
										:label="item.value"
										@change="judgeChange(it)"
										>{{ item.label }}</el-radio
									>
								</el-radio-group>
							</div>
							<div class="judge" v-if="type == 2">
								<div class="judge-title user">得分：</div>
								<el-input-number
									v-model="it.score"
									placeholder=""
									:step="1"
								></el-input-number>
							</div>
							<div class="item">
								难度：<el-rate
									v-model="it.question.difficulty"
									:max="5"
									disabled
									style="position: relative; top: 5px"
								></el-rate>
							</div>
							<div class="item">
								解析：{{ it.question.parsing }}
							</div>
						</div>
					</div>
					<div
						class="question-type"
						v-if="
							model.questionItem.filter(
								(m) => m.question.type == 4
							).length > 0
						"
					>
						填空题
					</div>
					<div
						class="paper-question"
						v-for="(it, index) in model.questionItem"
						:key="index"
						v-show="it.question.type == 4"
					>
						<div class="title">{{ (index += 1) }}</div>
						<div class="content">
							<div class="item">
								{{ it.question.title }}（{{
									it.sourceScore
								}}）分
							</div>
							<div
								v-for="o in it.question.subjectItem"
								:key="o"
								class="item"
							>
								{{ o.tag }}：{{ o.value }}
							</div>
							<div class="item" v-if="it.question.type == 4">
								<el-row :gutter="10">
									<el-col :span="2">标答：</el-col>
									<el-col :span="12">
										<p
											v-for="r in JSON.parse(
												it.question.answer
											)"
											:key="r.name"
										>
											{{ r.name }}
										</p>
									</el-col>
								</el-row>
							</div>
							<div class="item user" v-if="it.question.type == 4">
								<el-row :gutter="10">
									<el-col :span="2">用户回答：</el-col>
									<el-col :span="12">
										<p
											v-for="r in JSON.parse(it.answer)"
											:key="r.name"
										>
											{{ r.name }}
										</p>
									</el-col>
								</el-row>
							</div>
							<div class="judge" v-if="type == 2">
								<div class="judge-title user">批改：</div>
								<el-radio-group v-model="it.judge">
									<el-radio
										v-for="(item, index) in judgeOptions"
										:key="index"
										:label="item.value"
										@change="judgeChange(it)"
										>{{ item.label }}</el-radio
									>
								</el-radio-group>
							</div>
							<div class="judge" v-if="type == 2">
								<div class="judge-title user">得分：</div>
								<el-input-number
									v-model="it.score"
									placeholder=""
									:step="1"
								></el-input-number>
							</div>
							<div class="item">
								难度：<el-rate
									v-model="it.question.difficulty"
									:max="5"
									disabled
									style="position: relative; top: 5px"
								></el-rate>
							</div>
							<div class="item">
								解析：{{ it.question.parsing }}
							</div>
						</div>
					</div>
					<div
						class="question-type"
						v-if="
							model.questionItem.filter(
								(m) => m.question.type == 5
							).length > 0
						"
					>
						解答题
					</div>
					<div
						class="paper-question"
						v-for="(it, index) in model.questionItem"
						:key="index"
						v-show="it.question.type == 5"
					>
						<div class="title">{{ (index += 1) }}</div>
						<div class="content">
							<div class="item">
								{{ it.question.title }}（{{
									it.sourceScore
								}}）分
							</div>
							<div
								v-for="o in it.question.subjectItem"
								:key="o"
								class="item"
							>
								{{ o.tag }}：{{ o.value }}
							</div>
							<div class="item" v-if="it.question.type != 4">
								标答：{{ it.question.answer }}
							</div>
							<div class="item" v-if="it.question.type == 4">
								<el-row :gutter="10">
									<el-col :span="2">标答：</el-col>
									<el-col :span="12">
										<p
											v-for="r in JSON.parse(
												it.question.answer
											)"
											:key="r.name"
										>
											{{ r.name }}
										</p>
									</el-col>
								</el-row>
							</div>
							<div class="item user">
								用户回答：{{ it.answer }}
							</div>
							<div class="judge" v-if="type == 2">
								<div class="judge-title user">批改：</div>
								<el-radio-group v-model="it.judge">
									<el-radio
										v-for="(item, index) in judgeOptions"
										:key="index"
										:label="item.value"
										@change="judgeChange(it)"
										>{{ item.label }}</el-radio
									>
								</el-radio-group>
							</div>
							<div class="judge" v-if="type == 2">
								<div class="judge-title user">得分：</div>
								<el-input-number
									v-model="it.score"
									placeholder=""
									:step="1"
								></el-input-number>
							</div>
							<div class="item">
								难度：<el-rate
									v-model="it.question.difficulty"
									:max="5"
									disabled
									style="position: relative; top: 5px"
								></el-rate>
							</div>
							<div class="item">
								解析：{{ it.question.parsing }}
							</div>
						</div>
					</div>
				</el-scrollbar>
				<div class="footer" v-if="type == 2">
					<el-button type="primary" @click="save">提交批改</el-button>
				</div>
			</el-main>
		</el-container>
	</el-container>
</template>
<script>
export default {
	name: "fullpage",
	data() {
		return {
			id: 0,
			type: 1,
			active: 1,
			model: { questionItem: [] },
			user: {},
			paper: {},
			judgeOptions: [
				{
					label: "正确",
					value: true,
				},
				{
					label: "错误",
					value: false,
				},
			],
		};
	},
	beforeRouteEnter(to, from, next) {
		next((vm) => {
			if (from.id) {
				vm.id = from.id;
				vm.type = from.type;
				delete from.id;
				delete from.type;
			}
		});
	},
	mounted() {
		this.init();
	},
	methods: {
		async init() {
			const res = await this.$API.examuserlog.modelCorrect.get(this.id);
			if (res.code == 200) {
				this.user = res.data.user;
				this.model = res.data.userLog;
				this.questionArray = res.data.question;
				this.paper = res.data.paper;
				this.model.questionItem.forEach((m) => {
					res.data.question.forEach((row) => {
						if (m.id == row.id) {
							m.question = row;
						}
					});
				});
			} else {
				this.$alert(res.message, "提示", { type: "error" });
			}
		},
		judgeChange(row) {
			this.model.questionItem.forEach((m) => {
				if (m.id == row.id && m.judge) {
					m.score = m.sourceScore;
				}
				if (m.id == row.id && !m.judge) {
					m.score = 0;
				}
			});
		},
		async save() {
			const res = await this.$API.examuserlog.correct.put({
				id: this.model.id,
				questionItem: this.model.questionItem,
			});
			if (res.code == 200) {
				this.$message.success("批改成功");
			} else {
				this.$alert(res.message, "提示", { type: "error" });
			}
		},
		goBack() {
			this.$router.go(-1);
		},
	},
};
</script>
<style lang="scss" scoped>
.bg {
	background-color: #ffffff;
}
.correct-left {
	padding: 20px;
	font-size: 14px;
	line-height: 30px;
	color: #4c596b;
	.title {
		font-size: 16px;
	}
	.el-tag {
		cursor: pointer;
	}
}
.question-num {
	display: flex;
}
.question-type {
	padding: 15px 20px;
	background-color: #eff3f7;
	font-size: 16px;
}
.paper-question {
	font-size: 14px;
	padding: 15px 20px;
	display: flex;
	flex-direction: row;
	color: #4c596b;
	.title {
		width: 40px;
		font-weight: bold;
		padding-top: 6px;
	}
	.content {
		flex: 1;
		.item {
			padding: 6px 0px;
		}
		.user {
			color: #6ac144;
		}
		.judge {
			display: flex;
		}
		.judge-title {
			line-height: 30px;
		}
	}
	.el-input-number {
		width: 110px;
	}
}
.footer {
	text-align: center;
	padding-top: 10px;
}
.el-scrollbar {
	height: auto !important;
}
</style>
