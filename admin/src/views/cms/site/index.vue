<template>
  <el-splitter>
    <el-splitter-panel size="23%">
      <div class="bg-card border-border h-full rounded-[.5vw] p-2">
        <div
          class="mb-3 mt-1 flex items-center justify-between text-[14.5px] font-medium"
        >
          <div class="flex items-center">
            <el-icon class="mr-2" size="20px"><Menu /></el-icon> 站点列表
          </div>
          <el-button type="primary" icon="Plus" circle @click="headerAdd" />
        </div>
        <div class="space-y-2">
          <div
            v-for="site in sites"
            :key="site.id"
            class="hover:bg-muted/40 hover:ring-muted/60 group flex cursor-pointer items-center gap-3 rounded-md p-2 ring-1 ring-transparent"
            :class="
              selectedId === site.id ? 'bg-primary/10 ring-primary/50' : ''
            "
            @click="selectSite(site)"
          >
            <img
              :src="resolveUrl(site.logo)"
              :alt="site.name"
              class="border-border bg-muted h-[40px] w-[40px] rounded-full border object-cover"
            />
            <div class="min-w-0 flex-1">
              <span class="text-foreground block truncate text-sm">{{
                site.name
              }}</span>
            </div>
            <el-popconfirm
              title="确认删除该站点？"
              @confirm="removeSite(site.id)"
            >
              <template #reference>
                <el-button
                  link
                  type="danger"
                  size="small"
                  class="ml-auto opacity-0 transition-opacity group-hover:opacity-100"
                  @click.stop
                >
                  <el-icon><Delete /></el-icon>
                </el-button>
              </template>
            </el-popconfirm>
          </div>
        </div>
      </div>
    </el-splitter-panel>
    <el-splitter-panel :min="200" class="pl-2">
      <div class="bg-card border-border h-full rounded-[.5vw] p-2 pl-3">
        <el-scrollbar height="85vh">
          <info ref="infoRef" @complete="init"></info>
        </el-scrollbar>
      </div>
    </el-splitter-panel>
  </el-splitter>
</template>
<script setup lang="ts">
import { fetchCmsSiteList, deleteCmsSite } from "@/api/cms/site";
import { resolveUrl } from "@/utils/tools";
const info = defineAsyncComponent(() => import("./info.vue"));
const infoRef = ref();
type SiteItem = {
  id: string | number;
  name: string;
  logo: string;
};

const sites = ref<SiteItem[]>([]);

const selectedId = ref<string | number | null>(null);

function selectSite(site: SiteItem) {
  selectedId.value = site.id;
  infoRef.value.openModal(site.id);
}

async function removeSite(id: string | number) {
  await deleteCmsSite([id]);
  await init();
  headerAdd();
  selectedId.value = null;
  ElMessage.success("已删除");
}

const headerAdd = () => {
  infoRef.value.openModal();
};
const init = async () => {
  sites.value = await fetchCmsSiteList();
  if (sites.value.length > 0) {
    setTimeout(() => {
      selectSite(sites.value[0]);
    }, 100);
  }
};
onMounted(async () => {
  await init();
});
</script>
