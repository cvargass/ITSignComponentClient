using Microsoft.AspNetCore.Mvc;
using StoreFiles.Core.DTOs.Grafic;
using StoreFiles.Core.Services.GraficSigning;
using System;

namespace StoreFiles.API.Controllers.Grafics
{
    [Route("api/[controller]")]
    [ApiController]
    public class GraficsController : ControllerBase
    {
        private readonly IGraficSigningService _graficSigningService;

        public GraficsController(IGraficSigningService graficSigningService)
        {
            _graficSigningService = graficSigningService;
        }

        [HttpPost]
        public IActionResult PostGrafic(PostGraficDto postGraficDto)
        {
            string guidFile = _graficSigningService.StoreGrafic(postGraficDto);

            return Ok(new { guidFile });
        }

        [HttpGet("{guid}")]
        public IActionResult GetGrafic(string guid)
        {
            var response = _graficSigningService.GetGrafic(guid);

            if (response.flag)
                return Ok(new { Message = "¡ Gráfico Descargado !", FileBase64 = Convert.ToBase64String(response.bytesFile) });
            else
                return Ok(new { Message = "El gráfico consultado no existe" });
        }
    }
}
