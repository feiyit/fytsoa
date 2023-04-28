<template>
	<sc-dialog
		v-model="visible"
		show-fullscreen
		title="投票详情"
		width="800px"
		@close="close"
	>
		<el-descriptions border :column="2">
			<el-descriptions-item label="投票标题">{{
				model.title
			}}</el-descriptions-item>
			<el-descriptions-item label="投票类型">{{
				model.type == 1 ? "图文" : "视频"
			}}</el-descriptions-item>
			<el-descriptions-item label="时间">{{
				model.startTime + "-" + model.endTime
			}}</el-descriptions-item>
			<el-descriptions-item label="勾选规则">
				{{ model.tickRule == 1 ? "单选" : "多选" }}
			</el-descriptions-item>
			<el-descriptions-item label="防刷规则">{{
				model.swipeRule ? "已开启，一个IP只能投票一次" : "未开启"
			}}</el-descriptions-item>
			<el-descriptions-item label="投票项数量">{{
				model.items.length
			}}</el-descriptions-item>
		</el-descriptions>
		<el-scrollbar class="vote-wall" height="200px">
			<div class="vote-item" v-for="(item,index) in model.items" :key="index">
				<div class="title">{{item.title}}</div>
				<el-progress :stroke-width="10" :percentage="slec(item.count)" :color="colors[index]" />
			</div>
		</el-scrollbar>

		<template #footer>
			<el-button @click="close">取 消</el-button>
		</template>
	</sc-dialog>
</template>
<script>
export default {
	data() {
		return {
			visible: false,
			model: {},
			colors:['#f56c6c','#e6a23c','#5cb87a','#1989fa','#6f7ad3','#f56c6c','#e6a23c','#5cb87a','#1989fa','#6f7ad3'],
			counts:0
		};
	},
	mounted() {},
	methods: {
		async open(row) {
			var res = await this.$API.sysvote.model.get(row.id);
			this.model = res.data;
			console.log('data',res.data)
			res.data.items.forEach(item => {
				this.counts+=item.count
			});
			
			this.visible = true;
		},
		slec(count){
			if(count==0 || this.counts==0){
				return 0
			}
			return ((count/this.counts)*100).toFixed(0)
		},
		close() {
			this.visible = false;
		},
	},
};
</script>
<style>
.vote-wall{margin-top:20px;}
.vote-wall .vote-item{padding:0; margin-bottom: 15px;}
</style>