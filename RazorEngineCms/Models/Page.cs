﻿using System;
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

        internal static Page FindPage(string name, string variable)
        {
            var db = new ApplicationContext();
            var page = new Page { Name = name, Section = variable };
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
                                                        p.Section.Equals(variable, StringComparison.CurrentCultureIgnoreCase));
            }

            return page;
        }

        internal void CompileTemplate(ref List<string> errors, string template = null, string jsonModel = null)
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
    }
}