<template>
	<el-container>
		<el-header>
			<div class="left-panel">
				<el-button
					icon="el-icon-plus"
					type="primary"
					v-auth="'cmsarticle:add'"
					@click="open_dialog"
				/>
				<el-button
					icon="el-icon-delete"
					plain
					type="danger"
					:disabled="selection.length == 0"
					v-auth="'cmsarticle:delete'"
					@click="batch_del"
				/>
				<el-button
					type="danger"
					plain
					:disabled="selection.length == 0"
					v-auth="'cmsarticle:rdelete'"
					@click="batch_recycle"
					>删除到回收站</el-button
				>
				<el-button
					type="primary"
					plain
					:disabled="selection.length == 0"
					v-auth="'cmsarticle:recovery'"
					@click="batch_recovery"
					>回收站恢复</el-button
				>
				<scFilterBar
					filterName="filterName"
					:options="filterOptions"
					@filterChange="filterOk"
					style="margin-left: 10px"
				>
				</scFilterBar>
			</div>
			<div class="right-panel">
				<div class="right-panel-search">
					<el-input
						v-model="param.key"
						clearable
						placeholder="关键字"
						style="width: 200px"
					/>
					<el-cascader
						v-model="param.columnIds"
						:options="parentIdOptions"
						:props="parentProps"
						:style="{ width: '100%' }"
						placeholder="根据栏目搜索"
						clearable
						style="width: 200px"
						@change="columnChange"
					></el-cascader>
					<el-select
						v-model="param.status"
						clearable
						placeholder="请选择审核状态"
					>
						<el-option value="1" label="已审核">已审核</el-option>
						<el-option value="2" label="未审核">未审核</el-option>
					</el-select>
					<!-- <el-select
						v-model="param.attr"
						clearable
						placeholder="请选择属性"
					>
						<el-option value="1" label="推荐"></el-option>
						<el-option value="2" label="热点"></el-option>
						<el-option value="3" label="滚动"></el-option>
						<el-option value="4" label="评论"></el-option>
						<el-option value="5" label="回收站"></el-option>
					</el-select> -->
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
							v-auth="'cmsarticle:edit'"
							@click="open_dialog(scope.row)"
						>
							编辑
						</el-button>
						<el-divider
							direction="vertical"
							v-auths-all="[
								'cmsarticle:edit',
								'cmsarticle:delete',
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
									v-auth="'cmsarticle:delete'"
									>删除</el-button
								>
							</template>
						</el-popconfirm>
					</template>
				</el-table-column>
				<template #tag="{ data }">
					<span v-for="it in data.tag" :key="it">{{ it }}、</span>
				</template>
				<template #status="{ data }">
					<el-tag
						disable-transitions
						:type="data.status ? 'success' : 'danger'"
					>
						{{ data.status ? "正常" : "停用" }}
					</el-tag>
				</template>
				<template #title="{ data }">
					{{ "【" + data.columnName + "】-" + data.title }}
				</template>
				<template #attr="{ data }">
					<el-tag
						:type="data.attr.indexOf(1) > -1 ? 'success' : 'info'"
					>
						推荐
					</el-tag>
					<el-tag
						:type="data.attr.indexOf(2) > -1 ? 'success' : 'info'"
					>
						热点
					</el-tag>
					<el-tag
						:type="data.attr.indexOf(3) > -1 ? 'success' : 'info'"
					>
						滚动
					</el-tag>
					<el-tag
						:type="data.attr.indexOf(4) > -1 ? 'success' : 'info'"
					>
						评论
					</el-tag>
					<el-tag
						:type="data.attr.indexOf(5) > -1 ? 'success' : 'info'"
					>
						回收站
					</el-tag>
				</template>
			</scTable>
		</el-main>
		<modify ref="modify" @complete="complete" />
	</el-container>
</template>
<script>
import scFilterBar from "@/components/scFilterBar";
import { defineAsyncComponent } from "vue";
export default {
	components: {
		scFilterBar,
		modify: defineAsyncComponent(() => import("./modify")),
	},
	data() {
		return {
			apiObj: this.$API.cmsarticle.page,
			list: [],
			param: {
				key: undefined,
				id: 0,
				columnIds: [],
				attr: undefined,
				status: undefined,
				query: undefined,
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
					prop: "title",
					label: "文章标题",
					align: "left",
					width: 300,
					showOverflowTooltip: true,
				},
				{
					prop: "attr",
					label: "属性",
					width: 300,
					showOverflowTooltip: true,
				},
				{ prop: "sort", label: "权重", width: 80 },
				{
					prop: "tag",
					label: "标签",
					width: 120,
					showOverflowTooltip: true,
				},
				{ prop: "status", label: "发布状态", width: 100 },
				{ prop: "hits", label: "点击量", width: 100 },
				{
					prop: "createTime",
					width: 180,
				},
			],
			parentIdOptions: [],
			parentProps: {
				multiple: false,
				expandTrigger: "hover",
				label: "label",
				value: "value",
				children: "children",
			},
			filterOptions: [
				{
					label: "文章标题",
					value: "title",
					type: "text",
					placeholder: "请输入文章标题",
					operator: "1",
					operators: [
						{
							label: "包含",
							value: "1",
						},
						{
							label: "不包含",
							value: "13",
						},
					],
				},
				{
					label: "发布状态",
					value: "status",
					type: "switch",
					operator: "0",
					operators: [
						{
							label: "等于",
							value: "0",
						},
					],
				},
				{
					label: "文章栏目",
					value: "columnId",
					type: "treeSelect",
					placeholder: "请选择栏目",
					operator: "0",
					operators: [
						{
							label: "等于",
							value: "0",
						},
						{
							label: "不等于",
							value: "10",
						},
					],
					extend: {
						multiple: false,
						request: async () => {
							var list = await this.$API.cmscolumn.list.get();
							let _tree = [];
							list.data.some((m) => {
								_tree.push({
									id: m.id,
									value: m.id,
									label: m.title,
									parentId: m.parentId,
								});
							});
							return this.$TOOL.changeTree(_tree);
						},
					},
				},
				{
					label: "发布日期",
					value: "createTime",
					type: "date",
					operator: "2",
					operators: [
						{
							label: "等于",
							value: "2",
						},
						{
							label: "不等于",
							value: "13",
						},
					],
				},
			],
		};
	},
	mounted() {
		this.initTree();
	},
	methods: {
		async initTree() {
			const t = await this.$API.cmscolumn.list.get();
			let _tree = [];
			t.data.some((m) => {
				_tree.push({
					id: m.id,
					value: m.id,
					label: m.title,
					parentId: m.parentId,
				});
			});
			this.parentIdOptions = this.$TOOL.changeTree(_tree);
		},
		complete() {
			this.$refs.table.refresh();
		},
		search() {
			if (this.param.columnIds && this.param.columnIds.length > 0) {
				//this.param.id=this.param.columnIds.pop()
			} else {
				this.param.id = 0;
			}
			this.$refs.table.upData(this.param);
		},
		filterOk(data) {
			if (data) {
				this.param.query = JSON.stringify(data);
			}
			this.$refs.table.upData(this.param);
		},
		columnChange(e) {
			if (e) {
				this.param.id = e.pop();
			}
		},
		//删除
		async table_del(row) {
			var res = await this.$API.cmsarticle.delete.delete(row.id);
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
					var res = await this.$API.cmsarticle.delete.delete(
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
		batch_recycle() {
			this.$confirm(
				`确定将选中的 ${this.selection.length} 项删除到回收站吗？`,
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
					var res = await this.$API.cmsarticle.recycle.put(ids);
					if (res.code == 200) {
						this.$refs.table.refresh();
						loading.close();
						this.$message.success("操作成功");
					} else {
						this.$alert(res.message, "提示", { type: "error" });
					}
				})
				.catch(() => {});
		},
		batch_recovery() {
			this.$confirm(
				`确定将选中的 ${this.selection.length} 项在回收站中恢复吗？`,
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
					var res = await this.$API.cmsarticle.recovery.put(ids);
					if (res.code == 200) {
						this.$refs.table.refresh();
						loading.close();
						this.$message.success("操作成功");
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
