using iText.Kernel.Geom;

namespace StoreFiles.Core.Entities.TextChunkInfo
{
    public class TextChunkInfo
    {
        public string Text { get; set; }
        public Rectangle Rectangule { get; set; }
        public string FontFamily { get; set; }
        public float FontSize { get; set; }
        public float SpaceWidth { get; set; }
    }
}
