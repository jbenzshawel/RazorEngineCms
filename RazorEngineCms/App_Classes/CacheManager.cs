using RazorEngineCms.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;
using Newtonsoft.Json;
using RazorEngineCms.DAL;

namespace RazorEngineCms.App_Classes
{
    [Serializable]
    public class CacheManager
    {
        public IList<PageCache> CacheList { get; set; }

        private const string CACHE_KEY = "RazorEngineCms.App_Classes.CacheManager.CacheList";

        private Cache Cache { get { return HttpContext.Current.Cache; } }

        public CacheManager()
        {
            if (Cache != null)
            {
                this.UpdateCacheList();
            }
            else
            {
                this.CacheList = new List<PageCache>();
            }
        }

        public void UpdateCacheList()
        {
            if (Cache[CACHE_KEY] != null)
            {
                IList<PageCache> cacheList = JsonConvert.DeserializeObject<List<PageCache>>(Cache[CACHE_KEY].ToString());
                if (cacheList == null)
                {
                    this.CacheList = new List<PageCache>();
                }
                else // cache exists 
                {
                    this.CacheList = cacheList;
                }
            }
            else
            {
                if (this.CacheList == null)
                {
                    this.CacheList = new List<PageCache>();
                }
            }

        }

        public void AddPage(Page page, string param = null, string param2 = null)
        {
            var queryString = HttpContext.Current.Request.QueryString.ToString();
            var pageCache = new PageCache(page, param, param2, queryString);
            this.UpdateCacheList();
            this.CacheList.Add(pageCache);
            Cache[CACHE_KEY] = JsonConvert.SerializeObject(this.CacheList);
        }

        public void RemovePage(string name, string section, string param = null, string param2 = null)
        {
            var pageToRemove = this.CacheList.FirstOrDefault(
                cachedPage => string.Equals(cachedPage.Name, name, StringComparison.CurrentCultureIgnoreCase) && 
                              string.Equals(cachedPage.Variable, section, StringComparison.CurrentCultureIgnoreCase) &&
                              string.Equals(cachedPage.Param, param, StringComparison.CurrentCultureIgnoreCase) &&
                              string.Equals(cachedPage.Param2, param2, StringComparison.CurrentCultureIgnoreCase));
            this.UpdateCacheList();
            this.CacheList.Remove(pageToRemove);
            Cache[CACHE_KEY] = JsonConvert.SerializeObject(this);
        }

        public PageCache FindPage(string name, string section)
        {
            PageCache pageCache = null;

            if (this.CacheList.Count > 0)
            {
                pageCache = this.CacheList.FirstOrDefault(p => string.Equals(p.Name, name, StringComparison.CurrentCultureIgnoreCase) &&
                                           string.Equals(p.Variable, section, StringComparison.CurrentCultureIgnoreCase));

            }

            return pageCache;
        }

        public void ClearCache()
        {
            this.CacheList = new List<PageCache>();
            Cache[CACHE_KEY] = JsonConvert.SerializeObject(this.CacheList);
        }
    }
}