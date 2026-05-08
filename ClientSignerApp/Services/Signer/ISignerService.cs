using ClientSignerApp.DTOs.Tsa;
using System.Windows.Forms.VisualStyles;

namespace ClientSignerApp.Services.Signer
{
    public interface ISignerService
    {
        byte[] SignWithSmartCard(byte[] file, string guidFile, TsaDataDto? tsaParams);
        byte[] SignFile(byte[] file, string guidFile, string dataPosition, TsaDataDto? tsaParams, int? signingType = null, byte[] grafic = null);
    }
}