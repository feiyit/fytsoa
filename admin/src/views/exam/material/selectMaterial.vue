<template>
	<sc-dialog
		v-model="visible"
		show-fullscreen
		:title="titleMap[mode]"
		width="1100px"
		top="7vh"
		class="cur-dialog"
		@close="close"
	>
		<el-header style="height: 1px; padding: 0px"></el-header>
		<el-container>
			<el-aside width="220px" v-loading="showGrouploading">
				<el-container>
					<el-header>
						<el-input
							placeholder="输入关键字进行过滤"
							v-model="categoryFilterText"
							clearable
						></el-input>
					</el-header>
					<el-main class="nopadding">
						<el-scrollbar height="510px">
							<el-tree
								ref="category"
								class="menu"
								node-key="id"
								default-expand-all
								:data="category"
								:filter-node-method="categoryFilterNode"
								@node-click="categoryClick"
							>
								<template #default="{ node, data }">
									<span class="custom-tree-node">
										<span class="label">{{
											node.label
										}}</span>
										<span class="code">{{
											data.code
										}}</span>
									</span>
								</template>
							</el-tree>
						</el-scrollbar>
					</el-main>
				</el-container>
			</el-aside>

			<el-container>
				<el-header>
					<div class="right-panel">
						<div class="right-panel-search">
							<el-input
								v-model="param.key"
								clearable
								placeholder="关键字"
							/>
							<el-button
								icon="el-icon-search"
								type="primary"
								@click="search"
							/>
						</div>
					</div>
				</el-header>
				<el-main class="nopadding">
					<scTable
						ref="table"
						:api-obj="apiObj"
						:column="column"
						row-key="id"
						height="500px"
						:hide-context-menu="false"
					>
						<el-table-column
							align="center"
							fixed="right"
							label="操作"
							width="100"
						>
							<template #default="scope">
								<el-button
									text
									type="primary"
									size="small"
									icon="el-icon-check"
									@click="select(scope.row)"
								>
									确定选择
								</el-button>
							</template>
						</el-table-column>
						<template #audit="{ data }">
							<el-tag
								disable-transitions
								:type="data.audit ? 'success' : 'danger'"
							>
								{{ data.audit ? "已审核" : "未审核" }}
							</el-tag>
						</template>
						<template #category="{ data }">
							{{ data.category.name }}
						</template>
						<template #size="{ data }">
							{{ $TOOL.fileSize(data.size) }}
						</template>
						<template #name="{ data }">
							<el-link type="success" @click="look(data)">{{
								data.name
							}}</el-link>
						</template>
					</scTable>
				</el-main>
			</el-container>
			<modify ref="modify" @complete="complete" />
			<materialtype ref="materialtype" @complete="initCategory" />
		</el-container>
	</sc-dialog>
</template>
<script>
export default {
	data() {
		return {
			mode: "add",
			titleMap: {
				add: "选择素材",
			},
			visible: false,
			apiObj: this.$API.exammaterial.page,
			list: [],
			param: {
				key: "",
			},
			paramType: 0,
			category: [],
			categoryFilterText: "",
			selectColumn: {},
			column: [
				{
					label: "id",
					prop: "id",
					width: "200",
					sortable: true,
					hide: true,
				},
				{ prop: "name", label: "名称", width: 200, align: "left" },
				{
					prop: "sourceName",
					label: "原名称",
					width: 200,
					align: "left",
				},
				{ prop: "category", label: "分类", width: 100 },
				{ prop: "ext", label: "扩展名", width: 100 },
				{ prop: "size", label: "文件大小", width: 100 },
				{ prop: "urls", label: "地址", width: 300, align: "left" },
				{
					prop: "type",
					label: "文件模式",
					width: 100,
				},
				{ prop: "audit", label: "审核", width: 100 },
				{ prop: "createTime", label: "创建时间", width: 180 },
			],
		};
	},
	watch: {
		categoryFilterText(val) {
			this.$refs.category.filter(val);
		},
	},
	mounted() {
		this.initCategory();
	},
	methods: {
		//加载树数据
		async initCategory() {
			this.showGrouploading = true;
			const res = await this.$API.exammaterialcategory.list.get();
			this.showGrouploading = false;
			let _tree = [{ id: "1", value: "0", label: "所有", parentId: "0" }];
			res.data.some((m) => {
				_tree.push({
					id: m.id,
					value: m.id,
					label: m.name,
					code: m.enName,
					parentId: m.parentId,
				});
			});
			this.category = this.$TOOL.changeTree(_tree);
		},
		//树过滤
		categoryFilterNode(value, data) {
			if (!value) return true;
			return data.label.indexOf(value) !== -1;
		},
		//树点击事件
		categoryClick(data) {
			if (data.id == 1) {
				this.$refs.table.reload();
				return;
			}
			var params = {
				id: data.id,
			};
			this.selectColumn = data;
			this.$refs.table.reload(params);
		},
		select(m) {
			this.$emit("complete", m, this.paramType);
			this.visible = false;
		},
		search() {
			this.$refs.table.upData(this.param);
		},
		open(type) {
			this.paramType = type;
			this.visible = true;
		},
		close() {
			this.visible = false;
		},
		look(row) {
			window.open(this.$CONFIG.SERVER_URL + row.urls);
		},
		getFileName(data) {
			return data.substring(0, data.indexOf("."));
		},
	},
};
</script>
<style scoped>
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
.custom-tree-node:hover .code {
	display: none;
}
.add-column {
	padding: 8px !important;
	margin: 8px;
}
.cur-dialog >>> .el-dialog__body {
	padding: 10px 20px;
}
</style>
