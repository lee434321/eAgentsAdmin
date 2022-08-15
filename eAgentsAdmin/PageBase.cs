using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Globalization;
using System.Threading;

using PropertyOneAppWeb.system;
namespace PropertyOneAppWeb
{
    public class PageBase: System.Web.UI.Page
    {
        public PageBase()
        { 
        
        }

        protected override void InitializeCulture()
        {
            string lang = Helper.Aid.DbNull2Str(Session["lang"]);
            string langConfig = Helper.Aid.DbNull2Str(System.Configuration.ConfigurationManager.AppSettings["defaultLang"]);
            langConfig = langConfig == "" ? "en-US" : langConfig;
            lang = lang == "" ? langConfig : lang;
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(lang);            
        }
       
        protected Pager<T> SetPager<T>(List<T> etys, int pidx, int pageSize = 10) where T : class
        {
            int pageIdx = pidx; //当前页               
            int q = etys.Count / pageSize;
            int r = etys.Count % pageSize;

            Pager<T> pager = new Pager<T>();
            pager.pageSize = pageSize;
            pager.count = r == 0 ? q : q + 1;
            if (pageIdx > pager.count) //如果超过总页数则就是最后1页。
                pageIdx = pager.count;
            if (pageIdx == 0)//如果为0则修改为第1页(翻页时会出现)。
                pageIdx = 1;
            pager.pageIdx = pageIdx;

            List<T> etysOut = new List<T>();
            int idx = 0;
            for (int i = 0; i < pageSize; i++)
            {
                idx = (pageIdx - 1) * pageSize + i;
                if (idx < etys.Count)
                    etysOut.Add(etys[idx]);
                else
                    break;
            }
            pager.data = etysOut;
            return pager;            
        }
    }
}