using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AzureJSTree.BusinessObjects {
    public class JsTreeFile {
        public string id { get; set; }
        public string parent { get; set; }
        public string text { get; set; }
        public string uri { get; set; }
        public string cssClass { get; set; }
    }
}
