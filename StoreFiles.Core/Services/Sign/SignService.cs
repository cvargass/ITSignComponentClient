using iText.Kernel.Pdf.Canvas.Parser;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using SignLib.Certificates;
using SignLib.Pdf;
using StoreFiles.Core.DTOs.Sign;
using StoreFiles.Core.Entities.AxisPosition;
using StoreFiles.Core.Entities.InformationTsa;
using StoreFiles.Core.Options;
using StoreFiles.Core.Services.Logger;
using StoreFiles.Core.Services.Utils.QRGenerator;
using StoreFiles.Core.Services.Utils.WriterQR;
using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using iText.Kernel.Pdf;
using System.IO;
using iText.Kernel.Geom;
using StoreFiles.Core.Services.Utils;
using StoreFiles.Core.Services.Utils.TextLocationStrategy;
using StoreFiles.Core.DTOs.PostFileSigned;
using StoreFiles.Core.Services.StoreFiles;
using StoreFiles.API.Services.StoreFiles;

namespace StoreFiles.Core.Services.Sign
{
    public class SignService : ISignService
    {
        private readonly ILoggerService _logger;
        private readonly SignOptions _SignOptions;
        private readonly IWriterQRService _writerQRService;
        private string _textQr;
        private readonly IQRGeneratorService _qrGeneratorService;
        private readonly IUtilsService _utilsService;
        private readonly bool _savePDFFilesAPI;
        private readonly IStoreFileService _storeFileService;

        public SignService(ILoggerService logger,
                           IOptions<SignOptions> options,
                           IWriterQRService writerQRService,
                           IConfiguration configuration,
                           IQRGeneratorService qrGeneratorService,
                           IUtilsService utilsService,
                           IStoreFileService storeFileService)
        {
            _logger = logger;
            _SignOptions = options.Value;
            _writerQRService = writerQRService;
            _textQr = configuration["QrConfiguration:Text"].ToString();
            _qrGeneratorService = qrGeneratorService;
            _utilsService = utilsService;
            _savePDFFilesAPI = Convert.ToBoolean(configuration["SignConfigurations:SavePDFFilesAPI"]);
            _storeFileService = storeFileService;
        }

        public byte[] SignFile(SignDto signDto)
        {
            byte[] bytesFileSigned = null;

            //register the code page encoding
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            try
            {
                PdfSignature ps = new PdfSignature(_SignOptions.SerialNumber);

                string certificateInfo = GetCertificateInformation(signDto);

                byte[] bytesQR = _qrGeneratorService.GenerateQR(certificateInfo);
                var response = FindFirstLetterSigningKey(signDto.File, signDto.WordKeySigning);

                //if (signDto.QRPosition is not null)
                //    signDto.File = WriteQRInPDF(signDto.File, signDto.QRPosition, certificateInfo);

                //load the PDF document
                ps.LoadPdfDocument(signDto.File);
                ps.SigningReason = _SignOptions.SigningReason;
                ps.SigningLocation = _SignOptions.SigningLocation;
                ps.VisibleSignature = signDto.SignatureVisible;
                ps.SignatureImage = bytesQR;
                ps.SignatureImageType = SignatureImageType.ImageAndText;


                if (!string.IsNullOrEmpty(signDto.WordKeySigning))
                {
                    ps.SignatureAdvancedPosition = new System.Drawing.Rectangle(new System.Drawing.Point((int)response.InfoFirstLetterKey.GetX() - 10, (int)response.InfoFirstLetterKey.GetY() - 40), new System.Drawing.Size(80, 80));
                    ps.SignaturePage = response.NumberPage;
                }

                if (ps.VisibleSignature)
                {   
                    /*
                    if (signDto.SignPosition is not null)
                    {
                        ps.SignatureAdvancedPosition = new Rectangle(new Point(signDto.SignPosition.CoordinateX, signDto.SignPosition.CoordinateY), new Size(80, 80));
                        ps.SignaturePage = signDto.SignPosition.NumberPage;
                    } else
                    {
                        ps.SignaturePosition = GetSignPosition(signDto.IdSignPosition);
                    }
                    */
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

                if (signDto.InformationTsa is not null)
                    bytesFileSigned = ApplyTSASignature(bytesFileSigned, signDto.InformationTsa);

                if (_savePDFFilesAPI)
                    _storeFileService.StoreFileSignedAPI(bytesFileSigned);
            }
            catch (Exception ex)
            {
                _logger.LogExceptionError(ex.Message, ex);
            }

            return bytesFileSigned;
        }


        private (Rectangle InfoFirstLetterKey, int NumberPage) FindFirstLetterSigningKey(byte[] file, string textToFind)
        {
            var response = _utilsService.TextExistInFile(file, textToFind);

            if (response.exists)
            {
                using Stream pdfStream = new MemoryStream(file);
                using PdfDocument pdfDocument = new PdfDocument(new PdfReader(pdfStream));

                var textLocationStrategy = new TextLocationStrategy();

                //Parse page 1 of the document above
                var ex = PdfTextExtractor.GetTextFromPage(pdfDocument.GetPage((int)response.numberPage), textLocationStrategy);
                var matrizCharacters = textLocationStrategy.objectResult;

                var textCoordinates = _utilsService.FindCoordinatesText(matrizCharacters, textToFind);
                var infoFirstLetterKey = textCoordinates.FirstOrDefault();

                return (infoFirstLetterKey.Rectangule, (int)response.numberPage);
            } else
            {
                throw new Exception("La 'wordKeySigning' no se encontró en el documento.");
            }
        }


        public byte[] ApplyTSASignature(byte[] pdf, InformationTsa informationTsa) 
        {
            byte[] bytesTSASignedPDF = null;
            try
            {
                PdfSignature ps = new PdfSignature(_SignOptions.SerialNumber);
                ps.LoadPdfDocument(pdf);

                //Timestamp Signature
                ps.TimeStamping.ServerUrl = new Uri(informationTsa.Url);
                ps.TimeStamping.UserName = informationTsa.User;
                ps.TimeStamping.Password = informationTsa.Password;
                bytesTSASignedPDF = ps.ApplyTimestampSignature();
            }
            catch (Exception ex)
            {
                _logger.LogExceptionError(ex.Message, ex);
            }
            return bytesTSASignedPDF;
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

        public string GetCertificateInformation(SignDto signDto)
        {
            X509Certificate2 certificate = null;
            string certificateInfo = String.Empty;

            try
            {
                PdfSignature ps = new PdfSignature(_SignOptions.SerialNumber);

                if (signDto.UrlSignCertificate is not null && signDto.PasswordCertificate is not null)
                {
                    _SignOptions.UrlSignCertificate = signDto.UrlSignCertificate;
                    _SignOptions.PasswordCertificate = signDto.PasswordCertificate;
                }

                //Load the signature certificate from P12 file
                ps.DigitalSignatureCertificate = DigitalCertificate.LoadCertificate(_SignOptions.UrlSignCertificate, _SignOptions.PasswordCertificate);
                certificate = ps.DigitalSignatureCertificate;

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
                _logger.LogExceptionError(ex.Message, ex);
            }

            return certificateInfo;
        }
    }
}
