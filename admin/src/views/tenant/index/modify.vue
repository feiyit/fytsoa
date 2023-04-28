<template>
	<sc-dialog
		v-model="visible"
		show-fullscreen
		:title="titleMap[mode]"
		width="700px"
		@close="close"
	>
		<el-form
			ref="formRef"
			label-width="100px"
			:model="formData"
			:rules="rules"
		>
			<el-form-item label="租户名称" prop="name">
				<el-input
					v-model="formData.name"
					placeholder="请输入租户名称"
					:maxlength="100"
					show-word-limit
					clearable
					:style="{ width: '100%' }"
				></el-input>
			</el-form-item>
			<el-row :gutter="15">
				<el-col :span="12">
					<el-form-item label="负责人" prop="person">
						<el-input
							v-model="formData.person"
							placeholder="请输入负责人"
							:maxlength="50"
							show-word-limit
							clearable
							:style="{ width: '100%' }"
						></el-input>
					</el-form-item>
				</el-col>
				<el-col :span="12">
					<el-form-item label="联系方式" prop="phone">
						<el-input
							v-model="formData.phone"
							placeholder="请输入联系方式"
							clearable
							:style="{ width: '100%' }"
						>
						</el-input>
					</el-form-item>
				</el-col>
				<el-col :span="12">
					<el-form-item label="超管账号" prop="account">
						<el-input
							v-model="formData.account"
							placeholder="请输入超管账号"
							:maxlength="30"
							show-word-limit
							clearable
							:style="{ width: '100%' }"
						></el-input>
					</el-form-item>
				</el-col>
				<el-col :span="12">
					<el-form-item label="超管密码" prop="passWord">
						<el-input
							v-model="formData.passWord"
							placeholder="请输入超管密码"
							:maxlength="50"
							show-word-limit
							clearable
							show-password
							:style="{ width: '100%' }"
						></el-input>
					</el-form-item>
				</el-col>
			</el-row>
			<el-form-item label="状态" prop="status" required>
				<el-switch
					v-model="formData.status"
					inline-prompt
					style="
						--el-switch-on-color: #13ce66;
						--el-switch-off-color: #ff4949;
					"
					active-text="Y"
					inactive-text="N"
				></el-switch>
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
				tenantId: 0,
				name: undefined,
				person: undefined,
				phone: undefined,
				account: undefined,
				passWord: undefined,
				status: true,
			},
			rules: {
				name: [
					{
						required: true,
						message: "请输入租户名称",
						trigger: "blur",
					},
				],
				person: [
					{
						required: true,
						message: "请输入负责人",
						trigger: "blur",
					},
				],
				phone: [],
				account: [
					{
						required: true,
						message: "请输入超管账号",
						trigger: "blur",
					},
				],
				passWord: [
					{
						required: true,
						message: "请输入超管密码",
						trigger: "blur",
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
				var res = await this.$API.systenant.model.get(row.id);
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
						res = await this.$API.systenant.add.post(this.formData);
					} else {
						res = await this.$API.systenant.update.put(
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
				tenantId: 0,
				name: undefined,
				person: undefined,
				phone: undefined,
				account: undefined,
				passWord: undefined,
				status: true,
			};
			this.$refs.formRef.resetFields();
			this.visible = false;
		},
	},
};
</script>
