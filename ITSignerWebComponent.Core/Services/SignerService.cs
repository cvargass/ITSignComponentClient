﻿using ITSignerWebComponent.Core.DTOs.Signer.EmbedSignature;
using ITSignerWebComponent.Core.DTOs.Signer.PreSigned;
using ITSignerWebComponent.Core.Interfaces.Services;
using SignerPDF.DigitalSignature.Core.DTOs.InfoDataDto;
using SignerPDF.DigitalSignature.Core.Services;
using System;

namespace ITSignerWebComponent.Core.Services
{
    public class SignerService : ISignerService
    {
        private readonly string _signatureFieldName = "SignatureITSign";
        public (byte[] pdfPrepared, byte[] dataToSign) BeginPreSigningProcess(PreSignedDto signedDto)
        {
            byte[] bytesPdf = Convert.FromBase64String(signedDto.PdfToSign);
            (byte[], byte[]) data = (null, null);

            data = PreparePDFAndDataToSign(signedDto);

            return data;
        }

        private (byte[] pdfWithSignPlaceholder, byte[] dataToSign) PreparePDFAndDataToSign(PreSignedDto signedDto)
        {
            SignerDigitalCertificate signerDigitalCertificate = new SignerDigitalCertificate();

            var infoData = new InfoDataDto()
            {
                Pem = signedDto.Pem,
                PDFToSign = signedDto.PdfToSign,
                SignatureFieldName = _signatureFieldName,
                SignatureCreator = "No Disponible",
                Location = "Bogota D.C, Colombia",
                Reason = "Signature ITSign"
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
