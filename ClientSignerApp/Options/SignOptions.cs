namespace ClientSignerApp.Options
{
    public class SignOptions
    {
        public string SerialNumber { get; set; }
        public int SignDefaultPosition { get; set; }
        public string SigningReason { get; set; }
        public string SigningLocation { get; set; }
        public bool SignatureVisible { get; set; }
    }
}
