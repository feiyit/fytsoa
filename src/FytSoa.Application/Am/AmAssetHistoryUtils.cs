using System.Text.Json;
using System.Text.Json.Serialization;
using FytSoa.Common.Utils;
using FytSoa.Domain.Am;

namespace FytSoa.Application.Am;

/// <summary>
/// 资产留痕（溯源）写入辅助：统一构造 AmAssetHistory，避免各业务服务重复填充公共字段。
/// </summary>
internal static class AmAssetHistoryUtils
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public static string? ToJson(object? obj)
    {
        return obj == null ? null : JsonSerializer.Serialize(obj, JsonOptions);
    }

    public static AmAssetHistory Build(
        long tenantId,
        long assetId,
        string bizType,
        long bizId,
        string operation,
        object? before,
        object? after,
        string? remark = null,
        long? operatorId = null,
        DateTime? operateTime = null)
    {
        return new AmAssetHistory
        {
            Id = Unique.Id(),
            TenantId = tenantId,
            AssetId = assetId,
            BizType = bizType ?? string.Empty,
            BizId = bizId,
            Operation = operation ?? string.Empty,
            BeforeJson = ToJson(before),
            AfterJson = ToJson(after),
            Remark = remark,
            OperatorId = operatorId ?? AppUtils.LoginId,
            OperateTime = operateTime ?? DateTime.Now
        };
    }
}

