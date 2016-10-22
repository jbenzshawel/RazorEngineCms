using System;
using System.Linq;
using RazorEngineCms.DAL;
using RazorEngineCms.App_Classes;
using RazorEngineCms.ExtensionClasses;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace RazorEngineCms.Models
{
    public class Include
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Content { get; set; }

        [Required]
        public string Type { get; set; }

        public DateTime Updated { get; set; }

        public static Include GetInclude(string name, string type)
        {
            ApplicationContext db = new ApplicationContext();
            var Include = db.Include.FirstOrDefault(i => string.Equals(i.Name, name, StringComparison.CurrentCultureIgnoreCase) &&
                                                         string.Equals(i.Type, type, StringComparison.CurrentCultureIgnoreCase));
            return Include;
        }

        public static string GetContent(string name, string type)
        {
            string content = null;
            var include = GetInclude(name, type);

            if (include != null)
            {
                content = include.Content;
            } else
            {
                throw new IncludeNotFoundException("Include could not be found.", name, type);
            }

            return content;
        }

        public string ToJson(bool withPadding = false)
        {
            return this.ToJson<Include>(withPadding);
        }
    }
}