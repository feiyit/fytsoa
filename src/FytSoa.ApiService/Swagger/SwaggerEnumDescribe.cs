using System.Collections;
using System.Text;
using System.Xml.Linq;
using System.Xml.XPath;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace FytSoa.ApiService.Swagger;
/// <summary>
/// Swagger schema filter to modify description of enum types so they
/// show the XML docs attached to each member of the enum.
/// 参考地址 https://stackoverflow.com/questions/53282170/swaggerui-not-display-enum-summary-description-c-sharp-net-core
/// </summary>
public class DescribeEnumMembers: ISchemaFilter
{
    private readonly XDocument mXmlComments;

    /// <summary>
    /// Initialize schema filter.
    /// </summary>
    /// <param name="argXmlComments">Document containing XML docs for enum members.</param>
    public DescribeEnumMembers(XDocument argXmlComments)
        => mXmlComments = argXmlComments;

    /// <summary>
    /// Apply this schema filter.
    /// </summary>
    /// <param name="argSchema">Target schema object.</param>
    /// <param name="argContext">Schema filter context.</param>
    public void Apply(OpenApiSchema argSchema, SchemaFilterContext argContext) {
        var enumType = argContext.Type;

        if(!enumType.IsEnum) return;

        var sb = new StringBuilder(argSchema.Description);

        sb.AppendLine("<p>Possible values:</p>");
        sb.AppendLine("<ul>");

        foreach(var enumMemberName in Enum.GetNames(enumType)) {
            var fullEnumMemberName = $"F:{enumType.FullName}.{enumMemberName}";

            var enumMemberDescription = mXmlComments.XPathEvaluate(
                $"normalize-space(//member[@name = '{fullEnumMemberName}']/summary/text())"
            ) as string;

            if(string.IsNullOrEmpty(enumMemberDescription)) continue;

            sb.AppendLine($"<li><b>{enumMemberName}</b>: {enumMemberDescription}</li>");
        }

        sb.AppendLine("</ul>");

        argSchema.Description = sb.ToString();
    }
}