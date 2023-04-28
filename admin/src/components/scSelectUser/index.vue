<template>
	<div class="sel-user" v-if="!hideInput">
		<div class="sel-input">
			<el-tag
				v-for="(it, index) in resUser"
				:key="index"
				type="info"
				closable
				:disable-transitions="false"
				@close="delResUser(it)"
			>
				{{ it.fullName }}
			</el-tag>
			<span v-if="resUser.length == 0" class="placeholder">{{
				placeholder
			}}</span>
		</div>
		<div class="sel-append">
			<el-button icon="el-icon-finished" @click="dialogOpen" />
		</div>
	</div>
	<el-dialog
		ref="dialog"
		:model-value="selectOpen"
		title="选择用户"
		:width="width"
		v-bind="$attrs"
		:show-close="true"
	>
		<div class="select-user">
			<div class="org-user">
				<div class="search">
					<el-input
						v-model="param.key"
						placeholder="关键字"
						:maxlength="30"
						show-word-limit
						clearable
					></el-input>
					<el-button
						type="primary"
						icon="el-icon-search"
						@click="search"
						>搜索</el-button
					>
				</div>
				<div class="org-user-box">
					<div class="org">
						<el-scrollbar height="350px">
							<el-tree
								ref="group"
								class="menu"
								node-key="id"
								default-expand-all
								:data="group"
								:current-node-key="''"
								:highlight-current="true"
								:expand-on-click-node="false"
								@node-click="groupClick"
							></el-tree>
						</el-scrollbar>
					</div>
					<div class="user">
						<el-scrollbar height="350px">
							<div
								class="item"
								v-for="(it, index) in user.items"
								:key="index"
							>
								<el-checkbox
									v-model="it.checked"
									:disabled="it.disabled"
									@change="userChange($event, it)"
									>{{ it.fullName }}</el-checkbox
								>
							</div>
						</el-scrollbar>
					</div>
				</div>
			</div>
			<div class="select-res">
				<div class="select-sum">已选（{{ resUser.length }}）</div>
				<div class="select-list">
					<el-scrollbar height="350px">
						<div
							class="item"
							v-for="(it, index) in resUser"
							:key="index"
						>
							<div class="name">
								<span>{{
									it.fullName.charAt(0).toUpperCase()
								}}</span
								>{{ it.fullName }}
							</div>
							<!-- <el-icon @click="delResUser(it)"><el-icon-close /></el-icon> -->
						</div>
					</el-scrollbar>
				</div>
			</div>
		</div>
		<div class="footer">
			<el-button @click="close">取 消</el-button>
			<el-button type="primary" @click="saveSelect"> 确 定 </el-button>
		</div>
	</el-dialog>
</template>
<script>
export default {
	emits: ["onSelect"],
	props: {
		multiple: { type: Boolean, default: true },
		multipleLimit: { type: Number, default: 0 },
		placeholder: { type: String, default: "请选择" },
		width: { type: Number, default: 750 },
		hideInput: { type: Boolean, default: false },
		modelValue: null,
		selectOpen: { type: Boolean, default: false },
		defaultValue: { type: Array, default: () => [] },
		ignore: { type: Array, default: () => [] },
	},
	watch: {
		defaultValue: {
			handler() {
				this.resUser = this.defaultValue;
			},
			deep: true,
			immediate: true,
		},
	},
	data() {
		return {
			param: {
				key: "",
				orgId: 0,
				page: 1,
				limit: 20,
			},
			group: [],
			user: {},
			resUser: [],
		};
	},
	mounted() {
		this.getGroup();
		this.initUser();
	},
	methods: {
		//加载树数据
		async getGroup() {
			this.showGrouploading = true;
			const res = await this.$API.sysorganize.list.get();
			this.showGrouploading = false;
			let _tree = [{ id: "1", value: "0", label: "所有", parentId: "0" }];
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
		async initUser() {
			const that = this;
			const res = await this.$API.sysadmin.page.get(this.param);
			if (res.code == 200) {
				this.user = res.data;
				res.data.items.forEach((item) => {
					that.resUser.forEach((row) => {
						if (item.id == row.id) {
							item.checked = true;
						}
					});
				});
				res.data.items.forEach((item) => {
					that.ignore.forEach((row) => {
						if (item.id == row) {
							item.disabled = true;
						} else {
							item.disabled = false;
						}
					});
				});
			} else {
				this.$alert(res.message, "提示", { type: "error" });
			}
		},
		search() {
			this.initUser();
		},
		dialogOpen() {
			this.$emit("update:selectOpen", true);
		},
		//树点击事件
		groupClick(data) {
			this.user = {};
			this.param.orgId = data.value;
			this.initUser();
		},
		userChange(e, m) {
			const that = this;
			this.user.items.forEach((item) => {
				if (that.multiple == false && item.id != m.id) {
					item.checked = false;
				}
				if (item.id == m.id) {
					let index = that.resUser.findIndex((ele) => {
						return ele.id == m.id;
					});
					if (e == true && index == -1) {
						//判断是否多选
						if (
							(that.multiple && that.multipleLimit == 0) ||
							(that.multiple &&
								that.multipleLimit != 0 &&
								that.resUser.length < that.multipleLimit)
						) {
							that.resUser.push(m);
						}
						if (that.multiple == false) {
							//单选
							that.resUser = [m];
						}
					} else {
						that.resUser.splice(index, 1);
					}
				}
			});
			this.$emit("update:defaultValue", this.resUser);
		},
		delResUser(m) {
			let index = this.resUser.findIndex((ele) => {
				return ele.id == m.id;
			});
			this.resUser.splice(index, 1);
			this.user.items.forEach((item) => {
				if (item.id == m.id && m.checked) {
					item.checked = false;
				}
			});
			this.$emit("update:defaultValue", this.resUser);
		},
		saveSelect() {
			if (this.resUser.length == 0) {
				this.$message.warning("请选择用户信息");
				return;
			}
			this.$emit("update:defaultValue", this.resUser);
			this.$emit("onSelect", this.resUser);
			this.close();
		},
		close() {
			this.$emit("update:selectOpen", false);
		},
	},
};
</script>
<style scoped>
.sel-user {
	display: inline-flex;
	width: 100%;
	align-items: stretch;
	height: 32px;
}
.sel-user .sel-input {
	display: inline-flex;
	flex-grow: 1;
	flex-wrap: wrap;
	align-items: center;
	padding: 1px 6px;
	background-color: var(--el-input-bg-color, var(--el-fill-color-blank));
	background-image: none;
	border-radius: var(--el-input-border-radius, var(--el-border-radius-base));
	transition: var(--el-transition-box-shadow);
	box-shadow: 0 0 0 1px var(--el-input-border-color, var(--el-border-color))
		inset;
	border-top-right-radius: 0px;
	border-bottom-right-radius: 0px;
}
.sel-user .sel-input .placeholder {
	padding-left: 5px;
	color: #a6a7ae;
	font-weight: 400;
}
.sel-user .sel-append {
	display: inline-flex;
}
.sel-user .sel-append button {
	border-left: 0px;
	border-top-left-radius: 0px;
	border-bottom-left-radius: 0px;
}
.select-user {
	display: flex;
}
.org-user .search {
	display: flex;
	align-content: center;
}
.org-user-box {
	margin-top: 15px;
	display: flex;
	border: 1px solid #e4e7ed;
}
.org-user-box .org {
	height: 350px;
	width: 220px;
}
.org-user-box .user {
	height: 350px;
	width: 220px;
	border-left: 1px solid #e4e7ed;
	padding: 5px 0;
}
.org-user-box .user .item {
	padding: 0px 10px;
}
.select-res {
	border: 1px solid #e4e7ed;
	margin-left: 20px;
	width: 240px;
}
.select-sum {
	border-bottom: 1px solid #e4e7ed;
	padding: 8px 15px;
}
.select-list {
	height: 350px;
	padding: 8px 0;
}
.select-list .item {
	padding: 8px 15px;
	display: flex;
	justify-content: space-between;
	cursor: pointer;
}
.select-list .item:hover {
	background: #f5f7fa;
}
.select-list .item .name span {
	width: 26px;
	height: 26px;
	background: cornflowerblue;
	border-radius: 50px;
	color: #ffffff;
	display: inline-block;
	text-align: center;
	line-height: 26px;
	margin-right: 5px;
}
.select-list .item .el-icon {
	position: relative;
	top: 5px;
	cursor: pointer;
}
.footer {
	text-align: right;
	padding: 15px 15px 0 0;
}
</style>
>
