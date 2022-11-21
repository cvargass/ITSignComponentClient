namespace StoreFiles.Core.Options
{
    public class SignOptions
    {
        public string SerialNumber { get; set; }
        public int SignDefaultPosition { get; set; }
        public string UrlSignCertificate { get; set; }
        public string PasswordCertificate { get; set; }
        public string SigningReason { get; set; }
        public string SigningLocation { get; set; }
    }
}
