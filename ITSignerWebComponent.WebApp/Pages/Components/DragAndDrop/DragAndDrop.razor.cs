using BlazorBootstrap;
using CurrieTechnologies.Razor.SweetAlert2;
using ITSignerWebComponent.Core.DTOs.Signer.EmbedSignature;
using ITSignerWebComponent.Core.DTOs.Signer.InfoCertificate;
using ITSignerWebComponent.Core.DTOs.Signer.PreSigned;
using ITSignerWebComponent.Core.Entities.DragItem;
using ITSignerWebComponent.SignApp.APIStoreFiles;
using ITSignerWebComponent.SignApp.Data;
using ITSignerWebComponent.SignApp.Error;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using SignerPDF.DigitalSignature.Core.Domain.AxisPosition;
using StoreFiles.Core.DTOs.PostFileSigned;
using StoreFiles.Core.Services.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ITSignerWebComponent.SignApp.Pages.Components.DragAndDrop
{
    public partial class DragAndDrop
    {
        [Parameter]
        public string Filename { get; set; }

        [Inject]
        public IJSRuntime _jsRuntime { get; set; }
        [Inject]
        public IAPIStoreFilesService _apiStoreFilesService { get; set; }
        [Inject]
        public SweetAlertService _swalService { get; set; }
        [Inject]
        public IErrorService _errorService { get; set; }
        [Inject]
        private NavigationManager NavManager { get; set; }
        [Inject]
        public IComponentManagerService _apiService { get; set; }
        [Inject]
        public IUtilsService _UtilsService { get; set; }
        

        public bool ChangeFieldSigningName { get; set; } = false;
        public string[] ValidCAs { get; set; }
        public string Base64DocSigned { get; set; }
        public PreSignedDto PreSignedDto { get; set; } = new();
        public byte[] BytesImageGrafic { get; set; }
        public List<DragItem> AlldragItemsEnableList { get; set; } = new List<DragItem>();
        public List<DragItem> dragItemsList { get; set; } = new List<DragItem>();
        public int NumberPage { get; set; } = 1;


        protected async override void OnInitialized()
        {
            await this.LoadAllDragItemsEnable();
            await this.loadDragItem("drag-item-rubric");
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await _jsRuntime.InvokeVoidAsync("loadDraggerFunctionality", ".drag-item-rubric");
            await _jsRuntime.InvokeVoidAsync("loadDraggerFunctionality", ".drag-item-grafic");
        }
        protected async override void OnParametersSet()
        {
            base.OnParametersSet();

            if (Filename is not null)
            {
                PreSignedDto.PdfToSign = null;
                await _jsRuntime.InvokeVoidAsync("CleanPositions");
                var response = await _apiStoreFilesService.GetPendingFile(Filename, "pades");

                if (response.Item1)
                {
                    PreSignedDto.PdfToSign = response.Item2;
                    StateHasChanged();
                }
                else
                {
                    await _swalService.FireAsync("Error", "Se presento un inconveniente cargando el archivo " + Filename, "error");
                    ReloadPage();
                }
            }
        }

        private async Task SignDocument()
        {
            try
            {
                await _jsRuntime.InvokeVoidAsync("setLoadingSigningButton");
                string pem = await _jsRuntime.InvokeAsync<string>("getPemSelected");

                var infoCertificate = await _jsRuntime.InvokeAsync<string[]>("getInformationCertificate");

                var commonName = infoCertificate[0];
                var locality = infoCertificate[1];
                var state = infoCertificate[2];
                var countryName = infoCertificate[3];
                var organizationUnit = infoCertificate[4];
                var streetAddress = infoCertificate[5];

                if (locality is null)
                    locality = state + " - " + countryName;

                this.PreSignedDto.Pem = pem;
                this.PreSignedDto.InfoCertificate = new InfoCertificateDto();
                this.PreSignedDto.InfoCertificate.Location = commonName + " - " + locality;
                this.PreSignedDto.InfoCertificate.Reason = organizationUnit + " / " + streetAddress;
                this.PreSignedDto.InfoCertificate.DataCertificate = commonName + "\n" + locality + "\n" + countryName + "\n" + organizationUnit + "\n" + streetAddress + " / " + state + "\n" + " [DATE]";

                var strInfoPositions = await _jsRuntime.InvokeAsync<string>("GetPositionSignature");
                var strInfoPositionsGrafic = await _jsRuntime.InvokeAsync<string>("GetPositionGrafic");

                if (strInfoPositions is not null)
                {
                    var infoPositions = JsonConvert.DeserializeObject<AxisPosition>(strInfoPositions);
                    this.PreSignedDto.Position = infoPositions;
                    this.PreSignedDto.Position.NumberPage = NumberPage;
                    this.PreSignedDto.Visible = true;
                }
                else
                    this.PreSignedDto.Visible = false;

                if (strInfoPositionsGrafic is not null && BytesImageGrafic is not null)
                {
                    var infoPositionsGrafic = JsonConvert.DeserializeObject<StoreFiles.Core.Entities.AxisPosition.AxisPosition>(strInfoPositionsGrafic);
                    infoPositionsGrafic.NumberPage = NumberPage;
                    var pdfWithImage = _UtilsService.WriteImageInPDFCordinates(Convert.FromBase64String(this.PreSignedDto.PdfToSign), BytesImageGrafic, 140, 100, infoPositionsGrafic);
                    this.PreSignedDto.PdfToSign = Convert.ToBase64String(pdfWithImage);

                }

                var response = _apiService.GenerateDataToSign(PreSignedDto, ChangeFieldSigningName);

                if (response.bs64DataToSign is not null)
                {
                    var cms = await _jsRuntime.InvokeAsync<string>("generateCMSToEmbed", Convert.FromBase64String(response.bs64DataToSign));
                    var embedSignatureDto = new EmbedSignatureDto() { PdfFilePrepared = response.bs64PdfPrepared, Signature = cms };
                    var responseSign = _apiService.EmbedSignature(embedSignatureDto);

                    if (!string.IsNullOrEmpty(responseSign.message) && !string.IsNullOrEmpty(responseSign.fileSigned))
                    {
                        Base64DocSigned = responseSign.fileSigned;
                         await _apiStoreFilesService.PostSignedFile(new PostFileSignedDto { PdfGuid = Filename, PdfSignedBase64 = Base64DocSigned }, "pades");

                        await _jsRuntime.InvokeVoidAsync("setVisibleBtnDownloadDoc");
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Client cipher is not initialized") || ex.Message.Contains("Socket connection is not open"))
                {
                    await SignDocument();
                }
                else if (ex.Message.Contains("Field has been already signed."))
                {
                    ChangeFieldSigningName = true;
                    await SignDocument();
                }
                else if (ex.Message.Contains("A task was canceled."))
                {
                    await _swalService.FireAsync("Error", "Se ha excedido el tiempo limite de espera.", "error");
                }
                else
                {
                    _errorService.ProcessError(ex);
                    await _jsRuntime.InvokeVoidAsync("cleanLocalStorage");
                }

                StateHasChanged();
            }
        }

        private async void ReloadPage()
        {
            await _jsRuntime.InvokeVoidAsync("cleanStoragePageDdlSigner");
            NavManager.NavigateTo(NavManager.Uri, forceLoad: true);
        }

        public async void DownloadDoc()
        {
            await _jsRuntime.InvokeVoidAsync("downloadDocFile", Base64DocSigned);
        }

        private async Task HandleFileSelected(InputFileChangeEventArgs e)
        {
            var file = e.File;

            if (file != null)
            {
                if (e.File.ContentType == "image/jpeg")
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await file.OpenReadStream().CopyToAsync(memoryStream);
                        BytesImageGrafic = memoryStream.ToArray();
                        this.dragItemsList.Add(new() { Id = "drag-item-grafic", Class = "drag-item drag-item-grafic", Name = "Grafo", UrlIcon = "./images/graphic.png" });
                        StateHasChanged();
                    }
                } else
                {
                    await _swalService.FireAsync("Info", "La imagen debe ser en formato .JPG", "info");
                }
            }
        }

        private async Task LoadAllDragItemsEnable()
        {
            this.AlldragItemsEnableList.Add(new() { Id = "drag-item-rubric", Class = "drag-item drag-item-rubric", Name = "Rúbrica", UrlIcon = "./images/signature.png" });
            this.AlldragItemsEnableList.Add(new() { Id = "drag-item-grafic", Class = "drag-item drag-item-grafic", Name = "Grafo", UrlIcon = "./images/graphic.png" });
        }

        private async Task loadDragItem(string dragItemId)
        {
            this.dragItemsList.Add(this.AlldragItemsEnableList.Find(x => x.Id == dragItemId));
            StateHasChanged();
        }

        private async Task DeleteDragitem(string dragItemId)
        {
            this.dragItemsList.Remove(this.dragItemsList.Find(x => x.Id == dragItemId));

            if (dragItemId == "drag-item-rubric")
                await _jsRuntime.InvokeVoidAsync("CleanSignaturePosition");
            else
                await _jsRuntime.InvokeVoidAsync("CleanGraficPosition");

            StateHasChanged();

            await this.loadDragItem(dragItemId);
        }

        private void OnPageChanged(PdfViewerEventArgs args)
        {
            if(args.CurrentPage > 0)
            {
                NumberPage = args.CurrentPage;
            }
        }
    }
}
