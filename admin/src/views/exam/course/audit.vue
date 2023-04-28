<template>
	<el-container>
		<el-header class="header-tabs">
			<el-tabs type="card" v-model="param.type" @tab-change="tabChange">
				<el-tab-pane
					v-for="it in typeOption"
					:key="it.value"
					:label="it.label"
					:name="it.value"
				></el-tab-pane>
			</el-tabs>
		</el-header>
		<el-header>
			<div class="left-panel">
				<el-button
					icon="el-icon-select"
					type="primary"
					:disabled="selection.length == 0"
					v-auth="'examcourseaudit:audit'"
					@click="audit(1)"
					>审核通过</el-button
				>
				<el-button
					icon="el-icon-close"
					:disabled="selection.length == 0"
					type="danger"
					v-auth="'examcourseaudit:audit'"
					@click="audit(2)"
					>审核不通过</el-button
				>
			</div>
			<div class="right-panel">
				<div class="right-panel-search">
					<el-input
						v-model="param.key"
						clearable
						placeholder="关键字"
						style="width: 200px"
					/>
					<el-select
						v-model="param.audit"
						placeholder="请选择审核状态"
						clearable
					>
						<el-option
							v-for="(item, index) in auditOptions"
							:key="index"
							:label="item.label"
							:value="item.value"
						></el-option>
					</el-select>
					<el-select
						v-model="param.status"
						placeholder="请选择上架状态"
						clearable
					>
						<el-option
							v-for="(item, index) in statusOptions"
							:key="index"
							:label="item.label"
							:value="item.value"
						></el-option>
					</el-select>
					<el-select
						v-model="param.attr"
						placeholder="请选择属性"
						clearable
					>
						<el-option
							v-for="(item, index) in attrOptions"
							:key="index"
							:label="item.label"
							:value="item.value"
							:disabled="item.disabled"
						></el-option>
					</el-select>
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
						<el-button
							text
							type="primary"
							size="small"
							@click="open_dialog(scope.row)"
						>
							查看
						</el-button>
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
				<template #status="{ data }">
					<el-tag disable-transitions type="info">
						{{
							data.status == 1
								? "立即上架"
								: data.status == 2
								? "定时上架"
								: "暂不上架"
						}}
					</el-tag>
				</template>
				<template #type="{ data }">
					<el-tag disable-transitions type="success">
						{{
							data.type == 1
								? "点播"
								: data.type == 2
								? "直播"
								: "图文"
						}}
					</el-tag>
				</template>
				<template #difficulty="{ data }">
					{{ data.difficulty.name }}
				</template>
				<template #cover="{ data }">
					<el-image
						style="width: 100px; height: 60px"
						:src="data.cover"
						fit="contain"
					/>
				</template>
				<template #teacher="{ data }">
					{{ data.teacher.name }}
				</template>
				<template #attr="{ data }">
					<el-tag v-if="data.attr.includes(1)">评论</el-tag>
					<el-tag v-if="data.attr.includes(2)">互动</el-tag>
					<el-tag v-if="data.attr.includes(3)">推荐</el-tag>
					<el-tag v-if="data.attr.includes(4)">弹幕</el-tag>
					<el-tag v-if="data.attr.includes(5)">下载</el-tag>
					<el-tag v-if="data.attr.includes(6)">投票</el-tag>
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
			apiObj: this.$API.examcourse.page,
			list: [],
			param: {
				key: "",
				type: 0,
				attr: undefined,
				status: undefined,
				audit: undefined,
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
				{ prop: "title", label: "标题", width: 200, align: "left" },
				{ prop: "cover", label: "封面", width: 150 },
				{ prop: "audit", label: "审核状态", width: 100 },
				{ prop: "status", label: "上架状态", width: 100 },
				{ prop: "type", label: "课程类型", width: 100 },
				{ prop: "teacher", label: "讲师", width: 100 },
				{ prop: "gradeNames", label: "年级", width: 160 },
				{ prop: "subjectNames", label: "学科", width: 160 },
				{ prop: "difficulty", label: "难度", width: 100 },
				{ prop: "hits", label: "点击量", width: 80 },
				{ prop: "attr", label: "属性数组", width: 200 },
				{ prop: "createTime", label: "创建时间", width: 180 },
			],
			typeOption: [
				{ label: "所有", value: 0 },
				{ label: "点播", value: 1 },
				{ label: "直播", value: 2 },
				{ label: "图文", value: 3 },
			],
			attrOptions: [
				{
					label: "所有",
					value: 0,
				},
				{
					label: "评论",
					value: 1,
				},
				{
					label: "互动",
					value: 2,
				},
				{
					label: "推荐",
					value: 3,
				},
				{
					label: "弹幕",
					value: 4,
				},
				{
					label: "下载",
					value: 5,
				},
				{
					label: "投票",
					value: 6,
				},
			],
			statusOptions: [
				{
					label: "立即上架",
					value: 1,
				},
				{
					label: "定时上架",
					value: 2,
				},
				{
					label: "暂不上架",
					value: 3,
				},
			],
			auditOptions: [
				{
					label: "已审核",
					value: 1,
				},
				{
					label: "未审核",
					value: 2,
				},
			],
		};
	},
	mounted() {},
	methods: {
		tabChange() {
			this.$refs.table.upData(this.param);
		},
		complete() {
			this.$refs.table.refresh();
		},
		search() {
			this.$refs.table.upData(this.param);
		},
		//批量审核
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
					var res = await this.$API.examcourse.audit.put({
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
		open_dialog(row) {
			this.$refs.modify.open(row, 1);
		},
		selectionChange(selection) {
			this.selection = selection;
		},
	},
};
</script>
