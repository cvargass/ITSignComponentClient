using StoreFiles.Core.Entities.AxisPosition;
using StoreFiles.Core.Entities.TextChunkInfo;
using System.Collections.Generic;

namespace StoreFiles.Core.Services.Utils
{
    public interface IUtilsService
    {
        byte[] WriteImageInPDFCordinates(byte[] bytesDocPdf, byte[] bytesImage, int width, int height, AxisPosition position);
        List<TextChunkInfo> FindCoordinatesText(List<TextChunkInfo> matrizCharacters, string textToFind);
        (bool exists, int? numberPage) TextExistInFile(byte[] file, string textToFind);
    }
}