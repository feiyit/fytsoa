<template>
	<el-container>
		<el-header>
			<div class="left-panel">
				<el-button
					icon="el-icon-plus"
					type="primary"
					v-auth="'tenanthome:add'"
					@click="open_dialog"
				/>
				<el-button
					icon="el-icon-delete"
					plain
					type="danger"
					v-auth="'tenanthome:delete'"
					:disabled="selection.length == 0"
					@click="batch_del"
				/>
				<el-button
					type="danger"
					plain
					v-auth="'tenanthome:reset'"
					:disabled="selection.length == 0"
					@click="pwdreset"
					>密码重置</el-button
				>
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
					width="220"
				>
					<template #default="scope">
						<el-button
							text
							type="primary"
							size="small"
							v-auth="'tenanthome:info'"
							@click="infoOpen(scope.row)"
						>
							详情
						</el-button>
						<el-divider
							v-auth="'tenanthome:info'"
							direction="vertical"
						/>
						<el-button
							text
							type="primary"
							size="small"
							:disabled="scope.row.tenantId == 0"
							v-auth="'tenanthome:edit'"
							@click="open_dialog(scope.row)"
						>
							编辑
						</el-button>
						<el-divider
							direction="vertical"
							v-auth="'tenanthome:delete'"
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
									v-auth="'tenanthome:delete'"
									:disabled="scope.row.tenantId == 0"
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
			</scTable>
		</el-main>
		<modify ref="modify" @complete="complete" />
		<info ref="info" />
	</el-container>
</template>
<script>
import { defineAsyncComponent } from "vue";
export default {
	components: {
		modify: defineAsyncComponent(() => import("./modify")),
		info: defineAsyncComponent(() => import("./info")),
	},
	data() {
		return {
			apiObj: this.$API.systenant.page,
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
				{ prop: "name", label: "租户名称", width: 200, align: "left" },
				{ prop: "person", label: "负责人", width: 100 },
				{ prop: "phone", label: "手机号码", width: 120 },
				{ prop: "account", label: "默认账号", width: 100 },
				{ prop: "status", label: "状态", width: 100 },
				{ prop: "createTime", label: "创建时间", width: 180 },
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
		//删除
		async table_del(row) {
			var res = await this.$API.systenant.delete.delete([row.id]);
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
					var res = await this.$API.systenant.delete.delete(ids);
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
		infoOpen(row) {
			this.$refs.info.open(row);
		},
		selectionChange(selection) {
			this.selection = selection;
		},
		pwdreset() {
			this.$confirm(
				`确定执行【密码重置】选中的 ${this.selection.length} 项吗？`,
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
						if (element.tenantId != 0) {
							ids.push(element.id);
						}
					});
					var res = await this.$API.systenant.resetPwd.put(ids);
					if (res.code == 200) {
						loading.close();
						this.$message.success("重置密码成功");
					} else {
						this.$alert(res.message, "提示", { type: "error" });
					}
				})
				.catch(() => {});
		},
		menuHandle(obj) {
			if (obj.command == "add") {
				this.open_dialog({});
			}
			if (obj.command == "edit") {
				this.open_dialog(obj.row);
			}
			if (obj.command == "delete") {
				if (obj.row.tenant == 0) {
					this.$message.error("系统租户不允许删除");
					return;
				}
				this.table_del(obj.row);
			}
		},
	},
};
</script>
