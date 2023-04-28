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
			<el-form-item label="岗位编号" prop="number">
				<el-input
					v-model="formData.number"
					placeholder="请输入岗位编号,例如：10001"
					:maxlength="6"
					show-word-limit
					clearable
					:style="{ width: '100%' }"
				></el-input>
			</el-form-item>
			<el-form-item label="岗位名称" prop="name">
				<el-input
					v-model="formData.name"
					placeholder="请输入岗位名称"
					:maxlength="25"
					show-word-limit
					clearable
					:style="{ width: '100%' }"
				></el-input>
			</el-form-item>
			<el-form-item label="排序" prop="sort" required>
				<el-slider
					v-model="formData.sort"
					:max="100"
					:step="1"
				></el-slider>
			</el-form-item>
			<el-form-item label="状态" prop="status">
				<el-radio-group v-model="formData.status" size="medium">
					<el-radio
						v-for="(item, index) in statusOptions"
						:key="index"
						:label="item.value"
						:disabled="item.disabled"
					>
						{{ item.label }}
					</el-radio>
				</el-radio-group>
			</el-form-item>
			<el-form-item label="备注" prop="summary">
				<el-input
					v-model="formData.summary"
					type="textarea"
					placeholder="请输入备注"
					:maxlength="500"
					show-word-limit
					:autosize="{ minRows: 2, maxRows: 3 }"
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
				number: undefined,
				name: undefined,
				sort: 1,
				status: true,
				summary: undefined,
			},
			rules: {
				number: [
					{
						required: true,
						message: "请输入岗位编号",
						trigger: "blur",
					},
				],
				name: [
					{
						required: true,
						message: "请输入岗位名称",
						trigger: "blur",
					},
				],
				status: [
					{
						required: true,
						message: "状态不能为空",
						trigger: "change",
					},
				],
				summary: [
					{
						required: false,
						message: "请输入备注",
						trigger: "blur",
					},
				],
			},
			statusOptions: [
				{
					label: "正常",
					value: true,
				},
				{
					label: "停用",
					value: false,
				},
			],
		};
	},
	mounted() {
	},
	methods: {
		async open(row) {
			if (!row) {
				this.mode = "add";
			} else {
				this.mode = "edit";
				var res = await this.$API.syspost.model.get(row.id);
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
						res = await this.$API.syspost.add.post(this.formData);
					} else {
						res = await this.$API.syspost.update.put(this.formData);
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
				number: undefined,
				name: undefined,
				sort: 1,
				status: true,
				summary: undefined,
			};
			this.$refs.formRef.resetFields();
			this.visible = false;
		},
	},
};
</script>
