<template>
	<sc-dialog
		v-model="visible"
		show-fullscreen
		:title="titleMap[mode]"
		:destroy-on-close="true"
		width="800px"
		@close="close"
	>
		<el-form
			ref="formRef"
			label-width="100px"
			:model="formData"
			:rules="rules"
		>
			<el-form-item label="所属机构" prop="parentId">
				<el-tree-select
					v-model="formData.parentId"
					:data="parentIdOptions"
					:default-expand-all="true"
					:check-strictly="true"
					:style="{ width: '100%' }"
				/>
			</el-form-item>
			<el-row>
				<el-col :span="12">
					<el-form-item label="机构名称" prop="name">
						<el-input
							v-model="formData.name"
							placeholder="请输入机构名称"
							clearable
						></el-input>
					</el-form-item>
				</el-col>
				<el-col :span="12">
					<el-form-item label="机构编号">
						<el-input
							v-model="formData.number"
							placeholder="请输入机构编号"
							clearable
						></el-input>
					</el-form-item>
				</el-col>
				<el-col :span="12">
					<el-form-item label="机构负责人" prop="leaderUser">
						<el-input
							v-model="formData.leaderUser"
							placeholder="请输入机构负责人"
							clearable
						></el-input
					></el-form-item>
				</el-col>
				<el-col :span="12">
					<el-form-item label="联系电话" prop="leaderMobile">
						<el-input
							v-model="formData.leaderMobile"
							placeholder="请输入联系电话"
							clearable
						></el-input>
					</el-form-item>
				</el-col>
				<el-col :span="12">
					<el-form-item label="联系邮箱" prop="leaderEmail">
						<el-input
							v-model="formData.leaderEmail"
							placeholder="请输入联系邮箱"
							clearable
						></el-input>
					</el-form-item>
				</el-col>
				<el-col :span="12">
					<el-form-item label="状态">
						<el-switch v-model="formData.status"></el-switch>
					</el-form-item>
				</el-col>
			</el-row>

			<el-form-item label="排序" prop="sort" required>
				<el-slider
					v-model="formData.sort"
					:max="100"
					show-input
					:step="1"
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
				name: "",
				leaderUser: "",
				leaderMobile: "",
				leaderEmail: "",
				status: true,
				sort: 1,
			},
			rules: {
				parentId: [
					{
						required: true,
						message: "请选择所属机构",
						trigger: "change",
					},
				],
				parentIdList: [
					{
						required: true,
						type: "array",
						message: "请至少选择一个所属机构",
						trigger: "change",
					},
				],
				name: [
					{
						required: true,
						message: "请输入机构名称",
						trigger: "blur",
					},
				],
				leaderUser: [
					{
						required: true,
						message: "请输入机构负责人",
						trigger: "blur",
					},
				],
				leaderMobile: [
					{
						required: true,
						message: "请输入联系电话",
						trigger: "blur",
					},
				],
				leaderEmail: [],
			},
			parentIdOptions: [],
			parentIdProps: {
				expandTrigger: "hover",
				//checkStrictly: true,
			},
		};
	},
	mounted() {},
	methods: {
		async initTree() {
			const t = await this.$API.sysorganize.list.get();
			let _tree = [{ id: "1", value: "0", label: "组织", parentId: "0" }];
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
				var res = await this.$API.sysorganize.model.get(row.id);
				this.formData = res.data;
			}
			this.visible = true;
		},
		save() {
			console.log("this.formData", this.formData);
			this.$refs.formRef.validate(async (valid) => {
				if (valid) {
					this.isSaveing = true;
					let res = null;
					if (this.formData.id === 0) {
						res = await this.$API.sysorganize.add.post(
							this.formData
						);
					} else {
						res = await this.$API.sysorganize.update.put(
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
				parentId: undefined,
				name: "",
				leaderUser: "",
				leaderMobile: "",
				leaderEmail: "",
				status: true,
				sort: 1,
			};
			this.$refs.formRef.resetFields();
			this.visible = false;
		},
	},
};
</script>
