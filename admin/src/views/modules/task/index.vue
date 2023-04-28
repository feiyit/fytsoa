<template>
	<el-main>
		<el-row :gutter="15">
			<el-col
				:xl="6"
				:lg="6"
				:md="8"
				:sm="12"
				:xs="24"
				v-for="item in list"
				:key="item.id"
			>
				<el-card class="task task-item" shadow="hover">
					<h2>【{{ item.groupName }}】{{ item.taskName }}</h2>
					<ul>
						<li>
							<h4>
								{{ item.taskType == 1 ? "执行类" : "执行Api" }}
							</h4>
							<p style="height: 40px">
								{{
									item.taskType == 1
										? item.dllClassName
										: item.apiUrl
								}}
							</p>
						</li>
						<li>
							<h4>定时规则</h4>
							<p>{{ item.interval }}</p>
						</li>
						<li>
							<h4>最后一次执行时间</h4>
							<p>
								{{
									!item.lastRunTime ? "--" : item.lastRunTime
								}}
							</p>
						</li>
					</ul>
					<div class="bottom">
						<div class="state">
							<el-tag v-if="item.status == 1">
								{{ formatStatues(item.status) }}
							</el-tag>
							<el-tag v-if="item.status == 2" type="danger">
								{{ formatStatues(item.status) }}
							</el-tag>
							<el-tag v-if="item.status == 3" type="info">
								{{ formatStatues(item.status) }}
							</el-tag>
							<el-tag v-if="item.status == 4" type="warning">
								{{ formatStatues(item.status) }}
							</el-tag>
							<el-tag v-if="item.status == 5" type="danger">
								{{ formatStatues(item.status) }}
							</el-tag>
							<el-tag v-if="item.status == 6">
								{{ formatStatues(item.status) }}
							</el-tag>
						</div>
						<div class="handler">
							<el-popconfirm
								title="确定立即执行吗？"
								@confirm="start(item)"
							>
								<template #reference>
									<el-button
										type="primary"
										icon="el-icon-caret-right"
										circle
										v-auth="'modulestask:start'"
									></el-button>
								</template>
							</el-popconfirm>
							<el-dropdown @command="handleCommand">
								<el-button
									type="primary"
									icon="el-icon-more"
									circle
									plain
								></el-button>
								<template #dropdown>
									<el-dropdown-menu>
										<el-dropdown-item
											:command="{
												type: 'pause',
												model: item,
											}"
											v-auth="'modulestask:pause'"
										>
											暂停
										</el-dropdown-item>
										<el-dropdown-item
											:command="{
												type: 'run',
												model: item,
											}"
											v-auth="'modulestask:execute'"
										>
											执行
										</el-dropdown-item>
										<el-dropdown-item
											:command="{
												type: 'logs',
												model: item,
											}"
										>
											日志
										</el-dropdown-item>
										<el-dropdown-item
											:command="{
												type: 'edit',
												model: item,
											}"
											v-auth="'modulestask:edit'"
										>
											编辑
										</el-dropdown-item>
										<el-dropdown-item
											:command="{
												type: 'del',
												model: item,
											}"
											divided
											v-auth="'modulestask:delete'"
										>
											删除
										</el-dropdown-item>
									</el-dropdown-menu>
								</template>
							</el-dropdown>
						</div>
					</div>
				</el-card>
			</el-col>
			<el-col :xl="6" :lg="6" :md="8" :sm="12" :xs="24">
				<el-card
					class="task task-add"
					shadow="none"
					v-auth="'modulestask:add'"
					@click="add"
				>
					<el-icon><el-icon-plus /></el-icon>
					<p>添加计划任务</p>
				</el-card>
			</el-col>
		</el-row>
	</el-main>
	<modify ref="modify" @complete="init" />
	<logs ref="logs"></logs>
</template>

<script>
import modify from "./modify";
import logs from "./logs";

export default {
	name: "task",
	components: {
		modify,
		logs,
	},
	data() {
		return {
			list: [],
		};
	},
	mounted() {
		this.init();
	},
	methods: {
		async init() {
			const res = await this.$API.sysquartz.list.get();
			if (res.code == 200) {
				this.list = res.data;
				console.log("list", this.list);
			}
		},
		add(row) {
			if (row.id) {
				this.$refs.modify.open(row);
			} else {
				this.$refs.modify.open();
			}
		},
		del(task) {
			const that = this;
			this.$confirm(`确认删除 ${task.taskName} 计划任务吗？`, "提示", {
				type: "warning",
				confirmButtonText: "删除",
				confirmButtonClass: "el-button--danger",
			})
				.then(async () => {
					var res = await that.$API.sysquartz.delete.delete(task);
					if (res.code == 200) {
						that.init();
					} else {
						that.$alert(res.message, "提示", { type: "error" });
					}
				})
				.catch(() => {
					//取消
				});
		},
		formatStatues(val) {
			switch (val) {
				case 1:
					return "新增";
				case 2:
					return "删除";
				case 3:
					return "修改";
				case 4:
					return "暂停";
				case 5:
					return "停止";
				case 6:
					return "运行中";
				case 7:
					return "立即执行";
				default:
					return "";
			}
		},
		async start(item) {
			var res = await this.$API.sysquartz.start.put(item);
			if (res.code == 200) {
				this.init();
			} else {
				this.$alert(res.message, "提示", { type: "error" });
			}
		},
		async handleCommand(arg) {
			let res = null;
			switch (arg.type) {
				case "run":
					res = await this.$API.sysquartz.run.put(arg.model);
					break;
				case "pause":
					res = await this.$API.sysquartz.pause.put(arg.model);
					break;
				case "del":
					this.del(arg.model);
					break;
				case "edit":
					this.add(arg.model);
					break;
				case "logs":
					this.$refs.logs.opens(arg.model);
					break;
			}
			if (res) {
				if (res.code == 200 && res.data.status) {
					this.$message.success("操作成功");
					this.init();
				} else {
					this.$alert(res.data.message, "提示", { type: "error" });
				}
			}
		},
	},
};
</script>

<style scoped>
.task {
	height: 280px;
}
.task-item h2 {
	font-size: 15px;
	color: #3c4a54;
	padding-bottom: 15px;
}
.task-item li {
	list-style-type: none;
	margin-bottom: 10px;
}
.task-item li h4 {
	font-size: 12px;
	font-weight: normal;
	color: #999;
}
.task-item li p {
	margin-top: 5px;
}
.task-item .bottom {
	border-top: 1px solid #ebeef5;
	text-align: right;
	padding-top: 10px;
	display: flex;
	justify-content: space-between;
	align-items: center;
}

.task-add {
	display: flex;
	flex-direction: column;
	align-items: center;
	justify-content: center;
	text-align: center;
	cursor: pointer;
	color: #999;
}
.task-add:hover {
	color: #409eff;
}
.task-add i {
	font-size: 30px;
}
.task-add p {
	font-size: 12px;
	margin-top: 20px;
}
</style>
