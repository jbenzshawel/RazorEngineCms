﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Mono.CSharp;

namespace RazorEngineCMSTests
{
    [TestClass]
    public class UnitTest1
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
    }
}
