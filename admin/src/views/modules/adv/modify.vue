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
								v-model="formData.imgUrl"
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
					<el-row>
						<el-col :span="12">
							<el-form-item label="广告位名称" prop="title">
								<el-input
									v-model="formData.title"
									placeholder="请输入广告位名称"
									:maxlength="100"
									show-word-limit
									clearable
									:style="{ width: '100%' }"
								></el-input>
							</el-form-item>
						</el-col>
						<el-col :span="12">
							<el-form-item label="跳转方式" prop="target">
								<el-select
									v-model="formData.target"
									placeholder="请选择跳转方式"
									clearable
									:style="{ width: '100%' }"
								>
									<el-option
										v-for="(item, index) in targetOptions"
										:key="index"
										:label="item.label"
										:value="item.value"
										:disabled="item.disabled"
									></el-option>
								</el-select>
							</el-form-item>
						</el-col>
						<el-col :span="12">
							<el-form-item label="状态" prop="status" required>
								<el-switch
									v-model="formData.status"
								></el-switch>
							</el-form-item>
						</el-col>
						<el-col :span="12">
							<el-form-item
								label="时间限制"
								prop="isTimeLimit"
								required
							>
								<el-switch
									v-model="formData.isTimeLimit"
								></el-switch>
							</el-form-item>
						</el-col>
						<el-col v-show="formData.isTimeLimit" :span="12">
							<el-form-item label="开始时间" prop="beginTime">
								<el-date-picker
									v-model="formData.beginTime"
									format="yyyy-MM-dd"
									value-format="yyyy-MM-dd"
									:style="{ width: '100%' }"
									placeholder="请选择开始时间"
									clearable
								></el-date-picker>
							</el-form-item>
						</el-col>
						<el-col v-show="formData.isTimeLimit" :span="12">
							<el-form-item label="结束时间" prop="endTime">
								<el-date-picker
									v-model="formData.endTime"
									format="yyyy-MM-dd"
									value-format="yyyy-MM-dd"
									:style="{ width: '100%' }"
									placeholder="请选择结束时间"
									clearable
								></el-date-picker>
							</el-form-item>
						</el-col>
					</el-row>
					<el-form-item label="链接地址" prop="linkUrl">
						<el-input
							v-model="formData.linkUrl"
							placeholder="请输入链接地址"
							:maxlength="128"
							show-word-limit
							clearable
							:style="{ width: '100%' }"
						></el-input>
					</el-form-item>
					<el-form-item label="权重" prop="sort" required>
						<el-slider
							v-model="formData.sort"
							show-input
							:max="100"
							:step="1"
						></el-slider>
					</el-form-item>
					<el-form-item label="广告描述" prop="summary">
						<el-input
							v-model="formData.summary"
							type="textarea"
							placeholder="请输入广告描述"
							:maxlength="500"
							show-word-limit
							:autosize="{ minRows: 3, maxRows: 3 }"
							:style="{ width: '100%' }"
						></el-input>
					</el-form-item>
					<el-form-item label="广告代码" prop="codes">
						<el-input
							v-model="formData.codes"
							type="textarea"
							placeholder="请输入广告代码"
							:maxlength="500"
							show-word-limit
							:autosize="{ minRows: 3, maxRows: 3 }"
							:style="{ width: '100%' }"
						></el-input>
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
				columnId: 0,
				title: undefined,
				target: "_blank",
				status: true,
				isTimeLimit: false,
				beginTime: null,
				endTime: null,
				linkUrl: undefined,
				sort: 1,
				types: 1,
				hits: 0,
				imgUrl: null,
				summary: undefined,
				codes: undefined,
			},
			rules: {
				title: [
					{
						required: true,
						message: "请输入广告位名称",
						trigger: "blur",
					},
				],
				target: [
					{
						required: true,
						message: "请选择跳转方式",
						trigger: "change",
					},
				],
				beginTime: [],
				endTime: [],
				linkUrl: [
					{
						required: true,
						message: "请输入链接地址",
						trigger: "blur",
					},
				],
				summary: [],
				codes: [],
			},
			targetOptions: [
				{
					label: "新窗口",
					value: "_blank",
				},
				{
					label: "原窗口",
					value: "_self",
				},
			],
			imgUrlfileList: [],
			isUpHeadpic: false,
			uploadApi:this.$API.sysfile.adv,
		};
	},
	mounted() {
	},
	methods: {
		upSuccess(res) {
			this.formData.imgUrl=res.data.path
			if (res.code == 200) {
				this.$message.success("上传成功~");
			} else {
				this.$message.warning(res.message);
			}
		},
		async open(row, type = "edit") {
			if (type == "add") {
				this.mode = type;
				this.formData.columnId = row.id;
			} else {
				this.mode = type;
				var res = await this.$API.sysadvinfo.model.get(row.id);
				res.data.imgUrl=this.$CONFIG.SERVER_URL+res.data.imgUrl
				this.formData = res.data;
			}
			this.visible = true;
		},
		save() {
			this.$refs.formRef.validate(async (valid) => {
				if (valid) {
					if(this.formData.imgUrl){
						this.formData.imgUrl=this.formData.imgUrl.replace(this.$CONFIG.SERVER_URL,'')
					}
					this.isSaveing = true;
					let res = null;
					if (this.formData.id === 0) {
						res = await this.$API.sysadvinfo.add.post(
							this.formData
						);
					} else {
						res = await this.$API.sysadvinfo.update.put(
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
				columnId: 0,
				title: undefined,
				target: "_blank",
				status: true,
				isTimeLimit: false,
				beginTime: null,
				endTime: null,
				linkUrl: undefined,
				sort: 1,
				types: 1,
				hits: 0,
				imgUrl: null,
				summary: undefined,
				codes: undefined,
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
[data-theme="dark"] .user-else-info{
	background: #383838;
}
[data-theme="dark"] .select-img{
	border: 1px solid #6d6d6d;
}
[data-theme="dark"] .select-img .bg-gray{
	background: transparent;
}
</style>
