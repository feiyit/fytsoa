<template>
	<el-drawer
		v-model="visible"
		show-fullscreen
		:title="titleMap[mode]"
		size="70%"
		@close="close"
	>
		<el-form
			ref="formRef"
			label-width="100px"
			:model="formData"
			:rules="rules"
		>
			<div class="elem-quote"><span>基本信息</span></div>
			<el-form-item label="课程类型" prop="type">
				<el-radio-group v-model="formData.type" size="small">
					<el-radio label="1" border>点播</el-radio>
					<el-radio label="2" border>直播</el-radio>
					<el-radio label="3" border>图文</el-radio>
				</el-radio-group>
			</el-form-item>
			<el-row>
				<el-col :span="12">
					<el-form-item label="年级" prop="gradeId">
						<el-select
							v-model="formData.gradeId"
							placeholder="请选择年级"
							multiple
							clearable
							:style="{ width: '100%' }"
						>
							<el-option
								v-for="(item, index) in gradeIdOptions"
								:key="index"
								:label="item.label"
								:value="item.value"
								:disabled="item.disabled"
							></el-option>
						</el-select>
					</el-form-item>
				</el-col>
				<el-col :span="12">
					<el-form-item label="学科" prop="subjectId">
						<el-select
							v-model="formData.subjectId"
							placeholder="请选择学科"
							multiple
							clearable
							:style="{ width: '100%' }"
						>
							<el-option
								v-for="(item, index) in subjectIdOptions"
								:key="index"
								:label="item.label"
								:value="item.value"
								:disabled="item.disabled"
							></el-option>
						</el-select>
					</el-form-item>
				</el-col>
			</el-row>
			<el-form-item label="难度" prop="difficultyId">
				<el-radio-group v-model="formData.difficultyId">
					<el-radio
						v-for="(item, index) in difficultyIdOptions"
						:key="index"
						:label="item.value"
						:disabled="item.disabled"
						>{{ item.label }}</el-radio
					>
				</el-radio-group>
			</el-form-item>
			<el-form-item label="标题" prop="title">
				<el-input
					v-model="formData.title"
					placeholder="请输入标题"
					:maxlength="255"
					show-word-limit
					clearable
				></el-input>
			</el-form-item>
			<el-form-item label="封面" prop="cover">
				<div class="cur-cover">
					<sc-upload
						v-model="formData.cover"
						:apiObj="uploadApi"
						:width="280"
						:height="160"
						:onSuccess="upSuccess"
					>
						<div class="custom-empty">
							<el-icon><el-icon-upload /></el-icon>
							<p>请选择封面上传</p>
						</div>
					</sc-upload>
					<div class="cover-tip">
						<el-alert
							title="视频封面是指在商品列表展示的图片。建议尺寸750*560px或4：3，JPG、PNG格式， 图片小于1M。"
							type="info"
							show-icon
							:closable="false"
						/>
						<el-button
							type="primary"
							round
							icon="el-icon-picture"
							style="margin-top: 20px"
							@click="coverSelect(1)"
							>素材库中选择封面</el-button
						>
					</div>
				</div>
			</el-form-item>
			<el-form-item
				:label="formData.type == 1 ? '点播地址' : '直播地址'"
				prop="urls"
				v-if="formData.type != 3"
			>
				<el-input
					v-model="formData.urls"
					placeholder="请输入地址"
					:maxlength="900"
					show-word-limit
					style="width: 75%"
					clearable
				></el-input>
				<el-button
					type="primary"
					round
					icon="el-icon-video-camera"
					style="margin-left: 20px"
					@click="coverSelect(2)"
					>素材库中选择</el-button
				>
			</el-form-item>
			<el-form-item label="">
				<el-alert
					title="点播请选择MP4文件为播放内容，可支持PC+移动端，如果是直播，请输入M3U8输出的媒体流。"
					type="info"
					show-icon
					:closable="false"
				/>
			</el-form-item>
			<el-form-item label="" v-if="formData.type != 3 && formData.urls">
				<div style="width: 500px">
					<sc-video
						:src="formData.urls"
						:is-live="formData.type == 2 ? true : false"
						:options="videoOptions"
					></sc-video>
				</div>
			</el-form-item>
			<el-form-item label="课件" prop="courseware" class="courseware">
				<sc-form-table
					v-model="formData.courseware"
					:addTemplate="apiListAddTemplate"
					placeholder="暂无课件"
				>
					<el-table-column prop="name" label="课件名称" width="200">
						<template #default="scope">
							<el-input
								v-model="scope.row.name"
								placeholder="请输入课件名称"
							></el-input>
						</template>
					</el-table-column>
					<el-table-column prop="url" label="课件素材">
						<template #default="scope">
							<el-input
								v-model="scope.row.url"
								placeholder="请输入课件地址"
							></el-input>
						</template>
					</el-table-column>
					<el-table-column
						prop="option"
						label="操作"
						width="190"
						align="center"
					>
						<template #default="scope">
							<el-button
								type="primary"
								round
								icon="el-icon-folder"
								@click="coursewareSelect(scope.$index)"
								style="font-size: 12px"
								>素材库中选择课件</el-button
							>
						</template>
					</el-table-column>
				</sc-form-table>
			</el-form-item>
			<el-form-item label="内容描述" prop="summary">
				<el-input
					v-model="formData.summary"
					type="textarea"
					placeholder="请输入内容描述"
					:autosize="{ minRows: 4, maxRows: 4 }"
					:style="{ width: '100%' }"
				></el-input>
			</el-form-item>
			<el-form-item label="内容详细" prop="content">
				<sc-editor
					v-model="formData.content"
					placeholder="请输入内容详细"
				></sc-editor>
			</el-form-item>
			<div class="elem-quote"><span>教师设置</span></div>
			<el-form-item label="" prop="teacherId" class="select-teacher">
				<div class="speaker">
					<el-avatar
						class="avatar"
						:size="50"
						:src="formData.teacher.avatar"
					/>
					<div class="info">
						{{ formData.teacher.name }}
						<p>{{ formData.teacher.postName }}</p>
					</div>
				</div>
				<el-button
					type="primary"
					round
					icon="el-icon-user"
					@click="speakerSelect"
					>选择教师</el-button
				>
			</el-form-item>
			<div class="elem-quote"><span>上架/审核</span></div>
			<el-form-item label="审核" prop="audit" required>
				<el-switch
					v-model="formData.audit"
					inline-prompt
					active-text="已审核"
					inactive-text="未审核"
					width="68px"
				></el-switch>
			</el-form-item>
			<el-row>
				<el-col :span="12">
					<el-form-item label="上架状态" prop="status">
						<el-radio-group
							v-model="formData.status"
							class="course-status"
						>
							<el-radio
								v-for="(item, index) in statusOptions"
								:key="index"
								:label="item.value"
								:disabled="item.disabled"
								>{{ item.label }}</el-radio
							>
						</el-radio-group>
					</el-form-item>
				</el-col>
				<el-col :span="12">
					<el-form-item
						label="定时时间"
						prop="timing"
						v-if="formData.status == 2"
					>
						<el-date-picker
							type="datetime"
							v-model="formData.timing"
							:style="{ width: '100%' }"
							placeholder="请选择定时时间"
							clearable
						>
						</el-date-picker>
					</el-form-item>
				</el-col>
			</el-row>
			<div class="elem-quote"><span>属性设置</span></div>
			<el-row>
				<el-col :span="8">
					<el-form-item label="评论数" prop="commentSum">
						<el-input-number
							v-model="formData.commentSum"
							placeholder="请输入点击量"
							:disabled="true"
							:step="1"
						></el-input-number>
					</el-form-item>
				</el-col>
				<el-col :span="8">
					<el-form-item label="点击量" prop="hits">
						<el-input-number
							v-model="formData.hits"
							placeholder="请输入点击量"
							:min="0"
							:step="1"
						></el-input-number>
					</el-form-item>
				</el-col>
				<el-form-item label="其他属性" prop="attr">
					<el-checkbox-group v-model="formData.attr">
						<el-checkbox
							v-for="(item, index) in attrOptions"
							:key="index"
							:label="item.value"
							:disabled="item.disabled"
							>{{ item.label }}</el-checkbox
						>
					</el-checkbox-group>
				</el-form-item>
			</el-row>
		</el-form>

		<template #footer>
			<el-button @click="close">取 消</el-button>
			<el-button
				:loading="isSaveing"
				v-if="!isLook"
				type="primary"
				@click="save"
			>
				确 定
			</el-button>
		</template>
		<teacher ref="teacher" @complete="teacherComplete" />
		<select-material
			ref="selectMaterial"
			@complete="selectMaterialComplete"
		/>
	</el-drawer>
</template>
<script>
import scVideo from "@/components/scVideo";
import { defineAsyncComponent } from "vue";
const scEditor = defineAsyncComponent(() => import("@/components/scEditor"));
export default {
	emits: ["complete"],
	components: {
		scEditor,
		scVideo,
		teacher: defineAsyncComponent(() => import("./teacher")),
		selectMaterial: defineAsyncComponent(() =>
			import("@/views/exam/material/selectMaterial")
		),
	},
	data() {
		return {
			mode: "add",
			titleMap: {
				add: "新增",
				edit: "编辑",
			},
			uploadApi: this.$API.sysfile.knowledgeCover,
			isSaveing: false,
			visible: false,
			videoOptions: {
				poster: "",
				pip: true,
			},
			formData: {
				id: 0,
				type: "1",
				teacherId: "",
				teacher: { name: "姓名", postName: "讲师职称", avatar: "" },
				gradeId: [],
				subjectId: [],
				difficultyId: "",
				title: "",
				urls: "",
				courses: [],
				cover: "",
				audit: false,
				status: 1,
				timing: undefined,
				hits: 0,
				attr: [1],
				commentSum: 0,
				dot: "",
				courseware: [],
				isDelete: false,
				summary: "",
				content: "",
			},
			rules: {
				title: [
					{
						required: true,
						message: "请输入标题",
						trigger: "blur",
					},
				],
				gradeId: [
					{
						required: true,
						type: "array",
						message: "请至少选择一个年级",
						trigger: "change",
					},
				],
				subjectId: [
					{
						required: true,
						type: "array",
						message: "请至少选择一个学科",
						trigger: "change",
					},
				],
				difficultyId: [
					{
						required: true,
						message: "难度不能为空",
						trigger: "change",
					},
				],
				summary: [
					{
						required: true,
						message: "请输入视频简介",
						trigger: "blur",
					},
				],
				status: [
					{
						required: true,
						message: "上架状态不能为空",
						trigger: "change",
					},
				],
				teacherId: [{ required: true, message: "请选择讲师" }],
				cover: [{ required: true, message: "请选择封面" }],
				timing: [],
				attr: [],
				commentSum: [],
				hits: [
					{
						required: true,
						message: "请输入点击量",
						trigger: "blur",
					},
				],
			},
			gradeIdOptions: [],
			subjectIdOptions: [],
			difficultyIdOptions: [],
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
			attrOptions: [
				{
					label: "是否评论",
					value: 1,
				},
				{
					label: "是否互动",
					value: 2,
				},
				{
					label: "是否推荐",
					value: 3,
				},
				{
					label: "是否弹幕",
					value: 4,
				},
				{
					label: "是否下载",
					value: 5,
				},
				{
					label: "是否投票",
					value: 6,
				},
			],
			apiListAddTemplate: {
				name: "",
				url: "",
			},
			coursewareIndex: 0,
			isLook: false,
		};
	},
	mounted() {
		this.initGrandOption();
		this.initSubjectOption();
		this.initDifficultyOption();
	},
	methods: {
		async initGrandOption() {
			const res = await this.$API.syscode.list.get({ typeCode: "grand" });
			if (res.code == 200) {
				res.data.forEach((e) => {
					this.gradeIdOptions.push({
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
					this.subjectIdOptions.push({
						label: e.name,
						value: e.id,
					});
				});
			}
		},
		async initDifficultyOption() {
			const res = await this.$API.syscode.list.get({
				typeCode: "difficulty",
			});
			if (res.code == 200) {
				res.data.forEach((e, i) => {
					if (i == 0) {
						this.formData.difficultyId = e.id;
					}
					this.difficultyIdOptions.push({
						label: e.name,
						value: e.id,
					});
				});
			}
		},
		upSuccess(res) {
			this.formData.cover = res.data.path;
			this.videoOptions.poster = res.data.path;
			if (res.code == 200) {
				this.$message.success("上传成功~");
			} else {
				this.$message.warning(res.message);
			}
		},
		coursewareSelect(index) {
			this.coursewareIndex = index;
			this.$refs.selectMaterial.open(3);
		},
		speakerSelect() {
			this.$refs.teacher.open();
		},
		teacherComplete(row) {
			this.formData.teacherId = row.id;
			this.formData.teacher = row;
		},
		coverSelect(type) {
			this.$refs.selectMaterial.open(type);
		},
		selectMaterialComplete(row, type) {
			if (type == 1) {
				this.formData.cover = this.$CONFIG.SERVER_URL + row.urls;
				this.videoOptions.poster = this.formData.cover;
			}
			if (type == 2) {
				this.formData.urls = this.$CONFIG.SERVER_URL + row.urls;
			}
			if (type == 3) {
				this.formData.courseware[this.coursewareIndex].url =
					this.$CONFIG.SERVER_URL + row.urls;
			}
		},
		async open(row, type) {
			if (type) {
				this.isLook = true;
			}
			if (!row) {
				this.mode = "add";
			} else {
				this.mode = "edit";
				var res = await this.$API.examcourse.model.get(row.id);
				this.formData = res.data;
				this.formData.type = this.formData.type + "";
			}
			this.visible = true;
		},
		save() {
			this.$refs.formRef.validate(async (valid) => {
				if (valid) {
					this.isSaveing = true;
					let res = null;
					if (this.formData.id === 0) {
						res = await this.$API.examcourse.add.post(
							this.formData
						);
					} else {
						res = await this.$API.examcourse.update.put(
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
				type: "1",
				teacherId: "",
				teacher: { name: "姓名", postName: "讲师职称", avatar: "" },
				gradeId: [],
				subjectId: [],
				title: "",
				urls: "",
				courses: [],
				cover: "",
				audit: false,
				status: 1,
				timing: undefined,
				hits: 0,
				attr: [1],
				commentSum: 0,
				dot: "",
				courseware: [],
				isDelete: false,
				summary: "",
				content: "",
			};
			this.$refs.formRef.resetFields();
			this.visible = false;
		},
	},
};
</script>
<style scoped>
>>> .el-checkbox__label {
	font-weight: normal !important;
}
>>> .el-radio__label {
	font-weight: normal !important;
}
.elem-quote {
	border-left: 3px solid #409eff;
	padding: 0px 10px;
	border-radius: 0px;
	font-size: 16px;
	margin: 20px;
}
.select-teacher >>> .el-form-item__content,
.courseware >>> .el-form-item__content {
	display: block;
}
.speaker {
	display: flex;
	border: 1px dashed #409eff;
	border-radius: 4px;
	width: 300px;
	padding: 15px;
	margin-bottom: 10px;
}
.speaker .info {
	padding-left: 20px;
}
.speaker .info p {
	line-height: normal;
	font-size: 12px;
}
.custom-empty {
	width: 100%;
	height: 100%;
	display: flex;
	flex-direction: column;
	align-items: center;
	justify-content: center;
	background: #f5f7fa;
	border: 1px dashed #409eff;
	border-radius: 5px;
}
.custom-empty i {
	font-size: 30px;
	color: #888888;
}
.custom-empty p {
	font-size: 12px;
	font-weight: normal;
	margin-top: 10px;
	color: #888888;
}
.cur-cover {
	display: flex;
}
.cover-tip {
	padding-left: 20px;
	padding-top: 40px;
	line-height: 20px;
}
>>> .el-alert--info {
	background-color: #e0f6fa;
}
>>> .el-button {
	font-weight: normal;
}
</style>
