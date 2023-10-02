using StoreFiles.Core.DTOs.FileForSigning;

namespace StoreFiles.Core.Services.Xades
{
    public interface IXadesService
    {
        byte[] SignXadesFile(FileForSigningDto cadesFileDto);
    }
}