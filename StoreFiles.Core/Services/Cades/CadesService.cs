using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Options;
using SignLib.Cades;
using SignLib.Certificates;
using StoreFiles.Core.DTOs.Cades;
using StoreFiles.Core.Options;
using StoreFiles.Core.Services.Logger;
using System;
using System.Security.Cryptography.X509Certificates;

namespace StoreFiles.Core.Services.Cades
{
    public class CadesService : ICadesService
    {
        private readonly ILoggerService _loggerService;
        private readonly SignOptions _signOptions;

        public CadesService(ILoggerService loggerService,
                            IOptions<SignOptions> options)
        {
            _loggerService = loggerService;
            _signOptions = options.Value;
        }

        public byte[] SignCadesFile(CadesFileDto cadesFileDto)
        {
            byte[] signedFile = null;
            X509Certificate2 certificate= null;


            try
            {
                certificate = DigitalCertificate.LoadCertificate(_signOptions.UrlSignCertificate, _signOptions.PasswordCertificate);

                //digitally sign a file in PKCS#7 format
                //on the demo version it will be a 5 seconds delay for every operation (digital signature creation or verification)
                //this is the single restriction of the library
                //http://www.signfiles.com/file-sign-library/
                CadesSignature pkcs7Sign = new CadesSignature(_signOptions.SerialNumber);

                //set the certificate
                pkcs7Sign.DigitalSignatureCertificate = certificate;


                //optionally, the file can be timestamped
                //pkcs7Sign.TimeStamping.ServerUrl = new Uri("http://ca.signfiles.com/TSAServer.aspx");

                //thi file can be saved as .p7s or .p7m file
                signedFile = pkcs7Sign.ApplyDigitalSignature(cadesFileDto.File);
            }
            catch (Exception ex)
            {
                _loggerService.LogExceptionError(ex.Message, ex);
            }

            return signedFile;
        }
    }
}
