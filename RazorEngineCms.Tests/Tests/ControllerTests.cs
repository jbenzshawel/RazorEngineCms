using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RazorEngineCms.Controllers;
using RazorEngineCms.DAL.RepositoryService;
using RazorEngineCms.Models;
using RazorEngineCms.Tests.Mocks;


namespace RazorEngineCms.Tests
{
    [TestClass]
    public class ControllerTests
    {
        private IRepositoryService _RepositoryService { get; set; }

        private PageController _PageController { get; set; }

        public ControllerTests()
        {
            this._RepositoryService = new RepositoryService(new MockDAL().ApplicationContext.Object);
            this._PageController = new PageController(this._RepositoryService);
        }

        #region PageController Tests
        [TestMethod]
        public void PageListViewTest()
        {
            ActionResult listView = this._PageController.List();
            var listViewResult = listView as ViewResult;
            var model = listViewResult.Model as List<Page>;

            Assert.IsNotNull(listView);
            Assert.IsTrue(model.Count == 5);
        }

        [TestMethod]
        public void PageEditViewTest()
        {
            ActionResult editView = this._PageController.Edit("Example", "Page1");
            var editViewResult = editView as ViewResult;
            Page model = null;

            try
            {
                model = editViewResult.Model as Page;
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }

            Assert.IsNotNull(model);
            Assert.IsTrue(!string.IsNullOrEmpty(model.Template));
        }
        #endregion
    }
}
