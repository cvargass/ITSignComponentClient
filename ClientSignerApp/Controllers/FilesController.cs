using ClientSignerApp.DTOs.Sign;
using ClientSignerApp.Services.APIStoreFiles;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;

namespace ClientSignerApp.Controllers
{
    public class FilesController
    {
        private HttpClient _httpClient;
        private readonly string? _baseUrl;

        public FilesController(IConfiguration configuration)
        {
            _httpClient = new HttpClient();
            _baseUrl = configuration["APIStoreFiles:BaseUrl"];
        }

        public async Task<FileResponse?> GetPendingFile(string guidFile)
        {
            string url = $"{_baseUrl}/api/pades/pending-file/{guidFile}";
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                if (response.IsSuccessStatusCode)
                {
                    var fileResponse = await response.Content.ReadFromJsonAsync<FileResponse>();
                    return fileResponse;
                }

                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<bool> PostSignedFile(PostFileSignedDto postFileSignedDto)
        {
            bool flag = false;
            string url = $"{_baseUrl}/api/pades/upload-file-signed";

            var response = await _httpClient.PostAsJsonAsync(url, postFileSignedDto);

            if (response.IsSuccessStatusCode)
                flag = true;

            return flag;
        }
    }
}
