<template>
	<sc-dialog
		v-model="visible"
		show-fullscreen
		:title="titleMap[mode]"
		width="850px"
		@close="close"
	>
		<el-container>
			<el-aside width="200px" class="no-right-border">
				<sc-upload
					v-model="formData.cover"
					class="cover"
					:apiObj="uploadApi"
					:width="150"
					:height="200"
					:onSuccess="upSuccess"
				></sc-upload>
				<el-alert
					title="文档封面大小宽度300像素，高度400像素"
					type="success"
					class="malert"
					:closable="false"
				/>
			</el-aside>
			<el-container style="display: block">
				<el-form
					ref="formRef"
					label-width="100px"
					:model="formData"
					:rules="rules"
				>
					<el-form-item label="年级编号" prop="gradeId">
						<el-select
							v-model="formData.gradeId"
							placeholder="请选择年级"
							clearable
							:style="{ width: '100%' }"
							@change="gradeChange"
						>
							<el-option
								v-for="(item, index) in gradeOption"
								:key="index"
								:label="item.label"
								:value="item.value"
							></el-option>
						</el-select>
					</el-form-item>
					<el-form-item label="知识分类" prop="categoryId">
						<el-tree-select
							v-model="formData.categoryId"
							placeholder="请选择知识分类"
							:data="treedata"
							style="width: 100%"
						/>
					</el-form-item>
					<el-form-item label="标题" prop="title">
						<el-input
							v-model="formData.title"
							placeholder="请输入标题"
							:maxlength="255"
							show-word-limit
							clearable
						></el-input>
					</el-form-item>
					<el-form-item label="文档PDF" prop="document">
						<sc-upload-file
							:apiObj="uploadApi"
							accept=".pdf"
							v-model="formData.document"
							:limit="1"
							:multiple="false"
							drag
							:onSuccess="upPdfSuccess"
						>
							<el-icon class="el-icon--upload"
								><el-icon-upload-filled
							/></el-icon>
							<div class="el-upload__text">
								将PDF文件拖放到这里 或者 <em>点击上传</em>
							</div>
						</sc-upload-file>
					</el-form-item>
				</el-form>
			</el-container>
		</el-container>

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
			uploadApi: this.$API.sysfile.knowledge,
			isSaveing: false,
			visible: false,
			formData: {
				id: 0,
				gradeId: "",
				categoryId: "",
				title: "",
				cover: "",
				document: "",
				pageCount: 0,
			},
			rules: {
				gradeId: [
					{
						required: true,
						message: "请选择年级编号",
						trigger: "change",
					},
				],
				categoryId: [
					{
						required: true,
						message: "请选择知识分类",
						trigger: "change",
					},
				],
				title: [
					{
						required: true,
						message: "请输入知识标题",
						trigger: "blur",
					},
				],
			},
			gradeOption: [],
			treedata: [],
		};
	},
	mounted() {
		this.initGrandOption();
	},
	methods: {
		async initGrandOption() {
			const res = await this.$API.syscode.list.get({ typeCode: "grand" });
			if (res.code == 200) {
				res.data.forEach((e) => {
					this.gradeOption.push({
						label: e.name,
						value: e.id,
					});
				});
			}
		},
		async initCategory(id) {
			const res = await this.$API.examknowledgecategory.list.get({
				id: id,
			});
			if (res.code == 200) {
				let treeArr = [];
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
		gradeChange(v) {
			this.initCategory(v);
		},
		upSuccess(res) {
			this.formData.cover = res.data.path;
			if (res.code == 200) {
				this.$message.success("封面上传成功~");
			} else {
				this.$message.warning(res.message);
			}
		},
		upPdfSuccess(res) {
			this.formData.document = res.data.path;
			if (res.code == 200) {
				this.$message.success("文档上传成功~");
			} else {
				this.$message.warning(res.message);
			}
		},
		async open(row) {
			if (!row) {
				this.mode = "add";
			} else {
				this.mode = "edit";
				var res = await this.$API.examknowledge.model.get(row.id);
				this.initCategory(res.data.gradeId);
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
						res = await this.$API.examknowledge.add.post(
							this.formData
						);
					} else {
						res = await this.$API.examknowledge.update.put(
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
				categoryId: "",
				title: "",
				cover: "",
				document: "",
				pageCount: 0,
			};
			this.$refs.formRef.resetFields();
			this.visible = false;
		},
	},
};
</script>
<style scoped>
.cover {
	margin: auto;
}
.cover >>> .el-upload--picture-card {
	height: 200px;
}
.malert {
	margin: 10px auto;
	width: 150px;
}
</style>
