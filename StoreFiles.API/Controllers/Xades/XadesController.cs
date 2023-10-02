using Microsoft.AspNetCore.Mvc;
using StoreFiles.Core.DTOs.FileForSigning;
using StoreFiles.Core.DTOs.PostFile;
using StoreFiles.Core.DTOs.PostFileSigned;
using StoreFiles.Core.QueryFilters;
using StoreFiles.Core.Services.StoreFiles;
using StoreFiles.Core.Services.Xades;
using System;

namespace StoreFiles.API.Controllers.Xades
{
    [Route("api/xades")]
    [ApiController]
    public class XadesController : ControllerBase
    {
        private readonly IXadesService _xadesService;
        private readonly IStoreFileService _storeFileService;

        public XadesController(IXadesService xadesService, IStoreFileService storeFileService)
        {
            _xadesService = xadesService;
            _storeFileService = storeFileService;
        }

        [HttpPost]
        public IActionResult PostFile(PostFileDto postFileDto)
        {
            if (postFileDto.IdApp != 0 && postFileDto.IdUser != 0)
            {
                string guidFileName = _storeFileService.StoreFile(postFileDto, "[XADES]");

                return Ok(new { guidFileName });
            }
            else
                return BadRequest(new { Error = "Debe indicar el id de usuario y el id de aplicación." });
        }

        [HttpGet]
        public IActionResult GetFiles([FromQuery] PendingFileQueryFilter pendingFileQueryFilter)
        {
            string[] fileNames = _storeFileService.GetPendingFiles(pendingFileQueryFilter, "[XADES]");

            if (fileNames.Length > 0)
                return Ok(new { Message = "¡ Archivos Pendientes !", PendingFiles = fileNames });
            else
                return Ok(new { Message = "No existen archivos pendientes", PendingFiles = new string[0] });
        }

        [HttpGet("pending-file/{guidFile}")]
        public IActionResult GetFile(string guidFile)
        {
            var response = _storeFileService.GetFile(guidFile, "[XADES]");

            if (response.flag)
                return Ok(new { Message = "¡ Archivo Pendiente !", FileBase64 = Convert.ToBase64String(response.bytesFile) });
            else
                return Ok(new { Message = "El archivo consultado no existe" });
        }

        [HttpGet("signed-file/{guidFile}")]
        public IActionResult GetSignedFile(string guidFile)
        {
            var response = _storeFileService.GetFile(guidFile, "[XADES]", true);

            if (response.flag)
                return Ok(new { Message = "¡ Archivo Firmado !", FileBase64 = Convert.ToBase64String(response.bytesFile) });
            else
                return Ok(new { Message = "El archivo consultado no existe" });
        }

        [HttpPost("sign")]
        public IActionResult SignFile(FileForSigningDto fileForSigningDto)
        {
            var signedFile = _xadesService.SignXadesFile(fileForSigningDto);

            return Ok(new { Message = "¡ Archivo Firmado Correctamente!", signedFile });
        }

        [HttpPost("upload-file-signed")]
        public IActionResult PostFileSigned(PostFileSignedDto postFileSignedDto)
        {
            _storeFileService.StoreFileSigned(postFileSignedDto, "[XADES]");

            return Ok(new { Message = "¡ Archivo firmado y almacenado correctamente !" });
        }
    }
}
