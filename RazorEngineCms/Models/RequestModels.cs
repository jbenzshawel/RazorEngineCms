using System.ComponentModel.DataAnnotations;

namespace RazorEngineCms.Models
{
    public class PageRequest
    {
        [Required]
        public string Name { get; set; }

        public string Variable { get; set; }

        public string Model { get; set; }

        public bool CreateTemplateFile { get; set; }

        [Required]
        public string Template { get; set; }
    }
}