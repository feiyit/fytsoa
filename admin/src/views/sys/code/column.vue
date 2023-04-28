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
			<el-form-item label="上级栏目" prop="parentId">
				<el-tree-select
					placeholder="请选择上级栏目"
					clearable
					filterable
					default-expand-all
					:check-strictly="true"
					highlight-current
					:indent="24"
					v-model="formData.parentId"
					:data="parentIdOptions"
					:style="{ width: '100%' }"
				/>
			</el-form-item>
			<el-form-item label="栏目名称" prop="name">
				<el-input
					v-model="formData.name"
					placeholder="请输入栏目名称"
					:maxlength="30"
					show-word-limit
					clearable
					:style="{ width: '100%' }"
				></el-input>
			</el-form-item>
			<el-form-item label="栏目标识" prop="code">
				<el-input
					v-model="formData.code"
					placeholder="请输入栏目标识"
					:maxlength="30"
					show-word-limit
					clearable
					:style="{ width: '100%' }"
				></el-input>
			</el-form-item>

			<el-form-item label="状态" prop="isSystem" required>
				<el-switch
					v-model="formData.isSystem"
					active-text="是否为系统内置集成，如果为是，则不允许删除"
					active-color="#EB5B02"
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
				types: 1,
				parentId: "",
				parentIdList: [],
				name: undefined,
				isSystem: false,
				sort: 1,
			},
			rules: {
				parentId: [
					{
						required: true,
						message: "请选择上级栏目",
						trigger: "blur",
					},
				],
				name: [
					{
						required: true,
						message: "请输入栏位名称",
						trigger: "blur",
					},
				],
				code: [
					{
						required: true,
						message: "请输入栏位标识",
						trigger: "blur",
					},
				],
			},
			parentIdOptions: [],
			parentIdProps: {
				multiple: false,
				checkStrictly: true,
				expandTrigger: "hover",
			},
			defaultParam: { type: 1 },
		};
	},
	mounted() {},
	methods: {
		async initTree(param) {
			const t = await this.$API.syscodetype.list.get(param);
			let _tree = [
				{ id: "1", value: "0", label: "一级栏目", parentId: "0" },
			];
			t.data.some((m) => {
				_tree.push({
					id: m.id,
					value: m.id,
					label: m.name,
					parentId: m.parentId,
				});
			});
			this.parentIdOptions = this.$TOOL.changeTree(_tree);
		},
		async open(row) {
			this.defaultParam.type = row.type;
			this.initTree({ type: this.defaultParam.type });
			if (!row.id) {
				this.mode = "add";
				this.formData.types = row.type;
			} else {
				this.mode = "edit";
				var res = await this.$API.syscodetype.model.get(row.id);
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
						res = await this.$API.syscodetype.add.post(
							this.formData
						);
					} else {
						res = await this.$API.syscodetype.update.put(
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
				parentId: "",
				parentIdList: [],
				name: undefined,
				isSystem: false,
				sort: 1,
			};
			this.$refs.formRef.resetFields();
			this.visible = false;
		},
	},
};
</script>
