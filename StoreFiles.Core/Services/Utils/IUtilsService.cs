using StoreFiles.Core.Entities.AxisPosition;

namespace StoreFiles.Core.Services.Utils
{
    public interface IUtilsService
    {
        byte[] WriteImageInPDFCordinates(byte[] bytesDocPdf, byte[] bytesImage, int width, int height, AxisPosition position);
    }
}