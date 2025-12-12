using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Linq;

namespace StoreFiles.API.Controllers.Params
{
    [Route("api/[controller]")]
    [ApiController]
    public class ParamsController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public ParamsController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet("{paramKey}")]
        public IActionResult GetParam(string paramKey)
        {
            var paramValue = _configuration.GetSection(paramKey);
            if (paramValue is not null && paramValue.GetChildren().Count() > 0)
            {
                if (paramKey.Equals("TimeStamping"))
                {
                    string urlTsaServer = paramValue["TSAServer"];
                    string user = paramValue["User"];
                    string password = paramValue["Password"];

                    return Ok(new { urlTsaServer, user, password });
                }
                
                return Ok(new { ParamKey = paramKey, ParamValue = paramValue });
            }
            else
            {
                return NotFound(new { Message = $"El parámetro con clave '{paramKey}' no fue encontrado." });
            }
        }
    }
}
