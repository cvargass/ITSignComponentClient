using Microsoft.Extensions.Options;
using SignLib;
using SignLib.Certificates;
using StoreFiles.Core.DTOs.FileForSigning;
using StoreFiles.Core.Options;
using StoreFiles.Core.Services.Logger;
using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;

namespace StoreFiles.Core.Services.Xades
{
    public class XadesService : IXadesService
    {
        private readonly ILoggerService _loggerService;
        private readonly SignOptions _signOptions;
        private readonly string _urlTempFolder = "./TempCadesSigning/";
        private string _nameTempFile = "Temp1.xml";
        private string _nameTempFileSigned = "Temp1Signed.xml";

        public XadesService(ILoggerService loggerService,
                            IOptions<SignOptions> options)
        {
            _loggerService = loggerService;
            _signOptions = options.Value;
        }

        public byte[] SignXadesFile(FileForSigningDto cadesFileDto)
        {
            byte[] signedFile = null;
            X509Certificate2 certificate = null;
            string inputPath = _urlTempFolder + _nameTempFile;
            string outputPath = _urlTempFolder + _nameTempFileSigned;

            try
            {
                X509Certificate2 cert = new X509Certificate2(_signOptions.UrlSignCertificate, _signOptions.PasswordCertificate, X509KeyStorageFlags.Exportable);
                //certificate = DigitalCertificate.LoadCertificate(_signOptions.UrlSignCertificate, _signOptions.PasswordCertificate);

                //XadesSignature cs = new XadesSignature(_signOptions.SerialNumber);
                XmlSignatureSha256 cs = new XmlSignatureSha256(_signOptions.SerialNumber);
                //set the certificate
                //cs.DigitalSignatureCertificate = certificate;
                cs.DigitalSignatureCertificate = cert;

                cs.IncludeKeyInfo = true;
                cs.IncludeSignatureCertificate = true;
                cs.RemoveWhitespaces = true;

                cs.SignatureType = XmlSignatureType.DefaultWithComments;

                SaveFolderXadesFile(cadesFileDto.File);

                //apply the digital signature
                cs.ApplyDigitalSignature(inputPath, outputPath);

                signedFile = File.ReadAllBytes(outputPath);

                RemoveTempFolder();
            }
            catch (Exception ex)
            {
                 RemoveTempFolder();
                _loggerService.LogExceptionError(ex.Message, ex);
            }

            return signedFile;
        }

        private void RemoveTempFolder()
        {
            string[] files = Directory.GetFiles(_urlTempFolder);
            foreach (string file in files) {
                File.SetAttributes(file, FileAttributes.Normal); 
                File.Delete(file);
            }

            Directory.Delete(_urlTempFolder);
        }

        private void SaveFolderXadesFile(byte[] file)
        {
            if (!Directory.Exists(_urlTempFolder))
                Directory.CreateDirectory(_urlTempFolder);

            using var writer = new BinaryWriter(File.OpenWrite(_urlTempFolder + _nameTempFile));
            writer.Write(file);
        }
    }
}
