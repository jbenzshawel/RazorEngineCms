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

        private const string CACHE_KEY = "RazorEngineCms.App_Classes.CacheManager";
        
        private Cache Cache { get { return HttpContext.Current.Cache; } }

        public CacheManager()
        {
            if (Cache != null)
            {
                this.UpdateCacheList();
            }
        }

        public void UpdateCacheList()
        {
            if (Cache[CACHE_KEY] != null)
            {
                CacheManager cacheManager = JsonConvert.DeserializeObject<CacheManager>(Cache[CACHE_KEY].ToString());
                if (cacheManager == null)
                {
                    this.CacheList = new List<PageCache>();
                }
                else // cache exists 
                {
                    this.CacheList = cacheManager.CacheList;
                }
            }
            
        }

        public void AddPage(Page page)
        {
            var queryString = HttpContext.Current.Request.QueryString.ToString();
            var pageCache = new PageCache(page, queryString);
            this.UpdateCacheList();
            this.CacheList.Add(pageCache);
            Cache[CACHE_KEY] = JsonConvert.SerializeObject(this);
        }

        public void RemovePage(string name, string section)
        {
            var pageToRemove = this.CacheList.FirstOrDefault(cachedPage => cachedPage.Name.ToLower() == name.ToLower() && cachedPage.Variable.ToLower() == section.ToLower());
            this.UpdateCacheList();
            this.CacheList.Remove(pageToRemove);
            Cache[CACHE_KEY] = JsonConvert.SerializeObject(this);
        }

        public PageCache FindPage(string name, string section)
        {
            PageCache pageCache = this.CacheList.FirstOrDefault(p => string.Equals(p.Name, name, StringComparison.CurrentCultureIgnoreCase) &&
                                       string.Equals(p.Variable, section, StringComparison.CurrentCultureIgnoreCase));
            
            return pageCache;
        
    }

        public void ClearCache()
        {
            this.CacheList = new List<PageCache>();
            Cache[CACHE_KEY] = JsonConvert.SerializeObject(this);
        }
    }
}