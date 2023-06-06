using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using SignLib.Certificates;
using SignLib.Pdf;
using StoreFiles.Core.DTOs.Sign;
using StoreFiles.Core.Entities.AxisPosition;
using StoreFiles.Core.Options;
using StoreFiles.Core.Services.Logger;
using StoreFiles.Core.Services.Utils.WriterQR;
using System;
using System.Drawing;
using System.Text;

namespace StoreFiles.Core.Services.Sign
{
    public class SignService : ISignService
    {
        private readonly ILoggerService _logger;
        private readonly SignOptions _SignOptions;
        private readonly IWriterQRService _writerQRService;
        private readonly string _textQr;

        public SignService(ILoggerService logger,
                           IOptions<SignOptions> options,
                           IWriterQRService writerQRService,
                           IConfiguration configuration)
        {
            _logger = logger;
            _SignOptions = options.Value;
            _writerQRService = writerQRService;
            _textQr = configuration["QrConfiguration:Text"].ToString();
        }

        public byte[] SignFile(SignDto signDto)
        {
            byte[] bytesFileSigned = null;

            //register the code page encoding
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            try
            {
                PdfSignature ps = new PdfSignature(_SignOptions.SerialNumber);

                if (signDto.QRPosition is not null)
                    signDto.File = WriteQRInPDF(signDto.File, signDto.QRPosition, _textQr);

                //load the PDF document
                ps.LoadPdfDocument(signDto.File);
                ps.SigningReason = _SignOptions.SigningReason;
                ps.SigningLocation = _SignOptions.SigningLocation;
                ps.VisibleSignature = signDto.SignatureVisible;

                if (ps.VisibleSignature)
                {
                    if (signDto.SignPosition is not null)
                    {
                        ps.SignatureAdvancedPosition = new Rectangle(new Point(signDto.SignPosition.CoordinateX, signDto.SignPosition.CoordinateY), new Size(80, 80));
                        ps.SignaturePage = signDto.SignPosition.NumberPage;
                    } else
                    {
                        ps.SignaturePosition = GetSignPosition(signDto.IdSignPosition);
                    }
                }

                ps.HashAlgorithm = SignLib.HashAlgorithm.SHA256;

                if (signDto.UrlSignCertificate is not null && signDto.PasswordCertificate is not null)
                {
                    _SignOptions.UrlSignCertificate = signDto.UrlSignCertificate;
                    _SignOptions.PasswordCertificate = signDto.PasswordCertificate;
                }

                //Load the signature certificate from P12 file
                ps.DigitalSignatureCertificate = DigitalCertificate.LoadCertificate(_SignOptions.UrlSignCertificate, _SignOptions.PasswordCertificate);

                bytesFileSigned = ps.ApplyDigitalSignature();
            }
            catch (Exception ex)
            {
                _logger.LogExceptionError(ex.Message, ex);
            }

            return bytesFileSigned;
        }

        private SignaturePosition GetSignPosition(int? idPositionSign)
        {
            if (idPositionSign is null)
                idPositionSign = _SignOptions.SignDefaultPosition;

            switch (idPositionSign)
            {
                case 1:
                    return SignaturePosition.TopLeft;
                case 2:
                    return SignaturePosition.TopMiddle;
                case 3:
                    return SignaturePosition.TopRight;
                case 4:
                    return SignaturePosition.BottomLeft;
                case 5:
                    return SignaturePosition.BottomMiddle;
                case 6:
                    return SignaturePosition.BottomRight;
                default:
                    return SignaturePosition.BottomRight;
            }
        }

        public byte[] WriteQRInPDF(byte[] bytesPdf, AxisPosition dataQrPosition, string textQRUrlDoc)
        {
            bytesPdf = _writerQRService.WriteQRWithCoordinates(bytesPdf, textQRUrlDoc, dataQrPosition);

            return bytesPdf;
        }
    }
}
