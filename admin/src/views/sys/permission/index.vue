<template>
  <el-container class="h-full">
    <el-aside width="240px" class="h-full pr-2">
      <div
        ref="targetEl"
        class="bg-card border-border org-tree h-full w-full rounded-[.5vw] p-3"
      >
        <div class="menu-card__header">
          <span
            ><el-icon class="mr-2"><Stamp /></el-icon>角色列表</span
          >
        </div>
        <el-main>
          <el-scrollbar height="600px">
            <el-tree
              ref="treeRef"
              :data="treeData"
              node-key="id"
              default-expand-all
              @node-click="groupClick"
            />
          </el-scrollbar>
        </el-main>
      </div>
    </el-aside>
    <el-container>
      <el-header
        class="bg-card border-border flex flex-wrap items-center justify-between gap-3 rounded-[.5vw]"
        ><span v-if="nodeNow.id !== '0'">正在为《{{ nodeNow.name }}》授权</span
        ><span v-if="nodeNow.id === '0'">请选择角色授权</span></el-header
      >
      <el-main class="bg-card border-border org-tree mb-2 mt-2 rounded-[.5vw]">
        <el-scrollbar>
          <el-tree
            ref="menuTreeRef"
            class="menu"
            :data="menuTree"
            show-checkbox
            node-key="number"
            default-expand-all
            :render-content="renderContent"
          ></el-tree>
        </el-scrollbar>
      </el-main>
      <el-footer class="perm-footer">
        <div class="bg-card border-border rounded-[.5vw] p-3 text-center">
          <el-button
            type="primary"
            :disabled="btnDisable"
            :loading="isSaveing"
            round
            @click="saveAuthorize"
          >
            保存授权
          </el-button>
        </div>
      </el-footer>
    </el-container>
  </el-container>
</template>
<script setup lang="ts">
import { changeTree, uuid } from "@/utils/tools";
import {
  fetchRoleList,
  sysmenuList,
  addRolePermission,
  fetchPermissionByRole,
} from "@/api";
const treeData = ref([]);
const targetEl = ref();
const nodeNow = ref({ id: "0", name: undefined });
const menuTree = ref([]);
const menuTreeRef = ref();
const btnDisable = ref(true);
const isSaveing = ref(false);
const numberMenu = ref([]);

onMounted(async () => {
  const roles = await fetchRoleList();
  roles.forEach((m: any) => {
    m.label = m.name;
    m.value = m.id;
  });
  treeData.value = changeTree(roles);

  const menus = await sysmenuList();
  let _menutree: any = [];
  menus.some((m: any) => {
    _menutree.push({
      id: m.id,
      number: uuid(),
      value: m.id,
      label: m.name,
      parentId: m.parentId,
    });
    m.api.some((r: any) => {
      _menutree.push({
        id: 0,
        number: uuid(),
        value: r.url,
        label: r.name,
        code: r.code,
        method: r.method,
        parentId: m.id,
      });
    });
  });
  numberMenu.value = _menutree;
  menuTree.value = changeTree(_menutree);
  treeCss();
});
const treeCss = () => {
  nextTick(() => {
    const levelName = document.getElementsByClassName("floatRight");
    for (let i = 0; i < levelName.length; i++) {
      let parentNode = levelName[i].parentNode;
      parentNode.style.cssFloat = "left";
      parentNode.style.styleFloat = "left";
    }
    const clearFloat = document.getElementsByClassName("clearFloat");
    for (let j = 0; j < clearFloat.length; j++) {
      let parentNode = clearFloat[j].parentNode;
      parentNode.style.clear = "both";
      parentNode.style.clear = "both";
    }
  });
};

const groupClick = async (node: any) => {
  nodeNow.value = node;
  const menu = await fetchPermissionByRole(node.id);
  btnDisable.value = false;
  let selectNode: any = [];
  menu.forEach((item: any) => {
    numberMenu.value.forEach((m) => {
      if (item.menuId === m.id) {
        selectNode.push(m);
      }
    });
    item.api.forEach((api: any) => {
      numberMenu.value.forEach((m) => {
        if (api.code == m.code && api.name == m.label) {
          selectNode.push(m);
        }
      });
    });
  });
  menuTreeRef.value!.setCheckedNodes(selectNode, false);
};
const renderContent = (h, { node }) => {
  let classname = "";
  if (node.childNodes.length === 0) {
    classname = "floatRight";
  } else if (node.childNodes.length > 0) {
    classname = "clearFloat";
  }
  return h(
    "span",
    {
      class: ["el-tree-node__label", classname],
    },
    node.label
  );
};
const saveAuthorize = async () => {
  var nodes = menuTreeRef.value.getCheckedNodes();
  let childArr = menuTreeRef.value.getHalfCheckedNodes();
  nodes = nodes.concat(childArr);
  if (nodes.length == 0) {
    ElMessage.warning("请勾选授权的菜单信息~");
    return;
  }
  let _menus: any = [];
  nodes.forEach((item: any) => {
    if (item.id != 0) {
      let _api: any = [];
      var apiArr = nodes.filter((m) => item.id == m.parentId && m.id == 0);
      apiArr.forEach((row: any) => {
        _api.push({
          name: row.label,
          code: row.code,
          method: row.method,
          url: row.value,
        });
      });
      _menus.push({ menuId: item.id, api: _api });
    }
  });
  var data = { roleId: nodeNow.value.id, menus: _menus };
  isSaveing.value = true;
  await addRolePermission(data);
  isSaveing.value = false;
  ElMessage.success("授权成功~");
};
</script>
<style scoped>
.menu-card__header {
  padding: 5px 10px 10px 10px;
  border-bottom: 1px solid #2f3033;
  margin-bottom: 5px;
}
:deep(.perm-footer) {
  padding: 0px !important;
}
:deep(.org-tree .el-tree-node__content) {
  height: 36px;
  border-radius: 8px;
}
</style>
