<template>
	<el-form
		ref="loginForm"
		:model="form"
		:rules="rules"
		label-width="0"
		size="large"
		@keyup.enter="login"
	>
		<el-form-item prop="account">
			<el-input
				v-model="form.account"
				prefix-icon="el-icon-user"
				clearable
				:placeholder="$t('login.userPlaceholder')"
			>
				<template #append>
					<el-select v-model="userType" style="width: 130px">
						<el-option
							:label="$t('login.admin')"
							value="admin"
						></el-option>
						<el-option
							:label="$t('login.user')"
							value="user"
						></el-option>
					</el-select>
				</template>
			</el-input>
		</el-form-item>
		<el-form-item prop="password">
			<el-input
				v-model="form.password"
				prefix-icon="el-icon-lock"
				clearable
				show-password
				:placeholder="$t('login.PWPlaceholder')"
			></el-input>
		</el-form-item>
		<el-form-item prop="code">
			<el-input
				v-model.trim="form.code"
				:placeholder="$t('login.userCode')"
				clearable
				prefix-icon="el-icon-set-up"
			>
			</el-input>
			<el-image class="login-code" :src="codeUrl" @click="changeCode" />
		</el-form-item>
		<el-form-item style="margin-bottom: 10px">
			<el-col :span="12">
				<el-checkbox
					:label="$t('login.rememberMe')"
					v-model="form.autologin"
				></el-checkbox>
			</el-col>
			<el-col :span="12" class="login-forgot">
				<router-link to="/reset_password"
					>{{ $t("login.forgetPassword") }}？</router-link
				>
			</el-col>
		</el-form-item>
		<el-form-item>
			<el-button
				type="primary"
				style="width: 100%"
				:loading="islogin"
				round
				@click="login"
				>{{ $t("login.signIn") }}</el-button
			>
		</el-form-item>
		<div class="login-reg">
			{{ $t("login.noAccount") }}
			<router-link to="/user_register">{{
				$t("login.createAccount")
			}}</router-link>
		</div>
	</el-form>
</template>

<script>
export default {
	data() {
		return {
			userType: "admin",
			codeUrl: "img/loading.gif",
			form: {
				account: "",
				password: "",
				code: undefined,
				codeKey: "",
				autologin: false,
			},
			rules: {
				account: [
					{
						required: true,
						message: this.$t("login.userError"),
						trigger: "blur",
					},
				],
				password: [
					{
						required: true,
						message: this.$t("login.PWError"),
						trigger: "blur",
					},
				],
				code: [
					{
						required: true,
						trigger: "blur",
						message: "验证码不能空",
					},
				],
			},
			islogin: false,
		};
	},
	watch: {
		userType(val) {
			if (val == "admin") {
				this.form.user = "admin";
				this.form.password = "";
			} else if (val == "user") {
				this.form.user = "user";
				this.form.password = "";
			}
		},
	},
	mounted() {
		this.form.codeKey = this.$TOOL.uuid();
		this.codeUrl = this.$CONFIG.API_URL + "/captcha/" + this.form.codeKey;
	},
	methods: {
		async login() {
			var validate = await this.$refs.loginForm
				.validate()
				.catch(() => {});
			if (!validate) {
				return false;
			}

			this.islogin = true;

			const data = {
				account: this.form.account,
				password: this.$TOOL.crypto.MD5(this.form.password),
				code: this.form.code,
				codeKey: this.form.codeKey,
			};
			var user = await this.$API.auth.token.post(data);
			if (user.code == 200) {
				this.$TOOL.data.set("TOKEN", user.data.accessToken);
				this.$TOOL.data.set("USER_INFO", user.data.userInfo);
				this.$TOOL.data.set("DASHBOARDGRID", [
					"welcome",
					"ver",
					"time",
					"progress",
					"echarts",
					"about",
				]);
			} else {
				this.islogin = false;
				this.$message.warning(user.message);
				return false;
			}
			//获取菜单
			var res = await this.$API.sysmenu.authority.get();
			if (res.code == 200) {
				console.log("res", res);
				if (res.data.menu.length == 0) {
					this.islogin = false;
					this.$alert(
						"当前用户无任何菜单权限，请联系系统管理员",
						"无权限访问",
						{
							type: "error",
							center: true,
						}
					);
					return false;
				}
				this.$TOOL.data.set("MENU", res.data.menu);
				this.$TOOL.data.set("PERMISSIONS", res.data.directive);
			} else {
				this.islogin = false;
				this.$message.warning(res.message);
				return false;
			}
			if (user.data.userInfo.tenantId == 0) {
				this.$router.replace({
					path: "/",
				});
			} else {
				this.$router.replace({
					path: "/tenant",
				});
			}
			this.$message.success("Login Success 登录成功");
			this.islogin = false;
		},
		changeCode() {
			this.codeUrl =
				this.$CONFIG.API_URL +
				"/captcha/" +
				this.form.codeKey +
				`?timestamp=${new Date().getTime()}`;
		},
	},
};
</script>

<style scoped>
.login-code {
	position: absolute;
	top: 4px;
	right: 4px;
	cursor: pointer;
	border-radius: 5px;
}
</style>
