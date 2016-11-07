using RazorEngineCms.App_Classes;
using RazorEngineCms.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RazorEngineCms.DAL.Repository
{
    public class Repository<T> : IRepository<T> where T: class
    {
        internal ApplicationContext _db { get; set; }

        public Repository(ApplicationContext db)
        {
            this._db = db;
        }

        /// <summary>
        /// Find Page or Include by Id in db
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public T Find(int Id)
        {
            if (typeof (T) == typeof(Include))
            {
                return this._db.Include.FirstOrDefault(i => i.Id == Id) as T;
            }
            else if (typeof(T) == typeof(Page))
            {
                return this._db.Page.FirstOrDefault(p => p.Id == Id) as T;
            }

            return null;
        }

        /// <summary>
        /// List all pages or includes in a List of T
        /// </summary>
        /// <returns></returns>
        public List<T> All()
        {
            List<T> listT = new List<T>();

            if (typeof(T) == typeof(Page))
            {
                listT = this._db.Page.ToList() as List<T>;
            }
            else if (typeof(T) == typeof(Include))
            {
                listT = this._db.Include.ToList() as List<T>;
            }
            
            return listT;
        }

        /// <summary>
        /// Find a Page object by its section and name. 
        /// </summary>
        /// <param name="section"></param>
        /// <param name="name"></param>
        /// <param name="updated"></param>
        /// <returns></returns>
        public Page Find(string section, string name)
        {
            var page = new Page { Name = name, Section = section };
            var fileHelper = new FileHelper();
            // first see if there is a file template 
            if (fileHelper.Files.Any(f => string.Equals(f.Name, name, StringComparison.InvariantCultureIgnoreCase)))
            {
                var file = fileHelper.GetFile(name, section);
                page.CompiledTemplate = file.ToString();
            }
            else // get page from database if there isn't a file
            {
                page = this.FindInDb(section, name);
            }

            return page;
        }

        public Page FindInDb(string section, string name)
        {
            return this._db.Page
                                .FirstOrDefault(p => p.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase) &&
                                                        p.Section.Equals(section, StringComparison.InvariantCultureIgnoreCase));
        }

        /// <summary>
        /// Save object of type T (Include or Page)
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="errors"></param>
        /// <returns></returns>
        public async Task Save(T obj, ConcurrentBag<string> errors)
        {
            if (obj.GetType() == typeof(Page))
            {
                var page = obj as Page;
                // save copy in db regardless of saveAsFile param
                var pageInDb = this.FindInDb(page.Section, page.Name);
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
            }
            else if (obj.GetType() == typeof(Include))
            {
                var include = obj as Include;
                var includeInDb = this._db.Include.FirstOrDefault(i => i.Id == include.Id);
                // set updated to now before upsert 
                include.Updated = DateTime.Now;
                if (includeInDb != null) // update the include if it exists
                {
                    includeInDb.Name = include.Name;
                    includeInDb.Type = include.Type;
                    includeInDb.Content = include.Content;
                    includeInDb.Updated = include.Updated;

                }
                else // insert a new include 
                {
                    this._db.Include.Add(include);
                }
            }

            await this.SaveAsync(errors);
         }

        /// <summary>
        /// Copies object of type T (Include or Page)
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="errors"></param>
        /// <returns></returns>
        public async Task<T> Copy(T obj, ConcurrentBag<string> errors)
        {
            object returnObj = null;
            if (obj.GetType() == typeof(Page))
            {
                var page = obj as Page; 
                Page origPage = this._db.Page.FirstOrDefault(p => p.Id == page.Id);
                if (origPage != null)
                {
                    // clone the page 
                    origPage.Updated = DateTime.Now;
                    this._db.Page.Add(origPage);
                    returnObj = origPage;
                } // end if origPage != null
            }
            else if (obj.GetType() == typeof(Include))
            {
                var include = obj as Include;
                Include origInclude = this._db.Include.FirstOrDefault(i => i.Id == include.Id);
                if (origInclude != null)
                {
                    origInclude.Updated = DateTime.Now;
                    this._db.Include.Add(origInclude);
                    returnObj = origInclude;
                }

            }

            await this.SaveAsync(errors);

            return returnObj as T;
        }

        /// <summary>
        /// Deletes object of type T (Include or Page)
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="errors"></param>
        /// <returns></returns>
        public async Task Delete(T obj, ConcurrentBag<string> errors, bool ignoreFiles = false)
        {
            if (obj.GetType() == typeof(Page))
            {
                var page = obj as Page;
                Page pageModel;
                if (ignoreFiles)
                    pageModel = this.FindInDb(page.Section, page.Name);
                else
                    pageModel = this.Find(page.Section, page.Name);

                if (pageModel == null)
                {
                    errors.Add("Page not found");
                }
                else
                {
                    this._db.Page.Attach(pageModel);
                    this._db.Page.Remove(pageModel);
                }
            } 
            else if (obj.GetType() == typeof(Include))
            {
                var include = obj as Include;
                Include includeModel = this.Find(include.Id) as Include;
                if (includeModel == null)
                {
                    errors.Add("Include not found");
                }
                else
                {
                    this._db.Include.Attach(includeModel);
                    this._db.Include.Remove(includeModel);
                }
            }

            await this.SaveAsync(errors);
        }  

        /// <summary>
        /// Saves changes in _db ApplicationContext and adds errors 
        /// to errors concurrent bag.
        /// </summary>
        /// <param name="errors"></param>
        /// <returns></returns>
        private async Task SaveAsync(ConcurrentBag<string> errors)
        {
            try
            {
                if (await this._db.SaveChangesAsync() < 1)
                {
                    errors.Add("0 rows updated");

                }
            }
            catch (Exception ex)
            {
                errors.Add(string.Format("Error Saving Model: {0}", ex.Message));
                // if failed to save model get why
                if (this._db.GetValidationErrors().Any())
                {
                    foreach (var error in this._db.GetValidationErrors())
                    {
                        foreach (var valErr in error.ValidationErrors)
                        {
                            errors.Add(string.Format("Model Error: {0}, Model Property: {1}",
                                valErr.ErrorMessage, valErr.PropertyName));
                        }
                    } // end foreach this._db.GetValidationErrors()
                } // end if have validation errors
            }
        }
    }
}