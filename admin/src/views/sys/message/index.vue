<template>
	<el-container>
		<el-aside width="240px">
			<el-tree
				ref="category"
				class="menu"
				node-key="label"
				default-expand-all
				:data="category"
				:default-expanded-keys="['系统日志']"
				current-node-key="系统日志"
				:highlight-current="true"
				:expand-on-click-node="false"
				@node-click="typeClick"
			>
				<template #default="{ node, data }">
					<span class="custom-tree-node mess-tag">
						<span>
							<component
								class="mess-icon"
								:is="data.icon"
							></component>
							{{ node.label }}</span
						>
						<span v-if="data.sum > 0">
							<el-tag type="danger" effect="dark" size="mini">
								{{ data.sum }}
							</el-tag>
						</span>
					</span>
				</template>
			</el-tree>
		</el-aside>
		<el-container>
			<el-header>
				<div class="left-panel">
					<el-button
						icon="el-icon-plus"
						type="primary"
						v-auth="'sysmessage:add'"
						@click="open_dialog"
					/>
					<el-button
						icon="el-icon-delete"
						plain
						type="danger"
						:disabled="selection.length == 0"
						v-auth="'sysmessage:delete'"
						@click="batch_del"
					/>
					<el-button
						icon="el-icon-takeaway-box"
						plain
						type="danger"
						:disabled="selection.length == 0"
						v-auth="'sysmessage:recycle'"
						@click="recycle_del"
					/>
					<el-button
						icon="el-icon-circle-check"
						plain
						type="success"
						v-auth="'sysmessage:read'"
						@click="all_read"
						>全部已读</el-button
					>
					<el-button
						icon="el-icon-circle-check"
						plain
						type="success"
						:disabled="selection.length == 0"
						v-auth="'sysmessage:read'"
						@click="set_read"
						>设为已读</el-button
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
					:menu-default="['add']"
					@menu-handle="menuHandle"
					@selection-change="selectionChange"
				>
					<el-table-column fixed type="selection" width="60" />
					<template #isRead="{ data }">
						<el-tag
							disable-transitions
							:type="data.isRead ? 'success' : 'danger'"
						>
							{{ data.isRead ? "是" : "否" }}
						</el-tag>
					</template>
				</scTable>
			</el-main>
			<modify ref="modify" @complete="complete" />
		</el-container>
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
			apiObj: this.$API.sysmessage.page,
			list: [],
			param: {
				key: "",
			},
			selection: [],
			category: [
				{
					label: "消息",
					children: [
						{
							label: "所有消息",
							type: "level",
							value: "",
							sum: 0,
							icon: "el-icon-bell",
						},
						{
							label: "已读消息",
							type: "level",
							value: "1",
							sum: 0,
							icon: "el-icon-circle-check",
						},
						{
							label: "未读消息",
							type: "level",
							value: "2",
							sum: 0,
							icon: "el-icon-mute-notification",
						},
						{
							label: "回收站",
							type: "level",
							value: "3",
							sum: 0,
							icon: "el-icon-delete",
						},
					],
				},
				{
					label: "消息标签",
					children: [
						{
							label: "全部",
							type: "tag",
							value: "",
							sum: 0,
							icon: "el-icon-star",
						},
						{
							label: "工作",
							type: "tag",
							value: "工作",
							sum: 0,
							icon: "el-icon-star",
						},
						{
							label: "通知",
							type: "tag",
							value: "通知",
							sum: 0,
							icon: "el-icon-star",
						},
						{
							label: "需求",
							type: "tag",
							value: "需求",
							sum: 0,
							icon: "el-icon-star",
						},
						{
							label: "其它",
							type: "tag",
							value: "其它",
							sum: 0,
							icon: "el-icon-star",
						},
					],
				},
			],
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
					label: "留言标题",
					width: 200,
					align: "left",
					showOverflowTooltip: true,
				},
				{ prop: "typesName", label: "类型", width: 100 },
				{ prop: "email", label: "邮箱信息", width: 180 },
				{ prop: "mobile", label: "手机号码", width: 130 },
				{ prop: "tags", label: "留言标签", width: 200 },
				{
					prop: "summary",
					label: "留言内容",
					showOverflowTooltip: true,
				},
				{ prop: "isRead", label: "是否已读", width: 100 },
				{ prop: "userName", label: "用户姓名", width: 100 },
				{
					prop: "createTime",
					label: "添加时间",
					width: 180,
					fixed: "right",
				},
			],
		};
	},
	mounted() {
		this.initTotal();
	},
	methods: {
		async initTotal() {
			var res = await this.$API.sysmessage.total.get();
			this.category[0].children[0].sum = res.data.allCount;
			this.category[0].children[2].sum = res.data.unReadCount;
			this.category[0].children[3].sum = res.data.recycleCount;
		},
		typeClick(data) {
			console.log(data);
			if (data.type == "level") {
				this.$refs.table.upData({ status: data.value });
			}
			if (data.type == "tag") {
				this.$refs.table.upData({ key: data.value });
			}
		},
		complete() {
			this.$refs.table.refresh();
		},
		search() {
			this.$refs.table.upData(this.param);
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
					var res = await this.$API.sysmessage.delete.delete(
						ids.join(",")
					);
					loading.close();
					if (res.code == 200) {
						this.initTotal();
						this.$refs.table.refresh();
						this.$message.success("删除成功");
					} else {
						this.$alert(res.message, "提示", { type: "error" });
					}
				})
				.catch(() => {});
		},
		recycle_del() {
			this.$confirm(
				`确定移到回收站选中的 ${this.selection.length} 项吗？`,
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
					var res = await this.$API.sysmessage.recycle.delete(
						ids.join(",")
					);
					loading.close();
					if (res.code == 200) {
						this.initTotal();
						this.$refs.table.refresh();
						this.$message.success("删除成功");
					} else {
						this.$alert(res.message, "提示", { type: "error" });
					}
				})
				.catch(() => {});
		},
		all_read() {
			this.$confirm(`确定要设为全部已读吗？`, "提示", {
				type: "warning",
				confirmButtonText: "确定",
				cancelButtonText: "取消",
			})
				.then(async () => {
					const loading = this.$loading();
					var res = await this.$API.sysmessage.read.put([]);
					loading.close();
					if (res.code == 200) {
						this.initTotal();
						this.$refs.table.refresh();
						this.$message.success("操作成功");
					} else {
						this.$alert(res.message, "提示", { type: "error" });
					}
				})
				.catch(() => {});
		},
		set_read() {
			this.$confirm(
				`确定设置已读选中的 ${this.selection.length} 项吗？`,
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
					var res = await this.$API.sysmessage.read.put(ids);
					loading.close();
					if (res.code == 200) {
						this.initTotal();
						this.$refs.table.refresh();
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
			if (obj.command == "delete") {
				this.table_del(obj.row);
			}
		},
	},
};
</script>
<style>
.custom-tree-node {
	flex: 1;
	display: flex;
	align-items: center;
	justify-content: space-between;
	font-size: 14px;
	padding-right: 8px;
}
.mess-tag .el-tag {
	height: 18px;
	padding: 0 5px;
}
.mess-icon {
	width: 16px;
	height: 16px;
	vertical-align: middle;
	position: relative;
	top: -2px;
}
</style>
