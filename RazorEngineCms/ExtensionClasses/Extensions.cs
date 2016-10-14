using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Linq;
using Newtonsoft.Json;

namespace RazorEngineCms.ExtensionClasses
{
    public static class Extensions
    {
        /// <summary>
        /// NameValueCollection.ToDictionary 
        /// </summary>
        /// <param name="collection"></param>
        /// <returns></returns>
        public static IDictionary<string, string> ToDictionary(this NameValueCollection collection)
        {
            return collection.Cast<string>().ToDictionary(k => k, v => collection[v]);
        }

        public static bool DictionaryEqual<TKey, TValue>(this IDictionary<TKey, TValue> first, IDictionary<TKey, TValue> second)
        {
            return first.DictionaryEqual(second, null);
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
        public static bool DictionaryEqual<TKey, TValue>(this IDictionary<TKey, TValue> first, IDictionary<TKey, TValue> second, IEqualityComparer<TValue> valueComparer)
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