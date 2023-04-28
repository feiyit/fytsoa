<template>
	<sc-dialog
		v-model="visible"
		show-fullscreen
		:title="titleMap[mode]"
		width="1200px"
		top="5vh"
		@close="close"
	>
		<el-row :gutter="15">
			<el-col :span="16">
				<el-form
					ref="formRef"
					:model="formData"
					:rules="rules"
					size="default"
					label-width="100px"
				>
					<el-scrollbar height="520px" style="padding-right: 20px">
						<el-row>
							<el-col :span="12">
								<el-form-item label="类型" prop="type">
									<el-select
										v-model="formData.type"
										placeholder="请输入类型"
										clearable
										:style="{ width: '100%' }"
										@change="typeSlectChange"
									>
										<el-option
											v-for="item in typeOption"
											:key="item.value"
											:label="item.label"
											:value="item.value"
										/>
									</el-select>
								</el-form-item>
							</el-col>
							<el-col :span="12">
								<el-form-item label="年级" prop="grandId">
									<el-select
										v-model="formData.grandId"
										placeholder="请输入年级"
										clearable
										:style="{ width: '100%' }"
									>
										<el-option
											v-for="item in grandOption"
											:key="item.value"
											:label="item.label"
											:value="item.value"
										/>
									</el-select>
								</el-form-item>
							</el-col>
							<el-col :span="12">
								<el-form-item label="分数" prop="score">
									<el-input-number
										v-model="formData.score"
										placeholder="分数"
										:step="1"
									></el-input-number>
								</el-form-item>
							</el-col>
							<el-col :span="12">
								<el-form-item label="学科" prop="subjectId">
									<el-select
										v-model="formData.subjectId"
										placeholder="请输入学科"
										clearable
										:style="{ width: '100%' }"
									>
										<el-option
											v-for="item in subjectOption"
											:key="item.value"
											:label="item.label"
											:value="item.value"
										/>
									</el-select>
								</el-form-item>
							</el-col>
							<el-col :span="24">
								<el-form-item label="题干" prop="title">
									<el-input
										v-model="formData.title"
										placeholder="请输入题干"
										:maxlength="100"
										show-word-limit
										clearable
										:style="{ width: '100%' }"
									></el-input>
								</el-form-item>
							</el-col>
							<el-col
								:span="24"
								v-if="formData.type != 4 && formData.type != 5"
							>
								<el-form-item
									label="选项"
									prop="subjectItem"
									class="subject-item"
								>
									<sc-form-table
										v-model="formData.subjectItem"
										:addTemplate="apiListAddTemplate"
										placeholder="暂无内容"
										width="100%"
									>
										<el-table-column
											prop="tag"
											label="标记"
											width="100px"
										>
											<template #default="scope">
												<el-input
													v-model="scope.row.tag"
													placeholder="A"
													input-style="text-align: center"
												></el-input>
											</template>
										</el-table-column>
										<el-table-column
											prop="value"
											label="选项内容"
										>
											<template #default="scope">
												<el-input
													v-model="scope.row.value"
													placeholder="请输入选项内容"
												></el-input>
											</template>
										</el-table-column>
									</sc-form-table>
								</el-form-item>
							</el-col>
							<el-col :span="24">
								<el-form-item label="答案" prop="answer">
									<el-radio-group
										v-model="formData.answer"
										v-if="formData.type == 1"
									>
										<el-radio
											v-for="(
												item, index
											) in formData.subjectItem"
											:key="index"
											:label="item.tag"
											>{{ item.tag }}</el-radio
										>
									</el-radio-group>
									<el-checkbox-group
										v-model="formData.answerArr"
										v-if="formData.type == 2"
										@change="answerChange"
									>
										<el-checkbox
											v-for="(
												item, index
											) in formData.subjectItem"
											:key="index"
											:label="item.tag"
											:min="4"
											:max="4"
											>{{ item.tag }}</el-checkbox
										>
									</el-checkbox-group>
									<el-radio-group
										v-model="formData.answer"
										v-if="formData.type == 3"
									>
										<el-radio
											v-for="(
												item, index
											) in formData.subjectItem"
											:key="index"
											:label="item.value"
											>{{ item.tag }}</el-radio
										>
									</el-radio-group>
									<el-input
										v-model="formData.answer"
										placeholder=""
										v-if="formData.type == 5"
									></el-input>
									<sc-form-table
										v-model="formData.answerNull"
										:addTemplate="emptyAddTemplate"
										placeholder="暂无内容"
										width="100%"
										v-if="formData.type == 4"
									>
										<el-table-column
											prop="tag"
											label="答案"
											width="398"
										>
											<template #default="scope">
												<el-input
													v-model="scope.row.name"
													placeholder=""
												></el-input>
											</template>
										</el-table-column>
										<el-table-column
											prop="value"
											label="分数"
											width="200"
										>
											<template #default="scope">
												<el-input-number
													v-model="scope.row.score"
													:min="1"
												/>
											</template>
										</el-table-column>
									</sc-form-table>
								</el-form-item>
							</el-col>
							<el-col :span="24">
								<el-form-item label="解析">
									<el-input
										v-model="formData.parsing"
										type="textarea"
										placeholder="请输入解析"
										:maxlength="500"
										show-word-limit
										:autosize="{ minRows: 4, maxRows: 4 }"
										:style="{ width: '100%' }"
									></el-input>
								</el-form-item>
							</el-col>
							<el-col :span="24">
								<el-form-item label="难度" prop="difficulty">
									<el-rate
										v-model="formData.difficulty"
										:max="5"
										style="position: relative; top: 8px"
									></el-rate>
								</el-form-item>
							</el-col>
						</el-row>
					</el-scrollbar>
				</el-form>
			</el-col>
			<el-col :span="8">
				<el-row>
					<el-card class="exam-card" shadow="never">
						<template #header>
							<div class="card-header">
								<span>题目预览</span>
							</div>
						</template>
						<div class="item">{{ formData.title }}</div>
						<div
							v-for="o in formData.subjectItem"
							:key="o"
							class="item"
						>
							{{ o.tag }}：{{ o.value }}
						</div>
						<div class="item" v-if="formData.type != 4">
							答案：{{ formData.answer }}
						</div>
						<div class="item" v-if="formData.type == 4">
							<el-row :gutter="10">
								<el-col :span="4">答案：</el-col>
								<el-col :span="8">
									<p
										v-for="it in formData.answerNull"
										:key="it.name"
									>
										{{ it.name }}
									</p>
								</el-col>
							</el-row>
						</div>
						<div class="item">分数：{{ formData.score }}</div>
						<div class="item">
							难度：<el-rate
								v-model="formData.difficulty"
								:max="5"
								style="position: relative; top: 5px"
							></el-rate>
						</div>
						<div class="item">解析：{{ formData.parsing }}</div>
					</el-card>
				</el-row>
			</el-col>
		</el-row>

		<template #footer>
			<el-button @click="close">取 消</el-button>
			<el-button :loading="isSaveing" type="primary" @click="save">
				确 定
			</el-button>
		</template>
	</sc-dialog>
</template>
<script setup>
import {
	ref,
	getCurrentInstance,
	onMounted,
	defineExpose,
	defineEmits,
	watch,
} from "vue";
const app = getCurrentInstance();
const api = app.appContext.config.globalProperties.$API;
const message = app.appContext.config.globalProperties.$message;
const alert = app.appContext.config.globalProperties.$alert;

const mode = ref("add");
const titleMap = {
	add: "新增",
	edit: "编辑",
};
const typeOption = [
	{ label: "单选题", value: 1 },
	{ label: "多选题", value: 2 },
	{ label: "判断题", value: 3 },
	{ label: "填空题", value: 4 },
	{ label: "解答题", value: 5 },
];
let grandOption = [];
let subjectOption = [];
const isSaveing = ref(false);
const visible = ref(false);
const formRef = ref();
let formData = ref({
	id: 0,
	grandId: "",
	subjectId: "",
	type: 1,
	title: "",
	subjectItem: [
		{ tag: "A", value: "" },
		{ tag: "B", value: "" },
		{ tag: "C", value: "" },
		{ tag: "D", value: "" },
	],
	answer: "",
	answerArr: [],
	answerNull: [],
	parsing: "",
	score: 0,
	difficulty: 1,
	knowledgePoint: "",
});
const initGrandOption = async () => {
	const res = await api.syscode.list.get({ typeCode: "grand" });
	if (res.code == 200) {
		res.data.forEach((e) => {
			grandOption.push({ label: e.name, value: e.id });
		});
	}
};
const initSubjectOption = async () => {
	const res = await api.syscode.list.get({ typeCode: "subject" });
	if (res.code == 200) {
		res.data.forEach((e) => {
			subjectOption.push({ label: e.name, value: e.id });
		});
	}
};
onMounted(() => {
	initGrandOption();
	initSubjectOption();
});
const apiListAddTemplate = ref({
	tag: "",
	value: "",
});
const emptyAddTemplate = ref({
	name: "",
	score: 0,
});
watch(formData.value.answerNull, (newValue, oldValue) => {
	console.log(
		newValue,
		oldValue,
		" formData.value.answerNull",
		formData.value.answerNull
	);
	if (formData.value.answerNull.length > 0) {
		formData.value.answer = JSON.stringify(formData.value.answerNull);
	} else {
		formData.value.answer = "";
	}
});

const rules = {
	type: [
		{
			required: true,
			message: "请输入类型",
			trigger: "change",
		},
	],
	grandId: [
		{
			required: true,
			message: "请输入年级",
			trigger: "change",
		},
	],
	score: [
		{
			required: true,
			message: "分数",
			trigger: "blur",
		},
	],
	subjectItem: [],
	subjectId: [
		{
			required: true,
			message: "请输入学科",
			trigger: "change",
		},
	],
	title: [
		{
			required: true,
			message: "请输入题干",
			trigger: "blur",
		},
	],
	answer: [
		{
			required: true,
			message: "答案不能为空",
			trigger: "change",
		},
	],
	parsing: [],
	difficulty: [
		{
			required: true,
			message: "难度不能为空",
			trigger: "change",
		},
	],
};
const answerChange = (value) => {
	formData.value.answer = value.toString();
};
const typeSlectChange = (val) => {
	formData.value.answer = "";
	if (val === 3) {
		formData.value.subjectItem = [
			{ tag: "A", value: "对" },
			{ tag: "B", value: "错" },
		];
	} else if (val === 4 || val === 5) {
		formData.value.subjectItem = [];
	} else {
		formData.value.subjectItem = [
			{ tag: "A", value: "" },
			{ tag: "B", value: "" },
			{ tag: "C", value: "" },
			{ tag: "D", value: "" },
		];
	}
};
const open = async (row) => {
	if (!row) {
		mode.value = "add";
	} else {
		mode.value = "edit";
		const res = await api.examquestion.model.get(row.id);
		formData.value = res.data;
		if (formData.value.type == 2) {
			formData.value.answerArr = formData.value.answer.split(",");
		}
		if (formData.value.type == 4) {
			formData.value.answerNull = JSON.parse(formData.value.answer);
		}
	}
	visible.value = true;
};
defineExpose({
	open,
});
const emits = defineEmits(["complete"]);
const save = () => {
	formRef.value.validate(async (valid) => {
		if (valid) {
			isSaveing.value = true;
			let res = null;
			formData.value.answer = JSON.stringify(formData.value.answerNull);
			if (formData.value.id === 0) {
				res = await api.examquestion.add.post(formData.value);
			} else {
				res = await api.examquestion.update.put(formData.value);
			}
			isSaveing.value = false;
			if (res.code == 200) {
				emits("complete");
				visible.value = false;
				message.success("操作成功");
			} else {
				alert(res.message, "提示", { type: "error" });
			}
		}
	});
};
const close = () => {
	formData.value = {
		id: 0,
		grandId: "",
		subjectId: "",
		type: 1,
		title: "",
		subjectItem: [
			{ tag: "A", value: "" },
			{ tag: "B", value: "" },
			{ tag: "C", value: "" },
			{ tag: "D", value: "" },
		],
		answer: "",
		parsing: "",
		score: 0,
		difficulty: 1,
		knowledgePoint: "",
	};
	formRef.value.resetFields();
	visible.value = false;
};
</script>
<style>
.subject-item .el-form-item__content {
	display: block;
}
.exam-card {
	width: 100%;
}
.exam-card .item {
	padding: 6px 0px;
}
</style>
