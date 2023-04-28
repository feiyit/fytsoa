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
							<el-form-item label="归属栏目" prop="columnArr">
								<el-cascader
									v-model="formData.columnArr"
									:key="cascaderKey"
									:options="columnIdOptions"
									:props="columnIdProps"
									:style="{ width: '100%' }"
									placeholder="请选择归属栏目"
									clearable
									@change="columnChange"
								></el-cascader>
							</el-form-item>
							<el-row>
								<el-col :span="22">
									<el-form-item label="文章标题" prop="title">
										<el-input
											v-model="formData.title"
											placeholder="请输入文章标题"
											:maxlength="50"
											show-word-limit
											clearable
											:style="{ width: '100%' }"
										></el-input>
									</el-form-item>
								</el-col>
								<el-col :span="2" style="padding-left: 10px">
									<el-color-picker
										v-model="formData.titleColor"
									></el-color-picker>
								</el-col>
							</el-row>
							<el-form-item label="关键词" prop="keyWord">
								<el-input
									v-model="formData.keyWord"
									type="textarea"
									placeholder="请输入SEO关键词"
									:maxlength="200"
									show-word-limit
									:autosize="{ minRows: 2, maxRows: 4 }"
									:style="{ width: '100%' }"
								></el-input>
							</el-form-item>

							<el-form-item label="描述" prop="summary">
								<el-input
									v-model="formData.summary"
									type="textarea"
									placeholder="请输入SEO描述"
									:maxlength="300"
									show-word-limit
									:autosize="{ minRows: 2, maxRows: 4 }"
									:style="{ width: '100%' }"
								></el-input>
							</el-form-item>
							<el-form-item label="外链地址" prop="linkUrl">
								<el-input
									v-model="formData.linkUrl"
									placeholder="请输入外链地址"
									:maxlength="255"
									show-word-limit
									clearable
									prefix-icon="el-icon-link"
									:style="{ width: '100%' }"
								></el-input>
							</el-form-item>
							<el-form-item label="文章标签" prop="tag">
								<el-select
									v-model="formData.tag"
									multiple
									filterable
									allow-create
									clearable
									:maxlength="100"
									placeholder="手动输入文章标签"
									:style="{ width: '100%' }"
								></el-select>
							</el-form-item>
							<el-form-item label="权重" prop="sort">
								<el-slider
									v-model="formData.sort"
									placeholder="请输入作者"
									clearable
									:max="500"
									:style="{ width: '100%' }"
									show-input
								></el-slider>
							</el-form-item>
							<el-row style="padding-left: 40px">
								<el-col :span="6">
									<el-form-item
										label="总点击量"
										label-width="80px"
										prop="hits"
									>
										<el-input
											v-model="formData.hits"
											placeholder="请输入总点击量"
											:style="{ width: '100%' }"
										></el-input>
									</el-form-item>
								</el-col>
								<el-col :span="6">
									<el-form-item
										label="日点击量"
										label-width="80px"
										prop="dayHits"
									>
										<el-input
											v-model="formData.dayHits"
											placeholder="日点击量"
											:style="{ width: '100%' }"
										></el-input>
									</el-form-item>
								</el-col>
								<el-col :span="6">
									<el-form-item
										label="周点击量"
										label-width="80px"
										prop="weedHits"
									>
										<el-input
											v-model="formData.weedHits"
											placeholder="周点击量"
											:style="{ width: '100%' }"
										></el-input>
									</el-form-item>
								</el-col>
								<el-col :span="6">
									<el-form-item
										label="月点击量"
										label-width="80px"
										prop="monthHits"
									>
										<el-input
											v-model="formData.monthHits"
											placeholder="月点击量"
											:style="{ width: '100%' }"
										></el-input>
									</el-form-item>
								</el-col>
							</el-row>
						</el-col>
						<el-col :span="10">
							<el-form-item label="发布时间" prop="updateTime">
								<el-date-picker
									v-model="formData.updateTime"
									type="datetime"
									:style="{ width: '100%' }"
									clearable
								></el-date-picker>
							</el-form-item>
							<el-form-item
								label="审核状态"
								prop="status"
								required
							>
								<el-switch
									v-model="formData.status"
								></el-switch>
							</el-form-item>
							<el-form-item label="上传" prop="imgUrl">
								<sc-upload
									v-model="formData.imgUrl"
									:apiObj="uploadApi"
									:width="220"
									:onSuccess="upSuccess"
								></sc-upload>
							</el-form-item>
							<el-form-item label="文章属性" prop="attr">
								<el-checkbox-group
									v-model="formData.attr"
									size="medium"
								>
									<el-checkbox
										v-for="(item, index) in attrOptions"
										:key="index"
										:label="item.value"
										:disabled="item.disabled"
									>
										{{ item.label }}
									</el-checkbox>
								</el-checkbox-group>
							</el-form-item>
							<el-form-item label="作者" prop="author">
								<el-input
									v-model="formData.author"
									placeholder="请输入作者"
									clearable
									:style="{ width: '100%' }"
								></el-input>
							</el-form-item>
							<el-form-item label="来源" prop="source">
								<el-input
									v-model="formData.source"
									placeholder="请输入来源"
									:maxlength="200"
									clearable
									:style="{ width: '100%' }"
								></el-input>
							</el-form-item>
						</el-col>
					</el-row>
				</el-tab-pane>
				<el-tab-pane label="文章内容" lazy>
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
			cascaderKey:0,
			formData: {
				id: 0,
				title: undefined,
				titleColor: undefined,
				columnId: 0,
				columnArr: [],
				subTitle: undefined,
				updateTime: new Date(),
				keyWord: undefined,
				summary: undefined,
				linkUrl: undefined,
				content: undefined,
				imgUrl: undefined,
				status: true,
				tag: [],
				attr: [],
				author: undefined,
				source: undefined,
				sort: 0,
				hits: 0,
				dayHits: 0,
				weedHits: 0,
				monthHits: 0,
			},
			rules: {
				title: [
					{
						required: true,
						message: "请输入文章标题",
						trigger: "blur",
					},
				],
				columnArr: [
					{
						required: true,
						type: "array",
						message: "请至少选择一个归属栏目",
						trigger: "change",
					},
				],
				subTitle: [],
				updateTime: [
					{
						required: true,
						message: "发布时间不能为空",
						trigger: "change",
					},
				],
				keyWord: [
					{
						required: true,
						message: "请输入SEO关键词",
						trigger: "blur",
					},
				],
				summary: [
					{
						required: true,
						message: "请输入SEO描述",
						trigger: "blur",
					},
				],
				linkUrl: [],
				tag: [
					{
						required: true,
						message: "请输入文章标签",
						trigger: "blur",
					},
				],
				attr: [],
				sort: [],
				author: [],
				source: [],
				hits: [],
				dayHits: [],
				weedHits: [],
				monthHits: [],
			},
			attrOptions: [
				{
					label: "是否推荐",
					value: 1,
				},
				{
					label: "是否热点",
					value: 2,
				},
				{
					label: "是否滚动",
					value: 3,
				},
				{
					label: "是否评论",
					value: 4,
				},
				{
					label: "是否回收站",
					value: 5,
				},
			],
			columnIdOptions: [],
			columnIdProps: {
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
	mounted() {
		this.initTree();
	},
	methods: {
		async initTree() {
			const t = await this.$API.cmscolumn.list.get();
			this.columnList = t.data;
			let _tree = [];
			t.data.some((m) => {
				_tree.push({
					id: m.id,
					value: m.id,
					label: m.title,
					parentId: m.parentId,
				});
			});
			this.columnIdOptions = this.$TOOL.changeTree(_tree);
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
				var res = await this.$API.cmsarticle.model.get(row.id);
				this.cascaderKey++;
				this.formData = res.data;
			}
			this.visible = true;
		},
		columnChange(e) {
			if (e) {
				this.formData.columnId = e.pop();
				this.formData.columnArr.push(e.pop());
			}
		},
		save() {
			this.$refs.formRef.validate(async (valid) => {
				if (valid) {
					this.isSaveing = true;
					let res = null;
					if (this.formData.id === 0) {
						res = await this.$API.cmsarticle.add.post(
							this.formData
						);
					} else {
						res = await this.$API.cmsarticle.update.put(
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
				title: undefined,
				titleColor: undefined,
				columnId: [],
				subTitle: undefined,
				updateTime: new Date(),
				keyWord: undefined,
				summary: undefined,
				linkUrl: undefined,
				status: true,
				tag: [],
				attr: [],
				author: undefined,
				source: undefined,
				content: undefined,
				imgUrl: undefined,
				sort: 0,
				hits: 0,
				dayHits: 0,
				weedHits: 0,
				monthHits: 0,
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
