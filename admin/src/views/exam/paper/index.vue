<template>
	<el-container>
		<el-header class="header-tabs">
			<el-tabs type="card" v-model="param.typeId" @tab-change="tabChange">
				<el-tab-pane
					v-for="it in typeOption"
					:key="it.value"
					:label="it.label"
					:name="it.value"
				></el-tab-pane>
			</el-tabs>
		</el-header>
		<el-header style="height: auto">
			<sc-select-filter
				:data="filterData"
				:label-width="80"
				@on-change="filterChange"
			></sc-select-filter>
		</el-header>
		<el-header>
			<div class="left-panel">
				<el-button
					icon="el-icon-plus"
					type="primary"
					v-auth="'exampaperlist:add'"
					@click="open_dialog"
				/>
				<el-button
					icon="el-icon-delete"
					plain
					type="danger"
					:disabled="selection.length == 0"
					v-auth="'exampaperlist:delete'"
					@click="batch_del"
				/>
				<el-button
					icon="el-icon-collection"
					plain
					type="success"
					:disabled="selection.length == 0"
					v-auth="'exampaperlist:release'"
					@click="release"
					>发布</el-button
				>
				<el-button
					icon="el-icon-collection-tag"
					plain
					type="warning"
					:disabled="selection.length == 0"
					v-auth="'exampaperlist:revocation'"
					@click="undo"
					>撤销</el-button
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
							v-auth="'exampaperlist:edit'"
							@click="open_dialog(scope.row)"
						>
							编辑
						</el-button>
						<el-divider
							direction="vertical"
							v-auths-all="[
								'exampaperlist:edit',
								'exampaperlist:delete',
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
									v-auth="'exampaperlist:delete'"
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
						{{ data.status ? "已发布" : "未发布" }}
					</el-tag>
				</template>
				<template #grandCode="{ data }">
					{{ data.grandCode.name }}
				</template>
				<template #subjectCode="{ data }">
					{{ data.subjectCode.name }}
				</template>
				<template #typeCode="{ data }">
					{{ data.typeCode.name }}
				</template>
				<template #antiCheating="{ data }">
					<el-tag
						disable-transitions
						type="success"
						v-if="data.antiCheating.includes(1)"
					>
						题目打乱
					</el-tag>
					<el-tag
						disable-transitions
						type="success"
						v-if="data.antiCheating.includes(2)"
					>
						选项打乱
					</el-tag>
					<el-tag
						disable-transitions
						type="success"
						v-if="data.antiCheating.length == 0"
					>
						无
					</el-tag>
				</template>
				<template #totalSocre="{ data }">
					{{
						data.questionItem
							.map((item) => item.score)
							.reduce((prev, curr) => prev + curr, 0)
					}}
				</template>
				<template #questionSum="{ data }">
					{{ data.questionItem.length }}
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
			apiObj: this.$API.exampaper.page,
			list: [],
			param: {
				key: "",
				subject: 0,
				typeId: 0,
				grand: "",
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
					label: "试卷名称",
					width: 200,
					align: "left",
					fixed: "left",
				},
				{ prop: "grandCode", label: "年级", width: 100 },
				{ prop: "subjectCode", label: "学科", width: 100 },
				{ prop: "status", label: "发布状态", width: 100 },
				{ prop: "typeCode", label: "试卷类型", width: 100 },
				{ prop: "antiCheating", label: "防篡改", width: 200 },
				{ prop: "totalSocre", label: "总分", width: 120 },
				{ prop: "questionSum", label: "题目数", width: 120 },
				{ prop: "minutesLength", label: "考试时长(分)", width: 120 },
				{ prop: "createTime", label: "创建时间", width: 180 },
			],
			filterData: [
				{
					title: "学科",
					key: "subject",
					options: [
						{
							label: "全部",
							value: "",
						},
					],
				},
				{
					title: "年级",
					key: "grand",
					multiple: true,
					options: [
						{
							label: "全部",
							value: "",
						},
					],
				},
			],
			typeOption: [{ label: "所有", value: 0 }],
		};
	},
	mounted() {
		this.initGrandOption();
		this.initSubjectOption();
		this.initTypeOption();
	},
	methods: {
		async initGrandOption() {
			const res = await this.$API.syscode.list.get({ typeCode: "grand" });
			if (res.code == 200) {
				res.data.forEach((e) => {
					this.filterData[1].options.push({
						label: e.name,
						value: e.id,
					});
				});
			}
		},
		async initSubjectOption() {
			const res = await this.$API.syscode.list.get({
				typeCode: "subject",
			});
			if (res.code == 200) {
				res.data.forEach((e) => {
					this.filterData[0].options.push({
						label: e.name,
						value: e.id,
					});
				});
			}
		},
		async initTypeOption() {
			const res = await this.$API.syscode.list.get({
				typeCode: "paperType",
			});
			if (res.code == 200) {
				res.data.forEach((e) => {
					this.typeOption.push({ label: e.name, value: e.id });
				});
			}
		},
		complete() {
			this.$refs.table.refresh();
		},
		search() {
			this.$refs.table.upData(this.param);
		},
		async release() {
			let ids = [];
			this.selection.forEach((element) => {
				ids.push(element.id);
			});
			var res = await this.$API.exampaper.release.put(ids);
			if (res.code == 200) {
				this.$refs.table.refresh();
				this.$message.success("发布成功");
			} else {
				this.$alert(res.message, "提示", { type: "error" });
			}
		},
		async undo() {
			let ids = [];
			this.selection.forEach((element) => {
				ids.push(element.id);
			});
			var res = await this.$API.exampaper.undo.put(ids);
			if (res.code == 200) {
				this.$refs.table.refresh();
				this.$message.success("撤销成功");
			} else {
				this.$alert(res.message, "提示", { type: "error" });
			}
		},
		filterChange(data) {
			if (data.subject) {
				this.param.subject = data.subject;
			} else {
				this.param.subject = "0";
			}
			if (data.grand) {
				this.param.grand = data.grand;
			} else {
				this.param.grand = "";
			}
			this.$refs.table.upData(this.param);
		},
		tabChange() {
			this.$refs.table.upData(this.param);
		},
		//删除
		async table_del(row) {
			var res = await this.$API.exampaper.delete.delete([row.id]);
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
					var res = await this.$API.exampaper.delete.delete(ids);
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
