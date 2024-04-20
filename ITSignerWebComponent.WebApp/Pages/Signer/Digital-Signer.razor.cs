using CurrieTechnologies.Razor.SweetAlert2;
using ITSignerWebComponent.Core.DTOs.License;
using ITSignerWebComponent.SignApp.APIStoreFiles;
using ITSignerWebComponent.SignApp.Data;
using ITSignerWebComponent.SignApp.Error;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;

namespace ITSignerWebComponent.SignApp.Pages.Signer
{
    public partial class Digital_Signer
    {
        [Inject]
        public IComponentManagerService _apiService { get; set; }
        [Inject]
        private NavigationManager NavManager { get; set; }
        [Inject]
        private IConfiguration configuration { get; set; }
        [Inject]
        public IJSRuntime _jsRuntime { get; set; }
        [Inject]
        public IAPIStoreFilesService _apiStoreFilesService { get; set; }
        [Inject]
        public SweetAlertService _swalService { get; set; }
        [Inject]
        public IErrorService _errorService { get; set; }

        public bool FlagActivatedLicense { get; set; } = false;
        public int IdUser { get; set; } = 0;
        public int IdApp { get; set; } = 0;
        public string[] ValidCAs { get; set; }
        public string[] PendingFiles { get; set; }
        public bool FlagAPIFiles { get; set; } = false;
        public LicenseDto LicenseDto { get; set; } = new();
        public bool DisabledBtnActivateLicense { get; set; } = false;
        public bool ShowDragAndDropComponent { get; set; } = false;
        public string Filename { get; set; }

        protected async override void OnInitialized()
        {
            GetQueryParameters();

            var flagActivatedLicense = _apiService.LoadLicense();

            if (flagActivatedLicense)
            {
                LoadValidCertificatesAuthorities();
                FlagActivatedLicense = true;
                try
                {
                    await _jsRuntime.InvokeVoidAsync("cleanStoragePageDdlSigner");
                }
                catch (Exception)
                {
                }
            }
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
                    var response = await _apiStoreFilesService.GetPendingFiles(IdUser, IdApp, "pades");
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

        private void GetQueryParameters()
        {
            var uri = NavManager.ToAbsoluteUri(NavManager.Uri);

            if (QueryHelpers.ParseQuery(uri.Query).TryGetValue("idUser", out var _idUser))
                IdUser = Convert.ToInt32(_idUser);

            if (QueryHelpers.ParseQuery(uri.Query).TryGetValue("idApp", out var _idApp))
                IdApp = Convert.ToInt32(_idApp);
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
                    NavManager.NavigateTo(NavManager.Uri, forceLoad: true);
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

        public async void LoadPDFViewerForSigning(string nameFile)
        {
            if (FlagAPIFiles)
            {
                string pem = await _jsRuntime.InvokeAsync<string>("getPemSelected");

                if (!string.IsNullOrEmpty(pem))
                {
                    if (await isValidCertificateAuthority())
                    {
                        Filename = nameFile.Replace(".pdf", "");
                        ShowDragAndDropComponent = true;
                        StateHasChanged();
                    }
                    else
                    {
                        await _swalService.FireAsync("Error", "El certificado seleccionado no cuenta con una Autoridad de certificación válida", "error");
                    }
                }
                else
                {
                    Filename = "";
                    ShowDragAndDropComponent = false;
                    StateHasChanged();
                    await _swalService.FireAsync("Error", "Por favor seleccione el certificado que va usar para firmar", "error");
                }
            }
            else
            {
                await _swalService.FireAsync("Warning", "Debe configurar el API de archivos correctamente.", "warning");
            }
        }
    }
}
