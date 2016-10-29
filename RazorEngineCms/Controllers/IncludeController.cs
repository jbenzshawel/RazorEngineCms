using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using RazorEngineCms.App_Classes;
using RazorEngineCms.Models;
using System.Threading.Tasks;

namespace RazorEngineCms.Controllers
{
    public class IncludeController : BaseController
    {
        // GET: Include/New
        [AuthRedirect]
        public ActionResult New()
        {
            return View();
        }

        [AuthRedirect]
        public ActionResult Edit(int id)
        {
            var Include = this._db.Include.FirstOrDefault(i => i.Id == id);
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
                var includeInDb = this._db.Include.FirstOrDefault(i => i.Id == includeModel.Id);
                // set updated to now before upsert 
                includeModel.Updated = DateTime.Now;
                if (includeInDb != null) // update the include if it exists
                {
                    includeInDb.Name = includeModel.Name;
                    includeInDb.Type = includeModel.Type;
                    includeInDb.Content = includeModel.Content;
                    includeInDb.Updated = includeModel.Updated;

                }
                else // insert a new include 
                {
                    this._db.Include.Add(includeModel);
                }

                try
                {
                    isValid = await this._db.SaveChangesAsync() > 0;
                }
                catch (Exception ex)
                {
                    this.Errors.Add("Internal server error saving Include.");
                    this.Errors.Add(ex.Message);
                }
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

            if (this._db.Include.Any())
            {
                includes = this._db.Include.OrderByDescending(i => i.Updated).ToList(); 
            }


            return View(includes);
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult> Delete(string param)
        {
            bool status = false;
            int includeId;
            
            if (int.TryParse(param, out includeId))
            {
                var includeModel = this._db.Include.FirstOrDefault(i => i.Id == includeId);
                if (includeModel != null)
                {
                    try
                    {
                        this._db.Include.Attach(includeModel);
                        this._db.Include.Remove(includeModel);
                        status = await this._db.SaveChangesAsync() > 0;

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