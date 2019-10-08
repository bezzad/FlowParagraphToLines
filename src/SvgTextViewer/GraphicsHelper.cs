using System.Windows;

namespace SvgTextViewer
{
    public static class GraphicsHelper
    {
        public static int CompareTo(this Point pLeft, Point pRight)
        {
            if (pLeft.Y > pRight.Y)
                return 1;
            if (pLeft.Y < pRight.Y)
                return -1;
            if (pLeft.X > pRight.X)
                return 1;
            if (pLeft.X < pRight.X)
                return -1;

            return 0;
        }
    }
}
