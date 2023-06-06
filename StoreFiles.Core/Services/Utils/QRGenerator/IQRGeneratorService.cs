namespace StoreFiles.Core.Services.Utils.QRGenerator
{
    public interface IQRGeneratorService
    {
        byte[] GenerateQR(string text);
    }
}