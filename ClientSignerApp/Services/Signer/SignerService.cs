using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using SignLib.Certificates;
using SignLib.Pdf;
using System.Security.Cryptography.X509Certificates;
using ClientSignerApp.Services.Logger;
using ClientSignerApp.Options;
using System.Text;
using ClientSignerApp.Services.QRGenerator;
using System.Net;

namespace ClientSignerApp.Services.Signer
{
    public class SignerService : ISignerService
    {
        private readonly ILoggerService _logger;
        private readonly SignOptions _SignOptions;
        private readonly IConfiguration _configuration;
        private string _textQr;
        private IQRGeneratorService _qrGeneratorService;

        public SignerService(ILoggerService logger,
                           IOptions<SignOptions> options,
                           IConfiguration configuration,
                           IQRGeneratorService qrGeneratorService)
        {
            _logger = logger;
            _SignOptions = options.Value;
            _configuration = configuration;
            _textQr = configuration["QrConfiguration:Text"].ToString();
            _qrGeneratorService = qrGeneratorService;
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

        public byte[] SignWithSmartCard(byte[] file, string guidFile)
        {
            byte[] bytesFileSigned = null;
            X509Certificate2Collection certificates = null;
            //PdfSignature ps = new PdfSignature(_SignOptions.SerialNumber);
            PdfSignature ps = new PdfSignature(_configuration["SignConfigurations:SerialNumber"]);

            try
            {
                ps.VisibleSignature = false;
                ps.DigitalSignatureCertificate = DigitalCertificate.LoadCertificate(false, "", "Seleccione el certificado", "");

                if (ps.DigitalSignatureCertificate == null)
                    throw new InvalidOperationException("No se ha seleccionado ningún certificado.");

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

        public byte[] SignFile(byte[] file, string guidFile, string dataPosition)
        {
            byte[] bytesFileSigned = null;

            //register the code page encoding
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            try
            {
                PdfSignature ps = new PdfSignature(_configuration["SignConfigurations:SerialNumber"]);
                ps.DigitalSignatureCertificate = DigitalCertificate.LoadCertificate(false, "", "Seleccione el certificado", "");

                if (ps.DigitalSignatureCertificate == null)
                    throw new InvalidOperationException("No se ha seleccionado ningún certificado.");

                string certificateInfo = GetCertificateInformation(ps.DigitalSignatureCertificate);

                byte[] bytesQR = _qrGeneratorService.GenerateQR(certificateInfo);

                //load the PDF document
                ps.LoadPdfDocument(file);
                //ps.SigningReason = _SignOptions.SigningReason;
                //ps.SigningLocation = _SignOptions.SigningLocation;
                ps.VisibleSignature = true;
                ps.SignatureImage = bytesQR;
                ps.SignatureImageType = SignatureImageType.ImageAndText;


                if (dataPosition is not null)
                {
                    dataPosition = WebUtility.UrlDecode(dataPosition);
                    //_logger.LogInformation("DataPosition:" + dataPosition);
                    int numberPage = Convert.ToInt32(dataPosition.Split("|")[0]);
                    int coordinateX = Convert.ToInt32(dataPosition.Split("|")[1]);
                    int CoordinateY = Convert.ToInt32(dataPosition.Split("|")[2]);

                    ps.SignatureAdvancedPosition = new Rectangle(new Point(coordinateX, CoordinateY), new Size(200, 80));
                    ps.SignaturePage = numberPage;
                }
                else
                {
                    ps.SignaturePosition = GetSignPosition(1);
                }

                ps.HashAlgorithm = SignLib.HashAlgorithm.SHA256;

                bytesFileSigned = ps.ApplyDigitalSignature();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error firmando el documento con identificador {guidFile}.");
                _logger.LogError(ex.Message, ex);
            }

            return bytesFileSigned;
        }

        public string GetCertificateInformation(X509Certificate2 certificate)
        {
            string certificateInfo = String.Empty;

            try
            {
                if (certificate is not null)
                {
                    string[] itemsSubject = certificate.Subject.Split(",");
                    string issuerName = string.Empty;

                    issuerName = itemsSubject.Where(x => x.Contains("CN=")
                                                                || x.Contains("SN=")).FirstOrDefault();
                    if (issuerName is null)
                        issuerName = "issuerName";
                    else
                    {
                        issuerName = issuerName.Replace("CN=", "");
                        issuerName = issuerName.Replace("SN=", "");
                    }

                    _textQr = _textQr.Replace("[NAME]", issuerName);
                    _textQr = _textQr.Replace("[REASON]", _SignOptions.SigningReason);
                    _textQr = _textQr.Replace("[LOCATION]", _SignOptions.SigningLocation);
                    _textQr = _textQr.Replace("[DATE]", DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fffffffK"));

                    certificateInfo = _textQr;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
            }

            return certificateInfo;
        }
    }
}
