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
			<el-form-item label="所属角色" prop="parentId">
				<el-tree-select
					v-model="formData.parentId"
					placeholder="请选择所属角色"
					:data="parentIdOptions"
					collapse-tags
					check-strictly
					default-expand-all
					:style="{ width: '100%' }"
				/>
			</el-form-item>
			<el-form-item label="角色名称" prop="name">
				<el-input
					v-model="formData.name"
					clearable
					:maxlength="30"
					placeholder="请输入角色名称"
					show-word-limit
					:style="{ width: '100%' }"
				/>
			</el-form-item>
			<el-row>
				<el-col :span="12">
					<el-form-item label="角色最大数" prop="maxLength" required>
						<el-input-number
							v-model="formData.maxLength"
							:min="0"
							:max="100"
							controls-position="right"
						/>
						<span class="cur-tip">0为不限制</span>
					</el-form-item>
				</el-col>
				<el-col :span="12">
					<el-form-item label="状态" prop="status">
						<el-radio-group v-model="formData.status" size="medium">
							<el-radio
								v-for="(item, index) in statusOptions"
								:key="index"
								:disabled="item.disabled"
								:label="item.value"
							>
								{{ item.label }}
							</el-radio>
						</el-radio-group>
					</el-form-item>
				</el-col>
			</el-row>
			<el-form-item label="是否超管" prop="isSystem" required>
				<el-switch
					v-model="formData.isSystem"
					active-text="如果是超管，则不允许删除"
				/>
			</el-form-item>
			<el-form-item label="备注" prop="summary">
				<el-input
					v-model="formData.summary"
					:autosize="{ minRows: 2, maxRows: 4 }"
					:maxlength="500"
					placeholder="请输入备注"
					show-word-limit
					:style="{ width: '100%' }"
					type="textarea"
				/>
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
				parentId: undefined,
				name: undefined,
				isSystem: false,
				summary: undefined,
				status: true,
				maxLength: 0,
				sort: 1,
			},
			rules: {
				parentId: [
					{
						required: true,
						message: "请选择所属角色",
						trigger: "change",
					},
				],
				name: [
					{
						required: true,
						message: "请输入角色名称",
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
				summary: [],
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
			parentIdOptions: [],
			parentIdProps: {
				multiple: false,
				checkStrictly: true,
				expandTrigger: "hover",
			},
		};
	},
	mounted() {},
	methods: {
		async initTree() {
			const t = await this.$API.sysrole.list.get();
			let _tree = [
				{ id: "1", value: "0", label: "角色组", parentId: "0" },
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
			this.initTree();
			if (!row) {
				this.mode = "add";
			} else {
				this.mode = "edit";
				var res = await this.$API.sysrole.model.get(row.id);
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
						res = await this.$API.sysrole.add.post(this.formData);
					} else {
						res = await this.$API.sysrole.update.put(this.formData);
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
				parentId: undefined,
				name: undefined,
				isSystem: false,
				summary: undefined,
				status: true,
				maxLength: 0,
				sort: 1,
			};
			this.$refs.formRef.resetFields();
			this.visible = false;
		},
	},
};
</script>
<style scoped>
.cur-tip {
	display: inline-block;
	padding-left: 10px;
	color: #666;
	font-size: 12px;
}
</style>
