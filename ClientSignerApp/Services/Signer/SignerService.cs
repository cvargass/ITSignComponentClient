using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using SignLib.Certificates;
using SignLib.Pdf;
using System.Security.Cryptography.X509Certificates;
using ClientSignerApp.Services.Logger;
using ClientSignerApp.Options;
using System.Configuration;

namespace ClientSignerApp.Services.Signer
{
    public class SignerService : ISignerService
    {
        private readonly ILoggerService _logger;
        private readonly SignOptions _SignOptions;
        private readonly IConfiguration _configuration;

        public SignerService(ILoggerService logger,
                           IOptions<SignOptions> options,
                           IConfiguration configuration)
        {
            _logger = logger;
            _SignOptions = options.Value;
            _configuration = configuration;
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

        public byte[] SignWithSmartCard(byte[] file, string guidFile, bool signatureVisible = true, int signaturePosition = 6)
        {
            byte[] bytesFileSigned = null;
            X509Certificate2Collection certificates = null;
            //PdfSignature ps = new PdfSignature(_SignOptions.SerialNumber);
            PdfSignature ps = new PdfSignature(_configuration["SignConfigurations:SerialNumber"]);

            try
            {
                //ps.SigningReason = _SignOptions.SigningReason;
                //ps.SigningLocation = _SignOptions.SigningLocation;
                ps.VisibleSignature = signatureVisible;
                ps.DigitalSignatureCertificate = DigitalCertificate.LoadCertificate(false, "", "Seleccione el certificado", "");

                if (ps.DigitalSignatureCertificate == null)
                    throw new InvalidOperationException("No se ha seleccionado ningún certificado.");

                if (ps.VisibleSignature)
                    ps.SignaturePosition = GetSignPosition(signaturePosition);

                ps.LoadPdfDocument(file);
                bytesFileSigned = ps.ApplyDigitalSignature();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error firmando el documento con identificador {guidFile}.");
                _logger.LogError(ex.Message, ex);
            }

            return bytesFileSigned;
        }
    }
}
