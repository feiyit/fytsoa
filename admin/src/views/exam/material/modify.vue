<template>
	<sc-dialog
		v-model="visible"
		show-fullscreen
		:title="titleMap[mode]"
		width="850px"
		class="upload-dialog"
		@close="close"
	>
		<el-alert
			title="注：上传视频过程中，请不要关闭此页面，如果做其它操作，请打开浏览器新的窗口操作。"
			type="warning"
			description="上传非MP4文件，需要到视频转换功能，将其他格式视频，转换为MP4格式"
			:closable="false"
		/>
		<el-header>
			<el-tree-select
				ref="treeSelectParent"
				v-model="formData.categoryId"
				:data="treedata"
				:default-expand-all="true"
				:highlight-current="true"
				@node-click="nodeClick"
				filterable
			/>
			<el-upload
				ref="upload"
				v-model="fileList"
				:action="uploadUrl"
				:multiple="true"
				:limit="10"
				:data="uploadData"
				:show-file-list="false"
				:auto-upload="false"
				:on-change="uploadChange"
				:on-progress="uploadProgress"
				:on-remove="uploadRemove"
				:on-error="uploadError"
				:on-success="uploadSuccess"
			>
				<el-button type="primary" round icon="el-icon-finished"
					>选择文件（最多10个文件）</el-button
				>
			</el-upload>
		</el-header>
		<el-main>
			<el-table :data="tableData" border style="width: 100%">
				<el-table-column prop="name" label="文件名" />
				<el-table-column prop="size" label="大小" width="120">
					<template #default="scope">
						{{ $TOOL.fileSize(scope.row.size) }}
					</template>
				</el-table-column>
				<el-table-column prop="status" label="状态" width="100" />
				<el-table-column
					fixed="right"
					label="操作"
					align="center"
					width="120"
				>
					<template #default="scope">
						<el-button
							type="danger"
							circle
							icon="el-icon-delete"
							@click="handleDelete(scope.$index)"
						/>
					</template>
				</el-table-column>
			</el-table>
			<el-progress class="file-progress" :percentage="percentage" />
			<el-card class="box-card" shadow="never">
				<template #header>
					<div class="card-header">
						<span class="f15">上传日志</span>
						<el-button type="danger" text @click="clearlog"
							>清空日志</el-button
						>
					</div>
				</template>
				<div
					v-for="o in log"
					:key="o"
					class="text item"
					:class="o.class"
				>
					{{ o.msg }}
				</div>
			</el-card>
		</el-main>
		<template #footer>
			<el-button @click="close">关 闭</el-button>
			<el-button
				:loading="isSaveing"
				type="danger"
				icon="el-icon-upload"
				@click="startUpload"
			>
				开始上传，上传过程中请不要关闭此页面
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
				add: "上传文件",
				edit: "编辑",
			},
			uploadUrl: this.$API.exammaterial.upload.url,
			isSaveing: false,
			visible: false,
			formData: {
				id: 0,
				categoryId: "",
			},
			rules: {},
			uploadData: { categoryId: 0 },
			treedata: [],
			fileList: [],
			tableData: [],
			log: [],
			percentage: 0,
		};
	},
	mounted() {
		this.initCategory();
	},
	methods: {
		async initCategory() {
			const res = await this.$API.exammaterialcategory.list.get();
			if (res.code == 200) {
				let treeArr = [];
				res.data.forEach(function (m) {
					treeArr.push({
						id: m.id,
						value: m.id,
						label: m.name,
						parentId: m.parentId,
					});
				});
				this.treedata = this.$TOOL.changeTree(treeArr);
			}
		},
		async open(row, category) {
			this.formData.categoryId = category.id;
			this.uploadData.categoryId = category.id;
			this.visible = true;
		},
		nodeClick(node) {
			this.uploadData.categoryId = node.id;
		},
		uploadChange(file, files) {
			if (file.status == "ready") {
				this.tableData = files;
			}
		},
		uploadError(error, file) {
			this.log.push({
				msg: file.name + "上传失败，原因：" + error,
				class: "error",
			});
		},
		uploadSuccess(response, file) {
			if (response.code == 200) {
				this.log.push({
					msg: "文件：" + response.data.name + "上传成功~",
					class: "success",
				});
			} else {
				this.log.push({
					msg: "文件上传失败，原因：" + response.message,
					class: "error",
				});
				this.tableData.forEach((m) => {
					if (m.name == file.name) {
						m.status = "fail";
					}
				});
			}
		},
		uploadProgress(e) {
			this.percentage = Math.trunc(e.percent);
		},
		uploadRemove(file, files) {
			this.tableData = files;
		},
		handleDelete(index) {
			this.tableData.splice(index, 1);
		},
		startUpload() {
			if (this.tableData.length == 0) {
				this.$message.warning("请选择文件，在上传");
				return;
			}
			this.percentage = 0;
			this.$refs.upload.submit();
		},
		clearlog() {
			this.log = [];
			this.percentage = 0;
		},
		close() {
			this.clearlog();
			this.tableData = [];
			this.visible = false;
			this.$emit("complete");
		},
	},
};
</script>
<style scoped>
>>> .el-button {
	font-weight: normal;
}
.upload-dialog >>> .el-dialog__body {
	padding: 10px 20px;
}
.box-card {
	margin-top: 10px;
}
.box-card .item {
	padding: 5px;
	font-size: 12px;
}
.box-card .item.error {
	color: red;
}
.box-card .item.success {
	color: #6ac144;
}
.box-card .f15 {
	font-size: 15px;
	font-weight: normal;
}
.box-card .card-header {
	border-bottom: 1px solid #e6e7e8;
	padding: 0px 0px 10px 0px;
}
.card-header {
	display: flex;
	justify-content: space-between;
	align-items: center;
}
.file-progress {
	margin: 15px 0 5px 0;
}
</style>
