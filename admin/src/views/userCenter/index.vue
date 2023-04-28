<template>
	<el-container class="page-user">
		<el-aside style="width: 240px">
			<el-container>
				<el-header style="height: auto; display: block">
					<div class="user-info-top">
						<el-avatar
							:size="70"
							:src="$CONFIG.SERVER_URL + user.headPic"
						></el-avatar>
						<h2>{{ user.fullName }}</h2>
						<p>
							<el-tag
								v-for="(it, index) in user.role"
								:key="index"
								effect="dark"
								round
								size="large"
								disable-transitions
								>{{ it }}</el-tag
							>
						</p>
					</div>
				</el-header>
				<el-main class="nopadding">
					<el-menu class="menu" :default-active="page">
						<el-menu-item-group
							v-for="group in menu"
							:key="group.groupName"
							:title="group.groupName"
						>
							<el-menu-item
								v-for="item in group.list"
								:key="item.component"
								:index="item.component"
								@click="openPage"
							>
								<el-icon v-if="item.icon"
									><component :is="item.icon"
								/></el-icon>
								<template #title>
									<span>{{ item.title }}</span>
								</template>
							</el-menu-item>
						</el-menu-item-group>
					</el-menu>
				</el-main>
			</el-container>
		</el-aside>
		<el-main>
			<Suspense>
				<template #default>
					<component :is="page" />
				</template>
				<template #fallback>
					<el-skeleton :rows="3" />
				</template>
			</Suspense>
		</el-main>
	</el-container>
</template>

<script>
import { defineAsyncComponent } from "vue";

export default {
	name: "userCenter",
	components: {
		account: defineAsyncComponent(() => import("./user/account")),
		seting: defineAsyncComponent(() => import("./user/seting")),
		pushSettings: defineAsyncComponent(() => import("./user/pushSettings")),
		password: defineAsyncComponent(() => import("./user/password")),
		space: defineAsyncComponent(() => import("./user/space")),
		logs: defineAsyncComponent(() => import("./user/logs")),
	},
	data() {
		return {
			menu: [
				{
					groupName: "基本设置",
					list: [
						{
							icon: "el-icon-postcard",
							title: "账号信息",
							component: "account",
						},
						{
							icon: "el-icon-operation",
							title: "个人设置",
							component: "seting",
						},
						{
							icon: "el-icon-lock",
							title: "修改密码",
							component: "password",
						},
						{
							icon: "el-icon-bell",
							title: "通知设置",
							component: "pushSettings",
						},
					],
				},
				{
					groupName: "数据管理",
					list: [
						{
							icon: "el-icon-coin",
							title: "存储空间信息",
							component: "space",
						},
						{
							icon: "el-icon-clock",
							title: "操作日志",
							component: "logs",
						},
					],
				},
			],
			user: {
				id: 0,
				account: "fytsoa@outlook.com",
				name: "FytSoa",
				headPic: "",
				sex: "男",
				summary: "正所谓富贵险中求",
				role: [],
				post: [],
			},
			page: "account",
		};
	},
	//路由跳转进来 判断from是否有特殊标识做特殊处理
	beforeRouteEnter(to, from, next) {
		next((vm) => {
			if (from.is) {
				//删除特殊标识，防止标签刷新重复执行
				delete from.is;
				//执行特殊方法
				vm.$alert("路由跳转过来后含有特殊标识，做特殊处理", "提示", {
					type: "success",
					center: true,
				})
					.then(() => {})
					.catch(() => {});
			}
		});
	},
	mounted() {
		this.init();
	},
	methods: {
		async init() {
			const res = await this.$API.auth.user.get();
			this.user = res.data;
		},
		openPage(item) {
			this.page = item.index;
		},
	},
};
</script>
