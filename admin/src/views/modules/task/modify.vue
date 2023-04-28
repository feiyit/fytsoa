<template>
	<sc-dialog
		v-model="visible"
		show-fullscreen
		:title="titleMap[mode]"
		width="750px"
		@close="close"
	>
		<el-form
			ref="formRef"
			:model="formData"
			:rules="rules"
			label-width="100px"
		>
			<el-row>
				<el-col :span="12">
					<el-form-item label="任务分组" prop="groupName">
						<el-input
							v-model="formData.groupName"
							placeholder="请输入任务分组任务分组"
							:maxlength="30"
							show-word-limit
							clearable
							:style="{ width: '100%' }"
						></el-input>
					</el-form-item>
				</el-col>
				<el-col :span="12">
					<el-form-item label="间隔时间" prop="interval">
						<sc-cron
							v-model="formData.interval"
							placeholder="请输入Cron定时规则"
							clearable
						></sc-cron>
					</el-form-item>
				</el-col>
				<el-col :span="12">
					<el-form-item label="任务名称" prop="taskName">
						<el-input
							v-model="formData.taskName"
							placeholder="请输入任务名称"
							:maxlength="50"
							show-word-limit
							clearable
							:style="{ width: '100%' }"
						></el-input>
					</el-form-item>
				</el-col>
				<el-col :span="12">
					<el-form-item label="任务类型" prop="taskType">
						<el-select
							v-model="formData.taskType"
							placeholder="请选择任务类型"
							clearable
							:style="{ width: '100%' }"
						>
							<el-option
								v-for="(item, index) in triggerTypeOptions"
								:key="index"
								:label="item.label"
								:value="item.value"
								:disabled="item.disabled"
							></el-option>
						</el-select>
					</el-form-item>
				</el-col>
				<el-col :span="12">
					<el-form-item
						v-if="formData.taskType == 2"
						label="请求类型"
						prop="apiRequestType"
					>
						<el-radio-group v-model="formData.apiRequestType">
							<el-radio
								v-for="(item, index) in requestTypeOptions"
								:key="index"
								:label="item.value"
								:disabled="item.disabled"
							>
								{{ item.label }}
							</el-radio>
						</el-radio-group>
					</el-form-item>
				</el-col>
			</el-row>
			<el-form-item
				v-if="formData.taskType == 1"
				label="类名"
				prop="dllClassName"
			>
				<el-input
					v-model="formData.dllClassName"
					placeholder="请输入任务执行类名"
					:maxlength="200"
					show-word-limit
					clearable
					:style="{ width: '100%' }"
				></el-input>
			</el-form-item>
			<el-form-item
				v-if="formData.taskType == 2"
				label="请求地址"
				prop="apiUrl"
			>
				<el-input
					v-model="formData.apiUrl"
					placeholder="请输入请求地址"
					:maxlength="200"
					show-word-limit
					clearable
					:style="{ width: '100%' }"
				></el-input>
			</el-form-item>
			<el-row>
				<el-col :span="12">
					<el-form-item v-if="formData.taskType == 2" label="授权名">
						<el-input
							v-model="formData.apiAuthKey"
							placeholder="请输入授权名"
							:maxlength="200"
							show-word-limit
							clearable
							:style="{ width: '100%' }"
						></el-input>
					</el-form-item>
				</el-col>
				<el-col :span="12">
					<el-form-item v-if="formData.taskType == 2" label="授权值">
						<el-input
							v-model="formData.apiAuthValue"
							placeholder="请输入授权值"
							:maxlength="500"
							show-word-limit
							clearable
							:style="{ width: '100%' }"
						></el-input>
					</el-form-item>
				</el-col>
			</el-row>
			<el-form-item v-if="formData.taskType == 2" label="请求参数">
				<el-input
					v-model="formData.apiParameter"
					type="textarea"
					placeholder="请输入请求参数"
					:maxlength="800"
					show-word-limit
					:autosize="{ minRows: 3, maxRows: 3 }"
					:style="{ width: '100%' }"
				></el-input>
			</el-form-item>
			<el-form-item label="任务描述" prop="description">
				<el-input
					v-model="formData.description"
					type="textarea"
					placeholder="请输入任务描述"
					:maxlength="200"
					show-word-limit
					:autosize="{ minRows: 3, maxRows: 3 }"
					:style="{ width: '100%' }"
				></el-input>
			</el-form-item>
		</el-form>

		<template #footer>
			<el-button @click="close">取 消</el-button>
			<el-button :loading="isSaveing" type="primary" @click="save">
				确 定
			</el-button>
		</template>
	</sc-dialog>
</template>
<script>
import scCron from "@/components/scCron";
export default {
	components: {
		scCron,
	},
	emits: ["complete", "success", "closed"],
	data() {
		return {
			mode: "add",
			titleMap: {
				add: "新增",
				edit: "编辑",
			},
			isSaveing: false,
			visible: false,
			formData: this.initTask(),
			rules: {
				taskName: [
					{
						required: true,
						message: "请输入任务名称",
						trigger: "blur",
					},
				],
				groupName: [
					{
						required: true,
						message: "请输入分组名称",
						trigger: "blur",
					},
				],
				interval: [
					{
						required: true,
						message: "请输入任务时间间隔",
						trigger: "blur",
					},
				],
				apiUrl: [
					{
						required: true,
						message: "请输入调用的API地址",
						trigger: "blur",
					},
				],
				describe: [
					{
						required: true,
						message: "请输入任务描述",
						trigger: "blur",
					},
				],
				taskType: [
					{
						required: true,
						message: "请选择任务类型",
						trigger: "change",
					},
				],
				apiRequestType: [
					{
						required: true,
						message: "请选择API访问类型",
						trigger: "change",
					},
				],
				//ApiAuthKey: [
				//    { required: true, message: '请输入Api授权名', trigger: 'blur' }
				//],
				//ApiAuthValue: [
				//    { required: true, message: '请输入Api授权值', trigger: 'blur' }
				//],
				//ApiParameter: [
				//    { required: true, message: '请输入API参数', trigger: 'blur' }
				//],
				dllClassName: [
					{ required: true, message: "请输入类名", trigger: "blur" },
				],
				//DllActionName: [
				//    { required: true, message: '请输入方法名', trigger: 'blur' }
				//]
			},
			triggerTypeOptions: [
				{
					label: "Class",
					value: "1",
				},
				{
					label: "Api",
					value: "2",
				},
			],
			requestTypeOptions: [
				{
					label: "GET",
					value: "GET",
				},
				{
					label: "POST",
					value: "POST",
				},
			],
		};
	},
	methods: {
		async open(row) {
			if (!row) {
				this.mode = "add";
			} else {
				this.mode = "edit";
				Object.assign(this.formData, row);
			}
			this.visible = true;
		},
		initTask() {
			//const time = this.$TOOL.dateFormat(new Date());
			return {
				taskName: "",
				groupName: "",
				interval: "",
				apiUrl: "",
				describe: "",
				status: 1,
				taskType: "",
				apiRequestType: "GET",
				apiAuthKey: "",
				apiAuthValue: "",
				apiParameter: "",
				dllClassName: "",
				dllActionName: "",
				id: 0,
			};
		},
		save() {
			this.$refs.formRef.validate(async (valid) => {
				if (valid) {
					this.isSaveing = true;
					let res = null;
					if (this.formData.id === 0) {
						res = await this.$API.sysquartz.add.post(this.formData);
					} else {
						res = await this.$API.sysquartz.update.put(
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
			this.$refs.formRef.resetFields();
			this.formData = this.initTask();
			this.visible = false;
		},
	},
};
</script>
