<template>
	<el-container>
		<el-header>
			<div class="left-panel">
				<el-button
					icon="el-icon-plus"
					type="primary"
					v-auth="'sysrole:add'"
					@click="open_dialog"
				/>
				<el-button
					icon="el-icon-delete"
					plain
					type="danger"
					:disabled="selection.length == 0"
					v-auth="'sysrole:delete'"
					@click="batch_del"
				/>
			</div>
			<div class="right-panel">
				<div class="right-panel-search">
					<el-input
						v-model="param.key"
						clearable
						placeholder="角色名称"
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
				hide-pagination
				is-tree
				row-key="id"
				@menu-handle="menuHandle"
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
							size="small"
							text
							type="primary"
							v-auth="'sysrole:edit'"
							@click="open_dialog(scope.row)"
						>
							编辑
						</el-button>
						<el-divider
							direction="vertical"
							v-auths-all="['sysrole:edit', 'sysrole:delete']"
						/>
						<el-popconfirm
							title="确定删除吗？"
							@confirm="table_del(scope.row, scope.$index)"
						>
							<template #reference>
								<el-button
									text
									:disabled="scope.row.isSystem"
									type="primary"
									size="small"
									v-auth="'sysrole:delete'"
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
				<template #isSystem="{ data }">
					<el-tag :type="data.isSystem ? 'success' : 'danger'">
						{{ data.isSystem ? "是" : "否" }}
					</el-tag>
				</template>
				<template #maxLength="{ data }">
					<el-tag type="info">
						{{ data.maxLength == 0 ? "不限制" : data.maxLength }}
					</el-tag>
				</template>
			</scTable>
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
			apiObj: this.$API.sysrole.list,
			list: [],
			param: {
				key: "",
			},
			selection: [],
			column: [
				{
					label: "id",
					prop: "id",
					width: "100",
					sortable: true,
					hide: true,
				},
				{
					label: "角色名称",
					prop: "name",
					align: "left",
				},
				{
					label: "是否超管",
					prop: "isSystem",
					width: "100",
					filters: [
						{ text: "是", value: "1" },
						{ text: "否", value: "0" },
					],
				},
				{
					label: "状态",
					prop: "status",
					width: "100",
					filters: [
						{ text: "正常", value: "1" },
						{ text: "异常", value: "0" },
					],
				},
				{
					label: "角色最大数",
					prop: "maxLength",
					width: "100",
				},
				{
					label: "创建时间",
					prop: "createTime",
					width: "180",
					sortable: true,
				},
			],
		};
	},
	mounted() {
		//this.apiObj = this.$API.sysrole.list;
	},
	methods: {
		complete() {
			this.$refs.table.refresh();
		},
		search() {
			this.$refs.table.upData(this.param);
		},
		//删除
		async table_del(row) {
			var res = await this.$API.sysrole.delete.delete(row.id);
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
				`确定删除选中的 ${this.selection.length} 项吗？如果删除项中含有子集将会被一并删除`,
				"提示",
				{
					type: "warning",
					confirmButtonText: "确定",
					cancelButtonText: "取消",
				}
			)
				.then(async () => {
					let ids = [];
					this.selection.forEach((element) => {
						if (!element.isSystem) {
							ids.push(element.id);
						}
					});
					if (ids.length == 0) {
						this.$message.warning("请选择非系统管理员的角色删除");
						return;
					}
					const loading = this.$loading();

					var res = await this.$API.sysrole.delete.delete(
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
