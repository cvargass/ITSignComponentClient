namespace ClientSignerApp.Services.Signer
{
    public interface ISignerService
    {
        byte[] SignWithSmartCard(byte[] file, string guidFile);
        byte[] SignFile(byte[] file, string guidFile, string dataPosition);
    }
}