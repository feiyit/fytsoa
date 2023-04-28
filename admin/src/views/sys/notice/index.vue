<template>
	<el-container>
		<el-header>
			<div class="left-panel">
				<h3 class="notice-ft">文件夹</h3>
				<el-button
					icon="el-icon-edit"
					type="primary"
					v-auth="'sysnotice:add'"
					@click="addNew"
					>新建通知</el-button
				>
				<el-button
					icon="el-icon-files"
					type="primary"
					:disabled="list.length == 0 || !isReceipt"
					v-auth="'sysnotice:readok'"
					@click="setRead"
					>全部标记已读</el-button
				>
				<el-button
					icon="el-icon-delete"
					plain
					type="danger"
					:disabled="selection.length == 0"
					@click="batchDel"
				/>
				<el-button
					icon="el-icon-files"
					type="info"
					:disabled="selection.length == 0"
					v-auth="'sysnotice:status'"
					@click="archives"
					>存档</el-button
				>
			</div>
			<div class="right-panel">
				<div class="right-panel-search">
					<el-input
						v-model="param.key"
						:indeterminate="isIndeterminateModel"
						clearable
						placeholder="关键字"
					/>
					<el-button
						icon="el-icon-search"
						type="primary"
						@click="search"
					/>
				</div>
			</div>
		</el-header>
		<el-container class="notice-container">
			<el-aside width="240px">
				<el-menu default-active="1">
					<el-menu-item index="1" @click="searchType(2)">
						<el-icon><el-icon-message /></el-icon>
						<span>收件箱</span>
						<el-tag type="plain" round v-if="total.unread > 0">{{
							total.unread
						}}</el-tag>
					</el-menu-item>
					<el-menu-item index="2" @click="searchType(1)">
						<el-icon><el-icon-position /></el-icon>
						<span>发件箱</span>
					</el-menu-item>
					<el-menu-item index="3" @click="searchStatus(1)">
						<el-icon><el-icon-finished /></el-icon>
						<span>草稿</span>
						<el-tag type="plain" round v-if="total.draft > 0">{{
							total.draft
						}}</el-tag>
					</el-menu-item>
					<el-menu-item index="4" @click="searchStatus(3)">
						<el-icon><el-icon-delete /></el-icon>
						<span>已删除</span>
						<el-tag type="plain" round v-if="total.delete > 0">{{
							total.delete
						}}</el-tag>
					</el-menu-item>
					<el-menu-item index="5" @click="searchStatus(2)">
						<el-icon><el-icon-takeaway-box /></el-icon>
						<span>存档</span>
						<el-tag type="plain" round v-if="total.archive > 0">{{
							total.archive
						}}</el-tag>
					</el-menu-item>
					<el-divider>
						<el-icon><el-icon-star-filled /></el-icon>
					</el-divider>
					<el-menu-item index="6" @click="searchRead(0)">
						<el-icon><el-icon-files /></el-icon>
						<span>全部</span>
					</el-menu-item>
					<el-menu-item index="7" @click="searchRead(2)">
						<el-icon><el-icon-message-box /></el-icon>
						<span>已读</span>
					</el-menu-item>
					<el-menu-item index="8" @click="searchRead(1)">
						<el-icon><el-icon-message /></el-icon>
						<span>未读</span>
					</el-menu-item>
				</el-menu>
			</el-aside>
			<el-aside width="320px">
				<div class="notice-list-title">
					<el-checkbox
						v-model="selectRadio"
						size="large"
						@change="checkChange"
					/>
					<el-dropdown
						@command="handleDropdown"
						:disabled="!isReceipt"
					>
						<span class="el-dropdown-link">
							<el-icon><el-icon-filter /></el-icon> 筛选器
						</span>
						<template #dropdown>
							<el-dropdown-menu>
								<el-dropdown-item command="0"
									><el-icon><el-icon-files /></el-icon
									>全部</el-dropdown-item
								>
								<el-dropdown-item command="2"
									><el-icon><el-icon-message-box /></el-icon
									>已读</el-dropdown-item
								>
								<el-dropdown-item command="1"
									><el-icon><el-icon-message /></el-icon
									>未读</el-dropdown-item
								>
								<el-dropdown-item command="3"
									><el-icon><el-icon-link /></el-icon
									>带文件</el-dropdown-item
								>
							</el-dropdown-menu>
						</template>
					</el-dropdown>
				</div>
				<div class="notice-list-wall">
					<div
						class="notice-item"
						v-for="(it, index) in list"
						:key="index"
						:class="it.checked ? 'active' : ''"
						@click="goInfo(it)"
					>
						<div class="item-avatar">
							<el-avatar
								:src="resHead(it.sendUser?.avatar)"
								size="small"
							></el-avatar>
							<el-checkbox
								size="large"
								v-model="it.checked"
								@change="itemChange(it)"
							/>
						</div>
						<div class="item-info">
							<div class="item-uname">
								<h3 class="uname">
									{{ it.sendUser?.fullName }}-（{{
										it.sendUser?.loginAccount
									}}）
								</h3>
								<div class="item-tool">
									<el-tooltip
										:content="
											it.isRead
												? '标记为未读'
												: '标记为已读'
										"
										placement="top"
									>
										<el-icon @click.stop="setSignRead(it)"
											><el-icon-message /></el-icon
									></el-tooltip>
									<el-tooltip
										:content="
											it.status == 0
												? '标记为存档'
												: '取消存档'
										"
										placement="top"
									>
										<el-icon @click.stop="setArchive(it)"
											><el-icon-takeaway-box
										/></el-icon>
									</el-tooltip>
								</div>
							</div>
							<div class="title-time">
								<h3>
									{{ it.title }}
								</h3>
								<div class="time">
									{{ resTime(it.createTime) }}
								</div>
							</div>
							<p>
								{{ subStr(it.content) }}
							</p>
						</div>
						<div class="item-del" @click="tableDel(it)">
							<el-icon><el-icon-delete /></el-icon>
						</div>
					</div>
					<el-result icon="info" title="" v-if="list.length == 0">
						<template #sub-title>
							<p>空空如也~</p>
						</template>
					</el-result>
				</div>
			</el-aside>
			<el-main class="nopadding notice-main">
				<div v-show="lookType == 0" class="default-main">
					<el-icon><el-icon-message /></el-icon>
					<p>请选择左侧通知内容查看~</p>
				</div>
				<info
					ref="info"
					v-show="lookType == 1"
					:model="infoModel"
					@reply="goReply"
				/>
				<add
					ref="add"
					v-show="lookType == 2"
					:model="infoModel"
					@addComplete="sendComplete"
				/>
			</el-main>
		</el-container>
	</el-container>
</template>
<script>
import { defineAsyncComponent } from "vue";
export default {
	components: {
		info: defineAsyncComponent(() => import("./components/info")),
		add: defineAsyncComponent(() => import("./components/add")),
	},
	data() {
		return {
			apiObj: this.$API.sysnotice.page,
			list: [],
			total: {},
			param: {
				key: "",
				page: 1,
				status: 0,
				type: 2,
				readStatus: 0,
				limit: 20,
			},
			infoModel: { id: 0 },
			lookType: 0, // 0=默认 1=详情 2=创建
			isReceipt: true,
			selectRadio: false,
			selection: [],
			isIndeterminate: false,
		};
	},
	mounted() {
		this.init();
		this.initTotal();
	},
	methods: {
		async init() {
			var res = await this.$API.sysnotice.page.get(this.param);
			this.list = res.data.items;
		},
		async initTotal() {
			var res = await this.$API.sysnotice.total.get();
			this.total = res.data;
		},
		//查询详情
		async goInfo(m) {
			let param = m.id;
			if (!m.isRead && m.status == 0 && m.isSend == false) {
				param += "/1";
			}
			var res = await this.$API.sysnotice.model.get(param);
			this.infoModel = res.data;
			//发件箱
			if (m.status == 0 && m.isSend == true) {
				this.lookType = 1;
			}
			//收件箱
			if (m.status == 0 && m.isSend == false) {
				this.lookType = 1;
				this.initTotal();
			}
			//草稿
			if (m.status == 1 && m.isSend == true) {
				this.lookType = 2;
			}
		},
		//新建通知
		addNew() {
			this.lookType = 2;
			this.infoModel = { id: 0, user: null };
		},
		//回复通知
		goReply(user) {
			this.lookType = 2;
			this.infoModel = { id: 0, user: user };
		},
		//添加完成通知
		sendComplete() {
			this.initTotal();
		},
		searchStatus(status) {
			this.param.status = status;
			this.param.type = 0;
			this.param.readStatus = 0;
			this.isReceipt = false;
			this.lookType = 0;
			this.init();
		},
		searchRead(read) {
			this.param.readStatus = read;
			this.param.status = 0;
			this.param.type = 2;
			this.lookType = 0;
			this.init();
		},
		searchType(type) {
			this.isReceipt = type == 2 ? true : false;
			this.param.readStatus = 0;
			this.param.status = 0;
			this.param.type = type;
			this.lookType = 0;
			this.init();
		},
		handleDropdown(number) {
			this.param.status = 0;
			if (number == 3) {
				this.param.type = number;
			} else {
				this.param.readStatus = number;
				this.param.type = 2;
			}
			this.lookType = 0;
			this.init();
		},
		delHtmlTag(str) {
			return str.replace(/<[^>]+>/g, "");
		},
		subStr(str, length) {
			const s = this.delHtmlTag(str);
			const ss = s.substr(0, length);
			return ss;
		},
		resHead(img) {
			if (img && img.indexOf("http") == -1) {
				img = this.$CONFIG.SERVER_URL + img;
			}
			return img;
		},
		resTime(t) {
			const date = new Date(t);
			const month = date.getMonth() + 1;
			const strDate = date.getDate();
			return month + "月" + strDate + "日";
		},
		//设置/取消存档
		async setArchive(it) {
			var res = await this.$API.sysnotice.status.put({
				ids: [it.id],
				status: it.status == 0 ? 2 : 0,
			});
			if (res.code == 200) {
				this.init();
				this.initTotal();
			} else {
				this.$alert(res.message, "提示", { type: "error" });
			}
		},
		//设置已读/取消
		async setSignRead(it) {
			if (it.isRead) {
				const res = await this.$API.sysnotice.clearRead.put([it.id]);
				if (res.code == 200) {
					this.init();
					this.initTotal();
				} else {
					this.$alert(res.message, "提示", { type: "error" });
				}
			} else {
				const res = await this.$API.sysnotice.read.put([it.id]);
				if (res.code == 200) {
					this.init();
					this.initTotal();
				} else {
					this.$alert(res.message, "提示", { type: "error" });
				}
			}
		},
		//删除
		async tableDel(row) {
			var res = await this.$API.sysnotice.status.put({
				ids: [row.id],
				status: 3,
			});
			if (res.code == 200) {
				this.init();
				this.initTotal();
				this.lookType = 0;
				this.$message.success("删除成功");
			} else {
				this.$alert(res.message, "提示", { type: "error" });
			}
		},
		//批量删除
		batchDel() {
			this.optionFun("删除", 3);
		},
		//存档
		archives() {
			this.optionFun("存档", 2);
		},
		//设置已读
		setRead() {
			let checkedArr = this.list
				.filter((m) => m.checked)
				.map((item) => {
					return item.id;
				});

			this.$confirm(`确定要执行已读操作吗？`, "提示", {
				type: "warning",
				confirmButtonText: "确定",
				cancelButtonText: "取消",
			})
				.then(async () => {
					const loading = this.$loading();
					var res = await this.$API.sysnotice.read.put(checkedArr);
					if (res.code == 200) {
						loading.close();
						this.init();
						this.initTotal();
						this.$message.success("操作成功");
					} else {
						this.$alert(res.message, "提示", { type: "error" });
					}
				})
				.catch(() => {});
		},
		selectionChange(selection) {
			this.selection = selection;
		},
		optionFun(tip, status) {
			let checkedArr = this.list
				.filter((m) => m.checked)
				.map((item) => {
					return item.id;
				});
			if (checkedArr.length == 0) {
				this.$alert("请选择要" + tip + "的项", "提示", {
					type: "error",
				});
				return;
			}
			this.$confirm(
				`确定${tip}选中的 ${checkedArr.length} 项吗？`,
				"提示",
				{
					type: "warning",
					confirmButtonText: "确定",
					cancelButtonText: "取消",
				}
			)
				.then(async () => {
					const loading = this.$loading();
					var res = await this.$API.sysnotice.status.put({
						ids: checkedArr,
						status: status,
					});
					if (res.code == 200) {
						loading.close();
						this.init();
						this.initTotal();
						this.lookType = 0;
						this.$message.success("操作成功");
					} else {
						this.$alert(res.message, "提示", { type: "error" });
					}
				})
				.catch(() => {});
		},
		checkChange(e) {
			let checked = this.list.map((item) => {
				return item.id;
			});
			this.list.forEach((item) => {
				item.checked = e;
			});
			this.selection = e ? checked : [];
			this.isIndeterminate = e;
		},
		itemChange(e) {
			console.log("e", e);
		},
	},
};
</script>
<style scoped>
.notice-ft {
	margin-right: 20px;
}
.notice-container .el-aside {
	/* border-right: none; */
	background-color: #ffffff;
}
.notice-container .el-aside .el-menu-item {
	position: relative;
	padding: 13px 20px;
	height: auto;
	line-height: inherit;
}
.notice-container .el-aside .el-menu-item .el-tag {
	position: absolute;
	right: 10px;
}
.notice-list-title {
	display: flex;
	padding: 20px;
	border-bottom: 1px solid #e6e7e8;
}
.notice-list-title .el-checkbox.el-checkbox--large,
.notice-list-title .el-checkbox {
	height: auto;
	flex: 1;
}
.notice-list-wall {
}
.notice-item {
	display: flex;
	align-items: center;
	border-left: 3px solid #ffffff;
	border-bottom: 1px solid #e6e7e8;
}
.notice-item:hover,
.notice-item.active {
	border-left: 3px solid #409eff;
	background-color: #f5f7fa;
}
.notice-item:hover .item-del .el-icon {
	display: inline-block;
}
.notice-item:hover .item-avatar .el-checkbox,
.notice-item.active .item-avatar .el-checkbox {
	display: inline-block;
}
.notice-item:hover .item-avatar .el-avatar,
.notice-item.active .item-avatar .el-avatar {
	display: none;
}
.notice-item:hover .item-tool,
.notice-item.active .item-tool {
	display: block;
}
.item-info {
	padding: 10px;
	width: 80%;
	line-height: 24px;
}
.item-info p,
.item-info h3 {
	overflow: hidden;
	white-space: nowrap;
	text-overflow: ellipsis;
	font-weight: 400;
}
.item-info h3.uname {
	flex: 1;
	font-weight: 500;
}
.item-avatar {
	width: 42px;
	text-align: center;
	padding-left: 10px;
}
.item-avatar .el-checkbox {
	display: none;
}
.item-del {
	width: 30px;
	text-align: center;
	font-size: 15px;
	cursor: pointer;
	height: 92px;
	padding-top: 40px;
}
.item-del .el-icon {
	display: none;
}
.item-del:hover {
	background-color: #fef2f3;
	color: #e6182c;
}
.item-uname {
	display: flex;
}
.item-tool {
	font-size: 16px;
	display: none;
}
.title-time {
	display: flex;
	align-items: center;
}
.title-time h3 {
	width: 70%;
}
.item-tool .el-icon {
	margin-right: 10px;
	cursor: pointer;
}
.default-main {
	text-align: center;
	padding-top: 10%;
	font-size: var(--el-font-size-base);
	color: var(--el-text-color-regular);
}
.default-main .el-icon {
	font-size: 60px;
	color: #999999;
}
</style>
