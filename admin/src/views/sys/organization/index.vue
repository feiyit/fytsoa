<template>
  <div class="h-full">
    <el-card shadow="never" class="h-full">
      <el-tabs v-model="activeTab" style="height: 100%">
        <!-- 组织结构 -->
        <el-tab-pane label="组织结构" name="org" style="height: 95%">
          <OrgStructureTab :tenant-id="currentTenantId" />
        </el-tab-pane>

        <!-- 岗位管理 -->
        <el-tab-pane label="岗位管理" name="position" style="height: 95%">
          <PositionTab
            :tenant-id="currentTenantId"
            @positions-changed="loadPositionOptions"
          />
        </el-tab-pane>

        <!-- 任用管理 -->
        <el-tab-pane label="任用管理" name="employment" style="height: 95%">
          <EmploymentTab
            :user-options="userOptions"
            :tenant-id="currentTenantId"
            :position-options="positionOptions"
            @employment-changed="onEmploymentChanged"
          />
        </el-tab-pane>

        <el-tab-pane label="汇报关系" name="reporting" style="height: 95%">
          <ReportingTab ref="tableReportingRef" :tenant-id="currentTenantId" />
        </el-tab-pane>

        <el-tab-pane label="组织负责人" name="orgHead" style="height: 95%">
          <OrgHeadTab
            ref="tableOrgHeadRef"
            :tenant-id="currentTenantId"
            @org-head-changed="loadOrgHeadList"
          />
        </el-tab-pane>
      </el-tabs>
    </el-card>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from "vue";
import OrgStructureTab from "./components/OrgStructureTab.vue";
import PositionTab from "./components/PositionTab.vue";
import EmploymentTab from "./components/EmploymentTab.vue";
import ReportingTab from "./components/ReportingTab.vue";
import OrgHeadTab from "./components/OrgHeadTab.vue";
import { fetchPositionList, fetchAdminList } from "@/api";
import type { Position, UserAccount } from "@/types/identity";

const currentTenantId = ref<number>(0);

const activeTab = ref<
  "org" | "position" | "employment" | "reporting" | "orgHead"
>("org");

const positionOptions = ref<Position[]>([]);
const userOptions = ref<UserAccount[]>([]);
const tableReportingRef = ref();
const tableOrgHeadRef = ref();
// 仅用于加载岗位下拉数据，表格本身的查询与刷新交给 PositionTab 内部处理
const loadPositionOptions = async () => {
  positionOptions.value = await fetchPositionList();
};

const loadUserOptions = async () => {
  userOptions.value = await fetchAdminList({});
};

const loadReportingList = async () => {
  tableReportingRef.value?.refresh(currentTenantId.value ?? 0);
};

const loadOrgHeadList = async () => {
  tableOrgHeadRef.value?.refresh(currentTenantId.value ?? 0);
};

// 任用数据发生变更时，联动刷新“汇报关系”和“组织负责人”相关数据
const onEmploymentChanged = async () => {
  await Promise.all([loadReportingList(), loadOrgHeadList()]);
};

const refreshAll = async () => {
  await Promise.all([loadPositionOptions(), loadUserOptions()]);
  await Promise.all([loadReportingList(), loadOrgHeadList()]);
};

onMounted(async () => {
  await refreshAll();
});
</script>

<style scoped>
.el-table {
  --el-table-border-color: theme("colors.gray.200");
}
:deep(.el-card__body, .el-tabs__content, .el-tabs, .el-tabs--top) {
  height: 100%;
}
</style>
