using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RazorEngineCms.Models
{
    public class Page
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Variable { get; set; }

        public string Model { get; set; }

        public string Content { get; set; }
    }
}