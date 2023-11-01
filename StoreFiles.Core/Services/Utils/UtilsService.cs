using iText.IO.Image;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Layout;
using StoreFiles.Core.Entities.AxisPosition;
using StoreFiles.Core.Entities.TextChunkInfo;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

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
                iText.Layout.Element.Image image = new iText.Layout.Element.Image(imageData);
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

        public List<TextChunkInfo> FindCoordinatesText(List<TextChunkInfo> matrizCharacters, string textToFind)
        {
            int positionToFind = 0;
            int positionsMoved = 0;
            var arrCharTextToFind = textToFind.ToCharArray();
            List<TextChunkInfo> arrCharTextFounded = new();
            List<TextChunkInfo> matrizCharTextFound = new();

            //Me devuelve las posiciones en las coincidencias del primer caracter en toda la matriz
            IEnumerable<int> arrPositionsFirstLetter = matrizCharacters.Where(x => x.Text.Equals(arrCharTextToFind[positionToFind].ToString())).Select(x => matrizCharacters.IndexOf(x)).ToArray();


            //Recorre las coincidencias de la primera letra encontrada
            foreach (var index in arrPositionsFirstLetter)
            {
                positionToFind = 0;
                positionsMoved = 0;

                if (arrCharTextFounded.Count.Equals(arrCharTextToFind.Length))
                    break;

                arrCharTextFounded.Add(matrizCharacters[index]);

                //Recorre las posiciones del texto a buscar
                while (positionToFind < arrCharTextToFind.Length - 1)
                {
                    positionToFind++;
                    positionsMoved++;

                    string nextCharToFind = arrCharTextToFind[positionToFind].ToString();

                    var nextLetterMatriz = matrizCharacters[index + positionsMoved];

                    if (nextLetterMatriz.Text.Equals(nextCharToFind))
                    {
                        arrCharTextFounded.Add(nextLetterMatriz);
                    }
                    else
                    {
                        arrCharTextFounded.Clear();
                        break;
                    }
                }
            }

            return arrCharTextFounded;
        }

        public (bool exists, int? numberPage) TextExistInFile(byte[] file, string textToFind)
        {
            // Return if text exists and the number page
            bool exists = false;
            int? numberPage = null;

            using Stream pdfStream = new MemoryStream(file);
            using PdfDocument pdfDocument = new PdfDocument(new PdfReader(pdfStream));

            ITextExtractionStrategy strategy = new SimpleTextExtractionStrategy();
            StringBuilder buffBasic = new StringBuilder();

            for (int i = 1; i <= pdfDocument.GetNumberOfPages(); i++)
            {
                PdfPage page = pdfDocument.GetPage(i);
                String currentPageText = PdfTextExtractor.GetTextFromPage(page, strategy);
                bool textFound = currentPageText.Contains(textToFind);
                exists = textFound;

                if (exists)
                {
                    numberPage = i;
                    break;
                }
            }

            return (exists, numberPage);
        }
    }
}
