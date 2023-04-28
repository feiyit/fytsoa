namespace FytSoa.Exam;

public static class PageUtils
{
    public static string Htmls(int page, int pagesize, int total, string url)
        {
            var str = "";
            int maxi = total;
            if (maxi < 10)
            {
                for (int i = 1; i < maxi + 1; i++)
                {
                    if (i == page)
                    {
                        str+= "<li class=\"uk-active\"><span>" + i + "</span></li>";
                    }
                    else
                    {
                        str += "<li><a href=\"" + url + "?page=" + i + "\">" + i + "</a></li>";
                    }
                }
            }
            else
            {
                int maxfeye = page + 5;
                int minfeye = page - 4;
                if (page < 6)
                {
                    minfeye = 1;
                    maxfeye = 11;
                }
                if (maxfeye > maxi)
                {
                    maxfeye = maxi;
                }
                for (int f = minfeye; f < maxfeye + 1; f++)//每页显示9个分页数字
                {
                    if (f == page)
                    {
                        str+= "<li class=\"uk-active\"><span>" + f + "</span></li>";
                    }
                    else
                    {
                        str += "<li><a href=\"" + url + "?page=" + f + "\">" + f + "</a></li>";

                    }
                }

                if (page < total - 3)
                {
                    str += "<li class=\"uk-disabled\"><span>...</span></li>";
                    str += "<li class=\"uk-disabled\"><a href=\"" + url + "?page=" + page+1 + "\"><span uk-pagination-next></span></a></li>";
                }

            }
           
            /*var str = "<a href=\"" + url + "\">首页</a>";
            if (page > 1)
            {
                str += "<a href=\"" + url + "?page=" + (page - 1) + "\">上一页</a>";
            }
            else
            {
                str += "<a href=\"" + url + "\">上一页</a>";
            }
            if (page == total)
            {
                str += "<a href=\"javascript:void(0)\" disabled=\"disabled\">下一页</a>";
            }
            else
            {
                str += "<a href=\"" + url + "?page=" + (page + 1) + "\">下一页</a>";
            }
            if (total > 1)
            {
                str += "<a href=\"" + url + "?page=" + total + "\">尾页</a>";
            }
            else
            {
                str += "<a href=\"" + url + "\">尾页</a>";
            }*/
            return str;
        }
}