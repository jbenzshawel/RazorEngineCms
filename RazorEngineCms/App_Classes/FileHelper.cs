using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace RazorEngineCms.App_Classes
{
    public class File
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Variable { get; set; }

        public string Path { get; set; }

        public override string ToString()
        {
            if (System.IO.File.Exists(this.Path))
            {
               var compiledTemplate = System.IO.File.ReadAllText(this.Path);
                if (!string.IsNullOrEmpty(compiledTemplate))
                {
                    return compiledTemplate;
                }
            }

            return string.Empty;
        }
    }

    internal class FileHelper
    {
        public IList<File> Files { get; set; }

        public string TemplatesPath { get; set; }

        public FileHelper()
        {
            if (HttpContext.Current != null)
            {
                this.TemplatesPath = HttpContext.Current.Server.MapPath("~") + "/Views/CompiledTemplates";
            }
            this.Files = GetFiles();
        }

        public File GetFile(string name, string variable)
        {
            return this.Files.FirstOrDefault(f => string.Equals(f.Name, name, StringComparison.InvariantCultureIgnoreCase) &&
                                                                    (string.Equals(f.Variable, variable, StringComparison.InvariantCultureIgnoreCase) ||
                                                                        ((f.Variable == "_" || f.Variable == "") && variable == "")));
                                                               
        }

        public IList<File> GetFiles()
        {
            if (this.Files != null && this.Files.Count > 0)
            {
                return this.Files;
            }
            // since this is called by the constructor there is a chance this.Files has not been initialized 
            this.Files = new List<File>(); 

            if (Directory.Exists(this.TemplatesPath))
            {
                var filePathList = Directory.GetFiles(this.TemplatesPath);
                
                foreach(var filePath in filePathList)
                {
                    var fileName  = filePath.Replace(this.TemplatesPath, "");

                    var nameSplit = fileName.Replace(".html", "").Split('-');
                    if (nameSplit.Length > 2)
                    {
                        this.Files.Add(new File
                        {
                            Path = filePath,
                            Name = nameSplit[0].Replace("\\", ""),
                            Variable = nameSplit[1].Replace("\\", ""),
                            Id = nameSplit[3]
                        });
                    }
                }
            }

            return this.Files;
        }
    }
}