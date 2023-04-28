<template>
	<div>
		<div class="notice-main-title">{{ model.title }}</div>
		<div class="notice-main-user">
			<div class="user-main">
				<el-avatar
					:src="resHead(model.sendUser?.headPic)"
					size="default"
				></el-avatar>
				<div class="user-role">
					<h4>{{model.sendUser?.fullName}}</h4>
					<p>{{model.sendUser?.loginAccount}}</p>
				</div>
			</div>
			<div class="user-time">
				<el-button icon="el-icon-back" size="small" @click="reply" type="primary"
					>回复</el-button
				>
				<p>{{ model.createTime }}</p>
			</div>
		</div>
		<div class="notice-file">
			<el-alert title="附件" type="info" :closable="false">
				<p v-for="(it,index) in model.files" :key="index"><a :href="it.url" target="_blank">{{it.name}}</a></p>
			</el-alert>
		</div>
		<div class="notice-context" v-html="model.content"></div>
	</div>
</template>
<script>
export default {
	components: {},
	props: {
		model: {
			type: Object,
			default: () => {
				return { title: "", content: "" };
			},
		},
	},
	data() {
		return {};
	},
	mounted() {
		
	},
	methods: {
		reply(){
			this.$emit("reply",this.model.sendUser);
		},
		resHead(img) {
			if (img && img.indexOf("http") == -1) {
				img = this.$CONFIG.SERVER_URL + img;
			}
			return img;
		},
	},
};
</script>
<style scoped>
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
	padding: 10px 15px;
}
.notice-context {
	padding: 10px 20px;
}
</style>
