using StoreFiles.Core.DTOs.FileForSigning;

namespace StoreFiles.Core.Services.Cades
{
    public interface ICadesService
    {
        byte[] SignCadesFile(FileForSigningDto fileForSigningDto);
    }
}