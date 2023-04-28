<template>
	<sc-dialog
		v-model="visible"
		show-fullscreen
		:title="titleMap[mode]"
		width="650px"
		@close="close"
	>
		<el-form
			ref="formRef"
			:model="formData"
			:rules="rules"
			label-width="100px"
		>
			<el-form-item label="日程内容" prop="title">
				<el-input
					v-model="formData.title"
					placeholder="请输入日程内容"
					:maxlength="100"
					show-word-limit
					clearable
					:style="{ width: '100%' }"
				></el-input>
			</el-form-item>
			<el-form-item label="日程类型" prop="typeId">
				<el-select
					v-model="formData.typeId"
					placeholder="请选择日程类型"
					clearable
					:style="{ width: '100%' }"
				>
					<el-option
						v-for="(item, index) in typeOptions"
						:key="index"
						:label="item.label"
						:value="item.value"
						:disabled="item.disabled"
					></el-option>
				</el-select>
			</el-form-item>
			<el-form-item label="日程级别" prop="levelId">
				<el-select
					v-model="formData.levelId"
					placeholder="请选择日程级别"
					clearable
					:style="{ width: '100%' }"
				>
					<el-option
						v-for="item in levelOptions"
						:key="item.value"
						:label="item.label"
						:value="item.value"
					>
						<span style="float: left">{{ item.label }}</span>
						<span style="float: right"
							><small
								class="circle"
								:style="{ 'background-color': item.color }"
							></small></span
					></el-option>
				</el-select>
			</el-form-item>
			<el-form-item label="开始时间" prop="startTime">
				<el-date-picker
					type="datetime"
					v-model="formData.startTime"
					:style="{ width: '100%' }"
					placeholder="请选择开始时间"
					clearable
				>
				</el-date-picker>
			</el-form-item>
			<el-form-item label="结束时间" prop="endTime">
				<el-date-picker
					type="datetime"
					v-model="formData.endTime"
					:style="{ width: '100%' }"
					placeholder="请选择结束时间"
					clearable
				>
				</el-date-picker>
			</el-form-item>
			<el-form-item label="参与人" prop="userIds">
				<sc-table-select
					v-model="formData.userIds"
					:apiObj="apiObj"
					:table-width="450"
					:props="props"
					:style="{ width: '100%' }"
					multiple
					clearable
					collapse-tags
					collapse-tags-tooltip
				>
					<el-table-column prop="headPic" label="头像" width="80">
						<template #default="scope">
							<el-avatar
								:src="$CONFIG.SERVER_URL + scope.row.headPic"
								size="small"
							></el-avatar>
						</template>
					</el-table-column>
					<el-table-column
						prop="fullName"
						label="姓名"
						width="180"
					></el-table-column>
					<el-table-column
						prop="roleGroupName"
						label="角色"
					></el-table-column>
				</sc-table-select>
			</el-form-item>
		</el-form>

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
	emits: ["complete"],
	data() {
		return {
			mode: "add",
			titleMap: {
				add: "新增",
				edit: "编辑",
			},
			isSaveing: false,
			visible: false,
			apiObj: this.$API.sysadmin.page,
			props: {
				label: "fullName",
				value: "id",
				keyword: "keyword",
			},
			formData: {
				id: 0,
				title: undefined,
				typeId: undefined,
				levelId: undefined,
				startTime: null,
				endTime: null,
				userIds: [],
				toBusiness: undefined,
				remind: undefined,
				createTime: undefined,
			},
			rules: {
				title: [
					{
						required: true,
						message: "请输入日程内容",
						trigger: "blur",
					},
				],
				typeId: [
					{
						required: true,
						message: "请选择日程类型",
						trigger: "change",
					},
				],
				levelId: [
					{
						required: true,
						message: "请选择日程级别",
						trigger: "change",
					},
				],
				startTime: [
					{
						required: true,
						message: "请选择开始时间",
						trigger: "change",
					},
				],
				endTime: [
					{
						required: true,
						message: "请选择结束时间",
						trigger: "change",
					},
				],
				userIds: [
					{
						required: true,
						message: "请选择参与人",
						trigger: "change",
					},
				],
			},
			typeOptions: [],
			levelOptions: [],
		};
	},
	mounted() {
		this.initTypeOption();
		this.initLevelOption();
	},
	methods: {
		async initTypeOption() {
			const res = await this.$API.syscode.list.get({
				typeCode: "calendar-type",
			});
			if (res.code == 200) {
				res.data.forEach((e) => {
					this.typeOptions.push({
						label: e.name,
						value: e.id,
					});
				});
			}
		},
		async initLevelOption() {
			const res = await this.$API.syscode.list.get({
				typeCode: "calendar-level",
			});
			if (res.code == 200) {
				res.data.forEach((e) => {
					this.levelOptions.push({
						label: e.name,
						value: e.id,
						color: e.codeValues,
					});
				});
			}
		},
		async open(row) {
			if (!row) {
				this.mode = "add";
			} else {
				this.mode = "edit";
				var res = await this.$API.syscalendar.model.get(row.id);
				this.formData = res.data;
			}
			this.visible = true;
		},
		save() {
			this.$refs.formRef.validate(async (valid) => {
				if (valid) {
					this.isSaveing = true;
					let res = null;
					if (this.formData.id === 0) {
						res = await this.$API.syscalendar.add.post(
							this.formData
						);
					} else {
						res = await this.$API.syscalendar.update.put(
							this.formData
						);
					}
					this.isSaveing = false;
					if (res.code == 200) {
						this.$emit("complete");
						this.visible = false;
						this.$message.success("操作成功");
					} else {
						this.$alert(res.message, "提示", { type: "error" });
					}
				}
			});
		},
		close() {
			this.formData = {
				id: 0,
				title: undefined,
				typeId: undefined,
				levelId: undefined,
				startTime: null,
				endTime: null,
				userIds: [],
				toBusiness: undefined,
				remind: undefined,
			};
			this.$refs.formRef.resetFields();
			this.visible = false;
		},
	},
};
</script>
<style scoped>
.circle {
	display: inline-block;
	width: 10px;
	height: 10px;
	border-radius: 5px;
}
</style>
