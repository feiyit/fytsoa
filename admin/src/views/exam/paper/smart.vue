<template>
	<el-container>
		<el-aside width="500px" class="no-right-border">
			<el-header><span class="title">组卷选项</span></el-header>
			<el-form
				ref="smartForm"
				:model="formSmart"
				:rules="smartRules"
				label-width="100px"
				class="smart-action"
			>
				<el-form-item label="年级" prop="smartGrand">
					<el-select
						v-model="formSmart.smartGrand"
						placeholder="请选择年级"
						clearable
						:style="{ width: '100%' }"
					>
						<el-option
							v-for="(item, index) in smartGrandOptions"
							:key="index"
							:label="item.label"
							:value="item.value"
							:disabled="item.disabled"
						></el-option>
					</el-select>
				</el-form-item>
				<el-form-item label="学科" prop="smartSubject">
					<el-select
						v-model="formSmart.smartSubject"
						placeholder="请选择学科"
						clearable
						:style="{ width: '100%' }"
					>
						<el-option
							v-for="(item, index) in smartSubjectOptions"
							:key="index"
							:label="item.label"
							:value="item.value"
							:disabled="item.disabled"
						></el-option>
					</el-select>
				</el-form-item>
				<el-form-item label="单选题" prop="single">
					<el-input-number
						v-model="formSmart.single"
						placeholder="单选题"
						:step="1"
					></el-input-number>
				</el-form-item>
				<el-form-item label="多选题" prop="multiple">
					<el-input-number
						v-model="formSmart.multiple"
						placeholder="多选题"
						:step="1"
					></el-input-number>
				</el-form-item>
				<el-form-item label="判断题" prop="judge">
					<el-input-number
						v-model="formSmart.judge"
						placeholder="判断题"
						:step="1"
					></el-input-number>
				</el-form-item>
				<el-form-item label="填空题" prop="gapFilling">
					<el-input-number
						v-model="formSmart.gapFilling"
						placeholder="填空题"
						:step="1"
					></el-input-number>
				</el-form-item>
				<el-form-item label="解答题" prop="explain">
					<el-input-number
						v-model="formSmart.explain"
						placeholder="解答题"
						:step="1"
					></el-input-number>
				</el-form-item>
				<el-form-item label="难度" prop="difficulty">
					<el-rate
						v-model="formSmart.difficulty"
						:max="5"
						style="position: relative; top: 8px"
					></el-rate>
				</el-form-item>
				<el-form-item>
					<el-button
						type="primary"
						class="smart-btn"
						@click="submitSmart"
						>开始组卷</el-button
					>
				</el-form-item>
			</el-form>
		</el-aside>
		<el-container>
			<el-header><span class="title">组卷结果</span></el-header>
			<el-main>
				<el-form
					ref="formRef"
					:model="formData"
					:rules="rules"
					label-width="100px"
				>
					<el-row>
						<el-col :span="12">
							<el-form-item label="年级" prop="grandId">
								<el-select
									v-model="formData.grandId"
									placeholder="请选择年级"
									clearable
									disabled
									:style="{ width: '100%' }"
								>
									<el-option
										v-for="(
											item, index
										) in smartGrandOptions"
										:key="index"
										:label="item.label"
										:value="item.value"
										:disabled="item.disabled"
									></el-option>
								</el-select>
							</el-form-item>
						</el-col>
						<el-col :span="12">
							<el-form-item label="学科" prop="subjectId">
								<el-select
									v-model="formData.subjectId"
									placeholder="请选择学科"
									clearable
									disabled
									:style="{ width: '100%' }"
								>
									<el-option
										v-for="(
											item, index
										) in smartSubjectOptions"
										:key="index"
										:label="item.label"
										:value="item.value"
										:disabled="item.disabled"
									></el-option>
								</el-select>
							</el-form-item>
						</el-col>
						<el-col :span="12">
							<el-form-item label="试卷类型" prop="typeId">
								<el-select
									v-model="formData.typeId"
									placeholder="请选择试卷类型"
									clearable
									:style="{ width: '100%' }"
								>
									<el-option
										v-for="(item, index) in typeIdOptions"
										:key="index"
										:label="item.label"
										:value="item.value"
										:disabled="item.disabled"
									></el-option>
								</el-select>
							</el-form-item>
						</el-col>
						<el-col :span="12">
							<el-form-item label="考试时长" prop="minutesLength">
								<el-input-number
									v-model="formData.minutesLength"
									placeholder="考试时长"
									:step="1"
								></el-input-number>
								<span style="padding-left: 10px">分钟</span>
							</el-form-item>
						</el-col>
					</el-row>
					<el-form-item label="试卷名称" prop="title">
						<el-input
							v-model="formData.title"
							placeholder="请输入试卷名称"
							clearable
							:style="{ width: '100%' }"
						>
						</el-input>
					</el-form-item>
					<el-form-item label="防作弊" prop="antiCheating">
						<el-checkbox-group v-model="formData.antiCheating">
							<el-checkbox
								v-for="(item, index) in antiCheatingOptions"
								:key="index"
								:label="item.value"
								:disabled="item.disabled"
								>{{ item.label }}</el-checkbox
							>
						</el-checkbox-group>
					</el-form-item>
					<el-form-item
						label="考试时间"
						prop="times"
						v-if="formData.typeId == timeId"
					>
						<el-date-picker
							type="datetimerange"
							v-model="formData.times"
							:style="{ width: '100%' }"
							start-placeholder="开始时间"
							end-placeholder="结束时间"
							range-separator="至"
							clearable
						></el-date-picker>
					</el-form-item>
					<el-form-item label="">
						<el-card class="select-card" style="width: 100%">
							<el-empty
								description="请选择题目"
								:image-size="100"
								v-if="questionArray.length == 0"
							></el-empty>
							<div
								class="paper-question"
								v-for="(it, index) in questionArray"
								:key="index"
							>
								<div class="title">题目</div>
								<div class="content">
									<div class="item">
										{{ it.title }}（{{ it.score }}）分
									</div>
									<div
										v-for="o in it.subjectItem"
										:key="o"
										class="item"
									>
										{{ o.tag }}：{{ o.value }}
									</div>
									<div class="item" v-if="it.type != 4">
										答案：{{ it.answer }}
									</div>
									<div class="item" v-if="it.type == 4">
										<el-row :gutter="10">
											<el-col :span="4">答案：</el-col>
											<el-col :span="8">
												<p
													v-for="r in JSON.parse(
														it.answer
													)"
													:key="r.name"
												>
													{{ r.name }}
												</p>
											</el-col>
										</el-row>
									</div>
									<div class="item">
										难度：<el-rate
											v-model="it.difficulty"
											:max="5"
											disabled
											style="position: relative; top: 5px"
										></el-rate>
									</div>
									<div class="item">
										解析：{{ it.parsing }}
									</div>
								</div>
								<div class="action">
									<el-input-number
										v-model="it.score"
										:min="1"
									/>
									<el-button
										type="danger"
										icon="el-icon-delete"
										circle
										style="margin-top: 10px"
										@click="selectDel(it)"
									/>
								</div>
							</div>
						</el-card>
					</el-form-item>
					<el-form-item>
						<el-button
							type="primary"
							:disabled="questionArray.length == 0"
							@click="submitForm"
							>提交保存</el-button
						>
						<el-button @click="resetForm">重置</el-button>
						<el-button
							type="success"
							icon="el-icon-plus"
							@click="addQuestion"
							>添加题目</el-button
						>
					</el-form-item>
				</el-form></el-main
			>
		</el-container>
		<selectQuestion
			ref="selectQuestion"
			@completeSelect="completeSelect"
		></selectQuestion>
	</el-container>
</template>
<script>
import { defineAsyncComponent } from "vue";
export default {
	components: {
		selectQuestion: defineAsyncComponent(() => import("./selectQuestion")),
	},
	props: [],
	data() {
		return {
			formSmart: {
				smartGrand: undefined,
				smartSubject: undefined,
				single: 0,
				multiple: 0,
				judge: 0,
				gapFilling: 0,
				explain: 0,
				difficulty: 0,
			},
			smartRules: {
				smartGrand: [
					{
						required: true,
						message: "请选择年级",
						trigger: "change",
					},
				],
				smartSubject: [
					{
						required: true,
						message: "请选择学科",
						trigger: "change",
					},
				],
				single: [],
				multiple: [],
				judge: [],
				gapFilling: [],
				explain: [],
				difficulty: [],
			},
			smartGrandOptions: [],
			smartSubjectOptions: [],
			formData: {
				id: 0,
				grandId: "",
				subjectId: "",
				typeId: "",
				times: [],
				startTime: undefined,
				endTime: undefined,
				title: "",
				antiCheating: [],
				minutesLength: 0,
				questionItem: [],
				status: false,
			},
			questionArray: [],
			rules: {
				grandId: [
					{
						required: true,
						message: "请选择年级",
						trigger: "change",
					},
				],
				subjectId: [
					{
						required: true,
						message: "请选择学科",
						trigger: "change",
					},
				],
				typeId: [
					{
						required: true,
						message: "请选择试卷类型",
						trigger: "change",
					},
				],
				minutesLength: [
					{
						required: true,
						message: "考试时长",
						trigger: "blur",
					},
				],
				title: [
					{
						required: true,
						message: "请输入试卷名称",
						trigger: "blur",
					},
				],
			},
			typeIdOptions: [],
			antiCheatingOptions: [
				{
					label: "题序打乱",
					value: 1,
				},
				{
					label: "选项打乱",
					value: 2,
				},
			],
			timeId: "1542418312204521473",
		};
	},
	mounted() {
		this.initGrandOption();
		this.initSubjectOption();
		this.initTypeOption();
	},
	methods: {
		async initGrandOption() {
			const res = await this.$API.syscode.list.get({
				typeCode: "grand",
			});
			if (res.code == 200) {
				res.data.forEach((e) => {
					this.smartGrandOptions.push({ label: e.name, value: e.id });
				});
			}
		},
		async initSubjectOption() {
			const res = await this.$API.syscode.list.get({
				typeCode: "subject",
			});
			if (res.code == 200) {
				res.data.forEach((e) => {
					this.smartSubjectOptions.push({
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
					this.typeIdOptions.push({ label: e.name, value: e.id });
				});
			}
		},
		addQuestion() {
			if (!this.formData.grandId || !this.formData.subjectId) {
				this.$alert("请选择年级以及学科", "提示", { type: "error" });
				return;
			}
			this.$refs.selectQuestion.open({
				guard: this.formData.grandId,
				subject: this.formData.subjectId,
			});
		},
		completeSelect(list) {
			this.questionArray.forEach((m) => {
				list.forEach((row) => {
					if (row.id != m.id) {
						this.questionArray.push(row);
					}
				});
			});
		},
		selectDel(m) {
			const index = this.questionArray.findIndex((item) => {
				if (item.id == m.id) {
					return true;
				}
			});
			this.questionArray.splice(index, 1);
		},
		submitForm() {
			if (this.questionArray.length == 0) {
				this.$alert("需要选择题目", "提示", { type: "error" });
				return;
			}
			this.questionArray.forEach((m) => {
				this.formData.questionItem.push({ id: m.id, score: m.score });
			});
			this.$refs.formRef.validate(async (valid) => {
				if (valid) {
					if (this.formData.typeId == this.timeId) {
						this.formData.startTime = this.formData.times[0];
						this.formData.endTime = this.formData.times[1];
					}
					this.isSaveing = true;
					let res = await this.$API.exampaper.add.post(this.formData);
					this.isSaveing = false;
					if (res.code == 200) {
						this.$message.success("保存成功");
						this.resetForm();
					} else {
						this.$alert(res.message, "提示", { type: "error" });
					}
				}
			});
		},
		resetForm() {
			this.formData = {
				id: 0,
				grandId: "",
				subjectId: "",
				typeId: "",
				startTime: undefined,
				endTime: undefined,
				title: "",
				antiCheating: [],
				minutesLength: 0,
				questionItem: [],
				status: false,
			};
			this.questionArray = [];
			this.$refs.formRef.resetFields();
		},
		submitSmart() {
			this.formData.grandId = this.formSmart.smartGrand;
			this.formData.subjectId = this.formSmart.smartSubject;
			this.$refs["smartForm"].validate(async (valid) => {
				if (!valid) return;
				const res = await this.$API.examquestion.smart.post(
					this.formSmart
				);
				if (res.code == 200) {
					this.questionArray = res.data;
				} else {
					this.$alert(res.message, "提示", { type: "error" });
				}
			});
		},
	},
};
</script>
<style lang="scss" scoped>
.title {
	font-size: 14px;
}
.smart-action {
	padding: 20px;
}
.smart-btn {
	width: 75%;
}
.paper-question {
	display: flex;
	flex-direction: row;
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
	}
	.action {
		width: 110px;
		text-align: center;
	}
	.el-input-number {
		width: 110px;
	}
}
</style>
