using System;
using System.Linq;
using RazorEngineCms.DAL;
using RazorEngineCms.App_Classes;
using RazorEngineCms.ExtensionClasses;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Data.SqlClient;

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

        public DateTime? Updated { get; set; }

        public static Include GetInclude(string name, string type)
        {
            Include include = null;
            using (var dataHelper = new DataHelper())
            {
                var paramz = new List<SqlParameter>
                {
                    new SqlParameter
                    {
                        ParameterName = "@Name",
                        Value = name,
                        DbType = System.Data.DbType.String
                    },
                    new SqlParameter
                    {
                        ParameterName = "@Type",
                        Value = type,
                        DbType = System.Data.DbType.String
                    }
                };
                var includeDataTable = dataHelper.GetData("GetInclude", paramz);
                if (includeDataTable.Rows.Count > 0)
                {
                    include = includeDataTable.Rows[0].ToInclude();
                }
            }

            return include;
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