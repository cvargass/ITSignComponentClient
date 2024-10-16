using CurrieTechnologies.Razor.SweetAlert2;
using ITSignerWebComponent.Core.DTOs.License;
using ITSignerWebComponent.Core.DTOs.Signer.PreSigned;
using ITSignerWebComponent.SignApp.APIStoreFiles;
using ITSignerWebComponent.SignApp.Data;
using ITSignerWebComponent.SignApp.Error;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.JSInterop;
using StoreFiles.Core.DTOs.FileForSigning;
using StoreFiles.Core.DTOs.PostFileSigned;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ITSignerWebComponent.SignApp.Pages.Xades
{
    public partial class Xades_DropDownList
    {
        [Inject]
        public IComponentManagerService _apiService { get; set; }

        [Inject]
        public IAPIStoreFilesService _apiStoreFilesService { get; set; }

        [Inject]
        public SweetAlertService _swalService { get; set; }
        [Inject]
        public IJSRuntime _jsRuntime { get; set; }
        [Inject]
        private NavigationManager UriHelper { get; set; }
        [Inject]
        private IConfiguration configuration { get; set; }
        
        [Inject]
        private NavigationManager NavManager { get; set; }

        [Inject]
        public IErrorService _errorService { get; set; }
        public PreSignedDto PreSignedDto { get; set; } = new();
        public LicenseDto LicenseDto { get; set; } = new();
        public bool FlagActivatedLicense { get; set; } = false;
        public bool DisabledBtnActivateLicense { get; set; } = false;
        public string[] PendingFiles { get; set; }
        public string[] ValidCAs { get; set; }
        public string guidFileSelected { get; set; }
        public List<string> SelectedFiles { get; set; } = new();
        public bool FlagAPIFiles { get; set; } = false;
        public int IdUser { get; set; } = 0;
        public int IdApp { get; set; } = 0;
        public FileForSigningDto FileForSigningDto { get; set; } = new();
        protected override void OnInitialized()
        {
            GetQueryParameters();

            var flagActivatedLicense = _apiService.LoadLicense();

            if (flagActivatedLicense)
            {
                LoadValidCertificatesAuthorities();
                FlagActivatedLicense = true;
            }
        }

        private void GetQueryParameters()
        {
            var uri = NavManager.ToAbsoluteUri(NavManager.Uri);

            if (QueryHelpers.ParseQuery(uri.Query).TryGetValue("idUser", out var _idUser))
                IdUser = Convert.ToInt32(_idUser);

            if (QueryHelpers.ParseQuery(uri.Query).TryGetValue("idApp", out var _idApp))
                IdApp = Convert.ToInt32(_idApp);
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (FlagActivatedLicense && firstRender)
            {
                await _jsRuntime.InvokeVoidAsync("InitializeDdlFortify");
                await _jsRuntime.InvokeVoidAsync("enableFormSign");
            }
        }

        protected async override void OnAfterRender(bool firstRender)
        {
            base.OnAfterRender(firstRender);

            if (firstRender)
            {
                try
                {
                    var response = await _apiStoreFilesService.GetPendingFiles(IdUser, IdApp, "xades");
                    if (response.Item2 is not null)
                    {
                        PendingFiles = response.Item2;
                        FlagAPIFiles = true;
                    }

                    StateHasChanged();
                }
                catch (Exception)
                {
                    await _swalService.FireAsync("Warning", "No se ha configurado, o no se ha podido establecer conexión con el API de archivos.", "warning");
                }
            }
        }

        private void LoadValidCertificatesAuthorities()
        {
            this.ValidCAs = configuration.GetSection("CertificatesAuthorities").Get<string[]>();
        }

        public async void onActivateLicense()
        {
            try
            {
                DisableBtnActivateLicense();

                var response = await _apiService.ActivateLicense(LicenseDto);

                if (response.flagActivatedLicense)
                {
                    FlagActivatedLicense = true;
                    await _swalService.FireAsync("Exitoso", "¡ Licencia Activada !", "success");
                    UriHelper.NavigateTo(UriHelper.Uri, forceLoad: true);
                }
                else
                {
                    EnableBtnActivateLicense();
                    await _swalService.FireAsync("Error", response.message, "error");
                }
            }
            catch (Exception ex)
            {
                await _jsRuntime.InvokeVoidAsync("log", "Error on Activated: " + ex.InnerException);

                if (ex.Message.Contains("No se puede establecer una conexión"))
                    _errorService.ProcessError(ex, "Ha ocurrido un inconveniente conectando con el servicio de activacion de ITSign");
                else
                    _errorService.ProcessError(ex);

                EnableBtnActivateLicense();
            }
        }

        private void DisableBtnActivateLicense()
        {
            DisabledBtnActivateLicense = true;
            StateHasChanged();
        }

        private void EnableBtnActivateLicense()
        {
            DisabledBtnActivateLicense = false;
            StateHasChanged();
        }

        private async void ReloadPage()
        {
            await _jsRuntime.InvokeVoidAsync("cleanStoragePageDdlSigner");
            UriHelper.NavigateTo(UriHelper.Uri, forceLoad: true);
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

        public static byte[] GetBytes(Stream stream)
        {
            var bytes = new byte[stream.Length];
            stream.Seek(0, SeekOrigin.Begin);
            stream.ReadAsync(bytes, 0, bytes.Length);
            stream.Dispose();
            return bytes;
        }

        private async void onCheckBoxChange(string nameFile, object aChecked)
        {
            this.guidFileSelected = nameFile.Replace(".xml", "");

            if ((bool)aChecked)
            {
                if (!SelectedFiles.Contains(this.guidFileSelected))
                    SelectedFiles.Add(this.guidFileSelected);
            }
            else
            {
                if (SelectedFiles.Contains(this.guidFileSelected))
                    SelectedFiles.Remove(this.guidFileSelected);
            }
        }

        public async void onSubmit()
        {
            if (FlagAPIFiles)
            {
                string pem = await _jsRuntime.InvokeAsync<string>("getPemSelected");

                if (this.SelectedFiles.Count > 0)
                {
                    if (!string.IsNullOrEmpty(pem))
                    {
                        try
                        {

                            //if (true)
                            if (await isValidCertificateAuthority())
                            {
                                await _jsRuntime.InvokeVoidAsync("setLoadingSigningButton");
                                int signedFiles = 0;

                                foreach (var fileName in this.SelectedFiles)
                                {
                                    var response = await _apiStoreFilesService.GetPendingFile(fileName, "xades");

                                    if (response.Item1)
                                    {
                                        FileForSigningDto.File = Convert.FromBase64String(response.Item2);

                                        var cms = await _jsRuntime.InvokeAsync<string>("generateCMSToEmbed", FileForSigningDto.File);

                                        var flag = await _apiStoreFilesService.PostSignedFile(new PostFileSignedDto { PdfGuid = fileName, PdfSignedBase64 = cms }, "xades");
                                        
                                        if (flag)
                                            signedFiles++;
                                        else
                                        {
                                            await _swalService.FireAsync("Error", "Se presento un inconveniente firmando el archivo " + fileName, "error");
                                            ReloadPage();
                                        }

                                    }
                                    else
                                    {
                                        await _swalService.FireAsync("Error", "Se presento un inconveniente cargando el archivo " + fileName, "error");
                                        ReloadPage();
                                    }
                                }

                                if (signedFiles.Equals(this.SelectedFiles.Count))
                                {
                                    await _swalService.FireAsync("Exitoso", "¡ Archivos Firmados Correctamente !", "success");
                                    ReloadPage();
                                }
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
                                //await _jsRuntime.InvokeVoidAsync("InitializeFortify");
                                onSubmit();
                            }
                            else if(ex.Message.Contains("A task was canceled."))
                            {
                                await _swalService.FireAsync("Error", "Se ha excedido el tiempo limite de espera.", "error");
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
                        await _swalService.FireAsync("Error", "Por favor seleccione el certificado que va usar para firmar", "error");
                    }
                }
                else
                {
                    await _swalService.FireAsync("Warning", "Por favor seleccione los archivos a firmar", "warning");
                }
            }
            else
            {
                await _swalService.FireAsync("Warning", "Debe configurar el API de archivos correctamente.", "warning");
            }
        }

        private async void onCheckBoxMarkAllChange(object aChecked)
        {
            await _jsRuntime.InvokeVoidAsync("setCheckboxesValue", aChecked);
            SelectedFiles.Clear();

            if ((bool)aChecked)
            {
                foreach (var item in PendingFiles)
                {
                    this.guidFileSelected = item.Replace(".xml", "");
                    SelectedFiles.Add(this.guidFileSelected);
                }
            }
        }
    }
}
