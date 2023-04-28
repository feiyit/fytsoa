<template>
	<el-container>
		<el-aside width="220px" v-loading="showGrouploading">
			<el-container>
				<el-header>
					<el-select
						v-model="tenant"
						placeholder="请选择租户"
						clearable
						filterable
						@change="tenantChange"
					>
						<el-option
							v-for="item in tenantOptions"
							:key="item.value"
							:label="item.label"
							:value="item.value"
						/>
					</el-select>
				</el-header>
				<el-main class="nopadding">
					<el-tree
						ref="group"
						class="menu"
						node-key="id"
						default-expand-all
						:data="group"
						:current-node-key="''"
						:highlight-current="true"
						:expand-on-click-node="false"
						:filter-node-method="groupFilterNode"
						@node-click="groupClick"
					></el-tree>
				</el-main>
			</el-container>
		</el-aside>
		<el-container style="position: relative" v-loading="menuloading">
			<el-main class="nopadding">
				<el-tabs class="tabs-pages" stretch @tab-click="tabchange">
					<el-tab-pane lazy>
						<template #label>
							<span class="custom-tabs-label">
								<el-icon><el-icon-document-copy /></el-icon>
								<span>菜单权限</span>
							</span>
						</template>
						<el-main style="height: calc(100vh - 195px)">
							<el-scrollbar>
								<el-tree
									ref="tree"
									class="menu"
									:data="menuTree"
									show-checkbox
									node-key="number"
									default-expand-all
									:render-content="renderContent"
								></el-tree>
							</el-scrollbar>
						</el-main>
					</el-tab-pane>
				</el-tabs>
			</el-main>
			<div class="footer-btn">
				<el-button
					type="primary"
					:disabled="btnDisable"
					:loading="isSaveing"
					round
					v-auth="'tenantauthority:add'"
					@click="saveAuthorize"
				>
					保存授权
				</el-button>
			</div>
			<!-- <modify ref="modify" @complete="complete" /> -->
		</el-container>
	</el-container>
</template>
<script>
export default {
	components: {},
	data() {
		return {
			tenant: -1,
			tenantOptions: [
				{
					value: -1,
					label: "请选择租户",
				},
			],
			showGrouploading: false,
			groupFilterText: "",
			group: [],
			tabIndex: 0,
			menuTree: [],
			numberMenu: [],
			selectRole: { id: "" },
			menuloading: false,
			loading: false,
			btnDisable: true,
			form: {
				console: "work",
				list: [
					{ name: "工作台", code: "work" },
					{ name: "数据中心", code: "stats" },
				],
			},
		};
	},
	watch: {
		groupFilterText(val) {
			this.$refs.group.filter(val);
		},
	},
	mounted() {
		this.initTenant();
		this.initMenu();
	},
	methods: {
		async initTenant() {
			const res = await this.$API.systenant.page.get({ limit: 1000 });
			res.data.items.some((m) => {
				if (m.tenantId != 0) {
					this.tenantOptions.push({
						value: m.tenantId,
						label: m.name,
					});
				}
			});
		},
		//加载树数据
		async getGroup(tenant) {
			this.showGrouploading = true;
			const res = await this.$API.sysrole.tenantlist.get({
				tenantId: tenant,
			});
			this.showGrouploading = false;
			let _tree = [];
			res.data.some((m) => {
				_tree.push({
					id: m.id,
					value: m.id,
					label: m.name,
					parentId: m.parentId,
				});
			});
			this.group = this.$TOOL.changeTree(_tree);
		},
		async initMenu() {
			this.menuloading = true;
			var resMenu = await this.$API.sysmenu.list.get({ tenantId: 1 });
			this.menuloading = false;
			let menutree = [];
			resMenu.data.some((m) => {
				menutree.push({
					id: m.id,
					number: this.$TOOL.uuid(),
					value: m.id,
					label: m.name,
					parentId: m.parentId,
				});
				m.api.some((r) => {
					menutree.push({
						id: 0,
						number: this.$TOOL.uuid(),
						value: r.url,
						label: r.code,
						method: r.method,
						parentId: m.id,
					});
				});
			});
			this.numberMenu = menutree;
			this.menuTree = this.$TOOL.changeTree(menutree);
			this.changeCss();
		},
		renderContent(h, { node }) {
			let classname = "";
			if (node.childNodes.length === 0) {
				classname = "floatRight";
			} else if (node.childNodes.length > 0) {
				classname = "clearFloat";
			}
			return (
				<span class="el-tree-node__label" class={classname}>
					{node.label}
				</span>
			);
		},
		tenantChange() {
			console.log("tenantId", this.tenant);
			if (this.tenant != -1) {
				this.getGroup(this.tenant);
			}
		},
		changeCss() {
			this.$nextTick(() => {
				var levelName = document.getElementsByClassName("floatRight");
				for (var i = 0; i < levelName.length; i++) {
					let parentNode = levelName[i].parentNode;
					parentNode.style.cssFloat = "left";
					parentNode.style.styleFloat = "left";
				}
				var clearFloat = document.getElementsByClassName("clearFloat");
				for (var j = 0; j < clearFloat.length; j++) {
					let parentNode = clearFloat[j].parentNode;
					parentNode.style.clear = "both";
					parentNode.style.clear = "both";
				}
			});
		},
		//树过滤
		groupFilterNode(value, data) {
			if (!value) return true;
			return data.label.indexOf(value) !== -1;
		},
		//树点击事件
		async groupClick(data) {
			this.$refs.tree.setCheckedNodes([]);
			if (!data.children) {
				this.selectRole = data;
				this.btnDisable = false;
				const res = await this.$API.syspermission.role.get(data.id);
				const that = this;
				if (res.code == 200) {
					res.data.forEach((item) => {
						that.numberMenu
							.filter((m) => m.id == item.menuId && m.id != 0)
							.forEach((st) => {
								that.$refs.tree.setChecked(
									st.number,
									true,
									false
								);
							});
						if (item.api.length > 0) {
							let apiArr = that.numberMenu.filter(
								(m) => m.parentId == item.menuId && m.id == 0
							);
							item.api.forEach((row) => {
								apiArr.forEach((it) => {
									if (
										row.label == it.code &&
										row.method == it.method &&
										row.value == it.url
									) {
										that.$refs.tree.setChecked(
											it.number,
											true,
											false
										);
									}
								});
							});
						}
					});
				} else {
					this.$message.error(res.message);
				}
				//this.$refs.tree.setCheckedNodes(currentArr);
			} else {
				this.selectRole = {};
				this.btnDisable = true;
			}
		},
		tabchange(tab) {
			this.tabIndex = tab.index;
		},
		async saveAuthorize() {
			if (this.tabIndex == 1) {
				this.saveConsole();
				return;
			}
			var nodes = this.$refs.tree.getCheckedNodes();
			let childArr = this.$refs.tree.getHalfCheckedNodes();
			nodes = nodes.concat(childArr);
			if (!this.selectRole.id) {
				this.$message.warning("请选择要授权的角色信息~");
				return;
			}
			if (nodes.length == 0) {
				this.$message.warning("请勾选授权的菜单信息~");
				return;
			}
			let _menus = [];
			nodes.forEach((item) => {
				if (item.id != 0) {
					let _api = [];
					var apiArr = nodes.filter(
						(m) => item.id == m.parentId && m.id == 0
					);
					apiArr.forEach((row) => {
						_api.push({
							code: row.label,
							method: row.method,
							url: row.value,
						});
					});
					_menus.push({ menuId: item.id, api: _api });
				}
			});
			var data = { roleId: this.selectRole.id, menus: _menus };
			this.loading = true;
			const res = await this.$API.syspermission.plusRole.post(data);
			this.loading = false;
			if (res.code == 200) {
				this.$message.success("授权成功");
			} else {
				this.$message.console.error(res.message);
			}
		},
		saveConsole() {},
		complete() {
			this.$refs.table.refresh();
		},
	},
};
</script>
<style>
.tabs-pages {
	display: flex;
	flex-flow: column;
	flex-shrink: 0;
	height: 100%;
}
.tabs-pages > .el-tabs__header {
	margin: 0;
}
.tabs-pages > .el-tabs__header .el-tabs__nav-wrap {
	display: flex;
	/* justify-content: center; */
	padding-left: 25px;
}
.tabs-pages > .el-tabs__header .el-tabs__item {
	height: 60px;
	line-height: 60px;
	font-size: 14px;
}
.tabs-pages > .el-tabs__content {
	overflow-x: hidden;
	overflow: auto;
}
.tabs-pages .custom-tabs-label {
	padding: 5px 10px 7px 10px;
	border-radius: 6px;
}
.tabs-pages .custom-tabs-label .el-icon {
	vertical-align: middle;
}
.tabs-pages .custom-tabs-label span {
	vertical-align: middle;
	margin-left: 4px;
}
.tabs-pages .is-active .custom-tabs-label {
	background: #edf5ff;
}
.footer-btn {
	position: absolute;
	left: 0px;
	right: 0px;
	text-align: center;
	z-index: 10;
	bottom: 0px;
	border-top: 1px solid #e6e7e8;
	padding: 10px;
	background: #ffffff;
}
</style>
