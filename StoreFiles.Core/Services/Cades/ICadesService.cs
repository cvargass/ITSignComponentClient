using StoreFiles.Core.DTOs.Cades;

namespace StoreFiles.Core.Services.Cades
{
    public interface ICadesService
    {
        byte[] SignCadesFile(CadesFileDto cadesFileDto);
    }
}