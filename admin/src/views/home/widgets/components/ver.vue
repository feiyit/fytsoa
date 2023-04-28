<template>
	<el-card shadow="hover" header="版本信息">
		<div class="main-version" style="text-align: center">
			<img src="img/ver.svg" style="height: 100px" />
			<h2 style="margin-top: 15px">FytSoa {{ $CONFIG.CORE_VER }}</h2>
			<p>【新增】控制台可自定义编辑模块</p>
			<p>【新增】站点管理模块，支持配置多个站点</p>
			<p>【新增】投票模块完成，支持投票项自定义编辑，统计投票信息</p>
			<p>
				【新增】授权增加API授权，服务端并验证是否有权限调用API，基本实现服务端权限至按钮级别
			</p>
			<p>【优化】个人信息，支持修改</p>
			<p>【优化】表格组件，增加打印和导出功能（列可自定义）</p>
			<p>【更新】升级VueCli 5，使用webpack 5</p>
			<p>
				【新增】.env.development .env.production
				调整本地环境和生产环境模式
			</p>
			<p>
				【优化】资源管理器升级上传组件，完成文件类型搜索以及刷新和复制链接功能
			</p>
		</div>
	</el-card>
</template>

<script>
export default {
	title: "版本信息",
	icon: "el-icon-monitor",
	description: "当前项目版本信息",
	data() {
		return {
			ver: "loading...",
		};
	},
	mounted() {
		this.getVer();
	},
	methods: {
		async getVer() {
			// const ver = await this.$API.demo.ver.get()
			// this.ver = ver.data
		},
		golog() {
			window.open("");
		},
		gogit() {
			window.open("");
		},
	},
};
</script>
<style>
.main-version h2 {
	margin-bottom: 20px;
}
.main-version p {
	text-align: left;
	margin: 5px 0;
	color: #666;
	line-height: 1.8;
}
html.dark .main-version p {
	color: #ffffff;
}
</style>
