<template>
	<el-container>
		<el-header>
			<div class="left-panel">
				<el-button
					icon="el-icon-plus"
					type="primary"
					v-auth="'syscity:add'"
					@click="open_dialog"
				/>
				<el-button
					icon="el-icon-delete"
					plain
					type="danger"
					:disabled="selection.length == 0"
					v-auth="'syscity:delete'"
					@click="batch_del"
				/>
				<scTableFilter
					:column="column"
					@filterSubmit="filterOk"
				></scTableFilter>
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
				hide-pagination
				is-tree
				@menu-handle="menuHandle"
				@selection-change="selectionChange"
			>
				<!-- 固定列-选择列 -->
				<el-table-column fixed type="selection" width="60" />
				<el-table-column
					label="#"
					type="index"
					width="50"
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
							v-auth="'syscity:edit'"
							@click="open_dialog(scope.row)"
						>
							编辑
						</el-button>
						<el-divider
							direction="vertical"
							v-auths-all="['syscity:edit', 'syscity:delete']"
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
									v-auth="'syscity:delete'"
									>删除</el-button
								>
							</template>
						</el-popconfirm>
					</template>
				</el-table-column>
				<template #sort="{ data }">
					<el-link
						style="color: #5cd29d"
						:underline="false"
						@click="
							sort({
								parent: data.parentId,
								id: data.id,
								type: 0,
							})
						"
					>
						<el-icon><el-icon-top /></el-icon>
					</el-link>
					<el-divider direction="vertical" />
					<el-link
						style="color: #5cd29d"
						:underline="false"
						@click="
							sort({
								parent: data.parentId,
								id: data.id,
								type: 1,
							})
						"
					>
						<el-icon><el-icon-bottom /></el-icon>
					</el-link>
				</template>
			</scTable>
		</el-main>
		<modify ref="modify" @complete="complete" />
	</el-container>
</template>
<script>
import scTableFilter from "@/components/scTableFilter";
import { defineAsyncComponent } from "vue";
export default {
	components: {
		scTableFilter,
		modify: defineAsyncComponent(() => import("./modify")),
	},
	data() {
		return {
			apiObj: this.$API.syscity.list,
			list: [],
			param: {
				key: "",
			},
			selection: [],
			column: [
				{
					label: "id",
					prop: "id",
					width: "200",
					sortable: true,
					hide: true,
				},
				{ prop: "name", label: "城市名称", filter: true, type: "text" },
				{
					prop: "code",
					label: "城市编号",
					width: 100,
					filter: true,
					type: "text",
				},
				{
					prop: "sort",
					label: "排序",
					width: 100,
					filter: true,
					type: "select",
					extend: {
						request: () => {
							return [
								{
									label: "选项1",
									value: "1",
								},
								{
									label: "选项2",
									value: "2",
								},
							];
						},
					},
				},
				{
					prop: "layer",
					label: "深度",
					width: 100,
				},
				{
					prop: "longitude",
					label: "经度",
				},
				{ prop: "dimension", label: "维度" },
				{ prop: "addTime", label: "添加时间", width: 180 },
			],
		};
	},
	mounted() {},
	methods: {
		complete() {
			this.$refs.table.refresh();
		},
		search() {
			this.$refs.table.upData(this.param);
		},
		async sort(m) {
			await this.$API.syscity.sort.post(m);
			this.$refs.table.refresh();
		},
		//删除
		async table_del(row) {
			var res = await this.$API.syscity.delete.delete(row.id);
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
					var res = await this.$API.syscity.delete.delete(
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
		open_dialog(row) {
			if (row.id) {
				this.$refs.modify.open(row);
			} else {
				this.$refs.modify.open();
			}
		},
		selectionChange(selection) {
			this.selection = selection;
		},
		filterOk(data) {
			console.log("data", data);
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
