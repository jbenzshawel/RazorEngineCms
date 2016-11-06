using System.Collections.Concurrent;
using System.Configuration;
using System.Web.Mvc;
using RazorEngineCms.DAL.RepositoryService;

namespace RazorEngineCms.App_Classes
{
    public class BaseController : Controller
    {
        public ConcurrentBag<string> Errors { get; set; }
        
        internal bool AllowCache { get; set; }

        internal FileHelper FileHelper { get; set; }

        internal CacheManager CacheManager { get; set; }

        internal IRepositoryService  _repository { get; set; }

        public BaseController(IRepositoryService repository)
        {
            this._repository = repository;
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