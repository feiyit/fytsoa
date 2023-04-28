<template>
	<el-row :gutter="40">
		<el-col v-if="form.id == '0'">
			<el-empty
				description="请选择左侧菜单后操作"
				:image-size="100"
			></el-empty>
		</el-col>
		<template v-else>
			<el-col :lg="12">
				<h2>{{ form.name || "新增菜单" }}</h2>
				<el-form
					ref="dialogForm"
					:model="form"
					:rules="rules"
					label-width="80px"
					label-position="left"
				>
					<el-form-item label="显示名称" prop="name">
						<el-input
							v-model="form.name"
							clearable
							placeholder="菜单显示名字"
						></el-input>
					</el-form-item>
					<el-form-item label="上级菜单" prop="parentIdList">
						<el-cascader
							v-model="form.parentIdList"
							:options="menuOptions"
							:props="menuProps"
							:show-all-levels="false"
							placeholder="顶级菜单"
							clearable
							disabled
						></el-cascader>
					</el-form-item>
					<el-form-item label="类型" prop="types">
						<el-radio-group v-model="form.types">
							<el-radio-button label="menu">菜单</el-radio-button>
							<el-radio-button label="iframe"
								>Iframe</el-radio-button
							>
							<el-radio-button label="link">外链</el-radio-button>
							<el-radio-button label="button"
								>按钮</el-radio-button
							>
						</el-radio-group>
					</el-form-item>
					<el-form-item label="别名" prop="code">
						<el-input
							v-model="form.code"
							clearable
							placeholder="菜单别名"
						></el-input>
						<div class="el-form-item-msg">
							系统唯一且与内置组件名一致，否则导致缓存失效。如类型为Iframe的菜单，别名将代替源地址显示在地址栏
						</div>
					</el-form-item>
					<el-form-item label="菜单图标">
						<sc-icon-select
							v-model="form.icon"
							clearable
						></sc-icon-select>
					</el-form-item>
					<el-form-item label="路由地址" prop="urls">
						<el-input
							v-model="form.urls"
							clearable
							placeholder=""
						></el-input>
					</el-form-item>
					<el-form-item label="重定向" prop="redirect">
						<el-input
							v-model="form.redirect"
							clearable
							placeholder=""
						></el-input>
					</el-form-item>
					<el-form-item label="菜单高亮" prop="active">
						<el-input
							v-model="form.active"
							clearable
							placeholder=""
						></el-input>
						<div class="el-form-item-msg">
							子节点或详情页需要高亮的上级菜单路由地址
						</div>
					</el-form-item>
					<el-form-item label="视图" prop="vuePath">
						<el-input
							v-model="form.vuePath"
							clearable
							placeholder=""
						>
							<template #prepend>views/</template>
						</el-input>
						<div class="el-form-item-msg">
							如父节点、链接或Iframe等没有视图的菜单不需要填写
						</div>
					</el-form-item>
					<el-form-item label="颜色" prop="color">
						<el-color-picker
							v-model="form.color"
							:predefine="predefineColors"
						></el-color-picker>
					</el-form-item>
					<el-form-item label="是否显示">
						<el-radio-group v-model="form.status">
							<el-radio :label="true">显示</el-radio>
							<el-radio :label="false">隐藏</el-radio>
						</el-radio-group>
						<div class="el-form-item-msg">
							菜单不显示在导航中，但用户依然可以访问
						</div>
					</el-form-item>
					<el-form-item label="是否全屏">
						<el-radio-group v-model="form.fullPage">
							<el-radio :label="true">是</el-radio>
							<el-radio :label="false">否</el-radio>
						</el-radio-group>
						<div class="el-form-item-msg">
							一般配合功能列表中详细使用，以及不在资源中显示
						</div>
					</el-form-item>
					<el-form-item>
						<el-button
							type="primary"
							:loading="loading"
							v-auth="'tenantmenu:edit'"
							@click="save"
						>
							保 存
						</el-button>
					</el-form-item>
				</el-form>
			</el-col>
			<el-col :lg="12" class="apilist">
				<h2>接口权限</h2>
				<sc-form-table
					v-model="form.api"
					:addTemplate="apiListAddTemplate"
					placeholder="暂无匹配接口权限"
				>
					<el-table-column prop="code" label="标识" width="150">
						<template #default="scope">
							<el-input
								v-model="scope.row.code"
								placeholder="请输入内容"
							></el-input>
						</template>
					</el-table-column>
					<el-table-column prop="method" label="请求类型" width="130">
						<template #default="scope">
							<el-select v-model="scope.row.method">
								<el-option
									v-for="item in methodType"
									:key="item"
									:label="item"
									:value="item"
								/>
							</el-select>
						</template>
					</el-table-column>
					<el-table-column prop="url" label="Api url">
						<template #default="scope">
							<el-input
								v-model="scope.row.url"
								placeholder="请输入内容"
							></el-input>
						</template>
					</el-table-column>
				</sc-form-table>
			</el-col>
		</template>
	</el-row>
</template>

<script>
import scIconSelect from "@/components/scIconSelect";
import tool from "@/utils/tool";
export default {
	components: {
		scIconSelect,
	},
	props: {
		menu: { type: Array, default: () => [] },
	},
	data() {
		return {
			form: {
				id: "0",
				tenantId: 1,
				parentId: "",
				parentIdList: [],
				name: "",
				code: "",
				urls: "",
				vuePath: "",
				redirect: "",
				icon: "",
				active: "",
				color: "",
				types: "menu",
				status: true,
				fullPage: false,
				api: [],
			},
			menuOptions: [],
			menuProps: {
				multiple: false,
				checkStrictly: true,
				expandTrigger: "hover",
			},
			rules: {
				parentIdList: [
					{
						required: true,
						type: "array",
						message: "请选择上级菜单",
						trigger: "change",
					},
				],
				types: [
					{
						required: true,
						message: "菜单类型不能为空",
						trigger: "change",
					},
				],
				name: [
					{
						required: true,
						message: "请输入菜单名称",
						trigger: "blur",
					},
				],
				code: [
					{
						required: true,
						message: "请输入权限标识",
						trigger: "blur",
					},
				],
				vuePath: [],
				urls: [
					{
						required: true,
						message: "请输入路由地址",
						trigger: "blur",
					},
				],
			},
			predefineColors: [
				"#ff4500",
				"#ff8c00",
				"#ffd700",
				"#67C23A",
				"#00ced1",
				"#409EFF",
				"#c71585",
			],
			apiListAddTemplate: {
				code: "",
				method: "",
				url: "",
			},
			methodType: ["GET", "POST", "PUT", "DELETE"],
			loading: false,
		};
	},
	watch: {
		menu: {
			handler() {
				this.menuOptions = this.treeToMap(this.menu);
			},
			deep: true,
		},
	},
	mounted() {},
	methods: {
		//简单化菜单
		treeToMap(tree) {
			let _tree = [
				{ id: "1", value: "0", label: "顶级菜单", parentId: "0" },
			];
			tree.some((m) => {
				_tree.push({
					id: m.id,
					value: m.id,
					label: m.name,
					parentId: m.parentId,
				});
			});
			return tool.changeTree(_tree);
		},
		//保存
		async save() {
			this.loading = true;
			var res = await this.$API.sysmenu.update.put(this.form);
			this.loading = false;
			if (res.code == 200) {
				this.$emit("complete");
				this.$message.success("保存成功");
			} else {
				this.$message.warning(res.message);
			}
		},
		handleIcon(item) {
			this.form.icon = item;
		},
		//表单注入数据
		async setData(data) {
			const res = await this.$API.sysmenu.model.get(data.id);
			if (res.data) {
				if (!res.data.icon) {
					res.data.icon = "";
				}
				this.form = res.data;
			} else {
				this.form.id = 0;
			}
		},
	},
};
</script>

<style scoped>
h2 {
	font-size: 17px;
	color: #3c4a54;
	padding: 0 0 30px 0;
}
.apilist {
	border-left: 1px solid #eee;
}

[data-theme="dark"] h2 {
	color: #fff;
}
[data-theme="dark"] .apilist {
	border-color: #434343;
}
</style>
