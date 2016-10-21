using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RazorEngineCms.Models
{
    public class TemplateModel
    {
        public object PageModel { get; set; }

        public Include Includes { get; set; }
    }
}