using AzureJSTree.BusinessObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AzureJSTree.Interfaces {
    public interface IStorageService {
        Task<string> Upload(string Container, string Directory, string FileName, byte[] FileContents);

        Task Delete(string Container, string Directory, string fileName);

        List<string> ContainerList();

        Task<List<BlobDirectory>> DirectoryListAsync(string Container);
        List<BlobDirectory> DirectoryList(string Container);

        Task<List<BlobDirectory>> DirectoryListAsync(string Container, string Directory);
        List<BlobDirectory> DirectoryList(string Container, string Directory);

        Task<List<BlobFile>> FileListAsync(string Container, string Directory);
        List<BlobFile> FileList(string Container, string Directory);
    }
}
