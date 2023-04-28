<template>
	<el-container>
		<el-main class="nopadding">
			<el-header>
				<div class="left-panel">
					<sc-table-select
						v-model="selectUser"
						clearable
						:apiObj="apiObj"
						:table-width="450"
						:props="props"
						@change="userChange"
					>
						<el-table-column prop="headPic" label="头像" width="80">
							<template #default="scope">
								<el-avatar
									:src="
										$CONFIG.SERVER_URL + scope.row.headPic
									"
									size="small"
								></el-avatar>
							</template>
						</el-table-column>
						<el-table-column
							prop="fullName"
							label="姓名"
							width="180"
						></el-table-column>
						<el-table-column
							prop="roleGroupName"
							label="角色"
						></el-table-column>
					</sc-table-select>
				</div>
				<div class="right-panel">
					<el-select
						v-model="param.typeId"
						placeholder="日程类型"
						style="width: 160px"
						clearable
					>
						<el-option
							v-for="item in typeOption"
							:key="item.value"
							:label="item.label"
							:value="item.value"
						/>
					</el-select>
					<el-select
						v-model="param.levelId"
						placeholder="日程级别"
						style="width: 160px"
						clearable
					>
						<el-option
							v-for="item in levelOption"
							:key="item.value"
							:label="item.label"
							:value="item.value"
						>
							<span style="float: left">{{ item.label }}</span>
							<span style="float: right"
								><small
									class="circle"
									:style="{ 'background-color': item.color }"
								></small></span
						></el-option>
					</el-select>
					<el-button
						icon="el-icon-search"
						type="primary"
						@click="init"
					/>
				</div>
			</el-header>
			<el-calendar v-model="toDay" ref="calendar">
				<template #header="{ date }">
					<span>{{ date }}</span>
					<el-button-group>
						<el-button
							size="small"
							@click="selectDate('prev-month')"
							>上个月</el-button
						>
						<el-button size="small" @click="selectDate('today')"
							>今天</el-button
						>
						<el-button
							size="small"
							@click="selectDate('next-month')"
							>下个月</el-button
						>
					</el-button-group>
				</template>
				<template #dateCell="{ data }">
					<div class="calendar-item">
						<h2>{{ data.day.split("-")[2] }} 日</h2>
						<div
							v-if="
								getData(data.day) &&
								data.type == 'current-month'
							"
							class="calendar-item-info"
						>
							<template
								v-for="(item, index) in getData(data.day)"
								:key="index"
							>
								<div
									class="calendar-item-text"
									v-if="index < 2"
									:style="{
										'border-left':
											'3px solid ' +
											item.levelCode.codeValues,
									}"
								>
									{{ item.title }}
								</div>
							</template>
						</div>
					</div>
				</template>
			</el-calendar>
		</el-main>
		<el-aside style="width: 320px; border-left: 1px solid #e6e6e6">
			<el-container>
				<el-header>
					<h2 class="dayTitle">
						<el-icon><el-icon-calendar /></el-icon>{{ day }}
					</h2>
					<div class="right-panel">
						<el-button
							icon="el-icon-plus"
							type="primary"
							v-auth="'calendar:add'"
							@click="open"
							>新建日程</el-button
						>
					</div>
				</el-header>
				<el-main>
					<div class="task-list">
						<template v-if="dayItem">
							<el-card
								shadow="hover"
								v-for="task in dayItem"
								:key="task.id"
								:class="stateMap[task.state]"
								:style="{
									'border-color': task.levelCode.codeValues,
								}"
							>
								<h2>{{ task.title }}</h2>
								<div class="task-bottom">
									<el-tag type="success" round size="small">{{
										task.typeCode.name
									}}</el-tag>
									<div
										class="level"
										:style="{
											'background-color':
												task.levelCode.codeValues,
										}"
									>
										{{ task.levelCode.name }}
									</div>
									<el-dropdown size="small">
										<el-button icon="el-icon-more" circle />
										<template #dropdown>
											<el-dropdown-menu>
												<el-dropdown-item
													@click="delTask(task)"
													icon="el-icon-delete"
													>删除</el-dropdown-item
												>
												<el-dropdown-item
													@click="open(task)"
													divided
													icon="el-icon-edit"
													>编辑</el-dropdown-item
												>
											</el-dropdown-menu>
										</template>
									</el-dropdown>
								</div>
								<el-divider />
								<el-timeline>
									<el-timeline-item
										type="primary"
										:hollow="true"
									>
										{{ task.beginTime }}
									</el-timeline-item>
									<el-timeline-item
										type="primary"
										:hollow="true"
									>
										{{ task.endTime }}
									</el-timeline-item>
								</el-timeline>
								<div class="remind">
									<el-popover
										placement="top-start"
										v-for="(row, uindex) in task.user"
										:key="uindex"
										:title="row.fullName"
										trigger="hover"
									>
										<template #reference>
											<el-avatar
												size="small"
												:src="
													$CONFIG.SERVER_URL +
													row.headPic
												"
											/>
										</template>
										<p>{{ row.mobile }}</p>
										<p>{{ row.email }}</p>
									</el-popover>
								</div>
							</el-card>
						</template>
						<template v-else>
							<el-empty
								description="无工作任务"
								:image-size="100"
							></el-empty>
						</template>
					</div>
				</el-main>
			</el-container>
		</el-aside>
		<modify ref="modify" @complete="init" />
	</el-container>
</template>

<script>
import { defineAsyncComponent } from "vue";
export default {
	components: {
		modify: defineAsyncComponent(() => import("./modify")),
	},
	name: "calendar",
	data() {
		return {
			stateMap: {
				open: "open",
				complete: "complete",
				timeout: "timeout",
			},
			apiObj: this.$API.sysadmin.page,
			selectUser: {},
			props: {
				label: "fullName",
				value: "id",
				keyword: "keyword",
			},
			param: {
				id: undefined,
				typeId: undefined,
				levelId: undefined,
				toDay: null,
			},
			typeOption: [],
			levelOption: [],
			toDay: new Date(this.demoDay()),
			resData: [],
			groupTime: {},
			data: {},
		};
	},
	computed: {
		day() {
			return this.$TOOL.dateFormat(this.toDay, "yyyy-MM-dd");
		},
		dayItem() {
			return this.getData(this.day);
		},
	},
	mounted() {
		this.initTypeOption();
		this.initLevelOption();
		this.init();
	},
	methods: {
		async init() {
			if (!this.param.typeId) {
				this.param.typeId = undefined;
			}
			if (!this.param.levelId) {
				this.param.levelId = undefined;
			}
			const res = await this.$API.syscalendar.list.get(this.param);
			res.data.forEach((m) => {
				m.beginTime = m.startTime;
				m.startTime = this.$TOOL.dateFormat(m.startTime, "yyyy-MM-dd");
			});
			this.resData = res.data;
			this.data = this.getGroup(this.resData, "startTime");
		},
		async initTypeOption() {
			const res = await this.$API.syscode.list.get({
				typeCode: "calendar-type",
			});
			if (res.code == 200) {
				res.data.forEach((e) => {
					this.typeOption.push({
						label: e.name,
						value: e.id,
					});
				});
			}
		},
		async initLevelOption() {
			const res = await this.$API.syscode.list.get({
				typeCode: "calendar-level",
			});
			if (res.code == 200) {
				res.data.forEach((e) => {
					this.levelOption.push({
						label: e.name,
						value: e.id,
						color: e.codeValues,
					});
				});
			}
		},
		getData(date) {
			return this.data[date];
		},
		demoDay(n = 0) {
			var curDate = new Date();
			var oneDayTime = 24 * 60 * 60 * 1000;
			var rDate = new Date(curDate.getTime() + oneDayTime * n);
			return this.$TOOL.dateFormat(rDate, "yyyy-MM-dd");
		},
		userChange(val) {
			this.selectUser = val;
			this.param.id = val.id;
		},
		getGroup(data, key) {
			let groups = {};
			data.forEach((c) => {
				let value = c[key];
				groups[value] = groups[value] || [];
				groups[value].push(c);
			});
			return groups;
		},
		selectDate(val) {
			this.$refs.calendar.selectDate(val);
			this.param.toDay = this.$TOOL.dateFormat(this.toDay, "yyyy-MM-dd");
			this.init();
		},
		delTask(row) {
			this.$confirm(`确定删除选中的 ${row.title} 吗？`, "提示", {
				type: "warning",
				confirmButtonText: "确定",
				cancelButtonText: "取消",
			})
				.then(async () => {
					const loading = this.$loading();
					var res = await this.$API.syscalendar.delete.delete([
						row.id,
					]);
					if (res.code == 200) {
						loading.close();
						this.$message.success("删除成功");
						this.init();
					} else {
						this.$alert(res.message, "提示", { type: "error" });
					}
				})
				.catch(() => {});
		},
		open(row) {
			if (row.id) {
				this.$refs.modify.open(row);
			} else {
				this.$refs.modify.open();
			}
		},
	},
};
</script>

<style scoped>
.calendar-item h2 {
	font-size: 14px;
	padding-left: 8px;
}
.calendar-item-info {
	margin-top: 5px;
}
.calendar-item-info p {
	margin-top: 5px;
}

.task-list .el-card {
	margin-bottom: 15px;
	border-left: 5px solid #ddd;
	cursor: pointer;
}

.task-list h2 {
	font-size: 14px;
	font-weight: normal;
}
.task-list .remind {
	display: flex;
}
.task-list .remind .el-avatar {
	margin-right: 10px;
}
.task-list .el-divider--horizontal {
	margin: 12px 0;
}
.task-list .el-timeline-item {
	padding-bottom: 10px;
}
.task-list .el-timeline-item:last-child {
	padding-bottom: 0px;
}
.task-bottom {
	display: flex;
	justify-content: space-between;
	align-items: center;
	margin-top: 10px;
}
.task-bottom .level {
	border-radius: 10px;
	background-color: #67c23a;
	padding: 2px 10px;
	color: #ffffff;
}

.dayTitle {
	font-size: 14px;
	display: flex;
	align-items: center;
}
.dayTitle i {
	color: #999;
	margin-right: 10px;
}
.circle {
	display: inline-block;
	width: 10px;
	height: 10px;
	border-radius: 5px;
}
.calendar-item-text {
	white-space: nowrap;
	overflow: hidden;
	text-overflow: ellipsis;
	background-color: #fdf6ed;
	padding: 2px 5px;
	border-left: 3px solid #f56c6c;
	margin: 2px 0;
}
>>> .el-calendar-table .el-calendar-day {
	padding: 8px 0px !important;
}
</style>
