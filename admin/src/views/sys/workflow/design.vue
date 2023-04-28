<template>
	<el-container>
		<el-header>
			<el-page-header content="创建流程" @back="goBack" />
			<div class="flow-tab">
				<span
					:class="tabIndex == 0 ? 'active' : ''"
					@click="tabIndex = 0"
					>1.基础信息</span
				>
				<span
					:class="tabIndex == 1 ? 'active' : ''"
					@click="tabIndex = 1"
					>2.配置流程</span
				>
			</div>
			<el-button :loading="isSaveing" type="primary" @click="save"
				>发布</el-button
			>
		</el-header>
		<el-container class="flow-container">
			<div class="flow-main basic-main" v-show="tabIndex == 0">
				<el-card class="basic-card">
					<h3 class="flow-setting-title">基础配置</h3>

					<el-form
						ref="formRef"
						:model="formData"
						:rules="rules"
						label-width="100px"
						label-position="top"
					>
						<el-row :gutter="25">
							<el-col :span="12">
								<el-form-item label="审批流名称" prop="title">
									<el-input
										v-model="formData.title"
										placeholder="请输入审批流名称"
										:maxlength="50"
										show-word-limit
										clearable
										:style="{ width: '100%' }"
									></el-input>
								</el-form-item>
							</el-col>
							<el-col :span="12">
								<el-form-item
									label="审批被拒后重新提交"
									prop="refused"
								>
									<el-select
										v-model="formData.refused"
										placeholder="请选择审批被拒后重新提交"
										clearable
										:style="{ width: '100%' }"
									>
										<el-option
											v-for="(
												item, index
											) in refusedOptions"
											:key="index"
											:label="item.label"
											:value="item.value"
											:disabled="item.disabled"
										></el-option>
									</el-select>
								</el-form-item>
							</el-col>
							<el-col :span="12">
								<el-form-item label="图标" prop="icon">
									<el-input
										v-model="formData.icon"
										placeholder="请输入图标"
										clearable
										:style="{ width: '100%' }"
									></el-input>
								</el-form-item>
							</el-col>
							<el-col :span="12">
								<el-form-item
									label="状态"
									prop="status"
									required
								>
									<el-switch
										v-model="formData.status"
										inline-prompt
										active-text="启用"
										inactive-text="禁用"
										width="100"
									></el-switch>
								</el-form-item>
							</el-col>
							<el-col :span="24">
								<el-form-item
									label="流程说明 （请填写相关注意事项，方便员工在申请时查阅）"
									prop="summary"
								>
									<el-input
										v-model="formData.summary"
										type="textarea"
										placeholder="请输入流程说明 "
										:maxlength="200"
										show-word-limit
										:autosize="{ minRows: 4, maxRows: 4 }"
										:style="{ width: '100%' }"
									></el-input>
								</el-form-item>
							</el-col> </el-row></el-form
				></el-card>
			</div>
			<div class="flow-main" v-show="tabIndex == 1">
				<sc-workflow v-model="data.nodeConfig"></sc-workflow>
			</div>
		</el-container>
	</el-container>
</template>
<script>
import scWorkflow from "@/components/scWorkflow";
export default {
	name: "design",
	components: {
		scWorkflow,
	},
	data() {
		return {
			tabIndex: 0,
			isSaveing: false,
			formData: {
				id: 0,
				title: undefined,
				refused: 1,
				icon: undefined,
				status: true,
				summary: undefined,
				flow: undefined,
			},
			rules: {
				title: [
					{
						required: true,
						message: "请输入审批流名称",
						trigger: "blur",
					},
				],
				refused: [
					{
						required: true,
						message: "请选择审批被拒后重新提交",
						trigger: "change",
					},
				],
				icon: [
					{
						required: true,
						message: "请输入图标",
						trigger: "blur",
					},
				],
				summary: [
					{
						required: true,
						message: "请输入流程说明 ",
						trigger: "blur",
					},
				],
			},
			refusedOptions: [
				{
					label: "返回审批流初始层级",
					value: 1,
				},
				{
					label: "返回到上一级",
					value: 2,
				},
			],
			data: {
				id: 1,
				name: "请假审批",
				nodeConfig: {
					nodeName: "发起人",
					type: 0,
					nodeRoleList: [],
					childNode: {
						nodeName: "条件路由",
						type: 4,
						conditionNodes: [
							{
								nodeName: "长期",
								type: 3,
								priorityLevel: 1,
								conditionMode: 1,
								conditionList: [
									{
										label: "请假天数",
										field: "day",
										operator: ">",
										value: "7",
									},
								],
								childNode: {
									nodeName: "领导审批",
									type: 1,
									setType: 1,
									nodeUserList: [
										{
											id: "360000197302144442",
											name: "何敏",
										},
									],
									nodeRoleList: [],
									examineLevel: 1,
									directorLevel: 1,
									selectMode: 1,
									termAuto: false,
									term: 0,
									termMode: 1,
									examineMode: 1,
									directorMode: 0,
								},
							},
							{
								nodeName: "短期",
								type: 3,
								priorityLevel: 2,
								conditionMode: 1,
								conditionList: [],
								childNode: {
									nodeName: "直接主管审批",
									type: 1,
									setType: 2,
									nodeUserList: [],
									nodeRoleList: [],
									examineLevel: 1,
									directorLevel: 1,
									selectMode: 1,
									termAuto: false,
									term: 0,
									termMode: 1,
									examineMode: 1,
									directorMode: 0,
								},
							},
						],
						childNode: {
							nodeName: "抄送人",
							type: 2,
							userSelectFlag: true,
							nodeUserList: [
								{
									id: "220000200908305857",
									name: "何秀英",
								},
							],
						},
					},
				},
			},
		};
	},
	beforeRouteEnter(to, from, next) {
		next((vm) => {
			if (from.id) {
				vm.formData.id = from.id;
				delete from.id;
				vm.init();
			}
		});
	},
	mounted() {},
	methods: {
		async init() {
			var res = await this.$API.sysworkflow.model.get(this.formData.id);
			this.formData = res.data;
		},
		goBack() {
			this.$router.go(-1);
		},
		save() {
			this.formData.flow = JSON.stringify(this.data);
			this.$refs["formRef"].validate(async (valid) => {
				if (!valid) return;
				this.isSaveing = true;
				let res = null;
				if (this.formData.id === 0) {
					res = await this.$API.sysworkflow.add.post(this.formData);
				} else {
					res = await this.$API.sysworkflow.update.put(this.formData);
				}
				this.isSaveing = false;
				if (res.code == 200) {
					this.$message.success("发布成功");
				} else {
					this.$alert(res.message, "提示", { type: "error" });
				}
			});
		},
	},
};
</script>
<style scoped>
.flow-tab {
	display: flex;
}
.flow-tab span {
	display: inline-block;
	height: 59px;
	line-height: 59px;
	padding: 0 40px;
	font-size: 14px;
	cursor: pointer;
}
.flow-tab span.active {
	background-color: #f6f8fa;
}
.flow-container {
	height: calc(100% - 60px);
}
.flow-main {
	width: 100%;
	height: calc(100% - 0px);
	overflow: auto;
}
.basic-card {
	width: 900px;
	height: calc(100% - 40px);
	margin: 20px auto 0;
}
.flow-setting-title {
	margin-bottom: 15px;
}
>>> .basic-main .el-switch__core {
	width: 60px;
}
</style>
