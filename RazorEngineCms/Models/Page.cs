using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using RazorEngine;
using RazorEngine.Templating;
using RazorEngineCms.App_Classes;
using RazorEngineCms.Controllers;
using RazorEngineCms.DAL;
using System.Collections.Concurrent;

namespace RazorEngineCms.Models
{
    public class Page
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string Section { get; set; }

        public string Model { get; set; }

        [Required]
        public string Template { get; set; }

        public string CompiledModel { get; set; }

        public string CompiledTemplate { get; set; }

        public bool HasParams { get; set; }

        public DateTime Updated { get; set; }

        public Page()
        {

        }

        public Page(PageRequest pageRequest)
        {
            this.Name = pageRequest.Name;
            this.Section = pageRequest.Section;
            this.Model = pageRequest.Model;
            this.Template = pageRequest.Template;
            this.HasParams = pageRequest.HasParams;
        }

        internal static Page FindPage(string section, string name)
        {
            var db = new ApplicationContext();
            var page = new Page { Name = name, Section = section };
            var fileHelper = new FileHelper();
            // first see if there is a file template 
            if (fileHelper.Files.Any(f => string.Equals(f.Name, name, StringComparison.CurrentCultureIgnoreCase)))
            {
                var file = fileHelper.GetFile(name, section);
                page.CompiledTemplate = file.ToString();
            }
            else // get page from database if there isn't a file
            {
                page = db.Page
                                .FirstOrDefault(p => p.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase) &&
                                                        p.Section.Equals(section, StringComparison.CurrentCultureIgnoreCase));
            }

            return page;
        }

        internal void CompileTemplate(ref ConcurrentBag<string> errors, string template = null, string jsonModel = null)
        {
            if (string.IsNullOrEmpty(template))
            {
                template = this.Template;
            }

            if (string.IsNullOrEmpty(jsonModel))
            {
                jsonModel = this.CompiledModel;
            }

            var templateGuid = Guid.NewGuid().ToString();
            var cacheName = string.Format("{0}-{1}", this.Name, templateGuid);

            try
            {
                // null for modelType parameter since templates are dynamic 
                this.CompiledTemplate = Engine.Razor.RunCompile(template, cacheName, null,
                    JsonConvert.DeserializeObject(jsonModel));
            }
            catch (Exception ex)
            {
                errors.Add(string.Format("Template Compile Error: {0}", ex.Message));
                if (ex.GetType() == typeof(TemplateParsingException))
                {
                    errors.Add(string.Format("Line: {0}", ((TemplateParsingException)ex).Line));
                }
                errors.Add(string.Format("Stack Trace: \r\n {0}", ex.StackTrace));
            }
        }

        /// <summary>
        /// Clones a page in the database. Returns new integer id of copied page  
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        internal static int? Copy(Page page)
        {
            var boolRtn = false;
            int? copiedPageId = null; 
            if (page != null)
            {
                using (var db = new ApplicationContext())
                {
                    var origPage = db.Page.FirstOrDefault(p => p.Id == page.Id);
                    if (origPage != null)
                    {
                        // clone the page 
                        origPage.Updated = DateTime.Now;
                        db.Page.Add(origPage);
                        boolRtn = db.SaveChanges() > 0;
                        copiedPageId = origPage.Id;
                    } // end if origPage != null
                } // end using db application context 
            } // end if page != null 

            return copiedPageId;
        }
    }
}