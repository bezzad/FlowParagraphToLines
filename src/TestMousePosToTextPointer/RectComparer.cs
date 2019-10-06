using System.Collections.Generic;
using System.Windows;

namespace TestMousePosToTextPointer
{
    public class RectComparer : IComparer<Rect>
    {
        public int Compare(Rect leftRect, Rect rightRect)
        {
            var wordX = leftRect.Location.X;
            var wordXw = leftRect.Location.X + leftRect.Width;
            var wordY = leftRect.Location.Y;
            var wordYh = leftRect.Location.Y + leftRect.Height;
            var mouseX = rightRect.X;
            var mouseY = rightRect.Y;

            if (wordYh < mouseY) return -1;
            if (mouseY < wordY) return 1;
            if (wordXw < mouseX) return -1;
            if (mouseX < wordX) return 1;
            return 0;
        }
    }
}
