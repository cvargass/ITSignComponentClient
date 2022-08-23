using ITSignerWebComponent.SignApp.Responses.APIITSignResponse;
using System.Threading.Tasks;

namespace ITSignerWebComponent.Core.Interfaces.Services
{
    public interface ILicenseService
    {
        Task<ActivateLicenseResponse> ActivateLicense(string pinLicense);
        Task<(bool flag, string message)> isValidLicense(string license);
        bool LoadLicense();
        bool isValidConfigurationLicense(string license);
        void GenerateFileLicense(string license);
    }
}