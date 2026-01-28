# 折旧计提（按月生成批次与明细）

折旧计提模块用于记录“某个月份发生了多少折旧”，并保存明细（每个资产本期折旧金额）。

## 1) 你在这里要做什么

- 选择期间（YYYY-MM），创建计提批次
- 录入/生成明细（资产Id、本期折旧金额等）
- 确认/过账（将批次状态置为已确认）
- 导出计提结果（用于财务入账或报表）

## 2) 批次字段说明（常用）

| 字段 | 含义 |
|---|---|
| 期间（Period） | YYYY-MM |
| 状态（Status） | 0草稿/1已过账或确认 |
| 执行人（RunUserId） | 谁执行的计提 |
| 执行时间（RunTime） | 执行时间 |
| 本期合计（TotalAmount） | 明细金额合计 |
| 备注（Remark） | 说明 |

## 3) 明细字段（常见）

每行通常包含：
- 资产Id
- 本期折旧金额（Amount）
- 备注/扩展字段（按你们页面为准）

## 4) 推荐流程

```mermaid
flowchart LR
  A[选择期间] --> B[创建批次(草稿)]
  B --> C[填入/生成明细]
  C --> D[核对合计]
  D --> E[确认/过账]
```

## 5) 与其它模块的关系

- 折旧配置：计提的输入来源（每个资产怎么折旧、折旧期、残值率等）
- 资产台账：用于报表展示（资产维度的价值变化）
- 提醒：可选（例如提醒“每月计提”）

## 6) 净值回写（手动/调度）

系统支持“按计提明细回写资产净值”，仅对**状态=已确认**的批次生效。

- 调度任务方法：`FytSoa.TaskJob, FytSoa.ApiService`  
  - `DepreciationRunNetValueAsync`
- 手动触发接口（可选）：`/api/amdepreciationrun/syncNetBookValue`（POST）

appsettings 配置说明：

```json
"Depreciation": {
  "DepreciationRunMode": "Manual",
  "NetValueOperatorId": "1678330902595375105"
}
```

模式影响：
- Manual：确认计提批次时**不自动更新**资产净值，仅手动触发/调度时更新
- 非 Manual：批次**已确认**即刻回写资产净值

留痕：
- 净值回写会写入资产留痕（Operation=NET_VALUE）
- 留痕操作人来自 `Depreciation:NetValueOperatorId`
