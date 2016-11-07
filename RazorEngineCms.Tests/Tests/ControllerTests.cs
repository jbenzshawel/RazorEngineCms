using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RazorEngineCms.Controllers;
using RazorEngineCms.DAL.RepositoryService;
using RazorEngineCms.Models;
using RazorEngineCms.Tests.Mocks;
using Newtonsoft.Json;


namespace RazorEngineCms.Tests
{
    public struct JsonAction
    {
        public bool Status { get; set; }

        public List<string> Errors { get; set; }

        public string Updated { get; set; }
    }

    [TestClass]
    public class ControllerTests
    {
        private IRepositoryService _RepositoryService { get; set; }

        private PageController _PageController { get; set; }

        private IncludeController _includeController { get; set; }

        public ControllerTests()
        {
            this._RepositoryService = new RepositoryService(new MockDAL().ApplicationContext.Object);
            this._PageController = new PageController(this._RepositoryService);
            this._includeController = new IncludeController(this._RepositoryService);
        }

        #region PageController Tests
        [TestMethod]
        public void PageListViewTest()
        {
            ActionResult listView = this._PageController.List();
            var model = this._GetModel<List<Page>>(listView);
            
            Assert.IsNotNull(listView);
            Assert.IsTrue(model.Count == 5);
        }

        [TestMethod]
        public async Task SavePageTest()
        {
            var saveResult = await this._PageController.Save(new PageRequest
            {
                Name = "Controller Page Save",
                Section = "Tests",
                Updated = DateTime.Now,
                HasInclude = false,
                HasParams = true,
                CreateTemplateFile = false,
                Model = "var Model = new { _urlParameters };",
                Template = "@Model"
            });

            this._AssertJsonResult(saveResult);
        }

        [TestMethod]
        public void PageEditViewTest()
        {
            ActionResult editView = this._PageController.Edit("Example", "Page1");
            Page model = this._GetModel<Page>(editView);

            Assert.IsNotNull(model);
            Assert.IsTrue(!string.IsNullOrEmpty(model.Template));
        }
        #endregion

        #region Include Controller Tests

        [TestMethod]
        public void IncludeListViewTest()
        {
            ActionResult listView = this._includeController.List();
            var model = this._GetModel<List<Include>>(listView);

            Assert.IsNotNull(listView);
            Assert.IsTrue(model.Count == 5);
        }

        [TestMethod]
        public async Task SaveIncludeTest()
        {
            ActionResult saveResult = await this._includeController.Save(new Include
            {
                Name = "Test Save",
                Type = "javascript",
                Updated = DateTime.Now
            });

            this._AssertJsonResult(saveResult);
        }

        [TestMethod]
        public void IncludeEditViewTest()
        {
            ActionResult editView = this._includeController.Edit(1);
            Include model = this._GetModel<Include>(editView);

            Assert.IsNotNull(model);
            Assert.IsTrue(!string.IsNullOrEmpty(model.Content));
        }

        #endregion

        private T _GetModel<T>(ActionResult actionResult) where T : class
        {
            T model = null;
            try
            {
                if (actionResult.GetType() == typeof(JsonResult))
                {
                    model = ((JsonResult) actionResult).Data as T;
                }
                else
                {
                    var viewResult = actionResult as ViewResult;
                    model = viewResult.Model as T;
                }
            }
            catch(Exception ex) { }

            return model;
        }

        /// <summary>
        /// Asserts Model.Status is true and Model.Errors.Count = 0
        /// </summary>
        /// <param name="actionResult"></param>
        private void _AssertJsonResult(ActionResult actionResult)
        {
            // get error prop from JsonResult anonymous objct data 
            var model = this._GetModel<object>(actionResult);
            Type type = model.GetType();
            PropertyInfo errorsProperty = type.GetProperty("Errors");
            var errorsList = errorsProperty.GetValue(model, null) as ConcurrentBag<string>;
            PropertyInfo statusProperty = type.GetProperty("Status");

            Assert.IsTrue(bool.Parse(statusProperty.GetValue(model, null).ToString()));
            Assert.IsTrue(errorsList.Count == 0);
        }
    }
}
