using System;
using System.Configuration;
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

        public bool AllowCache { get; set; }

        internal FileHelper FileHelper { get; set; }

        internal CacheManager CacheManager { get; set; }

        private ApplicationContext _db { get; set; }

        public PageController()
        {
            _db = new ApplicationContext();
            Errors = new List<string>();
            FileHelper = new FileHelper();
            AllowCache = ConfigurationManager.AppSettings["AllowPageCaching"] == "true";
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
        public async Task<ActionResult> Save(PageRequest pageRequest)
        {
            var page = new Page(pageRequest);
            if (ModelState.IsValid)
            {
                if (!string.IsNullOrEmpty(page.Model))
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
                            Errors.AddRange(stringCompiler.Errors);
                        }
                    } // end using StringCompiler
                } // end if page.Model not empty
               
                if (Errors.Count == 0)
                {
                    // compile and save template
                    page = await CompileTemplateAndSavePage(page, saveAsFile: pageRequest.CreateTemplateFile);
                }  // end if no errors after compiling model
            } // end if valid model state 
            else
            {
                Errors.Add("Invalid parameters");
            }



            return Json(new { Status = Errors.Count == 0, Errors });
        }

        /// <summary>
        /// Finds a page to edit and returns the page. Returns 404 if not found
        /// </summary>
        /// <param name="name"></param>
        /// <param name="section"></param>
        /// <returns></returns>
        public ActionResult Edit(string name, string section)
        {
            var page = Page.FindPage(name, section);

            if (page != null)
            {
                return View(page);
            }

            // return 404 view if could not find page in db or files 
            return View("~/Views/Page/NotFound.cshtml");
        }

        /// <summary>
        /// Postback for deleting a page 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="variable"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> Delete(string name, string section = null)
        {
            bool status = false;
            if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(section))
            {
                try
                {
                    var page = Page.FindPage(name, section);
                    this._db.Page.Attach(page);
                    this._db.Page.Remove(page);
                    status = await this._db.SaveChangesAsync() > 0;
                }
                catch (Exception ex)
                {
                    this.Errors.Add(ex.Message);
                    status = false;
                }
            }
            else
            {
                this.Errors.Add("Page not found");
            }

            return Json(new { Status = status, this.Errors });
        }

        /// <summary>
        /// Returns view with the template for the passed in name and variable. The
        /// PageTemplate model is passed to the view. 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="section"></param>
        /// <param name="param"></param>
        /// <param name="param2"></param>
        /// <returns></returns>
        // GET: Preview/Name/Section 
        public ActionResult View(string name, string section, string param = null, string param2 = null)
        {
            Page page = null;
            var template = new PageTemplate { Content = string.Empty };

            if (AllowCache) // only look for page in cache if caching enabled
            {
                CacheManager = new CacheManager();
                PageCache cachedPage = CacheManager.FindPage(name, section);
                if (cachedPage != null && cachedPage.CompiledTemplate != null)
                {
                    page = new Page
                    {
                        Name = cachedPage.Name,
                        Section = cachedPage.Section,
                        CompiledModel = cachedPage.CompiledModel,
                        CompiledTemplate = cachedPage.CompiledTemplate,
                        Model = cachedPage.Model,
                        Template = cachedPage.Template,
                        HasParams = cachedPage.HasParams
                    };
                }
            }

            // if we couldn't get the page from cache get it from the db
            if (page == null || page.CompiledTemplate == null)
            {
                page = Page.FindPage(name, section);
                if (AllowCache)
                {
                    CacheManager.AddPage(page, param, param2);
                }
            }

            if (page != null && page.CompiledTemplate != null)
            {
                template.Content = page.CompiledTemplate;
                // return the page with a template if it is found
                return View(template);
            }

            // return 404 view if could not find page in db or files 
            return View("~/Views/Page/NotFound.cshtml");
        }

        /// <summary>
        /// Gets a list of pages from the database and returns a PageList to the View
        /// </summary>
        /// <returns></returns>
        public ActionResult List()
        {
            IList<Page> pageList = new PageList(); 
            foreach (var file in FileHelper.Files)
            {
                pageList.Add(new Page
                {
                    Id = -1,
                    Name = file.Name,
                    Section = file.Variable,
                    CompiledTemplate = FileHelper.GetFile(file.Name, file.Variable).ToString()
                });
            }
            foreach (var page in _db.Page)
            {
                pageList.Add(page);
            }
            return View(pageList);
        }

        /// <summary>
        /// Compiles a page template by parsing a json object in page.CompiledModel and
        /// uses RazorEngine to compile the Template view. Save results in the database 
        /// and optional as a file as well
        /// </summary>
        /// <param name="page"></param>
        /// <param name="saveAsFile"></param>
        /// <returns></returns>
        internal async Task<Page> CompileTemplateAndSavePage(Page page, bool saveAsFile = false)
        {
            var templateGuid = Guid.NewGuid().ToString();
            var cacheName = string.Format("{0}-{1}", page.Name, templateGuid);

            try
            {
                if (!string.IsNullOrEmpty(page.Model))
                {
                    // null for modelType parameter since templates are dynamic 
                    page.CompiledTemplate = Engine.Razor.RunCompile(page.Template, cacheName, null, JsonConvert.DeserializeObject(page.CompiledModel));
                }
                else // no template model so do not need to compile
                {
                    page.CompiledTemplate = page.Template;
                }
                
                if (saveAsFile)
                {
                    var fileName = string.Format("{0}-{1}-template-{2}.html", page.Name, string.IsNullOrEmpty(page.Section) ? "_" : page.Section, templateGuid);
                    var savePath = Server.MapPath("~") + @"\Views\CompiledTemplates\" + fileName;
                    // delete the template if it exists 
                    var oldFile = FileHelper.GetFile(page.Name, page.Section);
                    if (oldFile != null && System.IO.File.Exists(oldFile.Path))
                    {
                        System.IO.File.Delete(oldFile.Path);
                    }

                    using (var sw = System.IO.File.CreateText(savePath))
                    {
                        await sw.WriteAsync(page.CompiledTemplate);
                    }
                } // end if saveAsFile
            } // end try
            catch (Exception ex) // catch exception from saving as file
            {
                Errors.Add(string.Format("Template Compile Error: {0}", ex.Message));
                if (ex.GetType() == typeof(TemplateParsingException))
                {
                    Errors.Add(string.Format("Line: {0}", ((TemplateParsingException)ex).Line));
                }
                Errors.Add(string.Format("Stack Trace: \r\n {0}", ex.StackTrace));
                // if failed to save model get why
                if (_db.GetValidationErrors().Any())
                {
                    foreach (var error in _db.GetValidationErrors())
                    {
                        foreach (var valErr in error.ValidationErrors)
                        {
                            Errors.Add(string.Format("Model Error: {0}, Model Property: {1}",
                                valErr.ErrorMessage, valErr.PropertyName));
                        }
                    } // end foreach this._db.GetValidationErrors()
                } // end if have validation errors
            } // end catch

            // save copy in db regardless of saveAsFile param
            var pageInDb =
                        _db.Page.FirstOrDefault(p => p.Name.Equals(page.Name, StringComparison.CurrentCultureIgnoreCase) &&
                                                    p.Section.Equals(page.Section,
                                                    StringComparison.CurrentCultureIgnoreCase));
            if (pageInDb != null)
            {
                // update the page if it exists
                pageInDb.Model = page.Model;
                pageInDb.Template = page.Template;
                pageInDb.CompiledModel = page.CompiledModel;
                pageInDb.CompiledTemplate = page.CompiledTemplate;
            }
            else
            {
                _db.Page.Add(page);
            }

            try
            {
                await _db.SaveChangesAsync();
            } 
            catch (Exception ex) // catch exception from saving to db
            {
                this.Errors.Add(string.Format("Error Saving Model: {0}", ex.Message));
            } // end catch
            finally
            {
                _db.Dispose();
            }
            
            return page;
        }
    }
}