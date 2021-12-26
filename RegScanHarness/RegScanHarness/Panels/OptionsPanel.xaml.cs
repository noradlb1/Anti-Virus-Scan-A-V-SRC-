using System.Windows.Controls;

namespace ScanX.Panels
{
    /// <summary>
    /// Interaction logic for TopLeftPanel.xaml
    /// </summary>
    public partial class OptionsPanel : UserControl
    {
        public OptionsPanel()
        {
            InitializeComponent();
            this.chkRestore.IsChecked = Properties.Settings.Default.SettingRestore;
            this.chkLogging.IsChecked = Properties.Settings.Default.SettingLog;
            this.chkLogDetailed.IsChecked = Properties.Settings.Default.SettingDetails;
        }

        private void Start_Clicked(object sender, System.Windows.RoutedEventArgs e)
        {
            //
        }

        private void UserControl_Unloaded(object sender, System.Windows.RoutedEventArgs e)
        {
            Properties.Settings.Default.SettingRestore = (bool)this.chkRestore.IsChecked;
            Properties.Settings.Default.SettingLog = (bool)this.chkLogging.IsChecked;
            Properties.Settings.Default.SettingDetails = (bool)this.chkLogDetailed.IsChecked;
            Properties.Settings.Default.Save();
        }

        private void chkRestore_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            CheckBox chk = (CheckBox)sender;
            if (chk.Name == "chkRestore")
            {
                Properties.Settings.Default.SettingRestore.Equals((bool)chk.IsChecked);
            }
            else if (chk.Name == "chkLogging")
            {
                Properties.Settings.Default.SettingLog.Equals((bool)chk.IsChecked);
            }
            else
            {
                Properties.Settings.Default.SettingDetails.Equals((bool)chk.IsChecked);
            }
        }

        private void chkBox_Click(object sender, System.Windows.RoutedEventArgs e)
        {

        }
    }
}
