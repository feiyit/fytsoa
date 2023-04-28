<template>
	<el-container>
		<el-aside width="220px" v-loading="showGrouploading">
			<el-container>
				<el-header>
					<el-select
						v-model="param.types"
						placeholder="请选择"
						@change="typeChange"
					>
						<el-option
							v-for="item in options"
							:key="item.value"
							:label="item.label"
							:value="item.value"
						></el-option>
					</el-select>
				</el-header>
				<el-main class="nopadding">
					<el-tree
						ref="group"
						class="menu"
						node-key="id"
						default-expand-all
						:data="group"
						:props="props"
						:current-node-key="''"
						:highlight-current="true"
						:expand-on-click-node="false"
						@node-click="groupClick"
					>
						<template #default="{ node, data }">
							<span class="custom-tree-node">
								<span class="label">{{ node.label }}</span>
								<span class="do" v-if="data.routes">
									<el-icon
										@click.stop="remove_tree(node, data)"
										><el-icon-delete
									/></el-icon>
								</span>
							</span>
						</template>
					</el-tree>
				</el-main>
			</el-container>
		</el-aside>
		<el-container>
			<el-header>
				<div class="left-panel">
					<el-input v-model="param.path" :style="{ width: '360px' }">
						<template #prepend>
							<el-icon><el-icon-folder-opened /></el-icon>
						</template>
						<template #append>
							<el-button
								icon="el-icon-refresh"
								@click="refresh"
							/>
						</template>
					</el-input>
				</div>
				<div class="right-panel">
					<el-button
						icon="el-icon-copy-document"
						plain
						type="success"
						:disabled="!selectFile"
						v-copy="copyUrl"
						>复制地址</el-button
					>
					<el-button
						icon="el-icon-picture"
						plain
						type="success"
						:disabled="!selectFile"
						@click="lookImg"
						>查看原图</el-button
					>
					<el-button
						icon="el-icon-delete"
						plain
						type="danger"
						:disabled="!selectFile"
						v-auth="'sysfile:delete'"
						@click="file_del"
					/>
					<el-button
						icon="el-icon-download"
						plain
						type="primary"
						:disabled="!selectFile"
						@click="file_down"
					/>
					<el-button
						icon="el-icon-upload"
						plain
						type="primary"
						v-auth="'sysfile:upload'"
						@click="open_dialog"
						>本地上传</el-button
					>
				</div>
			</el-header>
			<el-main class="nopadding" style="padding: 10px">
				<el-scrollbar ref="scrollbar">
					<el-empty
						v-if="data.length == 0"
						description="无数据"
						:image-size="80"
					></el-empty>
					<div
						v-for="(item, index) in data"
						:key="index"
						class="sc-file-select__item"
						:class="{ active: value.includes(index + item.name) }"
						@click="select(index + item.name, item)"
					>
						<div class="sc-file-select__item__file">
							<div class="sc-file-select__item__select">
								<el-icon><el-icon-check /></el-icon>
							</div>
							<div class="sc-file-select__item__box"></div>
							<el-image
								v-if="_isImg(item.fileExt)"
								:src="serverApi + item.fileName"
								:preview-src-list="[serverApi + item.fileName]"
								fit="contain"
								lazy
							></el-image>
							<div v-else class="item-file item-file-doc">
								<el-icon class="el-icon--upload"
									><el-icon-document
								/></el-icon>
							</div>
						</div>
						<p :title="item.name">{{ item.name }}</p>
					</div>
				</el-scrollbar>
			</el-main>
			<el-image-viewer
				@close="closeImgViewer"
				:url-list="previewList"
				v-if="showImageViewer"
			/>
			<upload ref="upload" @complete="complete" />
		</el-container>
	</el-container>
</template>
<script>
import { defineAsyncComponent } from "vue";
import config from "@/config";
export default {
	components: {
		upload: defineAsyncComponent(() => import("./upload")),
	},
	data() {
		return {
			serverApi: undefined,
			showGrouploading: false,
			groupFilterText: "",
			group: [],
			props: {
				label: "path",
			},
			param: {
				types: "",
				path: "/upload/",
				key: "",
			},
			options: [
				{
					value: "",
					label: "所有文件",
				},
				{
					value: "file",
					label: "文件",
				},
				{
					value: "image",
					label: "图片",
				},
			],
			data: [],
			previewList: [],
			value: [],
			files: config.files,
			selectFile: undefined,
			copyUrl: undefined,
			showImageViewer: false,
		};
	},
	mounted() {
		this.serverApi = this.$CONFIG.SERVER_URL + "/";
		this.init();
		this.initFiles();
	},
	methods: {
		async init() {
			const res = await this.$API.sysfile.list.get();
			this.group = res.data;
		},
		async initFiles() {
			this.previewList = [];
			this.value = [];
			const res = await this.$API.sysfile.files.get({
				path: this.param.path,
				filetype: this.param.types,
			});
			if (res.code === 200) {
				this.data = res.data;
			} else {
				this.$alert("请求的地址不正确", "提示", { type: "error" });
			}
		},
		refresh() {
			this.value = [];
			this.initFiles();
		},
		complete() {
			this.init();
			this.initFiles();
		},
		open_dialog() {
			this.$refs.upload.open(this.param.path);
		},
		lookImg() {
			this.showImageViewer = true;
		},
		closeImgViewer() {
			this.showImageViewer = false;
		},
		select(item, row) {
			this.previewList = [];
			this.copyUrl = this.serverApi + row.fileName;
			this.previewList = [this.copyUrl];
			if (this.value.includes(item)) {
				this.value = "";
				this.selectFile = undefined;
			} else {
				this.value = item;
				this.selectFile = row;
			}
		},
		typeChange() {
			this.initFiles();
		},
		groupClick(data) {
			if (data.path != "根目录") {
				this.param.path = "/upload" + data.routes;
			} else {
				this.param.path = "/upload/";
			}
			this.selectFile = undefined;
			this.initFiles();
		},
		remove_tree(node, data) {
			this.$confirm(
				`确定删除 【${data.path}】 文件夹吗，如果删除将会删除当前文件夹及子目录和文件？`,
				"提示",
				{
					type: "warning",
					confirmButtonText: "确定",
					cancelButtonText: "取消",
				}
			)
				.then(async () => {
					const loading = this.$loading();
					var res = await this.$API.sysfile.delDirectory.delete(
						"upload" + data.routes
					);
					loading.close();
					if (res.code == 200) {
						this.param.path = "/upload/";
						this.init();
						this.initFiles();
						this.$message.success("删除成功");
					} else {
						this.$alert(res.message, "提示", { type: "error" });
					}
				})
				.catch(() => {});
		},
		file_del() {
			this.$confirm(
				`确定删除选中的 【${this.selectFile.name}】 吗？`,
				"提示",
				{
					type: "warning",
					confirmButtonText: "确定",
					cancelButtonText: "取消",
				}
			)
				.then(async () => {
					const loading = this.$loading();
					var res = await this.$API.sysfile.delFile.delete(
						this.selectFile.fileName
					);
					if (res.code == 200) {
						this.initFiles();
						loading.close();
						this.$message.success("删除成功");
					} else {
						this.$alert(res.message, "提示", { type: "error" });
					}
				})
				.catch(() => {});
		},
		file_down() {
			const fileData = this.serverApi + this.selectFile.fileName;
			const url = window.URL.createObjectURL(
				new Blob([fileData], {
					type: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet;charset=utf-8",
				})
			);
			const link = document.createElement("a");
			link.href = url;
			link.setAttribute("download", this.selectFile.name);
			document.body.appendChild(link);
			link.click();
			document.body.removeChild(link);
			window.URL.revokeObjectURL(url);
		},
		//内置函数
		_isImg(ext) {
			const imgExt = [".jpg", ".jpeg", ".png", ".gif", ".bmp"];
			return imgExt.indexOf(ext) != -1;
		},
		_getExt(fileUrl) {
			return fileUrl.substring(fileUrl.lastIndexOf(".") + 1);
		},
	},
};
</script>
<style scoped>
.sc-file-select {
	display: flex;
}
.sc-file-select__files {
	flex: 1;
}
.sc-file-select__item {
	padding: 10px;
}
.sc-file-select__list {
	height: 400px;
}
.sc-file-select__item {
	display: inline-block;
	float: left;
	margin: 0 15px 25px 0;
	width: 130px;
	cursor: pointer;
	background: #f9f9f9;
}
.sc-file-select__item__file {
	width: 110px;
	height: 110px;
	position: relative;
}
.sc-file-select__item__file .el-image {
	width: 110px;
	height: 110px;
}
.sc-file-select__item__box {
	position: absolute;
	top: 0;
	right: 0;
	bottom: 0;
	left: 0;
	border: 2px solid var(--el-color-success);
	z-index: 1;
	display: none;
}
.sc-file-select__item__box::before {
	content: "";
	position: absolute;
	top: 0;
	right: 0;
	bottom: 0;
	left: 0;
	background: var(--el-color-success);
	opacity: 0.2;
	display: none;
}
.sc-file-select__item:hover .sc-file-select__item__box {
	display: block;
}
.sc-file-select__item.active .sc-file-select__item__box {
	display: block;
}
.sc-file-select__item.active .sc-file-select__item__box::before {
	display: block;
}
.sc-file-select__item p {
	margin-top: 10px;
	white-space: nowrap;
	text-overflow: ellipsis;
	overflow: hidden;
	-webkit-text-overflow: ellipsis;
	text-align: center;
}
.sc-file-select__item__checkbox {
	position: absolute;
	width: 20px;
	height: 20px;
	top: 7px;
	right: 7px;
	z-index: 2;
	background: rgba(0, 0, 0, 0.2);
	border: 1px solid #fff;
	display: flex;
	flex-direction: column;
	align-items: center;
	justify-content: center;
}
.sc-file-select__item__checkbox i {
	font-size: 14px;
	color: #fff;
	font-weight: bold;
	display: none;
}
.sc-file-select__item__select {
	position: absolute;
	width: 20px;
	height: 20px;
	top: 0px;
	right: 0px;
	z-index: 2;
	background: var(--el-color-success);
	display: none;
	flex-direction: column;
	align-items: center;
	justify-content: center;
}
.sc-file-select__item__select i {
	font-size: 14px;
	color: #fff;
	font-weight: bold;
}
.sc-file-select__item.active .sc-file-select__item__checkbox {
	background: var(--el-color-success);
}
.sc-file-select__item.active .sc-file-select__item__checkbox i {
	display: block;
}
.sc-file-select__item.active .sc-file-select__item__select {
	display: flex;
}
.sc-file-select__item__file .item-file {
	width: 110px;
	height: 110px;
	display: flex;
	flex-direction: column;
	align-items: center;
	justify-content: center;
}
.sc-file-select__item__file .item-file i {
	font-size: 40px;
}
.sc-file-select__item__file .item-file.item-file-doc {
	color: #409eff;
}

.sc-file-select__item__upload {
	position: absolute;
	top: 0;
	right: 0;
	bottom: 0;
	left: 0;
	z-index: 1;
	background: rgba(255, 255, 255, 0.7);
	display: flex;
	flex-direction: column;
	align-items: center;
	justify-content: center;
}

.sc-file-select__side {
	width: 200px;
	margin-right: 15px;
	border-right: 1px solid rgba(128, 128, 128, 0.2);
	display: flex;
	flex-flow: column;
}
.sc-file-select__side-menu {
	flex: 1;
}
.sc-file-select__side-msg {
	height: 32px;
	line-height: 32px;
}

.sc-file-select__top {
	margin-bottom: 15px;
	display: flex;
	justify-content: space-between;
}
.sc-file-select__upload {
	display: inline-block;
}
.sc-file-select__top .tips {
	font-size: 12px;
	margin-left: 10px;
	color: #999;
}
.sc-file-select__top .tips i {
	font-size: 14px;
	margin-right: 5px;
	position: relative;
	bottom: -0.125em;
}
.sc-file-select__pagination {
	margin: 15px 0;
}

.sc-file-select__do {
	text-align: right;
}
.custom-tree-node {
	display: flex;
	flex: 1;
	align-items: center;
	justify-content: space-between;
	font-size: 14px;
	padding-right: 24px;
	height: 100%;
}
.custom-tree-node .code {
	font-size: 12px;
	color: #999;
}
.custom-tree-node .do {
	display: none;
}
.custom-tree-node .do i {
	margin-left: 5px;
	color: #999;
}
.custom-tree-node .do i:hover {
	color: #333;
}
.custom-tree-node:hover .code {
	display: none;
}
.custom-tree-node:hover .do {
	display: inline-block;
}
[data-theme="dark"] .sc-file-select__item {
	background: #383838;
}
</style>
