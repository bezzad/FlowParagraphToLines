using System;
using System.Windows;
using System.Windows.Controls;

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

        private void TextAlignChanged(object sender, RoutedEventArgs e)
        {
            var alignName = ((RadioButton) sender).Content as string;
            Reader.TextAlignment = (TextAlignment) Enum.Parse(typeof(TextAlignment), alignName, true);
        }
    }
}
