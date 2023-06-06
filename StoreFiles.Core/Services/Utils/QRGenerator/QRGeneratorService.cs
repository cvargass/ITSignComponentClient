using QRCoder;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace StoreFiles.Core.Services.Utils.QRGenerator
{
    public class QRGeneratorService : IQRGeneratorService
    {
        public byte[] GenerateQR(string text)
        {
            using QRCodeGenerator qrGenerator = new QRCodeGenerator();

            using QRCodeData qrCodeInfo = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);
            using QRCode qrCode = new QRCode(qrCodeInfo);
            using Bitmap qrBitmap = qrCode.GetGraphic(60);
            byte[] bitmapArray = GenerateBitmapToByteArray(qrBitmap);

            return bitmapArray;
        }

        private byte[] GenerateBitmapToByteArray(Bitmap qrBitmap)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                qrBitmap.Save(ms, ImageFormat.Png);
                return ms.ToArray();
            }
        }
    }
}
