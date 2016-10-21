using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RazorEngineCms.Models;

namespace RazorEngineCms.App_Classes
{
    using System;

    public class IncludeNotFoundException : Exception
    {
        public string Name { get; set; }

        public string Type { get; set; }

        public IncludeNotFoundException()
        {
        }

        public IncludeNotFoundException(string message, string name, string type)
            : base(message)
        {
            this.Name = name;
            this.Type = type;
        }

        public IncludeNotFoundException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}