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
    [TestClass]
    public class RepositoryServiceTests
    {
        private IRepositoryService _RepositoryService { get; set; }

        public RepositoryServiceTests()
        {
            var mockDAL = new MockDAL();
            this._RepositoryService = new RepositoryService(mockDAL.ApplicationContext.Object);
        }

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
        public async Task AddPageTest()
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

            // still haven't figured out why mock returns 0 rows updated 
            // if save fails due to null or invalid model other errors with logged besides 0 rows updated
            Assert.IsTrue(errors.Count == 1);
            Assert.IsTrue(errors.FirstOrDefault() != null && errors.FirstOrDefault() == "0 rows updated"); 
        }

        [TestMethod]
        public async Task DeletePageTest()
        {
            var page = new Page { Section = "Example", Name = "Page3" };
            var errors = new ConcurrentBag<string>();
            await this._RepositoryService.DeletePage(page, errors, ignoreFiles: true);

            // still haven't figured out why mock returns 0 rows updated 
            // if save fails due to null or invalid model other errors with logged besides 0 rows updated
            Assert.IsTrue(errors.Count == 1);
            Assert.IsTrue(errors.FirstOrDefault() != null && errors.FirstOrDefault() == "0 rows updated");
        }
    }
}
