using System.ComponentModel.DataAnnotations;

namespace ITSignerWebComponent.Core.DTOs.License
{
    public class LicenseDto
    {
        [Required(ErrorMessage = "Debe ingresar el código de Licencia")]
        public string License { get; set; }
    }
}
