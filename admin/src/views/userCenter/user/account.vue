<template>
	<el-card shadow="never" header="个人信息">
		<el-form
			ref="form"
			:model="form"
			label-width="120px"
			style="margin-top: 20px"
		>
			<el-form-item label="账号">
				<el-input v-model="form.account" disabled></el-input>
				<div class="el-form-item-msg">
					账号信息用于登录，系统不允许修改
				</div>
			</el-form-item>
			<el-form-item label="姓名">
				<el-input v-model="form.fullName"></el-input>
			</el-form-item>
			<el-form-item label="性别">
				<el-select v-model="form.sex" placeholder="请选择">
					<el-option label="保密" value="保密"></el-option>
					<el-option label="男" value="男"></el-option>
					<el-option label="女" value="女"></el-option>
				</el-select>
			</el-form-item>
			<el-form-item label="个性签名">
				<el-input v-model="form.summary" type="textarea"></el-input>
			</el-form-item>
			<el-form-item>
				<el-button
					type="primary"
					v-auth="'userCenter:editaccount'"
					@click="saveBasic"
					>保存</el-button
				>
			</el-form-item>
		</el-form>
	</el-card>
</template>

<script>
export default {
	data() {
		return {
			form: {
				id: 0,
				account: "fytsoa@outlook.com",
				name: "FytSoa",
				headPic: "",
				sex: "男",
				summary: "正所谓富贵险中求",
				role: [],
				post: [],
			},
		};
	},
	mounted() {
		this.init();
	},
	methods: {
		async init() {
			const res = await this.$API.auth.user.get();
			this.form = res.data;
		},
		async saveBasic() {
			if (!this.form.fullName) {
				this.$alert("姓名不能为空！", "提示", { type: "error" });
				return;
			}
			const user = this.$TOOL.data.get("USER_INFO");
			this.form.id = user.id;
			const res = await this.$API.sysadmin.basic.put(this.form);
			if (res.code == 200) {
				this.$message.success("保存成功");
			} else {
				this.$alert(res.message, "提示", { type: "error" });
			}
		},
	},
};
</script>

<style></style>
