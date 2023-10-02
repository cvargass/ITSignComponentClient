using StoreFiles.Core.DTOs.PostFile;
using StoreFiles.Core.DTOs.PostFileSigned;
using StoreFiles.Core.QueryFilters;

namespace StoreFiles.Core.Services.StoreFiles
{
    public interface IStoreFileService
    {
        string StoreFile(PostFileDto postFileDto, string typeFile);
        string[] GetPendingFiles(PendingFileQueryFilter pendingFileQueryFilter, string typeFile);
        (bool flag, byte[] bytesFile) GetFile(string guidFile, string typeFile, bool isSigned = false);
        void StoreFileSigned(PostFileSignedDto postFileSignedDto, string typeFile);
        void StoreFileSignedAPI(byte[] pdfSignedBase64);
    }
}