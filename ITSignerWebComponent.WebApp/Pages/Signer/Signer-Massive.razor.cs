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
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace ITSignerWebComponent.SignApp.Pages.Signer
{
    public partial class Signer_Massive
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
        public string guidFileSelected { get; set; }
        public List<string> SelectedFiles { get; set; } = new();
        public bool FlagAPIFiles { get; set; } = false;
        public int IdUser { get; set; } = 0;
        public int IdApp { get; set; } = 0;

        protected override void OnInitialized()
        {
            GetQueryParameters();

            var flagActivatedLicense = _apiService.LoadLicense();

            if (flagActivatedLicense)
            {
                FlagActivatedLicense = true;
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (FlagActivatedLicense && firstRender)
            {
                await _jsRuntime.InvokeVoidAsync("enableFormSign");
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
            UriHelper.NavigateTo(UriHelper.Uri, forceLoad: true);
        }

        private async void onCheckBoxChange(string nameFile, object aChecked)
        {
            this.guidFileSelected = nameFile.Replace(".pdf", "");

            if (!(bool)aChecked)
                await _jsRuntime.InvokeVoidAsync("setAllCheckboxesCbValue", false);

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

        private async void onCheckBoxMarkAllChange(object aChecked)
        {
            await _jsRuntime.InvokeVoidAsync("setCheckboxesValue", aChecked);
            SelectedFiles.Clear();

            if ((bool)aChecked)
            {
                foreach (var item in PendingFiles)
                {
                    this.guidFileSelected = item.Replace(".pdf", "");
                    SelectedFiles.Add(this.guidFileSelected);
                }
            }
        }

        private async Task ExecuteApp()
        {
            if (this.SelectedFiles.Count > 0)
            {
                await _jsRuntime.InvokeVoidAsync("setLoadingSigningButton");

                try
                {
                    string parameters = GenerateParameters();

                    var result = await _jsRuntime.InvokeAsync<string>(
                        "launchAppWithProtocol",
                        "appsigner",  // ProtocolName
                        parameters
                    );

                    int seconds = this.SelectedFiles.Count * 2;
                    await ReloadPageAfterDelay(seconds);
                }
                catch (Exception ex)
                {
                    await _swalService.FireAsync("Error", "Ha ocurrido un error cargando y firmando el archivo", "error");
                    ReloadPage();
                }
            }
            else
            {
                await _swalService.FireAsync("Warning", "Por favor seleccione los archivos a firmar", "warning");
            }
        }

        private string GenerateParameters()
        {
            // ESTRUCTURE: type_file1_file2_file3       By default signature is invisible
            string parameters = "massive_";
            foreach (var file in SelectedFiles)
            {
                parameters += file + "_";
            }
            return parameters.Trim();
        }

        private async Task ReloadPageAfterDelay(int seconds)
        {
            await Task.Delay(seconds * 1000); // Convert seconds to milliseconds
            UriHelper.NavigateTo(UriHelper.Uri, forceLoad: true);
        }
    }
}
