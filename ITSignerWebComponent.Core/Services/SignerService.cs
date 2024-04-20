using ITSignerWebComponent.Core.DTOs.Signer.EmbedSignature;
using ITSignerWebComponent.Core.DTOs.Signer.PreSigned;
using ITSignerWebComponent.Core.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using SignerPDF.DigitalSignature.Core.DTOs.InfoDataDto;
using SignerPDF.DigitalSignature.Core.Services;
using StoreFiles.Core.Services.Utils.QRGenerator;
using System;

namespace ITSignerWebComponent.Core.Services
{
    public class SignerService : ISignerService
    {
        private string _signatureFieldName = "SignatureITSign";
        private readonly IQRGeneratorService _qrGeneratorService;
        private readonly IConfiguration _configuration;

        public SignerService(IQRGeneratorService qrGeneratorService,
                             IConfiguration configuration)
        {
            _qrGeneratorService = qrGeneratorService;
            _configuration = configuration;
        }

        public (byte[] pdfPrepared, byte[] dataToSign) BeginPreSigningProcess(PreSignedDto signedDto, bool changeFieldName = false)
        {
            byte[] bytesPdf = Convert.FromBase64String(signedDto.PdfToSign);
            (byte[], byte[]) data = (null, null);

            data = PreparePDFAndDataToSign(signedDto, changeFieldName);

            return data;
        }

        private (byte[] pdfWithSignPlaceholder, byte[] dataToSign) PreparePDFAndDataToSign(PreSignedDto signedDto, bool changeFieldName = false)
        {
            SignerDigitalCertificate signerDigitalCertificate = new SignerDigitalCertificate();

            string infoCertificate = signedDto.InfoCertificate.DataCertificate;
            infoCertificate = infoCertificate.Replace("[DATE]", DateTime.Now.ToString());

            byte[] bytesQR = _qrGeneratorService.GenerateQR(infoCertificate);

            if (changeFieldName)
                _signatureFieldName += Guid.NewGuid().ToString().Substring(0, 1).ToUpper();

            if (_configuration["InfoSignature:Reason"] is not null)
                signedDto.InfoCertificate.Reason = _configuration["InfoSignature:Reason"];

            if (_configuration["InfoSignature:Location"] is not null)
                signedDto.InfoCertificate.Location = _configuration["InfoSignature:Location"];

            var infoData = new InfoDataDto()
            {
                Pem = signedDto.Pem,
                PDFToSign = signedDto.PdfToSign,
                SignatureFieldName = _signatureFieldName,
                SignatureCreator = "No Disponible",
                Location = signedDto.InfoCertificate.Location,
                Reason = signedDto.InfoCertificate.Reason,
                Visible = signedDto.Visible,
                Image = bytesQR,
                SignPosition = signedDto.Position
            };

            return signerDigitalCertificate.PreparePDFAndGetHash(infoData);
        }

        public byte[] SigningProcessEmbedSignature(EmbedSignatureDto signatureDto)
        {
            byte[] pdfSigned = null;

            SignerDigitalCertificate signerDigitalCertificate = new SignerDigitalCertificate();
            var signedDto = new SignerPDF.DigitalSignature.Core.DTOs.SignDto.SignDto()
            {
                PdfFilePrepared = signatureDto.PdfFilePrepared,
                Signature = signatureDto.Signature,
                SignatureFieldName = _signatureFieldName
            };

            var bytesPdfSigned = signerDigitalCertificate.EmbedSignature(signedDto);

            if (bytesPdfSigned != null)
                pdfSigned = bytesPdfSigned;
            else
                throw new Exception("Ha ocurrido un inconveniente incrustando la firma en el documento.");

            return pdfSigned;
        }
    }
}
