using iText.IO.Image;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using StoreFiles.Core.Entities.AxisPosition;
using System.IO;

namespace StoreFiles.Core.Services.Utils
{
    public class UtilsService : IUtilsService
    {
        public byte[] WriteImageInPDFCordinates(byte[] bytesDocPdf, byte[] bytesImage, int width, int height, AxisPosition position)
        {
            byte[] bytes = null;
            using Stream pdfStream = new MemoryStream(bytesDocPdf);

            using (MemoryStream outputPdfStream = new MemoryStream())
            {
                using PdfDocument pdfDocument = new PdfDocument(new PdfReader(pdfStream), new PdfWriter(outputPdfStream));
                using Document document = new Document(pdfDocument);

                ImageData imageData = ImageDataFactory.Create(bytesImage);
                Image image = new Image(imageData);
                image.ScaleAbsolute(width, height);
                image.SetFixedPosition(position.CoordinateX, position.CoordinateY);
                image.SetPageNumber(position.NumberPage);

                document.Add(image);

                document.Close();
                bytes = outputPdfStream.ToArray();
                outputPdfStream.Close();
            }

            return bytes;
        }
    }
}
