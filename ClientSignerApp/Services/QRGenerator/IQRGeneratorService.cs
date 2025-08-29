namespace ClientSignerApp.Services.QRGenerator
{
    public interface IQRGeneratorService
    {
        byte[] GenerateQR(string text);
    }
}