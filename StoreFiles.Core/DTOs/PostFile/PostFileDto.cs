namespace StoreFiles.Core.DTOs.PostFile
{
    public class PostFileDto
    {
        public string TypeSignature { get; set; }
        public string FilePdfBase64 { get; set; }
        public int IdUser { get; set; }
        public int IdApp { get; set; }
    }
}
