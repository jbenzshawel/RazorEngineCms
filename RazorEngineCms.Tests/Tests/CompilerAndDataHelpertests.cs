using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.CSharp;
using Newtonsoft.Json;
using RazorEngineCms.App_Classes;
using RazorEngineCms.ExtensionClasses;
using RazorEngineCms.DAL.RepositoryService;
using RazorEngineCms.Tests.Mocks;
using RazorEngineCms.Models;
using System.Collections.Concurrent;
using System.Linq;

namespace RazorEngineCMS.Tests
{
    [TestClass]
    public class CompilerAndDataHelperTests
    {

        private IRepositoryService _RepositoryService { get; set; }

        public CompilerAndDataHelperTests()
        {
            var mockDAL = new MockDAL();
            this._RepositoryService = new RepositoryService(mockDAL.ApplicationContext.Object);
        }

        [TestMethod]
        public void TestCompile()
        {
            var model = "Model = new { test = \"object\" };";

            using (var CSharpProvider = new CSharpCodeProvider())
            {
                var paramz = new System.CodeDom.Compiler.CompilerParameters()
                {
                    GenerateInMemory = true
                };
                paramz.ReferencedAssemblies.Add("Newtonsoft.Json.dll");
                var res = CSharpProvider.CompileAssemblyFromSource(paramz,
                   @"
                    using Newtonsoft.Json;
                    public class ModelClass {
                        public string Execute() {
                            object Model;" +
                            model +
                            @"return JsonConvert.SerializeObject(Model);        
                        }
                    }");

                var type = res.CompiledAssembly.GetType("ModelClass");

                var obj = Activator.CreateInstance(type);

                var output = type.GetMethod("Execute").Invoke(obj, new object[] { });

                Assert.IsNotNull(JsonConvert.DeserializeObject(output.ToString()));
            }

        }

        [TestMethod]
        public void TestStringCompiler()
        {
            var model = "var Model = new { test = \"object\" };";

            using (var stringCompiler = new StringCompiler())
            {
                stringCompiler.CompilePageModel(model);

                Assert.IsTrue(stringCompiler.IsValid);
            }
        }

        [TestMethod]
        public void CompilePageModelTest()
        {
            Page page = this._RepositoryService.FindPageInDb("Example", "Page1");

            using (var stringCompiler = new StringCompiler())
            {
                stringCompiler.CompilePageModel(page.Model);

                Assert.IsTrue(stringCompiler.IsValid);
            }
        }

        [TestMethod]
        public void CompilePageTemplateTest()
        {
            var errors = new ConcurrentBag<string>();
            Page page = this._RepositoryService.FindPageInDb("Example", "Page1");
            string jsonModel = null;
            using (var stringCompiler = new StringCompiler())
            {
                stringCompiler.CompilePageModel(page.Model);
                if (stringCompiler.IsValid)
                {
                    jsonModel = stringCompiler.ToString();
                }
            }
            
            if (!string.IsNullOrEmpty(jsonModel))
            {
                page.CompileTemplate(ref errors, page.Template, jsonModel);
            }
            else
            {
                errors.Add("Error compiling Model");
            }

            Assert.IsTrue(errors.Count() == 0);
            Assert.IsNotNull(page.CompiledTemplate);
        }

        // commented out since integration test and is failing
        // on CI build / test 
        //[TestMethod]
        //public void TestGetData()
        //{
        //    var dataHelper = new DataHelper();
        //    var results = dataHelper.GetData("prGetPages");

        //    Assert.IsTrue(results.Rows.Count > 0);
        //    Assert.IsTrue(!string.IsNullOrEmpty(results.ToJson()));
        //}
    }
}
