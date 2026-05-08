namespace ClientSignerApp.Services.SignatureGrafic
{
    public interface ISignatureGraficService
    {
        byte[] GenerateComposedGrafic(byte[] rubricaBytes, byte[] qrBytes, string signer, string reasonSignature, string locationSignature);
    }
}