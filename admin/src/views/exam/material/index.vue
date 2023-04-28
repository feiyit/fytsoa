<template>
	<el-container>
		<el-aside width="260px" v-loading="showGrouploading">
			<el-container>
				<el-header>
					<el-input
						placeholder="输入关键字进行过滤"
						v-model="categoryFilterText"
						clearable
					></el-input>
					<el-button
						type="primary"
						round
						icon="el-icon-plus"
						class="add-column"
						v-auth="'exammaterialcategory:add'"
						@click="categoryEdit"
					></el-button>
				</el-header>
				<el-main class="nopadding">
					<el-scrollbar>
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
									<span class="label">{{ node.label }}</span>
									<span class="code">{{ data.code }}</span>
									<span class="do">
										<el-icon
											@click.stop="categoryEdit(data)"
											v-auth="'exammaterialcategory:edit'"
											><el-icon-edit
										/></el-icon>
										<el-icon
											v-auth="
												'exammaterialcategory:delete'
											"
											@click.stop="
												categoryRemove(node, data)
											"
											><el-icon-delete
										/></el-icon>
									</span>
								</span>
							</template>
						</el-tree>
					</el-scrollbar>
				</el-main>
			</el-container>
		</el-aside>

		<el-container>
			<el-header>
				<div class="left-panel">
					<el-button
						icon="el-icon-plus"
						type="primary"
						:disabled="!selectColumn.id"
						v-auth="'exammaterial:add'"
						@click="open_dialog"
					/>
					<el-button
						icon="el-icon-delete"
						plain
						type="danger"
						:disabled="selection.length == 0"
						v-auth="'exammaterial:delete'"
						@click="batch_del"
					/>
				</div>
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
					@menu-handle="menuHandle"
					@selection-change="selectionChange"
				>
					<!-- 固定列-选择列 -->
					<el-table-column
						fixed
						type="selection"
						width="60"
						align="center"
					/>
					<el-table-column
						align="center"
						fixed="right"
						label="操作"
						width="220"
					>
						<template #default="scope">
							<el-button
								text
								type="primary"
								size="small"
								@click="look(scope.row)"
							>
								查看
							</el-button>
							<el-divider direction="vertical" />
							<el-button
								text
								type="primary"
								size="small"
								@click="rename(scope.row)"
							>
								重命名
							</el-button>
							<el-divider direction="vertical" />
							<el-popconfirm
								title="确定删除吗？"
								@confirm="table_del(scope.row, scope.$index)"
							>
								<template #reference>
									<el-button
										text
										type="primary"
										size="small"
										v-auth="'exammaterial:delete'"
										>删除</el-button
									>
								</template>
							</el-popconfirm>
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
				</scTable>
			</el-main>
		</el-container>
		<modify ref="modify" @complete="complete" />
		<materialtype ref="materialtype" @complete="initCategory" />
	</el-container>
</template>
<script>
import { defineAsyncComponent } from "vue";
import { ElMessageBox } from "element-plus";
export default {
	components: {
		modify: defineAsyncComponent(() => import("./modify")),
		materialtype: defineAsyncComponent(() => import("./category/modify")),
	},
	data() {
		return {
			apiObj: this.$API.exammaterial.page,
			list: [],
			param: {
				key: "",
			},
			category: [],
			categoryFilterText: "",
			selection: [],
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
		complete() {
			this.$refs.table.refresh();
		},
		search() {
			this.$refs.table.upData(this.param);
		},
		//删除
		async table_del(row) {
			var res = await this.$API.exammaterial.delete.delete([row.id]);
			if (res.code == 200) {
				this.$refs.table.refresh();
				this.$message.success("删除成功");
			} else {
				this.$alert(res.message, "提示", { type: "error" });
			}
		},
		//批量删除
		batch_del() {
			this.$confirm(
				`确定删除选中的 ${this.selection.length} 项吗？`,
				"提示",
				{
					type: "warning",
					confirmButtonText: "确定",
					cancelButtonText: "取消",
				}
			)
				.then(async () => {
					const loading = this.$loading();
					let ids = [];
					this.selection.forEach((element) => {
						ids.push(element.id);
					});
					var res = await this.$API.exammaterial.delete.delete(ids);
					if (res.code == 200) {
						this.$refs.table.refresh();
						loading.close();
						this.$message.success("删除成功");
					} else {
						this.$alert(res.message, "提示", { type: "error" });
					}
				})
				.catch(() => {});
		},
		categoryEdit(row) {
			if (row.id) {
				this.$refs.materialtype.open(row);
			} else {
				this.$refs.materialtype.open();
			}
		},
		categoryRemove(node, data) {
			this.$confirm(`确定要删除选中的 ${data.label} 项吗？`, "提示", {
				type: "warning",
				confirmButtonText: "确定",
				cancelButtonText: "取消",
			})
				.then(async () => {
					const loading = this.$loading();
					var res =
						await this.$API.exammaterialcategory.delete.delete([
							data.id,
						]);
					if (res.code == 200) {
						this.initCategory();
						loading.close();
						this.$message.success("删除成功");
					} else {
						this.$alert(res.message, "提示", { type: "error" });
					}
				})
				.catch(() => {});
		},
		open_dialog(row) {
			if (row.id) {
				this.$refs.modify.open(row, this.selectColumn);
			} else {
				this.$refs.modify.open(null, this.selectColumn);
			}
		},
		look(row) {
			window.open(this.$CONFIG.SERVER_URL + row.urls);
		},
		rename(row) {
			ElMessageBox.prompt("请输入文件新名称，无需输入扩展名", "重命名", {
				confirmButtonText: "确认",
				cancelButtonText: "取消",
				inputValue: this.getFileName(row.name),
				inputPattern: /^[\u4e00-\u9fa5A-Za-z0-9\-\\-_]*$/,
				inputErrorMessage: "只允许输入中文、英文、数字",
			})
				.then(async ({ value }) => {
					var res = await this.$API.exammaterial.reName.put({
						id: row.id,
						name: value,
					});
					if (res.code == 200) {
						this.$message.success("文件重命名成功");
						this.$refs.table.refresh();
					} else {
						this.$alert(res.message, "提示", { type: "error" });
					}
				})
				.catch(() => {});
		},
		selectionChange(selection) {
			this.selection = selection;
		},
		menuHandle(obj) {
			if (obj.command == "add") {
				this.open_dialog({});
			}
			if (obj.command == "edit") {
				this.open_dialog(obj.row);
			}
			if (obj.command == "delete") {
				this.table_del(obj.row);
			}
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
.add-column {
	padding: 8px !important;
	margin: 8px;
}
</style>
