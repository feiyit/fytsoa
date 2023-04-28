<template>
	<el-container>
		<el-aside width="260px" v-loading="showGrouploading">
			<el-container>
				<el-header>
					<el-input
						placeholder="输入关键字进行过滤"
						v-model="groupFilterText"
						clearable
					></el-input>
					<el-button
						type="primary"
						round
						icon="el-icon-plus"
						class="add-column"
						v-auth="'modulesadv:add'"
						@click="edit"
					></el-button>
				</el-header>
				<el-main class="nopadding">
					<el-tree
						ref="group"
						class="menu"
						node-key="id"
						default-expand-all
						:data="group"
						:filter-node-method="groupFilterNode"
						@node-click="groupClick"
					>
						<template #default="{ node, data }">
							<span class="custom-tree-node">
								<span class="label">{{ node.label }}</span>
								<span class="code">{{ data.code }}</span>
								<span class="do">
									<el-icon
										@click.stop="edit(data)"
										v-auth="'modulesadv:edit'"
										><el-icon-edit
									/></el-icon>
									<el-icon
										@click.stop="remove(node, data)"
										v-auth="'modulesadv:delete'"
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
					<el-button
						icon="el-icon-plus"
						type="primary"
						:disabled="!selectColumn.id"
						v-auth="'modulesadv:addadv'"
						@click="open_dialog"
					/>
					<el-button
						icon="el-icon-delete"
						plain
						type="danger"
						:disabled="selection.length == 0"
						v-auth="'modulesadv:deleteadv'"
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
						label="#"
						type="index"
						width="60"
					></el-table-column>
					<el-table-column
						align="center"
						fixed="right"
						label="操作"
						width="140"
					>
						<template #default="scope">
							<el-button
								size="small"
								text
								type="primary"
								v-auth="'modulesadv:editadv'"
								@click="open_dialog(scope.row)"
							>
								编辑
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
										v-auth="'modulesadv:deleteadv'"
										>删除</el-button
									>
								</template>
							</el-popconfirm>
						</template>
					</el-table-column>
					<template #status="{ data }">
						<el-tag
							disable-transitions
							:type="data.status ? 'success' : 'danger'"
						>
							{{ data.status ? "正常" : "停用" }}
						</el-tag>
					</template>
					<template #imgUrl="{ data }">
						<el-image
							v-if="data.imgUrl"
							style="width: 180px; height: 80px"
							:src="$CONFIG.SERVER_URL + data.imgUrl"
							fit="contain"
						/>
					</template>
				</scTable>
			</el-main>
			<modify ref="modify" @complete="complete" />
			<column ref="column" @complete="columnComplete" />
		</el-container>
	</el-container>
</template>
<script>
import { defineAsyncComponent } from "vue";
export default {
	components: {
		modify: defineAsyncComponent(() => import("./modify")),
		column: defineAsyncComponent(() => import("./column")),
	},
	data() {
		return {
			serverApi: undefined,
			apiObj: this.$API.sysadvinfo.page,
			list: [],
			showGrouploading: false,
			groupFilterText: "",
			group: [],
			param: {
				key: "",
			},
			selectColumn: {},
			selection: [],
			column: [
				{
					label: "id",
					prop: "id",
					width: "200",
					sortable: true,
					hide: true,
				},
				{
					prop: "title",
					label: "广告名称",
					align: "left",
					width: 300,
				},
				{ prop: "imgUrl", label: "广告图", width: 200 },
				{
					prop: "linkUrl",
					label: "链接地址",
					width: 200,
					showOverflowTooltip: true,
				},
				{ prop: "sort", label: "权重", width: 100 },
				{ prop: "status", label: "状态", width: 100 },
				{ prop: "createTime", label: "创建时间", width: 180 },
			],
		};
	},
	watch: {
		groupFilterText(val) {
			this.$refs.group.filter(val);
		},
	},
	mounted() {
		this.getGroup();
	},
	methods: {
		//加载树数据
		async getGroup() {
			this.showGrouploading = true;
			const res = await this.$API.sysadvcolumn.list.get();
			this.showGrouploading = false;
			let _tree = [{ id: "1", value: "0", label: "所有", parentId: "0" }];
			res.data.some((m) => {
				_tree.push({
					id: m.id,
					value: m.id,
					label: m.name,
					code: m.code,
					parentId: m.parentId,
				});
			});
			this.group = this.$TOOL.changeTree(_tree);
		},
		//树过滤
		groupFilterNode(value, data) {
			if (!value) return true;
			return data.label.indexOf(value) !== -1;
		},
		//树点击事件
		groupClick(data) {
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
		columnComplete() {
			this.getGroup();
		},
		search() {
			this.$refs.table.upData(this.param);
		},
		//删除
		async table_del(row) {
			var res = await this.$API.sysadvinfo.delete.delete(row.id);
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
					var res = await this.$API.sysadvinfo.delete.delete(
						ids.join(",")
					);
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
		edit(row) {
			if (row.id) {
				this.$refs.column.open(row);
			} else {
				this.$refs.column.open();
			}
		},
		remove(node, data) {
			this.$confirm(`确定要删除选中的 ${data.label} 项吗？`, "提示", {
				type: "warning",
				confirmButtonText: "确定",
				cancelButtonText: "取消",
			})
				.then(async () => {
					const loading = this.$loading();
					var res = await this.$API.sysadvcolumn.delete.delete(
						data.id
					);
					if (res.code == 200) {
						this.getGroup();
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
				this.$refs.modify.open(row);
			} else {
				if (this.selectColumn) {
					this.$refs.modify.open(this.selectColumn, "add");
					return;
				}
				this.$refs.modify.open({}, "add");
			}
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
	},
};
</script>
<style>
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
