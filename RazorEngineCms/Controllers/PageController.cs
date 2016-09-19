using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Threading.Tasks;
using RazorEngine;
using RazorEngine.Templating;
using RazorEngineCms.DAL;
using RazorEngineCms.Models;
using RazorEngineCms.App_Classes;
using Newtonsoft.Json;

namespace RazorEngineCms.Controllers
{
    public class PageController : Controller
    {
        public List<string> Errors { get; set; }

        private ApplicationContext _db { get; set; }

        public PageController()
        {
            this._db = new ApplicationContext();
            this.Errors = new List<string>(); 
        }

        // GET: Page
        public ActionResult New()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> New(PageRequest pageRequest)
        {
            var page = new Page(pageRequest);
            if (ModelState.IsValid)
            {
                // compile the model from the string Model declaration in pageRequest
                using (var stringCompiler = new StringCompiler())
                {
                    stringCompiler.CompilePageModel(page.Model);
                    if (stringCompiler.IsValid)
                    {
                        page.CompiledModel = stringCompiler.ToString();
                    }
                    else
                    {
                        this.Errors.AddRange(stringCompiler.Errors);
                    }
                } // end using stringCompiler
                
                if (this.Errors.Count == 0)
                {
                    // compile and save template
                    page =  await this.CompileTemplateAndSavePage(page);
                } // end if valid model state 
                else
                {
                    this.Errors.Add("Invalid parameters");
                }
            } // end if no errors after compiling model
            
            return  Json(new { this.Errors, Status = this.Errors.Count == 0 }); 
        }

        public ActionResult Page(string name, string variable)
        {
            var page = this._db.Page
                            .FirstOrDefault(p => string.Equals(p.Name, name, StringComparison.CurrentCultureIgnoreCase) &&
                                                 string.Equals(p.Variable, variable, StringComparison.CurrentCultureIgnoreCase));
            if (page != null)
            {
                var model = JsonConvert.DeserializeObject(page.CompiledTemplate);
                return View(model);
            }
            
            // To Do: redirect to 404 
            // return empty view if can't find page
            return View(new { Content = string.Empty });
        }


        internal async Task<Page> CompileTemplateAndSavePage(Page page)
        {
            try
            {
                var cacheName = string.Format("{0}-{1}", DateTime.Now.ToString(), Guid.NewGuid().ToString());
                // null for modelType parameter since templates are dynamic 
                page.CompiledTemplate = Engine.Razor.RunCompile(page.Template, cacheName, null, JsonConvert.DeserializeObject(page.CompiledModel));
                this._db.Page.Add(page);
                await this._db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                this.Errors.Add(ex.Message);
                // if failed to save model get why
                if (this._db.GetValidationErrors().Count() > 0)
                {
                    foreach (var error in this._db.GetValidationErrors())
                    {
                        foreach (var valErr in error.ValidationErrors)
                        {
                            this.Errors.Add(string.Format("Model Error: {0}, Model Property: {1}",
                                valErr.ErrorMessage, valErr.PropertyName));
                        }
                    } // end foreach this._db.GetValidationErrors()
                } // end if have validation errors
            } // end catch

            return page; 
        }
    }
}