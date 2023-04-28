<template>
	<sc-dialog
		v-model="visible"
		show-fullscreen
		:title="titleMap[mode]"
		width="1000px"
		top="8vh"
		@close="close"
	>
		<el-container>
			<el-aside width="240px" class="no-right-border">
				<div class="select-img">
					<div class="bg-gray">
						<div class="up-wall">
							<sc-upload
								v-model="formData.avatar"
								:apiObj="uploadApi"
								:width="148"
								:height="148"
								:onSuccess="upSuccess"
							></sc-upload>
						</div>
					</div>
				</div>
			</el-aside>
			<el-container style="display: block">
				<el-form
					ref="formRef"
					label-width="100px"
					:model="formData"
					:rules="rules"
				>
					<el-form-item label="专业" prop="professionId">
						<el-select
							v-model="formData.professionId"
							placeholder="请选择专业"
							clearable
							:style="{ width: '100%' }"
						>
							<el-option
								v-for="(item, index) in professionOption"
								:key="index"
								:label="item.label"
								:value="item.value"
							></el-option>
						</el-select>
					</el-form-item>
					<el-form-item label="教师姓名" prop="name">
						<el-input
							v-model="formData.name"
							placeholder="请输入教师姓名"
							:maxlength="90"
							show-word-limit
							clearable
						></el-input>
					</el-form-item>
					<el-form-item label="职称" prop="postName">
						<el-input
							v-model="formData.postName"
							placeholder="请输入职称"
							:maxlength="90"
							show-word-limit
							clearable
						></el-input>
					</el-form-item>
					<el-form-item label="年龄" prop="age">
						<el-input-number
							v-model="formData.age"
							placeholder="年龄"
							:step="1"
							:min="20"
							:max="80"
						>
						</el-input-number>
					</el-form-item>
					<el-form-item
						label="介绍"
						prop="summary"
						class="item-editor"
					>
						<sc-editor
							v-model="formData.summary"
							placeholder="请输入介绍"
							:height="260"
						></sc-editor>
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
import { defineAsyncComponent } from "vue";
const scEditor = defineAsyncComponent(() => import("@/components/scEditor"));
export default {
	emits: ["complete"],
	components: {
		scEditor,
	},
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
				professionId: "",
				name: "",
				postName: "",
				age: "",
				avatar: "",
				summary: "",
			},
			rules: {
				professionId: [
					{
						required: true,
						message: "请选择专业",
						trigger: "change",
					},
				],
				name: [
					{
						required: true,
						message: "请选择教师姓名",
						trigger: "change",
					},
				],
				postName: [
					{
						required: true,
						message: "请输入职称",
						trigger: "blur",
					},
				],
				avatar: [
					{
						required: true,
						message: "请选择头像",
						trigger: "change",
					},
				],
				age: [
					{
						required: true,
						message: "年龄",
						trigger: "blur",
					},
				],
				summary: [
					{
						required: true,
						message: "请输入介绍",
						trigger: "blur",
					},
				],
			},
			professionOption: [],
		};
	},
	mounted() {
		this.initProfessionOption();
	},
	methods: {
		async initProfessionOption() {
			const res = await this.$API.syscode.list.get({
				typeCode: "profession",
			});
			if (res.code == 200) {
				res.data.forEach((e) => {
					this.professionOption.push({
						label: e.name,
						value: e.id,
					});
				});
			}
		},
		upSuccess(res) {
			this.formData.avatar = res.data.path;
			if (res.code == 200) {
				this.$message.success("上传成功~");
			} else {
				this.$message.warning(res.message);
			}
		},
		async open(row) {
			if (!row) {
				this.mode = "add";
			} else {
				this.mode = "edit";
				var res = await this.$API.examteacher.model.get(row.id);
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
						res = await this.$API.examteacher.add.post(
							this.formData
						);
					} else {
						res = await this.$API.examteacher.update.put(
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
				professionId: "",
				name: "",
				postName: "",
				age: "",
				avatar: "",
				summary: "",
			};
			this.$refs.formRef.resetFields();
			this.visible = false;
		},
	},
};
</script>
<style lang="scss" scoped>
.select-img {
	border-radius: 5px;
	border: 1px solid #e6e6e6;
	padding: 10px;
	text-align: center;
	.bg-gray {
		background-color: #f5f7fa;
		border-radius: 4px;
		width: 220px;
		height: 220px;
		cursor: pointer;
		padding: 35px 0 0 35px;
		i {
			font-size: 40px;
			color: #ccd1d9;
		}
	}
	.bg-gray > .up-wall {
		width: 148px;
		height: 148px;
		overflow: hidden;
	}
}
.user-else-info {
	background-color: #f6f9fd;
	text-align: center;
	padding: 5px 0;
	.last-login {
		padding: 10px 0;
	}
	p {
		margin: 5px 0;
	}
}
.user-pic {
	width: 100%;
	height: 200px !important;
}
.cur-right {
	padding-left: 240px;
}
.phote-wall {
	width: 220px;
	height: 210px;
	position: relative;
	border: 0px;
	img {
		width: 100%;
		height: 210px;
	}
	.phote-edit {
		text-align: center;
		position: absolute;
		top: 0;
		right: 0;
		left: 0;
		bottom: 0;
		z-index: 10;
		background: rgba(0, 0, 0, 0.5);
		padding-top: 40%;
		display: none;
	}
	.el-link.el-link--default {
		color: #ffffff;
	}
	.el-link {
		font-size: 20px;
		margin: 0 10px;
	}
}
.phote-wall:hover .phote-edit {
	display: block;
}
.is-hide {
	display: none;
}
.no-right-border {
	border-right: none;
}
[data-theme="dark"] .user-else-info {
	background: #383838;
}
[data-theme="dark"] .select-img {
	border: 1px solid #6d6d6d;
}
[data-theme="dark"] .select-img .bg-gray {
	background: transparent;
}
</style>
<style scoped>
.item-editor >>> .el-form-item__content {
	display: block;
}
</style>
