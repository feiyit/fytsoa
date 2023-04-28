<template>
	<el-container>
		<el-aside width="280px" v-loading="showGrouploading">
			<el-header>
				<el-input
					placeholder="输入关键字进行过滤"
					v-model="groupFilterText"
					clearable
				></el-input>
				<el-button
					type="primary"
					round
					icon="el-icon-plus"
					class="add-column"
					v-auth="'cmssite:add'"
					@click="addSite"
				></el-button>
			</el-header>
			<el-main class="nopadding">
				<el-tree
					ref="group"
					class="menu"
					node-key="id"
					default-expand-all
					:data="group"
					:filter-node-method="groupFilterNode"
					@node-click="groupClick"
				>
					<template #default="{ node, data }">
						<span class="custom-tree-node">
							<span class="label"
								><el-icon><el-icon-Basketball /></el-icon
								>{{ node.label }}</span
							>
							<span class="do">
								<el-icon
									@click.stop="remove(data)"
									v-auth="'cmssite:delete'"
									><el-icon-delete
								/></el-icon>
							</span>
						</span>
					</template>
				</el-tree>
			</el-main>
		</el-aside>
		<el-container>
			<el-main class="site-main">
				<modify ref="modify" @complete="init" />
			</el-main>
		</el-container>
	</el-container>
</template>
<script>
import { defineAsyncComponent } from "vue";
export default {
	components: {
		modify: defineAsyncComponent(() => import("./modify")),
	},
	data() {
		return {
			showGrouploading: false,
			groupFilterText: "",
			group: [],
		};
	},
	watch: {
		groupFilterText(val) {
			this.$refs.group.filter(val);
		},
	},
	mounted() {
		this.init();
	},
	methods: {
		async init() {
			this.showGrouploading = true;
			const res = await this.$API.cmssite.list.get();
			this.showGrouploading = false;
			let _tree = [];
			res.data.some((m) => {
				_tree.push({
					id: m.id,
					value: m.id,
					label: m.name,
					parentId: "0",
				});
			});
			this.group = this.$TOOL.changeTree(_tree);
		},
		//树过滤
		groupFilterNode(value, data) {
			if (!value) return true;
			return data.label.indexOf(value) !== -1;
		},
		//树点击事件
		groupClick(data) {
			this.$refs.modify.open(data);
		},
		addSite() {
			this.$refs.modify.open();
		},
		async remove(data) {
			if (this.group.length == 1) {
				this.$message.warning("最少保留一条站点信息");
				return;
			}
			this.$confirm(`确定删除选中的 ${data.label} 站点吗？`, "提示", {
				type: "warning",
				confirmButtonText: "确定",
				cancelButtonText: "取消",
			})
				.then(async () => {
					var res = await this.$API.cmssite.delete.delete([data.id]);
					if (res.code == 200) {
						this.$refs.modify.open();
						this.init();
						this.$message.success("删除成功");
					} else {
						this.$alert(res.message, "提示", { type: "error" });
					}
				})
				.catch(() => {});
		},
	},
};
</script>
<style>
.custom-tree-node {
	display: flex;
	flex: 1;
	align-items: center;
	justify-content: space-between;
	font-size: 14px;
	padding-right: 24px;
	height: 100%;
}
.custom-tree-node .label i {
	font-size: 18px;
	position: relative;
	top: 4px;
	margin-right: 5px;
}
.custom-tree-node .do {
	display: none;
}
.custom-tree-node .do i {
	margin-left: 5px;
	color: #999;
}
.custom-tree-node .do i:hover {
	color: #333;
}
.custom-tree-node:hover .do {
	display: inline-block;
}
.add-column {
	padding: 8px !important;
	margin: 8px;
}
.site-main {
	background: #ffffff;
}
</style>
