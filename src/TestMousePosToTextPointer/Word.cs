using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Media;

namespace TestMousePosToTextPointer
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

    public static class WordHelper
    {
        /// <summary>
        /// Searches a section of the list for a given element using a binary search
        /// algorithm.
        /// </summary>
        /// <param name="words">list of words, which must be searched</param>
        /// <param name="index">offset to beginning search</param>
        /// <param name="count">count of elements must be searched after offset</param>
        /// <param name="value">the position of mouse on the canvas</param>
        /// <returns></returns>
        public static int BinarySearch(this IList<WordInfo> words, int index, int count, Point value)
        {
            if (index < 0 || count < 0)
                throw new ArgumentOutOfRangeException((index < 0 ? nameof(index) : nameof(count)));
            if (words.Count - index < count)
                throw new ArgumentException("Argument has invalid len from index for count");

            var lo = index;
            var hi = index + count - 1;
            // ReSharper disable once TooWideLocalVariableScope
            int mid; // declared here for performance
            while (lo <= hi)
            {
                mid = (lo + hi) / 2;
                var r = words[mid].CompareTo(value);
                if (r == 0)
                    return mid;
                if (r < 0)
                    hi = mid - 1;
                else
                    lo = mid + 1;
            }

            // return bitwise complement of the first element greater than value.
            // Since hi is less than lo now, ~lo is the correct item.
            return ~lo;
        }

        public static int BinarySearch(this IList<WordInfo> words, Point value)
        {
            return words.BinarySearch(0, words.Count, value);
        }
    }
}
