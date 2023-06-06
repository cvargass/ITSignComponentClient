using StoreFiles.Core.Entities.AxisPosition;

namespace StoreFiles.Core.Services.Utils.WriterQR
{
    public interface IWriterQRService
    {
        byte[] WriteQR(byte[] bytesPdf, string textQR, AxisPosition position);
        byte[] WriteQRWithCoordinates(byte[] bytesPdf, string textQR, AxisPosition position);
    }
}