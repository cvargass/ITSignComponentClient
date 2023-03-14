using CurrieTechnologies.Razor.SweetAlert2;
using ITSignerWebComponent.SignApp.APIStoreFiles;
using ITSignerWebComponent.SignApp.Responses.APIStoreResponses;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using StoreFiles.Core.DTOs.Cades;
using System.IO;

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
    }
}
