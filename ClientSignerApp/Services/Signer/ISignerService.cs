namespace ClientSignerApp.Services.Signer
{
    public interface ISignerService
    {
        byte[] SignWithSmartCard(byte[] file, string guidFile, bool signatureVisible = true, int signaturePosition = 6);
    }
}