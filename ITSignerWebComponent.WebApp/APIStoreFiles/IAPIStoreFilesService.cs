using ITSignerWebComponent.SignApp.Responses.APIStoreResponses;
using StoreFiles.Core.DTOs.FileForSigning;
using StoreFiles.Core.DTOs.PostFile;
using StoreFiles.Core.DTOs.PostFileSigned;
using System.Threading.Tasks;

namespace ITSignerWebComponent.SignApp.APIStoreFiles
{
    public interface IAPIStoreFilesService
    {
        Task<(bool, string)> GetPendingFile(string guidFile, string typeFile);
        Task<(bool, string[])> GetPendingFiles(int idUser, int idApp, string typeFile);
        Task<(bool, string)> GetSignedFile(string guidFile, string typeFile);
        Task<(bool, string)> PostFile(PostFileDto postFileDto, string typeFile);
        Task<bool> PostSignedFile(PostFileSignedDto postFileSignedDto, string typeFile);
        Task<FileForSigningResponse> SignFile(FileForSigningDto fileForSigningDto, string typeFile);
    }
}