# 数据关系图（ER，帮助理解“关联字段”）

该图用于帮助业务人员理解：为什么很多地方要填“资产Id / 单据Id / 工单Id”等，以及这些 Id 之间如何关联。

```mermaid
erDiagram
  AM_ASSET ||--o{ AM_ASSET_HISTORY : "AssetId"
  AM_DOC ||--o{ AM_DOC_ITEM : "DocId"
  AM_ASSET ||--o{ AM_DOC_ITEM : "AssetId"
  AM_INVENTORY_PLAN ||--o{ AM_INVENTORY_ITEM : "PlanId"
  AM_ASSET ||--o{ AM_INVENTORY_ITEM : "AssetId"
  AM_ASSET ||--o{ AM_MAINTENANCE_ORDER : "AssetId"
  AM_ASSET ||--o{ AM_ASSET_DEPRECIATION : "AssetId"
  AM_DEPRECIATION_RUN ||--o{ AM_DEPRECIATION_RUN_ITEM : "RunId"
  AM_REMINDER_RULE ||--o{ AM_REMINDER_TASK : "RuleId"

  AM_ASSET {
    long Id PK
    string AssetNo
    string Name
    long CategoryId
    long VendorId
    long WarehouseId
    long BinId
    long LocationId
    long OrgUnitId
    long CustodianId
    long UserId
  }

  AM_DOC {
    long Id PK
    string DocNo
    string DocType
    string SubType
    byte Status
  }

  AM_DOC_ITEM {
    long Id PK
    long DocId FK
    long AssetId
    decimal Qty
    decimal Amount
  }

  AM_ASSET_HISTORY {
    long Id PK
    long AssetId FK
    string BizType
    long BizId
    string Operation
  }
```

阅读建议：
- 如果你在留痕里看到 BizType=MAINTENANCE、BizId=xxx，通常表示这条留痕关联到“某个工单Id=xxx”。
- 如果你在提醒任务里看到 BizType=DOC、BizId=xxx，通常表示这条提醒关联到“某张单据Id=xxx”。

