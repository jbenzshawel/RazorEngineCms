using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace RazorEngineCms.App_Classes
{
    public class StringCompiler : IDisposable
    {
        public string JsonResult { get; set; }

        public IList<string> Errors { get; set; }

        public bool IsValid { get { return this.Errors.Count == 0; } }

        public StringCompiler()
        {
            this.Errors = new List<string>();
        }

        public void CompilePageModel(string model)
        {
            using (var CSharpProvider = new CSharpCodeProvider())
            {
                var compileModelGuid = Guid.NewGuid().ToString();
                var tempFileName = string.Format("temp-model-file-{0}.cs", compileModelGuid);
                var tempPath = HttpContext.Current.Server.MapPath("~") + @"\tmp\" + tempFileName;
    
                // create a temp file with model for page
                if (!File.Exists(tempPath))
                {
                    using (var stringWriter = File.CreateText(tempPath))
                    {
                        stringWriter.WriteLine("using System;");
                        stringWriter.WriteLine("using System.Collections.Generic;");
                        stringWriter.WriteLine("using System.Linq;");
                        stringWriter.WriteLine("using Newtonsoft.Json;");
                        stringWriter.WriteLine("public class ModelClass { ");
                        stringWriter.WriteLine("public string Execute() { ");
                        stringWriter.WriteLine(model);
                        stringWriter.WriteLine(" return JsonConvert.SerializeObject(Model);");
                        stringWriter.WriteLine("} ");
                        stringWriter.WriteLine("}");
                    }
                }

                var paramz = new CompilerParameters()
                {
                    GenerateInMemory = true,
                    GenerateExecutable = false,
                    OutputAssembly = string.Format("temp-assemly-{0}", compileModelGuid)
                    
                };
                paramz.ReferencedAssemblies.AddRange(new string[] { "System.dll",
                                                                    //"System.Collections.Generic",
                                                                    "System.Linq.dll",
                                                                    @"C:\Git\RazorEngineCms\packages\Newtonsoft.Json.9.0.1\lib\net45\Newtonsoft.Json.dll" });

                var providerResult = CSharpProvider.CompileAssemblyFromFile(paramz, tempPath);

                // try to compile model 
                try
                {
                    var type = providerResult.CompiledAssembly.GetType("ModelClass");
                    var obj = Activator.CreateInstance(type);
                    var output = type.GetMethod("Execute").Invoke(obj, new object[] { });
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
                            this.Errors.Add(string.Format("Compile Error: {0}, Line: {1}", providerResult.Errors[i].ErrorText, errorLine));
                        }
                    }
                    this.Errors.Add(ex.Message);
                } // end catch 
                finally
                {
                    // delete the temp model file when done
                    if (File.Exists(tempPath))
                    {
                        File.Delete(tempPath);
                    }
                } // end finally 

            } // end using CSharpCodeProvider 
            return; // void
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