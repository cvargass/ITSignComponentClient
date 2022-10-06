using StoreFiles.API.DTOs.PostFile;
using StoreFiles.API.DTOs.PostFileSigned;
using StoreFiles.API.QueryFilters;
using System;

namespace StoreFiles.API.Services
{
    public interface IStoreFileService
    {
        string StoreFile(PostFileDto postFileDto);
        string[] GetPendingFiles(PendingFileQueryFilter pendingFileQueryFilter);
        (bool flag, byte[] bytesFile) GetFile(string guidFile, bool isSigned = false);
        void StoreFileSigned(PostFileSignedDto postFileSignedDto);
    }
}