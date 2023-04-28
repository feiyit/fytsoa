<template>
	<sc-dialog
		v-model="visible"
		show-fullscreen
		:title="titleMap[mode]"
		width="1200px"
		top="5vh"
		@close="close"
		:destroy-on-close="true"
	>
		<el-container>
			<el-aside width="650px" class="no-right-border">
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
									:style="{ width: '100%' }"
								>
									<el-option
										v-for="(item, index) in grandIdOptions"
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
									:style="{ width: '100%' }"
								>
									<el-option
										v-for="(
											item, index
										) in subjectIdOptions"
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
				</el-form>
				<div class="paper-plus">
					<el-button
						icon="el-icon-plus"
						class="btn"
						@click="addQuestion"
						>添加题目</el-button
					>
				</div>
			</el-aside>
			<el-container>
				<el-scrollbar height="500px" style="width: 100%">
					<el-card class="select-card" body-style="width:100%">
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
								<div class="item">解析：{{ it.parsing }}</div>
							</div>
							<div class="action">
								<el-input-number v-model="it.score" :min="1" />
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
				</el-scrollbar>
			</el-container>
		</el-container>

		<template #footer>
			<el-button @click="close">取 消</el-button>
			<el-button :loading="isSaveing" type="primary" @click="save">
				确 定
			</el-button>
		</template>
		<selectQuestion
			ref="selectQuestion"
			@completeSelect="completeSelect"
		></selectQuestion>
	</sc-dialog>
</template>
<script>
import { defineAsyncComponent } from "vue";
export default {
	components: {
		selectQuestion: defineAsyncComponent(() => import("./selectQuestion")),
	},
	emits: ["complete"],
	data() {
		return {
			mode: "add",
			titleMap: {
				add: "新增",
				edit: "编辑",
			},
			isSaveing: false,
			visible: false,
			timeId: "1542418312204521473",
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
			grandIdOptions: [],
			subjectIdOptions: [],
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
					this.grandIdOptions.push({ label: e.name, value: e.id });
				});
			}
		},
		async initSubjectOption() {
			const res = await this.$API.syscode.list.get({
				typeCode: "subject",
			});
			if (res.code == 200) {
				res.data.forEach((e) => {
					this.subjectIdOptions.push({ label: e.name, value: e.id });
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
		async open(row) {
			if (!row) {
				this.mode = "add";
			} else {
				this.mode = "edit";
				var res = await this.$API.exampaper.model.get(row.id);
				this.formData = res.data;
				if (this.formData.typeId == this.timeId) {
					this.formData.times = [];
					this.formData.times.push(this.formData.startTime);
					this.formData.times.push(this.formData.endTime);
				}
				let idArr = this.formData.questionItem
					.map((m) => {
						return m.id;
					})
					.join(",");
				const list = await this.$API.examquestion.list.get({
					idArr: idArr,
				});
				list.data.forEach((m) => {
					this.formData.questionItem.forEach((row) => {
						if (m.id == row.id) {
							m.score = row.score;
						}
					});
				});
				this.questionArray = list.data;
			}
			this.visible = true;
		},
		save() {
			if (this.questionArray.length == 0) {
				this.$alert("需要选择题目", "提示", { type: "error" });
				return;
			}
			this.formData.questionItem = [];
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
					let res = null;
					if (this.formData.id === 0) {
						res = await this.$API.exampaper.add.post(this.formData);
					} else {
						res = await this.$API.exampaper.update.put(
							this.formData
						);
					}
					this.isSaveing = false;
					if (res.code == 200) {
						this.$emit("complete");
						this.visible = false;
						this.$message.success("操作成功");
					} else {
						this.$alert(res.message, "提示", { type: "error" });
					}
				}
			});
		},
		close() {
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
			this.visible = false;
		},
	},
};
</script>
<style lang="scss" scoped>
.no-right-border {
	border-right: none;
	padding-right: 15px;
}
.paper-plus {
	text-align: center;
	.btn {
		width: 50%;
	}
}
.select-card {
	width: 100%;
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
