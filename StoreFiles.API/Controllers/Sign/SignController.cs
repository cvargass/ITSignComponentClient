using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StoreFiles.Core.DTOs.Sign;
using StoreFiles.Core.Services.Sign;

namespace StoreFiles.API.Controllers.Sign
{
    [Route("api/[controller]")]
    [ApiController]
    public class SignController : ControllerBase
    {
        private readonly ISignService _signService;
        public SignController(ISignService signService)
        {
            _signService = signService;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult SignFile(SignDto SignDto)
        {
            var signedFile = _signService.SignFile(SignDto);
            return Ok(new { message = "Documento Firmado Exitosamente", signedFile });
        }
    }
}
