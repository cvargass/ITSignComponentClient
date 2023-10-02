using ITSignerWebComponent.Core.DTOs.License;
using ITSignerWebComponent.Core.DTOs.Signer.EmbedSignature;
using ITSignerWebComponent.Core.DTOs.Signer.PreSigned;
using System.Threading.Tasks;

namespace ITSignerWebComponent.SignApp.Data
{
    public interface IComponentManagerService
    {
        Task<(bool flagActivatedLicense, string message)> ActivateLicense(LicenseDto licenseDto);
        (string message, string fileSigned) EmbedSignature(EmbedSignatureDto embedSignatureDto);
        (string bs64DataToSign, string bs64PdfPrepared) GenerateDataToSign(PreSignedDto preSignedDto, bool changeFieldName = false);
        bool LoadLicense();
    }
}