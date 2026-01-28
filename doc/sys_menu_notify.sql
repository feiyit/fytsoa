-- 消息通知菜单（配置中心 -> 消息通知）
-- ParentId = 1984190103970516992（配置中心）
-- 说明：如果你们数据库里 Id 使用雪花/Unique.Id()，也可以把下面的 Id 替换成系统生成的新值。

INSERT INTO `sys_menu`
(`Id`, `Name`, `ParentId`, `ParentIdList`, `Code`, `Layer`, `Urls`, `Redirect`, `VuePath`,
 `Icon`, `Active`, `Color`, `Sort`, `Fullpage`, `Status`, `IsDel`, `Types`, `Api`,
 `CreateTime`, `CreateUser`, `UpdateTime`, `UpdateUser`, `TenantId`)
VALUES
(2026012800000000000, '消息通知', 1984190103970516992, '[]', 'config-notify', 1, '/config/notify', NULL, 'config/notify/index.vue',
 'lucide:bell', NULL, NULL, 304, b'0', b'1', b'0', 'menu', '[]',
 '2026-01-28 12:00:00', NULL, '2026-01-28 12:00:00', '系统', 0);

-- 可选：给超级管理员角色补一条权限（按你们实际 RoleId 调整）
-- INSERT INTO `sys_permission` (`RoleId`, `MenuId`, `Api`, `Types`, `TenantId`)
-- VALUES (1339756014718816256, 2026012800000000000, '[]', 3, 0);

