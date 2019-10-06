using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace TestMousePosToTextPointer
{
    public class TextCanvas : Canvas
    {
        protected FormattedText TextFormatter { get; set; }
        protected Point? StartSelectionPoint { get; set; }
        protected Point? EndSelectionPoint { get; set; }
        protected bool IsMouseDown { get; set; }
        protected Brush SelectedBrush { get; set; }
        protected Range HighlightRange { get; set; }

        public Thickness Padding { get; set; }
        public Dictionary<Rect, FormattedText> VisualWords { get; set; }




        public TextCanvas()
        {
            TextOptions.SetTextFormattingMode(this, TextFormattingMode.Display);
            Cursor = Cursors.IBeam;
            VisualWords = new Dictionary<Rect, FormattedText>();
            CreateFormattedWords();
            SelectedBrush = new SolidColorBrush(Colors.DarkCyan) { Opacity = 0.5 };

            // Add the event handler for MouseLeftButtonUp.
            MouseLeftButtonUp += TextCanvasMouseLeftButtonUp;
            MouseLeftButtonDown += TextCanvasMouseLeftButtonDown;
            MouseMove += TextCanvasMouseMove;
        }


        private void TextCanvasMouseMove(object sender, MouseEventArgs e)
        {
            if (IsMouseDown)
            {
                EndSelectionPoint = e.GetPosition(this);
                InvalidateVisual();
            }
        }
        private void TextCanvasMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Retrieve the coordinates of the mouse button event.
            IsMouseDown = true;
            ClearSelection();
            StartSelectionPoint = e.GetPosition((UIElement)sender);
            EndSelectionPoint = StartSelectionPoint;
            Debug.WriteLine("Mouse Left Button Down");
            InvalidateVisual();
        }
        private void TextCanvasMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            // Retrieve the coordinates of the mouse button event.
            if (IsMouseDown)
            {
                IsMouseDown = false;
                EndSelectionPoint = e.GetPosition((UIElement)sender);
                Debug.WriteLine("Mouse Left Button Up, " + HighlightRange);
                InvalidateVisual();
            }
        }

        protected void OnRender_old(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            //---------------------------------------------------
            var testString = "Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor";

            // Create the initial formatted text string.
            TextFormatter = new FormattedText(
                testString,
                CultureInfo.GetCultureInfo("en-us"),
                FlowDirection.LeftToRight,
                new Typeface("Arial"),
                32,
                Brushes.Black, 1)
            {
                MaxTextWidth = Width - Padding.Left - Padding.Right,
                MaxTextHeight = Height - Padding.Top - Padding.Bottom
            };

            // Set a maximum width and height. If the text overflows these values, an ellipsis "..." appears.

            // Use a larger font size beginning at the first (zero-based) character and continuing for 5 characters.
            // The font size is calculated in terms of points -- not as device-independent pixels.
            TextFormatter.SetFontSize(36 * (96.0 / 72.0), 0, 5);

            // Use a Bold font weight beginning at the 6th character and continuing for 11 characters.
            TextFormatter.SetFontWeight(FontWeights.Bold, 6, 11);

            // Use a linear gradient brush beginning at the 6th character and continuing for 11 characters.
            TextFormatter.SetForegroundBrush(
                new LinearGradientBrush(
                    Colors.Orange,
                    Colors.Teal,
                    90.0),
                6, 11);

            // Use an Italic font style beginning at the 28th character and continuing for 28 characters.
            TextFormatter.SetFontStyle(FontStyles.Italic, 28, 28);

            // Draw the formatted text string to the DrawingContext of the control.
            drawingContext.DrawText(TextFormatter, new Point(Padding.Left, Padding.Top));
            if (IsMouseDown)
            {
                // Build the geometry object that represents the text highlight.
                var textGeometry = TextFormatter.BuildHighlightGeometry(StartSelectionPoint.Value);
                var textGeometry2 = TextFormatter.BuildGeometry(StartSelectionPoint.Value);

                var radius = 20;
                drawingContext.DrawRectangle(null, new Pen(Brushes.Red, 1.0),
                    new Rect(StartSelectionPoint.Value.X - radius / 2, StartSelectionPoint.Value.Y - radius / 2, radius, radius));

                drawingContext.DrawGeometry(Brushes.DarkGray, new Pen(), textGeometry);
                drawingContext.DrawGeometry(Brushes.Chartreuse, new Pen(), textGeometry2);
            }

            //---------------------------------------------------
        }


        protected void CreateFormattedWords()
        {
            VisualWords.Clear();
            var text = "Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor";
            var x = Padding.Left;
            var y = Padding.Top;
            var lineHeight = 35;
            var spaceWidth = 5;

            var words = text.Split(' ');
            var random = false;
            foreach (var word in words)
            {
                // Create the initial formatted text string.
                var textFormatter = new FormattedText(
                    word,
                    CultureInfo.GetCultureInfo("en-us"),
                    FlowDirection.LeftToRight,
                    new Typeface("Arial"),
                    32,
                    Brushes.Black, 1);

                // Use an Italic font style beginning at the 28th character and continuing for 28 characters.
                if (random)
                    textFormatter.SetFontWeight(FontWeights.Bold, 0, word.Length);

                random = !random;

                if (x + textFormatter.Width > Width - Padding.Left - Padding.Right)
                {
                    y += lineHeight;// new line
                    x = Padding.Left;
                }

                VisualWords.Add(new Rect(new Point(x, y), new Size(textFormatter.Width, lineHeight)), textFormatter);
                x += textFormatter.Width;
                x += spaceWidth;
            }
        }

        protected override void OnRender(DrawingContext dc)
        {
            CreateFormattedWords();
            base.OnRender(dc);
            foreach (var (rect, word) in VisualWords)
                dc.DrawText(word, rect.Location);

            if (IsMouseDown || HighlightRange != null)
            {
                HighlightSelectedText(dc);
            }
        }

        public void ClearSelection()
        {
            HighlightRange = null;
            StartSelectionPoint = null;
            EndSelectionPoint = null;
        }


        protected void HighlightSelectedText(DrawingContext dc)
        {
            var wordRects = VisualWords.Keys.ToList();

            if (StartSelectionPoint.HasValue && EndSelectionPoint.HasValue)
            {
                var rectComparer = new RectComparer();
                var startWord = wordRects.BinarySearch(new Rect(StartSelectionPoint.Value, new Size(1, 1)),
                    rectComparer);
                var endWord =
                    wordRects.BinarySearch(new Rect(EndSelectionPoint.Value, new Size(1, 1)), rectComparer);

                if (startWord < 0)
                    return;

                if (endWord < 0)
                {
                    if (rectComparer.Compare(wordRects.LastOrDefault(),
                            new Rect(EndSelectionPoint.Value, new Size(1, 1))) <
                        0)
                        endWord = wordRects.Count - 1;
                    else if (rectComparer.Compare(wordRects.FirstOrDefault(),
                                 new Rect(EndSelectionPoint.Value, new Size(1, 1))) > 0)
                        endWord = 0;
                    else
                        return;
                }

                HighlightRange = new Range(startWord, endWord);
                if (IsMouseDown == false)
                    StartSelectionPoint = EndSelectionPoint = null;
            }

            var from = Math.Min(HighlightRange.Start, HighlightRange.End);
            var to = Math.Max(HighlightRange.Start, HighlightRange.End);

            for (var w = from; w <= to; w++)
            {
                var currentWord = wordRects[w];
                var isFirstOfLineWord = w == from || !wordRects[w - 1].Y.Equals(currentWord.Y);

                if (isFirstOfLineWord == false)
                {
                    var previousWord = wordRects[w - 1];
                    var startX = previousWord.Location.X + previousWord.Width;
                    var width = currentWord.Location.X - startX + currentWord.Width;
                    currentWord = new Rect(new Point(startX, currentWord.Y), new Size(width, currentWord.Height));
                }
                dc.DrawRectangle(SelectedBrush, null, currentWord);
            }
        }
    }
}
