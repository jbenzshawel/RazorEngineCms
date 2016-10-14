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
    public class PageCacheModel : Page
    {
        public string Param { get; set; }

        public string Param2 { get; set; }

        public IDictionary<string, string> QueryStringParams { get; set; }

        public DateTime DateTime { get; set; }

        public PageCacheModel()
        {

        }

        public PageCacheModel(Page page, string param, string param2, IDictionary<string, string> queryStringParams)
        {
            if (page != null)
            {
                this.Id = page.Id;
                this.Model = page.Model;
                this.Name = page.Name;
                this.Section = page.Section;
                this.Template = page.Template;
                this.CompiledModel = page.CompiledModel;
                this.CompiledTemplate = page.CompiledTemplate;
            }
           
            this.DateTime = DateTime.Now;
            this.Param = param;
            this.Param2 = param2;
            this.QueryStringParams = queryStringParams;
        }
    }
}