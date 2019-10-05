using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Automation.Text;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;
using MethodTimer;

namespace TestMousePosToTextPointer
{
    public static class DocumentExtensions
    {
        private static double LineHeight { get; set; } = 25;
        private static double LineHalfHeight => LineHeight / 2;
        private static FontFamily ArialFontFamily { get; set; } = new FontFamily("Arial");
        private static XmlLanguage Lang { get; set; } = XmlLanguage.GetLanguage("fa-IR");
        private static Visual Visual => Application.Current.MainWindow;

        // Point is specified relative to the given visual
        public static TextPointer ScreenPointToTextPointer(this FlowDocument document, Point screenPoint)
        {
            try
            {
                // Get text before point using automation
                var peer = new DocumentAutomationPeer(document);
                var textProvider = (ITextProvider)peer.GetPattern(PatternInterface.Text);
                var rangeProvider = textProvider?.RangeFromPoint(screenPoint);
                rangeProvider?.MoveEndpointByUnit(TextPatternRangeEndpoint.Start, TextUnit.Document, 1);
                var charsBeforePoint = rangeProvider?.GetText(int.MaxValue)?.Length ?? 0;

                // Find the pointer that corresponds to the TextPointer
                var pointer = document.ContentStart.GetPositionAtOffset(charsBeforePoint);

                // Adjust for difference between "text offset" and actual number of characters before pointer
                for (var i = 0; i < 10; i++)  // Limit to 10 adjustments
                {
                    var error = charsBeforePoint - new TextRange(document.ContentStart, pointer).Text.Length;
                    if (error == 0) break;
                    pointer = pointer?.GetPositionAtOffset(error);
                }
                return pointer;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        public static TextPointer GetPoint(this TextPointer start, int x)
        {
            var ret = start;
            var i = 0;
            while (i < x)
            {
                var b = ret.GetPointerContext(LogicalDirection.Backward);
                if (b == TextPointerContext.Text || b == TextPointerContext.None)
                    i++;

                if (ret.GetPositionAtOffset(1, LogicalDirection.Forward) == null)
                    return ret;
                else
                    ret = ret.GetPositionAtOffset(1, LogicalDirection.Forward);
            }

            return ret;
        }

        public static List<string> CalcLinesByFirstOfNextLine(this FlowDocument document, string content, double width)
        {
            var lines = new List<string>();
            var offsetRanges = new List<(int, int)>();

            var offset = 0;
            var startTextPointer = document.ContentStart;
            var startPoint = startTextPointer.GetCharacterRect(LogicalDirection.Backward).Location;
            var xPoint = startPoint.X;
            var yPoint = startPoint.Y + LineHalfHeight;

            // While we are not at the end of document
            while (offset < content.Length)
            {
                // next line start position
                yPoint += LineHeight;
                var nextLineStartPoint = Visual.PointToScreen(new Point(xPoint, yPoint));
                var nextLineStartTextPointer = document.ScreenPointToTextPointer(nextLineStartPoint);

                var nextOffset = nextLineStartTextPointer != null
                    ? new TextRange(document.ContentStart, nextLineStartTextPointer).Text.Length
                    : content.Length;

                if (nextOffset <= offset)
                    nextOffset = content.Length;

                offsetRanges.Add((offset, nextOffset - 1));
                var lineText = content.Substring(offset, nextOffset - offset);
                lines.Add(lineText);

                offset = nextOffset;
            }

            return lines;
        }

        public static FlowDocument Clone(this FlowDocument document, double width)
        {
            var docHolder = new FlowDocumentPageViewer
            {
                Width = width,
                Height = double.MaxValue
            };

            var doc = new FlowDocument
            {
                LineStackingStrategy = document.LineStackingStrategy,
                LineHeight = document.LineHeight,
                FlowDirection = document.FlowDirection,
                FontFamily = document.FontFamily,
                FontWeight = document.FontWeight,
                FontSize = document.FontSize,
                TextAlignment = document.TextAlignment,
                Language = document.Language,
                PagePadding = document.PagePadding,
                ColumnGap = document.ColumnGap,
                ColumnWidth = document.ColumnWidth,
                PageWidth = width,
                MaxPageWidth = width,
                PageHeight = double.NaN,
                MaxPageHeight = double.PositiveInfinity
            };
            docHolder.Document = document;

            return doc;
        }

        [Time]
        public static List<string> CalcLinesByEndOfLine(this FlowDocument document, string content, double width)
        {
            var lines = new List<string>();
            var offsetRanges = new List<(int, int)>();
//            var document = flowDocument.Clone(width);
//            var p = new Paragraph(new Run(content));
//            p.FontFamily = ArialFontFamily;
//            p.FontSize = document.FontSize;
//            p.FontStyle = FontStyles.Normal;
//            p.TextAlignment = document.TextAlignment;
//            p.LineHeight = document.LineHeight;
//            p.LineStackingStrategy = document.LineStackingStrategy;
//            p.Foreground = Brushes.Black;
//            p.Language = document.Language;
//            p.FontWeight = document.FontWeight;
//            document.Blocks.Add(new Paragraph(new Run(content)));

            var offset = 0;
            var startTextPointer = document.ContentStart;
            var startPoint = startTextPointer.GetCharacterRect(LogicalDirection.Backward).Location;
            var yPoint = startPoint.Y + LineHalfHeight;
            //var isRtl = Math.Abs(width - document.PagePadding.Right - xStartPoint) < 5;
            var xEndPoint = document.PagePadding.Left;

            // While we are not at the end of document
            while (offset < content.Length)
            {
                // end of line position
                var endPoint = Visual.PointToScreen(new Point(xEndPoint, yPoint));
                var endTextPointer = document.ScreenPointToTextPointer(endPoint);

                var nextOffset = endTextPointer != null
                    ? new TextRange(document.ContentStart, endTextPointer).Text.Length
                    : content.Length;

                if (nextOffset <= offset)
                    nextOffset = content.Length;

                offsetRanges.Add((offset, nextOffset - 1));
                var lineText = content.Substring(offset, nextOffset - offset);
                lines.Add(lineText);

                offset = nextOffset;
                yPoint += LineHeight;
            }

            return lines;
        }


    }
}
