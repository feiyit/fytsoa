<template>
	<sc-dialog
		v-model="visible"
		show-fullscreen
		title="租户详情"
		width="1000px"
		destroy-on-close
		@close="close"
	>
		<el-tabs v-model="activeName" tab-position="left">
			<el-tab-pane label="账号信息" name="first">
				<admin ref="admin" :tenant="model.tenantId" />
			</el-tab-pane>
			<el-tab-pane label="机构信息" name="second">
				<organize ref="organize" :tenant="model.tenantId" />
			</el-tab-pane>
			<el-tab-pane label="角色信息" name="third">
				<role ref="role" :tenant="model.tenantId" />
			</el-tab-pane>
			<el-tab-pane label="岗位信息" name="fourth">
				<post ref="post" :tenant="model.tenantId" />
			</el-tab-pane>
		</el-tabs>
	</sc-dialog>
</template>
<script>
import { defineAsyncComponent } from "vue";
export default {
	components: {
		admin: defineAsyncComponent(() => import("./components/admin")),
		organize: defineAsyncComponent(() => import("./components/organize")),
		role: defineAsyncComponent(() => import("./components/role")),
		post: defineAsyncComponent(() => import("./components/post")),
	},
	data() {
		return {
			visible: false,
			activeName: "first",
			model: {},
		};
	},
	mounted() {},
	methods: {
		async open(row) {
			console.log("row", row);
			this.model = row;
			this.visible = true;
		},
		close() {
			this.visible = false;
		},
	},
};
</script>
<style scoped>
>>> .el-dialog__body {
	padding: 10px 15px 0px 15px;
}
</style>
