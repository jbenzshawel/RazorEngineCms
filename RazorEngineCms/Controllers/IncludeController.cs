using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using RazorEngineCms.App_Classes;
using RazorEngineCms.Models;
using System.Threading.Tasks;
using RazorEngineCms.DAL.Repository;

namespace RazorEngineCms.Controllers
{
    public class IncludeController : BaseController
    {
        public IncludeController(IRepositoryService repository) : base(repository)
        {
        }

        // GET: Include/New
        [AuthRedirect]
        public ActionResult New()
        {
            return View();
        }

        [AuthRedirect]
        public ActionResult Edit(int id)
        {
            var Include = this._repository.FindInclude(id);
            return View(Include);
        }

        // POST: Include/Save
        [HttpPost]
        [Authorize]
        public async Task<ActionResult> Save(Include includeModel)
        {
            bool isValid = false;
            
            if (ModelState.IsValid)
            {
                this.Errors = await this._repository.SaveInclude(includeModel, this.Errors);
            }
            else
            {
                this.Errors.Add("Invalid model parameter.");
            }

            return Json(new { Status = isValid, Errors, Data = new { IncludeId = includeModel.Id } });
        }

        [AuthRedirect]
        public ActionResult List()
        {
            var includes = new List<Include>(); 

            if (this._repository.db.Include.Any())
            {
                includes = this._repository.db.Include.OrderByDescending(i => i.Updated).ToList(); 
            }


            return View(includes);
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult> Delete(string id)
        {
            bool status = false;
            int includeId;
            
            if (int.TryParse(id, out includeId))
            {
                var includeModel = this._repository.db.Include.FirstOrDefault(i => i.Id == includeId);
                if (includeModel != null)
                {
                    try
                    {
                        this._repository.db.Include.Attach(includeModel);
                        this._repository.db.Include.Remove(includeModel);
                        status = await this._repository.db.SaveChangesAsync() > 0;

                    }
                    catch (Exception ex)
                    {
                        this.Errors.Add(ex.Message);
                    }
                }
                else
                {
                    this.Errors.Add("Include not found");
                }
            } // end try parse includeId 

            return Json(new { Status = status, this.Errors });
        }
    }
}