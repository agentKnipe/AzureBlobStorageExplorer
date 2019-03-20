using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AzureJSTree.BusinessObjects {
    public class JsTreeFolder {
        public string id { get; set; }
        public string parent { get; set; }
        public string text { get; set; }
        public string state { get { return "closed"; } }
        public bool children { get { return true; } }
    }
}