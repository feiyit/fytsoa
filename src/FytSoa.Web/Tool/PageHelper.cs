using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace FytSoa.Web
{
    public class PageHelper
    {
        public static string Htmls(int page, int pagesize, int total, string url)
        {
            url += "?page=";
            // 基础参数校验
            if (pagesize <= 0) pagesize = 10;
            if (page < 1) page = 1;
            if (total < 0) total = 0;

            // 计算总页数
            int totalPages = total <= 0 ? 1 : (int)Math.Ceiling((double)total / pagesize);
            // 确保当前页不超过总页数
            if (page > totalPages) page = totalPages;

            StringBuilder sb = new StringBuilder();

            // 1. 上一页按钮
            string prevDisabled = page <= 1 ? "disabled" : "";
            string prevHref = page > 1 ? $"{url}{page - 1}" : "#";
            sb.AppendLine($"<li class=\"{prevDisabled}\">");
            sb.AppendLine($"    <a class=\"product-page\" href=\"{prevHref}\" tabindex=\"-1\">上一页</a>");
            sb.AppendLine("</li>");

            // 2. 页码按钮（显示1-4页示例，可扩展为动态显示合理页码范围）
            for (int i = 1; i <= totalPages; i++)
            {
                // 控制最多显示4个页码（匹配示例结构，可根据需求调整）
                if (i > 4) break;
            
                string active = i == page ? "active" : "";
                string pageHref = i == page ? "#" : $"{url}{i}";
                sb.AppendLine($"<li class=\"product-page {active}\"><a class=\"page-link\" href=\"{pageHref}\">{i}</a></li>");
            }

            // 3. 下一页按钮
            string nextDisabled = page >= totalPages ? "disabled" : "";
            string nextHref = page < totalPages ? $"{url}{page + 1}" : "#";
            sb.AppendLine($"<li class=\"{nextDisabled}\">");
            sb.AppendLine($"    <a class=\"product-page\" href=\"{nextHref}\">下一页</a>");
            sb.AppendLine("</li>");

            return sb.ToString();
        }


    }
}