<template>
	<el-container>
		<el-aside width="300px">
			<el-header class="f14">年级</el-header>
			<div class="guade">
				<el-input
					v-model="filterText"
					placeholder="根据关键字搜索"
					clearable
				/>
			</div>
			<el-scrollbar class="tree-scroll">
				<el-tree
					ref="treeRef"
					class="filter-tree"
					:data="treedata"
					default-expand-all
					:filter-node-method="treeFilterNode"
					@node-click="treeClick"
				/>
			</el-scrollbar>
		</el-aside>
		<el-main class="bg nopadding">
			<el-header>
				<div class="left-panel">
					<el-button
						icon="el-icon-plus"
						type="primary"
						:disabled="!selectGrade.id"
						v-auth="'examknowledgecategory:add'"
						@click="open_dialog"
					/>
					<el-button
						icon="el-icon-delete"
						plain
						type="danger"
						:disabled="selection.length == 0 || !selectGrade.id"
						v-auth="'examknowledgecategory:delete'"
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
			<el-main class="nopadding" style="height: 81vh">
				<scTable
					ref="table"
					:api-obj="apiObj"
					:column="column"
					hide-pagination
					is-tree
					row-key="id"
					@selection-change="selectionChange"
				>
					<!-- 固定列-选择列 -->
					<el-table-column fixed type="selection" width="60" />
					<el-table-column
						align="center"
						fixed="right"
						label="操作"
						width="140"
					>
						<template #default="scope">
							<el-button
								text
								type="primary"
								size="small"
								v-auth="'examknowledgecategory:edit'"
								@click="open_dialog(scope.row)"
							>
								编辑
							</el-button>
							<el-divider
								direction="vertical"
								v-auths-all="[
									'examknowledgecategory:edit',
									'examknowledgecategory:delete',
								]"
							/>
							<el-popconfirm
								title="确定删除吗？"
								@confirm="table_del(scope.row, scope.$index)"
							>
								<template #reference>
									<el-button
										text
										type="primary"
										size="small"
										v-auth="'examknowledgecategory:delete'"
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
					<template #grandCode="{ data }">
						{{ data.grandCode.name }}
					</template>
				</scTable>
			</el-main>
		</el-main>

		<modify ref="modify" @complete="complete" />
	</el-container>
</template>
<script>
import { defineAsyncComponent } from "vue";
export default {
	components: {
		modify: defineAsyncComponent(() => import("./modify")),
	},
	data() {
		return {
			apiObj: this.$API.examknowledgecategory.list,
			list: [],
			param: {
				key: "",
			},
			selection: [],
			treedata: [],
			filterText: "",
			selectGrade: { id: "" },
			column: [
				{
					label: "id",
					prop: "id",
					width: "200",
					sortable: true,
					hide: true,
				},
				{ prop: "name", label: "分类名称", width: 200 },
				{ prop: "grandCode", label: "年级", width: 100 },
				{ prop: "sort", label: "排序", width: 80 },
				{ prop: "createTime", label: "创建时间", width: 180 },
			],
		};
	},
	watch: {
		filterText(val) {
			this.$refs.treeRef.filter(val);
		},
	},
	mounted() {
		this.initGrandOption();
	},
	methods: {
		async initGrandOption() {
			const res = await this.$API.syscode.list.get({ typeCode: "grand" });
			if (res.code == 200) {
				let treeArr = [];
				res.data.forEach(function (m) {
					treeArr.push({
						id: m.id,
						value: m.id,
						label: m.name,
						parentId: 0,
					});
				});
				this.treedata = this.$TOOL.changeTree(treeArr);
			}
		},
		treeFilterNode(value, data) {
			if (!value) return true;
			const targetText = data.label;
			return targetText.indexOf(value) !== -1;
		},
		treeClick(data) {
			if (data.id == 1) {
				this.$refs.table.reload();
				return;
			}
			var params = {
				id: data.id,
			};
			this.selectGrade = data;
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
			var res = await this.$API.examknowledgecategory.delete.delete([
				row.id,
			]);
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
					var res =
						await this.$API.examknowledgecategory.delete.delete(
							ids
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
		open_dialog(row) {
			if (row.id) {
				this.$refs.modify.open(row, this.selectGrade);
			} else {
				this.$refs.modify.open(null, this.selectGrade);
			}
		},
		selectionChange(selection) {
			this.selection = selection;
		},
	},
};
</script>
<style scoped>
.f14 {
	font-size: 14px;
}
.bg {
	background-color: #ffffff;
}
.guade {
	padding: 15px;
}
.filter-tree >>> .el-tree-node__content {
	height: 35px;
}
.tree-scroll {
	height: 74vh;
}
</style>
