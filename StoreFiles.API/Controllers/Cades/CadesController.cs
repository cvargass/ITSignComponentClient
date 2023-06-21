using Microsoft.AspNetCore.Mvc;
using StoreFiles.Core.DTOs.Cades;
using StoreFiles.Core.DTOs.PostFile;
using StoreFiles.Core.DTOs.PostFileSigned;
using StoreFiles.Core.QueryFilters;
using StoreFiles.Core.Services.Cades;
using StoreFiles.Core.Services.StoreFiles;
using System;

namespace StoreFiles.API.Controllers.Cades
{
    [Route("api/cades")]
    [ApiController]
    public class CadesController : ControllerBase
    {
        private readonly ICadesService _cadesService;
        private readonly IStoreFileService _storeFileService;

        public CadesController(ICadesService cadesService, IStoreFileService storeFileService)
        {
            _cadesService = cadesService;
            _storeFileService = storeFileService;
        }

        [HttpPost("files")]
        public IActionResult PostFile(PostFileDto postFileDto)
        {
            if (postFileDto.IdApp != 0 && postFileDto.IdUser != 0)
            {
                string guidFileName = _storeFileService.StoreFileCades(postFileDto);

                return Ok(new { guidFileName });
            }
            else
                return BadRequest(new { Error = "Debe indicar el id de usuario y el id de aplicación." });
        }

        [HttpGet("files")]
        public IActionResult GetFiles([FromQuery] PendingFileQueryFilter pendingFileQueryFilter)
        {
            string[] fileNames = _storeFileService.GetCadesPendingFiles(pendingFileQueryFilter);

            if (fileNames.Length > 0)
                return Ok(new { Message = "¡ Archivos Pendientes !", PendingFiles = fileNames });
            else
                return Ok(new { Message = "No existen archivos pendientes", PendingFiles = new string[0] });
        }

        [HttpGet("pending-file/{guidFile}")]
        public IActionResult GetFile(string guidFile)
        {
            var response = _storeFileService.GetCadesFile(guidFile);

            if (response.flag)
                return Ok(new { Message = "¡ Archivo Pendiente !", FileBase64 = Convert.ToBase64String(response.bytesFile) });
            else
                return Ok(new { Message = "El archivo consultado no existe" });
        }

        [HttpGet("signed-file/{guidFile}")]
        public IActionResult GetSignedFile(string guidFile)
        {
            var response = _storeFileService.GetCadesFile(guidFile, true);

            if (response.flag)
                return Ok(new { Message = "¡ Archivo Firmado !", FileBase64 = Convert.ToBase64String(response.bytesFile) });
            else
                return Ok(new { Message = "El archivo consultado no existe" });
        }

        [HttpPost("sign")]
        public IActionResult SignFile(CadesFileDto cadesFileDto)
        {
            var signedFile = _cadesService.SignCadesFile(cadesFileDto);

            return Ok(new { Message = "¡ Archivo Firmado ! Recuerde que el archivo debe tener la extensión .p7z", signedFile });
        }

        [HttpPost("upload-file-signed")]
        public IActionResult PostFileSigned(PostFileSignedDto postFileSignedDto)
        {
            _storeFileService.StoreCadesFileSigned(postFileSignedDto);

            return Ok(new { Message = "¡ Archivo firmado almacenado correctamente !" });
        }
    }
}
