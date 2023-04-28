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
			<el-form-item label="字典名称" prop="name">
				<el-input
					v-model="formData.name"
					placeholder="请输入字典名称"
					:maxlength="30"
					show-word-limit
					clearable
					:style="{ width: '100%' }"
				></el-input>
			</el-form-item>
			<el-form-item label="字典阈值" prop="codeValues">
				<el-input
					v-model="formData.codeValues"
					placeholder="请输入字典阈值，如A、B"
					:maxlength="30"
					show-word-limit
					clearable
					:style="{ width: '100%' }"
				></el-input>
			</el-form-item>
			<el-form-item label="状态" prop="status" required>
				<el-switch
					v-model="formData.status"
					active-text="是否启用"
				></el-switch>
			</el-form-item>
			<el-form-item label="排序" prop="sort" required>
				<el-slider
					v-model="formData.sort"
					:max="100"
					:step="1"
				></el-slider>
			</el-form-item>
			<el-form-item label="备注">
				<el-input
					v-model="formData.summary"
					type="textarea"
					placeholder="请输入备注"
					:maxlength="100"
					show-word-limit
					:autosize="{ minRows: 2, maxRows: 4 }"
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
				tag: 1,
				typeId: 0,
				name: undefined,
				codeValues: undefined,
				status: true,
				sort: 1,
				summary: undefined,
			},
			rules: {},
		};
	},
	mounted() {
		if (this.$route.path === "/exam/setting") {
			this.formData.tag = 2;
		}
		if (this.$route.path === "/crm/config") {
			this.formData.tag = 3;
		}
	},
	methods: {
		async open(row, type = "edit") {
			if (type == "add") {
				this.mode = type;
				this.formData.typeId = row.id;
				this.formData.tag = row.type;
			} else {
				this.mode = type;
				var res = await this.$API.syscode.model.get(row.id);
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
						res = await this.$API.syscode.add.post(this.formData);
					} else {
						res = await this.$API.syscode.update.put(this.formData);
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
				codeValues: undefined,
				status: true,
				sort: 1,
				summary: undefined,
			};
			this.$refs.formRef.resetFields();
			this.visible = false;
		},
	},
};
</script>
