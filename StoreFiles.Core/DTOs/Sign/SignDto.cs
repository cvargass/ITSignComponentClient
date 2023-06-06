using StoreFiles.Core.Entities.AxisPosition;

namespace StoreFiles.Core.DTOs.Sign
{
    public class SignDto
    {
        public byte[] File { get; set; }
        public int? IdSignPosition { get; set; }
        public bool SignatureVisible { get; set; } = true;
        public string UrlSignCertificate { get; set; }
        public string PasswordCertificate { get; set; }
        public AxisPosition SignPosition { get; set; }
        public AxisPosition QRPosition { get; set; }
    }
}
