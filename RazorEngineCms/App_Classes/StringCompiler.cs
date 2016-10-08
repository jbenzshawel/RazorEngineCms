using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Web;

namespace RazorEngineCms.App_Classes
{
    /// <summary>
    /// Compiles C# code then returns what is assigned to the string
    /// "Model" object (e.g. "var Model = new { Test = \"example\" }").
    /// The Json serialized object of the Model variable is returned with
    /// the .ToString() extension.
    /// </summary>
    public class StringCompiler : IDisposable
    {
        public string JsonResult { get; set; }

        public IList<string> Errors { get; set; }

        public bool IsValid { get { return this.Errors.Count == 0; } }

        public StringCompiler()
        {
            this.Errors = new List<string>();
        }

        public void CompilePageModel(string model, string param = null, string param2 = null)
        {
            if (string.IsNullOrEmpty(model))
            {
                this.Errors.Add("model cannot be empty in CompilePageModel");
                return;
            }

            using (var CSharpProvider = new CSharpCodeProvider())
            {
                var compileModelGuid = Guid.NewGuid().ToString();
                var pageModelSource = string.Empty;
                using (var sw = new StringWriter())
                {
                    sw.WriteLine("using System;");
                    sw.WriteLine("using System.Data;");
                    sw.WriteLine("using System.Data.SqlClient;");
                    sw.WriteLine("using System.Collections.Generic;");
                    sw.WriteLine("using System.Linq;");
                    sw.WriteLine("using System.Web;");
                    sw.WriteLine("using System.Xml.Serialization;");
                    sw.WriteLine("using Newtonsoft.Json;");
                    sw.WriteLine("using RazorEngineCms.App_Classes;");
                    sw.WriteLine("public class ModelClass { ");
                    sw.WriteLine("public string Execute(HttpContext httpContext, string param = null, string param2 = null) { ");
                    sw.WriteLine(model);
                    sw.WriteLine("return JsonConvert.SerializeObject(Model);");
                    sw.WriteLine("} ");
                    sw.WriteLine("}");
                    pageModelSource = sw.ToString();
                } // end using StringWriter
            
                // define parameters for CSharpCodeProvider
                var paramz = new CompilerParameters()
                {
                    GenerateInMemory = true,
                    GenerateExecutable = false,
                    OutputAssembly = string.Format("temp-assemly-{0}", compileModelGuid)
                    
                };
                // if a reference is added in the above C# string make sure it is also added as a paramater 
                paramz.ReferencedAssemblies.AddRange(new string[] { "System.dll",
                                                                    "System.Linq.dll",
                                                                    "System.Data.dll",
                                                                    "System.Xml.dll",
                                                                    "System.Web.dll",
                                                                    @"C:\Git\RazorEngineCms\RazorEngineCms\bin\RazorEngineCms.dll",
                                                                    @"C:\Git\RazorEngineCms\packages\Newtonsoft.Json.9.0.1\lib\net45\Newtonsoft.Json.dll" });

                // try to compile model 
                var providerResult = CSharpProvider.CompileAssemblyFromSource(paramz, pageModelSource);
                try
                {
                    var type = providerResult.CompiledAssembly.GetType("ModelClass");
                    var obj = Activator.CreateInstance(type);
                    // Method ModelClass.Execute has one parameter of type HttpContext 
                    var httpContextParamater = HttpContext.Current;
                    // Invoke method. Method returns an object that will be parsed as JSON to pass to the view 
                    object output = null;
                    if (string.IsNullOrEmpty(param))
                    {
                        output = type.GetMethod("Execute").Invoke(obj, new object[] { httpContextParamater, param, param2 });
                    }
                    else
                    {
                        output = type.GetMethod("Execute").Invoke(obj, new object[] { httpContextParamater });                        
                    }
                    this.JsonResult = output.ToString();
                } // end try compile model
                catch (Exception ex)
                {
                    this.JsonResult = "{ \"Error\" : \"failed to compile model\" }";
                    if (providerResult.Errors.HasErrors)
                    {
                        for (var i = 0; i < providerResult.Errors.Count; i++)
                        {
                            var errorLine = providerResult.Errors[i].Line > 4 ? providerResult.Errors[i].Line - 4 : providerResult.Errors[i].Line;
                            // need to adjust error lines since ui editor is offset  
                            if (errorLine > 5)
                            {
                                errorLine = errorLine - 6;
                            }
                            this.Errors.Add(string.Format("Model Compile Error: {0}, Line: {1}", providerResult.Errors[i].ErrorText, errorLine)); 
                        }
                    }
                    this.Errors.Add(ex.Message);
                } // end catch 
            } // end using CSharpCodeProvider 
            return; 
        }

        // string override that returns json string of compiled model
        public override string ToString()
        {
            return this.JsonResult;
        }
        
        void IDisposable.Dispose()
        {
            this.JsonResult = string.Empty;
            this.Errors.Clear();
        }
    }
}