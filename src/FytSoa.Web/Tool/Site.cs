using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FytSoa.Application.Cms;
using FytSoa.Common;
using FytSoa.Common.Utils;
using FytSoa.Domain.Cms;
using FytSoa.Common.Cache;
using FytSoa.Common.Param;

namespace FytSoa.Web
{
    public class Site
    {
        private static CmsSiteDto _site;
        /// <summary>
        /// 当前用户对象
        /// </summary>
        public static CmsSiteDto CurrentSite
        {
            get
            {
               
                /*var webSite = MemoryService.Default.GetCache<CmsSiteDto>(KeyUtils.WEBCMSSITE);
                if (webSite!=null && webSite.Id!=0)
                {
                    _site = webSite;
                }
                else
                {
                    var siteService = AppUtils.GetService<CmsSiteService>();
                    _site = siteService.GetAsync(1364837482360868864).Result;
                    MemoryService.Default.SetCache(key: KeyUtils.WEBCMSSITE, _site);
                }
                return _site;*/
                var siteService = AppUtils.GetService<CmsSiteService>();
                return siteService.GetAsync(1364837482360868864).Result;
            }
        }

        private static List<CmsColumnDto> _columns;
        /// <summary>
        /// 当前栏目
        /// </summary>
        public static List<CmsColumnDto> Columns
        {
            get
            {
                var columnService = AppUtils.GetService<CmsColumnService>();
                return columnService.GetListAsync(new WhereParam(){Status = "1"}).Result;
                /*var column = MemoryService.Default.GetCache<List<CmsColumnDto>>(KeyUtils.NOWSITECOLUMN);
                Console.WriteLine($"栏目数量：{column}");
                if (column!=null && column.Count>0)
                {
                    _columns = Columns;
                }
                else
                {
                    var columnService = AppUtils.GetService<CmsColumnService>();
                    _columns = columnService.GetListAsync(new WhereParam(){Status = "1"}).Result;
                    Console.WriteLine($"_columns栏目数量：{_columns}");
                    MemoryService.Default.SetCache(key: KeyUtils.NOWSITECOLUMN, _columns);
                }
                return _columns;*/
            }
        }

        public static string Href(long tempId, string pentitle, string entitle)
        {
            var str = string.Empty;
            if (tempId == 1365210485116506112)
            {
                str="/column/"+pentitle+"/"+entitle;
            }
            if (tempId == 1390623120041316352)
            {
                str="/dynamic/"+pentitle+"/"+entitle;
            }
            if (tempId == 1991082190145982464)
            {
                str="/message/"+pentitle+"/"+entitle;
            }
            if (tempId == 2011348881002074113)
            {
                str="/product/"+entitle;
            }
            return str;
        }
    }
}
