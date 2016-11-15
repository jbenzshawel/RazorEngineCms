using System.Collections.Generic;
using System.Web.Mvc;
using RazorEngineCms.App_Classes;
using RazorEngineCms.Models;
using System.Threading.Tasks;
using RazorEngineCms.DAL.RepositoryService;

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
            if (ModelState.IsValid)
            {
                await this._repository.SaveInclude(includeModel, this.Errors);
            }
            else
            {
                this.Errors.Add("Invalid model parameter.");
            }

            return Json(new { Status = this.Errors.Count == 0, Errors, IncludeId = includeModel.Id, Updated = includeModel.Updated.ToString() });
        }

        [AuthRedirect]
        public ActionResult List()
        {
            List<Include> includes = this._repository.AllIncludes();
            
            return View(includes);
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult> Copy(string id)
        {
            int parsedId = 0;
            Include newInclude = null;
            if (int.TryParse(id, out parsedId))
            {
                newInclude = await this._repository.CopyInclude(new Include { Id = parsedId }, this.Errors);
            }
            else
            {
                this.Errors.Add("Invalid id.");
            }

            return Json(new { Status = this.Errors.Count == 0, this.Errors, NewInclude = newInclude });
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult> Delete(string id)
        {
            int parsedId = 0;
            if (int.TryParse(id, out parsedId))
            {
                await this._repository.DeleteInclude(new Include { Id = parsedId }, this.Errors);
            }
            else
            {
                this.Errors.Add("Invalid id.");
            }

            return Json(new { Status = this.Errors.Count == 0, this.Errors });
        }
    }
}