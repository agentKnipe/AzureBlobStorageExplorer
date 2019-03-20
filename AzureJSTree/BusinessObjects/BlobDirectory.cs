using AzureJSTree.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AzureJSTree.BusinessObjects {
    public class BlobDirectory {
        public string DirectoryURI { get; set; }

        public string ContainerName { get; set; }

        public string DirectoryParents { get; set; }

        public string DirectoryName {
            get {
                var uriParts = DirectoryURI.Split('/');

                /* return the last portion of the directory URI as the DirectoryName */
                return uriParts[uriParts.Count() - 2];
            }
        }
    }
}
