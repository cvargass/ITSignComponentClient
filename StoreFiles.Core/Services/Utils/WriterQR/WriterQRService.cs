using StoreFiles.Core.Entities.AxisPosition;
using StoreFiles.Core.Services.Utils.QRGenerator;

namespace StoreFiles.Core.Services.Utils.WriterQR
{
    public class WriterQRService : IWriterQRService
    {
        private readonly IQRGeneratorService _qrGeneratorService;
        private readonly IUtilsService _utilsService;

        public WriterQRService(IQRGeneratorService qrGeneratorService,
            IUtilsService utilsService)
        {
            _qrGeneratorService = qrGeneratorService;
            _utilsService = utilsService;
        }

        public byte[] WriteQR(byte[] bytesPdf, string textQR, AxisPosition position)
        {
            byte[] bytesQR = _qrGeneratorService.GenerateQR(textQR);

            byte[] bytesPdfWithQR = _utilsService.WriteImageInPDFCordinates(bytesPdf, bytesQR, 60, 60, position);

            return bytesPdfWithQR;

        }

        public byte[] WriteQRWithCoordinates(byte[] bytesPdf, string textQR, AxisPosition position)
        {
            byte[] bytesQR = _qrGeneratorService.GenerateQR(textQR);

            byte[] bytesPdfWithQR = _utilsService.WriteImageInPDFCordinates(bytesPdf, bytesQR, 80, 80, position);

            return bytesPdfWithQR;

        }
    }
}
