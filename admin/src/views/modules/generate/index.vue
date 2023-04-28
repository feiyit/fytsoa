<template>
	<el-container>
		<!-- <el-header>
			<div class="left-panel">
				<el-button
					icon="el-icon-files"
					type="primary"
					:disabled="selection.length == 0"
					@click="open_save"
					>批量生成代码</el-button
				>
			</div>
		</el-header> -->
		<el-main class="nopadding">
			<scTable
				ref="table"
				:api-obj="apiObj"
				:column="column"
				row-key="id"
				:hidePagination="true"
				:hide-context-menu="false"
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
					label="#"
					type="index"
					width="50"
				></el-table-column>
				<el-table-column
					align="center"
					fixed="right"
					label="操作"
					width="200"
				>
					<template #default="scope">
						<el-button
							size="small"
							text
							type="primary"
							v-auth="'generate:generate'"
							@click="open_save(scope.row)"
						>
							生成代码
						</el-button>
						<el-divider direction="vertical" />
						<el-button
							text
							type="primary"
							size="small"
							v-auth="'generate:look'"
							@click="open_dialog(scope.row)"
							>查看表</el-button
						>
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
		<column ref="column" />
		<save ref="save" />
	</el-container>
</template>
<script>
import { defineAsyncComponent } from "vue";
export default {
	components: {
		column: defineAsyncComponent(() => import("./column")),
		save: defineAsyncComponent(() => import("./save")),
	},
	data() {
		return {
			apiObj: this.$API.modulesgenerate.list,
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
					label: "名称",
					prop: "name",
					width: 200,
					align: "left",
				},
				{
					label: "描述",
					prop: "description",
					align: "left",
				},
			],
		};
	},
	mounted() {},
	methods: {
		open_save(row) {
			if (row.name) {
				this.$refs.save.open([row]);
			} else {
				this.$refs.save.open(this.selection);
			}
		},
		open_dialog(row) {
			this.$refs.column.open(row);
		},
		selectionChange(selection) {
			this.selection = selection;
		},
	},
};
</script>
