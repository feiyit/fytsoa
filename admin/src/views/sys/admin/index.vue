<template>
	<el-container>
		<el-aside width="220px" v-loading="showGrouploading">
			<el-container>
				<el-header>
					<el-input
						placeholder="输入关键字进行过滤"
						v-model="groupFilterText"
						clearable
					></el-input>
				</el-header>
				<el-main class="nopadding">
					<el-tree
						ref="group"
						class="menu"
						node-key="id"
						default-expand-all
						:data="group"
						:current-node-key="''"
						:highlight-current="true"
						:expand-on-click-node="false"
						:filter-node-method="groupFilterNode"
						@node-click="groupClick"
					></el-tree>
				</el-main>
			</el-container>
		</el-aside>
		<el-container>
			<el-header>
				<div class="left-panel">
					<el-button
						icon="el-icon-plus"
						type="primary"
						v-auth="'sysadmin:add'"
						@click="open_dialog"
					/>
					<el-button
						icon="el-icon-delete"
						plain
						type="danger"
						:disabled="selection.length == 0"
						v-auth="'sysadmin:delete'"
						@click="batch_del"
					/>
					<el-button
						type="primary"
						plain
						:disabled="selection.length == 0"
						v-auth="'sysadmin:role'"
						@click="permission"
						>分配角色</el-button
					>
					<el-button
						type="danger"
						plain
						:disabled="selection.length != 1"
						v-auth="'sysadmin:reset'"
						@click="pwdreset"
						>密码重置</el-button
					>
				</div>
				<div class="right-panel">
					<div class="right-panel-search">
						<el-input
							v-model="param.key"
							clearable
							placeholder="登录账号"
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
						width="140"
					>
						<template #default="scope">
							<el-button
								size="small"
								text
								type="primary"
								v-auth="'sysadmin:edit'"
								@click="open_dialog(scope.row)"
							>
								编辑
							</el-button>
							<el-divider
								direction="vertical"
								v-auths-all="[
									'sysadmin:edit',
									'sysadmin:delete',
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
										v-auth="'sysadmin:delete'"
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
					<template #avatar="{ data }">
						<el-avatar
							:src="$CONFIG.SERVER_URL + data.avatar"
							size="small"
						></el-avatar>
					</template>
					<template #OrganizeObj="{ data }">
						{{ data.organizeObj?.name }}
					</template>
				</scTable>
			</el-main>
			<modify ref="modify" @complete="complete" />
			<admin-role ref="adminRole" />
		</el-container>
	</el-container>
</template>
<script>
import { defineAsyncComponent } from "vue";
export default {
	components: {
		modify: defineAsyncComponent(() => import("./modify")),
		adminRole: defineAsyncComponent(() => import("./components/adminRole")),
	},
	data() {
		return {
			apiObj: this.$API.sysadmin.page,
			showGrouploading: false,
			groupFilterText: "",
			group: [],
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
				{
					label: "头像",
					prop: "avatar",
					width: "100",
				},
				{
					label: "姓名",
					prop: "fullName",
					width: "120",
					align: "left",
				},
				{
					label: "所属部门",
					prop: "OrganizeObj",
					align: "left",
					width: "200",
				},
				{
					label: "性别",
					prop: "sex",
					width: "80",
				},
				{
					label: "手机号码",
					prop: "mobile",
					width: "120",
				},
				{
					label: "状态",
					prop: "status",
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
			const res = await this.$API.sysrole.list.get();
			this.showGrouploading = false;
			let _tree = [{ id: "1", value: "0", label: "所有", parentId: "0" }];
			res.data.some((m) => {
				_tree.push({
					id: m.id,
					value: m.id,
					label: m.name,
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
			var params = {
				id: data.id,
			};
			this.$refs.table.reload(params);
		},

		complete() {
			this.$refs.table.refresh();
		},
		search() {
			this.$refs.table.upData(this.param);
		},
		pwdreset() {
			this.$confirm(
				`确定要重置 ${this.selection.length} 项的密码吗？`,
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
					var res = await this.$API.sysadmin.passreset.put(ids);
					if (res.code == 200) {
						this.$refs.table.refresh();
						loading.close();
						this.$message.success("重置成功");
					} else {
						this.$alert(res.message, "提示", { type: "error" });
					}
				})
				.catch(() => {});
		},
		//删除
		async table_del(row) {
			var res = await this.$API.sysadmin.delete.delete(row.id);
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
					var res = await this.$API.sysadmin.delete.delete(
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
		permission() {
			if (this.selection.length == 0) {
				this.$message.warning("请选择授权角色的用户");
				return;
			}
			let ids = [];
			this.selection.forEach((element) => {
				ids.push(element.id);
			});
			this.$refs.adminRole.open(ids);
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
