using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RazorEngineCms.Tests.Mocks;
using RazorEngineCms.DAL.RepositoryService;
using RazorEngineCms.Models;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace RazorEngineCms.Tests
{
    /// <summary>
    /// Contains test for the methods in the RazorEngineCms.DAL.RepositoryService class
    /// using mock data in MockDAL.ApplicationContext of type Mock&lt;ApplicationContext&gt;
    /// </summary>
    [TestClass]
    public class RepositoryServiceTests
    {
        private IRepositoryService _RepositoryService { get; set; }

        public RepositoryServiceTests()
        {
            var mockDAL = new MockDAL();
            this._RepositoryService = new RepositoryService(mockDAL.ApplicationContext.Object);
        }

        #region Page Repository Tests
        [TestMethod]
        public void AllPagesTest()
        {
            List<Page> allPages = this._RepositoryService.AllPages();

            Assert.IsTrue(allPages.Count == 5);
            Assert.IsNotNull(allPages.FirstOrDefault(p => p.Id == 2));
        }

        [TestMethod]
        public void FindPageTest()
        {
            Page page = this._RepositoryService.FindPageInDb("Example", "Page2");

            Assert.IsNotNull(page);
            Assert.IsNotNull(page.Model);
            Assert.IsNotNull(page.Template);
        }

        [TestMethod]
        public async Task SavePageTest()
        {
            var page = new Page
            {
                Section = "Example",
                Name = "New Page",
                Model = "var Model = new { Test = \"Test\" };",
                Template = "@Model",
                Updated = DateTime.Now
            };
            var errors = new ConcurrentBag<string>();
            await this._RepositoryService.SavePage(page, errors);

            this._AssertErrors(errors);
        }

        [TestMethod]
        public async Task DeletePageTest()
        {
            var page = new Page { Section = "Example", Name = "Page3" };
            var errors = new ConcurrentBag<string>();
            await this._RepositoryService.DeletePage(page, errors, ignoreFiles: true);

            this._AssertErrors(errors);
        }

        [TestMethod]
        public async Task CopyPageTest()
        {
            var page = this._RepositoryService.FindPageInDb("Example", "Page1");
            var errors = new ConcurrentBag<string>();
            Page copiedPage = null;
            if (page != null)
            {
                copiedPage = await this._RepositoryService.CopyPage(page, errors);
            }
            else
            {
                errors.Add("Page not found");
            }

            this._AssertErrors(errors);
            Assert.IsNotNull(copiedPage);
        }
        #endregion

        #region Include RepositoryTests
        [TestMethod]
        public void AllIncludesTest()
        {
            List<Include> includeList = this._RepositoryService.AllIncludes();

            Assert.IsTrue(includeList.Count() == 5);
            Assert.IsNotNull(includeList.FirstOrDefault(i => i.Id == 1));
        }

        [TestMethod]
        public void FindIncludeTest()
        {
            Include include = this._RepositoryService.FindInclude(1);

            Assert.IsNotNull(include);
            Assert.IsTrue(!string.IsNullOrEmpty(include.Content));
        }

        [TestMethod]
        public async Task SaveIncludeTest()
        {
            var include = new Include
            {
                Name = "New Include!!!",
                Content = "Test",
                Type = "javascript"
            };
            var errors = new ConcurrentBag<string>();
            await this._RepositoryService.SaveInclude(include, errors);

           }

        [TestMethod]
        public async Task DeleteIncludeTest()
        {
            var errors = new ConcurrentBag<string>();

            await this._RepositoryService.DeleteInclude(new Include { Id = 1 }, errors);

            this._AssertErrors(errors);
        }
        #endregion

        private void _AssertErrors(ConcurrentBag<string> errors)
        {
            Assert.IsTrue(errors.Count == 0);
            //Assert.IsTrue(errors.FirstOrDefault() != null && errors.FirstOrDefault() == "0 rows updated");

        }
    }
}
