using RazorEngineCms.DAL;
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

        public bool CreateTemplateFile { get; set; }

        [Required]
        public string Template { get; set; }
    }

    public class AjaxPageRequest
    {
        public int Id { get; set; }

        public string Section { get; set; }

        public string Name { get; set; }

        internal Page GetPage()
        {
            Page pageModel = null;
            ApplicationContext _db = new ApplicationContext(); 
            if (this.Id > 0)  // if id is set get model by id 
            {
                pageModel = _db.Page.FirstOrDefault(p => p.Id == this.Id);
            }
            else if (!string.IsNullOrEmpty(this.Name) && !string.IsNullOrEmpty(this.Section)) // else try and get it by name and section 
            {
                pageModel = Page.FindPage(this.Name, this.Section);
            }

            return pageModel;
        }
    }
}