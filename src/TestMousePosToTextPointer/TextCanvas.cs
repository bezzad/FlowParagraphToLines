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
            var rectComparer = new RectComparer();
            var wordRects = VisualWords.Keys.ToList();

            int GetCorrectWordIndex(Point selectedPoint, bool forceToFind = false)
            {
                var selectedRect = new Rect(selectedPoint, new Size(1, 1));
                var result = 0;

                if (forceToFind && rectComparer.Compare(wordRects.LastOrDefault(), selectedRect) < 0)
                    result = wordRects.Count - 1;
                else if (forceToFind && rectComparer.Compare(wordRects.FirstOrDefault(), selectedRect) > 0)
                    result = 0;
                else
                    result = wordRects.BinarySearch(selectedRect, rectComparer);

                //if (result < 0)
                //{
                //    if (forceToNext)
                //        selectedPoint.X += 10;

                //    if (forceToPrevious)
                //        selectedPoint.X -= 10;

                //    if (selectedPoint.X * 2 > Padding.Left + Padding.Right && selectedPoint.X < Width)
                //        return GetCorrectWordIndex(selectedPoint, forceToNext, forceToPrevious);
                //}
#if DEBUG
                if (result < 0)
                    dc.DrawEllipse(Brushes.Red, null, selectedRect.Location, 5, 5);
#endif

                return result;
            }

            if (StartSelectionPoint.HasValue && EndSelectionPoint.HasValue && StartSelectionPoint.Value.CompareTo(EndSelectionPoint.Value) != 0)
            {
                var forceToFind = StartSelectionPoint.Value.CompareTo(EndSelectionPoint.Value) > 0;
                var startWord = GetCorrectWordIndex(StartSelectionPoint.Value);
                var endWord = GetCorrectWordIndex(EndSelectionPoint.Value);

                if (startWord < 0 && endWord >= 0) // startWord is out but endWord is in correct range
                    startWord = GetCorrectWordIndex(StartSelectionPoint.Value, true);

                if (endWord < 0 && startWord >= 0) // endWord is out but startWord is in correct range
                    endWord = GetCorrectWordIndex(EndSelectionPoint.Value, true);

                if ((startWord < 0 || endWord < 0) == false)
                {
                    HighlightRange = new Range(startWord, endWord);
                }

                if (IsMouseDown == false)
                    StartSelectionPoint = EndSelectionPoint = null;
            }

            if (HighlightRange == null)
                return;

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
