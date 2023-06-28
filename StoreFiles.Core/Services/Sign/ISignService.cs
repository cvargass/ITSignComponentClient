using StoreFiles.Core.DTOs.Sign;
using System.Security.Cryptography.X509Certificates;

namespace StoreFiles.Core.Services.Sign
{
    public interface ISignService
    {
        byte[] SignFile(SignDto signDto);
        string GetCertificateInformation(SignDto signDto);
    }
}