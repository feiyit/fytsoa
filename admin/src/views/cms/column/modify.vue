<template>
	<sc-dialog
		class="column-dialog"
		v-model="visible"
		show-fullscreen
		:title="titleMap[mode]"
		width="1100px"
		@close="close"
	>
		<el-form
			ref="formRef"
			label-width="100px"
			:model="formData"
			:rules="rules"
		>
			<el-tabs class="tabs-pages" stretch>
				<el-tab-pane label="基本信息" lazy>
					<el-row :gutter="20" style="margin: 0px">
						<el-col :span="14">
							<el-form-item label="栏目名称" prop="title">
								<el-input
									v-model="formData.title"
									placeholder="请输入栏目名称"
									:maxlength="30"
									show-word-limit
									clearable
									:style="{ width: '100%' }"
								></el-input>
							</el-form-item>
							<el-form-item label="英文标题" prop="enTitle">
								<el-input
									v-model="formData.enTitle"
									placeholder="请输入英文标题"
									:maxlength="50"
									show-word-limit
									clearable
									:style="{ width: '100%' }"
								></el-input>
							</el-form-item>
							<el-form-item label="栏目副标题" prop="subTitle">
								<el-input
									v-model="formData.subTitle"
									placeholder="请输入栏目副标题"
									:maxlength="50"
									show-word-limit
									clearable
									:style="{ width: '100%' }"
								></el-input>
							</el-form-item>
							<el-form-item label="Seo关键字" prop="keyWord">
								<el-input
									v-model="formData.keyWord"
									type="textarea"
									placeholder="请输入Seo关键字"
									:maxlength="100"
									:autosize="{ minRows: 2, maxRows: 3 }"
									:style="{ width: '100%' }"
								></el-input>
							</el-form-item>
							<el-form-item label="Seo描述" prop="summary">
								<el-input
									v-model="formData.summary"
									type="textarea"
									placeholder="请输入Seo描述"
									:maxlength="100"
									:autosize="{ minRows: 2, maxRows: 3 }"
									:style="{ width: '100%' }"
								></el-input>
							</el-form-item>
						</el-col>
						<el-col :span="10">
							<el-form-item label="所属上级" prop="parentIdList">
								<el-cascader
									v-model="formData.parentIdList"
									:options="parentIdOptions"
									:props="parentProps"
									:style="{ width: '100%' }"
									placeholder="请选择所属上级"
									clearable
								></el-cascader>
							</el-form-item>
							<el-form-item label="栏目模板" prop="templateId">
								<el-select
									v-model="formData.templateId"
									placeholder="请选择栏目模板"
									clearable
									:style="{ width: '100%' }"
								>
									<el-option
										v-for="(
											item, index
										) in templateIdOptions"
										:key="index"
										:label="item.label"
										:value="item.value"
										:disabled="item.disabled"
									></el-option>
								</el-select>
							</el-form-item>
							<el-form-item label="上传" prop="imgUrl">
								<sc-upload
									v-model="formData.imgUrl"
									:apiObj="uploadApi"
									:width="220"
									:onSuccess="upSuccess"
								></sc-upload>
							</el-form-item>
							<el-form-item label="栏目状态" prop="status">
								<el-switch
									v-model="formData.status"
								></el-switch>
							</el-form-item>
							<el-form-item label="外链地址" prop="linkUrl">
								<el-input
									v-model="formData.linkUrl"
									placeholder="请输入外链地址"
									clearable
									:style="{ width: '100%' }"
								>
									<template #prepend>Http://</template>
								</el-input>
							</el-form-item>
						</el-col>
					</el-row>
				</el-tab-pane>
				<el-tab-pane label="内容详情" lazy>
					<sc-editor
						v-model="formData.content"
						placeholder="请输入"
						:height="340"
					></sc-editor>
				</el-tab-pane>
			</el-tabs>
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
			isSaveing: false,
			visible: false,
			formData: {
				id: 0,
				parentIdList: [],
				templateId: undefined,
				imgUrl: null,
				status: true,
				linkUrl: undefined,
				title: undefined,
				subTitle: undefined,
				number: undefined,
				enTitle: undefined,
				keyWord: undefined,
				summary: undefined,
				content: "",
			},
			rules: {
				parentIdList: [
					{
						required: true,
						type: "array",
						message: "请至少选择一个所属上级",
						trigger: "change",
					},
				],
				templateId: [
					{
						required: true,
						message: "请选择栏目模板",
						trigger: "change",
					},
				],
				linkUrl: [],
				title: [
					{
						required: true,
						message: "请输入栏目名称",
						trigger: "blur",
					},
				],
				subTitle: [],
				number: [
					{
						required: true,
						message: "请输入栏目编号",
						trigger: "blur",
					},
				],
				enTitle: [],
				keyWord: [],
				summary: [],
				content: [],
			},
			parentIdOptions: [],
			templateIdOptions: [],
			parentProps: {
				multiple: false,
				checkStrictly: true,
				expandTrigger: "hover",
				label: "label",
				value: "value",
				children: "children",
			},
			uploadApi: this.$API.sysfile.artice,
		};
	},
	mounted() {},
	methods: {
		async initTree() {
			const t = await this.$API.cmscolumn.list.get();
			let _tree = [
				{ id: "1", value: "0", label: "根目录", parentId: "0" },
			];
			t.data.some((m) => {
				_tree.push({
					id: m.id,
					value: m.id,
					label: m.title,
					parentId: m.parentId,
				});
			});
			this.templateIdOptions = [];
			this.parentIdOptions = this.$TOOL.changeTree(_tree);
			const _temp = await this.$API.cmstemplate.page.get();
			this.templateIdOptions.push({ label: "请选择栏目模板", value: "" });
			_temp.data.items.some((m) => {
				this.templateIdOptions.push({ label: m.name, value: m.id });
			});
		},
		upSuccess(res) {
			this.formData.imgUrl = res.data.path;
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
				var res = await this.$API.cmscolumn.model.get(row.id);
				res.data.parentIdList.pop();
				if (res.data.parentIdList.length == 0)
					res.data.parentIdList = ["0"];
				this.formData = res.data;
			}
			this.initTree();
			this.visible = true;
		},
		save() {
			this.$refs.formRef.validate(async (valid) => {
				if (valid) {
					if (this.formData.imgUrl) {
						this.formData.imgUrl = this.formData.imgUrl.replace(
							this.$CONFIG.SERVER_URL,
							""
						);
					}
					this.isSaveing = true;
					let res = null;
					if (this.formData.id === 0) {
						res = await this.$API.cmscolumn.add.post(this.formData);
					} else {
						res = await this.$API.cmscolumn.update.put(
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
				parentIdList: [],
				templateId: undefined,
				imgUrl: null,
				status: true,
				linkUrl: undefined,
				title: undefined,
				subTitle: undefined,
				number: undefined,
				enTitle: undefined,
				keyWord: undefined,
				summary: undefined,
				content: "",
			};
			this.$refs.formRef.resetFields();
			this.visible = false;
		},
	},
};
</script>
<style>
.column-dialog .el-dialog__body {
	padding-top: 0px;
	padding-bottom: 0px;
}
.tabs-pages {
	display: flex;
	flex-flow: column;
	flex-shrink: 0;
	height: 100%;
}
.tabs-pages > .el-tabs__header {
	margin: 0;
}
.tabs-pages > .el-tabs__header .el-tabs__nav-wrap {
	display: flex;
	justify-content: center;
	margin-bottom: 20px;
}
.tabs-pages > .el-tabs__header .el-tabs__item {
	height: 60px;
	line-height: 60px;
	font-size: 14px;
}
.tabs-pages > .el-tabs__content {
	overflow-x: hidden;
	overflow: auto;
}
</style>
