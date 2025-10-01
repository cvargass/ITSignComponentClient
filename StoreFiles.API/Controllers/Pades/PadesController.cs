using Microsoft.AspNetCore.Mvc;
using StoreFiles.Core.DTOs.PostFile;
using StoreFiles.Core.DTOs.PostFileSigned;
using StoreFiles.Core.DTOs.Sign;
using StoreFiles.Core.DTOs.UpdateFilePending;
using StoreFiles.Core.QueryFilters;
using StoreFiles.Core.Services.Sign;
using StoreFiles.Core.Services.StoreFiles;
using System;

namespace StoreFiles.API.Controllers.StoreFiles
{
    [Route("api/pades")]
    [ApiController]
    public class PadesController : ControllerBase
    {
        private readonly IStoreFileService _storeFileService;
        private readonly ISignService _signService;
        public PadesController(IStoreFileService storeFileService, ISignService signService)
        {
            _storeFileService = storeFileService;
            _signService = signService;
        }

        [HttpPost]
        public IActionResult PostFile(PostFileDto postFileDto)
        {
            if (postFileDto.IdApp != 0 && postFileDto.IdUser != 0)
            {
                string guidFileName = _storeFileService.StoreFile(postFileDto, "[PADES]");

                return Ok(new { guidFileName });
            }
            else
                return BadRequest(new { Error = "Debe indicar el id de usuario y el id de aplicación." });
        }

        [HttpGet]
        public IActionResult GetFiles([FromQuery]PendingFileQueryFilter pendingFileQueryFilter)
        {
            string[] fileNames = _storeFileService.GetPendingFiles(pendingFileQueryFilter, "[PADES]");

            if (fileNames.Length > 0)
                return Ok(new { Message = "¡ Archivos Pendientes !", PendingFiles = fileNames });
            else
                return Ok(new { Message = "No existen archivos pendientes", PendingFiles = new string[0] });
        }

        [HttpGet("pending-file/{guidFile}")]
        public IActionResult GetFile(string guidFile)
        {
            var response = _storeFileService.GetFile(guidFile, "[PADES]");

            if (response.flag)
                return Ok(new { Message = "¡ Archivo Pendiente !", FileBase64 = Convert.ToBase64String(response.bytesFile) });
            else
                return Ok(new { Message = "El archivo consultado no existe" });
        }

        [HttpGet("signed-file/{guidFile}")]
        public IActionResult GetSignedFile(string guidFile)
        {
            var response = _storeFileService.GetFile(guidFile, "[PADES]", true);

            if (response.flag)
                return Ok(new { Message = "¡ Archivo Firmado !", FileBase64 = Convert.ToBase64String(response.bytesFile) });
            else
                return Ok(new { Message = "El archivo consultado no existe" });
        }

        [RequestSizeLimit(104857600)] // 100 MB
        [HttpPost("upload-file-signed")]
        public IActionResult PostFileSigned(PostFileSignedDto postFileSignedDto)
        {
            _storeFileService.StoreFileSigned(postFileSignedDto, "[PADES]");

            return Ok(new { Message = "¡ Archivo firmado almacenado correctamente !" });
        }

        [RequestSizeLimit(104857600)] // 100 MB
        [HttpPut("upload-file-pending")]
        public IActionResult UpdateFilePending(UpdateFilePendingDto updateFilePendingDto)
        {
            _storeFileService.UpdateFilePending(updateFilePendingDto, "[PADES]");

            return Ok(new { Message = "¡ Archivo firmado almacenado correctamente !" });
        }

        [HttpPost("sign")]
        public IActionResult SignFile(SignDto signDto)
        {
            var signedFile = _signService.SignFile(signDto);
            return Ok(new { message = "Documento Firmado Exitosamente", signedFile });
        }
    }
}
