using System.ComponentModel.DataAnnotations;

namespace StoreFiles.Core.DTOs.Cades
{
    public class CadesFileDto
    {
        [Required(ErrorMessage = "Debe ingresar el archivo a firmar")]
        public byte[] File { get; set; }
    }
}
