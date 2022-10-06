using StoreFiles.API.DTOs.PostFile;
using StoreFiles.API.DTOs.PostFileSigned;
using System.Threading.Tasks;

namespace ITSignerWebComponent.SignApp.APIStoreFiles
{
    public interface IAPIStoreFilesService
    {
        Task<(bool, string)> GetPendingFile(string guidFile);
        Task<(bool, string[])> GetPendingFiles(int idUser, int idApp);
        Task<(bool, string)> GetSignedFile(string guidFile);
        Task<(bool, string)> PostFile(PostFileDto postFileDto);
        Task<bool> PostSignedFile(PostFileSignedDto postFileSignedDto);
    }
}