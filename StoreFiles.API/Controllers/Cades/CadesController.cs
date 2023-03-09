using Microsoft.AspNetCore.Mvc;
using StoreFiles.Core.DTOs.Cades;
using StoreFiles.Core.Services.Cades;

namespace StoreFiles.API.Controllers.Cades
{
    [Route("api/cades")]
    [ApiController]
    public class CadesController : ControllerBase
    {
        private readonly ICadesService _cadesService;

        public CadesController(ICadesService cadesService)
        {
            _cadesService = cadesService;
        }

        [HttpPost("sign")]
        public IActionResult SignFile(CadesFileDto cadesFileDto)
        {
            var signedFile = _cadesService.SignCadesFile(cadesFileDto);

            return Ok(new { Message = "¡ Archivo Firmado ! Recuerde que el archivo debe tener la extensión .p7z", signedFile });
        }
    }
}
