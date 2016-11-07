using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Newtonsoft.Json;
using RazorEngine;
using RazorEngine.Templating;
using RazorEngineCms.App_Classes;
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

        public bool HasInclude { get; set; }

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
            this.HasInclude = pageRequest.HasInclude;
            this.Updated = (DateTime)pageRequest.Updated;
        }

        public void CompileTemplate(ref ConcurrentBag<string> errors, string template = null, string jsonModel = null)
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
                if (this.HasInclude)
                {
                    var templateModel = new TemplateModel
                    {
                        PageModel = jsonModel != null ? JsonConvert.DeserializeObject(jsonModel) : null,
                        Includes = new Include()
                    };

                    this.CompiledTemplate = Engine.Razor.RunCompile(template, cacheName, typeof(TemplateModel), templateModel);
                }
                else
                {
                    // null for modelType parameter since templates are dynamic 
                    this.CompiledTemplate = Engine.Razor.RunCompile(template, cacheName, null,
                        JsonConvert.DeserializeObject(jsonModel));
                }
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
    }
}