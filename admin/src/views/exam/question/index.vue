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
					v-auth="'examquestion:add'"
					@click="open_dialog"
				/>
				<el-button
					icon="el-icon-delete"
					plain
					type="danger"
					:disabled="selection.length == 0"
					v-auth="'examquestion:delete'"
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
				ref="tableRef"
				:api-obj="apiObj"
				:column="column"
				row-key="id"
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
							v-auth="'examquestion:edit'"
							@click="open_dialog(scope.row)"
						>
							编辑
						</el-button>
						<el-divider
							direction="vertical"
							v-auths-all="[
								'examquestion:edit',
								'examquestion:delete',
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
									v-auth="'examquestion:delete'"
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
				<template #grandCode="{ data }">
					{{ data.grandCode.name }}
				</template>
				<template #subjectCode="{ data }">
					{{ data.subjectCode.name }}
				</template>
				<template #type="{ data }">
					{{ resQuestionType(data.type) }}
				</template>
				<template #answer="{ data }">
					<span v-if="data.type != 4">{{ data.answer }}</span>
					<div v-if="data.type == 4">
						<p v-for="it in JSON.parse(data.answer)" :key="it.name">
							{{ it.name }}
						</p>
					</div>
				</template>
			</scTable>
		</el-main>
		<suspense>
			<template #default>
				<modify ref="modifyRef" @complete="complete" />
			</template>
		</suspense>
	</el-container>
</template>
<script setup>
import { ref, getCurrentInstance, onMounted } from "vue";
import modify from "./modify.vue";
const app = getCurrentInstance();
const api = app.appContext.config.globalProperties.$API;
const apiObj = ref(api.examquestion.page);
const message = app.appContext.config.globalProperties.$message;
const alert = app.appContext.config.globalProperties.$alert;
const confirm = app.appContext.config.globalProperties.$confirm;
const selection = ref([]);
const modifyRef = ref();
const tableRef = ref();
const column = [
	{
		label: "id",
		prop: "id",
		width: "200",
		sortable: true,
		hide: true,
	},
	{ prop: "title", label: "题目", width: 300, align: "left", fixed: "left" },
	{ prop: "grandCode", label: "年级编号", width: 100 },
	{ prop: "subjectCode", label: "学科编号", width: 100 },
	{ prop: "type", label: "类型", width: 100 },
	{ prop: "answer", label: "答案", width: 200, align: "left" },
	{ prop: "score", label: "分数", width: 80 },
	{ prop: "difficulty", label: "难度", width: 80 },
	{ prop: "createTime", label: "创建时间", width: 180 },
];
const typeOption = [
	{ label: "所有", value: 0 },
	{ label: "单选题", value: 1 },
	{ label: "多选题", value: 2 },
	{ label: "判断题", value: 3 },
	{ label: "填空题", value: 4 },
	{ label: "解答题", value: 5 },
];
const filterData = ref([
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
]);
const param = ref({
	key: "",
	subject: 0,
	type: 0,
	grand: "",
});
const tabChange = () => {
	tableRef.value.upData(param.value);
};
const filterChange = (data) => {
	if (data.subject) {
		param.value.subject = data.subject;
	} else {
		param.value.subject = "0";
	}
	if (data.grand) {
		param.value.grand = data.grand;
	} else {
		param.value.grand = "";
	}
	tableRef.value.upData(param.value);
};
const complete = () => {
	tableRef.value.refresh();
};
const search = () => {
	tableRef.value.upData(param.value);
};
const selectionChange = (array) => {
	selection.value = array;
};
const resQuestionType = (number) => {
	let str = "解答题";
	switch (number) {
		case 1:
			str = "单选题";
			break;
		case 2:
			str = "多选题";
			break;
		case 3:
			str = "判断题";
			break;
		case 4:
			str = "填空题";
			break;
	}
	return str;
};
const table_del = async (row) => {
	var res = await api.examquestion.delete.delete([row.id]);
	if (res.code == 200) {
		tableRef.value.refresh();
		message.success("删除成功");
	} else {
		alert(res.message, "提示", { type: "error" });
	}
};
const batch_del = () => {
	confirm(`确定删除选中的 ${selection.value.length} 项吗？`, "提示", {
		type: "warning",
		confirmButtonText: "确定",
		cancelButtonText: "取消",
	})
		.then(async () => {
			let ids = [];
			selection.value.forEach((element) => {
				ids.push(element.id);
			});
			var res = await api.examquestion.delete.delete(ids);
			if (res.code == 200) {
				tableRef.value.refresh();
				message.success("删除成功");
			} else {
				alert(res.message, "提示", { type: "error" });
			}
		})
		.catch(() => {});
};
const open_dialog = (row) => {
	if (row.id) {
		modifyRef.value.open(row);
	} else {
		modifyRef.value.open();
	}
};
const initGrandOption = async () => {
	const res = await api.syscode.list.get({ typeCode: "grand" });
	if (res.code == 200) {
		res.data.forEach((e) => {
			filterData.value[1].options.push({ label: e.name, value: e.id });
		});
	}
};
const initSubjectOption = async () => {
	const res = await api.syscode.list.get({ typeCode: "subject" });
	if (res.code == 200) {
		res.data.forEach((e) => {
			filterData.value[0].options.push({ label: e.name, value: e.id });
		});
	}
};
onMounted(() => {
	initGrandOption();
	initSubjectOption();
});
</script>
