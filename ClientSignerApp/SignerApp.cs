using ClientSignerApp.Controllers;
using ClientSignerApp.DTOs.Sign;
using ClientSignerApp.Services.APIStoreFiles;
using ClientSignerApp.Services.Logger;
using ClientSignerApp.Services.Signer;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;

namespace ClientSignerApp
{
    public partial class SignerApp : Form
    {
        private string[]? _guidFiles;
        private FilesController filesController;
        private readonly ILoggerService _loggerService;
        private readonly ISignerService _signerService;
        private readonly IConfiguration _configuration;
        private string SigningMode;
        private string SignaturePosition;

        public SignerApp(IAPIStoreFilesService APIStoreFilesService,
                         ILoggerService loggerService,
                         ISignerService signerService,
                         IConfiguration configuration)
        {
            InitializeComponent();
            filesController = new FilesController(configuration);
            _loggerService = loggerService;
            _signerService = signerService;
            _configuration = configuration;
        }


        public void SetGuidFiles(string[] guidFile)
        {
            _guidFiles = guidFile;
        }

        private async Task<string> GetFile(string guidFile)
        {
            string strFile = String.Empty;
            var fileResponse = await filesController.GetPendingFile(guidFile);

            if (fileResponse is not null)
            {
                if (fileResponse.FileBase64 is not null)
                {
                    strFile = fileResponse.FileBase64;
                } else
                {
                    _loggerService.LogInformation($"{fileResponse.Message} - Identificativo: {guidFile}");
                }
            }
            else
            {
                _loggerService.LogInformation($"No se pudo obtener el archivo identificador {guidFile} para firmar.");
            }

            return strFile;
        }
        public async Task SignDocument()
        {
            await LoadSigningConfiguration();

            if (this.SigningMode == "massive")
            {
                await BeginSigningFiles();
            }
            else if (this.SigningMode == "individual")
            {
                await BeginSigningFile();
            } else
            {
               await CloseApplication();
            }
        }

        private async Task LoadSigningConfiguration()
        {
            if (this.SigningMode == "massive")
            {
                //ESTRUCTURE type_file1_file2_file3
                //Remove first value {signigMode}
                Range range = new Range(1, _guidFiles.Length);
                _guidFiles = _guidFiles.Take(range).ToArray();
            } 
            else if (this.SigningMode == "individual")
            {
                //ESTRUCTURE type_file1_page|x|y
                //Remove first and last value {signigMode...signaturePosition}
                Range range = new Range(1, _guidFiles.Length - 1);
                this.SignaturePosition = _guidFiles[_guidFiles.Length - 1];
                _guidFiles = _guidFiles.Take(range).ToArray();
            } else
            {
                await CloseApplication();
            }
        }

        private async Task BeginSigningFiles()
        {
            foreach (var guidFile in _guidFiles)
            {
                var strFile = await GetFile(guidFile);

                if (!string.IsNullOrEmpty(strFile))
                {
                    byte[] signedFile = _signerService.SignWithSmartCard(Convert.FromBase64String(strFile), guidFile);

                    if (signedFile is not null)
                    {
                        PostFileSignedDto postFileSignedDto = new PostFileSignedDto
                        {
                            PdfSignedBase64 = Convert.ToBase64String(signedFile),
                            PdfGuid = guidFile
                        };
                        var fileResponse = await filesController.PostSignedFile(postFileSignedDto);
                    }
                }
            }

            await CloseApplication();
        }

        private async Task BeginSigningFile()
        {
            foreach (var guidFile in _guidFiles)
            {
                var strFile = await GetFile(guidFile);

                if (!string.IsNullOrEmpty(strFile))
                {
                    byte[] signedFile = _signerService.SignFile(Convert.FromBase64String(strFile), guidFile, this.SignaturePosition);

                    if (signedFile is not null)
                    {
                        PostFileSignedDto postFileSignedDto = new PostFileSignedDto
                        {
                            PdfSignedBase64 = Convert.ToBase64String(signedFile),
                            PdfGuid = guidFile
                        };
                        var fileResponse = await filesController.PostSignedFile(postFileSignedDto);
                    }
                }
            }

            await CloseApplication();
        }

        private async Task CloseApplication()
        {
            await this.SendNotificationReloadPage();
            Application.Exit();
        }

        private async Task SendNotificationReloadPage()
        {
            try
            {
                var url = _configuration["PageFiles:Url"] + "/notifyHub";
                var connection = new HubConnectionBuilder()
                .WithUrl(url)
                .Build();

                await connection.StartAsync();
                await connection.InvokeAsync("ReloadPage");
                await connection.DisposeAsync();
            }
            catch (Exception ex)
            {
                _loggerService.LogError(ex.Message);
            }
        }

        internal void SetSigningType(string type)
        {
            this.SigningMode = type;
        }
    }
}
