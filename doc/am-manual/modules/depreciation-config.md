# 折旧配置（每个资产怎么折旧）

折旧配置用于定义“每一项资产的折旧方法与参数”，是折旧计提的输入来源。

## 1) 你在这里要做什么

- 为每个资产配置折旧方法（不折旧/直线法/双倍余额/年数总和）
- 配置折旧期（月）、残值率、开始日期等
- 维护累计折旧、上次计提期间、折旧状态等

## 2) 字段说明（常用）

| 字段 | 含义 |
|---|---|
| 资产Id（AssetId） | 哪个资产 |
| 折旧方法（Method） | 0不折旧/1直线法/2双倍余额/3年数总和 |
| 折旧期（LifeMonths） | 以月为单位 |
| 残值率（SalvageRate） | % |
| 开始日期（StartDate） | 从何时开始计提 |
| 累计折旧（AccumAmount） | 已计提的折旧金额 |
| 上次期间（LastPeriod） | 上次计提到哪个月（YYYY-MM） |
| 状态（Status） | 0未启用/1折旧中/2已停用 |

## 3) 什么时候需要配置折旧

通常在以下情况配置/调整：
- 新增资产入账后
- 折旧政策变更（例如折旧期调整）
- 资产转固/启用
- 资产处置或停止折旧

## 4) 与其它模块的关系

- 资产台账：折旧配置以资产为维度，资产的原值/净值可能与折旧相关
- 折旧计提：每月根据折旧配置生成计提批次与明细
- 资产留痕：若你们需要审计，可把折旧配置的关键调整写入留痕（可选）

## 5) 净值计算（手动/调度）

系统已提供“按折旧配置计算资产净值”的任务方法，仅对**状态=折旧中**的配置生效。

- 调度任务方法：`FytSoa.TaskJob, FytSoa.ApiService`  
  - `AssetDepreciationNetValueAsync`
- 手动触发接口（可选）：`/api/amassetdepreciation/recalcNetBookValue`（POST）

appsettings 配置（Manual=仅手动；非 Manual=允许调度/自动执行）：

```json
"Depreciation": {
  "AssetDepreciationMode": "Manual",
  "NetValueOperatorId": "1678330902595375105"
}
```

说明：
- 当净值更新时，会写入资产留痕（Operation=NET_VALUE）
- 留痕操作人来自 `Depreciation:NetValueOperatorId`
