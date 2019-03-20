using AzureJSTree.BusinessObjects;
using AzureJSTree.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AzureJSTree.Services {
    public class AzureBlobStorageService : IStorageService {
        private readonly IConfiguration _Config;

        private CloudStorageAccount _CSA;
        private CloudBlobClient _CBC;

        public AzureBlobStorageService(IConfiguration config) {
            _Config = config;

            var storageConnString = _Config.GetValue<string>("AzureStorageConfig:StorageConnectionString");

            _CSA = CloudStorageAccount.Parse(storageConnString);
            _CBC = _CSA.CreateCloudBlobClient();
        }

        public async Task<string> Upload(string Container, string Directory, string FileName, byte[] FileContents) {
            var cloudBlobContainer = _CBC.GetContainerReference(Container);
            await cloudBlobContainer.CreateIfNotExistsAsync();

            var blobFileFullName = FileName;
            if (!string.IsNullOrEmpty(Directory)) {
                blobFileFullName = $"{Directory}/{FileName}";
            }

            var blob = cloudBlobContainer.GetBlockBlobReference(blobFileFullName);
            if (!await blob.ExistsAsync()) {
                await blob.UploadFromByteArrayAsync(FileContents, 0, FileContents.Length);
            }

            return blob.Uri.ToString();
        }

        public Task Delete(string Container, string Directory, string fileName) {
            throw new NotImplementedException();
        }

        public List<string> ContainerList() {
            var returnList = new List<string>();
            var containers = _CBC.ListContainers();

            foreach (var container in containers) {
                returnList.Add(container.Name);
            }

            return returnList;
        }

        public async Task<List<BlobDirectory>> DirectoryListAsync(string Container) {
            BlobContinuationToken blobContinuationToken = null;
            var returnList = new List<BlobDirectory>();
            var blobContainer = _CBC.GetContainerReference(Container);

            do {
                var directories = await blobContainer.ListBlobsSegmentedAsync(null, blobContinuationToken);

                foreach (var directory in directories.Results.Where(w => w as CloudBlobDirectory != null).ToList()) {
                    var uriParts = directory.Uri.ToString().Split('/');
                    var newDirectory = new BlobDirectory() { DirectoryURI = directory.Uri.ToString(), ContainerName = directory.Container.Name };

                    returnList.Add(newDirectory);
                }
            }
            while (blobContinuationToken != null);

            return returnList;
        }

        public List<BlobDirectory> DirectoryList(string Container) {
            BlobContinuationToken blobContinuationToken = null;
            var returnList = new List<BlobDirectory>();
            var blobContainer = _CBC.GetContainerReference(Container);

            do {
                var directories = blobContainer.ListBlobsSegmented(null, blobContinuationToken);

                foreach (var directory in directories.Results.Where(w => w as CloudBlobDirectory != null).ToList()) {
                    var uriParts = directory.Uri.ToString().Split('/');
                    var newDirectory = new BlobDirectory() { DirectoryURI = directory.Uri.ToString(), ContainerName = directory.Container.Name };

                    returnList.Add(newDirectory);
                }
            }
            while (blobContinuationToken != null);

            return returnList;
        }

        public async Task<List<BlobDirectory>> DirectoryListAsync(string Container, string Directory) {
            BlobContinuationToken blobContinuationToken = null;
            var returnList = new List<BlobDirectory>();
            var blobContainer = _CBC.GetContainerReference(Container);
            var subDirectory = blobContainer.GetDirectoryReference(Directory);

            do {
                var directories = await subDirectory.ListBlobsSegmentedAsync(blobContinuationToken);

                foreach (var directory in directories.Results.Where(w => w as CloudBlobDirectory != null).ToList()) {
                    var uriParts = directory.Uri.ToString().Split('/');
                    var newDirectory = new BlobDirectory() { DirectoryURI = directory.Uri.ToString(), DirectoryParents = Directory, ContainerName = directory.Container.Name };

                    returnList.Add(newDirectory);
                }
            }
            while (blobContinuationToken != null);

            return returnList;
        }

        public List<BlobDirectory> DirectoryList(string Container, string Directory) {
            BlobContinuationToken blobContinuationToken = null;
            var returnList = new List<BlobDirectory>();
            var blobContainer = _CBC.GetContainerReference(Container);
            var subDirectory = blobContainer.GetDirectoryReference(Directory);

            do {
                var directories = subDirectory.ListBlobsSegmented(blobContinuationToken);

                foreach (var directory in directories.Results.Where(w => w as CloudBlobDirectory != null).ToList()) {
                    var uriParts = directory.Uri.ToString().Split('/');
                    var newDirectory = new BlobDirectory() { DirectoryURI = directory.Uri.ToString(), DirectoryParents = Directory, ContainerName = directory.Container.Name };

                    returnList.Add(newDirectory);
                }
            }
            while (blobContinuationToken != null);

            return returnList;
        }

        public async Task<List<BlobFile>> FileListAsync(string Container, string Directory) {
            BlobContinuationToken blobContinuousToken = null;
            var returnList = new List<BlobFile>();

            var cloudBlobContainer = _CBC.GetContainerReference(Container);
            var directory = cloudBlobContainer.GetDirectoryReference(Directory);

            var sasToken = GenerateSharedAccessSignature(cloudBlobContainer);

            do {
                var results = await directory.ListBlobsSegmentedAsync(blobContinuousToken);
                blobContinuousToken = results.ContinuationToken;

                foreach (var blob in results.Results.Where(w => w as CloudBlobDirectory == null).ToList()) {
                    var newBlob = new BlobFile();
                    newBlob.Container = Container;
                    newBlob.Directory = Directory;
                    newBlob.DownloadURL = blob.Uri.ToString();
                    newBlob.sasToken = sasToken;

                    returnList.Add(newBlob);
                }
            }
            while (blobContinuousToken != null);

            return returnList;
        }

        public List<BlobFile> FileList(string Container, string Directory) {
            BlobContinuationToken blobContinuousToken = null;
            var returnList = new List<BlobFile>();

            var cloudBlobContainer = _CBC.GetContainerReference(Container);
            var directory = cloudBlobContainer.GetDirectoryReference(Directory);

            var sasToken = GenerateSharedAccessSignature(cloudBlobContainer);

            do {
                var results = directory.ListBlobsSegmented(blobContinuousToken);
                blobContinuousToken = results.ContinuationToken;

                foreach (var blob in results.Results.Where(w => w as CloudBlobDirectory == null).ToList()) {
                    var newBlob = new BlobFile();
                    newBlob.Container = Container;
                    newBlob.Directory = Directory;
                    newBlob.DownloadURL = blob.Uri.ToString();
                    newBlob.sasToken = sasToken;

                    returnList.Add(newBlob);
                }
            }
            while (blobContinuousToken != null);

            return returnList;
        }

        private string GenerateSharedAccessSignature(CloudBlobContainer Container) {
            var sasConstraint = new SharedAccessBlobPolicy();
            sasConstraint.SharedAccessExpiryTime = DateTimeOffset.UtcNow.AddMinutes(5);
            sasConstraint.Permissions = SharedAccessBlobPermissions.Read;

            var sasToken = Container.GetSharedAccessSignature(sasConstraint);

            return sasToken;
        }
    }
}
