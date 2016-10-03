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
    /// <summary>
    /// Contains an IList&gt;PageCache&lt; CacheList property that is parsed from a cached object.
    /// If cache empty CacheList returns an empty list.
    /// </summary>
    public class CacheManager
    {
        /// <summary>
        /// Contains an IList&gt;PageCache&lt; CacheList property that is parsed from a cached object.
        /// If cache empty CacheList returns an empty list.
        /// </summary>
        public IList<PageCache> CacheList { get; set; }

        private const string CACHE_KEY = "RazorEngineCms.App_Classes.CacheManager.CacheList";

        private Cache Cache { get { return HttpContext.Current.Cache; } }

        public CacheManager()
        {
            if (Cache != null)
            {
                this.UpdateCacheList();
            }
            
            if (this.CacheList == null)
            {
                this.CacheList = new List<PageCache>();
            }
        }

        /// <summary>
        /// Updates CacheList property with latest content in cache 
        /// </summary>
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
            var pageToRemove = this.FindPage(name, section, param, param2);
            this.UpdateCacheList();
            this.CacheList.Remove(pageToRemove);
            Cache[CACHE_KEY] = JsonConvert.SerializeObject(this);
        }

        public PageCache FindPage(string name, string section, string param = null, string param2 = null)
        {
            PageCache pageCache = null;

            if (this.CacheList.Count > 0)
            {
                pageCache = this.CacheList.FirstOrDefault(
                cachedPage => string.Equals(cachedPage.Name, name, StringComparison.CurrentCultureIgnoreCase) &&
                              string.Equals(cachedPage.Section, section, StringComparison.CurrentCultureIgnoreCase) &&
                              string.Equals(cachedPage.Param, param, StringComparison.CurrentCultureIgnoreCase) &&
                              string.Equals(cachedPage.Param2, param2, StringComparison.CurrentCultureIgnoreCase));

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