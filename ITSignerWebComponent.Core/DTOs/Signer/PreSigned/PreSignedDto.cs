using ITSignerWebComponent.Core.DTOs.Signer.InfoCertificate;
using SignerPDF.DigitalSignature.Core.Domain.AxisPosition;
using System.ComponentModel.DataAnnotations;

namespace ITSignerWebComponent.Core.DTOs.Signer.PreSigned
{
    public class PreSignedDto
    {
        //[Required(ErrorMessage = "Debe seleccionar el documento a firmar")]
        public string PdfToSign { get; set; }
        public string Pem { get; set; }
        public bool Visible { get; set; }
        public InfoCertificateDto InfoCertificate { get; set; }
        public AxisPosition Position { get; set; }
    }
}
