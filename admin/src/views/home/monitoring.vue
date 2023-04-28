<template>
	<el-main class="p10">
		<el-row :gutter="10">
			<el-col :span="6">
				<el-card class="box-card">
					<scEcharts height="300px" :option="optionCpu"></scEcharts>
				</el-card>
			</el-col>
			<el-col :span="6">
				<el-card class="box-card">
					<scEcharts
						height="300px"
						:option="optionMemory"
					></scEcharts>
				</el-card>
			</el-col>
			<el-col :span="6">
				<el-card class="box-card">
					<scEcharts height="300px" :option="optionDisk"></scEcharts>
				</el-card>
			</el-col>
			<el-col :span="6">
				<el-card class="box-card">
					<scEcharts
						height="300px"
						:option="optionNetwork"
					></scEcharts>
				</el-card>
			</el-col>
			<el-col :span="24">
				<el-card class="box-card">
					<template #header>
						<div class="card-header">服务器信息</div>
					</template>
					<el-descriptions :column="3" border>
						<el-descriptions-item label="系统架构" width="150px">{{
							os.osArchitecture
						}}</el-descriptions-item>
						<el-descriptions-item label="进程架构">{{
							os.processArchitecture
						}}</el-descriptions-item>
						<el-descriptions-item label="系统运行时长">{{
							os.runTime
						}}</el-descriptions-item>
						<el-descriptions-item label="64位操作系统">
							<el-tag size="small">{{
								os.is64BitOperatingSystem
							}}</el-tag>
						</el-descriptions-item>
						<el-descriptions-item label="运行环境">{{
							os.frameworkDescription
						}}</el-descriptions-item>
						<el-descriptions-item label="系统标识符">{{
							os.runtimeIdentifier
						}}</el-descriptions-item>
						<el-descriptions-item label="环境版本">{{
							os.version
						}}</el-descriptions-item>

						<el-descriptions-item label="机器名字">{{
							os.machineName
						}}</el-descriptions-item>
						<el-descriptions-item label="进程ID">{{
							os.processId
						}}</el-descriptions-item>
						<el-descriptions-item label="CPU核数">
							<el-tag size="small">{{
								os.processorCount
							}}</el-tag>
							核</el-descriptions-item
						>
						<el-descriptions-item label="Tick数量">{{
							os.tickCount
						}}</el-descriptions-item>
						<el-descriptions-item label="用户名称">{{
							os.userName
						}}</el-descriptions-item>
						<el-descriptions-item label="内存大小">{{
							os.workingSet
						}}</el-descriptions-item>
						<el-descriptions-item label="网络域名称">{{
							os.userDomainName
						}}</el-descriptions-item>
						<el-descriptions-item label="系统目录">{{
							os.systemDirectory
						}}</el-descriptions-item>
						<el-descriptions-item label="系统名称" :span="3">{{
							os.osDescription
						}}</el-descriptions-item>
						<el-descriptions-item label="当前目录" :span="3">{{
							os.currentDirectory
						}}</el-descriptions-item>
					</el-descriptions>
				</el-card>
			</el-col>
			<el-col :span="24">
				<el-card class="box-card">
					<template #header>
						<div class="card-header">网络信息</div>
					</template>
					<el-descriptions :column="3" border>
						<el-descriptions-item
							label="网卡名称"
							width="150px"
							:span="2"
							>{{ netWork.name }}</el-descriptions-item
						>
						<el-descriptions-item label="网络链接速度">{{
							netWork.speed
						}}</el-descriptions-item>
						<el-descriptions-item label="DNS">{{
							netWork.dns
						}}</el-descriptions-item>
						<el-descriptions-item label="网络类型">
							{{ netWork.networkType }}
						</el-descriptions-item>
						<el-descriptions-item label="网卡MAC">{{
							netWork.mac
						}}</el-descriptions-item>
						<el-descriptions-item label="网卡信息">{{
							netWork.trademark
						}}</el-descriptions-item>
					</el-descriptions>
				</el-card>
			</el-col>
		</el-row>
	</el-main>
</template>
<script>
import scEcharts from "@/components/scEcharts";
export default {
	name: "chart",
	components: {
		scEcharts,
	},
	data() {
		return {
			optionNetwork: {
				title: {
					text: "网络信息",
					subtext: "上行速率/下行速率(KB/S)",
				},
				grid: {
					top: "80px",
				},
				tooltip: {
					trigger: "axis",
				},
				legend: {
					data: ["上行速率", "下行速率"],
				},
				xAxis: {
					type: "category",
					data: [],
				},
				yAxis: {
					type: "value",
					axisLabel: {
						formatter: "{value}kb",
					},
				},
				series: [
					{
						data: [],
						type: "line",
					},
					{
						data: [],
						type: "line",
					},
				],
			},
			optionMemory: {
				title: {
					text: "内存信息",
					subtext: "内存使用率",
				},
				series: [
					{
						type: "gauge",
						center: ["50%", "60%"],
						anchor: {
							show: true,
							showAbove: true,
							size: 20,
							itemStyle: {
								borderWidth: 5,
							},
						},
						itemStyle: {
							color: "#58D9F9",
							shadowColor: "rgba(0,138,255,0.45)",
							shadowBlur: 10,
							shadowOffsetX: 2,
							shadowOffsetY: 2,
						},
						detail: {
							valueAnimation: true,
							formatter: "{value}%",
						},
						progress: {
							show: true,
						},
						data: [
							{
								value: 0,
							},
						],
					},
				],
			},
			optionDisk: {
				title: {
					text: "存储信息",
					subtext: "存储使用率",
				},
				series: [
					{
						type: "gauge",
						center: ["50%", "60%"],
						anchor: {
							show: true,
							showAbove: true,
							size: 20,
							itemStyle: {
								borderWidth: 5,
							},
						},
						detail: {
							valueAnimation: true,
							formatter: "{value}%",
						},
						itemStyle: {
							color: "#ff6e76",
							shadowColor: "rgba(255, 121, 129,0.45)",
							shadowBlur: 10,
							shadowOffsetX: 2,
							shadowOffsetY: 2,
						},
						progress: {
							show: true,
						},
						data: [
							{
								value: 0,
							},
						],
					},
				],
			},
			optionCpu: {
				title: {
					text: "CPU监控",
					subtext: "CPU当前总使用率",
				},
				series: [
					{
						type: "gauge",
						center: ["50%", "60%"],
						anchor: {
							show: true,
							showAbove: true,
							size: 20,
							itemStyle: {
								borderWidth: 5,
							},
						},
						itemStyle: {
							color: "#409eff",
							shadowColor: "rgba(64, 158, 255,0.45)",
							shadowBlur: 10,
							shadowOffsetX: 2,
							shadowOffsetY: 2,
						},
						progress: {
							show: true,
						},
						detail: {
							valueAnimation: true,
							formatter: "{value}%",
						},
						data: [
							{
								value: 0,
							},
						],
					},
				],
			},
			os: { runTime: "" },
			netWork: {},
			rate: {},
			netAxis: [],
			netSend: [],
			netReceived: [],
		};
	},
	mounted() {
		this.initInfo();
		this.initRate();
		setInterval(this.initRate, 10000);
	},
	methods: {
		async initInfo() {
			var res = await this.$API.workbench.systeminfo.get();
			this.os = res.data.os;
			this.netWork = res.data.netWork;
		},
		async initRate() {
			var res = await this.$API.workbench.resourceuse.get();
			this.optionMemory.series[0].data[0].value = res.data.memoryRate;
			this.optionCpu.series[0].data[0].value = res.data.cpuRate;
			this.optionDisk.series[0].data[0].value = res.data.diskRate;
			this.os.runTime = res.data.runTime;
			const d = new Date();
			const hour = d.getHours();
			const minutes = d.getMinutes();
			const seconds = d.getSeconds();
			if (this.netAxis.length < 7) {
				this.netAxis.push(hour + ":" + minutes + ":" + seconds);
				this.netSend.push(res.data.netWorkUp);
				this.netReceived.push(res.data.netWorkDown);
			} else {
				this.netAxis.shift();
				this.netSend.shift();
				this.netReceived.shift();
			}
			this.optionNetwork.xAxis.data = this.netAxis;
			this.optionNetwork.series[0].data = this.netSend;
			this.optionNetwork.series[1].data = this.netReceived;
		},
	},
};
</script>
<style lang="scss" scoped>
.p10 {
	padding: 10px;
}
.card-header {
	font-size: 15px;
	font-weight: 400;
}
.card-header.center {
	text-align: center;
}
</style>
