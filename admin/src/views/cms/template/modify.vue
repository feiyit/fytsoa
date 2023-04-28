<template>
	<sc-dialog
		v-model="visible"
		show-fullscreen
		:title="titleMap[mode]"
		width="650px"
		@close="close"
	>
		<el-form
			ref="formRef"
			label-width="100px"
			:model="formData"
			:rules="rules"
		>
			<el-form-item label="模板名称" prop="name">
				<el-input
					v-model="formData.name"
					placeholder="请输入模板名称"
					:maxlength="30"
					show-word-limit
					clearable
					:style="{ width: '100%' }"
				></el-input>
			</el-form-item>
			<el-form-item label="模板地址" prop="urls">
				<el-input
					v-model="formData.urls"
					placeholder="请输入模板地址"
					:maxlength="100"
					show-word-limit
					clearable
					:style="{ width: '100%' }"
				></el-input>
			</el-form-item>
			<el-form-item label="可用状态" prop="status" required>
				<el-switch v-model="formData.status"></el-switch>
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
	emits: ['complete'],
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
				urls: undefined,
				status: true,
			},
			rules: {
				name: [
					{
						required: true,
						message: "请输入模板名称",
						trigger: "blur",
					},
				],
				urls: [
					{
						required: true,
						message: "请输入模板地址",
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
				var res = await this.$API.cmstemplate.model.get(row.id);
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
						res = await this.$API.cmstemplate.add.post(
							this.formData
						);
					} else {
						res = await this.$API.cmstemplate.update.put(
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
				urls: undefined,
				status: true,
			};
			this.$refs.formRef.resetFields();
			this.visible = false;
		},
	},
};
</script>
