using System.ComponentModel.DataAnnotations;

namespace StoreFiles.Core.DTOs.FileForSigning
{
    public class FileForSigningDto
    {
        [Required(ErrorMessage = "Debe ingresar el archivo a firmar")]
        public byte[] File { get; set; }
    }
}
