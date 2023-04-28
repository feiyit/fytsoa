<template>
	<sc-dialog
		class="column-dialog"
		v-model="visible"
		show-fullscreen
		:title="titleMap[mode]"
		width="850px"
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
					<el-form-item label="商品名称" prop="title">
						<el-input
							v-model="formData.title"
							placeholder="请输入商品名称"
							:maxlength="50"
							show-word-limit
							clearable
							:style="{ width: '100%' }"
						></el-input>
					</el-form-item>
					<el-row>
						<el-col :span="12"
							><el-form-item label="积分数" prop="point">
								<el-input
									v-model="formData.point"
									placeholder="请输入积分数"
									:maxlength="10"
									clearable
									:style="{ width: '100%' }"
								></el-input> </el-form-item
						></el-col>
						<el-col :span="12"
							><el-form-item label="原价" prop="price">
								<el-input
									v-model="formData.price"
									placeholder="请输入原价"
									:maxlength="10"
									clearable
									:style="{ width: '100%' }"
								></el-input> </el-form-item
						></el-col>
						<el-col :span="12">
							<el-form-item label="库存数" prop="stock">
								<el-input
									v-model="formData.stock"
									placeholder="请输入库存数"
									:maxlength="10"
									clearable
									:style="{ width: '100%' }"
								></el-input> </el-form-item
						></el-col>
						<el-col :span="12">
							<el-form-item label="限购数量" prop="limits">
								<el-input
									v-model="formData.limits"
									placeholder="请输入限购数量"
									:maxlength="10"
									clearable
									:style="{ width: '100%' }"
								></el-input> </el-form-item
						></el-col>
						<el-col :span="12"
							><el-form-item
								label="上架状态"
								prop="status"
								required
							>
								<el-switch
									v-model="formData.status"
								></el-switch> </el-form-item
						></el-col>
					</el-row>
					<el-divider>
						<el-icon><el-icon-sold-out /></el-icon> 商品规格
					</el-divider>
					<el-form-item label="商品规格" prop="prizes">
				<sc-form-table
					v-model="formData.specs"
					:addTemplate="addTemplate"
					drag-sort
					placeholder="暂无数据"
				>
					<el-table-column prop="time" label="规格名称（例如：品牌）" width="220">
						<template #default="scope">
							<el-input
								v-model="scope.row.name"
								placeholder="请输入内容"
							></el-input>
						</template>
					</el-table-column>
					<el-table-column prop="type" label="规格值（例如：李宁）" width="300">
						<template #default="scope">
							<el-input
								v-model="scope.row.value"
								placeholder="请输入内容"
							></el-input>
						</template>
					</el-table-column>
				</sc-form-table>
			</el-form-item>
				</el-tab-pane>
				<el-tab-pane label="商品图片" lazy>
					<sc-upload-multiple
						v-model="formData.imgUrls"
						:apiObj="uploadApi"
						:width="220"
					></sc-upload-multiple>
				</el-tab-pane>
				<el-tab-pane label="商品详情" lazy>
					<sc-editor
						v-model="formData.details"
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
				title: "",
				specs:[],
				point: undefined,
				price: undefined,
				limits:0,
				stock: undefined,
				status: true,
				details: undefined,
				imgUrls: undefined,
			},
			rules: {
				title: [
					{
						required: true,
						message: "请输入商品名称",
						trigger: "blur",
					},
				],
				point: [
					{
						required: true,
						message: "请输入积分数",
						trigger: "blur",
					},
				],
				price: [
					{
						required: true,
						message: "请输入原价",
						trigger: "blur",
					},
				],
				stock: [
					{
						required: true,
						message: "请输入库存数",
						trigger: "blur",
					},
				],
				limits:[
					{
						required: true,
						message: "请输入库存数",
						trigger: "blur",
					},
				]
			},
			uploadApi: this.$API.sysfile.shop,
			addTemplate: {
				name: "",
				value: "",
			},
		};
	},
	mounted() {},
	methods: {
		async open(row) {
			if (!row) {
				this.mode = "add";
			} else {
				this.mode = "edit";
				const that = this;
				let res = await this.$API.shoppointsgoods.model.get(row.id);
				if (res.data.imgUrls) {
					let imgs = res.data.imgUrls.split(",");
					let newImgs = [];
					imgs.forEach(function (m) {
						newImgs.push(that.$CONFIG.SERVER_URL + m);
					});
					res.data.imgUrls = newImgs.join(",");
				}

				this.formData = res.data;
			}
			this.visible = true;
		},
		save() {
			const that = this;
			this.$refs.formRef.validate(async (valid) => {
				if (valid) {
					if (this.formData.imgUrls) {
						let imgs = this.formData.imgUrls.split(",");
						let newImgs = [];
						imgs.forEach(function (m) {
							newImgs.push(
								m.replace(that.$CONFIG.SERVER_URL, "")
							);
						});
						this.formData.imgUrls = newImgs.join(",");
					}
					this.isSaveing = true;
					let res = null;
					if (this.formData.id === 0) {
						res = await this.$API.shoppointsgoods.add.post(
							this.formData
						);
					} else {
						res = await this.$API.shoppointsgoods.update.put(
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
				title: "",
				specs:[],
				point: undefined,
				price: undefined,
				limits:0,
				stock: undefined,
				status: false,
				details: undefined,
				imgUrls: undefined,
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
	background: #fff;
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
[data-theme="dark"] .tabs-pages > .el-tabs__header {
	background: #2b2b2b;
}
</style>
