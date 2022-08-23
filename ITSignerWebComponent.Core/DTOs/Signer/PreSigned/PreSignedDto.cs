using System.ComponentModel.DataAnnotations;

namespace ITSignerWebComponent.Core.DTOs.Signer.PreSigned
{
    public class PreSignedDto
    {
        //[Required(ErrorMessage = "Debe seleccionar el documento a firmar")]
        public string PdfToSign { get; set; }
        public string Pem { get; set; }
    }
}
