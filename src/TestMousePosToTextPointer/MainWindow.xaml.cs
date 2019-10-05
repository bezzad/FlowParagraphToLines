using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Automation.Text;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;

namespace TestMousePosToTextPointer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private double LineHeight { get; set; } = 25;
        private FontFamily ArialFontFamily { get; set; } = new FontFamily("Arial");
        private XmlLanguage Lang { get; set; } = XmlLanguage.GetLanguage("fa-IR");
        private string ContentText { get; set; }
        private List<string> Lines { get; set; } = new List<string>();


        public MainWindow()
        {
            InitializeComponent();

            TestFlowPage.LineStackingStrategy = LineStackingStrategy.BlockLineHeight;
            TestFlowPage.LineHeight = LineHeight;
            TestFlowPage.FlowDirection = FlowDirection.RightToLeft;
            TestFlowPage.LineHeight = LineHeight;
            TestFlowPage.FontFamily = ArialFontFamily;
            TestFlowPage.FontWeight = FontWeights.Normal;
            TestFlowPage.FontSize = TestFlowPage.FontSize;
            TestFlowPage.LineStackingStrategy = LineStackingStrategy.BlockLineHeight;
            TestFlowPage.TextAlignment = TextAlignment.Left;
            TestFlowPage.Language = Lang;
            TestFlowPage.PagePadding = new Thickness(10);
            TestFlowPage.ColumnGap = 0;
            TestFlowPage.ColumnWidth = double.PositiveInfinity;

            ContentText = GetContent();
            SizeChanged += delegate
            {
                var width = FlowDocumentFrame.ActualWidth.Equals(double.NaN) ? 555.555 : FlowDocumentFrame.ActualWidth;
                CalcLines(ContentText, width);
                ListBoxLines.Items.Clear();

                foreach (var line in Lines)
                    ListBoxLines.Items.Add(line);
            };
        }

        private string GetContent()
        {
            var para = TestFlowPage.Blocks.FirstBlock as Paragraph;
            var run = para?.Inlines.FirstInline as Run;
            return run?.Text;
        }


        private static TextPointer GetPoint(TextPointer start, int x)
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

        List<(int, int)> OffsetRanges = new List<(int, int)>();

        private List<string> CalcLinesByFirstOfNextLine(string content, double width)
        {
            Lines.Clear();
            OffsetRanges.Clear();

            var lineHalfHeight = LineHeight / 2;
            var offset = 0;
            var startTextPointer = TestFlowPage.ContentStart;
            var startPoint = startTextPointer.GetCharacterRect(LogicalDirection.Backward).Location;
            var xPoint = startPoint.X;
            var yPoint = startPoint.Y + lineHalfHeight;

            // While we are not at the end of document
            while (offset < content.Length)
            {
                // next line start position
                yPoint += LineHeight;
                var nextLineStartPoint = PointToScreen(new Point(xPoint, yPoint));
                var nextLineStartTextPointer = TestFlowPage.ScreenPointToTextPointer(nextLineStartPoint);

                var nextOffset = nextLineStartTextPointer != null
                    ? new TextRange(TestFlowPage.ContentStart, nextLineStartTextPointer).Text.Length
                    : content.Length;

                if (nextOffset <= offset)
                    nextOffset = content.Length;

                OffsetRanges.Add((offset, nextOffset - 1));
                var lineText = content.Substring(offset, nextOffset - offset);
                Lines.Add(lineText);

                offset = nextOffset;
            }

            return Lines;
        }

        private List<string> CalcLinesByEndOfLine(string content, double width)
        {
            Lines.Clear();
            OffsetRanges.Clear();

            var lineHalfHeight = LineHeight / 2;
            var offset = 0;
            var startTextPointer = TestFlowPage.ContentStart;
            var startPoint = startTextPointer.GetCharacterRect(LogicalDirection.Backward).Location;
            var xPoint = startPoint.X;
            var yPoint = startPoint.Y + lineHalfHeight;

            // While we are not at the end of document
            while (offset < content.Length)
            {
                // next line start position
                yPoint += LineHeight;
                var nextLineStartPoint = PointToScreen(new Point(xPoint, yPoint));
                var nextLineStartTextPointer = TestFlowPage.ScreenPointToTextPointer(nextLineStartPoint);

                var nextOffset = nextLineStartTextPointer != null
                    ? new TextRange(TestFlowPage.ContentStart, nextLineStartTextPointer).Text.Length
                    : content.Length;

                if (nextOffset <= offset)
                    nextOffset = content.Length;

                OffsetRanges.Add((offset, nextOffset - 1));
                var lineText = ContentText.Substring(offset, nextOffset - offset);
                Lines.Add(lineText);

                offset = nextOffset;
            }

            return Lines;
        }

        private List<string> CalcLines(string content, double width)
        {
            return CalcLinesByEndOfLine(content, width);
        }

        private void Test_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                var pointer = Mouse.GetPosition(this);
                var screenPoint = PointToScreen(pointer);
                TextPointer caretPointer = TestFlowPage.ScreenPointToTextPointer(screenPoint);

                var offset = new TextRange(TestFlowPage.ContentStart, caretPointer).Text.Length;

                var startToCaret = new TextRange(TestFlowPage.ContentStart, caretPointer);
                startToCaret.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Red);
                var caretToEnd = new TextRange(caretPointer, TestFlowPage.ContentEnd);
                caretToEnd.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Black);

                LabelMsg.DataContext = $"{pointer}\t offset:{offset}\t {ContentText[offset - 1]}";

            }
            catch (Exception exception)
            {
                LabelMsg.DataContext = exception.Message;
            }
        }
    }

    public static class DocumentExtensions
    {
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
    }
}
