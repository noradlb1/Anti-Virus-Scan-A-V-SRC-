#region About

#endregion

#region ToDo
//lock controls - done
//list item layout - 
//fix scan counters - 
//filter $RecycleBin - 
//check match paths - 
//progressbars subscan - 
//mru scan - 
//options - 
//backup/restore - 
//delete keys/values - 
//styles/theming? - 
//icons/images - 
//article/publish - 
#endregion

#region Directives
using System;
using System.Windows;
using RegScanHarness.Implementation;
using RegScanHarness.Panels;
using System.Windows.Controls.Primitives;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using VTRegScan;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System.Collections;
using vhRCMW;
#endregion

#region Scan Data
public class ResultsList : ObservableCollection<ScanData>
{
    public ResultsList() : base() { }
}
#endregion

namespace RegScanHarness
{
    public partial class Window1 : RCMW
    {
        #region Fields
        RegScanPanel _pnlRegScan;
        OptionsPanel _pnlOptions;
        HelpPanel _pnlHelp;
        RegScanActivePanel _pnlRegScanActive;
        ScanResultsPanel _pnlScanResults;
        BitmapImage _bmpMonitor;
        BitmapImage _bmpDrive;
        BitmapImage _bmpOptions;
        BitmapImage _bmpAbout;
        cRegScan _RegScan;
        private System.Windows.Threading.DispatcherTimer _aTimer;
        private static bool _bTimerOn = false;
        private static int _iKeyCounter = 0;
        private static int _iResultsCounter = 0;
        private static int _iSegmentCounter = 0;
        private static int _iProgressMax = 0;
        private static string _sLabel = "";
        private static string _sMatch = "";
        private static string _sPath = "";
        private static string _sSegment = "";
        private static string _sScanImagePath = "";
        private static ResultsList _lResults;
        #endregion

        #region Properties
        private bool ControlScan { get; set; }
        private bool UserScan { get; set; }
        private bool SoftwareScan { get; set; }
        private bool FontScan { get; set; }
        private bool HelpScan { get; set; }
        private bool LibraryScan { get; set; }
        private bool StartupScan { get; set; }
        private bool VdmScan { get; set; }
        private bool HistoryScan { get; set; }
        private bool DeepScan { get; set; }
        #endregion

        public Window1()
        {
            InitializeComponent();
            InitFields(); 
        }

        #region Library Events
        private void RegScan_CurrentPath(string path)
        {
            _sPath = path;
        }

        private void RegScan_KeyCount()
        {
            _iKeyCounter += 1;
        }

        private void RegScan_LabelChange(string phase, string label)
        {
            _sLabel = label;
        }

        private void RegScan_MatchItem(cLightning.ROOT_KEY root, string key, string value, string path, RESULT_TYPE id)
        {
            _sMatch = path;
            _iResultsCounter += 1;
            int n = IdConverter((int)id);
           // _lResults.Add(new ScanData(root, key, value, path, _sScanImagePath, n));
        }

        private void RegScan_ProcessChange()
        {
            //
        }

        private void RegScan_ProcessCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            _sSegment = "Scan Complete!";
            //_bTimerOn = false;
        }

        private void RegScan_ScanComplete()
        {
            _sSegment = "Scan Complete!";
            _bTimerOn = false;
        }

        private void RegScan_ScanCount(int count)
        {
            _iProgressMax = count;
        }

        private void RegScan_StatusChange(string label)
        {
            _sSegment = label;
            _iSegmentCounter += 1;
        }

        private void _aTimer_Tick(object sender, EventArgs e)
        {
            _pnlRegScanActive.txtScanPhase.Text = _sLabel;
            _pnlRegScanActive.txtScanningKey.Text = _sPath;
            _pnlRegScanActive.txtKeyCount.Text = _iKeyCounter.ToString();
            _pnlRegScanActive.txtLastMatch.Text = _sMatch;
            _pnlRegScanActive.txtMatchCount.Text = _iResultsCounter.ToString();
            _pnlRegScanActive.prgMain.Maximum = _iProgressMax;
            _pnlRegScanActive.prgMain.Value = _iSegmentCounter;
            _pnlRegScanActive.txtSegmentsRemaining.Text = (_pnlRegScanActive.prgMain.Maximum - _iSegmentCounter).ToString();
            _pnlRegScanActive.txtSegmentsScanned.Text = _iSegmentCounter.ToString();
            this.txtStatusBar.Text = _sSegment;
            if (_bTimerOn == false)
            {
                _pnlRegScanActive.prgMain.Value = _iProgressMax;
                StopRegScan();
            }

        }
        #endregion

        #region Control Events
        private void Button_Clicked(object sender, RoutedEventArgs e)
        {
            //
            Button b = e.OriginalSource as Button;
            if (b != null)
            {
                switch (b.Name)
                {
                    case "btnRegScanStart":
                        if ((string)b.Content == "Start")
                        {
                            int c = ScanCount();
                            if (c > 0)
                            {
                                _pnlRegScanActive.prgMain.Maximum = c;
                                grdContainer.Children.Clear();
                                grdContainer.Children.Add(_pnlRegScanActive);
                                RegScanSetup();
                                StartRegScan();
                            }
                            else
                            {
                                MessageBox.Show("You do not have any scan segments selected, please check at least one",
                                    "Invalid Selection!",
                                    MessageBoxButton.OK, MessageBoxImage.Exclamation);
                            }
                        }
                        break;
                    case "btnRegScanCancel":
                        if ((string)b.Content == "Cancel")
                        {
                            if (_bTimerOn == true)
                            {
                                // cancel out
                                _RegScan.CancelProcessAsync();
                                CancelRegScan();
                            }
                        }
                        else
                        {
                            grdContainer.Children.Clear();
                            grdContainer.Children.Add(_pnlScanResults);
                            if (_pnlScanResults.lstResults.DataContext != null)
                            {
                                _pnlScanResults.lstResults.DataContext = null;
                            }
                            _pnlScanResults.lstResults.DataContext = _lResults;
                        }
                        break;
                    case "btnSelectAll":
                        CheckItems(true);
                        break;
                    case "btnDeselectAll":
                        CheckItems(false);
                        break;
                    case "btnRemove":

                        break;
                }
            }
        }

        private void Link_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ResultsCheck();

            TextBlock lnk = (TextBlock)sender;

            if (lnk.Name == "txtHelpLink" && this.btnHelp.IsChecked == false)
            {
                ToggleToolBarButtons("btnHelp");
                TogglePanels("btnHelp");
            }
            else if (lnk.Name == "txtOptionsLink" && this.btnOptions.IsChecked == false)
            {
                ToggleToolBarButtons("btnOptions");
                TogglePanels("btnOptions");
            }
        }

        private void ToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            ResultsCheck();

            ToggleButton tb = (ToggleButton)sender;
            if (!tb.IsChecked == true)
            {
                return;
            }
            ToggleToolBarButtons(tb.Name);
            TogglePanels(tb.Name);
        }
        #endregion

        #region Helpers
        private void CancelRegScan()
        {
            LockControls(false);
            _bTimerOn = false;
            _aTimer.IsEnabled = false;
            _pnlRegScanActive.btnRegScanCancel.Content = "Cancel";
            ResetRegScan();
            ToggleToolBarButtons("btnRegscan");
            TogglePanels("btnRegscan");
        }

        private void CheckItems(bool check)
        {
            if (_lResults != null)
            {
                foreach (ScanData o in _lResults)
                {
                    o.Check = check;
                }
            }
        }

        private static int IdConverter(int id)
        {
            if (id < 11)// "Control Scan"
            {
                _sScanImagePath = "/Images/reg-ctrl48.ico";
                return 10;
            }
            else if (id == 11)// "User Scan"
            {
                _sScanImagePath = "/Images/reg-user48.ico";
                return 8;
            }
            else if (id < 15)// "System Software"
            {
                _sScanImagePath = "/Images/reg-sys48.ico";
                return 9;
            }
            else if (id == 15)// "System Fonts"
            {
                _sScanImagePath = "/Images/reg-font48.ico";
                return 6;
            }
            else if (id == 16)// "System Help Files"
            {
                _sScanImagePath = "/Images/reg-help48.ico";
                return 6;
            }
            else if (id == 17)// "Shared Libraries"
            {
                _sScanImagePath = "/Images/reg-lib48.ico";
                return 8;
            }
            else if (id == 18)// "Startup Entries"
            {
                _sScanImagePath = "/Images/reg-start48.ico";
                return 7;
            }
            else if (id == 19)// "Installation Strings"
            {
                _sScanImagePath = "/Images/reg-inst48.ico";
                return 4;
            }
            else if (id == 20)// "Virtual Devices"
            {
                _sScanImagePath = "/Images/reg-vdf48.ico";
                return 8;
            }
            else if (id < 25)// "History and Start Menu"
            {
                _sScanImagePath = "/Images/reg-hist48.ico";
                return 5;
            }
            else if (id < 27)// "Deep System Scan"
            {
                _sScanImagePath = "/Images/reg-deep48.ico";
                return 9;
            }
            else
            {
                _sScanImagePath = "/Images/reg-hist48.ico";
                return 5;
            }
        }

        private void InitFields()
        {
            _pnlRegScan = new RegScanPanel();
            _pnlOptions = new OptionsPanel();
            _pnlHelp = new HelpPanel();
            _pnlRegScanActive = new RegScanActivePanel();
            _pnlScanResults = new ScanResultsPanel();
            _bmpMonitor = new BitmapImage(new Uri("/Images/monitor2.png", UriKind.Relative));
            _bmpDrive = new BitmapImage(new Uri("/Images/drive.png", UriKind.Relative));
            _bmpOptions = new BitmapImage(new Uri("/Images/options.png", UriKind.Relative));
            _bmpAbout = new BitmapImage(new Uri("/Images/about.png", UriKind.Relative));

            _RegScan = new cRegScan();
            _RegScan.CurrentPath += new cRegScan.CurrentPathDelegate(RegScan_CurrentPath);
            _RegScan.KeyCount += new cRegScan.KeyCountDelegate(RegScan_KeyCount);
            _RegScan.LabelChange += new cRegScan.LabelChangeDelegate(RegScan_LabelChange);
            _RegScan.MatchItem += new cRegScan.MatchItemDelegate(RegScan_MatchItem);
            _RegScan.ProcessChange += new cRegScan.ProcessChangeDelegate(RegScan_ProcessChange);
            _RegScan.ProcessCompleted += new cRegScan.ProcessCompletedEventHandler(RegScan_ProcessCompleted);
            _RegScan.ScanComplete += new cRegScan.ScanCompleteDelegate(RegScan_ScanComplete);
            _RegScan.ScanCount += new cRegScan.ScanCountDelegate(RegScan_ScanCount);
            _RegScan.StatusChange += new cRegScan.StatusChangeDelegate(RegScan_StatusChange);

            _aTimer = new System.Windows.Threading.DispatcherTimer();
            _aTimer.Interval = new TimeSpan(1000);
            _aTimer.IsEnabled = false;
            _aTimer.Tick += new EventHandler(_aTimer_Tick);

            btnRegscan.IsChecked = true;
            _lResults = new ResultsList();
        }

        private bool HasResults()
        {
            return grdContainer.Children.Contains(_pnlScanResults);
        }

        private void LockControls(bool locked)
        {
            this.stkToolBarPanel.IsEnabled = locked;
          //  this.grdNonClient.IsEnabled = locked;
        }

        private bool RegScanSetup()
        {
            ControlScan = (bool)_pnlRegScan.chkControlScan.IsChecked;
            UserScan = (bool)_pnlRegScan.chkUserScan.IsChecked;
            SoftwareScan = (bool)_pnlRegScan.chkSoftwareScan.IsChecked;
            FontScan = (bool)_pnlRegScan.chkFontScan.IsChecked;
            HelpScan = (bool)_pnlRegScan.chkHelpScan.IsChecked;
            LibraryScan = (bool)_pnlRegScan.chkLibraryScan.IsChecked;
            StartupScan = (bool)_pnlRegScan.chkStartupScan.IsChecked;
            VdmScan = (bool)_pnlRegScan.chkVdmScan.IsChecked;
            HistoryScan = (bool)_pnlRegScan.chkHistoryScan.IsChecked;
            DeepScan = (bool)_pnlRegScan.chkDeepScan.IsChecked;

            if (ScanCount() == 0)
            {
                return false;
            }

            _pnlRegScanActive.stkUserScan.Visibility = UserScan ? Visibility.Visible : Visibility.Collapsed;
            _pnlRegScanActive.stkControlScan.Visibility = ControlScan ? Visibility.Visible : Visibility.Collapsed;
            _pnlRegScanActive.stkSoftwareScan.Visibility = SoftwareScan ? Visibility.Visible : Visibility.Collapsed;
            _pnlRegScanActive.stkFontsScan.Visibility = FontScan ? Visibility.Visible : Visibility.Collapsed;
            _pnlRegScanActive.stkHelpScan.Visibility = HelpScan ? Visibility.Visible : Visibility.Collapsed;
            _pnlRegScanActive.stkLibrariesScan.Visibility = LibraryScan ? Visibility.Visible : Visibility.Collapsed;
            _pnlRegScanActive.stkStartupScan.Visibility = StartupScan ? Visibility.Visible : Visibility.Collapsed;
            _pnlRegScanActive.stkVdmScan.Visibility = VdmScan ? Visibility.Visible : Visibility.Collapsed;
            _pnlRegScanActive.stkHistoryScan.Visibility = HistoryScan ? Visibility.Visible : Visibility.Collapsed;
            _pnlRegScanActive.stkDeepScan.Visibility = DeepScan ? Visibility.Visible : Visibility.Collapsed;

            _RegScan.ScanControl = ControlScan;
            _RegScan.ScanUser = UserScan;
            _RegScan.ScanUninstallStrings = SoftwareScan;
            _RegScan.ScanFont = FontScan;
            _RegScan.ScanHelp = HelpScan;
            _RegScan.ScanSharedDll = LibraryScan;
            _RegScan.ScanStartupEntries = StartupScan;
            _RegScan.ScanVDM = VdmScan;
            _RegScan.ScanHistory = HistoryScan;
            _RegScan.ScanDeep = DeepScan;

            return true;
        }

        private void ResetRegScan()
        {
            // reset panel vars
            _pnlRegScanActive.txtScanPhase.Text = "";
            _sLabel = "";
            _pnlRegScanActive.txtScanningKey.Text = "";
            _sPath = "";
            _pnlRegScanActive.txtKeyCount.Text = "";
            _iKeyCounter = 0;
            _pnlRegScanActive.txtLastMatch.Text = "";
            _sMatch = "";
            _pnlRegScanActive.txtMatchCount.Text = "";
            _iResultsCounter = 0;
            _pnlRegScanActive.txtSegmentsRemaining.Text = "";
            _iSegmentCounter = 0;
            _pnlRegScanActive.txtSegmentsScanned.Text = "";
            // reset data
            _lResults.Clear();
            _pnlScanResults.lstResults.DataContext = null;
            // reset counters
            _bTimerOn = false;
            _iKeyCounter = 0;
            _iResultsCounter = 0;
            _iSegmentCounter = 0;
            _iProgressMax = 0;
            _sLabel = "";
            _sMatch = "";
            _sPath = "";
            _sSegment = "";
            // reset progressbars
            ResetProgressBars();
        }

        private void ResetProgressBars()
        {
            foreach (Object o in _pnlRegScanActive.stkProgressBarPanel.Children)
            {
                if (o.GetType() == typeof(StackPanel))
                {
                    StackPanel p = o as StackPanel;
                    foreach (Object n in p.Children)
                    {
                        if (n.GetType() == typeof(CircularProgressBar.CircularProgressBar))
                        {
                            CircularProgressBar.CircularProgressBar t = n as CircularProgressBar.CircularProgressBar;
                            t.Maximum = 0;
                            t.Value = 0;
                        }
                    }
                }
            }
        }

        private void ResultsCheck()
        {
            if (HasResults())
            {
                CancelRegScan();
            }
        }

        private int ScanCount()
        {
            int i = 0;
            foreach (Object o in _pnlRegScan.stkCheckPanel.Children)
            {
                if (o.GetType() == typeof(CheckBox))
                {
                    CheckBox t = o as CheckBox;
                    if (t.IsChecked == true)
                    {
                        i += 1;
                    }
                }
            }
            return i;
        }

        private void StartRegScan()
        {
            LockControls(false);
            _bTimerOn = true;
            _aTimer.IsEnabled = true;
            _RegScan.AsyncScan();
        }

        private void StopRegScan()
        {
            LockControls(true);
            _bTimerOn = false;
            _aTimer.IsEnabled = false;
            _pnlRegScanActive.btnRegScanCancel.Content = "Next";
        }

        private void TogglePanels(string name)
        {
            grdContainer.Children.Clear();

            switch (name)
            {
                case "btnRegscan":
                    grdContainer.Children.Add(_pnlRegScan);
                    imgStatusBar.Source = _bmpMonitor;
                    txtStatusBar.Text = "Status: Registry Scan Pending..";
                    break;
                case "btnOptions":
                    grdContainer.Children.Add(_pnlOptions);
                    imgStatusBar.Source = _bmpOptions;
                    txtStatusBar.Text = "Status: Review Scanning Options..";
                    break;
                case "btnHelp":
                    grdContainer.Children.Add(_pnlHelp);
                    imgStatusBar.Source = _bmpAbout;
                    txtStatusBar.Text = "Status: Review Help Information..";
                    break;
            }
        }

        private void ToggleToolBarButtons(string button)
        {
            foreach (Object o in this.stkToolBarPanel.Children)
            {
                if (o.GetType() == typeof(ToggleButton))
                {
                    ToggleButton t = o as ToggleButton;
                    if (t.Name != button)
                    {
                        t.IsChecked = false;
                    }
                    else
                    {
                        t.IsChecked = true;
                    }
                }
            }
        }
        #endregion
    }
}
