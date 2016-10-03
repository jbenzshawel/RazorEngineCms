using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Web.Caching;
using Newtonsoft.Json;

namespace RazorEngineCms.Models
{
    [NotMapped]
    public class PageCache : Page
    {
        public string QueryString { get; set; }

        public DateTime DateTime { get; set; }

        public PageCache()
        {

        }

        public PageCache(Page page, string queryString)
        {
            this.Id = page.Id;
            this.Model = page.Model;
            this.Name = page.Name;
            this.Variable = page.Variable;
            this.Template = page.Template;
            this.QueryString = queryString;
            this.CompiledModel = page.CompiledModel;
            this.CompiledTemplate = page.CompiledTemplate;
            this.DateTime = DateTime.Now;
        }
    }
}