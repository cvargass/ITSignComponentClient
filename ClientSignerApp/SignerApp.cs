using ClientSignerApp.Controllers;
using ClientSignerApp.DTOs.Sign;
using ClientSignerApp.Services.APIStoreFiles;
using ClientSignerApp.Services.Logger;
using ClientSignerApp.Services.Signer;
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
        private bool _signatureVisible;
        private int _signaturePosition;

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
        private void SignerApp_Load(object sender, EventArgs e)
        {
            LoadSigningConfiguration();
            BeginSigningFiles();
        }

        private void LoadSigningConfiguration()
        {
            bool generateDefaultValues = false;
            if (_guidFiles is not null && _guidFiles.Length > 0)
            {
                var lastParameter = _guidFiles.ToArray()[_guidFiles.Length - 1];

                if (lastParameter.Contains("|"))
                {
                    var configParams = lastParameter.Split('|');
                    _signatureVisible = configParams[0].ToLower() == "true" ? true : false;
                    _signaturePosition = int.Parse(configParams[1]);
                    _guidFiles = _guidFiles.Take(_guidFiles.Length - 1).ToArray(); //Elimina el último valor que es configuracion
                }
                else
                    generateDefaultValues = true;

            } else
                generateDefaultValues = true;

            if (generateDefaultValues)
            {
                _signatureVisible = _configuration["SignConfigurations:SignatureVisible"] is not null ? Convert.ToBoolean(_configuration["SignConfigurations:SignatureVisible"]) : true;
                _signaturePosition = _configuration["SignConfigurations:SignDefaultPosition"] is not null ? Convert.ToInt32(_configuration["SignConfigurations:SignDefaultPosition"]) : 6;
            }
        }

        private async void BeginSigningFiles()
        {
            foreach (var guidFile in _guidFiles)
            {
                var strFile = await GetFile(guidFile);

                if (!string.IsNullOrEmpty(strFile))
                {
                    byte[] signedFile = _signerService.SignWithSmartCard(Convert.FromBase64String(strFile), guidFile, _signatureVisible, _signaturePosition);

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

            CloseApplication();
        }

        private void CloseApplication()
        {
            Application.Exit();
        }
    }
}
