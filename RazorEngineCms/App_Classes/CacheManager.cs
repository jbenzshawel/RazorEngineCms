﻿using RazorEngineCms.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;
using RazorEngineCms.DAL;
using RazorEngineCms.ExtensionClasses;

namespace RazorEngineCms.App_Classes
{
    // ToDo: Refactor how cache works to rely less on parsing json models
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

        private Cache Cache { get; set; }

        public CacheManager()
        {
            this.Cache = HttpContext.Current.Cache;
            this.UpdateCacheList();
        }

        /// <summary>
        /// Updates CacheList property with latest content in cache 
        /// </summary>
        public void UpdateCacheList()
        {
            if (Cache[CACHE_KEY] != null)
            {
                IList<PageCacheModel> cacheList = Cache[CACHE_KEY] as List<PageCacheModel>;
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
        /// Adds a page to CacheList and Cache[CACHE_KEY]
        /// </summary>
        /// <param name="page"></param>
        /// <param name="param"></param>
        /// <param name="param2"></param>
        public void AddPage(Page page, string param = null, string param2 = null)
        {
            var queryString = HttpContext.Current.Request.QueryString.ToDictionary();
            var pageCache = new PageCacheModel(page, param, param2, queryString);
            this.UpdateCacheList();
            this.CacheList.Add(pageCache);
            Cache[CACHE_KEY] = this.CacheList;
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
            var pageToRemove = this.FindPage(name, section, param, param2);
            this.UpdateCacheList();
            this.CacheList.Remove(pageToRemove);
            Cache[CACHE_KEY] = this.CacheList;
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
                cachedPage => string.Equals(cachedPage.Name, name, StringComparison.CurrentCultureIgnoreCase) &&
                              string.Equals(cachedPage.Section, section, StringComparison.CurrentCultureIgnoreCase) &&
                              string.Equals(cachedPage.Param, param, StringComparison.CurrentCultureIgnoreCase) &&
                              string.Equals(cachedPage.Param2, param2, StringComparison.CurrentCultureIgnoreCase) && 
                              cachedPage.QueryStringParams.DictionaryEqual(queryString));

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