<template>
	<div>
		<div class="erceipt">
			<el-button @click="openSelectUser">收件人</el-button>
			<div class="erceipt-list">
				<div class="item" v-for="(it, index) in erceipt" :key="index">
					<span>{{ it.fullName }}</span>
					<el-icon @click="delErceipt(it)"><el-icon-close /></el-icon>
				</div>
			</div>
		</div>
		<div class="notice-main-title">
			<el-input
				v-model="formData.title"
				placeholder="通知主题"
				:maxlength="30"
				show-word-limit
				clearable
				:style="{ width: '100%' }"
			></el-input>
		</div>

		<div class="notice-file">
			<sc-upload-file
				v-model="formData.files"
				:apiObj="uploadApi"
				:limit="3"
				:maxSize="100"
				:onSuccess="upSuccess"
				tip="最多上传3个文件,单个文件不要超过100M"
			>
				<el-button type="primary" icon="el-icon-upload"
					>上传附件</el-button
				>
			</sc-upload-file>
		</div>
		<div class="notice-context">
			<sc-editor
				v-model="formData.content"
				placeholder="请输入"
				:height="340"
			></sc-editor>
		</div>
		<div class="notice-btn">
			<el-button-group>
				<el-button
					icon="el-icon-position"
					@click="save(0)"
					:loading="isSaveing"
					type="primary"
					>发送</el-button
				>
				<el-button
					icon="el-icon-finished"
					@click="save(1)"
					:loading="isSaveing"
					type="primary"
					>存草稿</el-button
				>
			</el-button-group>
		</div>
		<select-user
			hide-input
			v-model:selectOpen="isOpenUser"
			@onSelect="selectUserRes"
		></select-user>
	</div>
</template>
<script>
import { defineAsyncComponent } from "vue";
const scEditor = defineAsyncComponent(() => import("@/components/scEditor"));
const selectUser = defineAsyncComponent(() =>
	import("@/components/scSelectUser")
);
export default {
	components: {
		scEditor,
		selectUser,
	},
	props: {
		model: {
			type: Object,
			default: () => {
				return { title: "", content: "", user: null };
			},
		},
	},
	watch: {
		model: {
			handler(newV) {
				this.clearForm();
				//回复
				if (newV.user && newV.id == 0) {
					this.erceipt.push(newV.user);
				}
				//草稿修改
				if (!newV.user && newV.id != 0) {
					this.erceipt = newV.acceptUserList;
					this.formData = newV;
				}
			},
			deep: true,
		},
	},
	data() {
		return {
			formData: {
				id: 0,
				title: "",
				content: "",
				acceptUserIds: [],
				files: [],
			},
			uploadApi: this.$API.sysfile.notice,
			isOpenUser: false,
			erceipt: [],
			isSaveing: false,
		};
	},
	mounted() {},
	methods: {
		openSelectUser() {
			if (this.isOpenUser) {
				this.isOpenUser = false;
			}
			this.isOpenUser = true;
		},
		selectUserRes(res) {
			this.isOpenUser = false;
			this.erceipt = res;
		},
		delErceipt(m) {
			let index = this.erceipt.findIndex((ele) => {
				return ele.id == m.id;
			});
			this.erceipt.splice(index, 1);
		},
		upSuccess(res) {
			if (res.code == 200) {
				this.$message.success("上传成功~");
				this.formData.files.push({
					name: res.data.name,
					url: res.data.path,
				});
			} else {
				this.$message.warning(res.message);
			}
		},
		async save(status) {
			if (this.erceipt.length == 0) {
				this.$message.warning("请选择收件人~");
				return;
			}
			if (!this.formData.title) {
				this.$message.warning("请输入通知主题~");
				return;
			}
			if (!this.formData.content) {
				this.$message.warning("请输入内容~");
				return;
			}
			this.isSaveing = true;
			this.formData.acceptUserIds = this.erceipt.map((item) => {
				return item.id;
			});
			this.formData.status = status;
			let res = null;
			if (this.formData.id === 0) {
				res = await this.$API.sysnotice.add.post(this.formData);
			} else {
				res = await this.$API.sysnotice.update.put(this.formData);
			}
			this.isSaveing = false;
			if (res.code == 200) {
				this.$emit("addComplete");
				this.clearForm();
				this.$message.success("保存成功");
			} else {
				this.$alert(res.message, "提示", { type: "error" });
			}
		},
		clearForm() {
			this.erceipt = [];
			this.formData = {
				id: 0,
				title: "",
				content: "",
				acceptUserIds: [],
				files: [],
			};
		},
	},
};
</script>
<style scoped>
.erceipt {
	padding: 10px 15px 0px 15px;
	display: flex;
	align-content: center;
}
.erceipt .erceipt-list {
	flex: 1;
	display: flex;
	align-content: center;
	border-bottom: 1px solid #e6e7e8;
}
.erceipt .erceipt-list .item {
	line-height: 28px;
	height: 28px;
	padding: 0 10px;
	background: #f2f6fc;
	border-radius: 5px;
	margin-left: 10px;
}
.erceipt .erceipt-list .item .el-icon {
	margin-left: 10px;
	position: relative;
	top: 2px;
	cursor: pointer;
}
.notice-main-title {
	border-bottom: 1px solid #e6e7e8;
	padding: 15.5px;
	font-size: 18px;
}
.notice-main-user {
	padding: 15px 10px;
}
.notice-main-user,
.notice-main-user .user-main {
	display: flex;
}
.notice-main-user .user-main {
	flex: 1;
}
.notice-main-user .user-role {
	margin-left: 10px;
}
.notice-main-user .user-time {
	text-align: right;
}
.notice-main-user .user-time p {
	margin-top: 5px;
}
.notice-file {
	padding: 10px 15px 0px 15px;
}
.notice-context {
	padding: 10px 15px;
}
.notice-btn {
	padding: 0 15px;
}
</style>
