<template>
	<el-card shadow="hover" header="服务器信息" class="item-background">
		<el-table :data="data" border>
			<el-table-column prop="key" label="名称" width="130" />
			<el-table-column prop="value" label="配置信息" />
		</el-table>
	</el-card>
</template>

<script>
export default {
	title: "服务器信息",
	icon: "el-icon-setting",
	description: "服务器可展示基础配置信息",
	data() {
		return {
			data: [],
		};
	},
	mounted(){
		//this.init();
	},
	methods: {
		async init(){
			const res=await this.$API.workbench.basic.get();
			this.data=res.data;
		}
	}
};
</script>

<style scoped>
.item-background p {
	color: #999;
	margin-top: 10px;
	line-height: 1.8;
}
</style>
