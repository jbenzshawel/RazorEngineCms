using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Threading.Tasks;
using RazorEngineCms.Models;
using RazorEngineCms.App_Classes;
using RazorEngineCms.ExtensionClasses;
using System.Collections.Concurrent;
using RazorEngineCms.DAL.Repository;

namespace RazorEngineCms.Controllers
{
    public class PageController : BaseController
    {
        public IDictionary<string, string> QueryStringParams { get; set; }

        public PageController(IRepositoryService repository) : base(repository)
        {
            this.QueryStringParams = System.Web.HttpContext.Current.Request.QueryString.ToDictionary();
        }

        #region Shared Actions

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
                                (!string.IsNullOrEmpty(iPage.CompiledModel) || iPage.HasInclude) &&
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
                page = this._repository.FindPage(section, name);
            }

            // when page doesn't have parameters can use pre-compiled template 
            var templateIsCompiled = page != null &&
                                     (!string.IsNullOrEmpty(page.CompiledTemplate) || !templateNeedsCompiled(page)) &&
                                     !page.HasParams;
            if (templateIsCompiled) //
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
        #endregion

        #region CMS Pages

        /// <summary>
        /// Gets a list of pages from the database and returns a PageList to the View
        /// </summary>
        /// <returns></returns>
        [AuthRedirect]
        public ActionResult List()
        {
            List<Page> pageList = new List<Page>();
            if (FileHelper.Files.Any())
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

            List<Page> pagesInDb = this._repository.AllPages();
            if (pagesInDb.Count > 0)
            {
                pageList.AddRange(pagesInDb);
            }

            return View(pageList);
        }

        // GET: Page/New
        [AuthRedirect]
        public ActionResult New()
        {
            return View();
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
            var page = this._repository.FindPage(section, name);

            if (page != null)
            {
                return View(page);
            }

            // return 404 view if could not find page in db or files 
            return View("~/Views/NotFound.cshtml");
        }

        /// <summary>
        /// Compiles template model and template then saves results in Pages table
        /// or as a file in /Views/CompiledTemplates/
        /// </summary>
        /// <param name="pageRequest"></param>
        /// <returns>JsonResult with boolean status and list of errors</returns>
        // POST: Page/New
        [HttpPost]
        [Authorize]
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
                    page = await _CompileTemplateAndSavePage(page, saveAsFile: pageRequest.CreateTemplateFile);
                }  // end if no errors after compiling model
            } // end if valid model state 
            else
            {
                Errors.Add("Invalid parameters");
            }

            return Json(new { Status = Errors.Count == 0, Errors });
        }

        /// <summary>
        /// Postback for deleting a page 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="variable"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize]
        public async Task<ActionResult> Delete(AjaxPageRequest pageRequest)
        {
            this.Errors = await this._repository.DeletePage(new Page
            {
                Id = pageRequest.Id,
                Name = pageRequest.Name,
                Section = pageRequest.Section
            }, this.Errors);

            return Json(new { Status = this.Errors.Count == 0, this.Errors });
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult> Copy(AjaxPageRequest pageRequest)
        {
            bool status = false;
            Page copiedPage = null;
            Page origPage = this._repository.FindPage(pageRequest.Section, pageRequest.Name);
            if (origPage != null)
            {
                copiedPage =  await this._repository.CopyPage(origPage, this.Errors);
                status = copiedPage.Id > 0;
                if (!status)
                {
                    this.Errors.Add("Server error copying page.");
                }
            }
            else
            {
                this.Errors.Add("Could not find page.");
            }

            return Json(new { Status = status, NewId = copiedPage.Id, Errors = this.Errors });
        }

        /// <summary>
        /// Compiles a page template by parsing a json object in page.CompiledModel and
        /// uses RazorEngine to compile the Template view. Save results in the database 
        /// and optional as a file as well
        /// </summary>
        /// <param name="page"></param>
        /// <param name="saveAsFile"></param>
        /// <returns></returns>
        private async Task<Page> _CompileTemplateAndSavePage(Page page, bool saveAsFile = false)
        {
            if ((!string.IsNullOrEmpty(page.Model) || page.HasInclude) &&
                !page.HasParams) // don't want to save compiled template if want to use params in Model
            {
                ConcurrentBag<string> errors = this.Errors;
                page.CompileTemplate(ref errors);
            }
            else if (string.IsNullOrEmpty(page.Model) && !page.HasParams && !page.HasInclude) // no template model or include so do not need to compile template
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

            var saveErrors = await this._repository.SavePage(page, this.Errors);
            if (saveErrors.Count > 0)
            {
                this.Errors.AddRange(saveErrors);
            }

            return page;
        }
    }

    #endregion
}