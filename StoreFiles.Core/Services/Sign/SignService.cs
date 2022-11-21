using Microsoft.Extensions.Options;
using SignLib.Certificates;
using SignLib.Pdf;
using StoreFiles.Core.DTOs.Sign;
using StoreFiles.Core.Options;
using StoreFiles.Core.Services.Logger;
using System;
using System.Text;

namespace StoreFiles.Core.Services.Sign
{
    public class SignService : ISignService
    {
        private readonly ILoggerService _logger;
        private readonly SignOptions _SignOptions;

        public SignService(ILoggerService logger,
                           IOptions<SignOptions> options)
        {
            _logger = logger;
            _SignOptions = options.Value;
        }

        public byte[] SignFile(SignDto signDto)
        {
            byte[] bytesFileSigned = null;

            //register the code page encoding
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            try
            {
                PdfSignature ps = new PdfSignature(_SignOptions.SerialNumber);

                //load the PDF document
                ps.LoadPdfDocument(signDto.File);
                ps.SignaturePosition = GetSignPosition(signDto.IdSignPosition);
                ps.SigningReason = _SignOptions.SigningReason;
                ps.SigningLocation = _SignOptions.SigningLocation;
                ps.VisibleSignature = signDto.SignatureVisible;

                ps.HashAlgorithm = SignLib.HashAlgorithm.SHA256;

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
    }
}
