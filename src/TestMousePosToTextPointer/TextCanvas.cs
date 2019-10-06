using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace TestMousePosToTextPointer
{
    public class TextCanvas : Canvas
    {
        public static readonly DependencyProperty TextProperty;
        // Create a collection of child visual objects.
        private readonly VisualCollection _children;
        private FormattedText TextFormatter { get; set; }

        // Provide a required override for the VisualChildrenCount property.
        protected override int VisualChildrenCount => _children?.Count ?? 0;

        protected Point StartSelectionPoint { get; set; }
        protected Point EndSelectionPoint { get; set; }
        protected bool MouseDown { get; set; } 

        public string Text

        {
            get { return (string)GetValue(TextProperty); }

            set { SetValue(TextProperty, value); }
        }

        public Thickness Padding { get; set; }

        static TextCanvas()
        {
            TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(TextCanvas),
                new FrameworkPropertyMetadata("Default Text",
                    FrameworkPropertyMetadataOptions.SubPropertiesDoNotAffectRender | FrameworkPropertyMetadataOptions.AffectsRender));
        }

        public TextCanvas()
        {
            TextOptions.SetTextFormattingMode(this, TextFormattingMode.Display);

            //_children = new VisualCollection(this)
            //{
            //    CreateDrawingVisualText()
            //};

            // Add the event handler for MouseLeftButtonUp.
            MouseLeftButtonUp += MyVisualHost_MouseLeftButtonUp;
            MouseLeftButtonDown += MyVisualHost_MouseLeftButtonDown;
        }

        // Capture the mouse event and hit test the coordinate point value against
        // the child visual objects.
        private void MyVisualHost_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Retrieve the coordinates of the mouse button event.
            var pt = e.GetPosition((UIElement)sender);
            MouseDown = true;
            StartSelectionPoint = pt;
            InvalidateVisual();

            // Initiate the hit test by setting up a hit test result callback method.
            VisualTreeHelper.HitTest(this, null, MyCallback, new PointHitTestParameters(StartSelectionPoint));
        }

        // Capture the mouse event and hit test the coordinate point value against
        // the child visual objects.
        private void MyVisualHost_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            // Retrieve the coordinates of the mouse button event.
            MouseDown = false;
            EndSelectionPoint = e.GetPosition((UIElement)sender);
            InvalidateVisual();

            // Initiate the hit test by setting up a hit test result callback method.
            VisualTreeHelper.HitTest(this, null, MyCallback, new PointHitTestParameters(EndSelectionPoint));
        }

        protected override HitTestResult HitTestCore(PointHitTestParameters hitTestParameters)
        {
            if (VisualTreeHelper.GetDescendantBounds(this).Contains(hitTestParameters.HitPoint))
            {
                StartSelectionPoint = hitTestParameters.HitPoint;
                return new PointHitTestResult(this, hitTestParameters.HitPoint);
            }

            return null;

            //TextFormatter.BuildHighlightGeometry(hitTestParameters.HitPoint, 0, 10);
            //return new PointHitTestResult(this, hitTestParameters.HitPoint);
        }

        protected override GeometryHitTestResult HitTestCore(GeometryHitTestParameters hitTestParameters)
        {
            // Possibly checking every pixel within the hitTestParameters.HitGeometry.Bounds rectangle
            var geometry = new RectangleGeometry(VisualTreeHelper.GetDescendantBounds(this));
            return new GeometryHitTestResult
                (this, geometry.FillContainsWithDetail(hitTestParameters.HitGeometry));

            //InvalidateVisual();
            //return base.HitTestCore(hitTestParameters);
        }

        // If a child visual object is hit, toggle its opacity to visually indicate a hit.
        public HitTestResultBehavior MyCallback(HitTestResult result)
        {
            if (result.VisualHit.GetType() == typeof(System.Windows.Media.DrawingVisual))
            {
                ((System.Windows.Media.DrawingVisual)result.VisualHit).Opacity =
                    ((System.Windows.Media.DrawingVisual)result.VisualHit).Opacity == 1.0 ? 0.4 : 1.0;
            }

            // Stop the hit test enumeration of objects in the visual tree.
            return HitTestResultBehavior.Stop;
        }

        protected override Visual GetVisualChild(int index)
        {
            if (index < 0 || index >= _children.Count)
            {
                throw new ArgumentOutOfRangeException();
            }

            return _children[index];
        }

        // Create a DrawingVisual that contains text.
        private System.Windows.Media.DrawingVisual CreateDrawingVisualText()
        {
            // Create an instance of a DrawingVisual.
            System.Windows.Media.DrawingVisual drawingVisual = new System.Windows.Media.DrawingVisual();

            // Retrieve the DrawingContext from the DrawingVisual.
            DrawingContext drawingContext = drawingVisual.RenderOpen();

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
                // Set a maximum width and height. If the text overflows these values, an ellipsis "..." appears.
                MaxTextWidth = 500,
                MaxTextHeight = 540
            };

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
            drawingContext.DrawText(TextFormatter, new Point(0, 0));

            //---------------------------------------------------


            // Close the DrawingContext to persist changes to the DrawingVisual.
            drawingContext.Close();

            return drawingVisual;
        }

        protected override void OnRender(DrawingContext drawingContext)
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
            if (MouseDown)
            {
                // Build the geometry object that represents the text highlight.
                var textGeometry = TextFormatter.BuildHighlightGeometry(StartSelectionPoint);
                var textGeometry2 = TextFormatter.BuildGeometry(StartSelectionPoint);
                
                var radius = 20;
                drawingContext.DrawRectangle(null, new Pen(Brushes.Red, 1.0),
                    new Rect(StartSelectionPoint.X-radius/2, StartSelectionPoint.Y - radius / 2, radius, radius));

                drawingContext.DrawGeometry(Brushes.DarkGray, new Pen(), textGeometry);
                drawingContext.DrawGeometry(Brushes.Chartreuse, new Pen(), textGeometry2);
            }

            //---------------------------------------------------
        }
    }
}
