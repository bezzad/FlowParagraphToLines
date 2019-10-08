using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows;

namespace SvgTextViewer
{
    public class WordInfo : Word, IComparable<Point>
    {
        public WordInfo(string text, int offset, bool isRtl)
            : base(text, offset)
        {
            Styles.Add(StyleType.Direction, new InlineStyle(StyleType.Direction, isRtl ? "rtl" : "ltr"));
        }

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