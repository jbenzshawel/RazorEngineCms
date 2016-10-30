using RazorEngineCms.DAL;
using System.Collections.Concurrent;
using System.Configuration;
using System.Web.Mvc;

namespace RazorEngineCms.App_Classes
{
    public class BaseController : Controller
    {
        public ConcurrentBag<string> Errors { get; set; }
        
        internal bool AllowCache { get; set; }

        internal FileHelper FileHelper { get; set; }

        internal CacheManager CacheManager { get; set; }

        internal ApplicationContext _db { get; set; }

        public BaseController()
        {
            this._db = new ApplicationContext();
            this.Errors = new ConcurrentBag<string>();
            this.FileHelper = new FileHelper();
            this.AllowCache = ConfigurationManager.AppSettings["AllowPageCaching"] == "true";
            if (this.AllowCache)
            {
                this.CacheManager = new CacheManager();
            }
        }
    }
}