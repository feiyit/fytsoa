using System.Net;
using System.Text;

namespace FytSoa.Application.Utils;

internal static class ExcelExportUtils
{
    // Generate an .xls that Excel can open (HTML table with Excel MIME type).
    public static byte[] ToHtmlXls(string title, IReadOnlyList<string> headers, IEnumerable<IReadOnlyList<object?>> rows)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<html>");
        sb.AppendLine("<head>");
        sb.AppendLine("<meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\" />");
        sb.AppendLine($"<title>{Html(title)}</title>");
        sb.AppendLine("</head>");
        sb.AppendLine("<body>");
        sb.AppendLine("<table border=\"1\" cellspacing=\"0\" cellpadding=\"3\">");

        sb.AppendLine("<tr>");
        foreach (var h in headers)
        {
            sb.Append("<th>");
            sb.Append(Html(h));
            sb.AppendLine("</th>");
        }
        sb.AppendLine("</tr>");

        foreach (var r in rows)
        {
            sb.AppendLine("<tr>");
            foreach (var c in r)
            {
                // Force text format to avoid scientific notation for long ids.
                sb.Append("<td style=\"mso-number-format:'\\@';\">");
                sb.Append(Html(FormatCell(c)));
                sb.AppendLine("</td>");
            }
            sb.AppendLine("</tr>");
        }

        sb.AppendLine("</table>");
        sb.AppendLine("</body>");
        sb.AppendLine("</html>");

        var body = Encoding.UTF8.GetBytes(sb.ToString());
        var bom = Encoding.UTF8.GetPreamble();
        var bytes = new byte[bom.Length + body.Length];
        Buffer.BlockCopy(bom, 0, bytes, 0, bom.Length);
        Buffer.BlockCopy(body, 0, bytes, bom.Length, body.Length);
        return bytes;
    }

    private static string FormatCell(object? v)
    {
        if (v == null) return string.Empty;
        if (v is DateTime dt) return dt.ToString("yyyy-MM-dd HH:mm:ss");
        if (v is DateTimeOffset dto) return dto.ToString("yyyy-MM-dd HH:mm:ss");
        return v.ToString() ?? string.Empty;
    }

    private static string Html(string? s) => WebUtility.HtmlEncode(s ?? string.Empty);
}

