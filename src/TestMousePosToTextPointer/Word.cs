using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Media;

namespace TestMousePosToTextPointer
{
    public class Word
    {
        public Word(FormattedText format, Point drawPoint, Rect area, int offset)
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
        public bool IsRtl => Format.FlowDirection == FlowDirection.RightToLeft;
    }

    public class WordInfo : Word, IComparable<Point>
    {
        public WordInfo(FormattedText format, Point drawPoint, Rect area, int offset) 
            : base(format, drawPoint, area, offset) { }

        public int CompareTo([AllowNull] Point other)
        {
            var impressivePadding = ImpressivePaddingPercent * Area.Width;
            var wordX = Area.Location.X + impressivePadding;
            var wordXw = Area.Location.X + Area.Width - impressivePadding;
            var wordY = Area.Location.Y;
            var wordYh = Area.Location.Y + Area.Height;
            var mouseX = other.X;
            var mouseY = other.Y;

            if (wordYh < mouseY) return -1;
            if (mouseY < wordY) return 1;
            if (wordXw < mouseX) return -1;
            if (mouseX < wordX) return 1;
            return 0;
        }
    }
}
