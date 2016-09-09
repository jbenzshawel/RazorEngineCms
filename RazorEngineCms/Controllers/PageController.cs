using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RazorEngine;
using RazorEngine.Templating;
using RazorEngineCms.DAL;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using Newtonsoft.Json;

namespace RazorEngineCms.Controllers
{
    public class PageController : Controller
    {
        public object Model { get; set; }

        private ApplicationContext _db { get; set; }

        public PageController()
        {
            this._db = new ApplicationContext(); 
        }

        // GET: Page
        public ActionResult New()
        {
            return View();
        }

        public ActionResult Page(string name, string variable)
        {
            var page = this._db.Page
                            .FirstOrDefault(p => string.Equals(p.Name, name, StringComparison.CurrentCultureIgnoreCase) &&
                                                 string.Equals(p.Variable, variable, StringComparison.CurrentCultureIgnoreCase));
            if (page != null)
            {
                this.Model = this.CompilePageModel(page.Model);

                var cacheName = string.Format("{0}-{1}-key", name, variable);

                // null for modelType parameter since templates are dynamic 
                var model = new {
                    Content = Engine.Razor.RunCompile(page.Content, cacheName, null, this.Model)
                };
                return View(model);
            }
            
            // To Do: redirect to 404 
            // return empty view if can't find page
            return View(new { Content = string.Empty });
        }

        internal object CompilePageModel(string model)
        {
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
                return JsonConvert.DeserializeObject(output.ToString());
            }
        }
    }
}