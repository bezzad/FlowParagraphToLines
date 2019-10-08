using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace SvgTextViewer
{
    public abstract class Word
    {
        protected Word(string text, int offset)
        {
            Text = text;
            OffsetRange = new Range(offset, offset + text.Length - 1);
            ImpressivePaddingPercent = 0.2; // 20% of word length
            NextSpaceWidth = 5;
            Styles = new Dictionary<StyleType, InlineStyle>();
        }

        public FormattedText Format { get; set; }
        public Point DrawPoint { get; set; }
        public Rect Area { get; set; }
        public Range OffsetRange { get; set; }
        public Dictionary<StyleType, InlineStyle> Styles { get; set; }
        public int NextSpaceWidth { get; set; }
        public double ImpressivePaddingPercent { get; set; }
        public string Text { get; set; }

        public bool IsRtl => Styles[StyleType.Direction].Value == "rtl";
        public int Offset => OffsetRange.Start;
        public double Width => Area.Size.Width;
        public double Height => Area.Size.Height;
    }
}
