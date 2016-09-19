using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
public class ModelClass { 
public string Execute() { 
Model = new { test = "object" };
 return JsonConvert.SerializeObject(Model);
} 
}
