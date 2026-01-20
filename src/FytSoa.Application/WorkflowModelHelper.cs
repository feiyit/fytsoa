using System.Text.Json;

namespace FytSoa.Application.Wf;

/// <summary>
/// 工作流模型解析辅助类
/// - 负责从流程设计 JSON（wf_definition_model.ModelJson）中解析审批/抄送节点
/// - 根据表单数据计算本次实例应当走的审批节点顺序
/// - 提供统一的“首个审批人”“抄送节点”等查询能力
/// </summary>
public static class WorkflowModelHelper
{
    #region 模型 DTO

    /// <summary>
    /// 路由节点：从流程模型 JSON 中抽取出来的“审批节点”简化结构
    /// </summary>
    public sealed class RouteNode
    {
        /// <summary>
        /// 节点唯一标识（优先使用模型里的 id/nodeId，找不到时退化为 nodeName）
        /// </summary>
        public string NodeId { get; set; } = string.Empty;

        /// <summary>
        /// 节点名称（nodeName，仅用于展示或兜底匹配）
        /// </summary>
        public string NodeName { get; set; } = string.Empty;

        /// <summary>
        /// 节点原始 JSON 文本（GetRawText），避免 JsonDocument 释放导致的引用失效
        /// </summary>
        public string NodeJson { get; set; } = string.Empty;
    }

    /// <summary>
    /// 从“首个审批节点”解析出来的用户信息（用于实例启动时创建第一批任务）
    /// </summary>
    public sealed class WorkflowUserJson
    {
        /// <summary>
        /// 用户 Id（设计器 nodeUserList 中的 id）
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// 用户名称
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 所属审批节点 Id（来自流程模型 JSON，可为空）
        /// </summary>
        public string NodeId { get; set; } = string.Empty;

        /// <summary>
        /// 所属审批节点名称（来自流程模型 JSON，可为空）
        /// </summary>
        public string NodeName { get; set; } = string.Empty;
    }

    #endregion

    #region 节点级配置：超时自动审批

    /// <summary>
    /// 超时审批配置：从单个审批节点 JSON 中解析出来
    /// </summary>
    public sealed class TimeoutConfig
    {
        /// <summary>
        /// 是否开启超时自动审批（termAuto）
        /// </summary>
        public bool TermAuto { get; set; }

        /// <summary>
        /// 超时小时数（term），为 0 则不生效
        /// </summary>
        public int TermHours { get; set; }

        /// <summary>
        /// 超时后执行模式（termMode）：0=自动通过，1=自动拒绝
        /// </summary>
        public int TermMode { get; set; }
    }

    /// <summary>
    /// 根据流程模型 JSON + 任务节点信息（NodeId/NodeName），解析该节点的超时审批配置
    /// </summary>
    public static TimeoutConfig? FindTimeoutConfig(string modelJson, string taskNodeId, string taskNodeName)
    {
        if (string.IsNullOrWhiteSpace(modelJson))
            return null;

        using var doc = JsonDocument.Parse(modelJson);
        var root = doc.RootElement;

        TimeoutConfig? Traverse(JsonElement node)
        {
            if (!node.TryGetProperty("type", out var typeProp))
                return null;
            var type = typeProp.GetInt32();

            // 仅在 type=1（审批节点）上解析超时配置
            if (type == 1)
            {
                // 节点 Id：优先使用 id/nodeId
                string nodeId = string.Empty;
                if (node.TryGetProperty("id", out var idProp))
                {
                    nodeId = idProp.ToString();
                }
                else if (node.TryGetProperty("nodeId", out var nodeIdProp))
                {
                    nodeId = nodeIdProp.ToString();
                }

                var nodeName = node.TryGetProperty("nodeName", out var nameProp)
                    ? nameProp.GetString() ?? string.Empty
                    : string.Empty;

                var matchById = !string.IsNullOrWhiteSpace(taskNodeId) &&
                                !string.IsNullOrWhiteSpace(nodeId) &&
                                string.Equals(nodeId, taskNodeId, StringComparison.Ordinal);

                var matchByName = !matchById &&
                                  !string.IsNullOrWhiteSpace(taskNodeName) &&
                                  !string.IsNullOrWhiteSpace(nodeName) &&
                                  string.Equals(nodeName, taskNodeName, StringComparison.Ordinal);

                if (matchById || matchByName)
                {
                    var cfg = new TimeoutConfig();
                    if (node.TryGetProperty("termAuto", out var termAutoProp) &&
                        termAutoProp.ValueKind == JsonValueKind.True)
                    {
                        cfg.TermAuto = true;
                    }
                    else
                    {
                        cfg.TermAuto = node.TryGetProperty("termAuto", out var termAutoAny) &&
                                       termAutoAny.GetBoolean();
                    }

                    cfg.TermHours = node.TryGetProperty("term", out var termProp)
                        ? termProp.GetInt32()
                        : 0;
                    cfg.TermMode = node.TryGetProperty("termMode", out var termModeProp)
                        ? termModeProp.GetInt32()
                        : 0;

                    return cfg;
                }
            }

            // 递归 childNode
            if (node.TryGetProperty("childNode", out var child) &&
                child.ValueKind == JsonValueKind.Object)
            {
                var found = Traverse(child);
                if (found != null) return found;
            }

            // 递归条件分支
            if (node.TryGetProperty("conditionNodes", out var conds) &&
                conds.ValueKind == JsonValueKind.Array)
            {
                foreach (var cond in conds.EnumerateArray())
                {
                    var found = Traverse(cond);
                    if (found != null) return found;
                }
            }

            return null;
        }

        return Traverse(root);
    }

    /// <summary>
    /// 根据流程模型 JSON + 任务节点信息，解析多人审批方式 examineMode
    /// 约定：1=顺序审批，2=会签（全部通过），3=或签（任一人通过即可）
    /// 如果配置缺失或解析失败，默认返回 2（会签），以保持与原有实现行为一致
    /// </summary>
    public static int FindExamineMode(string modelJson, string taskNodeId, string taskNodeName)
    {
        if (string.IsNullOrWhiteSpace(modelJson))
            return 2;

        using var doc = JsonDocument.Parse(modelJson);
        var root = doc.RootElement;

        int Traverse(JsonElement node)
        {
            if (!node.TryGetProperty("type", out var typeProp))
                return 0;
            var type = typeProp.GetInt32();

            if (type == 1)
            {
                // 节点 Id：优先使用 id/nodeId
                string nodeId = string.Empty;
                if (node.TryGetProperty("id", out var idProp))
                {
                    nodeId = idProp.ToString();
                }
                else if (node.TryGetProperty("nodeId", out var nodeIdProp))
                {
                    nodeId = nodeIdProp.ToString();
                }

                var nodeName = node.TryGetProperty("nodeName", out var nameProp)
                    ? nameProp.GetString() ?? string.Empty
                    : string.Empty;

                var matchById = !string.IsNullOrWhiteSpace(taskNodeId) &&
                                !string.IsNullOrWhiteSpace(nodeId) &&
                                string.Equals(nodeId, taskNodeId, StringComparison.Ordinal);

                var matchByName = !matchById &&
                                  !string.IsNullOrWhiteSpace(taskNodeName) &&
                                  !string.IsNullOrWhiteSpace(nodeName) &&
                                  string.Equals(nodeName, taskNodeName, StringComparison.Ordinal);

                if (matchById || matchByName)
                {
                    // examineMode 可能不存在，或是 number/string，统一做健壮解析
                    if (node.TryGetProperty("examineMode", out var modeProp))
                    {
                        try
                        {
                            int value;
                            if (modeProp.ValueKind == JsonValueKind.Number)
                            {
                                value = modeProp.GetInt32();
                            }
                            else
                            {
                                var text = modeProp.GetString() ?? string.Empty;
                                value = int.TryParse(text, out var parsed) ? parsed : 0;
                            }

                            if (value <= 0) value = 2;
                            return value;
                        }
                        catch
                        {
                            // ignore parse error, fallback to default
                            return 2;
                        }
                    }

                    // 未配置 examineMode 时默认按会签处理
                    return 2;
                }
            }

            // 递归 childNode
            if (node.TryGetProperty("childNode", out var child) &&
                child.ValueKind == JsonValueKind.Object)
            {
                var foundMode = Traverse(child);
                if (foundMode != 0) return foundMode;
            }

            // 递归条件分支
            if (node.TryGetProperty("conditionNodes", out var conds) &&
                conds.ValueKind == JsonValueKind.Array)
            {
                foreach (var cond in conds.EnumerateArray())
                {
                    var foundMode = Traverse(cond);
                    if (foundMode != 0) return foundMode;
                }
            }

            return 0;
        }

        var result = Traverse(root);
        return result == 0 ? 2 : result;
    }

    #endregion

    #region 通用条件判断

    /// <summary>
    /// 根据字段路径从表单数据中读取值（仅支持一层或 a.b 形式）
    /// </summary>
    private static object? GetFieldValue(IDictionary<string, object?> formData, string field)
    {
        if (string.IsNullOrWhiteSpace(field)) return null;
        var parts = field.Split('.', StringSplitOptions.RemoveEmptyEntries);
        object? current = formData;
        foreach (var part in parts)
        {
            if (current is IDictionary<string, object?> dict)
            {
                dict.TryGetValue(part, out current);
            }
            else
            {
                return null;
            }
        }
        return current;
    }

    private static bool Compare(object? left, string op, string right)
    {
        var lv = left?.ToString() ?? string.Empty;
        var rv = right ?? string.Empty;
        op = op?.ToLowerInvariant() ?? "=";

        if (double.TryParse(lv, out var ln) && double.TryParse(rv, out var rn))
        {
            return op switch
            {
                ">" => ln > rn,
                ">=" => ln >= rn,
                "<" => ln < rn,
                "<=" => ln <= rn,
                "!=" => ln != rn,
                "=" => ln == rn,
                _ => ln == rn,
            };
        }

        return op switch
        {
            ">" => string.Compare(lv, rv, StringComparison.Ordinal) > 0,
            ">=" => string.Compare(lv, rv, StringComparison.Ordinal) >= 0,
            "<" => string.Compare(lv, rv, StringComparison.Ordinal) < 0,
            "<=" => string.Compare(lv, rv, StringComparison.Ordinal) <= 0,
            "!=" => !string.Equals(lv, rv, StringComparison.Ordinal),
            "include" => lv.Contains(rv, StringComparison.OrdinalIgnoreCase),
            "notinclude" => !lv.Contains(rv, StringComparison.OrdinalIgnoreCase),
            "=" => string.Equals(lv, rv, StringComparison.Ordinal),
            _ => string.Equals(lv, rv, StringComparison.Ordinal),
        };
    }

    /// <summary>
    /// 判断条件节点是否命中（根据 conditionList &amp; conditionMode）
    /// </summary>
    private static bool MatchCondition(JsonElement conditionNode, IDictionary<string, object?> formData)
    {
        if (!conditionNode.TryGetProperty("conditionList", out var list) ||
            list.ValueKind != JsonValueKind.Array ||
            list.GetArrayLength() == 0)
        {
            return false;
        }

        var mode = conditionNode.TryGetProperty("conditionMode", out var modeProp)
            ? modeProp.GetInt32()
            : 1;

        bool result = mode == 1; // 1=且(AND),2=或(OR)

        foreach (var item in list.EnumerateArray())
        {
            var field = item.TryGetProperty("field", out var f) ? f.GetString() ?? string.Empty : string.Empty;
            var op = item.TryGetProperty("operator", out var o) ? o.GetString() ?? "=" : "=";
            var value = item.TryGetProperty("value", out var v) ? v.GetString() ?? string.Empty : string.Empty;
            var left = GetFieldValue(formData, field);
            var c = Compare(left, op, value);
            if (mode == 1)
            {
                result = result && c;
                if (!result) break;
            }
            else
            {
                result = result || c;
                if (result) break;
            }
        }

        return result;
    }

    #endregion

    #region 审批路由：构建审批节点顺序

    /// <summary>
    /// 根据流程模型 JSON + 表单数据，按当前条件计算本次实例的审批节点执行顺序
    /// 仅收集 type=1（审批节点）
    /// </summary>
    public static List<RouteNode> BuildApproverRoute(string modelJson, IDictionary<string, object?> formData)
    {
        var result = new List<RouteNode>();
        if (string.IsNullOrWhiteSpace(modelJson))
            return result;

        using var doc = JsonDocument.Parse(modelJson);
        var root = doc.RootElement;

        if (root.TryGetProperty("childNode", out var firstChild) &&
            firstChild.ValueKind == JsonValueKind.Object)
        {
            Traverse(firstChild, result, formData);
        }

        return result;

        // 递归解析 childNode / conditionNodes，构建串行审批路由
        static void Traverse(JsonElement node, List<RouteNode> list, IDictionary<string, object?> formData)
        {
            if (!node.TryGetProperty("type", out var typeProp))
                return;
            var type = typeProp.GetInt32();

            if (type == 1)
            {
                // 1 = 审批节点：抽取节点 Id / 名称，并放入路由序列
                var nodeName = node.TryGetProperty("nodeName", out var nameProp)
                    ? nameProp.GetString() ?? string.Empty
                    : string.Empty;

                // 优先读取设计器中的 id / nodeId 作为节点唯一标识
                string nodeId = string.Empty;
                if (node.TryGetProperty("id", out var idProp))
                {
                    nodeId = idProp.ToString();
                }
                else if (node.TryGetProperty("nodeId", out var nodeIdProp))
                {
                    nodeId = nodeIdProp.ToString();
                }

                // 如果模型中没有配置 id，则退化为使用 nodeName 作为标识
                if (string.IsNullOrWhiteSpace(nodeId))
                {
                    nodeId = string.IsNullOrWhiteSpace(nodeName) ? "NODE_AUTO" : nodeName;
                }

                if (!string.IsNullOrWhiteSpace(nodeName))
                {
                    list.Add(new RouteNode
                    {
                        NodeId = nodeId,
                        NodeName = nodeName,
                        NodeJson = node.GetRawText(),
                    });
                }
            }

            if (type == 4)
            {
                // 条件分支：选择一个条件分支，然后继续后续 childNode
                if (node.TryGetProperty("conditionNodes", out var conds) &&
                    conds.ValueKind == JsonValueKind.Array &&
                    conds.GetArrayLength() > 0)
                {
                    JsonElement? selected = null;

                    foreach (var cond in conds.EnumerateArray())
                    {
                        if (MatchCondition(cond, formData))
                        {
                            selected = cond;
                            break;
                        }
                    }

                    // 如果没有任何条件命中，则默认走最后一个分支（否则流程无法继续）
                    if (!selected.HasValue)
                    {
                        selected = conds[conds.GetArrayLength() - 1];
                    }

                    if (selected.Value.TryGetProperty("childNode", out var condChild) &&
                        condChild.ValueKind == JsonValueKind.Object)
                    {
                        Traverse(condChild, list, formData);
                    }
                }

                if (node.TryGetProperty("childNode", out var after) &&
                    after.ValueKind == JsonValueKind.Object)
                {
                    Traverse(after, list, formData);
                }

                return;
            }

            if (node.TryGetProperty("childNode", out var child) &&
                child.ValueKind == JsonValueKind.Object)
            {
                Traverse(child, list, formData);
            }
        }
    }

    /// <summary>
    /// 从流程模型 JSON 中查找第一个抄送节点（type=2），返回该节点的原始 JSON 字符串
    /// </summary>
    public static string? FindFirstCcNodeJson(string modelJson)
    {
        if (string.IsNullOrWhiteSpace(modelJson))
            return null;

        using var doc = JsonDocument.Parse(modelJson);
        var root = doc.RootElement;

        string? Traverse(JsonElement node)
        {
            if (node.TryGetProperty("type", out var typeProp) && typeProp.GetInt32() == 2)
            {
                return node.GetRawText();
            }

            if (node.TryGetProperty("childNode", out var child) &&
                child.ValueKind == JsonValueKind.Object)
            {
                var found = Traverse(child);
                if (!string.IsNullOrWhiteSpace(found)) return found;
            }

            if (node.TryGetProperty("conditionNodes", out var conds) &&
                conds.ValueKind == JsonValueKind.Array)
            {
                foreach (var cond in conds.EnumerateArray())
                {
                    var found = Traverse(cond);
                    if (!string.IsNullOrWhiteSpace(found)) return found;
                }
            }

            return null;
        }

        return Traverse(root);
    }

    #endregion

    #region 首个审批人：实例启动时使用

    /// <summary>
    /// 从流程模型 JSON 中找到第一个审批节点（type=1）的原始 JSON 字符串。
    /// 用于流程实例启动时，配合不同 setType 解析首个审批节点的实际审批人。
    /// </summary>
    public static string? FindFirstApproverNodeJson(string? modelJson)
    {
        if (string.IsNullOrWhiteSpace(modelJson))
            return null;

        using var doc = JsonDocument.Parse(modelJson);
        var root = doc.RootElement;

        string? Traverse(JsonElement node)
        {
            if (node.TryGetProperty("type", out var typeProp) && typeProp.GetInt32() == 1)
            {
                return node.GetRawText();
            }

            if (node.TryGetProperty("childNode", out var child) && child.ValueKind == JsonValueKind.Object)
            {
                var found = Traverse(child);
                if (!string.IsNullOrWhiteSpace(found)) return found;
            }

            if (node.TryGetProperty("conditionNodes", out var conds) && conds.ValueKind == JsonValueKind.Array)
            {
                foreach (var cond in conds.EnumerateArray())
                {
                    var found = Traverse(cond);
                    if (!string.IsNullOrWhiteSpace(found)) return found;
                }
            }

            return null;
        }

        return Traverse(root);
    }

    /// <summary>
    /// 从流程模型 JSON 中找到第一个审批节点（type=1）的用户列表
    /// 用于流程实例启动时，创建首批审批任务
    /// </summary>
    public static List<WorkflowUserJson> FindFirstApproverUsers(string? modelJson)
    {
        var result = new List<WorkflowUserJson>();
        if (string.IsNullOrWhiteSpace(modelJson))
            return result;

        using var doc = JsonDocument.Parse(modelJson);
        var root = doc.RootElement;

        JsonElement? Traverse(JsonElement node)
        {
            if (node.TryGetProperty("type", out var typeProp) && typeProp.GetInt32() == 1)
            {
                return node;
            }

            if (node.TryGetProperty("childNode", out var child) && child.ValueKind == JsonValueKind.Object)
            {
                var found = Traverse(child);
                if (found.HasValue) return found;
            }

            if (node.TryGetProperty("conditionNodes", out var conds) && conds.ValueKind == JsonValueKind.Array)
            {
                foreach (var cond in conds.EnumerateArray())
                {
                    var found = Traverse(cond);
                    if (found.HasValue) return found;
                }
            }

            return null;
        }

        var approverNode = Traverse(root);
        if (!approverNode.HasValue) return result;

        // 节点名称
        var nodeName = approverNode.Value.TryGetProperty("nodeName", out var nodeNameProp)
            ? nodeNameProp.GetString() ?? string.Empty
            : string.Empty;

        // 节点 Id：优先读取 id / nodeId，没有则退化为 nodeName
        string nodeId = string.Empty;
        if (approverNode.Value.TryGetProperty("id", out var idProp2))
        {
            nodeId = idProp2.ToString();
        }
        else if (approverNode.Value.TryGetProperty("nodeId", out var nodeIdProp))
        {
            nodeId = nodeIdProp.ToString();
        }
        if (string.IsNullOrWhiteSpace(nodeId))
        {
            nodeId = string.IsNullOrWhiteSpace(nodeName) ? "NODE_START" : nodeName;
        }
        if (string.IsNullOrWhiteSpace(nodeName))
        {
            nodeName = "首个审批节点";
        }

        if (approverNode.Value.TryGetProperty("nodeUserList", out var arr) &&
            arr.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in arr.EnumerateArray())
            {
                var id = item.TryGetProperty("id", out var idProp) ? idProp.ToString() : string.Empty;
                var name = item.TryGetProperty("name", out var nameProp) ? nameProp.ToString() : string.Empty;
                if (!string.IsNullOrWhiteSpace(id))
                {
                    result.Add(new WorkflowUserJson
                    {
                        Id = id,
                        Name = name,
                        NodeId = nodeId,
                        NodeName = nodeName,
                    });
                }
            }
        }

        return result;
    }

    #endregion
}
