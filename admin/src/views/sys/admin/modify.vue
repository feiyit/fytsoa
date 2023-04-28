<template>
	<sc-dialog
		v-model="visible"
		show-fullscreen
		:title="titleMap[mode]"
		width="900px"
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
				<div class="user-else-info">
					<p class="last-login">
						上次登录：{{ formData.upLoginTime }}
					</p>
					<el-row>
						<el-col :span="8">
							<span>{{ formData.loginCount }}</span>
							<p>次数</p>
						</el-col>
						<el-col :span="8">
							<span
								><el-icon
									><el-icon-circle-check-filled /></el-icon
							></span>
							<p>状态</p>
						</el-col>
						<el-col :span="8">
							<span>13</span>
							<p>消息</p>
						</el-col>
					</el-row>
				</div>
			</el-aside>
			<el-container
				><el-form
					ref="formRef"
					label-width="100px"
					:model="formData"
					:rules="rules"
				>
					<el-row>
						<el-col :span="12">
							<el-form-item label="所属部门" prop="organizeId">
								<el-tree-select
									v-model="formData.organizeId"
									placeholder="请选择所属部门"
									:data="organizeOptions"
									default-expand-all
									check-strictly
									:style="{ width: '100%' }"
								/>
							</el-form-item>
						</el-col>
						<el-col :span="12">
							<el-form-item label="所属角色" prop="roleGroup">
								<el-tree-select
									v-model="formData.roleGroup"
									placeholder="请选择所属角色"
									:data="roleOptions"
									multiple
									collapse-tags
									check-strictly
									default-expand-all
									:style="{ width: '100%' }"
								/>
							</el-form-item>
						</el-col>
						<el-col :span="12">
							<el-form-item label="登录账号" prop="loginAccount">
								<el-input
									v-model="formData.loginAccount"
									placeholder="请输入登录账号"
									:maxlength="30"
									show-word-limit
									clearable
									:style="{ width: '100%' }"
								></el-input>
							</el-form-item>
						</el-col>

						<el-col :span="12">
							<el-form-item label="登录密码" prop="loginPassWord">
								<el-input
									v-model="formData.loginPassWord"
									placeholder="请输入登录密码"
									:maxlength="30"
									:disabled="formData.id != 0"
									show-word-limit
									clearable
									show-password
									:style="{ width: '100%' }"
								></el-input>
							</el-form-item>
						</el-col>

						<el-col :span="12">
							<el-form-item label="姓名" prop="fullName">
								<el-input
									v-model="formData.fullName"
									placeholder="请输入姓名"
									:maxlength="20"
									clearable
									:style="{ width: '100%' }"
								></el-input>
							</el-form-item>
						</el-col>
						<el-col :span="12">
							<el-form-item label="手机号码" prop="mobile">
								<el-input
									v-model="formData.mobile"
									placeholder="请输入手机号码"
									:maxlength="11"
									clearable
									:style="{ width: '100%' }"
								></el-input>
							</el-form-item>
						</el-col>
						<el-col :span="12">
							<el-form-item label="所属岗位" prop="postGroup">
								<sc-select
									v-model="formData.postGroup"
									:apiObj="$API.syspost.list"
									placeholder="请选择所属岗位"
									clearable
									filterable
									multiple
									style="width: 100%"
								></sc-select>
							</el-form-item>
						</el-col>
						<el-col :span="12">
							<el-form-item label="性别" prop="sex">
								<el-radio-group
									v-model="formData.sex"
									size="medium"
								>
									<el-radio
										v-for="(item, index) in sexOptions"
										:key="index"
										:label="item.value"
										:disabled="item.disabled"
									>
										{{ item.label }}
									</el-radio>
								</el-radio-group>
							</el-form-item>
						</el-col>
						<el-col :span="12">
							<el-form-item label="邮箱" prop="email">
								<el-input
									v-model="formData.email"
									placeholder="请输入邮箱"
									:maxlength="50"
									show-word-limit
									clearable
									:style="{ width: '100%' }"
								></el-input>
							</el-form-item>
						</el-col>
						<el-col :span="12">
							<el-form-item label="状态" prop="status" required>
								<el-switch
									v-model="formData.status"
									active-text="冻结用户，无法登录"
								></el-switch>
							</el-form-item>
						</el-col>
					</el-row>

					<el-form-item label="描述" prop="summary">
						<el-input
							v-model="formData.summary"
							type="textarea"
							placeholder="请输入描述"
							:maxlength="200"
							show-word-limit
							:autosize="{ minRows: 2, maxRows: 3 }"
							:style="{ width: '100%' }"
						></el-input>
					</el-form-item> </el-form
			></el-container>
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
	data() {
		return {
			uploadApi: this.$API.sysfile.avatar,
			mode: "add",
			titleMap: {
				add: "新增",
				edit: "编辑",
			},
			isSaveing: false,
			visible: false,
			formData: {
				id: 0,
				organizeId: undefined,
				organizeIdList: [],
				loginAccount: "",
				roleGroup: undefined,
				loginPassWord: "",
				postGroup: [],
				avatar: undefined,
				fullName: "",
				mobile: "",
				sex: "男",
				email: "",
				status: true,
				summary: "",
				loginCount: 0,
			},
			rules: {
				organizeId: [
					{
						required: true,
						message: "请选择所属部门",
						trigger: "change",
					},
				],
				loginAccount: [
					{
						required: true,
						message: "请输入登录账号",
						trigger: "blur",
					},
				],
				roleGroup: [
					{
						required: true,
						message: "请择选所属角色",
						trigger: "change",
					},
				],
				loginPassWord: [
					{
						required: true,
						message: "请输入登录密码",
						trigger: "blur",
					},
				],
				postGroup: [
					{
						required: true,
						message: "请选择所属岗位",
						trigger: "change",
					},
				],
				fullName: [
					{
						required: true,
						message: "请输入姓名",
						trigger: "blur",
					},
				],
				mobile: [
					{
						required: true,
						message: "请输入手机号码",
						trigger: "blur",
					},
				],
				sex: [
					{
						required: true,
						message: "性别不能为空",
						trigger: "change",
					},
				],
				email: [],
				summary: [],
			},
			organizeOptions: [],
			roleOptions: [],
			sexOptions: [
				{
					label: "男",
					value: "男",
				},
				{
					label: "女",
					value: "女",
				},
			],
			roleProps: { multiple: true, expandTrigger: "hover" },
			organizeProps: {
				multiple: false,
				label: "label",
				value: "value",
				children: "children",
				checkStrictly: true,
				expandTrigger: "hover",
			},
			isUpHeadpic: false,
			headPicfileList: [],
		};
	},
	mounted() {
		this.init();
	},
	methods: {
		upSuccess(res) {
			this.formData.avatar = res.data.path;
			if (res.code == 200) {
				this.$message.success("上传成功~");
			} else {
				this.$message.warning(res.message);
			}
		},
		async init() {
			const org = await this.$API.sysorganize.list.get();
			let orgArr = [];
			org.data.forEach(function (m) {
				orgArr.push({
					id: m.id,
					value: m.id,
					label: m.name,
					parentId: m.parentId,
				});
			});
			this.organizeOptions = this.$TOOL.changeTree(orgArr);
			const role = await this.$API.sysrole.list.get();
			let roleArr = [];
			role.data.forEach(function (m) {
				roleArr.push({
					id: m.id,
					value: m.id,
					label: m.name,
					parentId: m.parentId,
				});
			});
			this.roleOptions = this.$TOOL.changeTree(roleArr);
		},
		async open(row) {
			if (!row) {
				this.mode = "add";
			} else {
				this.mode = "edit";
				var res = await this.$API.sysadmin.model.get(row.id);
				res.data.avatar = this.$CONFIG.SERVER_URL + res.data.avatar;
				this.formData = res.data;
			}
			this.visible = true;
		},
		save() {
			this.$refs.formRef.validate(async (valid) => {
				if (valid) {
					if (this.formData.avatar) {
						this.formData.avatar = this.formData.avatar.replace(
							this.$CONFIG.SERVER_URL,
							""
						);
					}
					this.formData.loginPassWord = this.$TOOL.crypto.MD5(
						this.formData.loginPassWord
					);
					this.isSaveing = true;
					let res = null;
					if (this.formData.id === 0) {
						res = await this.$API.sysadmin.add.post(this.formData);
					} else {
						res = await this.$API.sysadmin.update.put(
							this.formData
						);
					}
					this.isSaveing = false;
					if (res.code == 200) {
						this.$emit("complete");
						this.visible = false;
						this.$message.success("操作成功");
					} else {
						this.formData.loginPassWord = undefined;
						this.$alert(res.message, "提示", { type: "error" });
					}
				}
			});
		},
		close() {
			this.formData = {
				id: 0,
				organizeId: undefined,
				roleGroup: undefined,
				loginAccount: "",
				loginPassWord: "",
				postGroup: [],
				fullName: "",
				mobile: "",
				sex: "男",
				avatar: undefined,
				email: "",
				status: true,
				summary: "",
				loginCount: 0,
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
