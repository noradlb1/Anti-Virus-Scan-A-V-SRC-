using System.Windows;
using System.Windows.Controls;

namespace ScanX.Panels
{
    /// <summary>
    /// Interaction logic for RegScanPanel.xaml
    /// </summary>
    public partial class RegScanPanel : UserControl
    {
        public RegScanPanel()
        {
            InitializeComponent();
        }

        private void Start_Clicked(object sender, RoutedEventArgs e)
        {
            //
        }

        private void Checkbox_Checked(object sender, RoutedEventArgs e)
        {
           // CheckBox c = (CheckBox)sender;
           // c.IsChecked = !c.IsChecked;
        }
    }
}
