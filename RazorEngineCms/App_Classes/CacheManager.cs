using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Caching;
using RazorEngineCms.Models;
using RazorEngineCms.ExtensionClasses;

namespace RazorEngineCms.App_Classes
{
    /// <summary>
    /// Contains an IList&gt;PageCache&lt; CacheList property that is parsed from a cached object.
    /// If cache empty CacheList returns an empty list.
    /// </summary>
    public class CacheManager
    {
        /// <summary>
        /// Contains an IList&gt;PageCache&lt; CacheList proper ,mhwq   ty that is parsed from a cached object.
        /// If cache empty CacheList returns an empty list.
        /// </summary>
        public IList<PageCacheModel> CacheList { get; set; }

        private const string CACHE_KEY = "RazorEngineCms.App_Classes.CacheManager.CacheList";

        public Cache Cache { get; set; }

        public bool AllowCache { get; set; }

        public CacheManager()
        {
            this.Cache = HttpContext.Current != null ? HttpContext.Current.Cache : null;
            this.AllowCache = ConfigurationManager.AppSettings["AllowPageCaching"] == "true";
            if (this.Cache != null)
                this.UpdateCacheList();
        }

        /// <summary>
        /// Updates CacheList property with latest content in cache 
        /// </summary>
        public void UpdateCacheList()
        {
            if (this.AllowCache)
            {
                IList<PageCacheModel> cacheList = Cache?[CACHE_KEY] as List<PageCacheModel>;
                if (cacheList != null)
                {
                    this.CacheList = cacheList;
                }
            }
            
            if (this.CacheList == null)
            {
                this.CacheList = new List<PageCacheModel>();
            }
        }

        /// <summary>
        /// Add item of type T to HttpContext.Current.Cache (if accessable)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="obj"></param>
        public void Add<T>(string key, T obj)
        {
            if (Cache != null)
            {
                Cache[key] = obj;
            }
        }

        /// <summary>
        /// Gets an item of type T from HttpContext.Current.Cache
        /// returns null if Cache empty or Cache[key] not found
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public T Get<T>(string key) where T: class
        {
            T obj = null;
            if (Cache?[key] != null)
            {
                obj = Cache[key] as T;
            }
            return obj;
        }

        /// <summary>
        /// Adds a page to CacheList and Cache[CACHE_KEY]
        /// </summary>
        /// <param name="page"></param>
        /// <param name="param"></param>
        /// <param name="param2"></param>
        public void AddPage(Page page, string param = null, string param2 = null)
        {
            if (Cache != null && this.AllowCache)
            {
                var queryString = HttpContext.Current.Request.QueryString.ToDictionary();
                var pageCache = new PageCacheModel(page, param, param2, queryString);
                this.UpdateCacheList();
                this.CacheList.Add(pageCache);
                Cache[CACHE_KEY] = this.CacheList;
            }

        }

        /// <summary>
        /// Removes a page from CacheList and Cache[CACHE_KEY]
        /// </summary>
        /// <param name="name"></param>
        /// <param name="section"></param>
        /// <param name="param"></param>
        /// <param name="param2"></param>
        public void RemovePage(string name, string section, string param = null, string param2 = null)
        {
            if (Cache != null && this.AllowCache)
            {
                var pageToRemove = this.FindPage(name, section, param, param2);
                this.UpdateCacheList();
                this.CacheList.Remove(pageToRemove);
                Cache[CACHE_KEY] = this.CacheList;
            }
        }

        public bool PageCacheExists(string name, string section, string param, string param2, IDictionary<string,string> queryString)
        {
            var boolRtn = false;
            var cachedPage = this.FindPage(name, section, param, param2, queryString);

            if (cachedPage != null && cachedPage.Id > 0)
            {
                boolRtn = true;
            }

            return boolRtn;
        }

        /// <summary>
        /// Searches CacheList and returns a PageCacheModel of the page if found 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="section"></param>
        /// <param name="param"></param>
        /// <param name="param2"></param>
        /// <param name="queryString"></param>
        /// <returns></returns>
        public PageCacheModel FindPage(string name, string section, string param = null, string param2 = null, IDictionary<string, string> queryString = null)
        {
            if (this.CacheList == null)
            {
                this.UpdateCacheList();
            }
            var pageCache = new PageCacheModel();

            if (this.CacheList.Count > 0)
            {
                pageCache = this.CacheList.FirstOrDefault(
                cachedPage => string.Equals(cachedPage.Name, name, StringComparison.InvariantCultureIgnoreCase) &&
                              string.Equals(cachedPage.Section, section, StringComparison.InvariantCultureIgnoreCase) &&
                              string.Equals(cachedPage.Param, param, StringComparison.InvariantCultureIgnoreCase) &&
                              string.Equals(cachedPage.Param2, param2, StringComparison.InvariantCultureIgnoreCase) && 
                              cachedPage.QueryStringParams.Equal(queryString));

            }

            return pageCache;
        }

        /// <summary>
        /// Clears CacheList and Cache[CACHE_KEY]
        /// </summary>
        public void ClearCache()
        {
            this.CacheList = new List<PageCacheModel>();
            Cache[CACHE_KEY] = this.CacheList;
        }
    }
}