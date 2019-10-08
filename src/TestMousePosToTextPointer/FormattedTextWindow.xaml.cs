using System.Windows;

namespace TestMousePosToTextPointer
{
    /// <summary>
    /// Interaction logic for FormattedTextWindow.xaml
    /// </summary>
    public partial class FormattedTextWindow : Window
    {
        public FormattedTextWindow()
        {
            InitializeComponent();
            DpiChanged += delegate { Reader.Render(); };
        }
    }
}
