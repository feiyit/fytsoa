<template>
	<sc-dialog
		v-model="visible"
		show-fullscreen
		:title="titleMap[mode]"
		width="1000px"
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
					<p class="last-login">上次登录：{{ formData.loginTime }}</p>
					<el-row>
						<el-col :span="8">
							<span>{{ formData.loginSum }}</span>
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
							<el-form-item label="会员组" prop="groupId">
								<el-select
									v-model="formData.groupId"
									placeholder="请选择会员组"
									clearable
									:style="{ width: '100%' }"
								>
									<el-option
										v-for="(item, index) in groupIdOptions"
										:key="index"
										:label="item.label"
										:value="item.value"
										:disabled="item.disabled"
									></el-option>
								</el-select>
							</el-form-item>
						</el-col>
						<el-col :span="12">
							<el-form-item label="姓名" prop="nickName">
								<el-input
									v-model="formData.nickName"
									placeholder="请输入姓名"
									:maxlength="50"
									show-word-limit
									clearable
									:style="{ width: '100%' }"
								></el-input>
							</el-form-item>
						</el-col>
						<el-col :span="12">
							<el-form-item label="登录账号" prop="loginName">
								<el-input
									v-model="formData.loginName"
									placeholder="请输入登录账号"
									:maxlength="30"
									show-word-limit
									clearable
									:style="{ width: '100%' }"
								></el-input>
							</el-form-item>
						</el-col>
						<el-col :span="12">
							<el-form-item label="登录密码" prop="loginPwd">
								<el-input
									v-model="formData.loginPwd"
									placeholder="请输入登录密码"
									:maxlength="20"
									show-word-limit
									clearable
									show-password
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
									show-word-limit
									clearable
									:style="{ width: '100%' }"
								></el-input>
							</el-form-item>
						</el-col>
						<el-col :span="12">
							<el-form-item label="电子邮箱" prop="email">
								<el-input
									v-model="formData.email"
									placeholder="请输入电子邮箱"
									:maxlength="50"
									show-word-limit
									clearable
									:style="{ width: '100%' }"
								></el-input>
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
							<el-form-item label="状态" prop="status" required>
								<el-switch
									v-model="formData.status"
								></el-switch>
							</el-form-item>
						</el-col>
					</el-row>
					<el-form-item label="个性签名" prop="autograph">
						<el-input
							v-model="formData.autograph"
							type="textarea"
							placeholder="请输入个性签名"
							:maxlength="200"
							:autosize="{ minRows: 3, maxRows: 4 }"
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
	emits: ["complete"],
	data() {
		return {
			mode: "add",
			titleMap: {
				add: "新增",
				edit: "编辑",
			},
			uploadApi: this.$API.sysfile.avatar,
			isSaveing: false,
			visible: false,
			formData: {
				id: 0,
				groupId: undefined,
				nickName: undefined,
				loginName: undefined,
				loginPwd: "",
				mobile: undefined,
				email: undefined,
				sex: "男",
				avatar: null,
				status: true,
				autograph: undefined,
				loginSum: 0,
				point: 0,
				money: 0,
			},
			rules: {
				groupId: [
					{
						required: true,
						message: "请选择会员组",
						trigger: "change",
					},
				],
				nickName: [
					{
						required: true,
						message: "请输入姓名",
						trigger: "blur",
					},
				],
				loginName: [
					{
						required: true,
						message: "请输入登录账号",
						trigger: "blur",
					},
				],
				loginPwd: [
					{
						required: true,
						message: "请输入登录密码",
						trigger: "blur",
					},
					{
						pattern:
							/(?=.*\d)(?=.*[a-zA-Z])(?=.*[^a-zA-Z0-9]).{6,20}/,
						message:
							"密码必须由数字、字母、特殊字符组合,请输入6-16位",
						trigger: "blur",
					},
				],
				mobile: [],
				email: [],
				sex: [
					{
						required: true,
						message: "性别不能为空",
						trigger: "change",
					},
				],
				autograph: [],
			},
			groupIdOptions: [],
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
		};
	},
	mounted() {
		this.init();
	},
	methods: {
		async init() {
			const t = await this.$API.membergroup.page.get({
				limit: 100,
				status: 1,
			});

			if (t.code == 200) {
				t.data.items.forEach((element) => {
					this.groupIdOptions.push({
						label: element.name,
						value: element.id,
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
				var res = await this.$API.member.model.get(row.id);
				if (!res.data.avatar && res.data.avatar.indexOf("http") == -1) {
					res.data.avatar = this.$CONFIG.SERVER_URL + res.data.avatar;
				}
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
						res = await this.$API.member.add.post(this.formData);
					} else {
						res = await this.$API.member.update.put(this.formData);
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
				groupId: undefined,
				nickName: undefined,
				loginName: undefined,
				loginPwd: "",
				mobile: undefined,
				email: undefined,
				sex: "男",
				avatar: null,
				status: true,
				autograph: undefined,
				loginSum: 0,
				point: 0,
				money: 0,
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
