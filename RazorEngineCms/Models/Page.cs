using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RazorEngineCms.Models
{
    public class Page
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string Variable { get; set; }

        public string Model { get; set; }

        [Required]
        public string Template { get; set; }

        public string CompiledModel { get; set; }

        public string CompiledTemplate { get; set; }

        public Page(PageRequest pageRequest)
        {
            this.Name = pageRequest.Name;
            this.Variable = pageRequest.Variable;
            this.Model = pageRequest.Model;
            this.Template = pageRequest.Template;
        }
    }
}