using SkiaSharp;

namespace ClientSignerApp.Services.SignatureGrafic
{
    public class SignatureGraficService : ISignatureGraficService
    {
        public byte[] GenerateComposedGrafic(
            byte[] rubricaBytes,
            byte[] qrBytes,
            string signer,
            string reasonSignature,
            string locationSignature)
        
        {
            // =========================
            // TAMAÑO FINAL
            // =========================

            int width = 1280;
            int height = 300;

            using var surface = SKSurface.Create(
                new SKImageInfo(width, height));

            var canvas = surface.Canvas;

            // =========================
            // TARJETA PRINCIPAL
            // =========================

            var cardRect = new SKRoundRect(
                new SKRect(20, 20, width - 20, height - 20),
                25,
                25);

            using var cardPaint = new SKPaint
            {
                Color = SKColors.White,
                IsAntialias = true
            };

            canvas.DrawRoundRect(cardRect, cardPaint);

            // =========================
            // SOMBRA SUAVE
            // =========================

            using var shadowPaint = new SKPaint
            {
                ImageFilter = SKImageFilter.CreateDropShadow(
                    0,
                    2,
                    8,
                    8,
                    new SKColor(0, 0, 0, 40))
            };

            canvas.DrawRoundRect(cardRect, shadowPaint);

            canvas.DrawRoundRect(cardRect, cardPaint);

            // =========================
            // IMÁGENES
            // =========================

            using var rubricaBitmap = SKBitmap.Decode(rubricaBytes);
            using var qrBitmap = SKBitmap.Decode(qrBytes);

            // =========================
            // RÚBRICA
            // =========================

            int rubricaWidth = 420;
            int rubricaHeight = 170;

            var rubricaRect = new SKRect(
                40,
                45,
                40 + rubricaWidth,
                45 + rubricaHeight);

            canvas.DrawBitmap(
                rubricaBitmap,
                rubricaRect);

            // =========================
            // TEXTO
            // =========================
            using var titlePaint = new SKPaint
            {
                Color = new SKColor(70, 70, 70),
                IsAntialias = true
            };
            using var fontTitlePaint = new SKFont { Size = 34, Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold) };

            using var textPaint = new SKPaint
            {
                Color = new SKColor(90, 90, 90),
                IsAntialias = true,
            };
            using var fontTextPaint = new SKFont { Size = 24, Typeface = SKTypeface.FromFamilyName("Arial") };

            float textX = 500;
            float startY = 80;
            float lineHeight = 42;

            // Nombre dividido en dos líneas si es largo
            var nombres = signer.Split(' ');

            string linea1 = string.Join(" ", nombres.Take(2));
            string linea2 = string.Join(" ", nombres.Skip(2));

            //Signer
            //canvas.DrawText(signer, textX, startY, fontTitlePaint, titlePaint);
            canvas.DrawText(linea1.ToUpper(), textX, startY, fontTitlePaint, titlePaint);
            canvas.DrawText(linea2.ToUpper(), textX, startY + lineHeight, fontTitlePaint, titlePaint);

            //Fecha
            canvas.DrawText(DateTime.Now.ToString(), textX, startY + (lineHeight * 1.9f), fontTextPaint, textPaint);

            //Reason
            canvas.DrawText(reasonSignature, textX, startY + (lineHeight * 2.5f), fontTextPaint, textPaint);

            //Location
            canvas.DrawText(locationSignature, textX, startY + (lineHeight * 3.0f), fontTextPaint, textPaint);

            // =========================
            // QR
            // =========================

            int qrSize = 245;

            var qrRect = new SKRect(
                860,
                20,
                1000 + qrSize,
                35 + qrSize);

            canvas.DrawBitmap(
                qrBitmap,
                qrRect);

            // =========================
            // EXPORTAR PNG
            // =========================

            using var image = surface.Snapshot();

            using var data = image.Encode(
                SKEncodedImageFormat.Png,
                100);

            return data.ToArray();
        }
    }
}
