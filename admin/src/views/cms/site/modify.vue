<template>
	<div>
		<el-form
			ref="formRef"
			label-width="100px"
			:model="formData"
			:rules="rules"
		>
			<el-tabs v-model="activeName">
				<el-tab-pane label="基本信息" name="first">
					<el-form-item label="网站名称" prop="name">
						<el-input
							v-model="formData.name"
							placeholder="请输入网站名称"
							:maxlength="32"
							show-word-limit
							clearable
						></el-input>
					</el-form-item>
					<el-form-item label="网站Logo" prop="logo">
						<sc-upload
							v-model="formData.logo"
							:apiObj="uploadApi"
							:width="148"
							:height="148"
							:onSuccess="upLogoSuccess"
						></sc-upload>
					</el-form-item>
					<el-form-item label="网站网址" prop="siteUrl">
						<el-input
							v-model="formData.siteUrl"
							placeholder="请输入网站网址"
							:maxlength="128"
							show-word-limit
							clearable
						></el-input>
					</el-form-item>
					<el-form-item label="SEO标题" prop="seoTitle">
						<el-input
							v-model="formData.seoTitle"
							placeholder="请输入SEO标题"
							:maxlength="128"
							show-word-limit
							clearable
						></el-input>
					</el-form-item>
					<el-form-item label="SEO关键字" prop="seoKey">
						<el-input
							v-model="formData.seoKey"
							placeholder="请输入SEO关键字"
							type="textarea"
							:maxlength="512"
							show-word-limit
							clearable
						></el-input>
					</el-form-item>
					<el-form-item label="SEO描述" prop="seoDescribe">
						<el-input
							v-model="formData.seoDescribe"
							placeholder="请输入SEO描述"
							type="textarea"
							:maxlength="512"
							show-word-limit
							clearable
						></el-input>
					</el-form-item>
					<el-form-item label="网站版权信息" prop="copyright">
						<el-input
							v-model="formData.copyright"
							placeholder="请输入网站版权信息"
							type="textarea"
							:maxlength="1024"
							show-word-limit
							clearable
						></el-input>
					</el-form-item>
					<el-form-item label="网站开启状态" prop="status">
						<el-switch v-model="formData.status"></el-switch>
					</el-form-item>
					<el-form-item label="网站关闭原因" prop="closeInfo">
						<el-input
							v-model="formData.closeInfo"
							placeholder="请输入网站关闭原因"
							type="textarea"
							:maxlength="1024"
							show-word-limit
							clearable
						></el-input>
					</el-form-item>
				</el-tab-pane>
				<el-tab-pane label="公司信息" name="second">
					<el-form-item label="公司电话" prop="companyTel">
						<el-input
							v-model="formData.companyTel"
							placeholder="请输入公司电话"
							:maxlength="32"
							show-word-limit
							clearable
						></el-input>
					</el-form-item>
					<el-form-item label="公司传真" prop="companyFax">
						<el-input
							v-model="formData.companyFax"
							placeholder="请输入公司传真"
							:maxlength="32"
							show-word-limit
							clearable
						></el-input>
					</el-form-item>
					<el-form-item label="公司邮箱" prop="companyEmail">
						<el-input
							v-model="formData.companyEmail"
							placeholder="请输入公司邮箱"
							:maxlength="32"
							show-word-limit
							clearable
						></el-input>
					</el-form-item>
					<el-form-item label="公司地址" prop="companyAddress">
						<el-input
							v-model="formData.companyAddress"
							placeholder="请输入公司地址"
							:maxlength="64"
							show-word-limit
							clearable
						></el-input>
					</el-form-item>
					<el-form-item label="客服信息" prop="customerService">
						<el-input
							v-model="formData.customerService"
							placeholder="请输入客服信息"
							:maxlength="128"
							show-word-limit
							clearable
						></el-input>
					</el-form-item>
				</el-tab-pane>
				<el-tab-pane label="二维码" name="third">
					<el-form-item label="二维码" prop="codes">
						<sc-upload
							v-model="formData.codes"
							:apiObj="uploadApi"
							:width="148"
							:height="148"
							:onSuccess="upCodeSuccess"
						></sc-upload>
					</el-form-item>
				</el-tab-pane>
			</el-tabs>
		</el-form>
		<div class="site-btn">
			<el-button @click="close">清 空</el-button>
			<el-button
				:loading="isSaveing"
				type="primary"
				v-auth="'cmssite:edit'"
				@click="save"
			>
				确 定
			</el-button>
		</div>
	</div>
</template>
<script>
export default {
	emits: ["complete"],
	data() {
		return {
			uploadApi: this.$API.sysfile.site,
			activeName: "first",
			isSaveing: false,
			formData: {
				id: 0,
				name: "",
				logo: "",
				siteUrl: "",
				seoTitle: "",
				seoKey: "",
				seoDescribe: "",
				copyright: "",
				status: false,
				closeInfo: "",
				companyTel: "",
				companyFax: "",
				companyEmail: "",
				companyAddress: "",
				customerService: "",
				codes: "",
			},
			rules: {
				name: [
					{
						required: true,
						message: "请输入站点名称",
						trigger: "blur",
					},
				],
			},
		};
	},
	mounted() {},
	methods: {
		async open(row) {
			if (!row) {
				this.close();
			} else {
				var res = await this.$API.cmssite.model.get(row.id);
				res.data.logo = this.$CONFIG.SERVER_URL + res.data.logo;
				res.data.code = this.$CONFIG.SERVER_URL + res.data.code;
				this.formData = res.data;
			}
		},
		save() {
			this.$refs.formRef.validate(async (valid) => {
				if (valid) {
					if (this.formData.logo) {
						this.formData.logo = this.formData.logo.replace(
							this.$CONFIG.SERVER_URL,
							""
						);
					}
					if (this.formData.code) {
						this.formData.code = this.formData.code.replace(
							this.$CONFIG.SERVER_URL,
							""
						);
					}

					this.isSaveing = true;
					let res = null;
					if (this.formData.id === 0) {
						res = await this.$API.cmssite.add.post(this.formData);
					} else {
						res = await this.$API.cmssite.update.put(this.formData);
					}
					this.isSaveing = false;
					if (res.code == 200) {
						this.$emit("complete");
						this.visible = false;
						this.formData.logo =
							this.$CONFIG.SERVER_URL + this.formData.logo;
						this.formData.code =
							this.$CONFIG.SERVER_URL + this.formData.code;
						this.$message.success("操作成功");
					} else {
						this.$alert(res.message, "提示", { type: "error" });
					}
				}
			});
		},
		upLogoSuccess(res) {
			this.formData.logo = res.data.path;
			if (res.code == 200) {
				this.$message.success("上传成功~");
			} else {
				this.$message.warning(res.message);
			}
		},
		upCodeSuccess(res) {
			this.formData.code = res.data.path;
			if (res.code == 200) {
				this.$message.success("上传成功~");
			} else {
				this.$message.warning(res.message);
			}
		},
		close() {
			this.formData = {
				id: 0,
				name: "",
				logo: "",
				siteUrl: "",
				seoTitle: "",
				seoKey: "",
				seoDescribe: "",
				copyright: "",
				status: false,
				closeInfo: "",
				companyTel: "",
				companyFax: "",
				companyEmail: "",
				companyAddress: "",
				customerService: "",
				codes: "",
			};
			this.$refs.formRef.resetFields();
			this.visible = false;
		},
	},
};
</script>
<style>
.site-btn {
	padding: 20px 100px;
}
</style>
