﻿using ITSignerWebComponent.Core.Interfaces.Services;
using ITSignerWebComponent.SignApp.Responses.APIITSignResponse;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace ITSignerWebComponent.Core.Services
{
    public class LicenseService : ILicenseService
    {
        #region URLs API ITSIGN
            private readonly string _baseUrlAPIITSign = "https://signtest.italm.com.co";
            //private readonly string _baseUrlAPIITSign = "https://localhost:44380";
            private readonly string _urlGetPendingCustomer = "api/external/pending-customer-component?pinLicense=[PIN-LICENSE]";
            private readonly string _urlActivateLicense = "api/external/activate-licence?pinLicense=[PIN-LICENSE]";
        #endregion

        private readonly IConfiguration _configuration;
        public readonly IEncryptorService _encryptorService;
        private readonly string _licensePIN;
        private readonly string nameFileLicense;
        private readonly string _extension = ".license";
        private readonly string licensePin = "[PIN_ENCRYPT]";
        private readonly int linePinEncrypt = 4;
        private readonly HttpClient httpClient;

        public LicenseService(IConfiguration configuration, IEncryptorService encryptorService)
        {
            _configuration = configuration;
            _encryptorService = encryptorService;
            _licensePIN = _configuration["License:PIN"];
            this.nameFileLicense = $"{_licensePIN}{_extension}";
            httpClient = new HttpClient() { BaseAddress = new Uri(_baseUrlAPIITSign) };
        }

        public async Task<(bool flag, string message)> isValidLicense(string license)
        {
            bool isValid = false;
            string message = string.Empty;

            var response = await GetPendingCustomerComponent(license);

            if (response is not null)
            {
                if (response.Flag)
                    isValid = true;
                else
                    message = "El PIN ingresado no es válido.";
            }
            else
                message = "Ha ocurrido un inconveniento conectando al servicio de validaciòn de Licencia.";

            return (isValid, message);
        }

        public bool isValidConfigurationLicense(string license)
        {
            bool isValid = false;

            if (license.ToLower() == this._licensePIN.ToLower())
            {
                isValid = true;
            }

            return isValid;
        }

        public async void GenerateFileLicense(string license)
        {
            var response = await GetPendingCustomerComponent(license);

            if (response is not null && response.Flag)
            {
                var customerLicensePending = response.CustomerComponent;

                string base64ContentKey = GenerateKeyLicenseContent(customerLicensePending.Licencia);

                string base64Content = GenerateLicenseContent(base64ContentKey);

                using (StreamWriter outputFile = new StreamWriter(new FileStream($"./{nameFileLicense}", FileMode.Create)))
                {
                    outputFile.WriteLine(base64Content);
                    outputFile.Flush();
                };

                File.SetAttributes($"./{nameFileLicense}", FileAttributes.Hidden);

                await ActivateLicense(license);
            }
        }

        private string GenerateKeyLicenseContent(string licencia)
        {
            var encryptPIN = _encryptorService.EncryptString(licencia);

            return encryptPIN;
        }

        public bool LoadLicense()
        {
            bool isValidLicense = false;
            try
            {
                IEnumerable<string> linesFileLicense = File.ReadLines($"./{nameFileLicense}");

                string licenseEncrypt = _encryptorService.EncryptString(nameFileLicense.Replace(_extension, ""));

                isValidLicense = linesFileLicense.ToArray()[linePinEncrypt-1].Equals(licenseEncrypt);
            }
            catch (Exception)
            {
                return isValidLicense;
            }

            return isValidLicense;
        }

        public string GenerateLicenseContent(string pin)
        {
            string licenseContent = "Component ITSign License \n======================== \n \n[PIN_ENCRYPT]\n \nLicense file automatically generated by ITSign Component at the time the license is activated. Permission is hereby granted to make use of the signature services provided by ItSign.\nTo anyone who obtains a copy\nof this software, activities in the Software including copying, modifying, merging, sub-licensing and/or selling copies of the Software are restricted.\n\nTHE SOFTWARE IS PROVIDED 'AS IS' WITHOUT WARRANTY OF ANY KIND, EXPRESS OR\nIMPLIED, LIMITED TO WARRANTIES OF MERCHANTABILITY,\n FITNESS FOR A PARTICULAR PURPOSE AND NON-INFRINGEMENT. IN NO CASE THE\nTHE AUTHORS OR COPYRIGHT HOLDERS SHALL BE RESPONSIBLE FOR ANY CLAIMS, DAMAGES OR OTHERWISE.\nLIABILITY, WHETHER IN AN ACTION IN CONTRACT, TORT OR OTHERWISE, ARISING OUT OF,\nOUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OF OR OTHER DEALINGS IN\nTHE SOFTWARE.\n";

            return licenseContent.Replace(licensePin, pin);
        }

        private async Task<GetCustomerResponse> GetPendingCustomerComponent(string pinLicense)
        {
            string url = _urlGetPendingCustomer;
            url = url.Replace("[PIN-LICENSE]", pinLicense);

            GetCustomerResponse getCustomerResponse = null;
            var response = await httpClient.GetAsync(url);

            //if (response.IsSuccessStatusCode)
                getCustomerResponse = await response.Content.ReadFromJsonAsync<GetCustomerResponse>();


            return getCustomerResponse;
        }

        public async Task<ActivateLicenseResponse> ActivateLicense(string pinLicense)
        {
            string url = _urlActivateLicense;
            url = url.Replace("[PIN-LICENSE]", pinLicense);

            ActivateLicenseResponse activateLicenseResponse = null;
            var response = await httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
                activateLicenseResponse = await response.Content.ReadFromJsonAsync<ActivateLicenseResponse>();


            return activateLicenseResponse;
        }
    }
}