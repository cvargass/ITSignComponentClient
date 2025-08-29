using Net.Codecrete.QrCodeGenerator;

namespace ClientSignerApp.Services.QRGenerator
{
    public class QRGeneratorService : IQRGeneratorService
    {
        public byte[] GenerateQR(string text)
        {
            var qr = QrCode.EncodeText(text, QrCode.Ecc.Medium);
            var pngAsBytes = qr.ToBmpBitmap(10, 3);

            return pngAsBytes;
        }
    }
}
