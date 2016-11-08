using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Web;
using RazorEnginePageModelClasses;
using RazorEngineCms.ExtensionClasses;
using RazorEngineCms.App_Classes;

namespace RazorEngineCms.App_Classes
{
    internal struct StringCompilerModel
    {
        public string Template { get; set; }

        public CompilerParameters CopilerParams { get; set; }
    }

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

        public void CompilePageModel(string model, string param = "", string param2 = "")
        {
            if (string.IsNullOrEmpty(model))
            {
                this.Errors.Add("model cannot be empty in CompilePageModel");
                return;
            }

            using (var CSharpProvider = new CSharpCodeProvider())
            {
                StringCompilerModel stringCompilerModel = this._AddModelToCodeTemplate(model);

                // try to compile model and invoke Execute method
                var providerResult = CSharpProvider.CompileAssemblyFromSource(stringCompilerModel.CopilerParams, stringCompilerModel.Template);
                try
                {
                    var modelClassType = providerResult.CompiledAssembly.GetType("ModelClass");
                    var classInstance = Activator.CreateInstance(modelClassType);
                    // Method ModelClass.Execute has two parameters UrlParameters and HttpContext
                    var urlParameters = new UrlParameters
                    {
                        Param = param,
                        Param2 = param2,
                        Url = HttpContext.Current != null ? HttpContext.Current.Request.Url : null,
                        QueryString = HttpContext.Current != null ? HttpContext.Current.Request.QueryString.ToDictionary() : null
                    };
                    var httpContext = HttpContext.Current;
                    // Invoke ModelClass.Execute method with paramaters UrlParameters and HttpContext
                    // Method returns an object that will be parsed as JSON to pass to the view 
                    object output = modelClassType.GetMethod("Execute").Invoke(classInstance, new object[] { urlParameters, httpContext });
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
                            if (errorLine > 6)
                            {
                                errorLine = errorLine - 7;
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

        /// <summary>
        /// If a reference is added in the C# code string template below  
        /// make sure it is also added as a CSharpCodeProvider reference 
        /// as a string[] in StringCompilerModel.CompilerParams
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private StringCompilerModel _AddModelToCodeTemplate(string model)
        {
            var compilerModel = new StringCompilerModel();
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
                sw.WriteLine("using RazorEnginePageModelClasses;");
                sw.WriteLine("public class ModelClass { ");
                sw.WriteLine("public string Execute(UrlParameters _urlParameters, HttpContext _httpContext) { ");
                sw.WriteLine(model);
                sw.WriteLine("return JsonConvert.SerializeObject(Model);");
                sw.WriteLine("} ");
                sw.WriteLine("}");
                compilerModel.Template = sw.ToString();
            } // end using StringWriter
            
            // define parameters for CSharpCodeProvider
            var paramz = new CompilerParameters()
            {
                GenerateInMemory = true,
                GenerateExecutable = false,
                OutputAssembly = string.Format("temp-assemly-{0}", Guid.NewGuid().ToString())

            };
            Dictionary<string, Assembly> assemblies = FileHelper.GetAssemblyFiles();
            
            paramz.ReferencedAssemblies.AddRange(new string[] { "System.dll",
                                                        "System.Linq.dll",
                                                        "System.Data.dll",
                                                        "System.Xml.dll",
                                                        "System.Web.dll"});

            string newtonSoft = typeof(Newtonsoft.Json.JsonConvert).Assembly.Location;
            if (!string.IsNullOrEmpty(newtonSoft))      
                paramz.ReferencedAssemblies.Add(newtonSoft);

            string razorEngineCmsPageModel = typeof(UrlParameters).Assembly.Location;
            if (!string.IsNullOrEmpty(razorEngineCmsPageModel))
                paramz.ReferencedAssemblies.Add(razorEngineCmsPageModel);
            
            compilerModel.CopilerParams = paramz;
            return compilerModel;
        }
    }
}