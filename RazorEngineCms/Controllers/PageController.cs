using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Threading.Tasks;
using RazorEngineCms.DAL;
using RazorEngineCms.Models;
using RazorEngineCms.App_Classes;
using RazorEngineCms.ExtensionClasses;
using System.Collections.Concurrent;

namespace RazorEngineCms.Controllers
{
    public class PageController : Controller
    {
        public ConcurrentBag<string> Errors { get; set; }

        public IDictionary<string,string> QueryStringParams { get; set; }

        internal bool AllowCache { get; set; }

        internal FileHelper FileHelper { get; set; }

        internal CacheManager CacheManager { get; set; }

        private ApplicationContext _db { get; set; }

        public PageController()
        {
            this._db = new ApplicationContext();
            this.Errors = new ConcurrentBag<string>();
            this.FileHelper = new FileHelper();
            this.AllowCache = ConfigurationManager.AppSettings["AllowPageCaching"] == "true";
            if (this.AllowCache)
            {
                this.CacheManager = new CacheManager();
            }
            this.QueryStringParams = System.Web.HttpContext.Current.Request.QueryString.ToDictionary();
        }

        // GET: Page/New
        [AuthRedirect]
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
        [AuthRedirect]
        public async Task<ActionResult> Save(PageRequest pageRequest)
        {
            var page = new Page(pageRequest);
            if (ModelState.IsValid)
            {
                if (!string.IsNullOrEmpty(page.Model))
                {
                    // compile the model from the string Model declaration in pageRequest
                    // always want to compile model before saving to catch any compile errors
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
        [AuthRedirect]
        public ActionResult Edit(string section, string name)
        {
            var page = Page.FindPage(section, name);

            if (page != null)
            {
                return View(page);
            }

            // return 404 view if could not find page in db or files 
            return View("~/Views/NotFound.cshtml");
        }

        /// <summary>
        /// Postback for deleting a page 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="variable"></param>
        /// <returns></returns>
        [HttpPost]
        [AuthRedirect]
        public async Task<ActionResult> Delete(AjaxPageRequest pageRequest)
        {
            bool status = false;
            Page pageModel = pageRequest.GetPage();

            if (pageModel == null)          
            {
                this.Errors.Add("Page not found");
            }
            else
            {
                try
                {
                    this._db.Page.Attach(pageModel);
                    this._db.Page.Remove(pageModel);
                    status = await this._db.SaveChangesAsync() > 0;
                }
                catch (Exception ex)
                {
                    this.Errors.Add(ex.Message);
                    status = false;
                }
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
        // GET: /Section/Name 
        public ActionResult View(string section, string name, string param = null, string param2 = null)
        {
            Page page = null; // will store the page once we find it
            Func<Page, bool> templateNeedsCompiled = (iPage) => (string.IsNullOrEmpty(iPage.CompiledTemplate) &&
                                !string.IsNullOrEmpty(iPage.CompiledModel) &&
                                !string.IsNullOrEmpty(iPage.Template));
            // templage model that will be passed to the View
            var template = new PageTemplate { Content = string.Empty };

            // if AllowCache enabled in Web.Config look for the page in cache
            if (this.AllowCache)
            {
                PageCacheModel cachedPage = this.CacheManager.FindPage(name, section, param, param2, this.QueryStringParams);
                if (cachedPage != null && cachedPage.CompiledTemplate != null)
                {
                    page = cachedPage.ToPage(); 
                }
            }

            // if we couldn't get the page from cache get it from the db
            if (page == null || page.CompiledTemplate == null)
            {
                page = Page.FindPage(section, name);
            }

            // when page doesn't have parameters can use pre-compiled template 
            var templateIsCompiled = page != null &&
                                     (!string.IsNullOrEmpty(page.CompiledTemplate) || !templateNeedsCompiled(page)) &&
                                     !page.HasParams;
            if (templateIsCompiled)
            {
                template.Content = page.CompiledTemplate ?? page.Template;
            }
            else if (page != null && (page.HasParams || templateNeedsCompiled(page)))
            {
                // if page has url params pass them to model and compile it
                if (page.HasParams && string.IsNullOrEmpty(page.CompiledModel))
                {
                    using (var stringCompiler = new StringCompiler())
                    {
                        stringCompiler.CompilePageModel(page.Model, param, param2);
                        if (stringCompiler.IsValid)
                        {
                            page.CompiledModel = stringCompiler.ToString();
                        }
                        else
                        {
                            this.Errors.AddRange(stringCompiler.Errors);
                        }
                    } // end using stringCompiler
                } // end if page.HasParams and page / param are not null

                if (templateNeedsCompiled(page))
                {
                    ConcurrentBag<string> errors = this.Errors;
                    page.CompileTemplate(ref errors, page.Template, page.CompiledModel);
                }

                template.Content = page.CompiledTemplate;
            } // end else if page has paramaters 

            // cache page before returning template if enabled 
            if (this.AllowCache)
            {
                
                if (page != null && !this.CacheManager.PageCacheExists(page.Name, page.Section, param, param2, this.QueryStringParams))
                {
                    this.CacheManager.AddPage(page, param, param2);
                }
            } 

            // return the page with a template if it is found
            if (!string.IsNullOrEmpty(template.Content))
            {
                return View(template);
            }


            if (Errors.Count > 0)
            {
                return View("~/Views/ServerError.cshtml", Errors);
            }

            // return 404 view if could not find page in db or files 
            return View("~/Views/NotFound.cshtml");
        }

        [HttpPost]
        [AuthRedirect]
        public ActionResult Copy(AjaxPageRequest pageRequest)
        {
            var status = false;
            int? copiedPageId = null;
            Page pageModel = pageRequest.GetPage();

            if (pageModel != null)
            {
                copiedPageId = Page.Copy(pageModel);
                status = copiedPageId != null ? true : false;
                if (!status)
                {
                    this.Errors.Add("Server error copying page.");
                }
            }
            else
            {
                this.Errors.Add("Could not find page.");
            }

            return Json(new { Status = status, NewId = copiedPageId, Errors = this.Errors });
        }

        /// <summary>
        /// Gets a list of pages from the database and returns a PageList to the View
        /// </summary>
        /// <returns></returns>
        [AuthRedirect]
        public ActionResult List()
        {
            IList<Page> pageList = new PageList();
            if (FileHelper.Files.Count > 0)
            {
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
            }
            
            if (_db.Page.Count() > 0)
            {
                foreach (var page in _db.Page)
            {
                pageList.Add(page);
            }
            }
            
            return View(pageList);
        }

        /// <summary>
        /// If caching enabled clears CacheList and Cache[CACHE_KEY]
        /// </summary>
        /// <returns></returns>
        [AuthRedirect]
        public ActionResult ClearCache()
        {
            if (this.AllowCache)
            {
                CacheManager.ClearCache();
                System.Web.HttpContext.Current.Response.Write("<h1>Page cache has been cleared</h1>");
            }

            return new EmptyResult();
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
            if (!string.IsNullOrEmpty(page.Model) && !page.HasParams)
            {
                ConcurrentBag<string> errors = this.Errors;
                page.CompileTemplate(ref errors);
            }
            else if (string.IsNullOrEmpty(page.Model) && !page.HasParams) // no template model so do not need to compile
            {
                page.CompiledTemplate = page.Template;
            }

            if (saveAsFile && !page.HasParams)
            {
                try
                {
                    var fileName = string.Format("{0}-{1}-template-{2}.html", page.Name, string.IsNullOrEmpty(page.Section) ? "_" : page.Section, Guid.NewGuid());
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

                } // end try
                catch (Exception ex) // catch exception from saving as file
                {
                    this.Errors.Add(string.Format("Error Saving Model as File: {0}", ex.Message));
                } // end catch
            } // end if saveAsFile

            // save copy in db regardless of saveAsFile param
            var pageInDb = this._db.Page.FirstOrDefault(p => p.Name.Equals(page.Name, StringComparison.CurrentCultureIgnoreCase) &&
                                                    p.Section.Equals(page.Section, StringComparison.CurrentCultureIgnoreCase));

            // if the page has url parameters do not want to save precompiled template and model
            if (page.HasParams)
            {
                page.CompiledModel = null;
                page.CompiledTemplate = null;
            }
            // set updated to now before upsert 
            page.Updated = DateTime.Now;
            if (pageInDb != null) // update the page if it exists
            {
                pageInDb.Model = page.Model;
                pageInDb.Template = page.Template;
                pageInDb.CompiledModel = page.CompiledModel;
                pageInDb.CompiledTemplate = page.CompiledTemplate;
                pageInDb.HasParams = page.HasParams;
                pageInDb.Updated = page.Updated;
            }
            else // insert a new page 
            {
                this._db.Page.Add(page);
            }

            try
            {
                await this._db.SaveChangesAsync();
            }
            catch (Exception ex) // catch exception from saving to db
            {
                this.Errors.Add(string.Format("Error Saving Model: {0}", ex.Message));
                // if failed to save model get why
                if (this._db.GetValidationErrors().Any())
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
            finally
            {
                this._db.Dispose();
            }

            return page;
        }
    }
}