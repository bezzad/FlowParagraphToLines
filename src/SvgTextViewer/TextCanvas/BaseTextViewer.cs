using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace SvgTextViewer.TextCanvas
{
    public abstract class BaseTextViewer : Canvas
    {
        public static readonly DependencyProperty FontSizeProperty = DependencyProperty.Register(
            "FontSize", typeof(double), typeof(TextViewer), new PropertyMetadata(default(double)));
        public static readonly DependencyProperty LineHeightProperty = DependencyProperty.Register(
            "LineHeight", typeof(int), typeof(TextViewer), new PropertyMetadata(default(int)));
        public static readonly DependencyProperty IsJustifyProperty = DependencyProperty.Register(
            "IsJustify", typeof(bool), typeof(TextViewer), new PropertyMetadata(default(bool)));
        public static readonly DependencyProperty PaddingProperty = DependencyProperty.Register(
            "Padding", typeof(Thickness), typeof(BaseTextViewer), new PropertyMetadata(default(Thickness)));
        public static readonly DependencyProperty FontFamilyProperty = DependencyProperty.Register(
            "FontFamily", typeof(FontFamily), typeof(BaseTextViewer), new PropertyMetadata(default(FontFamily)));

        public FontFamily FontFamily
        {
            get => (FontFamily) GetValue(FontFamilyProperty);
            set => SetValue(FontFamilyProperty, value);
        }
        public double FontSize
        {
            get => (double)GetValue(FontSizeProperty);
            set => SetValue(FontSizeProperty, value);
        }
        public int LineHeight
        {
            get => (int)GetValue(LineHeightProperty);
            set => SetValue(LineHeightProperty, value);
        }
        public bool IsJustify
        {
            get => (bool)GetValue(IsJustifyProperty);
            set => SetValue(IsJustifyProperty, value);
        }
        public Thickness Padding
        {
            get => (Thickness) GetValue(PaddingProperty);
            set => SetValue(PaddingProperty, value);
        }
        
        protected List<WordInfo> DrawnWords { get; set; }
        


        protected BaseTextViewer()
        {
            TextOptions.SetTextFormattingMode(this, TextFormattingMode.Display);
            Cursor = Cursors.IBeam;
            DrawnWords = new List<WordInfo>();
            FontFamily = new FontFamily("Arial");
            FontSize = 24;
            LineHeight = 25;
        }
        
        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            Render();
        }

        public void Render() { InvalidateVisual(); }
    }
}
