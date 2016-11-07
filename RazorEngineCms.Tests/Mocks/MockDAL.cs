using System;
using System.Data.Entity;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RazorEngineCms.DAL;
using RazorEngineCms.Models;
using RazorEngineCMS.Tests;
using Moq;

namespace RazorEngineCms.Tests.Mocks
{
    /// <summary>
    /// Contains a Mock ApplicationContext with 5 Page objects and 5 Include objects
    /// </summary>
    public class MockDAL
    {
        /// <summary>
        /// Number of Page objects to Mock
        /// </summary>
        public const int NUMBER_PAGES = 5;

        /// <summary>
        /// Number of Include objects to Mock
        /// </summary>
        public const int NUMBER_INCLUDES = 5;

        /// <summary>
        /// Type of Includes to be Mocked
        /// </summary>
        public const string INCLUDE_TYPE = "html";

        /// <summary>
        /// Mock ApplicationContext. Can access ApplicationContext 
        /// with ApplicationContext.Object
        /// </summary>
        public Mock<ApplicationContext> ApplicationContext { get; set; }
                
        public MockDAL()
        {
            this._SetupApplicationContext();
        }

        /// <summary>
        /// Sets up mock Application Context with a mock Page List object containing 5 pages and 
        /// a mock Include List object containg 5 includes
        /// </summary>
        private void _SetupApplicationContext()
        {
            // setup mock db.Page
            IQueryable<Page> mockPagesQueryable = this._GetPages(NUMBER_PAGES);
            Mock<IDbSet<Page>> mockPageDbSet = new Mock<IDbSet<Page>>();
            mockPageDbSet.Setup(dbSet => dbSet.Provider).Returns(mockPagesQueryable.Provider);
            mockPageDbSet.Setup(dbSet => dbSet.Expression).Returns(mockPagesQueryable.Expression);
            mockPageDbSet.Setup(dbSet => dbSet.ElementType).Returns(mockPagesQueryable.ElementType);
            mockPageDbSet.Setup(dbSet => dbSet.GetEnumerator()).Returns(mockPagesQueryable.GetEnumerator());

            // setup mock db.Include
            IQueryable<Include> mockIncludesQueryable = this._GetIncludes(NUMBER_INCLUDES);
            Mock<IDbSet<Include>> mockIncludeDbSet = new Mock<IDbSet<Include>>();
            mockIncludeDbSet.Setup(dbSet => dbSet.Provider).Returns(mockIncludesQueryable.Provider);
            mockIncludeDbSet.Setup(dbSet => dbSet.Expression).Returns(mockIncludesQueryable.Expression);
            mockIncludeDbSet.Setup(dbSet => dbSet.ElementType).Returns(mockIncludesQueryable.ElementType);
            mockIncludeDbSet.Setup(dbSet => dbSet.GetEnumerator()).Returns(mockIncludesQueryable.GetEnumerator());
            
            // attach mock db page and include to application context 
            this.ApplicationContext = new Mock<ApplicationContext>();
            this.ApplicationContext.Setup(db => db.Page).Returns(mockPageDbSet.Object);
            this.ApplicationContext.Setup(db => db.Include).Returns(mockIncludeDbSet.Object);
            this.ApplicationContext.Setup(db => db.SaveChanges()).Returns(1);
            this.ApplicationContext.Setup(db => db.SaveChangesAsync()).ReturnsAsync(1);
        }

        /// <summary>
        /// Returns a list of mock Page objects having the count of the 
        /// integer passed in
        /// </summary>
        /// <param name="numberOfPages"></param>
        /// <returns></returns>
        private IQueryable<Page> _GetPages(int numberOfPages)
        {
            List<Page> mockPages = new List<Page>();
            for (var i = 1; i < numberOfPages + 1; i++)
            {
                mockPages.Add(new Page
                {
                    Id = i,
                    Section = "Example",
                    Name = "Page" + i,
                    Updated = DateTime.Now.AddHours(i),
                    Model = @"int pageId = _urlParameters.Param != null && _urlParameters.Param.Length > 0 ? int.Parse(_urlParameters.Param) : 0;
                            DataTable pageData = null; 
                            if (pageId != null && pageId > 0) {
                                var paramz = new List<SqlParameter> {
                                new SqlParameter {" +
                                    "ParameterName = \"ID\"," +
                                    @"Value = pageId,
                                                DbType = DbType.Int32
                                            }
                                };
                                        var dataHelper = new DataHelper();" +
                                        "pageData = dataHelper.GetData(\"prGetPageById\", paramz);" +
                                    @"}
                                    var Model = new { data = pageData, urlParameters = _urlParameters };",
                    Template = @"<h1>Model Dump</h1>
                                <pre>
                                  @Model
                                </pre>"
                });
            }
            return mockPages.AsQueryable();
        }

        /// <summary>
        /// Returns a list of includes objects having the count of the
        /// integer passed in
        /// </summary>
        /// <param name="numberOfIncludes"></param>
        /// <returns></returns>
        private IQueryable<Include> _GetIncludes(int numberOfIncludes)
        {
            List<Include> includes = new List<Include>();
            for(var i = 1; i < numberOfIncludes + 1; i++)
            {
                includes.Add(new Include
                {
                    Id = i,
                    Name = "Include" + i,
                    Content = "<h1>Heading " + i + "</h1>",
                    Updated = DateTime.Now.AddHours(i),
                    Type = INCLUDE_TYPE
                });
            }

            return includes.AsQueryable();
        }
    }
}