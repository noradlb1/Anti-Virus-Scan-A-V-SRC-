using System.Windows;

namespace ScanX
{
    /// <summary>
    /// Interaction logic for wndAbout.xaml
    /// </summary>
    public partial class wndAbout : Window
    {
        public wndAbout()
        {
            InitializeComponent();
           // System.Windows.Controls.Primitives.Thumb.
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
