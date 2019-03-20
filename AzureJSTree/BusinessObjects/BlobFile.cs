using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AzureJSTree.BusinessObjects {
    public class BlobFile {
        public string Container { get; set; }
        public string Directory { get; set; }
        public string DownloadURL { get; set; }
        public string sasToken { get; set; }

        public string FileName {
            get {
                var uriParts = DownloadURL.Split('/');
                return uriParts.Last();
            }
        }

        public string FileNameNoExt {
            get {
                return FileName.Split('.')[0];
            }
        }
    }
}
