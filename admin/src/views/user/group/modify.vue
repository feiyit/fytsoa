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
			label-width="100px"
			:model="formData"
			:rules="rules"
		>
			<el-form-item label="会员组名称" prop="name">
				<el-input
					v-model="formData.name"
					placeholder="请选择会员组名称"
					clearable
					:style="{ width: '100%' }"
				></el-input>
			</el-form-item>
			<el-row>
				<el-col :span="12">
					<el-form-item label="升级积分" prop="upPoint">
						<el-input-number
							v-model="formData.upPoint"
							placeholder="请输入升级积分"
							:step="1"
						></el-input-number>
					</el-form-item>
				</el-col>
				<el-col :span="12">
					<el-form-item label="升级金额" prop="upMoney">
						<el-input-number
							v-model="formData.upMoney"
							placeholder="请输入升级金额"
							:step="1"
						></el-input-number>
					</el-form-item>
				</el-col>
			</el-row>
			<el-form-item label="状态" prop="status" required>
				<el-switch v-model="formData.status"></el-switch>
			</el-form-item>
			<el-form-item label="描述" prop="summary">
				<el-input
					v-model="formData.summary"
					type="textarea"
					placeholder="请输入描述"
					:maxlength="200"
					:autosize="{ minRows: 3, maxRows: 4 }"
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
export default {
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
			formData: {
				id: 0,
				name: undefined,
				upPoint: 0,
				upMoney: 0,
				status: true,
				summary: undefined,
			},
			rules: {
				name: [
					{
						required: true,
						message: "请选择会员组名称",
						trigger: "blur",
					},
				],
				upPoint: [
					{
						required: true,
						message: "请输入升级积分",
						trigger: "blur",
					},
				],
				upMoney: [
					{
						required: true,
						message: "请输入升级金额",
						trigger: "blur",
					},
				],
				summary: [],
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
				var res = await this.$API.membergroup.model.get(row.id);
				this.formData = res.data;
			}
			this.visible = true;
		},
		save() {
			this.$refs.formRef.validate(async (valid) => {
				if (valid) {
					this.isSaveing = true;
					let res = null;
					if (this.formData.id === 0) {
						res = await this.$API.membergroup.add.post(
							this.formData
						);
					} else {
						res = await this.$API.membergroup.update.put(
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
				name: undefined,
				upPoint: 0,
				upMoney: 0,
				status: true,
				summary: undefined,
			};
			this.$refs.formRef.resetFields();
			this.visible = false;
		},
	},
};
</script>
