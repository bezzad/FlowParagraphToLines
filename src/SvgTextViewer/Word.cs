using System.Windows;
using System.Windows.Media;

namespace SvgTextViewer
{
    public abstract class Word
    {
        protected Word(FormattedText format, Point drawPoint, Rect area, int offset)
        {
            Format = format;
            DrawPoint = drawPoint;
            Area = area;
            OffsetRange = new Range(offset, offset + Text.Length - 1);
        }

        public FormattedText Format { get; set; }
        public Point DrawPoint { get; set; }
        public Rect Area { get; set; }
        public Range OffsetRange { get; set; }
        public double ImpressivePaddingPercent { get; set; } = 0.2; // 20% of word length

        public string Text => Format.Text;
        public int Offset => OffsetRange.Start;
        public double Width => Area.Size.Width;
        public double Height => Area.Size.Height;
        public bool IsRtl => Format.FlowDirection == FlowDirection.RightToLeft;
    }
}
