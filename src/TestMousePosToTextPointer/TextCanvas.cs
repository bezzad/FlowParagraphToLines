using System;
using System.Collections.Generic;
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
        public static readonly DependencyProperty TextProperty;
        protected FormattedText TextFormatter { get; set; }
        protected Point StartSelectionPoint { get; set; }
        protected Point EndSelectionPoint { get; set; }
        protected bool IsMouseDown { get; set; }
        public string Text

        {
            get { return (string)GetValue(TextProperty); }

            set { SetValue(TextProperty, value); }
        }
        public Thickness Padding { get; set; }
        public Dictionary<Rect, FormattedText> VisualWords { get; set; }

        static TextCanvas()
        {
            TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(TextCanvas),
                new FrameworkPropertyMetadata("Default Text",
                    FrameworkPropertyMetadataOptions.SubPropertiesDoNotAffectRender | FrameworkPropertyMetadataOptions.AffectsRender));
        }

        public TextCanvas()
        {
            TextOptions.SetTextFormattingMode(this, TextFormattingMode.Display);
            VisualWords = new Dictionary<Rect, FormattedText>();
            CreateFormattedWords();

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

        // Capture the mouse event and hit test the coordinate point value against
        // the child visual objects.
        private void TextCanvasMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Retrieve the coordinates of the mouse button event.
            var pt = e.GetPosition((UIElement)sender);
            IsMouseDown = true;
            StartSelectionPoint = pt;
            InvalidateVisual();
        }

        // Capture the mouse event and hit test the coordinate point value against
        // the child visual objects.
        private void TextCanvasMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            // Retrieve the coordinates of the mouse button event.
            IsMouseDown = false;
            EndSelectionPoint = e.GetPosition((UIElement)sender);
            InvalidateVisual();
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
                var textGeometry = TextFormatter.BuildHighlightGeometry(StartSelectionPoint);
                var textGeometry2 = TextFormatter.BuildGeometry(StartSelectionPoint);

                var radius = 20;
                drawingContext.DrawRectangle(null, new Pen(Brushes.Red, 1.0),
                    new Rect(StartSelectionPoint.X - radius / 2, StartSelectionPoint.Y - radius / 2, radius, radius));

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
            var spaceFormatter = new FormattedText(
                " ",
                CultureInfo.GetCultureInfo("en-us"),
                FlowDirection.LeftToRight,
                new Typeface("Arial"),
                32,
                Brushes.Black,
                1);

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
                VisualWords.Add(new Rect(new Point(x, y), new Size(spaceFormatter.WidthIncludingTrailingWhitespace, lineHeight)), spaceFormatter);
                x += spaceFormatter.WidthIncludingTrailingWhitespace;
            }
        }

        protected override void OnRender(DrawingContext dc)
        {
            CreateFormattedWords();
            base.OnRender(dc);
            foreach (var word in VisualWords)
            {
                dc.DrawText(word.Value, word.Key.Location);
            }

            if (IsMouseDown)
            {
                var wordRects = VisualWords.Keys.ToList();
                var startWord = wordRects.BinarySearch(new Rect(StartSelectionPoint, new Size(1, 1)), new RectComparer());
                var endWord = wordRects.BinarySearch(new Rect(EndSelectionPoint, new Size(1, 1)), new RectComparer());

                if (startWord < 0)
                    return;

                if (endWord == -25)
                    endWord = wordRects.Count - 1;
                else if(endWord == -1 || endWord == -13)
                    endWord = 0;
                else if(endWord < 0)
                    return;

                var from = Math.Min(startWord, endWord);
                var to = Math.Max(startWord, endWord);

                for (; from <= to; from++)
                {
                    dc.DrawRectangle(null, new Pen(Brushes.Red, 1.0), wordRects[from]);
                }
            }
        }
    }

    public class RectComparer : IComparer<Rect>
    {
        public int Compare(Rect r1, Rect r2)
        {
            var wordX = r1.Location.X;
            var wordXW = r1.Location.X + r1.Width;
            var wordY = r1.Location.Y;
            var wordYH = r1.Location.Y + r1.Height;
            var mouseX = r2.X;
            var mouseY = r2.Y;

            if (wordYH < mouseY) return -1;
            if (mouseY < wordY) return 1;
            if (wordXW < mouseX) return -1;
            if (mouseX < wordX) return 1;
            return 0;
        }
    }
}
