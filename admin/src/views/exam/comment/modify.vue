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
			<el-form-item label="分类编号" prop="categoryId">
				<el-input
					v-model="categoryId"
					placeholder="请输入分类编号"
					:maxlength="50"
					show-word-limit
					clearable
				></el-input>
			</el-form-item>
			<el-form-item label="用户编号" prop="userId">
				<el-input
					v-model="userId"
					placeholder="请输入用户编号"
					:maxlength="50"
					show-word-limit
					clearable
				></el-input>
			</el-form-item>
			<el-form-item label="评论内容" prop="content">
				<el-input
					v-model="content"
					placeholder="请输入评论内容"
					:maxlength="900"
					show-word-limit
					clearable
				></el-input>
			</el-form-item>
			<el-form-item label="评星" prop="star">
				<el-input
					v-model="star"
					placeholder="请输入评星"
					:maxlength="11"
					show-word-limit
					clearable
				></el-input>
			</el-form-item>
			<el-form-item label="回复内容" prop="replyBody">
				<el-input
					v-model="replyBody"
					placeholder="请输入回复内容"
					:maxlength="900"
					show-word-limit
					clearable
				></el-input>
			</el-form-item>
			<el-form-item label="创建时间" prop="createTime">
				<el-input
					v-model="createTime"
					placeholder="请输入创建时间"
					:maxlength="0"
					show-word-limit
					clearable
				></el-input>
			</el-form-item>
			<el-form-item label="创建人" prop="createUser">
				<el-input
					v-model="createUser"
					placeholder="请输入创建人"
					:maxlength="255"
					show-word-limit
					clearable
				></el-input>
			</el-form-item>
			<el-form-item label="修改时间" prop="updateTime">
				<el-input
					v-model="updateTime"
					placeholder="请输入修改时间"
					:maxlength="0"
					show-word-limit
					clearable
				></el-input>
			</el-form-item>
			<el-form-item label="修改人" prop="updateUser">
				<el-input
					v-model="updateUser"
					placeholder="请输入修改人"
					:maxlength="255"
					show-word-limit
					clearable
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
				categoryId: "",
				userId: "",
				content: "",
				star: "",
				replyBody: "",
				createTime: "",
				createUser: "",
				updateTime: "",
				updateUser: "",
			},
			rules: {},
		};
	},
	mounted() {},
	methods: {
		async open(row) {
			if (!row) {
				this.mode = "add";
			} else {
				this.mode = "edit";
				var res = await this.$API.examcomment.model.get(row.id);
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
						res = await this.$API.examcomment.add.post(
							this.formData
						);
					} else {
						res = await this.$API.examcomment.update.put(
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
				categoryId: "",
				userId: "",
				content: "",
				star: "",
				replyBody: "",
				createTime: "",
				createUser: "",
				updateTime: "",
				updateUser: "",
			};
			this.$refs.formRef.resetFields();
			this.visible = false;
		},
	},
};
</script>
