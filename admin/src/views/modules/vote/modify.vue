<template>
	<sc-dialog
		v-model="visible"
		show-fullscreen
		:title="titleMap[mode]"
		width="1000px"
		@close="close"
	>
		<el-container>
			<el-aside width="480px" class="vote-left">
				<el-steps
					:active="active"
					align-center
					style="margin-bottom: 10px"
				>
					<el-step title="基本设置"></el-step>
					<el-step title="高级设置"></el-step>
				</el-steps>
				<el-form
					ref="formRef"
					label-width="100px"
					:model="formData"
					:rules="rules"
				>
					<div v-show="active == 1">
						<el-form-item label="投票标题" prop="title">
							<el-input
								v-model="formData.title"
								placeholder="请输入投票标题"
								:maxlength="90"
								show-word-limit
								clearable
							></el-input></el-form-item
						><el-form-item label="投票类型" prop="type">
							<el-radio-group v-model="formData.type" size="mini">
								<el-radio
									v-for="(item, index) in typeOptions"
									:key="index"
									:label="item.value"
									>{{ item.label }}</el-radio
								>
							</el-radio-group></el-form-item
						>
						<el-form-item label="开始时间" prop="startTime">
							<el-col :span="11">
								<el-date-picker
									v-model="formData.startTime"
									type="date"
									placeholder="开始时间"
									style="width: 100%"
								/>
							</el-col>
							<el-col :span="2" class="text-center">
								<span class="text-gray-500">-</span>
							</el-col>
							<el-col :span="11">
								<el-date-picker
									v-model="formData.endTime"
									type="date"
									placeholder="结束时间"
									style="width: 100%"
								/>
							</el-col>
						</el-form-item>
						<el-form-item label="勾选规则" prop="tickRule">
							<el-radio-group
								v-model="formData.tickRule"
								size="mini"
							>
								<el-radio
									v-for="(item, index) in tickRuleOptions"
									:key="index"
									:label="item.value"
									:disabled="item.disabled"
									>{{ item.label }}</el-radio
								>
							</el-radio-group></el-form-item
						>
					</div>
					<div v-show="active == 2">
						<el-form-item label="防刷规则" prop="swipeRule">
							<el-switch v-model="formData.swipeRule"></el-switch>
							<div class="alert">
								开启后，一个IP只能投票一次
							</div> </el-form-item
						><el-form-item label="文件地址" prop="fileUrl">
							<sc-upload
								v-model="formData.fileUrl"
								:apiObj="uploadApi"
								:width="148"
								:height="148"
								:onSuccess="upSuccess"
							></sc-upload> </el-form-item
						><el-form-item label="规则" prop="summary">
							<el-input
								v-model="formData.summary"
								type="textarea"
								placeholder="请输入投票规则"
								:maxlength="500"
								show-word-limit
								:autosize="{ minRows: 2, maxRows: 3 }"
								:style="{ width: '100%' }"
							></el-input>
						</el-form-item>
					</div>
				</el-form>
			</el-aside>
			<el-container class="vote-item">
				<h2>投票项</h2>
				<sc-form-table
					v-model="formData.items"
					:addTemplate="apiListAddTemplate"
					placeholder="暂无投票项"
				>
					<el-table-column prop="title" label="投票项">
						<template #default="scope">
							<el-input
								v-model="scope.row.title"
								placeholder="请输入投票项内容"
							></el-input>
						</template>
					</el-table-column>
				</sc-form-table>
			</el-container>
		</el-container>

		<template #footer>
			<el-button v-show="active != 1" @click="pre">上一步</el-button>
			<el-button type="primary" v-show="active == 1" @click="next">
				下一步
			</el-button>
			<el-button
				:loading="isSaveing"
				v-show="active != 1"
				type="primary"
				@click="save"
			>
				提 交
			</el-button>
		</template>
	</sc-dialog>
</template>
<script>
export default {
	emits: ["complete"],
	data() {
		return {
			mode: "add",
			titleMap: {
				add: "新增",
				edit: "编辑",
			},
			apiListAddTemplate: {
				title: "",
			},
			active: 1,
			uploadApi: this.$API.sysfile.vote,
			isSaveing: false,
			visible: false,
			formData: {
				id: 0,
				title: "",
				type: 1,
				startTime: "",
				endTime: "",
				tickRule: 1,
				swipeRule: true,
				fileUrl: "",
				summary: "",
				items: [],
			},
			typeOptions: [
				{
					label: "图文",
					value: 1,
				},
				{
					label: "视频",
					value: 2,
				},
			],
			tickRuleOptions: [
				{
					label: "单选",
					value: 1,
				},
				{
					label: "多选",
					value: 2,
				},
			],
			rules: {
				title: [
					{
						required: true,
						message: "请输入投票标题",
						trigger: "change",
					},
				],
				startTime: [
					{
						required: true,
						message: "请选择开始时间",
						trigger: "change",
					},
				],
				endTime: [
					{
						required: true,
						message: "请选择结束时间",
						trigger: "change",
					},
				],
				type: [
					{
						required: true,
						message: "投票类型不能为空",
						trigger: "change",
					},
				],
				tickRule: [
					{
						required: true,
						message: "勾选规则不能为空",
						trigger: "change",
					},
				],
			},
		};
	},
	mounted() {},
	methods: {
		async open(row) {
			if (!row) {
				this.mode = "add";
			} else {
				this.mode = "edit";
				var res = await this.$API.sysvote.model.get(row.id);
				this.formData = res.data;
			}
			this.visible = true;
		},
		next() {
			this.$refs.formRef.validate(async (valid) => {
				if (valid) {
					this.active += 1;
				}
			});
		},
		pre() {
			this.active -= 1;
		},
		upSuccess(res) {
			this.formData.fileUrl = res.data.path;
			if (res.code == 200) {
				this.$message.success("上传成功~");
			} else {
				this.$message.warning(res.message);
			}
		},
		save() {
			if(this.formData.items.length==0){
				this.$message.warning('需要添加投票项~');
				return
			}
			this.$refs.formRef.validate(async (valid) => {
				if (valid) {
					this.isSaveing = true;
					let res = null;
					if (this.formData.id === 0) {
						res = await this.$API.sysvote.add.post(this.formData);
					} else {
						res = await this.$API.sysvote.update.put(this.formData);
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
				title: "",
				type: 1,
				startTime: "",
				endTime: "",
				tickRule: 1,
				swipeRule: true,
				fileUrl: "",
				summary: "",
				items: [],
			};
			this.active = 1;
			this.$refs.formRef.resetFields();
			this.visible = false;
		},
	},
};
</script>
<style>
.vote-left {
	padding-right: 20px;
}
.text-center {
	text-align: center;
}
.alert {
	padding-left: 20px;
	font-size: 12px;
	color: #999;
}
.vote-item {
	padding: 0 0 0 20px;
	display: block;
}
.vote-item h2 {
	font-size: 18px;
	margin-bottom: 10px;
}
</style>
