using System;
using System.Collections.Generic;

namespace RazorEnginePageModelClasses
{
    public class UrlParameters
    {
        public string Param { get; set; }

        public string Param2 { get; set; }

        public IDictionary<string,string> QueryString { get; set; }

        public Uri Url { get; set; }
    }
}
