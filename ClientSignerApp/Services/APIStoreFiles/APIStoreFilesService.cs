using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;

namespace ClientSignerApp.Services.APIStoreFiles
{
    public class APIStoreFilesService : IAPIStoreFilesService
    {
        private readonly HttpClient httpClient;
        private readonly string _domain;

        public APIStoreFilesService(HttpClient httpClient, IConfiguration configuration)
        {
            this.httpClient = httpClient;
            _domain = configuration["APIStoreFiles:BaseUrl"].ToString();
        }

        public async Task<(bool, string)> GetPendingFile(string guidFile)
        {
            string fileBase64 = null;
            bool flag = false;
            string url = GenerateUrl($"api/pades/pending-file/{guidFile}");

            try
            {
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
            }
            catch (Exception ex)
            {

                throw ex;
            }

           

            return (flag, fileBase64);
        }

        private string GenerateUrl(string endpointUrl)
        {
            string url = string.Empty;

            if (!string.IsNullOrEmpty(_domain))
                url = $"{_domain}/{endpointUrl}";
            else
                url = $"/{endpointUrl}";

            return url;
        }
    }

    public class FileResponse
    {
        public string Message { get; set; }
        public string FileBase64 { get; set; }
    }
}
