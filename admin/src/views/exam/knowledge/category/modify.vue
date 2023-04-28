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
			<el-form-item label="分类父级" prop="parentId">
				<el-tree-select
					v-model="formData.parentId"
					:data="treedata"
					style="width: 100%"
				/>
			</el-form-item>
			<el-form-item label="分类名称" prop="name">
				<el-input
					v-model="formData.name"
					placeholder="请输入分类名称"
					:maxlength="255"
					show-word-limit
					clearable
				></el-input>
			</el-form-item>
			<el-form-item label="排序" prop="sort">
				<el-slider
					:max="100"
					:step="1"
					v-model="formData.sort"
				></el-slider>
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
				gradeId: "",
				name: "",
				parentId: "",
				sort: 0,
			},
			rules: {
				name: [
					{
						required: true,
						message: "请输入单行文本",
						trigger: "blur",
					},
				],
				parentId: [
					{
						required: true,
						message: "请选择上级选择",
						trigger: "change",
					},
				],
			},
			treedata: [],
		};
	},
	mounted() {},
	methods: {
		async initCategory(id) {
			const res = await this.$API.examknowledgecategory.list.get({
				id: id,
			});
			if (res.code == 200) {
				let treeArr = [
					{ id: "1", value: "0", label: "顶级菜单", parentId: "0" },
				];
				res.data.forEach(function (m) {
					treeArr.push({
						id: m.id,
						value: m.id,
						label: m.name,
						parentId: m.parentId,
					});
				});
				this.treedata = this.$TOOL.changeTree(treeArr);
			}
		},
		async open(row, grade) {
			this.initCategory(grade.id);
			this.formData.gradeId = grade.id;
			if (!row) {
				this.mode = "add";
			} else {
				this.mode = "edit";
				var res = await this.$API.examknowledgecategory.model.get(
					row.id
				);
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
						res = await this.$API.examknowledgecategory.add.post(
							this.formData
						);
					} else {
						res = await this.$API.examknowledgecategory.update.put(
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
				gradeId: "",
				name: "",
				parentId: "",
				sort: 0,
			};
			this.$refs.formRef.resetFields();
			this.visible = false;
		},
	},
};
</script>
