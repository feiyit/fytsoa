# 资产留痕（资产溯源/审计轨迹）

资产留痕用于把资产相关的关键操作记录成“时间线”，回答这些问题：

- 这台资产是谁在什么时候建档的？
- 为什么资产现在在这个地点/这个人名下？
- 这段时间资产发生了哪些业务（调拨/维修/盘点/处置）？
- 出现差异时，能否定位到具体单据/工单/盘点记录？

## 1) 留痕记录长什么样

一条留痕记录通常包含：

- 资产Id（AssetId）
- 业务类型（BizType）：例如 ASSET / INBOUND / TRANSFER / MAINTENANCE / INVENTORY 等
- 业务Id（BizId）：对应业务对象的主键（例如：单据Id、工单Id、盘点计划Id）
- 操作（Operation）：例如 CREATE / UPDATE / DELETE / SCAN 等
- 操作人Id（OperatorId）
- 操作时间（OperateTime）
- 备注（Remark）
- 变更前（BeforeJson）与变更后（AfterJson）（JSON）

## 2) 业务类型（BizType）建议口径

常见值（下拉中已提供）：
- ASSET：资产台账相关（建档/改档/删除）
- INBOUND/OUTBOUND/RETURN/TRANSFER/CHANGE/DISPOSE/INV_ADJUST：单据类型
- INVENTORY：盘点计划/盘点明细
- MAINTENANCE：维修/保养工单

> 约定：BizType 建议尽量使用“业务域的稳定编码”（例如单据的 DocType），这样统计更方便。

## 3) 操作（Operation）建议口径

常见值（下拉中已提供）：
- CREATE：新增
- UPDATE：修改
- DELETE：删除
- SCAN：盘点扫码/录入
- STATUS：状态变化（可选）
- LOCATION：地点变化（可选）
- OWNER：责任人/使用人变化（可选）

## 4) 如何查询

你可以通过以下方式组合筛选：
- 按资产：先选择资产，再看该资产全量时间线
- 按业务类型：例如只看 MAINTENANCE（维修/保养）相关
- 按操作：例如只看 UPDATE（修改）
- 关键词：辅助查 BizType/Operation（也可以在备注中增加关键字便于检索）

## 5) 如何看 BeforeJson / AfterJson

建议理解方式：

- BeforeJson：这次操作发生前，该业务对象（或该资产的一部分关键字段）是什么状态
- AfterJson：这次操作发生后，变成了什么状态

常见用法：
- 对比前后地点、责任人、状态、金额等字段
- 追踪某次变更为何发生（通常在 Remark 中写明原因或关联单据号/工单号）

## 6) 常见问题

### Q1：为什么我操作了其它模块，这里没有数据？

留痕是“记录表”，需要业务流程在关键动作时写入记录。若系统尚未接入留痕写入逻辑，或历史数据未回填，则会出现“没有数据”的情况。

### Q2：为什么旧数据没有留痕？

留痕通常只从“上线接入后”开始记录。之前发生过的操作不会自动补齐，除非额外执行“回填任务/脚本”。

