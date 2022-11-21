using StoreFiles.Core.DTOs.PostFile;
using StoreFiles.Core.DTOs.PostFileSigned;
using StoreFiles.Core.QueryFilters;

namespace StoreFiles.Core.Services.StoreFiles
{
    public interface IStoreFileService
    {
        string StoreFile(PostFileDto postFileDto);
        string[] GetPendingFiles(PendingFileQueryFilter pendingFileQueryFilter);
        (bool flag, byte[] bytesFile) GetFile(string guidFile, bool isSigned = false);
        void StoreFileSigned(PostFileSignedDto postFileSignedDto);
    }
}