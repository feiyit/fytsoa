<template>
	<el-main class="safety">
		<el-card shadow="never">
			<el-tabs @tab-click="tabClick">
				<el-tab-pane label="敏感信息设置">
					<el-alert
						title="敏感关键字（使用英文逗号分隔）"
						type="info"
						:closable="false"
						show-icon
					></el-alert>
					<el-input
						v-model="model.sensitive"
						type="textarea"
						:autosize="{ minRows: 8, maxRows: 15 }"
						placeholder="请输入内容"
					></el-input>
					<el-button
						type="primary"
						icon="el-icon-setting"
						style="margin-top: 10px"
						:loading="isSaveing"
						v-auth="'syssafety:save'"
						@click="saveSetting"
					>
						确定保存
					</el-button>
				</el-tab-pane>
				<el-tab-pane label="IP访问黑名单">
					<el-alert
						title="设置IP黑名单，不设置全部可访问，如设置IP后，设置的IP不可访问（注：设置IP方式为一行一个IP地址）"
						type="info"
						:closable="false"
						show-icon
					></el-alert>
					<el-input
						v-model="model.ipLimit"
						type="textarea"
						:autosize="{ minRows: 8, maxRows: 15 }"
						placeholder="设置IP方式为一行一个IP地址"
					></el-input>
					<el-button
						type="primary"
						icon="el-icon-setting"
						style="margin-top: 10px"
						:loading="isSaveing"
						v-auth="'syssafety:save'"
						@click="saveSetting"
					>
						确定保存
					</el-button>
				</el-tab-pane>
				<el-tab-pane label="上传文件白名单">
					<el-alert
						title="设置只允许上传文件的后缀名，格式为：”jpg|gif"
						type="info"
						:closable="false"
						show-icon
					></el-alert>
					<el-input
						v-model="model.uploadWhite"
						type="textarea"
						:autosize="{ minRows: 8, maxRows: 15 }"
						placeholder="格式为：”jpg|gig“，以竖线分隔"
					></el-input>
					<el-button
						type="primary"
						icon="el-icon-setting"
						style="margin-top: 10px"
						:loading="isSaveing"
						v-auth="'syssafety:save'"
						@click="saveSetting"
					>
						确定保存
					</el-button>
				</el-tab-pane>
			</el-tabs>
		</el-card>
	</el-main>
</template>
<script>
export default {
	data() {
		return {
			tabIndex: 0,
			isSaveing: false,
			model: {
				sensitive: undefined,
				ipLimit: undefined,
				uploadWhite: undefined,
			},
		};
	},
	mounted() {
		this.init();
	},
	methods: {
		async init() {
			var res = await this.$API.syssafety.model.get();
			if (res.code != 200) {
				this.$message.error(res.message);
				return;
			}
			this.model = res.data;
		},
		tabClick(tab) {
			this.tabIndex = tab.index;
		},
		async saveSetting() {
			this.isSaveing = true;
			var res = await this.$API.syssafety.save.post(this.model);
			if (res.code != 200) {
				this.$message.error(res.message);
				return;
			}
			this.isSaveing = false;
			this.$message.success("保存成功");
		},
	},
};
</script>
<style scoped>
.safety .el-textarea {
	margin-top: 10px;
}
</style>
