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
                var lines = TestFlowPage.CalcLinesByEndOfLine(ContentText, width);
                ListBoxLines.Items.Clear();

                foreach (var line in lines)
                    ListBoxLines.Items.Add(line);
            };
        }

        private string GetContent()
        {
            var para = TestFlowPage.Blocks.FirstBlock as Paragraph;
            var run = para?.Inlines.FirstInline as Run;
            return run?.Text;
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

}
