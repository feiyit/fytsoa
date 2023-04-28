<template>
	<sc-dialog
		v-model="visible"
		show-fullscreen
		:title="titleMap[mode]"
		top="10vh"
		width="1100px"
		destroy-on-close
		@close="close"
	>
		<el-form
			ref="formRef"
			:model="formData"
			:rules="rules"
			size="medium"
			label-width="100px"
		>
			<el-row>
				<el-col :span="8">
					<el-form-item label="命名空间" prop="namespace">
						<el-input
							v-model="formData.namespace"
							placeholder="例如：Sys"
							:maxlength="30"
							show-word-limit
							clearable
							prefix-icon="el-icon-collection-tag"
							:style="{ width: '100%' }"
						></el-input>
					</el-form-item>
				</el-col>
				<el-col :span="8">
					<el-form-item label="API版本" prop="apiVersion">
						<el-input
							v-model="formData.apiVersion"
							placeholder="例如：v1"
							:maxlength="20"
							show-word-limit
							clearable
							prefix-icon="el-icon-discount"
							:style="{ width: '100%' }"
						></el-input>
					</el-form-item>
				</el-col>
				<el-col :span="8">
					<el-form-item label="栅格" prop="isGrid">
						<el-switch v-model="formData.isGrid" />
					</el-form-item>
				</el-col>
			</el-row>
		</el-form>
		<el-table
			ref="table"
			:data="tableData"
			height="450"
			row-key="dbColumnName"
		>
			<el-table-column
				label="#"
				type="index"
				width="50"
			></el-table-column>
			<el-table-column prop="dbColumnName" label="列名" width="120" />
			<el-table-column prop="dataType" label="数据类型" width="120" />
			<el-table-column prop="isPrimarykey" label="是否主键" width="100">
				<template #default="scope">
					<el-tag
						disable-transitions
						:type="scope.row.isPrimarykey ? 'success' : 'danger'"
					>
						{{ scope.row.isPrimarykey ? "是" : "否" }}
					</el-tag>
				</template>
			</el-table-column>
			<el-table-column prop="isSearch" label="搜索" width="100">
				<template #default="scope">
					<el-switch v-model="scope.row.isSearch" />
				</template>
			</el-table-column>
			<el-table-column prop="isColumn" label="列表展示" width="100">
				<template #default="scope">
					<el-switch v-model="scope.row.isColumn" />
				</template>
			</el-table-column>
			<el-table-column prop="isAdd" label="添加/编辑" width="100">
				<template #default="scope">
					<el-switch v-model="scope.row.isAdd" />
				</template>
			</el-table-column>
			<el-table-column prop="required" label="必填项" width="100">
				<template #default="scope">
					<el-switch v-model="scope.row.required" />
				</template>
			</el-table-column>
			<el-table-column prop="componentType" label="组件类型" width="160">
				<template #default="scope">
					<el-select
						v-model="scope.row.componentType"
						placeholder="Select"
					>
						<el-option
							v-for="item in componentOptions"
							:key="item.value"
							:label="item.label"
							:value="item.value"
						/>
					</el-select>
				</template>
			</el-table-column>
			<el-table-column prop="columnDescription" label="描述" />
		</el-table>

		<template #footer>
			<el-button @click="close">取 消</el-button>
			<el-button :loading="isSaveing" type="primary" @click="save">
				确 定
			</el-button>
		</template>
	</sc-dialog>
</template>
<script>
export default {
	data() {
		return {
			mode: "add",
			titleMap: {
				add: "生成代码",
			},
			tableData: [],
			apiObj: this.$API.modulesgenerate.column,
			isSaveing: false,
			visible: false,
			formData: {
				tableNames: [],
				namespace: "",
				types: 2,
				isGrid: false,
				apiVersion: "",
				tableColumnInfo: [],
			},
			rules: {
				namespace: [
					{
						required: true,
						message: "例如：Sys",
						trigger: "blur",
					},
				],
				isGrid: [
					{
						required: true,
						message: "单选框组不能为空",
						trigger: "change",
					},
				],
				apiVersion: [
					{
						required: true,
						message: "例如：v1",
						trigger: "blur",
					},
				],
			},
			typesOptions: [
				{
					label: "全部表",
					value: 1,
				},
				{
					label: "部分表",
					value: 2,
				},
			],
			ignoreShow: [
				"Id",
				"CreateUser",
				"UpdateTime",
				"UpdateUser",
				"TenantId",
			],
			componentOptions: [
				{
					value: "input",
					label: "文本框",
				},
				{
					value: "select",
					label: "下拉框",
				},
				{
					value: "textarea",
					label: "多行文本框",
				},
				{
					value: "switch",
					label: "开关",
				},
				{
					value: "time",
					label: "日期",
				},
			],
		};
	},
	methods: {
		async save() {
			this.$refs.formRef.validate(async (valid) => {
				if (valid) {
					this.formData.tableColumnInfo = this.tableData;
					//this.isSaveing = true;
					const res = await this.$API.modulesgenerate.code.post(
						this.formData
					);
					//this.isSaveing = false;
					if (res.code == 200) {
						this.$message.success(
							"生成成功，到根目录【Generator】文件夹中查看"
						);
					} else {
						this.$alert(res.message, "提示", { type: "error" });
					}
				}
			});
		},
		async open(row) {
			this.formData.tableNames = [];
			row.forEach((element) => {
				this.formData.tableNames.push(element.name);
			});
			const res = await this.$API.modulesgenerate.column.get({
				tableName: row[0].name,
			});
			this.tableData = res.data.items;
			this.tableData.forEach((item) => {
				const index = this.ignoreShow.findIndex((row) => {
					if (row == item.dbColumnName) {
						return true;
					}
				});
				item.isColumn = index == -1 ? true : false;
				item.isAdd = index == -1 ? true : false;
				item.isSearch = false;
				if (item.isAdd) {
					item.required = !item.isNullable;
				}
				if (item.isAdd) {
					item.componentType = "input";
				}
			});

			this.visible = true;
		},
		close() {
			this.visible = false;
		},
	},
};
</script>
