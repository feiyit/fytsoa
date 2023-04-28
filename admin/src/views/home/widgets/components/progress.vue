<template>
	<el-card shadow="hover" header="项目进度">
		<div class="progress">
			<el-progress type="dashboard" :indeterminate="true" :percentage="85.5" :width="160">
				<template #default="{ percentage }">
				    <div class="percentage-value">{{ percentage }}%</div>
				    <div class="percentage-label">当前进度</div>
				  </template>
			</el-progress>
		</div>

	</el-card>
</template>

<script>
	export default {
		title: "项目进度",
		icon: "el-icon-odometer",
		description: "当前负责项目进度-百分比",
		data() {
			return {

			}
		}
	}
</script>

<style scoped>
	.progress {text-align: center;}
	.progress .percentage-value {font-size: 28px;}
	.progress .percentage-label {font-size: 12px;margin-top: 10px;}
</style>
