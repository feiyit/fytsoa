<template>
	<el-card shadow="hover" header="欢迎">
		<div class="welcome">
			<div class="logo">
				<img src="img/logo.png" />
				<h2>欢迎进入 FytSoa SPMS</h2>
			</div>
			<div class="tips">
				<div class="tips-item">
					<div class="tips-item-icon">
						<el-icon><el-icon-menu /></el-icon>
					</div>
					<div class="tips-item-message">
						这里是项目控制台，你可以点击右上方的“自定义”按钮来添加移除或者移动部件。
					</div>
				</div>
				<div class="tips-item">
					<div class="tips-item-icon">
						<el-icon><el-icon-warning /></el-icon>
					</div>
					<div class="tips-item-message">
						您可以在此快速构建您的应用，维护应用数据并设置系统相关操作。
					</div>
				</div>
				<div class="tips-item">
					<div class="tips-item-icon">
						<el-icon><el-icon-position /></el-icon>
					</div>
					<div class="tips-item-message">
						<el-row :gutter="0" class="box">
							<el-col
								:span="4"
								v-for="(it, index) in lnk"
								:key="index"
								@click="goUrl(it)"
							>
								<div class="icon-box">
									<el-icon
										><component :is="it.icon"
									/></el-icon>
									<p>{{ it.name }}</p>
								</div>
							</el-col>
						</el-row>
					</div>
				</div>
			</div>
			<div class="actions">
				<el-button
					type="primary"
					icon="el-icon-check"
					size="large"
					@click="godoc"
					>联系我们</el-button
				>
			</div>
		</div>
	</el-card>
</template>

<script>
export default {
	title: "欢迎",
	icon: "el-icon-present",
	description: "项目特色以及文档链接",
	data() {
		return {
			lnk: [
				{
					icon: "el-icon-setting",
					name: "系统设置",
					url: "/sys/safety",
				},
				{
					icon: "el-icon-document-copy",
					name: "数据字典",
					url: "/sys/code",
				},
				{
					icon: "el-icon-document",
					name: "文章管理",
					url: "/cms/article",
				},
				{ icon: "el-icon-avatar", name: "会员管理", url: "/user/list" },
				{
					icon: "el-icon-histogram",
					name: "投票管理",
					url: "/modules/vote",
				},
				{
					icon: "el-icon-timer",
					name: "任务调度",
					url: "/modules/task",
				},
			],
		};
	},
	methods: {
		godoc() {
			window.open("https://gitee.com/feiyit/fytsoa");
		},
		goUrl(m) {
			this.$router.replace({
				path: m.url,
			});
		},
	},
};
</script>

<style scoped>
.welcome .logo {
	text-align: center;
}
.welcome .logo img {
	vertical-align: bottom;
	width: 100px;
	height: 100px;
	margin-bottom: 20px;
}
.welcome .logo h2 {
	font-size: 30px;
	font-weight: normal;
	display: flex;
	align-items: center;
	justify-content: center;
}

.tips {
	margin-top: 20px;
	padding: 0 40px;
}
.tips-item {
	display: flex;
	align-items: center;
	justify-content: center;
	padding: 7.5px 0;
}
.tips-item-icon {
	width: 40px;
	height: 40px;
	display: flex;
	align-items: center;
	justify-content: center;
	border-radius: 50%;
	font-size: 18px;
	margin-right: 20px;
	color: var(--el-color-primary);
	background: rgba(180, 180, 180, 0.1);
}
.tips-item-message {
	flex: 1;
	font-size: 14px;
}

.actions {
	text-align: center;
	margin: 40px 0 20px 0;
}
.box {
	border-top: 1px solid #eee;
	border-left: 1px solid #eee;
}
.icon-box {
	text-align: center;
	background: #fff;
	border-bottom: 1px solid #eee;
	border-right: 1px solid #eee;
	color: #666;
	padding: 10px 10px;
	cursor: pointer;
}
.icon-box i {
	font-size: 22px;
	transition: color 0.15s linear;
}
.icon-box p {
	color: #999;
	margin-top: 2px;
	transition: color 0.15s linear;
}
.icon-box:hover i,
.icon-box:hover p {
	color: #5cb6ff;
}
html.dark .icon-box {
	border-bottom: 1px solid #555657;
	border-right: 1px solid #555657;
	background: #2f2f2f;
}
html.dark .box {
	border-top: 1px solid #555657;
	border-left: 1px solid #555657;
}
html.dark .icon-box p {
	color: #ffffff;
}
html.dark .el-icon {
	color: #ffffff;
}
</style>
