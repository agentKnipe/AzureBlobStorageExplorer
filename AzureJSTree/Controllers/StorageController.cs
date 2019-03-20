using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureJSTree.BusinessObjects;
using AzureJSTree.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace AzureJSTree.Controllers
{
    [Route("[controller]/[action]")]
    public class StorageController : Controller
    {
        private readonly IStorageService _Storage;

        public StorageController(IStorageService storage) {
            _Storage = storage;
        }

        [HttpPost]
        public JsonResult GetNode([FromBody] Node Node){
            var containers = new List<JsTreeFolder>();

            if (Node.NodeID == "#") {
                var containerList = _Storage.ContainerList();

                for (int i = 0; i < containerList.Count(); i++) {
                    var newJSTree = new JsTreeFolder() {
                        id = containerList[i],
                        text = containerList[i],
                        parent = "#"
                    };

                    containers.Add(newJSTree);
                }
            }
            else {
                var nodeParts = Node.NodeID.Split('/');
                var container = nodeParts[0];
                var directory = string.Join("/", nodeParts.Skip(1).ToArray());
                var directoryList = new List<BlobDirectory>();
                var parent = container;

                if (!string.IsNullOrEmpty(directory)) {
                    parent = $"{container}/{directory}";
                }

                directoryList = _Storage.DirectoryList(container, directory);

                for (int i = 0; i < directoryList.Count(); i++) {
                    var newJSTree = new JsTreeFolder() {
                        id = $"{parent}/{directoryList[i].DirectoryName}",
                        text = directoryList[i].DirectoryName,
                        parent = parent
                    };

                    containers.Add(newJSTree);
                }

            }

            var serializedObjects = JsonConvert.SerializeObject(containers);
            return Json(containers);
        }

        [HttpPost]
        public JsonResult GetNodeContents([FromBody] Node Node) {
            var nodeObjects = new List<JsTreeFile>();

            var nodeParts = Node.NodeID.Split('/');
            var container = nodeParts[0];
            var directory = string.Join("/", nodeParts.Skip(1).ToArray());
            var directoryList = new List<BlobDirectory>();
            var fileList = new List<BlobFile>();
            var parent = container;

            if (!string.IsNullOrEmpty(directory)) {
                parent = $"{container}/{directory}";
            }

            directoryList = _Storage.DirectoryList(container, directory);
            fileList = _Storage.FileList(container, directory).OrderByDescending(o => o.FileName).ToList();

            for (int i = 0; i < directoryList.Count(); i++) {
                var newJSTreeContent = new JsTreeFile() {
                    id = $"{parent}/{directoryList[i].DirectoryName}",
                    text = directoryList[i].DirectoryName,
                    parent = parent,
                    cssClass = "directory"
                };

                nodeObjects.Add(newJSTreeContent);
            }

            for (int i = 0; i < fileList.Count(); i++) {
                var newJSTreeContent = new JsTreeFile() {
                    id = $"{parent}/{fileList[i].FileNameNoExt}",
                    text = fileList[i].FileName,
                    parent = parent,
                    cssClass = "file",
                    uri = fileList[i].DownloadURL + fileList[i].sasToken
                };

                nodeObjects.Add(newJSTreeContent);
            }

            //var serializedObjects = JsonConvert.SerializeObject(nodeObjects);
            return Json(nodeObjects);
        }
    }
}