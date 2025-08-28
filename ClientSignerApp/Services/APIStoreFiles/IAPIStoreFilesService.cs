
namespace ClientSignerApp.Services.APIStoreFiles
{
    public interface IAPIStoreFilesService
    {
        Task<(bool, string)> GetPendingFile(string guidFile);
    }
}