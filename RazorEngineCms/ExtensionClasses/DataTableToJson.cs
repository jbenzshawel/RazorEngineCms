using System.Collections.Generic;
using System.Data;
using Newtonsoft.Json;

namespace RazorEngineCms.ExtensionClasses
{
    public class JsonResultObject
    {
        public IList<object> Data { get; set; }
        public bool Status { get; set; }

        public IList<string> Errors { get; set; }

        public bool HasRows { get; set; }

        public JsonResultObject()
        {
            this.Data = new List<object>();
            this.Status = true;
            this.Errors = new List<string>();
        }

    }
    public static class DataTableExtensions 
    {
        /// <summary>
        /// Returns a Json Serialized JsonResultObject with DataTable
        /// Rows in the Data property. Other properties in returned object
        /// include Status, HasRows, and Errors. 
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static string ToJson(this DataTable @this)
        {
            DataTable dataTable = @this;
            var resultObject = new JsonResultObject();
            if (dataTable.Rows.Count > 0)
            {
                resultObject.HasRows = true;
                foreach (DataRow row in dataTable.Rows)
                {
                    resultObject.Data.Add(row);
                }
            } else
            {
                resultObject.HasRows = false;
                resultObject.Errors.Add("DataTable is empty");
            }
            return JsonConvert.SerializeObject(resultObject);
        }
    }
}