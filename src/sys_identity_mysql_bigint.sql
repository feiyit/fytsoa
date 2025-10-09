
-- =====================================================================
-- 工作流系统 - 系统基础库（MySQL 8.0，无外键；表名前缀 sys_*；主键 BIGINT）
-- 说明：本库仅包含“系统级用户/组织/RBAC/登录”相关表。
-- 建表策略：
--   - 主键：BIGINT UNSIGNED，自增（AUTO_INCREMENT）或应用侧生成雪花ID皆可；此处默认自增。
--   - 多租户：tenant_id 作为首要过滤列。
--   - 无外键：通过索引与注释标识逻辑关联，便于分库分表与异构同步。
--   - 时间：DATETIME(3)，UTC；created_at/updated_at 默认值与自动更新。
-- =====================================================================

SET NAMES utf8mb4 COLLATE utf8mb4_0900_ai_ci;

-- 清理（按依赖顺序）
DROP TABLE IF EXISTS sys_user_session;
DROP TABLE IF EXISTS sys_login_audit;
DROP TABLE IF EXISTS sys_password_reset_token;
DROP TABLE IF EXISTS sys_user_role;
DROP TABLE IF EXISTS sys_role_permission;
DROP TABLE IF EXISTS sys_permission;
DROP TABLE IF EXISTS sys_role;
DROP TABLE IF EXISTS sys_delegation;
DROP TABLE IF EXISTS sys_org_unit_head;
DROP TABLE IF EXISTS sys_employment_reporting;
DROP TABLE IF EXISTS sys_employment;
DROP TABLE IF EXISTS sys_position;
DROP TABLE IF EXISTS sys_org_unit_closure;
DROP TABLE IF EXISTS sys_org_unit;
DROP TABLE IF EXISTS sys_user_account;
DROP TABLE IF EXISTS sys_tenant;

CREATE TABLE sys_tenant (
  id           BIGINT UNSIGNED NOT NULL AUTO_INCREMENT COMMENT '主键：租户ID',
  code         VARCHAR(64) NOT NULL COMMENT '租户编码（唯一，建议英数字）',
  name         VARCHAR(200)NOT NULL COMMENT '租户名称',
  created_at   DATETIME(3) NOT NULL DEFAULT CURRENT_TIMESTAMP(3) COMMENT '创建时间（UTC）',
  PRIMARY KEY (id),
  UNIQUE KEY uk_sys_tenant_code (code)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci
COMMENT='【租户】系统多租户标识表；逻辑与其他表通过 tenant_id 关联';

CREATE TABLE sys_user_account (
  id                 BIGINT UNSIGNED NOT NULL AUTO_INCREMENT COMMENT '主键：用户ID',
  tenant_id          BIGINT UNSIGNED NOT NULL COMMENT '租户ID（逻辑外键：sys_tenant.id）',
  user_name          VARCHAR(100) NOT NULL COMMENT '登录名（租户内唯一；建议统一小写存储）',
  display_name       VARCHAR(200) NOT NULL COMMENT '显示名',
  email              VARCHAR(200) NULL     COMMENT '邮箱（租户内唯一，可空）',
  phone              VARCHAR(50)  NULL     COMMENT '手机号（可空）',
  password_hash      VARCHAR(255) NOT NULL COMMENT '口令哈希（argon2id/bcrypt 字符串）',
  password_algo      VARCHAR(32)  NOT NULL DEFAULT 'argon2id' COMMENT '哈希算法：argon2id/bcrypt 等',
  password_updated_at DATETIME(3) NOT NULL DEFAULT CURRENT_TIMESTAMP(3) COMMENT '口令最近更新时间（UTC）',
  must_change_password TINYINT(1) NOT NULL DEFAULT 0 COMMENT '是否下次登录强制改密：1是 0否',
  is_active          TINYINT(1)   NOT NULL DEFAULT 1 COMMENT '是否启用：1启用 0禁用',
  is_locked          TINYINT(1)   NOT NULL DEFAULT 0 COMMENT '是否锁定：1锁定 0正常',
  failed_login_count INT          NOT NULL DEFAULT 0 COMMENT '连续失败登录次数',
  last_login_at      DATETIME(3)  NULL     COMMENT '最近登录时间（UTC）',
  last_login_ip      VARCHAR(64)  NULL     COMMENT '最近登录IP',
  created_at         DATETIME(3)  NOT NULL DEFAULT CURRENT_TIMESTAMP(3) COMMENT '创建时间（UTC）',
  updated_at         DATETIME(3)  NOT NULL DEFAULT CURRENT_TIMESTAMP(3) ON UPDATE CURRENT_TIMESTAMP(3) COMMENT '更新时间（UTC）',
  PRIMARY KEY (id),
  KEY idx_sys_user_tenant (tenant_id),
  UNIQUE KEY uk_sys_user_tenant_username (tenant_id, user_name),
  UNIQUE KEY uk_sys_user_tenant_email    (tenant_id, email)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci
COMMENT='【用户账号】账户与密码登录信息；不建外键，依靠业务保证一致性';

CREATE TABLE sys_org_unit (
  id            BIGINT UNSIGNED NOT NULL AUTO_INCREMENT COMMENT '主键：组织ID',
  tenant_id     BIGINT UNSIGNED NOT NULL COMMENT '租户ID（逻辑外键：sys_tenant.id）',
  code          VARCHAR(64)  NOT NULL COMMENT '组织编码（租户内唯一）',
  name          VARCHAR(200) NOT NULL COMMENT '组织名称（部门/团队/分公司等）',
  parent_id     BIGINT UNSIGNED NULL     COMMENT '父组织ID（逻辑外键：sys_org_unit.id；根组织为空）',
  is_active     TINYINT(1)   NOT NULL DEFAULT 1 COMMENT '是否启用：1启用 0停用',
  created_at    DATETIME(3)  NOT NULL DEFAULT CURRENT_TIMESTAMP(3) COMMENT '创建时间（UTC）',
  updated_at    DATETIME(3)  NOT NULL DEFAULT CURRENT_TIMESTAMP(3) ON UPDATE CURRENT_TIMESTAMP(3) COMMENT '更新时间（UTC）',
  PRIMARY KEY (id),
  KEY idx_sys_org_tenant (tenant_id),
  KEY idx_sys_org_parent (tenant_id, parent_id),
  UNIQUE KEY uk_sys_org_code (tenant_id, code)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci
COMMENT='【组织】树形结构的部门/项目/业务单元；与 sys_org_unit_closure 支持祖先后代查询';

CREATE TABLE sys_org_unit_closure (
  tenant_id     BIGINT UNSIGNED NOT NULL COMMENT '租户ID（逻辑外键：sys_tenant.id）',
  ancestor_id   BIGINT UNSIGNED NOT NULL COMMENT '祖先组织ID（逻辑外键：sys_org_unit.id）',
  descendant_id BIGINT UNSIGNED NOT NULL COMMENT '后代组织ID（逻辑外键：sys_org_unit.id）',
  depth         INT             NOT NULL COMMENT '深度（祖先到后代的边数，祖先=后代时为0）',
  PRIMARY KEY (tenant_id, ancestor_id, descendant_id),
  KEY idx_sys_org_closure_ancestor (tenant_id, ancestor_id, depth),
  KEY idx_sys_org_closure_desc     (tenant_id, descendant_id, depth)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci
COMMENT='【组织闭包】用于快速查询任意组织的祖先/后代路径（含自身 depth=0）';

CREATE TABLE sys_position (
  id            BIGINT UNSIGNED NOT NULL AUTO_INCREMENT COMMENT '主键：岗位ID',
  tenant_id     BIGINT UNSIGNED NOT NULL COMMENT '租户ID（逻辑外键：sys_tenant.id）',
  code          VARCHAR(64)  NOT NULL COMMENT '岗位编码（租户内唯一）',
  name          VARCHAR(200) NOT NULL COMMENT '岗位名称（如：部门经理、财务专员）',
  parent_id     BIGINT UNSIGNED NULL     COMMENT '父岗位ID（逻辑外键：sys_position.id；可空）',
  is_managerial TINYINT(1)   NOT NULL DEFAULT 0 COMMENT '是否管理序列：1是 0否（参考字段）',
  is_active     TINYINT(1)   NOT NULL DEFAULT 1 COMMENT '是否启用',
  created_at    DATETIME(3)  NOT NULL DEFAULT CURRENT_TIMESTAMP(3) COMMENT '创建时间（UTC）',
  updated_at    DATETIME(3)  NOT NULL DEFAULT CURRENT_TIMESTAMP(3) ON UPDATE CURRENT_TIMESTAMP(3) COMMENT '更新时间（UTC）',
  PRIMARY KEY (id),
  KEY idx_sys_pos_tenant (tenant_id),
  KEY idx_sys_pos_parent (tenant_id, parent_id),
  UNIQUE KEY uk_sys_pos_code (tenant_id, code)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci
COMMENT='【岗位】岗位/职级目录，支持层级（如经理/主管/专员等）';

CREATE TABLE sys_employment (
  id              BIGINT UNSIGNED NOT NULL AUTO_INCREMENT COMMENT '主键：任用ID',
  tenant_id       BIGINT UNSIGNED NOT NULL COMMENT '租户ID（逻辑外键：sys_tenant.id）',
  user_id         BIGINT UNSIGNED NOT NULL COMMENT '用户ID（逻辑外键：sys_user_account.id）',
  org_id          BIGINT UNSIGNED NOT NULL COMMENT '组织ID（逻辑外键：sys_org_unit.id）',
  position_id     BIGINT UNSIGNED NOT NULL COMMENT '岗位ID（逻辑外键：sys_position.id）',
  is_primary      TINYINT(1)      NOT NULL DEFAULT 1 COMMENT '是否主任用：1主 0兼职',
  valid_from      DATETIME(3)     NOT NULL COMMENT '任用生效时间（UTC）',
  valid_to        DATETIME(3)     NULL     COMMENT '任用失效时间（UTC，NULL 表示当前有效）',
  note            VARCHAR(500)    NULL     COMMENT '备注（如试用/借调等）',
  created_at      DATETIME(3)     NOT NULL DEFAULT CURRENT_TIMESTAMP(3) COMMENT '创建时间（UTC）',
  updated_at      DATETIME(3)     NOT NULL DEFAULT CURRENT_TIMESTAMP(3) ON UPDATE CURRENT_TIMESTAMP(3) COMMENT '更新时间（UTC）',
  PRIMARY KEY (id),
  KEY idx_sys_emp_tenant_user (tenant_id, user_id),
  KEY idx_sys_emp_tenant_org  (tenant_id, org_id),
  KEY idx_sys_emp_tenant_pos  (tenant_id, position_id),
  KEY idx_sys_emp_valid_range (tenant_id, valid_from, valid_to),
  UNIQUE KEY uk_sys_emp_unique (tenant_id, user_id, org_id, position_id, valid_from)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci
COMMENT='【任用】用户在某组织担任某岗位的关系；支持历史与多岗位';

CREATE TABLE sys_employment_reporting (
  id                   BIGINT UNSIGNED NOT NULL AUTO_INCREMENT COMMENT '主键：任用汇报关系ID',
  tenant_id            BIGINT UNSIGNED NOT NULL COMMENT '租户ID（逻辑外键：sys_tenant.id）',
  subordinate_emp_id   BIGINT UNSIGNED NOT NULL COMMENT '下属任用ID（逻辑外键：sys_employment.id）',
  manager_emp_id       BIGINT UNSIGNED NOT NULL COMMENT '上级任用ID（逻辑外键：sys_employment.id）',
  relation             VARCHAR(20)     NOT NULL COMMENT '关系类型：LINE(直属)/FUNCTIONAL(职能)',
  valid_from           DATETIME(3)     NOT NULL COMMENT '关系生效时间（UTC）',
  valid_to             DATETIME(3)     NULL     COMMENT '关系失效时间（UTC，NULL 表示当前有效）',
  note                 VARCHAR(500)    NULL     COMMENT '备注',
  created_at           DATETIME(3)     NOT NULL DEFAULT CURRENT_TIMESTAMP(3) COMMENT '创建时间（UTC）',
  updated_at           DATETIME(3)     NOT NULL DEFAULT CURRENT_TIMESTAMP(3) ON UPDATE CURRENT_TIMESTAMP(3) COMMENT '更新时间（UTC）',
  PRIMARY KEY (id),
  KEY idx_sys_report_sub (tenant_id, subordinate_emp_id),
  KEY idx_sys_report_mgr (tenant_id, manager_emp_id),
  KEY idx_sys_report_rng (tenant_id, valid_from, valid_to),
  UNIQUE KEY uk_sys_report_unique (tenant_id, subordinate_emp_id, manager_emp_id, relation, valid_from)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci
COMMENT='【任用汇报】定义任用到任用的上下级关系；用于计算直属主管/N级主管';

CREATE TABLE sys_org_unit_head (
  id              BIGINT UNSIGNED NOT NULL AUTO_INCREMENT COMMENT '主键：组织负责人ID',
  tenant_id       BIGINT UNSIGNED NOT NULL COMMENT '租户ID（逻辑外键：sys_tenant.id）',
  org_id          BIGINT UNSIGNED NOT NULL COMMENT '组织ID（逻辑外键：sys_org_unit.id）',
  employment_id   BIGINT UNSIGNED NOT NULL COMMENT '负责人任用ID（逻辑外键：sys_employment.id）',
  head_type       VARCHAR(20)     NOT NULL DEFAULT 'PRIMARY' COMMENT '负责人类型：PRIMARY/DEPUTY/ACTING',
  valid_from      DATETIME(3)     NOT NULL COMMENT '生效时间（UTC）',
  valid_to        DATETIME(3)     NULL     COMMENT '失效时间（UTC，NULL 表示当前有效）',
  note            VARCHAR(500)    NULL     COMMENT '备注',
  created_at      DATETIME(3)     NOT NULL DEFAULT CURRENT_TIMESTAMP(3) COMMENT '创建时间（UTC）',
  updated_at      DATETIME(3)     NOT NULL DEFAULT CURRENT_TIMESTAMP(3) ON UPDATE CURRENT_TIMESTAMP(3) COMMENT '更新时间（UTC）',
  PRIMARY KEY (id),
  KEY idx_sys_head_org (tenant_id, org_id),
  KEY idx_sys_head_emp (tenant_id, employment_id),
  UNIQUE KEY uk_sys_head_unique (tenant_id, org_id, employment_id, head_type, valid_from)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci
COMMENT='【组织负责人】定义某组织的负责人任用；用于“最近部门负责人”解析';

CREATE TABLE sys_role (
  id          BIGINT UNSIGNED NOT NULL AUTO_INCREMENT COMMENT '主键：角色ID',
  tenant_id   BIGINT UNSIGNED NOT NULL COMMENT '租户ID（逻辑外键：sys_tenant.id）',
  code        VARCHAR(64)  NOT NULL COMMENT '角色编码（租户内唯一）',
  name        VARCHAR(200) NOT NULL COMMENT '角色名称',
  is_system   TINYINT(1)   NOT NULL DEFAULT 0 COMMENT '是否系统内置：1是 0否',
  created_at  DATETIME(3)  NOT NULL DEFAULT CURRENT_TIMESTAMP(3) COMMENT '创建时间（UTC）',
  updated_at  DATETIME(3)  NOT NULL DEFAULT CURRENT_TIMESTAMP(3) ON UPDATE CURRENT_TIMESTAMP(3) COMMENT '更新时间（UTC）',
  PRIMARY KEY (id),
  KEY idx_sys_role_tenant (tenant_id),
  UNIQUE KEY uk_sys_role_code (tenant_id, code)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci
COMMENT='【角色】RBAC 角色目录';

CREATE TABLE sys_permission (
  id          BIGINT UNSIGNED NOT NULL AUTO_INCREMENT COMMENT '主键：权限点ID',
  code        VARCHAR(100) NOT NULL COMMENT '权限点编码（系统全局唯一）',
  name        VARCHAR(200) NOT NULL COMMENT '权限点名称',
  created_at  DATETIME(3)  NOT NULL DEFAULT CURRENT_TIMESTAMP(3) COMMENT '创建时间（UTC）',
  PRIMARY KEY (id),
  UNIQUE KEY uk_sys_perm_code (code)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci
COMMENT='【权限点】系统功能点字典；通常在运维中台统一维护';

CREATE TABLE sys_role_permission (
  role_id       BIGINT UNSIGNED NOT NULL COMMENT '角色ID（逻辑外键：sys_role.id）',
  permission_id BIGINT UNSIGNED NOT NULL COMMENT '权限点ID（逻辑外键：sys_permission.id）',
  assigned_at   DATETIME(3) NOT NULL DEFAULT CURRENT_TIMESTAMP(3) COMMENT '分配时间（UTC）',
  PRIMARY KEY (role_id, permission_id),
  KEY idx_sys_rp_role (role_id),
  KEY idx_sys_rp_perm (permission_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci
COMMENT='【角色-权限】角色与权限点的多对多关系表（无外键）';

CREATE TABLE sys_user_role (
  user_id     BIGINT UNSIGNED NOT NULL COMMENT '用户ID（逻辑外键：sys_user_account.id）',
  role_id     BIGINT UNSIGNED NOT NULL COMMENT '角色ID（逻辑外键：sys_role.id）',
  assigned_at DATETIME(3) NOT NULL DEFAULT CURRENT_TIMESTAMP(3) COMMENT '分配时间（UTC）',
  PRIMARY KEY (user_id, role_id),
  KEY idx_sys_ur_user (user_id),
  KEY idx_sys_ur_role (role_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci
COMMENT='【用户-角色】用户与角色的多对多关系表（无外键）';

CREATE TABLE sys_delegation (
  id            BIGINT UNSIGNED NOT NULL AUTO_INCREMENT COMMENT '主键：代理ID',
  tenant_id     BIGINT UNSIGNED NOT NULL COMMENT '租户ID（逻辑外键：sys_tenant.id）',
  delegator_id  BIGINT UNSIGNED NOT NULL COMMENT '被代理人用户ID（逻辑外键：sys_user_account.id）',
  delegatee_id  BIGINT UNSIGNED NOT NULL COMMENT '代理人用户ID（逻辑外键：sys_user_account.id）',
  scope         VARCHAR(200) NOT NULL DEFAULT 'ALL' COMMENT '代理范围：ALL / PROCESS:{code} / ORG:{id} / JSON 扩展',
  start_at      DATETIME(3)  NOT NULL COMMENT '开始时间（UTC）',
  end_at        DATETIME(3)  NOT NULL COMMENT '结束时间（UTC）',
  is_active     TINYINT(1)   NOT NULL DEFAULT 1 COMMENT '是否启用：1启用 0停用',
  note          VARCHAR(500) NULL COMMENT '备注',
  created_at    DATETIME(3)  NOT NULL DEFAULT CURRENT_TIMESTAMP(3) COMMENT '创建时间（UTC）',
  updated_at    DATETIME(3)  NOT NULL DEFAULT CURRENT_TIMESTAMP(3) ON UPDATE CURRENT_TIMESTAMP(3) COMMENT '更新时间（UTC）',
  PRIMARY KEY (id),
  KEY idx_sys_delegation_tenant (tenant_id),
  KEY idx_sys_delegation_user   (tenant_id, delegator_id),
  KEY idx_sys_delegation_agent  (tenant_id, delegatee_id),
  KEY idx_sys_delegation_time   (tenant_id, start_at, end_at)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci
COMMENT='【审批代理】在时间窗内将被代理人的任务转交代理人处理；指派时需合并代理人候选';

CREATE TABLE sys_password_reset_token (
  id             BIGINT UNSIGNED NOT NULL AUTO_INCREMENT COMMENT '主键：重置令牌ID',
  tenant_id      BIGINT UNSIGNED NOT NULL COMMENT '租户ID（逻辑外键：sys_tenant.id）',
  user_id        BIGINT UNSIGNED NOT NULL COMMENT '用户ID（逻辑外键：sys_user_account.id）',
  token_hash     VARCHAR(255)NOT NULL COMMENT '令牌哈希（只存哈希，原始令牌仅发给用户）',
  expires_at     DATETIME(3) NOT NULL COMMENT '过期时间（UTC）',
  used_at        DATETIME(3) NULL     COMMENT '实际使用时间（UTC）',
  created_at     DATETIME(3) NOT NULL DEFAULT CURRENT_TIMESTAMP(3) COMMENT '创建时间（UTC）',
  PRIMARY KEY (id),
  KEY idx_sys_prt_lookup (tenant_id, user_id, expires_at)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci
COMMENT='【密码重置令牌】用于找回密码/安全改密；仅存储哈希与过期时间';

CREATE TABLE sys_user_session (
  id                 BIGINT UNSIGNED NOT NULL AUTO_INCREMENT COMMENT '主键：会话ID',
  tenant_id          BIGINT UNSIGNED NOT NULL COMMENT '租户ID（逻辑外键：sys_tenant.id）',
  user_id            BIGINT UNSIGNED NOT NULL COMMENT '用户ID（逻辑外键：sys_user_account.id）',
  refresh_token_hash VARCHAR(255) NOT NULL COMMENT '刷新令牌哈希（只存哈希）',
  issued_at          DATETIME(3)  NOT NULL DEFAULT CURRENT_TIMESTAMP(3) COMMENT '签发时间（UTC）',
  expires_at         DATETIME(3)  NOT NULL COMMENT '过期时间（UTC）',
  revoked_at         DATETIME(3)  NULL     COMMENT '撤销时间（UTC）',
  user_agent         VARCHAR(500) NULL     COMMENT 'UA 信息（浏览器/APP 版本等）',
  ip_address         VARCHAR(64)  NULL     COMMENT '登录IP（可记录代理链）',
  metadata           JSON         NULL     COMMENT '可选：设备指纹、地理信息等',
  PRIMARY KEY (id),
  KEY idx_sys_session_user   (tenant_id, user_id),
  KEY idx_sys_session_active (tenant_id, user_id, expires_at)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci
COMMENT='【登录会话】管理刷新令牌/多设备登录；支持单点登出与黑名单';

CREATE TABLE sys_login_audit (
  id            BIGINT UNSIGNED NOT NULL AUTO_INCREMENT COMMENT '主键：登录审计ID',
  tenant_id     BIGINT UNSIGNED NOT NULL COMMENT '租户ID（逻辑外键：sys_tenant.id）',
  user_id       BIGINT UNSIGNED NULL     COMMENT '用户ID（可空：用户名不存在或失败时为空）',
  user_name     VARCHAR(100) NOT NULL COMMENT '尝试登录名（原样记录）',
  is_success    TINYINT(1)   NOT NULL COMMENT '是否成功：1成功 0失败',
  reason        VARCHAR(200) NULL     COMMENT '失败原因标签（如 PASSWORD_MISMATCH/LOCKED 等）',
  ip_address    VARCHAR(64)  NULL     COMMENT '来源IP',
  user_agent    VARCHAR(500) NULL     COMMENT 'UA 信息',
  occurred_at   DATETIME(3)  NOT NULL DEFAULT CURRENT_TIMESTAMP(3) COMMENT '发生时间（UTC）',
  PRIMARY KEY (id),
  KEY idx_sys_login_tenant_time (tenant_id, occurred_at),
  KEY idx_sys_login_user_time   (tenant_id, user_id, occurred_at)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci
COMMENT='【登录审计】记录所有登录尝试（成功/失败），用于风控与合规审计';
