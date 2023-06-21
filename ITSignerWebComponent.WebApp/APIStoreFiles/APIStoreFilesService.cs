using ITSignerWebComponent.SignApp.Responses.APIStoreResponses;
using Microsoft.Extensions.Configuration;
using Microsoft.JSInterop;
using StoreFiles.Core.DTOs.Cades;
using StoreFiles.Core.DTOs.PostFile;
using StoreFiles.Core.DTOs.PostFileSigned;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace ITSignerWebComponent.SignApp.APIStoreFiles
{
    public class APIStoreFilesService : IAPIStoreFilesService
    {
        private readonly HttpClient httpClient;
        private readonly IJSRuntime _jsRuntime;
        private readonly string _domain;

        public APIStoreFilesService(HttpClient httpClient, IJSRuntime jsRuntime, IConfiguration configuration)
        {
            this.httpClient = httpClient;
            _jsRuntime = jsRuntime;
            _domain = configuration["APIStoreFiles:Domain"].ToString();
        }

        public async Task<(bool, string[])> GetPendingFiles(int idUser, int idApp)
        {
            string[] fileNamesPending = null;
            string url = GenerateUrl($"api/Files?idUser={idUser}&idApp={idApp}");
            var response = await httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var responseService = await response.Content.ReadFromJsonAsync<PendingFilesResponse>();
                fileNamesPending = responseService.PendingFiles;
            }
            else
            {
                await _jsRuntime.InvokeVoidAsync("log", response.RequestMessage + " ReasonPhrase: " + response.ReasonPhrase + " StatusCode: " + response.StatusCode);
            }

            return (response.IsSuccessStatusCode, fileNamesPending);
        }

        public async Task<(bool, string[])> GetCadesPendingFiles(int idUser, int idApp)
        {
            string[] fileNamesPending = null;
            string url = GenerateUrl($"api/cades/files?idUser={idUser}&idApp={idApp}");
            var response = await httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var responseService = await response.Content.ReadFromJsonAsync<PendingFilesResponse>();
                fileNamesPending = responseService.PendingFiles;
            }
            else
            {
                await _jsRuntime.InvokeVoidAsync("log", response.RequestMessage + " ReasonPhrase: " + response.ReasonPhrase + " StatusCode: " + response.StatusCode);
            }

            return (response.IsSuccessStatusCode, fileNamesPending);
        }

        private string GenerateUrl(string endpointUrl)
        {
            string url = string.Empty;
            
            if (!string.IsNullOrEmpty(_domain))
                url = $"/{_domain}/{endpointUrl}";
            else
                url = $"/{endpointUrl}";

            return url;
        }

        public async Task<(bool, string)> GetPendingFile(string guidFile)
        {
            string fileBase64 = null;
            bool flag = false;
            string url = GenerateUrl($"api/Files/pending-file/{guidFile}");

            var response = await httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var responseService = await response.Content.ReadFromJsonAsync<FileResponse>();
                if (responseService.FileBase64 != null)
                {
                    fileBase64 = responseService.FileBase64;
                    flag = true;
                }
            }
            else
            {
                await _jsRuntime.InvokeVoidAsync("log", response.RequestMessage + " ReasonPhrase: " + response.ReasonPhrase + " StatusCode: " + response.StatusCode);
            }

            return (flag, fileBase64);
        }

        public async Task<(bool, string)> GetCadesPendingFile(string guidFile)
        {
            string fileBase64 = null;
            bool flag = false;
            string url = GenerateUrl($"api/cades/pending-file/{guidFile}");

            var response = await httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var responseService = await response.Content.ReadFromJsonAsync<FileResponse>();
                if (responseService.FileBase64 != null)
                {
                    fileBase64 = responseService.FileBase64;
                    flag = true;
                }
            }
            else
            {
                await _jsRuntime.InvokeVoidAsync("log", response.RequestMessage + " ReasonPhrase: " + response.ReasonPhrase + " StatusCode: " + response.StatusCode);
            }

            return (flag, fileBase64);
        }

        public async Task<(bool, string)> GetSignedFile(string guidFile)
        {
            string fileSignedBase64 = null;
            string url = GenerateUrl($"api/Files/signed-file/{guidFile}");

            var response = await httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var responseService = await response.Content.ReadFromJsonAsync<FileResponse>();
                fileSignedBase64 = responseService.FileBase64;
            }
            else
            {
                await _jsRuntime.InvokeVoidAsync("log", response.RequestMessage + " ReasonPhrase: " + response.ReasonPhrase + " StatusCode: " + response.StatusCode);
            }

            return (response.IsSuccessStatusCode, fileSignedBase64);
        }

        public async Task<(bool, string)> GetCadesSignedFile(string guidFile)
        {
            string fileSignedBase64 = null;
            string url = GenerateUrl($"api/cades/signed-file/{guidFile}");

            var response = await httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var responseService = await response.Content.ReadFromJsonAsync<FileResponse>();
                fileSignedBase64 = responseService.FileBase64;
            }
            else
            {
                await _jsRuntime.InvokeVoidAsync("log", response.RequestMessage + " ReasonPhrase: " + response.ReasonPhrase + " StatusCode: " + response.StatusCode);
            }

            return (response.IsSuccessStatusCode, fileSignedBase64);
        }

        public async Task<(bool, string)> PostFile(PostFileDto postFileDto)
        {
            string guidFile = string.Empty;
            string url = GenerateUrl("api/Files");

            var response = await httpClient.PostAsJsonAsync(url, postFileDto);

            if (response.IsSuccessStatusCode)
            {
                var tokenResponse = await response.Content.ReadFromJsonAsync<PostFileResponse>();
                guidFile = tokenResponse.GuidFile;
            }
            else
            {
                await _jsRuntime.InvokeVoidAsync("log", response.RequestMessage + " ReasonPhrase: " + response.ReasonPhrase + " StatusCode: " + response.StatusCode);
            }

            return (response.IsSuccessStatusCode, guidFile);
        }

        public async Task<(bool, string)> PostCadesFile(PostFileDto postFileDto)
        {
            string guidFile = string.Empty;
            string url = GenerateUrl("api/cades/files");

            var response = await httpClient.PostAsJsonAsync(url, postFileDto);

            if (response.IsSuccessStatusCode)
            {
                var tokenResponse = await response.Content.ReadFromJsonAsync<PostFileResponse>();
                guidFile = tokenResponse.GuidFile;
            }
            else
            {
                await _jsRuntime.InvokeVoidAsync("log", response.RequestMessage + " ReasonPhrase: " + response.ReasonPhrase + " StatusCode: " + response.StatusCode);
            }

            return (response.IsSuccessStatusCode, guidFile);
        }

        public async Task<bool> PostSignedFile(PostFileSignedDto postFileSignedDto)
        {
            bool flag = false;
            string url = GenerateUrl("api/Files/upload-file-signed");

            var response = await httpClient.PostAsJsonAsync(url, postFileSignedDto);

            if (response.IsSuccessStatusCode)
                flag = true;
            else
            {
                await _jsRuntime.InvokeVoidAsync("log", response.RequestMessage + " ReasonPhrase: " + response.ReasonPhrase + " StatusCode: " + response.StatusCode);
            }

            return flag;
        }

        public async Task<bool> PostCadesSignedFile(PostFileSignedDto postFileSignedDto)
        {
            bool flag = false;
            string url = GenerateUrl("api/cades/upload-file-signed");

            var response = await httpClient.PostAsJsonAsync(url, postFileSignedDto);

            if (response.IsSuccessStatusCode)
                flag = true;
            else
            {
                await _jsRuntime.InvokeVoidAsync("log", response.RequestMessage + " ReasonPhrase: " + response.ReasonPhrase + " StatusCode: " + response.StatusCode);
            }

            return flag;
        }

        public async Task<CadesFileResponse> SignCadesFile(CadesFileDto cadesFileDto)
        {
            string guidFile = string.Empty;
            string url = GenerateUrl("api/Cades/sign");
            CadesFileResponse cadesFileResponse = null;

            var response = await httpClient.PostAsJsonAsync(url, cadesFileDto);

            if (response.IsSuccessStatusCode)
            {
                cadesFileResponse = await response.Content.ReadFromJsonAsync<CadesFileResponse>();
            }
            else
            {
                await _jsRuntime.InvokeVoidAsync("log", response.RequestMessage + " ReasonPhrase: " + response.ReasonPhrase + " StatusCode: " + response.StatusCode);
            }

            return cadesFileResponse;
        }

    }
}
