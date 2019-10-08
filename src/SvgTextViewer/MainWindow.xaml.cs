using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace SvgTextViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var fonts = Fonts.SystemFontFamilies.OrderBy(f => f.Source).ToList();
            CmbFontFamily.ItemsSource = fonts;
            CmbFontFamily.SelectedIndex = fonts.FindIndex(f => f.Source == "Arial");
            CmbFontSize.ItemsSource = new List<double>() { 8, 9, 10, 11, 12, 14, 16, 18, 20, 22, 24, 26, 28, 36, 48, 72 };
            CmbFontSize.SelectedIndex = 10;
            CmbLineHeight.ItemsSource = new List<double>() { 10, 11, 12, 13, 14, 16, 18, 20, 22, 24, 26, 28, 36, 48, 72, 96 };
            CmbLineHeight.SelectedIndex = 10;
            DpiChanged += delegate { Reader.Render(); };
        }
    }
}
