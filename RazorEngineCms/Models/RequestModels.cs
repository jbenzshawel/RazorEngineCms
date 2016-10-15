using System.ComponentModel.DataAnnotations;

namespace RazorEngineCms.Models
{
    public class PageRequest
    {
        [Required]
        public string Section { get; set; }

        public string Name { get; set; }

        public string Model { get; set; }

        public bool HasParams { get; set; }

        public bool CreateTemplateFile { get; set; }

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