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
			<el-form-item label="上级栏目" prop="parentIdList">
				<el-cascader
					v-model="formData.parentIdList"
					:options="parentIdOptions"
					:props="parentIdProps"
					:style="{ width: '100%' }"
					placeholder="请选择上级栏目"
					clearable
					filterable
				></el-cascader>
			</el-form-item>
			<el-row>
				<el-col :span="12">
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
				</el-col>
				<el-col :span="12">
					<el-form-item label="栏目标识" prop="code">
						<el-input
							v-model="formData.flag"
							placeholder="请输入栏目标识"
							:maxlength="30"
							show-word-limit
							clearable
							:style="{ width: '100%' }"
						></el-input>
					</el-form-item>
				</el-col>
				<el-col :span="12">
					<el-form-item label="广告位宽度" prop="width">
						<el-input
							v-model="formData.width"
							placeholder="请输入宽度"
							:maxlength="10"
							show-word-limit
							clearable
							:style="{ width: '100%' }"
						></el-input>
					</el-form-item>
				</el-col>
				<el-col :span="12">
					<el-form-item label="广告位高度" prop="height">
						<el-input
							v-model="formData.height"
							placeholder="请输入高度"
							:maxlength="10"
							show-word-limit
							clearable
							:style="{ width: '100%' }"
						></el-input>
					</el-form-item>
				</el-col>
			</el-row>
			<el-form-item label="状态" prop="status" required>
				<el-switch v-model="formData.status"></el-switch>
			</el-form-item>
			<el-form-item label="广告位说明" prop="summary">
				<el-input
					v-model="formData.summary"
					type="textarea"
					placeholder="简要说明一下，广告的位置，作用等。"
					:maxlength="500"
					show-word-limit
					:autosize="{ minRows: 3, maxRows: 3 }"
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
				parentIdList: [],
				name: undefined,
				flag: undefined,
				width: undefined,
				height: undefined,
				status: true,
				summary: undefined,
			},
			rules: {
				parentIdList: [
					{
						required: true,
						type: "array",
						message: "请选择上级栏目",
						trigger: "change",
					},
				],
				name: [
					{
						required: true,
						message: "请输入栏目名称",
						trigger: "blur",
					},
				],
				flag: [
					{
						required: true,
						message: "请选择广告类型",
						trigger: "change",
					},
				],
				width: [
					{
						required: true,
						message: "请输入宽度",
						trigger: "blur",
					},
					{
						pattern: /^\d+$/,
						message: "必须为数字",
						trigger: "blur",
					},
				],
				height: [
					{
						required: true,
						message: "请输入高度",
						trigger: "blur",
					},
					{
						pattern: /^\d+$/,
						message: "必须为数字",
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
		};
	},
	mounted() {},
	methods: {
		async initTree() {
			const t = await this.$API.sysadvcolumn.list.get();
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
			this.initTree();
			if (!row) {
				this.mode = "add";
			} else {
				this.mode = "edit";
				var res = await this.$API.sysadvcolumn.model.get(row.id);
				res.data.parentIdList.pop();
				if (res.data.parentIdList.length == 0)
					res.data.parentIdList = ["0"];
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
						res = await this.$API.sysadvcolumn.add.post(
							this.formData
						);
					} else {
						res = await this.$API.sysadvcolumn.update.put(
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
				parentIdList: [],
				name: undefined,
				flag: undefined,
				width: undefined,
				height: undefined,
				status: true,
				summary: undefined,
			};
			this.$refs.formRef.resetFields();
			this.visible = false;
		},
	},
};
</script>
