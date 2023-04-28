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
			<el-form-item label="上级城市" prop="parentIdList">
				<el-cascader
					v-model="formData.parentIdList"
					:options="parentIdOptions"
					:props="parentIdProps"
					:style="{ width: '100%' }"
					placeholder="请选择上级城市"
					clearable
				></el-cascader>
			</el-form-item>
			<el-form-item label="城市名称" prop="name">
				<el-input
					v-model="formData.name"
					placeholder="请输入城市名称"
					:maxlength="50"
					show-word-limit
					clearable
					:style="{ width: '100%' }"
				></el-input>
			</el-form-item>
			<el-form-item label="城市编码" prop="code">
				<el-input
					v-model="formData.code"
					placeholder="请输入城市编码"
					:maxlength="30"
					show-word-limit
					clearable
					:style="{ width: '100%' }"
				></el-input>
			</el-form-item>
			<el-row>
				<el-col :span="12">
					<el-form-item label="经度" prop="longitude">
						<el-input
							v-model="formData.longitude"
							placeholder="请输入经度"
							:maxlength="30"
							show-word-limit
							clearable
							:style="{ width: '100%' }"
						></el-input>
					</el-form-item>
				</el-col>
				<el-col :span="12">
					<el-form-item label="维度" prop="dimension">
						<el-input
							v-model="formData.dimension"
							placeholder="请输入维度"
							:maxlength="30"
							show-word-limit
							clearable
							:style="{ width: '100%' }"
						></el-input>
					</el-form-item>
				</el-col>
			</el-row>
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
				code: undefined,
				longitude: undefined,
				dimension: undefined,
			},
			rules: {
				parentIdList: [
					{
						required: true,
						type: "array",
						message: "请至少选择一个上级城市",
						trigger: "change",
					},
				],
				name: [
					{
						required: true,
						message: "请输入城市名称",
						trigger: "blur",
					},
				],
				code: [
					{
						required: true,
						message: "请输入城市编码",
						trigger: "blur",
					},
				],
				dimension: [],
			},
			parentIdOptions: [],
			parentIdProps: {
				multiple: false,
				checkStrictly: true,
				expandTrigger: "hover",
			},
		};
	},
	mounted() {
		
	},
	methods: {
		async initTree() {
			const t = await this.$API.syscity.list.get();
			let _tree = [
				{ id: "1", value: "0", label: "根目录", parentId: "0" },
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
				var res = await this.$API.syscity.model.get(row.id);
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
						res = await this.$API.syscity.add.post(this.formData);
					} else {
						res = await this.$API.syscity.update.put(this.formData);
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
				code: undefined,
				longitude: undefined,
				dimension: undefined,
			};
			this.$refs.formRef.resetFields();
			this.visible = false;
		},
	},
};
</script>
