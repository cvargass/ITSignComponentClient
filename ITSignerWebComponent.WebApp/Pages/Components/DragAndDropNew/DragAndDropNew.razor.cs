using BlazorBootstrap;
using CurrieTechnologies.Razor.SweetAlert2;
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
using StoreFiles.Core.DTOs.UpdateFilePending;
using StoreFiles.Core.Services.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ITSignerWebComponent.SignApp.Pages.Components.DragAndDropNew
{
    public partial class DragAndDropNew
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
        public bool IsBtnSigningDisabled { get; set; } = false;


        protected async override void OnInitialized()
        {
            await this.LoadAllDragItemsEnable();
            await this.loadDragItem("drag-item-rubric");
            await _jsRuntime.InvokeVoidAsync("clearGuidPendingSigning");
            await _jsRuntime.InvokeVoidAsync("clearFileSigned");
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
                await _jsRuntime.InvokeVoidAsync("setGuidPendingSigning", Filename);

                IsBtnSigningDisabled = true;

                //SignaturePosition
                var strInfoPositions = await _jsRuntime.InvokeAsync<string>("GetPositionSignature");
                AxisPosition position = new AxisPosition() { CoordinateX = 10, CoordinateY = 10 }; // Default position if none is provided
                if (strInfoPositions is not null)
                {
                    var infoPositions = JsonConvert.DeserializeObject<AxisPosition>(strInfoPositions);
                    position = infoPositions;
                }

                //GraficPosition
                var strInfoPositionsGrafic = await _jsRuntime.InvokeAsync<string>("GetPositionGrafic");
                if (strInfoPositionsGrafic is not null && BytesImageGrafic is not null)
                {
                    var infoPositionsGrafic = JsonConvert.DeserializeObject<StoreFiles.Core.Entities.AxisPosition.AxisPosition>(strInfoPositionsGrafic);
                    infoPositionsGrafic.NumberPage = NumberPage;
                    var pdfWithImage = _UtilsService.WriteImageInPDFCordinates(Convert.FromBase64String(this.PreSignedDto.PdfToSign), BytesImageGrafic, 140, 100, infoPositionsGrafic);
                    await _apiStoreFilesService.UpdatePendingFile(new UpdateFilePendingDto { PdfGuid = Filename, PdfSignedBase64 = Convert.ToBase64String(pdfWithImage) }, "pades");
                }
                
                var parameters = this.GenerateParameters(position);

                var result = await _jsRuntime.InvokeAsync<string>(
                    "launchAppWithProtocol",
                    "appsigner",  // ProtocolName
                    parameters
                );

                //int seconds = 6;
                //await ReloadPageAfterDelay(seconds);
            }
            catch (Exception ex)
            {
                await _swalService.FireAsync("Error", "Ha ocurrido un error cargando y firmando el archivo", "error");
                ReloadPage();
            }
        }

        private string GenerateParameters(AxisPosition position)
        {
            // ESTRUCTURE: type_file1_page|x|y       Must specify the position of the signature
            string parameters = "individual_";
            parameters += Filename + "_";
            parameters += NumberPage + "|";
            parameters += position.CoordinateX + "|";
            parameters += position.CoordinateY;

            return parameters.Trim();
        }

        private async Task ReloadPageAfterDelay(int seconds)
        {
            await Task.Delay(seconds * 1000); // Convert seconds to milliseconds
            NavManager.NavigateTo(NavManager.Uri, forceLoad: true);
        }

        private async void ReloadPage()
        {
            await _jsRuntime.InvokeVoidAsync("cleanStoragePageDdlSigner");
            NavManager.NavigateTo(NavManager.Uri, forceLoad: true);
        }

        public async void DownloadDoc()
        {
            await _jsRuntime.InvokeVoidAsync("downloadDocSignedFile");
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
