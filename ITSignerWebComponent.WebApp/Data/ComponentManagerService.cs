using AutoMapper;
using ITSignerWebComponent.Core.DTOs.License;
using ITSignerWebComponent.Core.DTOs.Signer.EmbedSignature;
using ITSignerWebComponent.Core.DTOs.Signer.PreSigned;
using ITSignerWebComponent.Core.Interfaces.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;

namespace ITSignerWebComponent.SignApp.Data
{
    public class ComponentManagerService : IComponentManagerService
    {
        private readonly IMapper _mapper;
        private readonly ILicenseService _licenseService;
        private readonly ISignerService _signerService;
        private readonly IJSRuntime _jsRuntime;
        private readonly NavigationManager _navigation;

        public ComponentManagerService(
                                   IMapper mapper,
                                   ILicenseService licenseService,
                                   ISignerService signerService,
                                   IConfiguration configuration,
                                   IJSRuntime jsRuntime,
                                   NavigationManager navigation)
        {
            _mapper = mapper;
            _licenseService = licenseService;
            _signerService = signerService;
            _jsRuntime = jsRuntime;
            _navigation = navigation;
        }


        public async Task<(bool flagActivatedLicense, string message)> ActivateLicense(LicenseDto licenseDto)
        {
            bool flagActivatedLicense = false;
            string message;
            var response = await _licenseService.isValidLicense(licenseDto.License);

            if (response.flag)
            {
                if (_licenseService.isValidConfigurationLicense(licenseDto.License))
                {
                    _licenseService.GenerateFileLicense(licenseDto.License);
                    message = "¡ Licencia activada correctamente !";
                    flagActivatedLicense = true;
                } else {
                    message = "¡ Por favor revise su archivo de configuración y verifique que el valor del PIN configurado allí concuerde !";
                    flagActivatedLicense = false;
                }
            }
            else
                message = response.message;

            return (flagActivatedLicense, message);
        }

        public bool LoadLicense()
        {
            bool flagLicenseActivated = false;

            bool loadedLicense = _licenseService.LoadLicense();

            if (loadedLicense)
            {
                flagLicenseActivated = true;
            }

            return flagLicenseActivated;
        }

        public (string bs64DataToSign, string bs64PdfPrepared) GenerateDataToSign(PreSignedDto preSignedDto)
        {
            var data = _signerService.BeginPreSigningProcess(preSignedDto);

            return (
                Convert.ToBase64String(data.dataToSign),
                Convert.ToBase64String(data.pdfPrepared)
            );
        }

        public (string message, string fileSigned) EmbedSignature(EmbedSignatureDto embedSignatureDto)
        {
            var bytesPdfSigned = _signerService.SigningProcessEmbedSignature(embedSignatureDto);

            return ("¡ Archivo firmado correctamente !", Convert.ToBase64String(bytesPdfSigned));
        }
    }
}
