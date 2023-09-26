using iText.Kernel.Pdf.Canvas.Parser.Data;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using iText.Kernel.Pdf.Canvas.Parser;
using System.Collections.Generic;
using iText.Kernel.Geom;
using StoreFiles.Core.Entities.TextChunkInfo;

namespace StoreFiles.Core.Services.Utils.TextLocationStrategy
{
    class TextLocationStrategy : LocationTextExtractionStrategy
    {
        public List<TextChunkInfo> objectResult = new List<TextChunkInfo>();

        public override void EventOccurred(IEventData data, EventType type)
        {
            if (!type.Equals(EventType.RENDER_TEXT))
                return;

            TextRenderInfo renderInfo = (TextRenderInfo)data;

            string curFont = renderInfo.GetFont().GetFontProgram().ToString();

            float curFontSize = renderInfo.GetFontSize();

            IList<TextRenderInfo> text = renderInfo.GetCharacterRenderInfos();
            foreach (TextRenderInfo t in text)
            {
                string letter = t.GetText();
                Vector letterStart = t.GetBaseline().GetStartPoint();
                Vector letterEnd = t.GetAscentLine().GetEndPoint();
                Rectangle letterRect = new Rectangle(letterStart.Get(0), letterStart.Get(1), letterEnd.Get(0) - letterStart.Get(0), letterEnd.Get(1) - letterStart.Get(1));

                if (letter != " " && !letter.Contains(' '))
                {
                    TextChunkInfo chunk = new TextChunkInfo();
                    chunk.Text = letter;
                    chunk.Rectangule = letterRect;
                    chunk.FontFamily = curFont;
                    chunk.FontSize = curFontSize;
                    chunk.SpaceWidth = t.GetSingleSpaceWidth() / 2f;

                    objectResult.Add(chunk);
                }
            }
        }
    }
}
