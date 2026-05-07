using StoreFiles.Core.DTOs.Grafic;

namespace StoreFiles.Core.Services.GraficSigning
{
    public interface IGraficSigningService
    {
        string StoreGrafic(PostGraficDto postGraficDto);
        (bool flag, byte[] bytesFile) GetGrafic(string guidFile);
    }
}