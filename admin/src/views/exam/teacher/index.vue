<template>
	<el-container>
		<el-header>
			<div class="left-panel">
				<el-button
					icon="el-icon-plus"
					type="primary"
					v-auth="'examteacher:add'"
					@click="open_dialog"
				/>
				<el-button
					icon="el-icon-delete"
					plain
					type="danger"
					:disabled="selection.length == 0"
					v-auth="'examteacher:delete'"
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
							v-auth="'examteacher:edit'"
							@click="open_dialog(scope.row)"
						>
							编辑
						</el-button>
						<el-divider
							direction="vertical"
							v-auths-all="[
								'examteacher:edit',
								'examteacher:delete',
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
									v-auth="'examteacher:delete'"
									>删除</el-button
								>
							</template>
						</el-popconfirm>
					</template>
				</el-table-column>
				<template #professionCode="{ data }">
					{{ data.professionCode.name }}
				</template>
				<template #name="{ data }">
					<el-avatar class="user-avatar" :src="data.avatar" />{{
						data.name
					}}
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
			apiObj: this.$API.examteacher.page,
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
				{ prop: "name", label: "教师姓名", width: 200, align: "left" },
				{ prop: "professionCode", label: "专业", width: 100 },
				{ prop: "postName", label: "职称", width: 100 },
				{ prop: "focus", label: "关注数", width: 100 },
				{ prop: "age", label: "年龄", width: 100 },
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
			var res = await this.$API.examteacher.delete.delete([row.id]);
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
					var res = await this.$API.examteacher.delete.delete(ids);
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
<style lang="scss" scoped>
.user-avatar {
	vertical-align: middle;
	margin-right: 5px;
}
</style>
