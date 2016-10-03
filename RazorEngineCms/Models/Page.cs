using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using RazorEngineCms.App_Classes;
using RazorEngineCms.DAL;

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

        public bool HasParams { get; set; }

        public Page()
        {

        }

        public Page(PageRequest pageRequest)
        {
            this.Name = pageRequest.Name;
            this.Variable = pageRequest.Variable;
            this.Model = pageRequest.Model;
            this.Template = pageRequest.Template;
        }

        internal static Page FindPage(string name, string variable)
        {
            var db = new ApplicationContext();
            var page = new Page { Name = name, Variable = variable };
            var fileHelper = new FileHelper();
            // first see if there is a file template 
            if (fileHelper.Files.Any(f => string.Equals(f.Name, name, StringComparison.CurrentCultureIgnoreCase)))
            {
                var file = fileHelper.GetFile(name, variable);
                page.CompiledTemplate = file.ToString();
            }
            else // get page from database if there isn't a file
            {
                page = db.Page
                                .FirstOrDefault(p => p.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase) &&
                                                        p.Variable.Equals(variable, StringComparison.CurrentCultureIgnoreCase));
            }

            return page;
        }
    }
}