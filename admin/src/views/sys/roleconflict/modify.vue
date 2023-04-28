<template>
	<sc-dialog
		v-model="visible"
		show-fullscreen
		destroy-on-close
		:title="titleMap[mode]"
		width="600px"
		@close="close"
	>
		<el-form
			ref="formRef"
			label-width="100px"
			:model="formData"
			:rules="rules"
		>
			<el-form-item label="角色A" prop="roleA">
				<el-tree-select
					v-model="formData.roleA"
					placeholder="请选择角色A"
					:data="roleOptions"
					collapse-tags
					check-strictly
					default-expand-all
					:style="{ width: '100%' }"
				/>
			</el-form-item>
			<el-form-item label="角色B" prop="roleB">
				<el-tree-select
					v-model="formData.roleB"
					placeholder="请选择角色B"
					:data="roleOptions"
					collapse-tags
					check-strictly
					default-expand-all
					:style="{ width: '100%' }"
				/>
			</el-form-item>
			<el-form-item label="互斥说明" prop="summary">
				<el-input
					v-model="formData.summary"
					placeholder="请输入互斥说明"
					:maxlength="255"
					show-word-limit
					clearable
					:autosize="{ minRows: 2, maxRows: 4 }"
					:style="{ width: '100%' }"
					type="textarea"
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
				roleA: "",
				roleB: "",
				summary: "",
			},
			rules: {
				roleA: [
					{
						required: true,
						message: "请选择角色A",
						trigger: "change",
					},
				],
				roleB: [
					{
						required: true,
						message: "请选择角色B",
						trigger: "change",
					},
				],
				summary: [
					{
						required: true,
						message: "请输入互斥说明",
						trigger: "blur",
					},
				],
			},
			roleOptions: [],
			roleList: [],
		};
	},
	mounted() {
		this.initTree();
	},
	methods: {
		async initTree() {
			const t = await this.$API.sysrole.list.get();
			this.roleList = t.data;
			let _tree = [];
			t.data.some((m) => {
				_tree.push({
					id: m.id,
					value: m.id,
					label: m.name,
					parentId: m.parentId,
				});
			});
			this.roleOptions = this.$TOOL.changeTree(_tree);
		},
		async open(row) {
			if (!row) {
				this.mode = "add";
			} else {
				this.mode = "edit";
				var res = await this.$API.sysroleconflict.model.get(row.id);
				this.formData = res.data;
			}
			this.visible = true;
		},
		save() {
			const roleA = this.roleList.find(
				(m) => m.id == this.formData.roleA
			);
			const roleB = this.roleList.find(
				(m) => m.id == this.formData.roleB
			);
			if (roleA.parentIdList.find((m) => m == roleB.id)) {
				this.$message.warning("角色B不能和角色A存在父子关系");
				return;
			}
			if (roleB.parentIdList.find((m) => m == roleA.id)) {
				this.$message.warning("角色A不能和角色B存在父子关系");
				return;
			}
			if (roleB.id == roleA.id) {
				this.$message.warning("角色A不能和角色B不允许相同");
				return;
			}
			this.$refs.formRef.validate(async (valid) => {
				if (valid) {
					this.isSaveing = true;
					let res = null;
					if (this.formData.id === 0) {
						res = await this.$API.sysroleconflict.add.post(
							this.formData
						);
					} else {
						res = await this.$API.sysroleconflict.update.put(
							this.formData
						);
					}
					this.isSaveing = false;
					if (res.code == 200) {
						this.$emit("complete");
						this.visible = false;
						this.$message.success("保存成功");
					} else {
						this.$alert(res.message, "提示", { type: "error" });
					}
				}
			});
		},
		close() {
			this.formData = {
				id: 0,
				roleA: "",
				roleB: "",
				summary: "",
			};
			this.$refs.formRef.resetFields();
			this.visible = false;
		},
	},
};
</script>
