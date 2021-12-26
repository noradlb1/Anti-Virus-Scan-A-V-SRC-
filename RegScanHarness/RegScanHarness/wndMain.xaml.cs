#region About
/**************************************************************************************
*  ScanX 1.1    Registry Cleaner                                                      *
*                                                                                     *
*  Created:     January 1, 2007 (vb6)                                                 *
*  Updated:     December 31, 2011                                                     *
*  Purpose:     Registry Cleaner Application                                          *
*  Methods:     (listed)                                                              *
*  Revision:    1.1.3                                                                 *
*  IDE:         C# 2008                                                               *
*  Author:      John Underhill (Steppenwolfe)                                         *
*                                                                                     *
***************************************************************************************/
// Use it, abuse it, launch a 1000 registry cleaners into the wild for all I care, but by using this, you must agree that..
// I (the author) relinquish, repudiate, and disavow all and any responsibility for this software from now until the end of time. 
// (and if that statement isn't clear enough..)
// There is also absolutely no guarantee of fitness, implied or otherwise, and no support whatsoever for this software, ever.
#endregion

#region Directives
using System;
using System.Windows;
using ScanX.Implementation;
using ScanX.Panels;
using System.Windows.Controls.Primitives;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using VTRegScan;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;
using System.ComponentModel;
using System.Windows.Threading;
using System.Threading;
#endregion

namespace ScanX
{
    public partial class wndMain : Window
    {
        #region Enums
        private enum ScanTypeEnum
        {
            RegistryScan,
            MruScan,
            None
        }
        #endregion

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
        private static cRestore _Restore = new cRestore();
        private DispatcherTimer _aUpdateTimer;
        private static DispatcherTimer _aRestoreTimer;
        private static bool _bRestoreComplete = false;
        private static bool _bRestoreSucess = false;
        private static int _iRestoreCounter = 0;
        private static int _iKeyCounter = 0;
        private static int _iResultsCounter = 0;
        private static int _iSegmentCounter = 0;
        private static int _iProgressMax = 0;
        private static string _sLabel = "";
        private static string _sPhase = "";
        private static string _sMatch = "";
        private static string _sPath = "";
        private static string _sHive = "";
        private static string _sSegment = "";
        private static int[] _aSubScan;
        private static DateTime _dTime;
        private static TimeSpan _tTimeElapsed;
        private ObservableCollection<ScanData> _Results = new ObservableCollection<ScanData>();
        private Logging _cLog;
        private BackgroundWorker _oProcessAsyncBackgroundWorker = null;
        public delegate void ProcessCompletedEventHandler(object sender, RunWorkerCompletedEventArgs e);
        public event ProcessCompletedEventHandler ProcessCompleted;
        #endregion

        #region Properties
        private bool IsTimerOn { get; set; }
        private bool ControlScan {get; set;}
        private bool UserScan { get; set; }
        private bool SoftwareScan { get; set; }
        private bool FontScan { get; set; }
        private bool HelpScan { get; set; }
        private bool LibraryScan { get; set; }
        private bool StartupScan { get; set; }
        private bool UninstallScan { get; set; }
        private bool VdmScan { get; set; }
        private bool HistoryScan { get; set; }
        private bool DeepScan { get; set; }
        private bool MRUScan { get; set; }
        private bool IsScanLoaded { get; set; }
        private bool IsResetPending { get; set; }
        #endregion
        
        #region Constructor
        public wndMain()
        {
            InitializeComponent();
            InitFields();
            _cLog = new Logging(AppDomain.CurrentDomain.BaseDirectory + "log.txt");
           // cSecurity cs = new cSecurity();
           // cs.ChangeObjectOwnership(@"HKEY_CLASSES_ROOT\CLSID\{722b3793-5367-4446-b6bb-db89b05c1f24}", cSecurity.SE_OBJECT_TYPE.SE_REGISTRY_KEY);
           // cs.ChangeKeyPermissions(cSecurity.ROOT_KEY.HKEY_CLASSES_ROOT, @"CLSID\{722b3793-5367-4446-b6bb-db89b05c1f24}", cs.UserName(cSecurity.EXTENDED_NAME_FORMAT.NameSamCompatible), cSecurity.eRegistryAccess.Registry_Full_Control, cSecurity.eAccessType.Access_Allowed, cSecurity.eInheritenceFlags.Child_Inherit_Level);
        }
        #endregion

        #region Library Events
        private void RegScan_CurrentPath(string hive, string path)
        {
            _sHive = hive;
            _sPath = path;
        }

        private void RegScan_KeyCount()
        {
            _iKeyCounter += 1;
        }

        private void RegScan_LabelChange(string phase, string label)
        {
            _sPhase = phase;
            _sLabel = label;
        }

        private void RegScan_MatchItem(cLightning.ROOT_KEY root, string key, string value, string data, RESULT_TYPE id)
        {
            _sMatch = data;
            _iResultsCounter += 1;
        }

        private void RegScan_ProcessChange()
        {
            _iSegmentCounter += 1;
        }
        
        private void RegScan_ProcessCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            _sSegment = "Scan Complete!";
        }

        private void RegScan_ScanComplete()
        {
            _sSegment = "Scan Complete!";
            this.IsTimerOn = false;
        }

        private void RegScan_ScanCount(int count)
        {
            _iProgressMax = count;
        }

        private void RegScan_StatusChange(string label)
        {
            _sSegment = label;
        }

        private void RegScan_SubScanComplete(string id)
        {
            SubProgressCounter(id);
        }

        private void _aRestoreTimer_Tick(object sender, EventArgs e)
        {
            _iRestoreCounter += 1;
            if (_iRestoreCounter > 1000)
            {
                _iRestoreCounter = 1;
            }
            prgRestore.Value = _iRestoreCounter;
        }

        private void _aUpdateTimer_Tick(object sender, EventArgs e)
        {
            _pnlRegScanActive.txtScanPhase.Text = _sPhase;
            _pnlRegScanActive.txtScanDescription.Text = _sLabel;
            _pnlRegScanActive.txtScanningKey.Text = _sPath;
            _pnlRegScanActive.txtScanningHive.Text = _sHive;
            _pnlRegScanActive.txtKeyCount.Text = _iKeyCounter.ToString();
            if (_sMatch.Length > 0)
            {
                _pnlRegScanActive.txtLastMatch.Text = _sMatch;
            }
            _pnlRegScanActive.txtMatchCount.Text = _iResultsCounter.ToString();
            _pnlRegScanActive.prgMain.Maximum = _iProgressMax;
            _pnlRegScanActive.prgMain.Value = _iSegmentCounter;
            _pnlRegScanActive.txtSegmentsRemaining.Text = (_pnlRegScanActive.prgMain.Maximum - _iSegmentCounter).ToString();
            _pnlRegScanActive.txtSegmentsScanned.Text = _iSegmentCounter.ToString();
            this.txtStatusBar.Text = _sSegment;

            SubProgressUpdate();

            _tTimeElapsed = DateTime.Now.Subtract(_dTime);
            _pnlRegScanActive.txtTimeElapsed.Text = ((int)_tTimeElapsed.TotalSeconds).ToString() + " seconds";
            if (this.IsTimerOn == false)
            {
                _pnlRegScanActive.prgMain.Value = _iProgressMax;
                ScanStop();
            }
        }
        #endregion
        
        #region Overrides
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            if (GlassHelper.ExtendGlass(this, 1, 1, 26, 1))
            {
                grdWindowGrid.Margin = new Thickness(1, 26, 1, 1);
                grdNonClient.Visibility = Visibility.Visible;
            }
            else
            {
                grdWindowGrid.Margin = new Thickness(0, 0, 0, 0);
                grdNonClient.Visibility = Visibility.Collapsed;
            }
        }
        #endregion

        #region Control Events
        private void Button_Clicked(object sender, RoutedEventArgs e)
        {
            // panel button actions
            Button b = e.OriginalSource as Button;
            if (b != null)
            {
                switch (b.Name)
                {
                    case "btnRegScanStart":
                        if ((string)b.Content == "Start")
                        {
                            if (ScanCount() > 0)
                            {
                                LogEntry("Scan Started...");
                                ScanStart();
                            }
                            else
                            {
                                LogEntry("Scan Aborted, No Segments selected.");
                                MessageBox.Show("You do not have any scan segments selected, please check at least one",
                                    "Invalid Selection!", 
                                    MessageBoxButton.OK, MessageBoxImage.Exclamation);
                            }
                        }
                        break;
                    case "btnRegScanCancel":
                        if ((string)b.Content == "Cancel")
                        {
                            LogEntry("Scan Aborted by User.");
                            if (this.IsTimerOn == true)
                            {
                                // cancel out
                                ScanCancel();
                            }
                        }
                        else
                        {
                            TogglePanels("Results");
                        }
                        break;
                    case "btnSelectAll":
                        CheckItems(true);
                        break;
                    case "btnDeselectAll":
                        CheckItems(false);
                        break;
                    case "btnRemove":
                        RemoveItems();
                        this.IsResetPending = true;
                        break;
                    case "btnRestore":
                        ShowRestore();
                        break;
                    case "btnShowLog":
                        ShowLog();
                        break;
                    case "btnHelpMain":
                        OpenApp("http://www.vtdev.com/net/scanx.html");
                        break;
                    case "btnHelpHome":
                        OpenApp("http://www.vtdev.com/");
                        break;
                    case "btnHelpAbout":
                        // load about form
                        wndAbout w = new wndAbout();
                        w.ShowDialog();
                        break;
                }
            }
        }

        private void Link_Click(object sender, MouseButtonEventArgs e)
        {
            Image lnk = (Image)sender;

            if (lnk.Name == "imgAbout" )
            {
                // load about form
                wndAbout w = new wndAbout();
                w.ShowDialog();
            }
        }

        private void ToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            ToggleButton tb = (ToggleButton)sender;
            if (!tb.IsChecked == true)
            {
                tb.IsChecked = true;
                return;
            }
            ToggleToolBarButtons(tb.Name);
            TogglePanels(tb.Name);
        }
        #endregion

        #region Helpers
        private void CheckItems(bool check)
        {
            if (_Results != null)
            {
                foreach (ScanData o in _Results)
                {
                    o.Check = check;
                }
            }
        }

        private static void DoEvents()
        {
            DispatcherFrame frame = new DispatcherFrame(true);
            Dispatcher.CurrentDispatcher.BeginInvoke(
            DispatcherPriority.Background, (SendOrPostCallback)delegate(object arg)
            {
                var f = arg as DispatcherFrame;
                f.Continue = false;
            },
            frame
            );
            Dispatcher.PushFrame(frame);
        } 

        private string IdToImage(int id)
        {
            if (id < 11)// "Control Scan"
            {
                return "/Images/regctrl.png";
            }
            else if (id == 11)// "User Scan"
            {
                return "/Images/reguser.png";
            }
            else if (id < 15)// "System Software"
            {
                return "/Images/regsystem.png";
            }
            else if (id == 15)// "System Fonts"
            {
                return "/Images/regfont.png";
            }
            else if (id == 16)// "System Help Files"
            {
                return "/Images/reghelp.png";
            }
            else if (id == 17)// "Shared Libraries"
            {
                return "/Images/reglib.png";
            }
            else if (id == 18)// "Startup Entries"
            {
                return "/Images/regstart.png";
            }
            else if (id == 19)// "Installation Strings"
            {
                return "/Images/reginst.png";
            }
            else if (id == 20)// "Virtual Devices"
            {
                return "/Images/regvdf.png";
            }
            else if (id < 25)// "History and Start Menu"
            {
                return "/Images/reghist.png";
            }
            else if (id < 27)// "Deep System Scan"
            {
                return "/Images/regdeep.png";
            }
            else
            {
                return "/Images/regmru.png";
            }
        }

        private void InitFields()
        {
            _pnlRegScan = new RegScanPanel();
            _pnlOptions = new OptionsPanel();
            _pnlHelp = new HelpPanel();
            _pnlRegScanActive = new RegScanActivePanel();
            _pnlScanResults = new ScanResultsPanel();
            _bmpMonitor = new BitmapImage(new Uri("/Images/monitor.png", UriKind.Relative));
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
            _RegScan.SubScanComplete += new cRegScan.SubScanCompleteDelegate(RegScan_SubScanComplete);
            _RegScan.ScanCount += new cRegScan.ScanCountDelegate(RegScan_ScanCount);
            _RegScan.StatusChange += new cRegScan.StatusChangeDelegate(RegScan_StatusChange);
            // text updates
            _aUpdateTimer = new System.Windows.Threading.DispatcherTimer();
            _aUpdateTimer.Interval = new TimeSpan(1000);
            _aUpdateTimer.IsEnabled = false;
            _aUpdateTimer.Tick += new EventHandler(_aUpdateTimer_Tick);
            btnRegscan.IsChecked = true;
            // restore timer
            _aRestoreTimer = new System.Windows.Threading.DispatcherTimer();
            _aRestoreTimer.Interval = new TimeSpan(5000);
            _aRestoreTimer.IsEnabled = false;
            _aRestoreTimer.Tick += new EventHandler(_aRestoreTimer_Tick);
            // add the panels
            grdContainer.Children.Add(_pnlRegScanActive);
            grdContainer.Children.Add(_pnlScanResults);
            grdContainer.Children.Add(_pnlOptions);
            grdContainer.Children.Add(_pnlHelp);
        }

        private void UnLockControls(bool unlocked)
        {
            this.stkToolBarPanel.IsEnabled = unlocked;
            this.grdNonClient.IsEnabled = unlocked;
        }

        private void LogDetail(ScanData data, bool result)
        {
            if (Properties.Settings.Default.SettingDetails == true)
            {
                string entry = "";

                if (result)
                {
                    entry = "Attempted deletion of value: " + data.Root.ToString() + @"\" + data.Key + " successful.";
                }
                else
                {
                    entry = "Attempted deletion of value: " + data.Root.ToString() + @"\" + data.Key + " failed.";
                }
                _cLog.WriteLine(entry);
            }
        }

        private void LogEntry(string entry)
        {
            if (Properties.Settings.Default.SettingLog == true)
            {
                _cLog.WriteLog(entry);
            }
        }

        private void ModSecVal(cLightning.ROOT_KEY RootKey, string SubKey, cSecurity.eInheritenceFlags flags)
        {
            string sKey = RootKey.ToString();
            cSecurity sec = new cSecurity();
            string name = sec.UserName(cSecurity.EXTENDED_NAME_FORMAT.NameSamCompatible);

            if (name == null)
            {
                name = sec.UserName();
            }
            sKey += @"\" + SubKey;
            sec.ChangeObjectOwnership(sKey, cSecurity.SE_OBJECT_TYPE.SE_REGISTRY_KEY);
            sec.ChangeKeyPermissions((cSecurity.ROOT_KEY)RootKey, SubKey, name, cSecurity.eRegistryAccess.Registry_Full_Control, cSecurity.eAccessType.Access_Allowed, flags);
        }

        private void OpenApp(string path)
        {
            cLightning lgt = new cLightning();
            lgt.ShellOpen(path, cLightning.SHOW_COMMANDS.SW_SHOWNOACTIVATE);
        }

        private void RemoveItems()
        {
            bool res = false;
            bool val = false;
            bool ret = false;
            int items = 0;

            // test for checked items first
            foreach (ScanData o in _Results)
            {
                if (o.Check == true)
                {
                    val = true;
                    break;
                }
            }
            if (val)
            {
                //set a restore point

                res = (bool)Properties.Settings.Default.SettingRestore;

                if (res)
                {
                    MessageBoxResult chc = MessageBox.Show("Would you like to create a System Restore Point before proceeding? The Restore process may take several minutes to complete..",
                        "System Restore", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (chc == MessageBoxResult.Yes)
                    {
                        // restore visual
                        RestoreProgressStart();
                        if (!_bRestoreSucess)
                        {
                            RestoreProgressStop();
                            res = false;
                            MessageBoxResult msg = MessageBox.Show("System Restore is either disabled or unavailable on this computer. " +
                                "Do you wish to set up System Restore before proceeding? Without System Restore, changes to your system can Not be Undone",
                                        "Restore Disabled!", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
                            if (msg == MessageBoxResult.Yes)
                            {
                                if (!ShowProtection())
                                {
                                    _pnlOptions.chkRestore.IsChecked = false;
                                }
                                return;
                            }
                            else if (msg == MessageBoxResult.No)
                            {
                                LogEntry("System Restore deemed unavailable or deactivated. Option disabled.");
                                _pnlOptions.chkRestore.IsChecked = false;
                            }
                            else
                            {
                                return;
                            }
                        }
                        else
                        {
                            RestoreProgressStop();
                        }
                    }
                }

                cLightning lightning = new cLightning();

                // iterate through and remove
                foreach (ScanData o in _Results)
                {
                    if (o.Check == true)
                    {
                        switch (o.Id)
                        {
                            // delete value
                            case 1:
                            case 2:
                            case 3:
                            case 4:
                            case 5:
                            case 7:
                            case 9:
                            case 10:
                            case 11:
                            case 12:
                            case 13:
                            case 14:
                            case 15:
                            case 16:
                            case 17:
                            case 18:
                            case 19:
                            case 21:
                            case 22:
                            case 23:
                            case 24:
                            case 25:
                            case 26:
                            case 27:
                                {
                                    if (o.Value == "Default")
                                    {
                                        o.Value = "";
                                    }
                                    ret = lightning.DeleteValue(o.Root, o.Key, o.Value);
                                    if (ret == false)
                                    {
                                        ModSecVal(o.Root, o.Key, cSecurity.eInheritenceFlags.Child_Inherit_Level);
                                        ret = lightning.DeleteValue(o.Root, o.Key, o.Value);
                                    }
                                    items += 1;
                                    break;
                                }
                            // delete key
                            case 6:
                            case 8:
                                {
                                    ret = (lightning.DeleteKey(o.Root, o.Key));
                                    if (ret == false)
                                    {
                                        ModSecVal(o.Root, o.Key, cSecurity.eInheritenceFlags.Container_Inherit);
                                        ret = lightning.DeleteValue(o.Root, o.Key, o.Value);
                                    }
                                    items += 1;
                                    break;
                                }
                            // recreate value
                            case 20:
                                {
                                    ret = (lightning.DeleteValue(o.Root, o.Key, o.Value));
                                    lightning.WriteMulti(o.Root, o.Key, "VDD", "");
                                    items += 1;
                                    break;
                                }

                        }
                        LogDetail(o, ret);
                    }
                }
                // finalize restore
                if (res)
                {
                    _Restore.EndRestore(false);
                }
                LogEntry("Items removal action completed successfully.");
                ScanCancel();
            }
            else
            {
                MessageBoxResult can = MessageBox.Show("You haven't selected any items for removal. To Reset the Scan Results, select Cancel",
                        "No Items Selected", MessageBoxButton.OKCancel, MessageBoxImage.Exclamation);
                if (can == MessageBoxResult.Cancel)
                {
                    LogEntry("Scan Results were Reset.");
                    ScanCancel();
                }
            }
        }

        private void Reset()
        {
            ResetEngine();
            ResetTimer();
            ResetData();
            ResetContext();
            ResetProgressBars();
        }

        private void ResetContext()
        {
            // reset panel vars
            _pnlRegScanActive.txtScanPhase.Text = "";
            _sLabel = "";
            _pnlRegScanActive.txtScanningKey.Text = "";
            _sPath = "";
            _pnlRegScanActive.txtScanningHive.Text = "";
            _sHive = "";
            _pnlRegScanActive.txtKeyCount.Text = "";
            _iKeyCounter = 0;
            _pnlRegScanActive.txtLastMatch.Text = "";
            _sMatch = "";
            _pnlRegScanActive.txtMatchCount.Text = "";
            _iResultsCounter = 0;
            _pnlRegScanActive.txtSegmentsRemaining.Text = "";
            _iSegmentCounter = 0;
            _pnlRegScanActive.txtSegmentsScanned.Text = "";
            _pnlRegScanActive.btnRegScanCancel.Content = "Cancel";
            // reset counters
            this.IsTimerOn = false;
            _iKeyCounter = 0;
            _iResultsCounter = 0;
            _iSegmentCounter = 0;
            _iProgressMax = 0;
            _sSegment = "";
            
            this.IsScanLoaded = false;
        }

        private void ResetData()
        {
            if (_pnlScanResults.lstResults.DataContext != null)
            {
                _pnlScanResults.lstResults.ItemsSource = null;
                _pnlScanResults.lstResults.DataContext = null;
            }
            if (_RegScan.Data.Count > 0)
            {
                _Results = new ObservableCollection<ScanData>(_RegScan.Data);

                foreach (ScanData o in _Results)
                {
                    o.ImagePath = IdToImage(o.Id);
                }
                _pnlScanResults.lstResults.ItemsSource = _Results;
                _pnlScanResults.lstResults.DataContext = _Results;
            }
        }

        private void ResetEngine()
        {
            _RegScan.CancelProcessAsync(); 
            _RegScan.Data.Clear();
            this.IsResetPending = true;
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
                            t.Maximum = 100;
                            t.Value = 0;
                        }
                    }
                }
            }
        }

        private void ResetTimer()
        {
            _aUpdateTimer.IsEnabled = false;
        }

        private void RestoreProgressStart()
        {
            _dTime = DateTime.Now;
            _bRestoreComplete = false;
            _aRestoreTimer.IsEnabled = true;
            grdRestore.Visibility = Visibility.Visible;
            _pnlScanResults.lstResults.IsEnabled = false;
            UnLockControls(false);
            // launch restore on a new thread
            _oProcessAsyncBackgroundWorker = new BackgroundWorker();
            _oProcessAsyncBackgroundWorker.WorkerSupportsCancellation = true;
            _oProcessAsyncBackgroundWorker.DoWork += new DoWorkEventHandler(_oProcessAsyncBackgroundWorker_DoWork);
            _oProcessAsyncBackgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(_oProcessAsyncBackgroundWorker_RunWorkerCompleted);
            _oProcessAsyncBackgroundWorker.RunWorkerAsync();

            double safe = 0;
            do
            {
                DoEvents();
                _tTimeElapsed = DateTime.Now.Subtract(_dTime);
                safe = _tTimeElapsed.TotalSeconds;
                // break at 5 minutes, something has gone wrong
                if (safe > 300)
                {
                    break;
                }
            } while (_bRestoreComplete != true);
        }

        private void RestoreProgressStop()
        {
            grdRestore.Visibility = Visibility.Collapsed;
            _pnlScanResults.lstResults.IsEnabled = true;
            UnLockControls(true);
            _aRestoreTimer.IsEnabled = false;
            _iRestoreCounter = 0;
        }

        private void _oProcessAsyncBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            _bRestoreComplete = true;
        }

        private void _oProcessAsyncBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            _bRestoreComplete = false;
            _bRestoreSucess = _Restore.StartRestore("Scan-X Restore Point");
        }

        private void ShowLog()
        {
            if (_RegScan.FileExists(_cLog.FilePath))
            {
                Process.Start(_cLog.FilePath);
            }
        }

        private void ShowRestore()
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.System) + @"\rstrui.exe";
            if (_RegScan.FileExists(path))
            {
                Process.Start(path);
            }
        }

        private bool ShowProtection()
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.System) + @"\SystemPropertiesProtection.exe";
            if (_RegScan.FileExists(path))
            {
                if (Process.Start(path) != null)
                {
                    return true;
                }
            }
            return false;
        }

        private void ScanCancel()
        {
            Reset();
            UnLockControls(true);
            ToggleToolBarButtons("btnRegscan");
            TogglePanels("btnRegscan");
        }

        private void ScanComplete(int items)
        {
            UnLockControls(true);
            this.IsTimerOn = false;
            _aUpdateTimer.IsEnabled = false;
            _pnlRegScanActive.btnRegScanCancel.Content = "Status: Registry Scan Completed.. " + items.ToString() + " removed..";
            ResetContext();
            ToggleToolBarButtons("btnRegscan");
            TogglePanels("btnRegscan");
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

        private bool ScanSetup()
        {
            RegScanPanel panel = (RegScanPanel)grdContainer.Children[0];
            this.ControlScan = (bool)panel.chkControlScan.IsChecked == true;
            this.UserScan = (bool)panel.chkUserScan.IsChecked == true;
            this.SoftwareScan = (bool)panel.chkSoftwareScan.IsChecked == true;
            this.FontScan = (bool)panel.chkFontScan.IsChecked == true;
            this.HelpScan = (bool)panel.chkHelpScan.IsChecked == true;
            this.LibraryScan = (bool)panel.chkLibraryScan.IsChecked == true;
            this.StartupScan = (bool)panel.chkStartupScan.IsChecked == true;
            this.UninstallScan = (bool)panel.chkUninstallScan.IsChecked == true;
            this.VdmScan = (bool)panel.chkVdmScan.IsChecked == true;
            this.HistoryScan = (bool)panel.chkHistoryScan.IsChecked == true;
            this.DeepScan = (bool)panel.chkDeepScan.IsChecked == true;
            this.MRUScan = (bool)panel.chkMruScan.IsChecked == true;

            int c = ScanCount();
            _pnlRegScanActive.prgMain.Maximum = c;

            if (ScanCount() == 0)
            {
                return false;
            }
            IsScanLoaded = true;

            _aSubScan = new int[12];

            ResetProgressBars();

            _pnlRegScanActive.stkUserScan.Visibility = this.UserScan ? Visibility.Visible : Visibility.Collapsed;
            _pnlRegScanActive.stkControlScan.Visibility = this.ControlScan ? Visibility.Visible : Visibility.Collapsed;
            _pnlRegScanActive.stkSoftwareScan.Visibility = this.SoftwareScan ? Visibility.Visible : Visibility.Collapsed;
            _pnlRegScanActive.stkFontsScan.Visibility = this.FontScan ? Visibility.Visible : Visibility.Collapsed;
            _pnlRegScanActive.stkHelpScan.Visibility = this.HelpScan ? Visibility.Visible : Visibility.Collapsed;
            _pnlRegScanActive.stkLibrariesScan.Visibility = this.LibraryScan ? Visibility.Visible : Visibility.Collapsed;
            _pnlRegScanActive.stkStartupScan.Visibility = this.StartupScan ? Visibility.Visible : Visibility.Collapsed;
            _pnlRegScanActive.stkUninstall.Visibility = this.UninstallScan ? Visibility.Visible : Visibility.Collapsed;
            _pnlRegScanActive.stkVdmScan.Visibility = this.VdmScan ? Visibility.Visible : Visibility.Collapsed;
            _pnlRegScanActive.stkHistoryScan.Visibility = this.HistoryScan ? Visibility.Visible : Visibility.Collapsed;
            _pnlRegScanActive.stkDeepScan.Visibility = this.DeepScan ? Visibility.Visible : Visibility.Collapsed;
            _pnlRegScanActive.stkMru.Visibility = this.MRUScan ? Visibility.Visible : Visibility.Collapsed;

            _RegScan.ScanControl = this.ControlScan;
            _RegScan.ScanUser = this.UserScan;
            _RegScan.ScanFile = this.SoftwareScan;
            _RegScan.ScanFont = this.FontScan;
            _RegScan.ScanHelp = this.HelpScan;
            _RegScan.ScanSharedDll = this.LibraryScan;
            _RegScan.ScanStartupEntries = this.StartupScan;
            _RegScan.ScanUninstallStrings = this.UninstallScan;
            _RegScan.ScanVDM = this.VdmScan;
            _RegScan.ScanHistory = this.HistoryScan;
            _RegScan.ScanDeep = this.DeepScan;
            _RegScan.ScanMru = this.MRUScan;

            return true;
        }

        private void ScanStart()
        {
            _dTime = DateTime.Now;
            UnLockControls(false);
            this.IsTimerOn = true;
            _aUpdateTimer.IsEnabled = true;
            TogglePanels("Active");
            ScanSetup();
            _RegScan.AsyncScan();
        }

        private void ScanStop()
        {
            ResetData();
            UnLockControls(true);
            this.IsTimerOn = false;
            _aUpdateTimer.IsEnabled = false;
            _pnlRegScanActive.btnRegScanCancel.Content = "Next";
        }

        private static void SubProgressCounter(string id)
        {
            switch (id)
            {
                case "CONTROL":
                    _aSubScan[0] = 100;
                    break;
                case "USER":
                    _aSubScan[1] = 100;
                    break;
                case "SOFTWARE":
                    _aSubScan[2] = 100;
                    break;
                case "FONT":
                    _aSubScan[3] = 100;
                    break;
                case "HELP":
                    _aSubScan[4] = 100;
                    break;
                case "SHAREDDLL":
                    _aSubScan[5] = 100;
                    break;
                case "STARTUP":
                    _aSubScan[6] = 100;
                    break;
                case "UNINSTALL":
                    _aSubScan[7] = 100;
                    break;
                case "VDM":
                    _aSubScan[8] = 100;
                    break;
                case "HISTORY":
                    _aSubScan[9] = 100;
                    break;
                case "DEEP":
                    _aSubScan[10] = 100;
                    break;
                case "MRU":
                    _aSubScan[11] = 100;
                    break;
            }
        }

        private void SubProgressUpdate()
        {
            _pnlRegScanActive.prgControlScan.Value = _aSubScan[0];
            _pnlRegScanActive.prgUserScan.Value = _aSubScan[1];
            _pnlRegScanActive.prgSoftwareScan.Value = _aSubScan[2];
            _pnlRegScanActive.prgFontScan.Value = _aSubScan[3];
            _pnlRegScanActive.prgHelpScan.Value = _aSubScan[4];
            _pnlRegScanActive.prgLibrariesScan.Value = _aSubScan[5];
            _pnlRegScanActive.prgStartupScan.Value = _aSubScan[6];
            _pnlRegScanActive.prgUninstallScan.Value = _aSubScan[7];
            _pnlRegScanActive.prgVdmScan.Value = _aSubScan[8];
            _pnlRegScanActive.prgHistoryScan.Value = _aSubScan[9];
            _pnlRegScanActive.prgDeepScan.Value = _aSubScan[10];
            _pnlRegScanActive.prgMruScan.Value = _aSubScan[11];
        }

        private void TogglePanels(string name)
        {
            //0-_pnlRegScan
            //1-_pnlRegScanActive
            //2-_pnlScanResults
            //3-_pnlOptions
            //4-_pnlHelp
            if (this.IsScanLoaded)
            {
                ResetContext();
            }
            // reset visibility
            foreach (UIElement o in grdContainer.Children)
            {
                o.Visibility = Visibility.Collapsed;
            }
            // toggle visible panel
            switch (name)
            {
                case "btnRegscan":
                    if (!this.IsResetPending && _Results.Count > 0)
                    {
                        grdContainer.Children[2].Visibility = Visibility.Visible;
                    }
                    else
                    {
                        this.IsResetPending = false;
                        if (_Results.Count > 0)
                        {
                            Reset();
                        }
                        grdContainer.Children[0].Visibility = Visibility.Visible;
                        imgStatusBar.Source = _bmpMonitor;
                        txtStatusBar.Text = "Status: Registry Scan Pending..";
                    }
                    break;
                case "Active":
                    grdContainer.Children[1].Visibility = Visibility.Visible;
                    imgStatusBar.Source = _bmpMonitor;
                    txtStatusBar.Text = "Status: Scanning Registry..";
                    break;
                case "Results":
                    grdContainer.Children[2].Visibility = Visibility.Visible;
                    imgStatusBar.Source = _bmpMonitor;
                    txtStatusBar.Text = "Status: Scan Completed!";
                    break;
                case "btnOptions":
                    grdContainer.Children[3].Visibility = Visibility.Visible;
                    imgStatusBar.Source = _bmpOptions;
                    txtStatusBar.Text = "Review Scanning Options..";
                    break;
                case "btnHelp":
                    grdContainer.Children[4].Visibility = Visibility.Visible;
                    imgStatusBar.Source = _bmpAbout;
                    txtStatusBar.Text = "Review Help Information..";
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
