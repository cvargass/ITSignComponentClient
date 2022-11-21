namespace StoreFiles.Core.DTOs.Sign
{
    public class SignDto
    {
        public byte[] File { get; set; }
        public int? IdSignPosition { get; set; }
        public bool SignatureVisible { get; set; } = true;
    }
}
