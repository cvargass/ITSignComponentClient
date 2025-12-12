using ClientSignerApp.DTOs.Tsa;

namespace ClientSignerApp.Services.Signer
{
    public interface ISignerService
    {
        byte[] SignWithSmartCard(byte[] file, string guidFile, TsaDataDto? tsaParams);
        byte[] SignFile(byte[] file, string guidFile, string dataPosition, TsaDataDto? tsaParams);
    }
}