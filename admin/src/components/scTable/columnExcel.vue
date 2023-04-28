<template>
	<div>
		<div
			v-if="usercolumn.length > 0"
			class="setting-column"
			v-loading="isSave"
		>
			<div class="setting-column__title">
				<span class="move_b"></span>
				<span class="show_b">显示</span>
				<span class="name_b">名称</span>
			</div>
			<div class="setting-column__list" ref="list">
				<ul>
					<li v-for="item in usercolumn" :key="item.prop">
						<span class="move_b">
							<el-tag class="move" style="cursor: move"
								><el-icon-d-caret
									style="width: 1em; height: 1em"
							/></el-tag>
						</span>
						<span class="show_b">
							<el-switch
								v-model="item.hide"
								:active-value="false"
								:inactive-value="true"
							></el-switch>
						</span>
						<span class="name_b" :title="item.prop">{{
							item.label
						}}</span>
					</li>
				</ul>
			</div>
			<div class="setting-column__bottom">
				<el-button @click="cancel" :disabled="isSave">取消</el-button>
				<el-button @click="save" type="primary">确定导出</el-button>
			</div>
		</div>
		<el-empty
			v-else
			description="暂无可配置的列"
			:image-size="80"
		></el-empty>
	</div>
</template>

<script>
import Sortable from "sortablejs";
import Export from "@/utils/exportToExcel";

export default {
	components: {
		Sortable,
	},
	props: {
		column: { type: Object, default: () => {} },
		data: { type: Object, default: () => {} },
	},
	data() {
		return {
			isSave: false,
			usercolumn: JSON.parse(JSON.stringify(this.column || [])),
			excelcolumn: [],
		};
	},
	mounted() {
		this.usercolumn.length > 0 && this.rowDrop();
	},
	methods: {
		rowDrop() {
			const _this = this;
			const tbody = this.$refs.list.querySelector("ul");
			Sortable.create(tbody, {
				handle: ".move",
				animation: 300,
				ghostClass: "ghost",
				onEnd({ newIndex, oldIndex }) {
					const tableData = _this.usercolumn;
					const currRow = tableData.splice(oldIndex, 1)[0];
					tableData.splice(newIndex, 0, currRow);
				},
			});
		},
		cancel() {
			this.$emit("excelCancel");
		},
		save() {
			this.excelcolumn = this.usercolumn;
            let fields={}
            this.excelcolumn.forEach(item => {
                if(!item.hide){
                    eval("fields."+item.prop+"='"+item.label+"';");
                }
            });
            Export(JSON.parse(JSON.stringify(this.data || [])), fields, "文档-"+new Date().getTime());
            this.$message.success("导出成功");
		},
	},
};
</script>

<style scoped>
.setting-column {
}
.print-main {
	display: none;
}
.setting-column__title {
	border-bottom: 1px solid #ebeef5;
	padding-bottom: 15px;
}
.setting-column__title span {
	display: inline-block;
	font-weight: bold;
	color: #909399;
	font-size: 12px;
}
.setting-column__title span.move_b {
	width: 30px;
	margin-right: 15px;
}
.setting-column__title span.show_b {
	width: 60px;
}
.setting-column__title span.name_b {
	width: 140px;
}
.setting-column__title span.width_b {
	width: 60px;
	margin-right: 15px;
}
.setting-column__title span.sortable_b {
	width: 60px;
}
.setting-column__title span.fixed_b {
	width: 60px;
}

.setting-column__list {
	max-height: 314px;
	overflow: auto;
}
.setting-column__list li {
	list-style: none;
	margin: 10px 0;
	display: flex;
	align-items: center;
}
.setting-column__list li > span {
	display: inline-block;
	font-size: 12px;
}
.setting-column__list li span.move_b {
	width: 30px;
	margin-right: 15px;
}
.setting-column__list li span.show_b {
	width: 60px;
}
.setting-column__list li span.name_b {
	width: 140px;
	white-space: nowrap;
	text-overflow: ellipsis;
	overflow: hidden;
	cursor: default;
}
.setting-column__list li span.width_b {
	width: 60px;
	margin-right: 15px;
}
.setting-column__list li span.sortable_b {
	width: 60px;
}
.setting-column__list li span.fixed_b {
	width: 60px;
}
.setting-column__list li.ghost {
	opacity: 0.3;
}

.setting-column__bottom {
	border-top: 1px solid #ebeef5;
	padding-top: 15px;
	text-align: right;
}
</style>
