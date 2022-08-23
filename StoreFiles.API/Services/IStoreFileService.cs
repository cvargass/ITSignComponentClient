using StoreFiles.API.DTOs.PostFile;
using StoreFiles.API.DTOs.PostFileSigned;
using System;

namespace StoreFiles.API.Services
{
    public interface IStoreFileService
    {
        string StoreFile(PostFileDto postFileDto);
        string[] GetPendingFiles();
        (bool flag, byte[] bytesFile) GetFile(string guidFile, bool isSigned = false);
        void StoreFileSigned(PostFileSignedDto postFileSignedDto);
    }
}