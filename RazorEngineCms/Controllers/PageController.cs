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

        internal FileHelper FileHelper { get; set; }

        private ApplicationContext _db { get; set; }

        public PageController()
        {
            this._db = new ApplicationContext();
            this.Errors = new List<string>();
            this.FileHelper = new FileHelper(); 
        }

        // GET: Page/New
        public ActionResult New()
        {
            return View();
        }

        /// <summary>
        /// Compiles template model and template then saves results in Pages table
        /// or as a file in /Views/CompiledTemplates/
        /// </summary>
        /// <param name="pageRequest"></param>
        /// <returns>JsonResult with boolean status and list of errors</returns>
        // POST: Page/New
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
                } // end using StringCompiler
                
                if (this.Errors.Count == 0)
                {
                    // compile and save template
                    page =  await this.CompileTemplateAndSavePage(page, saveAsFile: pageRequest.CreateTemplateFile);
                }  // end if no errors after compiling model
            } // end if valid model state 
            else
            {
                this.Errors.Add("Invalid parameters");
            } 

            return  Json(new { Status = this.Errors.Count == 0, this.Errors }); 
        }

        public ActionResult Page(string name, string variable)
        {
            //var page = this._db.Page
            //                .FirstOrDefault(p => string.Equals(p.Name, name, StringComparison.CurrentCultureIgnoreCase) &&
            //                                     string.Equals(p.Variable, variable, StringComparison.CurrentCultureIgnoreCase));
            //if (page != null)
            //{
            //    return View(new { Content = page.CompiledTemplate } );
            //}

                
            PageTemplate template = template = new PageTemplate { Content = string.Empty };

            if (FileHelper.Files.Any(f => string.Equals(f.Name, name, StringComparison.CurrentCultureIgnoreCase))) // get template from files?
            {
                var file = FileHelper.Files.Where(f => string.Equals(f.Name, name, StringComparison.CurrentCultureIgnoreCase) &&
                                                                    (string.Equals(f.Variable, variable, StringComparison.CurrentCultureIgnoreCase) ||
                                                                        (f.Variable == "_" && variable == "")))
                                                                    .FirstOrDefault();
                if (file != null && System.IO.File.Exists(file.Path))
                {
                    var page = new Page();
                    page.CompiledTemplate = System.IO.File.ReadAllText(file.Path);
                    if (!string.IsNullOrEmpty(page.CompiledTemplate))
                    {
                         template = new PageTemplate { Content = page.CompiledTemplate };
                    }
                }
            }

            if (!string.IsNullOrEmpty(template.Content))
            {
                return View(template);
            }

            return View("~/Views/Page/NotFound.cshtml");
        }


        internal async Task<Page> CompileTemplateAndSavePage(Page page, bool saveAsFile = false)
        {
            try
            {
                var templateGuid = Guid.NewGuid().ToString();
                var cacheName = string.Format("{0}-{1}", page.Name, templateGuid);
                // null for modelType parameter since templates are dynamic 
                page.CompiledTemplate = Engine.Razor.RunCompile(page.Template, cacheName, null, JsonConvert.DeserializeObject(page.CompiledModel));
                if (saveAsFile)
                {
                    var fileName = string.Format("{0}-{1}-template-{2}.html", page.Name, string.IsNullOrEmpty(page.Variable) ? "_" : page.Variable, templateGuid);
                    var savePath = Server.MapPath("~") + @"\Views\CompiledTemplates\" + fileName;
                    if(!System.IO.File.Exists(savePath))
                    {
                        using (var sw = System.IO.File.CreateText(savePath))
                        {
                            await sw.WriteAsync(page.CompiledTemplate);
                        }
                    }
                }
                else // save in db 
                {
                    this._db.Page.Add(page);
                    await this._db.SaveChangesAsync();
                }
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