using System.Windows;

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
            DpiChanged += delegate { Reader.Render(); };
        }
    }
}
