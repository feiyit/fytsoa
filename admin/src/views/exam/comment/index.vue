<template>
	<el-container>
		<el-header>
			<div class="left-panel">
				<el-button
					icon="el-icon-select"
					type="primary"
					:disabled="selection.length == 0"
					v-auth="'examcomment:audit'"
					@click="audit(1)"
					>审核通过</el-button
				>
				<el-button
					icon="el-icon-close"
					:disabled="selection.length == 0"
					type="danger"
					v-auth="'examcomment:audit'"
					@click="audit(2)"
					>审核不通过</el-button
				>
				<el-button
					icon="el-icon-delete"
					plain
					type="danger"
					:disabled="selection.length == 0"
					v-auth="'examcomment:delete'"
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
				:hide-context-menu="false"
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
					width="80"
				>
					<template #default="scope">
						<el-popconfirm
							title="确定删除吗？"
							@confirm="table_del(scope.row, scope.$index)"
						>
							<template #reference>
								<el-button
									text
									type="primary"
									size="small"
									v-auth="'examcomment:delete'"
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
				<template #course="{ data }">
					{{ data.course.title }}
				</template>
				<template #user="{ data }">
					{{ data.user.nickName }}
				</template>
				<template #replyBody="{ data }">
					<el-tag
						disable-transitions
						effect="dark"
						@click="look(data)"
					>
						{{ data.replyBody?.length }}
					</el-tag>
				</template>
			</scTable>
		</el-main>
		<sc-dialog
			v-model="visible"
			show-fullscreen
			title="回复内容"
			width="750px"
			@close="close"
		>
			<el-scrollbar height="400px">
				<el-timeline>
					<el-timeline-item
						v-for="(item, index) in reply"
						:key="index"
						:timestamp="item.replyTime"
					>
						<p>
							<el-link type="primary" :underline="false">{{
								item.nickName
							}}</el-link
							>：<el-link type="primary" :underline="false">{{
								item.byNickName
							}}</el-link>
						</p>
						{{ item.content }}
					</el-timeline-item>
				</el-timeline>
			</el-scrollbar>
		</sc-dialog>
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
			apiObj: this.$API.examcomment.page,
			list: [],
			param: {
				key: "",
			},
			visible: false,
			selection: [],
			reply: [],
			column: [
				{
					label: "id",
					prop: "id",
					width: "200",
					sortable: true,
					hide: true,
				},
				{ prop: "course", label: "课程", width: 200, align: "left" },
				{ prop: "user", label: "用户", width: 100 },
				{ prop: "audit", label: "审核状态", width: 100 },
				{
					prop: "content",
					label: "评论内容",
					width: 200,
					align: "left",
					showOverflowTooltip: true,
				},
				{ prop: "star", label: "评星", width: 100 },
				{ prop: "replyBody", label: "回复数", width: 100 },
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
		//审核
		audit(type) {
			this.$confirm(
				`确定执行` +
					(type == 1 ? "【审核通过】" : "【审核不通过】") +
					`选中的 ${this.selection.length} 项吗？`,
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
					var res = await this.$API.examcomment.audit.put({
						ids: ids,
						audit: type == 1 ? true : false,
					});
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
					var res = await this.$API.examcomment.delete.delete(ids);
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
		look(row) {
			if (row.replyBody.length == 0) {
				this.$message.warning("没有回复内容");
				return;
			}
			this.reply = row.replyBody;
			this.visible = true;
		},
		selectionChange(selection) {
			this.selection = selection;
		},
		close() {
			this.visible = false;
		},
	},
};
</script>
