using RazorEngineCms.DAL;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace RazorEngineCms.Models
{
    public class PageRequest
    {
        [Required]
        public string Section { get; set; }

        public string Name { get; set; }

        public string Model { get; set; }

        public bool HasParams { get; set; }

        public bool HasInclude { get; set; }

        public bool CreateTemplateFile { get; set; }

        public DateTime? Updated { get; set; }

        [Required]
        public string Template { get; set; }
    }

    public class AjaxPageRequest
    {
        public int Id { get; set; }

        public string Section { get; set; }

        public string Name { get; set; }
         
    }
}