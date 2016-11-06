using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.CSharp;
using Newtonsoft.Json;
using RazorEngineCms.App_Classes;

namespace RazorEngineCMS.Tests
{
    [TestClass]
    public class CompilerAndDataHelperTests
    {
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
        public void TestGetData()
        {
            var dataHelper = new DataHelper();
            var results = dataHelper.GetData("prGetPages");

            Assert.IsTrue(results.Rows.Count > 0);
        }
    }
}
