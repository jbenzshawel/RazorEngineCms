using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Linq;
using Newtonsoft.Json;
using RazorEngineCms.Models;

namespace RazorEngineCms.ExtensionClasses
{
    public static class ExtensionsClasses
    {
        /// <summary>
        /// NameValueCollection.ToDictionary 
        /// </summary>
        /// <param name="collection"></param>
        /// <returns></returns>
        public static IDictionary<string, string> ToDictionary(this NameValueCollection @this)
        {
            return @this.Cast<string>().ToDictionary(k => k, v => @this[v]);
        }

        /// <summary>
        /// http://stackoverflow.com/questions/3928822/comparing-2-dictionarystring-string-instances
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public static bool Equal<TKey, TValue>(this IDictionary<TKey, TValue> first, IDictionary<TKey, TValue> second)
        {
            return first.Equal(second, null);
        }

        /// <summary>
        /// http://stackoverflow.com/questions/3928822/comparing-2-dictionarystring-string-instances
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <param name="valueComparer"></param>
        /// <returns></returns>
        public static bool Equal<TKey, TValue>(this IDictionary<TKey, TValue> first, IDictionary<TKey, TValue> second, IEqualityComparer<TValue> valueComparer)
        {
            if (first == second) return true;
            if ((first == null) || (second == null)) return false;
            if (first.Count != second.Count) return false;

            valueComparer = valueComparer ?? EqualityComparer<TValue>.Default;

            foreach (var kvp in first)
            {
                TValue secondValue;
                if (!second.TryGetValue(kvp.Key, out secondValue)) return false;
                if (!valueComparer.Equals(kvp.Value, secondValue)) return false;
            }
            return true;
        }

        /// <summary>
        /// Cast a PageCacheModel to a Page type
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static Page ToPage(this PageCacheModel @this)
        {
            var page = new Page
            {
                Name = @this.Name,
                Section = @this.Section,
                CompiledModel = @this.CompiledModel,
                CompiledTemplate = @this.CompiledTemplate,
                Model = @this.Model,
                Template = @this.Template,
                HasParams = @this.HasParams
            };
            return page;
        }

        /// <summary>
        /// ConcurrentBag.AddRange
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <param name="toAdd"></param>
        public static void AddRange<T>(this ConcurrentBag<T> @this, IEnumerable<T> toAdd)
        {
            foreach (var element in toAdd)
            {
                @this.Add(element);
            }
        }

        /// <summary>
        /// Returns a Json Serialized object of this as a string
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <param name="withPadding"></param>
        /// <returns></returns>
        public static string ToJson<T>(this T @this, bool withPadding = false)
        {
            if (withPadding)
            {
                return JsonConvert.SerializeObject(@this, Formatting.Indented);

            }
            return JsonConvert.SerializeObject(@this);
        }

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
            }
            else
            {
                resultObject.HasRows = false;
                resultObject.Errors.Add("DataTable is empty");
            }
            return JsonConvert.SerializeObject(resultObject);
        }
    }

    /// <summary>
    /// Model used for Serializing DataTable Json
    /// </summary>
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
}