using CurrieTechnologies.Razor.SweetAlert2;
using ITSignerWebComponent.SignApp.APIStoreFiles;
using ITSignerWebComponent.SignApp.Error;
using ITSignerWebComponent.SignApp.Responses.APIStoreResponses;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Configuration;
using Microsoft.JSInterop;
using StoreFiles.Core.DTOs.Cades;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ITSignerWebComponent.SignApp.Pages.Cades
{
    public partial class Cades
    {

        [Inject]
        public IJSRuntime _jsRuntime { get; set; }
        [Inject]
        public IAPIStoreFilesService _apiStoreFilesService { get; set; }
        public byte[] Base64Doc { get; set; }
        public string Base64DocSigned { get; set; }
        public CadesFileDto CadesFileDto { get; set; } = new();
        public string Filename { get; set; }

        [Inject]
        public SweetAlertService _swalService { get; set; }

        public CadesFileResponse ResponseService { get; private set; }
        public bool SignedFile { get; private set; }
        public string[] ValidCAs { get; set; }
        [Inject]
        private IConfiguration configuration { get; set; }
        [Inject]
        public IErrorService _errorService { get; set; }
        [Inject]
        private NavigationManager UriHelper { get; set; }

        protected override async void OnInitialized()
        {
            LoadValidCertificatesAuthorities();
            await _jsRuntime.InvokeVoidAsync("cleanLocalStorage");
        }

        public async void onSignCadesFileV2()
        {
            if (this.Base64Doc is null)
            {
                await _swalService.FireAsync("Error", "Debe seleccionar el archivo a firmar", "error");
            }
            else
            {
                string pem = await _jsRuntime.InvokeAsync<string>("getPemSelected");
                if (!string.IsNullOrEmpty(pem))
                {
                    try
                    {

                        if (await isValidCertificateAuthority())
                        {
                            await _jsRuntime.InvokeVoidAsync("setLoadingSigningButton");

                            CadesFileDto.File = this.Base64Doc;

                            var cms = await _jsRuntime.InvokeAsync<string>("generateCMSToEmbed", this.Base64Doc);

                            SignedFile = true;

                            await _jsRuntime.InvokeVoidAsync("downloadFileP7z", cms, Filename);
                            await _swalService.FireAsync("Exitoso", "¡ Archivo Firmado y Descargado Correctamente !", "success");
                            ReloadPage();
                        }
                        else
                        {
                            await _swalService.FireAsync("Error", "El certificado seleccionado no cuenta con una Autoridad de certificación válida", "error");
                        }
                    }
                    catch (Exception ex)
                    {
                        if (ex.Message.Contains("Client cipher is not initialized") || ex.Message.Contains("Socket connection is not open"))
                        {
                            onSignCadesFileV2();
                        }
                        else
                        {
                            _errorService.ProcessError(ex);
                            await _jsRuntime.InvokeVoidAsync("cleanLocalStorage");
                        }
                    }
                }
                else
                {
                    await _swalService.FireAsync("Error", "Por favor seleccione el certificado y a continuación de click sobre el botón Continuar", "error");
                }
            }
                
        }

        public async void onSignCadesFile()
        {
            if (this.Base64Doc is null)
            {
                await _swalService.FireAsync("Error", "Debe seleccionar el archivo a firmar", "error");
            } else
            {
                CadesFileDto.File = this.Base64Doc;

                var response = await _apiStoreFilesService.SignCadesFile(CadesFileDto);
                if (response is not null)
                {
                    ResponseService = response;
                    SignedFile = true;
                    
                    await _jsRuntime.InvokeVoidAsync("downloadFileP7z", ResponseService.SignedFile, Filename);
                    await _swalService.FireAsync("Exitoso", "¡ Archivo Firmado Correctamente ! ", "success");
                }
                else
                {
                    await _swalService.FireAsync("Error", "Se presento un inconveniente y no se pudo firmar el documento", "error");
                }
            }
        }

        public async void OnInputFileChange(InputFileChangeEventArgs e)
        {

            if (e.File.ContentType != "text/plain")
            {
                await _swalService.FireAsync("Error", "El archivo debe ser de tipo texto (.txt)", "error");
            } else
            {
                Filename = e.File.Name;
                long maxFileSize = 1024 * 1024 * 5; // 5 MB or whatever, don't just use max int

                var readStream = e.File.OpenReadStream(maxFileSize);

                var buf = new byte[readStream.Length];

                var ms = new MemoryStream(buf);

                await readStream.CopyToAsync(ms);

                var buffer = ms.ToArray();

                this.Base64Doc = buffer;
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await _jsRuntime.InvokeVoidAsync("InitializeFortify");
            await _jsRuntime.InvokeVoidAsync("enableFormSign");
        }
        private async Task<bool> isValidCertificateAuthority()
        {
            bool isValid = false;

            foreach (var authority in this.ValidCAs)
            {
                if (await _jsRuntime.InvokeAsync<bool>("isValidAuthorityCertificate", authority))
                {
                    return true;
                }
            }

            return isValid;
        }

        private void LoadValidCertificatesAuthorities()
        {
            this.ValidCAs = configuration.GetSection("CertificatesAuthorities").Get<string[]>();
        }

        private async void ReloadPage()
        {
            await _jsRuntime.InvokeVoidAsync("cleanLocalStorage");
            UriHelper.NavigateTo(UriHelper.Uri, forceLoad: true);
        }

    }
}
