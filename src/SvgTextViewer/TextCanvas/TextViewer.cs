using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using MethodTimer;

namespace SvgTextViewer.TextCanvas
{
    public class TextViewer : BaseTextViewer
    {
        [Time]
        protected void BuildPage(List<List<WordInfo>> content)
        {
            var startPoint = new Point(IsContentRtl ? ActualWidth - Padding.Right : Padding.Left, Padding.Top);
            var nonDirectionalWordsStack = new Stack<WordInfo>();
            var lineWidth = ActualWidth - Padding.Left - Padding.Right;
            var lineRemainWidth = lineWidth;
            var lineBuffer = new List<WordInfo>();
            DrawWords.Clear();


            void AddLine()
            {
                if (nonDirectionalWordsStack.Any())
                    while (nonDirectionalWordsStack.TryPop(out var nWord))
                        AddWord(nWord);
                Lines.Add(lineBuffer);
                lineBuffer = new List<WordInfo>(); // create new line buffer, without cleaning last line
                lineRemainWidth = lineWidth;
                startPoint.Y += LineHeight; // new line
                startPoint.X = IsContentRtl
                    ? ActualWidth - Padding.Right
                    : Padding.Left;
                nonDirectionalWordsStack.Clear();
            }

            void AddWord(WordInfo word)
            {
                var areaLoc = word.IsRtl ? new Point(startPoint.X - word.Width, startPoint.Y) : startPoint;
                var wordArea = new Rect(areaLoc, new Size(word.Width, LineHeight));
                word.Area = wordArea;
                word.DrawPoint = startPoint;
                DrawWords.Add(word);

                startPoint.X += word.IsRtl ? -word.Width : word.Width;
                startPoint.X += word.IsRtl ? -word.SpaceWidth : word.SpaceWidth;
            }

            foreach (var para in content)
            {
                foreach (var word in para)
                {
                    // Create the initial formatted text string.
                    var wordFormatter = word.GetFormattedText(FontFamily, FontSize, PixelsPerDip, LineHeight);

                    if (lineRemainWidth - word.Width <= 0)
                    {
                        AddLine();
                    }

                    lineBuffer.Add(word);
                    if (IsContentRtl != word.IsRtl)
                        nonDirectionalWordsStack.Push(word);
                    else if (nonDirectionalWordsStack.Any())
                    {
                        while (nonDirectionalWordsStack.TryPop(out var nWord))
                        {
                            AddWord(nWord);
                        }
                    }
                    else
                    {
                        AddWord(word);
                    }

                    lineRemainWidth -= word.Width + word.SpaceWidth;
                }

                // new line + ParagraphSpace
                AddLine();
                startPoint.Y += ParagraphSpace;
            }
        }


        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            if (DesignerProperties.GetIsInDesignMode(this))
                return;

            BuildPage(PageContent);

            foreach (var word in PageContent[0])
            {
                dc.DrawText(word.Format, word.DrawPoint);
                if (ShowWireFrame)
                    dc.DrawRectangle(null, WireFramePen, word.Area);
            }
        }
    }
}