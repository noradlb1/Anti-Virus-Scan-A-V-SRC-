#region Directives
using System;
using System.IO;
using System.Collections;
using System.ComponentModel;
using System.Text;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Security.Permissions;
#endregion

#region Notes
/// "Control Scan"
/// "User Scan"
/// "System Software"
/// "System Fonts"
/// "System Help Files"
/// "Shared Libraries"
/// "Startup Entries"
/// "Installation Strings"
/// "Virtual Devices"
/// "History and Start Menu"
/// "Deep System Scan"

/// CONTEXT ID STRINGS:
/// --ControlScan--
/// AppIDPaths              -1
/// ProcServerPaths         -2
/// TypeLibPaths            -3
/// InterfacePaths          -4 type             -5 proxy
/// TypePaths               -6 help             -7 win32
/// ClassSubPaths           -8 ext              -9 open             -10 edit
/// --UserScan--
/// userscan                -11 usr
/// --FullScan--
/// fullscan                -12 class name      -13 clsid           -14 icon
/// --FontScan--
/// fontscan                -15 paths
/// --HelpScan--
/// help                    -16 html/chm
/// --Shared Libraries--
/// LibShared               -17 lib
/// --Startup--
/// Startup Enries          -18 path
/// --Install Scan--
/// UnInstall Strings       -19 path
/// --Virtual Devices--
/// VDF Check               -20 repair
/// --Link Scan--
/// History and Start       -21 expl            -22 start               -23 linkhist               -24 linkstart
/// --Deep Scan--
/// DeepScan                -25 ms              -26 sft

/// Phase 1 -CLSID
/// 1) valid registration  - HKEY_CLASSES_ROOT\CLSID\..\InProcSvr32(+)
/// 2) typelib paths       - HKEY_CLASSES_ROOT\..
/// 3) appid paths         - HKEY_CLASSES_ROOT\CLSID\... Val-AppID <-> HKEY_CLASSES_ROOT\AppID
/// Phase 2 -Interface
/// 4) type lib paths      - HKEY_CLASSES_ROOT\Interface\TypeLib <-> CLSID\TypeLib
/// 5) interface paths     - HKEY_CLASSES_ROOT\Interface\...\ProxyStubClsid32 <-> CLSID
/// Phase 3 -TypeLib
/// 6) empty keys          - HKEY_CLASSES_ROOT\TypeLib\..\HELPDIR
/// 7) typelib paths       - HKEY_CLASSES_ROOT\TypeLib\..\..\win32
/// Phase 4 -File Extensions
/// 8) empty ext keys      - HKEY_CLASSES_ROOT\...

/// -File Types -6 step-
/// 1) test ext names      - HKEY_CLASSES_ROOT\
/// 2) test class names    - HKEY_CLASSES_ROOT\.xxx DefVal <-> HKEY_CLASSES_ROOT\..
/// 3) test clsid          - HKEY_CLASSES_ROOT\..\CLSID DefVal <-> HKEY_CLASSES_ROOT\CLSID
/// 4) object menu path    - HKEY_CLASSES_ROOT\..\shell\edit\command
/// 5) object open path    - HKEY_CLASSES_ROOT\..\shell\open\command
/// 5) object icon path    - HKEY_CLASSES_ROOT\..\DefaultIcon
/// 6) empty ext key       - HKEY_CLASSES_ROOT\...

/// -Help Files -2 step-
/// 1) path test           - HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\Help
/// 2) path test           - HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\HTML Help

/// -Font Scan-
/// - path test            - HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Fonts

/// -History Lists-
/// - path test            - HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\MenuOrder\Start Menu\Programs

/// -ShortCuts -3 step-
/// 1) path link test      - HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\ComDlg32\OpenSaveMRU - file list
/// 2) binary link test    - HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\FileExts - explorer
/// 3) binary link test    - HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\UserAssist\...\Count - start menu

/// -Shared Dlls-
/// - file paths           - HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\SharedDLLs

/// -Deep Scan-
/// - test path values     - HKEY_LOCAL_MACHINE\SOFTWARE

/// -Current User-
/// - test path values     - HKEY_CURRENT_USER\Software\Microsoft\VBExpress\8.0\FileMRUList

/// -Startup Entries -3 step-
/// 1) test paths          - HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Run
/// 2) test paths          - HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\RunOnce
/// 3) test paths          - HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\RunOnceEx

/// -Uninstall Strings-
/// -test paths            - HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall

/// Virtual Devices
/// -value test            - HKEY_LOCAL_MACHINE\System\ControlSet\Control\VirtualDeviceDrivers

/// Integrity Test
/// -compare keys          - HKEY_USERS\... <-> HKEY_CURRENT_USER
#endregion

namespace VTRegScan
{
    #region ScanData
    public class ScanData : INotifyPropertyChanged
    {
        string k = "";
        string v = "";
        string d = "";
        string c = "";
        string n = "";
        int i = 0;
        int s = 0;
        bool b = false;
        cLightning.ROOT_KEY r = cLightning.ROOT_KEY.HKEY_CURRENT_USER;
        public ScanData(cLightning.ROOT_KEY root, string key, string value, string data, string img, string name, int scope, int id)
        {
            r = root;
            k = key;
            v = value;
            d = data;
            c = img;
            n = name;
            i = id;
            s = scope;
        }
        public cLightning.ROOT_KEY Root
        {
            get { return r; }
            set { r = value; OnPropertyChanged("Root"); }
        }
        public string Key
        {
            get { return k; }
            set { k = value; OnPropertyChanged("Key"); }
        }
        public string Value
        {
            get { return v; }
            set { v = value; OnPropertyChanged("Value"); }
        }
        public string Data
        {
            get { return d; }
            set { d = value; OnPropertyChanged("Data"); }
        }
        public bool Check
        {
            get { return b; }
            set { b = value; OnPropertyChanged("Check"); }
        }
        public string ImagePath
        {
            get { return c; }
            set { c = value; OnPropertyChanged("ImagePath"); }
        }
        public string Name
        {
            get { return n; }
            set { n = value; OnPropertyChanged("Name"); }
        }
        public int Id
        {
            get { return i; }
            set { i = value; OnPropertyChanged("Id"); }
        }
        public int Scope
        {
            get { return s; }
            set { s = value; OnPropertyChanged("Scope"); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(String info)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(info));
            }
        }
    }
    #endregion

    #region Public Enums
    public enum RESULT_TYPE
    {                           //del strat:
        ControlAppID = 1,       //1     value
        ControlProcServer,      //2     value
        ControlTypeLib,         //3     value
        ControlInterfaceType,   //4     value
        ControlInterfaceProxy,  //5     value
        ControlTypeHelp,        //6     key
        ControlTypeWin32,       //7     value
        ControlClassSubExt,     //8     key
        ControlClassSubOpen,    //9     value
        ControlClassSubEdit,    //10    value
        User,                   //11    value
        FullClassName,          //12    value
        FullClsid,              //13    value
        FullIcon,               //14    value
        Font,                   //15    value
        Help,                   //16    value
        Shared,                 //17    value
        Startup,                //18    value
        Uninstall,              //19    value
        Vdf,                    //20    value
        HistoryExplorer,        //21    value
        HistoryStart,           //22    value
        HistoryLink,            //23    value
        HistoryMenu,            //24    value
        DeepMs,                 //25    value
        DeepSft,                //26    value
        Mru                     //27    value
    }
    #endregion

    [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
    public class cRegScan
    {
        #region Constants
        private const int INVALID_HANDLE_VALUE = -1;
        private const string CHR_PERIOD     = ".";
        private const string CHR_BSLASH     = @"\";
        private const string CHR_FSLASH     = "/";
        private const string CHR_PERC       = "%";
        private const string CHR_DASH       = "-";
        private const string CHR_COMMA      = ",";
        private const string CHR_COLAN      = ":";
        private const string CHR_ASTERISK   = "*";
        private const string CHR_SPACE      = " ";
        private const string CHR_TILDE      = "~";
        private const string STR_KILO       = "KB";
        private const string STR_PACK       = "SERVICE PACK";
        private const string STR_UIST       = "UninstallString";
        private const string STR_MS         = "MICROSOFT";
        private const string STR_RC         = "RECYCLE";
        private const string STR_POL        = "POLICIES";
        private const string STR_MSPATH     = @"SOFTWARE\Microsoft\";
        private const string STR_INSPRP     = "InstallProperties";

        private const string STR_DLL        = "DLL";
        private const string STR_DBLZERO    = "00";
        private const string STR_CLASS      = "CLSID";
        private const string STR_CLASSB     = @"CLSID\";
        private const string STR_TYPE       = "TypeLib";
        private const string STR_TYPEB      = @"TypeLib\";
        private const string STR_INTERFACE  = "Interface";
        private const string STR_INTERFACEB = @"Interface\";
        private const string STR_DEFICON    = @"\DefaultIcon";
        private const string STR_DEFAULT    = "Default";
        private const string STR_EMPTY      = "Value Not Set";
        private const string STR_NONAME     = "Value Name Empty";
        private const string STR_APPID      = "AppID";
        private const string STR_WIN32      = "WIN32";
        private const string STR_PROXY      = "ProxyStubClsid32";
        private const string STR_OWLIST     = "OpenWithList";
        private const string STR_OWLISTB    = @"\OpenWithList";
        private const string STR_OWPROG     = "OpenWithProgids";
        private const string STR_OWPROGB    = @"\OpenWithProgids";
        private const string STR_SHELLOPEN  = @"\shell\open\command";
        private const string STR_SHELLEDIT  = @"\shell\edit\command";

        private const string HIST_START     = @"Software\Microsoft\Windows\CurrentVersion\Explorer\UserAssist";
        private const string HIST_EXPLORER  = @"Software\Microsoft\Windows\CurrentVersion\Explorer\FileExts";
        private const string HIST_EXPLORERB = @"Software\Microsoft\Windows\CurrentVersion\Explorer\FileExts\";
        private const string HIST_FILE      = @"Software\Microsoft\Windows\CurrentVersion\Explorer\ComDlg32\OpenSaveMRU";
        private const string HIST_FILEB     = @"Software\Microsoft\Windows\CurrentVersion\Explorer\ComDlg32\OpenSaveMRU\";
        private const string HIST_FILEPID   = @"Software\Microsoft\Windows\CurrentVersion\Explorer\ComDlg32\OpenSavePidlMRU";
        private const string HIST_FILEPIDB  = @"Software\Microsoft\Windows\CurrentVersion\Explorer\ComDlg32\OpenSavePidlMRU\";
        private const string LINK_START     = @"Software\Microsoft\Windows\CurrentVersion\Explorer\MenuOrder\Start Menu\Programs";
        private const string LINK_STARTB    = @"Software\Microsoft\Windows\CurrentVersion\Explorer\MenuOrder\Start Menu\Programs\";

        private const string MRU_DOCUMENTS  = @"Software\Microsoft\Windows\CurrentVersion\Explorer\RecentDocs";
        private const string MRU_TYPEDURLS  = @"Software\Microsoft\Internet Explorer\TypedURLs";
        private const string MRU_RUNMRU     = @"Software\Microsoft\Windows\CurrentVersion\Explorer\RunMRU";
        private const string MRU_FILESEARCH = @"Software\Microsoft\Search Assistant\ACMru\5603";
        private const string MRU_INTERNET   = @"Software\Microsoft\Search Assistant\ACMru\5001";
        private const string MRU_PPL        = @"Software\Microsoft\Search Assistant\ACMru\5647";
        private const string MRU_WORDPAD    = @"Software\Microsoft\Windows\CurrentVersion\Applets\Wordpad\Recent File List";
        private const string MRU_REGFAV     = @"Software\Microsoft\Windows\CurrentVersion\Applets\Regedit\Favorites";
        private const string MRU_REGEDIT    = @"Software\Microsoft\Windows\CurrentVersion\Applets\Regedit";
        private const string MRU_PAINT      = @"Software\Microsoft\Windows\CurrentVersion\Applets\Paint\Recent File List";
        private const string MRU_COMDLG     = @"Software\Microsoft\Windows\CurrentVersion\Explorer\ComDlg32\LastVisitedMRU";
        private const string MRU_EXPLORER   = @"Software\Microsoft\Windows\CurrentVersion\Explorer\Wallpaper\MRU";
        private const string MRU_MEDREC     = @"Software\Microsoft\MediaPlayer\Player\RecentFileList";
        private const string MRU_MEDURL     = @"Software\Microsoft\MediaPlayer\Player\RecentURLList";

        private const string FLT_COM        = ".COM";
        private const string STR_PATH       = @":\";
        private const string STR_VDD        = "VDD";
        private const string STR_HELP       = "HELPDIR";
        private const string STR_PROC32     = "InProcServer32";
        private const string STR_PROC32B    = @"\INPROCSERVER32";
        private const string STR_PROC       = "InProcServer";
        private const string STR_PROCB      = @"\INPROCSERVER";
        private const string STR_LOCAL32    = "LocalServer32";
        private const string STR_LOCAL32B   = @"\LOCALSERVER32";
        private const string STR_LOCAL      = "LocalServer";
        private const string STR_LOCALB     = @"\LOCALSERVER";
        private const string STR_EMPTYKEY   = "Empty Key";
        private const string STR_EMPTYVALUE = "Empty Value";

        private const string APP_REG        = "REGSVR32.EXE";
        private const string APP_RUN        = "RUNDLL32.EXE";
        private const string APP_MSIE       = "MSIEXEC.EXE";
        private const string APP_START      = "START";
        private const string REG_HKCU       = @"HKEY_CURRENT_USER\";
        private const string REG_HKLM       = @"HKEY_LOCAL_MACHINE\";
        private const string REG_HKCR       = @"HKEY_CLASSES_ROOT\";
        private const string REG_HKCUB      = @"HKEY_CURRENT_USER";
        private const string REG_HKLMB      = @"HKEY_LOCAL_MACHINE";
        private const string REG_HKCRB      = @"HKEY_CLASSES_ROOT";
        private const string REG_HKLMCLSID  = @"HKEY_LOCAL_MACHINE\SOFTWARE\Classes\CLSID\";
        private const string REG_HKCRCLSID  = @"HKEY_CLASSES_ROOT\CLSID\";
        private const string REG_HKLMFONTS  = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Fonts";
        private const string REG_HKLMHELP   = @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\Help";
        private const string REG_HKLMSHARE  = @"SOFTWARE\Microsoft\Windows\CurrentVersion\SharedDLLs";
        private const string REG_HKLMUISL   = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\";
        private const string REG_HKLMVDEV   = @"SYSTEM\ControlSet\Control\VirtualDeviceDrivers";
        private const string MEDIATYPES     = @"Applications\wmplayer.exe\SupportedTypes";

        private const int MAXDWORD                  = 0xFFFF;
        private const int FILE_ATTRIBUTE_READONLY   = 0x1;
        private const int FILE_ATTRIBUTE_HIDDEN     = 0x2;
        private const int FILE_ATTRIBUTE_SYSTEM     = 0x4;
        private const int FILE_ATTRIBUTE_DIRECTORY  = 0x10;
        private const int FILE_ATTRIBUTE_ARCHIVE    = 0x20;
        private const int FILE_ATTRIBUTE_NORMAL     = 0x80;
        private const int FILE_ATTRIBUTE_TEMPORARY  = 0x100;
        #endregion

        #region API
        [DllImport("shell32.dll")] 
        [return : MarshalAs(UnmanagedType.Bool)]
        private static extern bool SHGetPathFromIDListW(IntPtr pidl, [MarshalAs(UnmanagedType.LPTStr)] StringBuilder pszPath);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetFileAttributes(string lpFileName);

        [DllImport("user32.dll")]
        [return : MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetInputState();

        [DllImport("comdlg32.dll")]
        private static extern int GetFileTitle(string lpFileName, StringBuilder lpszTitle, int cbBuf);

        [DllImport("shlwapi.dll")]
        private static extern int PathFileExists(string pszPath);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.U4)]
        private static extern int GetLongPathName([MarshalAs(UnmanagedType.LPTStr)]string lpszShortPath,
            [MarshalAs(UnmanagedType.LPTStr)]StringBuilder lpszLongPath, [MarshalAs(UnmanagedType.U4)]int cchBuffer);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetCurrentProcess();

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr GetModuleHandle(string moduleName);

        [DllImport("kernel32", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetProcAddress(IntPtr hModule,
            [MarshalAs(UnmanagedType.LPStr)]string procName);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsWow64Process(IntPtr hProcess, out bool wow64Process);

        #endregion

        #region Properties
        public bool ScanControl { get; set; }
        public bool ScanUser { get; set; }
        public bool ScanFile { get; set; }
        public bool ScanFont { get; set; }
        public bool ScanHelp { get; set; }
        public bool ScanSharedDll { get; set; }
        public bool ScanStartupEntries { get; set; }
        public bool ScanUninstallStrings { get; set; }
        public bool ScanVDM { get; set; }
        public bool ScanHistory { get; set; }
        public bool ScanDeep { get; set; }
        public bool ScanMru { get; set; }
        public List<ScanData> Data { get; set; }
        //List<ScanData> myList = new List<ScanData>();
        #endregion

        #region Delegates
        public delegate void LabelChangeDelegate(string phase, string label);
        public delegate void CurrentPathDelegate(string hive, string path);
        public delegate void KeyCountDelegate();
        public delegate void MatchItemDelegate(cLightning.ROOT_KEY root, string subkey, string value, string data, RESULT_TYPE id);
        public delegate void StatusChangeDelegate(string label);
        public delegate void ProcessChangeDelegate();
        public delegate void ScanCountDelegate(int count);
        public delegate void ScanCompleteDelegate();
        public delegate void SubScanCompleteDelegate(string id);
        #endregion

        #region Events
        [Description("Status update")]
        public event LabelChangeDelegate LabelChange;
        [Description("Current processing path")]
        public event CurrentPathDelegate CurrentPath;
        [Description("Key processed count")]
        public event KeyCountDelegate KeyCount;
        [Description("Match item was found")]
        public event MatchItemDelegate MatchItem;
        [Description("Processing status has changed")]
        public event StatusChangeDelegate StatusChange;
        [Description("Processing shifted to new task")]
        public event ProcessChangeDelegate ProcessChange;
        [Description("Task counter")]
        public event ScanCountDelegate ScanCount;
        [Description("Scan Completed")]
        public event ScanCompleteDelegate ScanComplete;
        [Description("Scan Completed")]
        public event SubScanCompleteDelegate SubScanComplete;
        #endregion

        #region Fields
        private static Regex _regPath = new Regex(
            @"([?a-z A-Z]:.*\\)([?\w.]+)", RegexOptions.IgnoreCase |
                RegexOptions.CultureInvariant |
                RegexOptions.IgnorePatternWhitespace |
                RegexOptions.Compiled
            );

        private static Regex _regName = new Regex(
            @"(\w+[.]\w+$+)", RegexOptions.IgnoreCase |
                RegexOptions.CultureInvariant |
                RegexOptions.IgnorePatternWhitespace |
                RegexOptions.Compiled
            );
        
        private cLightning _cLightning = new cLightning();
        private ArrayList _aExtensions = new ArrayList();
        private string[] _aDriveRoot;
        private string _sWindowsDirectory;
        private string _sFontsDirectory;
        private string _sSystem32Directory;
        private string _sProgramsDirectory;
        private string _sUserProgramsDirectory;
        #endregion

        #region Constructor
        public cRegScan()
        {
            _aExtensions = GetFileExtensions();
            StoreLogicalDrives();
            StoreCommonPaths();
            Data = new List<ScanData>();
        }
        #endregion

        #region Methods
        #region Asynchronous Processing
        private BackgroundWorker _oProcessAsyncBackgroundWorker = null;
        public delegate void ProcessCompletedEventHandler(object sender, RunWorkerCompletedEventArgs e);
        public event ProcessCompletedEventHandler ProcessCompleted;

        public void AsyncScan()
        {
            if (_oProcessAsyncBackgroundWorker != null)
            {
                ResetAsync();
            }

            _oProcessAsyncBackgroundWorker = new BackgroundWorker();
            _oProcessAsyncBackgroundWorker.WorkerSupportsCancellation = true;
            _oProcessAsyncBackgroundWorker.DoWork += ProcessAsyncBackgroundWorker_DoWork;
            _oProcessAsyncBackgroundWorker.RunWorkerCompleted += ProcessAsyncBackgroundWorker_RunWorkerCompleted;
            _oProcessAsyncBackgroundWorker.Disposed += new EventHandler(_oProcessAsyncBackgroundWorker_Disposed);
            _oProcessAsyncBackgroundWorker.RunWorkerAsync();
        }

        private void _oProcessAsyncBackgroundWorker_Disposed(object sender, EventArgs e)
        {
            _oProcessAsyncBackgroundWorker.DoWork -= ProcessAsyncBackgroundWorker_DoWork;
            _oProcessAsyncBackgroundWorker.RunWorkerCompleted -= ProcessAsyncBackgroundWorker_RunWorkerCompleted;
            _oProcessAsyncBackgroundWorker = null;
        }

        public void CancelProcessAsync()
        {
            if (_oProcessAsyncBackgroundWorker != null)
            {
                _oProcessAsyncBackgroundWorker.CancelAsync();
                ResetAsync();
            }
        }

        private void ResetAsync()
        {
            if (_oProcessAsyncBackgroundWorker != null)
            {
                _oProcessAsyncBackgroundWorker.Dispose();
            }
        }

        private void ProcessAsyncBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                if (!StartScan())
                {
                    CancelProcessAsync();
                }
            }
            catch (StopProcessingException)
            {
                // end
            }
        }

        private void ProcessAsyncBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (ProcessCompleted != null)
            {
                ProcessCompleted(this, e);
            }
        }

        private class StopProcessingException : Exception
        {
        }
        #endregion

        #region Control
        public bool StartScan()
        {
            // saves a lot of needless null checks..
            if (LabelChange == null || CurrentPath == null || KeyCount == null || MatchItem == null ||
                StatusChange == null || ProcessChange == null || ScanCount == null || ScanComplete == null || SubScanComplete == null)
            {
                return false;
            }

            // signal number of pending tasks
            ScanCounter();

            // control registration scan
            if (ScanControl)
            {
                StatusChange("Scanning: System Control Classes");
                ControlScan();
                ProcessChange();
                SubScanComplete("CONTROL");
            }
            // user software scan
            if (ScanUser)
            {
                StatusChange("Scanning: User Software");
                UserScan();
                ProcessChange();
                SubScanComplete("USER");
            }
            // system software scan
            if (ScanFile)
            {
                StatusChange("Scanning: Software Registrations");
                FileScan();
                ProcessChange();
                SubScanComplete("SOFTWARE");
            }
            // font scan
            if (ScanFont)
            {
                StatusChange("Scanning: System Fonts");
                FontScan(cLightning.ROOT_KEY.HKEY_LOCAL_MACHINE, @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Fonts");
                ProcessChange();
                SubScanComplete("FONT");
            }
            // help strings
            if (ScanHelp)
            {
                StatusChange("Scanning: Registered Help Files");
                HelpScan(cLightning.ROOT_KEY.HKEY_LOCAL_MACHINE, @"SOFTWARE\Microsoft\Windows\Help");
                ProcessChange();
                HelpScan(cLightning.ROOT_KEY.HKEY_LOCAL_MACHINE, @"SOFTWARE\Microsoft\Windows\HTML Help");
                ProcessChange();
                SubScanComplete("HELP");
            }
            // shared dlls
            if (ScanSharedDll)
            {
                StatusChange("Scanning: Shared Libraries");
                SharedDllScan(cLightning.ROOT_KEY.HKEY_LOCAL_MACHINE, @"SOFTWARE\Microsoft\Windows\CurrentVersion\SharedDLLs");
                ProcessChange();
                SubScanComplete("SHAREDDLL");
            }
            // startup entries
            if (ScanStartupEntries)
            {
                StatusChange("Scanning: Startup Options");
                StartupEntries(cLightning.ROOT_KEY.HKEY_LOCAL_MACHINE, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run");
                ProcessChange();
                StartupEntries(cLightning.ROOT_KEY.HKEY_LOCAL_MACHINE, @"SOFTWARE\Microsoft\Windows\CurrentVersion\RunOnce");
                ProcessChange();
                StartupEntries(cLightning.ROOT_KEY.HKEY_LOCAL_MACHINE, @"SOFTWARE\Microsoft\Windows\CurrentVersion\RunOnceEx");
                ProcessChange();
                SubScanComplete("STARTUP");
            }
            // installed software
            if (ScanUninstallStrings)
            {
                StatusChange("Scanning: Installed Software");
                UninstallStringsScan(cLightning.ROOT_KEY.HKEY_LOCAL_MACHINE, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall");
                ProcessChange();
                SubScanComplete("UNINSTALL");
            }
            // virtual devices
            if (ScanVDM)
            {
                StatusChange("Scanning: Virtual Devices");
                VDMScan(cLightning.ROOT_KEY.HKEY_LOCAL_MACHINE, @"System\ControlSet\Control\VirtualDeviceDrivers");
                ProcessChange();
                SubScanComplete("VDM");
            }
            // shortcuts
            if (ScanHistory)
            {
                StatusChange("Scanning: Application History");
                HistoryScan();
                ProcessChange();
                SubScanComplete("HISTORY");
            }
            // deep scan
            if (ScanDeep)
            {
                StatusChange("Scanning: Deep System Scan");
                DeepScan();
                ProcessChange();
                SubScanComplete("DEEP");
            }
            // mru scan
            if (ScanMru)
            {
                StatusChange("Scanning: Most Recently Used Lists");
                MruScan(cLightning.ROOT_KEY.HKEY_CURRENT_USER, MRU_DOCUMENTS);
                MruScan(cLightning.ROOT_KEY.HKEY_CURRENT_USER, MRU_TYPEDURLS);
                MruScan(cLightning.ROOT_KEY.HKEY_CURRENT_USER, MRU_RUNMRU);
                MruScan(cLightning.ROOT_KEY.HKEY_CURRENT_USER, MRU_FILESEARCH);
                MruScan(cLightning.ROOT_KEY.HKEY_CURRENT_USER, MRU_INTERNET);
                MruScan(cLightning.ROOT_KEY.HKEY_CURRENT_USER, MRU_PPL);
                MruScan(cLightning.ROOT_KEY.HKEY_CURRENT_USER, MRU_WORDPAD);
                MruScan(cLightning.ROOT_KEY.HKEY_CURRENT_USER, MRU_REGFAV);
                MruScan(cLightning.ROOT_KEY.HKEY_CURRENT_USER, MRU_REGEDIT);
                MruScan(cLightning.ROOT_KEY.HKEY_CURRENT_USER, MRU_PAINT);
                MruScan(cLightning.ROOT_KEY.HKEY_CURRENT_USER, MRU_COMDLG);
                MruScan(cLightning.ROOT_KEY.HKEY_CURRENT_USER, MRU_EXPLORER);
                MruScan(cLightning.ROOT_KEY.HKEY_CURRENT_USER, MRU_MEDREC);
                MruScan(cLightning.ROOT_KEY.HKEY_CURRENT_USER, MRU_MEDURL);
                ProcessChange();
                SubScanComplete("MRU");
            }
            ScanComplete();
            return true;
        }

        private void ScanCounter()
        {
            int ct = 0;
            if (ScanControl)
            {
                ct += 1;
            }
            if (ScanUser)
            {
                ct += 1;
            }
            if (ScanFile)
            {
                ct += 1;
            }
            if (ScanFont)
            {
                ct += 1;
            }
            if (ScanHelp)
            {
                ct += 2;
            }
            if (ScanSharedDll)
            {
                ct += 1;
            }
            if (ScanStartupEntries)
            {
                ct += 3;
            }
            if (ScanUninstallStrings)
            {
                ct += 1;
            }
            if (ScanVDM)
            {
                ct += 1;
            }
            if (ScanHistory)
            {
                ct += 1;
            }
            if (ScanDeep)
            {
                ct += 1;
            }
            if (ScanMru)
            {
                ct += 1;
            }
            if (ScanCount != null)
            {
                ScanCount(ct);
            }
        }
        //store: root, subkey, value, path, id
        //scandata: key root, string key, string value, string path, string img, string name, int scope, int id
        private void StoreResults(cLightning.ROOT_KEY root, string subkey, string value, string data, RESULT_TYPE id)
        {
            int i = (int)id;
            if (value.Length == 0)
            {
                value = STR_DEFAULT;
            }
            Data.Add(new ScanData(root, subkey, value, data, "", IdConverter(i), IdToScope(i), i));
            // notify
            MatchItem(root, subkey, value, data, id);
        }
        #endregion

        #region Phase 1 : Custom Controls
        ///Purpose:                             Test for valid software class registration and command paths (CLSID)
        ///Scan 1                               - CLSID
        ///1) valid registration                - HKEY_CLASSES_ROOT\CLSID\..\InProcSvr32(+)
        ///2) typelib paths                     - HKEY_CLASSES_ROOT\..
        ///3) appid paths                       - HKEY_CLASSES_ROOT\CLSID\... Val-AppID <-> HKEY_CLASSES_ROOT\AppID
        ///Scan 2                               - Interface
        ///4) type lib paths                    - HKEY_CLASSES_ROOT\Interface\TypeLib <-> CLSID\TypeLib
        ///5) interface paths                   - HKEY_CLASSES_ROOT\Interface\...\ProxyStubClsid32 <-> CLSID
        ///Scan 3                               - TypeLib
        ///6) empty keys                        - HKEY_CLASSES_ROOT\TypeLib\..\HELPDIR
        ///7) typelib paths                     - HKEY_CLASSES_ROOT\TypeLib\..\..\win32
        ///Scan 4                               - File Extensions
        ///8) empty ext keys                    - HKEY_CLASSES_ROOT\...
        public void ControlScan()
        {
            // scan hkcr keys
            LabelChange("Application ID Paths", "Comparing internal ID strings..");

            ArrayList al = _cLightning.EnumKeys(cLightning.ROOT_KEY.HKEY_CLASSES_ROOT, STR_CLASS);

            foreach (string s in al)
            {
                AppIDPaths(cLightning.ROOT_KEY.HKEY_CLASSES_ROOT, STR_CLASSB + s);
                ProcServerPaths(cLightning.ROOT_KEY.HKEY_CLASSES_ROOT, STR_CLASSB + s);
                TypeLibPaths(cLightning.ROOT_KEY.HKEY_CLASSES_ROOT, STR_CLASSB + s);
                CurrentPath(REG_HKCRB, s);
                KeyCount();
            }

            // test interface keys
            LabelChange("Interface Types", "Testing Class Interfaces..");

            al = _cLightning.EnumKeys(cLightning.ROOT_KEY.HKEY_CLASSES_ROOT, STR_INTERFACE);

            foreach (string s in al)
            {
                InterfacePaths(cLightning.ROOT_KEY.HKEY_CLASSES_ROOT, STR_INTERFACEB + s);
                CurrentPath(REG_HKCRB, STR_INTERFACEB + s);
                KeyCount();
            }

            // test typelib keys
            LabelChange("Type Libraries", "Testing Type Library references..");

            al = _cLightning.EnumKeys(cLightning.ROOT_KEY.HKEY_CLASSES_ROOT, STR_TYPE);

            foreach (string s in al)
            {
                TypePaths(cLightning.ROOT_KEY.HKEY_CLASSES_ROOT, STR_TYPEB + s);
                CurrentPath(REG_HKCRB, STR_TYPEB + s);
                KeyCount();
            }

            // file extensions scan
            LabelChange("Class Key Sub Paths", "Testing Class key path references..");

            al = _cLightning.EnumKeys(cLightning.ROOT_KEY.HKEY_CLASSES_ROOT, "");

            foreach (string s in al)
            {
                ClassSubPaths(cLightning.ROOT_KEY.HKEY_CLASSES_ROOT, s);
                CurrentPath(REG_HKCRB, s);
                KeyCount();
            }
        }

        private void AppIDPaths(cLightning.ROOT_KEY Key, string SubKey)
        {
            // test for valid app registration ids
            string id;
            // CLSID pointer matches registered Application ->HKCR\CLSID\{value} <-> HKCR\AppId\{value}
            if (_cLightning.ValueExists(Key, SubKey, STR_APPID))
            {
                id = _cLightning.ReadString(Key, SubKey, STR_APPID);
                if (!_cLightning.KeyExists(Key, STR_APPID + CHR_BSLASH + id))
                {
                    StoreResults(cLightning.ROOT_KEY.HKEY_CLASSES_ROOT, SubKey, STR_APPID, id, RESULT_TYPE.ControlAppID);
                }
            }
        }

        private void ClassSubPaths(cLightning.ROOT_KEY Key, string SubKey)
        {
            // test class key subpaths
            string sp = "";
            
            if (SubKey.Contains(STR_CLASS) || SubKey.Contains(STR_TYPE) || SubKey.Contains(STR_INTERFACE))
            {
                return;
            }
            // default application ->HKCR\extension\default->path
            if (SubKey.StartsWith(CHR_PERIOD))
            {
                if (_cLightning.KeyIsEmpty(Key, SubKey))
                {
                    StoreResults(cLightning.ROOT_KEY.HKEY_CLASSES_ROOT, SubKey, STR_DEFAULT, STR_EMPTY, RESULT_TYPE.ControlClassSubExt);
                }
            }
            else
            {
                // default shell ->HKCR\name\shell\open\command\default->path
                if (_cLightning.KeyExists(Key, SubKey + STR_SHELLOPEN))
                {
                    sp = _cLightning.ReadString(Key, SubKey + STR_SHELLOPEN, "");
                    if (sp.Length > 4)
                    {
                        if (IsValidPath(sp))
                        {
                            sp = CleanPath(sp);
                            if (!FileExists(sp))
                            {
                                StoreResults(cLightning.ROOT_KEY.HKEY_CLASSES_ROOT, SubKey + STR_SHELLOPEN, STR_DEFAULT, sp, RESULT_TYPE.ControlClassSubOpen);
                            }
                        }
                    }
                }
                // default editing tool ->HKCR\name\shell\edit\command\default->path
                if (_cLightning.KeyExists(Key, SubKey + STR_SHELLEDIT))
                {
                    sp = _cLightning.ReadString(Key, SubKey + STR_SHELLEDIT, "");
                    if (sp.Length > 4)
                    {
                        if (IsValidPath(sp))
                        {
                            sp = CleanPath(sp);
                            if (!FileExists(sp) && IsFileCandidate(sp))
                            {
                                StoreResults(cLightning.ROOT_KEY.HKEY_CLASSES_ROOT, SubKey + STR_SHELLEDIT, STR_DEFAULT, sp, RESULT_TYPE.ControlClassSubEdit);
                            }
                        }
                    }
                }
            }
        }

        private void InterfacePaths(cLightning.ROOT_KEY Key, string SubKey)
        {
            // test paths from \proxystub -> CLSID
            // test paths from \typelib -> TypeLib
            // remove value
            string sp = "";
            ArrayList al = KeyCollector(Key, SubKey);

            // test pointers to valid type libraries HKCR\Interface\*name*\TypeLib <-> HKCR\TypeLib\{value}
            foreach (string s in al)
            {
                if (s.Contains(STR_TYPE))
                {
                    sp = _cLightning.ReadString(Key, s, "");
                    if (!_cLightning.KeyExists(Key, STR_TYPEB + sp))
                    {
                        StoreResults(cLightning.ROOT_KEY.HKEY_CLASSES_ROOT, s, STR_DEFAULT, sp, RESULT_TYPE.ControlInterfaceType);
                    }
                    break;
                }
            }
            // test pointers to valid stub HKCR\Interface\*name*\ProxyStubClsid32 <-> HKCR\CLSID\{value}
            foreach (string s in al)
            {
                if (s.Contains(STR_PROXY) && (!Is64BitOperatingSystem())) //invalid in 64bit OS
                {
                    sp = _cLightning.ReadString(Key, s, "");
                    if (!_cLightning.KeyExists(Key, STR_CLASSB + sp))
                    {
                        StoreResults(cLightning.ROOT_KEY.HKEY_CLASSES_ROOT, s, STR_DEFAULT, sp, RESULT_TYPE.ControlInterfaceProxy);
                    }
                    break;
                }
            }
        }

        private void ProcServerPaths(cLightning.ROOT_KEY Key, string SubKey)
        {
            // process server subkeys
            string sp;
            // test pointers to valid paths HKCR\CLSID\*Proc* <-> library path
            // test for proc subkey existence
            if (_cLightning.KeyExists(Key, SubKey + STR_PROC32B))
            {
                ///* get the path
                sp = _cLightning.ReadString(Key, SubKey + STR_PROC32B, "");
                ///* test path length and type
                if (sp.Length > 0)
                {
                    if (IsValidPath(sp))
                    {
                        // format path and test
                        if (!FileExists(CleanPath(sp)) && IsFileCandidate(sp))
                        {
                            // add hklm path
                            StoreResults(cLightning.ROOT_KEY.HKEY_CLASSES_ROOT, SubKey + CHR_BSLASH + STR_PROC32, STR_DEFAULT, sp, RESULT_TYPE.ControlProcServer);
                        }
                    }
                }
            }
            if (_cLightning.KeyExists(Key, SubKey + STR_LOCAL32B))
            {
                sp = _cLightning.ReadString(Key, SubKey + STR_LOCAL32B, "");
                if (sp.Length > 0)
                {
                    if (IsValidPath(sp))
                    {
                        if (!FileExists(CleanPath(sp)) && IsFileCandidate(sp))
                        {
                            StoreResults(cLightning.ROOT_KEY.HKEY_CLASSES_ROOT, SubKey + CHR_BSLASH + STR_LOCAL32, STR_DEFAULT, sp, RESULT_TYPE.ControlProcServer);
                        }
                    }
                }
            }
            if (_cLightning.KeyExists(Key, SubKey + STR_PROCB))
            {
                sp = _cLightning.ReadString(Key, SubKey + STR_PROCB, "");
                if (sp.Length > 0)
                {
                    if (IsValidPath(sp))
                    {
                        if (!FileExists(CleanPath(sp)) && IsFileCandidate(sp))
                        {
                            StoreResults(cLightning.ROOT_KEY.HKEY_CLASSES_ROOT, SubKey + CHR_BSLASH + STR_PROC, STR_DEFAULT, sp, RESULT_TYPE.ControlProcServer);
                        }
                    }
                }
            }
            if (_cLightning.KeyExists(Key, SubKey + STR_LOCALB))
            {
                sp = _cLightning.ReadString(Key, SubKey + STR_LOCALB, "");
                if (sp.Length > 0)
                {
                    if (IsValidPath(sp))
                    {
                        if (!FileExists(CleanPath(sp)) && IsFileCandidate(sp))
                        {
                            StoreResults(cLightning.ROOT_KEY.HKEY_CLASSES_ROOT, SubKey + CHR_BSLASH + STR_LOCAL, STR_DEFAULT, STR_EMPTYVALUE, RESULT_TYPE.ControlProcServer);
                        }
                    }
                }
            }
        }

        private void TypeLibPaths(cLightning.ROOT_KEY Key, string SubKey)
        {
            // test typelib registration id
            string sr;

            // test pointers to valid type library registration HKCR\CLSID\*name*\TypeLib {value} <-> HKCR\TypeLib\{value}
            // test for typelib subkey
            if (_cLightning.KeyExists(Key, SubKey + CHR_BSLASH + STR_TYPE))
            {
                // get the clsid
                sr = _cLightning.ReadString(Key, SubKey + CHR_BSLASH + STR_TYPE, "");
                // test id length
                if (sr.Length > 0)
                {
                    // tlb is not registered
                    if (!_cLightning.KeyExists(Key, STR_TYPEB + sr))
                    {
                        StoreResults(cLightning.ROOT_KEY.HKEY_CLASSES_ROOT, SubKey + CHR_BSLASH + STR_TYPE, STR_DEFAULT, sr, RESULT_TYPE.ControlTypeLib);
                    }
                }
            }
        }

        private void TypePaths(cLightning.ROOT_KEY Key, string SubKey)
        {
            // test for empty help keys
            // 6- delete key
            // 7- delete values
            string u = "";
            string sp = "";
            ArrayList al = KeyCollector(Key, SubKey);

            foreach (string s in al)
            {
                u = s.ToUpper();
                // test pointers to valid help file registration HKCR\\TypeLib\*name*\helpdir->path
                if (u.Contains(STR_HELP))
                {
                    if (_cLightning.KeyIsEmpty(Key, SubKey))
                    {
                        StoreResults(cLightning.ROOT_KEY.HKEY_CLASSES_ROOT, s, STR_DEFAULT, STR_EMPTYVALUE, RESULT_TYPE.ControlTypeHelp);
                    }
                }
                // test pointers to valid win32 library registration HKCR\\TypeLib\*name*\win32->path
                else if (u.Contains(STR_WIN32))
                {
                    sp = _cLightning.ReadString(Key, s, "");
                    if (sp.Length > 0)
                    {
                        if (IsFileCandidate(sp))
                        {
                            sp = CleanPath(sp);
                            if (!FileExists(sp))
                            {
                                StoreResults(cLightning.ROOT_KEY.HKEY_CLASSES_ROOT, s, STR_DEFAULT, sp, RESULT_TYPE.ControlTypeWin32);
                            }
                        }
                    }
                }
            }
        }
        #endregion

        #region Phase 2 : User Software
        ///Purpose:                         Test for valid software paths
        ///Scan 1                           -Software
        ///Location                         HKEY_CURRENT_USER\Software
        ///Collect all software keys from \Software branch, and scan for valid path entries
        public void UserScan()
        {
            LabelChange("User Software Paths", "Testing for invalid user software paths..");

            string sp = "";
            ArrayList al = KeyCollector(cLightning.ROOT_KEY.HKEY_CURRENT_USER, "Software");
            ArrayList cv = new ArrayList();

            foreach (string k in al)
            {
                cv = _cLightning.EnumValues(cLightning.ROOT_KEY.HKEY_CURRENT_USER, k);
                foreach (string v in cv)
                {
                    if (v.Length > 4)
                    {
                        if (IsValidRoot(v))
                        {
                            sp = CleanPath(v);
                            if (IsFileCandidate(sp))
                            {
                                if (!FileExists(sp))
                                {
                                    string d =_cLightning.ReadString(cLightning.ROOT_KEY.HKEY_CURRENT_USER, k, v);
                                    StoreResults(cLightning.ROOT_KEY.HKEY_CURRENT_USER, k, v, d, RESULT_TYPE.User);
                                }
                            }
                            else if (!DirectoryExists(sp))
                            {
                                StoreResults(cLightning.ROOT_KEY.HKEY_CURRENT_USER, k, v, STR_EMPTY, RESULT_TYPE.User);
                            }
                        }
                    }
                    CurrentPath(REG_HKCUB, k);
                    KeyCount();
                }
            }
        }
        #endregion

        #region Phase 3 : System Software
        ///Purpose: Scan 1-5 -System Software  Test for valid id registrations
        ///1) test class names        - HKEY_CLASSES_ROOT\.xxx DefVal <-> HKEY_CLASSES_ROOT\..
        ///2) test clsid              - HKEY_CLASSES_ROOT\..\CLSID DefVal <-> HKEY_CLASSES_ROOT\CLSID
        ///3) object menu path        - HKEY_CLASSES_ROOT\..\shell\edit\command
        ///4) object open path        - HKEY_CLASSES_ROOT\..\shell\open\command
        ///5) object icon path        - HKEY_CLASSES_ROOT\..\DefaultIcon
        private void FileScan()
        {
            LabelChange("Class Name Registrations", "Checking for invalid class id's..");

            ArrayList al = _cLightning.EnumKeys(cLightning.ROOT_KEY.HKEY_CLASSES_ROOT, "");
            string sr = "";

            foreach (string s in al)
            {
                if (s.StartsWith(CHR_PERIOD))
                {
                    sr = _cLightning.ReadString(cLightning.ROOT_KEY.HKEY_CLASSES_ROOT, s, "");
                    if (sr.Length > 0)
                    {
                        if (!_cLightning.KeyExists(cLightning.ROOT_KEY.HKEY_CLASSES_ROOT, sr))
                        {
                            StoreResults(cLightning.ROOT_KEY.HKEY_CLASSES_ROOT, s, STR_DEFAULT, sr, RESULT_TYPE.FullClassName);
                        }
                    }
                }
                else
                {
                    // clsid test
                    if (_cLightning.KeyExists(cLightning.ROOT_KEY.HKEY_CLASSES_ROOT, s + CHR_BSLASH + STR_CLASS))
                    {
                        sr = _cLightning.ReadString(cLightning.ROOT_KEY.HKEY_CLASSES_ROOT, s + CHR_BSLASH + STR_CLASS, "");
                        if (sr.Length > 0)
                        {
                            if (sr.StartsWith("{"))
                            {
                                if (!_cLightning.KeyExists(cLightning.ROOT_KEY.HKEY_CLASSES_ROOT, STR_CLASSB + sr))
                                {
                                    StoreResults(cLightning.ROOT_KEY.HKEY_CLASSES_ROOT, s + CHR_BSLASH + STR_CLASS, STR_DEFAULT, sr, RESULT_TYPE.FullClsid);
                                }
                            }
                        }
                    }
                }

                // default icon
                if (_cLightning.KeyExists(cLightning.ROOT_KEY.HKEY_CLASSES_ROOT, s + CHR_BSLASH + "DefaultIcon"))
                {
                    sr = _cLightning.ReadString(cLightning.ROOT_KEY.HKEY_CLASSES_ROOT, s + STR_DEFICON, "");
                    if (sr.Length > 0 && IsFileCandidate(sr))
                    {
                        sr = CleanPath(sr);
                        if (IsValidRoot(sr) && !FileExists(CleanPath(sr)) && IsFileCandidate(sr))
                        {
                            StoreResults(cLightning.ROOT_KEY.HKEY_CLASSES_ROOT, s + STR_DEFICON, STR_DEFAULT, sr, RESULT_TYPE.FullIcon);
                        }
                    }
                }
                CurrentPath(REG_HKCUB, s);
                KeyCount();
            }
        }
        #endregion

        #region Phase 4 : Fonts
        ///Locations:                 1) HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Fonts
        ///References:                From HKLM -> fonts folder
        ///Method:                    Path testing for valid occurence.

        private void FontScan(cLightning.ROOT_KEY Key, string SubKey)
        {
            LabelChange("Font Paths", "Path testing for invalid fonts..");
            // 15- delete value
            ArrayList al = _cLightning.EnumValues(Key, SubKey);
            string sr = "";

            CurrentPath(REG_HKLMB, REG_HKLMFONTS);
            KeyCount();

            foreach (string s in al)
            {
                if (s.Length > 0)
                {
                    string v = _cLightning.ReadString(Key, SubKey, s);
                    if (IsValidPath(v))
                    {
                        sr = CleanPath(v);
                        if (IsValidRoot(sr) && !FileExists(sr) && IsFileCandidate(sr))
                        {
                            StoreResults(cLightning.ROOT_KEY.HKEY_LOCAL_MACHINE, REG_HKLMFONTS, s, sr, RESULT_TYPE.Font);
                        }
                    }
                    else
                    {
                        sr = _sFontsDirectory + CleanPath(v);
                        if (!FileExists(sr) && HasExtension(sr))
                        {
                            StoreResults(cLightning.ROOT_KEY.HKEY_LOCAL_MACHINE, REG_HKLMFONTS, s, v, RESULT_TYPE.Font);
                        }
                    }
                }

            }
        }
        #endregion

        #region Phase 5 : Help Files
        ///Locations:                 1) HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\Help
        ///References:                From HKLM -> Help registration
        ///Method:                    Path testing for valid occurence.
        private void HelpScan(cLightning.ROOT_KEY Key, string SubKey)
        {
            LabelChange("Application Help Files", "Path testing for invalid system help files..");
            ArrayList al = _cLightning.EnumValues(Key, SubKey);
            string sr = "";

            CurrentPath(REG_HKLMB, SubKey);
            KeyCount();

            foreach (string s in al)
            {
                if (s.Length > 0)
                {
                    sr = _cLightning.ReadString(Key, SubKey, s);
                    if (sr.Length > 0)
                    {
                        // combine file name and path
                        if (!sr.EndsWith(CHR_BSLASH))
                        {
                            sr += CHR_BSLASH;
                        }
                        sr += s;
                        if (IsValidPath(sr))
                        {
                            if (IsValidRoot(sr) && !FileExists(sr) && IsFileCandidate(sr))
                            {
                                StoreResults(cLightning.ROOT_KEY.HKEY_LOCAL_MACHINE, SubKey, s, sr, RESULT_TYPE.Help);
                            }
                        }
                    }
                }
            }
        }
        #endregion

        #region Phase 6 : Shared DLLs
        ///Locations:                 1) HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\SharedDLLs
        ///References:                From HKLM -> path test
        ///Method:                    Path testing for valid occurence.
        private void SharedDllScan(cLightning.ROOT_KEY Key, string SubKey)
        {
            LabelChange("Shared Libraries", "Path testing for valid shared library connections..");
            // 17- delete value
            ArrayList al = _cLightning.EnumValues(Key, SubKey);
            //string sr = "";

            CurrentPath(REG_HKLMB, SubKey);
            KeyCount();

            foreach (string s in al)
            {
                if (IsValidPath(s))
                {
                    //sr = CleanPath(s);
                    if (IsValidRoot(s) && !FileExists(s) && IsFileCandidate(s))
                    {
                        StoreResults(cLightning.ROOT_KEY.HKEY_LOCAL_MACHINE, REG_HKLMSHARE, s, "0", RESULT_TYPE.Shared);
                    }
                }
            }
        }
        #endregion

        #region Phase 7 : Startup Entries
        ///Location:                  1) HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Run
        ///Location:                  2) HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\RunOnce
        ///Location:                  3) HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\RunOnceEx
        ///References:                From HKLM -> path test
        ///Method:                    Path testing for valid occurence.
        private void StartupEntries(cLightning.ROOT_KEY Key, string SubKey)
        {
            LabelChange("Startup Application Paths", "Testing startup software entries..");
            // 18- delete value
            ArrayList al = _cLightning.EnumValues(Key, SubKey);
            string sr = "";
            CurrentPath(REG_HKLMB, SubKey);
            KeyCount();

            foreach (string s in al)
            {
                sr = _cLightning.ReadString(Key, SubKey, s);
                // empty value
                if (sr.Length == 0)
                {
                    StoreResults(cLightning.ROOT_KEY.HKEY_LOCAL_MACHINE, SubKey, s, STR_EMPTYVALUE, RESULT_TYPE.Startup);
                }
                else
                {
                    // test for shell directory shorthand
                    sr = TestSystemPaths(sr);
                    sr = CleanPath(sr);
                    if (IsValidRoot(sr) && !FileExists(CleanPath(sr)) && HasExtension(sr))
                    {
                        StoreResults(cLightning.ROOT_KEY.HKEY_LOCAL_MACHINE, SubKey, s, sr, RESULT_TYPE.Startup);
                    }
                }
            }
        }
        #endregion

        #region Phase 8 : Installed Software
        ///Locations:                 1) HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall
        ///References:                From HKLM -> path test
        ///Method:                    Path testing for valid occurence.
        private void UninstallStringsScan(cLightning.ROOT_KEY Key, string SubKey)
        {
            LabelChange("Uninstall Executable Paths", "Path testing for user installed software..");
            // 18- delete value
            ArrayList al = _cLightning.EnumKeys(Key, SubKey);
            string sr = "";

            foreach (string s in al)
            {
                sr = s.ToUpper();
                // ms stuff to skip
                if (!sr.Contains(STR_KILO) && !sr.Contains(STR_PACK))
                {
                    sr = _cLightning.ReadString(Key, SubKey + CHR_BSLASH + s, STR_UIST);
                    if (sr.Length != 0)
                    {
                        sr = CleanPath(sr);
                        if (IsValidRoot(sr) && !FileExists(sr) && HasExtension(sr))
                        {
                            StoreResults(cLightning.ROOT_KEY.HKEY_LOCAL_MACHINE, REG_HKLMUISL + s, STR_UIST, sr, RESULT_TYPE.Uninstall);
                        }
                    }
                }
                CurrentPath(REG_HKLMB, s);
                KeyCount();
            }
        }
        #endregion

        #region Phase 9 : Virtual Devices
        ///Locations:                 1) HKEY_LOCAL_MACHINE\SYSTEM\ControlSet\Control\VirtualDeviceDrivers
        ///References:                From HKLM -> fix for 16bit VDM value type mismatch
        ///Method:                    Value type testing for valid entry
        private void VDMScan(cLightning.ROOT_KEY Key, string SubKey)
        {
            LabelChange("Virtual Device Registration", "Testing for VDM bug..");
            CurrentPath(REG_HKLMB, SubKey);
            KeyCount();

            if (_cLightning.ReadBinary(Key, SubKey, STR_VDD).Length > 0)
            {
                StoreResults(cLightning.ROOT_KEY.HKEY_LOCAL_MACHINE, REG_HKLMVDEV, SubKey, STR_VDD, RESULT_TYPE.Vdf);
            }
        }
        #endregion

        #region Phase 10: History and Start Menu
        ///References:                From HKLM -> scan for valid link paths
        ///Method:                    Value type testing for valid entry
        ///Location:                  HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\MenuOrder\Start Menu\Programs
        ///Location:                  HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\FileExts\...\OpenWithList | OpenWithProgids
        ///Location:                  HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\ComDlg32\OpenSaveMRU
        ///Location:                  HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\UserAssist\...\Count
        private void HistoryScan()
        {
            LabelChange("History Settings", "Testing for invalid history links..");
            // test FileExts lists - EXPLORER HISTORY
            // 18- delete value
            ArrayList al = _cLightning.EnumKeys(cLightning.ROOT_KEY.HKEY_CURRENT_USER, HIST_EXPLORER);
            ArrayList av;
            cLightning.ROOT_KEY Key = cLightning.ROOT_KEY.HKEY_CURRENT_USER;
            string sr = "";

            /*foreach (string s in al) note* won't let you delete default value [if empty] anymore..
            {
                if (_cLightning.KeyExists(Key, HIST_EXPLORERB + s + STR_OWLISTB))
                {
                    if (_cLightning.KeyIsEmpty(Key, HIST_EXPLORERB + s + STR_OWLISTB))
                    {
                        StoreResults(cLightning.ROOT_KEY.HKEY_CURRENT_USER, HIST_EXPLORERB + s + STR_OWLISTB, STR_OWLIST, STR_EMPTY, RESULT_TYPE.HistoryExplorer);
                    }
                }
                if (_cLightning.KeyExists(Key, HIST_EXPLORERB + s + STR_OWPROGB))
                {
                    if (_cLightning.KeyIsEmpty(Key, HIST_EXPLORERB + s + STR_OWPROGB))
                    {
                        StoreResults(cLightning.ROOT_KEY.HKEY_CURRENT_USER, HIST_EXPLORERB + s + STR_OWPROGB, STR_OWPROG, STR_EMPTY, RESULT_TYPE.HistoryExplorer);
                    }
                }
                CurrentPath(REG_HKCUB, s);
                KeyCount();
            }*/
            // test OpenSaveMRU paths FILE HISTORY -win7 now OpenSavePidlMRU?
            al = _cLightning.EnumKeys(cLightning.ROOT_KEY.HKEY_CURRENT_USER, HIST_FILE);
            foreach (string s in al)
            {
                av = _cLightning.EnumValues(cLightning.ROOT_KEY.HKEY_CURRENT_USER, HIST_FILEB + s);
                foreach (string v in av)
                {
                    if (v.Length > 0 && IsValidPath(v))
                    {
                        sr = CleanPath(v);
                        if (IsValidRoot(sr) && !FileExists(CleanPath(sr)) && IsFileCandidate(sr))
                        {
                            StoreResults(cLightning.ROOT_KEY.HKEY_CURRENT_USER, HIST_FILEB + s, v, sr, RESULT_TYPE.HistoryStart);
                        }
                    }
                }
                CurrentPath(REG_HKCUB, s);
                KeyCount();
            }
            // test user assist - START MENU HISTORY
            al = KeyCollector(Key, HIST_START);
            foreach (string s in al)
            {
                if (s.Contains("Count"))
                {
                    av = _cLightning.EnumValues(Key, s);
                    foreach (string v in av)
                    {
                        // decrypt
                        sr = _cLightning.Rot13(v);
                        sr = sr.Substring(sr.IndexOf(CHR_COLAN) + 1);
                        sr = CleanPath(sr);
                        if (IsValidRoot(sr) && !FileExists(CleanPath(sr)) && HasExtension(sr))
                        {
                            StoreResults(cLightning.ROOT_KEY.HKEY_CURRENT_USER, s, v, sr, RESULT_TYPE.HistoryMenu);
                        }
                    }
                }
                CurrentPath(REG_HKCUB, s);
                KeyCount();
            }
            // test start menu links - START MENU LINKS
            al = _cLightning.EnumKeys(cLightning.ROOT_KEY.HKEY_CURRENT_USER, LINK_START);
            string sm = Environment.GetFolderPath(Environment.SpecialFolder.StartMenu) + @"\Programs\";
            string su = Environment.GetFolderPath(Environment.SpecialFolder.Startup) + @"\Programs\";
            foreach (string s in al)
            {
                if (!FileExists(sm + s + CHR_BSLASH))
                {
                    if (!FileExists(su + s + CHR_BSLASH))
                    {
                        if (_cLightning.EnumKeys(Key, LINK_STARTB + s).Count == 0)
                        {
                            StoreResults(cLightning.ROOT_KEY.HKEY_CURRENT_USER, LINK_STARTB + s, s, sm + s, RESULT_TYPE.HistoryLink);
                        }
                    }
                }
                CurrentPath(REG_HKCUB, s);
                KeyCount();
            }
        }
        #endregion

        #region Phase 11 : Deep Scan
        ///References:                From HKLM -> scan for valid link paths
        ///Method:                    Value type testing for valid entry
        ///Locations:                 HKEY_LOCAL_MACHINE\SOFTWARE
        ///Locations:                 HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Installer\UserData\...\Products
        private void DeepScan()
        {
            LabelChange("Deep Native Software Strings", "Deep software scan..");
            // 25- delete value
            // 26- delete value
            cLightning.ROOT_KEY Key = cLightning.ROOT_KEY.HKEY_LOCAL_MACHINE;
            ArrayList al = KeyCollector(Key, "SOFTWARE");
            ArrayList av;
            string sr = "";

            foreach (string s in al)
            {
                // microsoft keys
                if (s.Contains(STR_MSPATH))
                {
                    if (s.Contains(STR_INSPRP))
                    {
                        av = _cLightning.EnumValues(Key, s);
                        foreach (string v in av)
                        {
                            if (v.Length > 4 && IsFileCandidate(v))
                            {
                                sr = CleanPath(v);
                                if (IsValidRoot(sr) && !FileExists(sr) && HasExtension(sr))
                                {
                                    StoreResults(cLightning.ROOT_KEY.HKEY_LOCAL_MACHINE, s, v, sr, RESULT_TYPE.DeepMs);
                                }
                            }
                        }
                    }
                }
                else
                {
                    // software keys
                    av = _cLightning.EnumValues(Key, s);
                    foreach (string v in av)
                    {
                        if (v.Length > 4 && IsFileCandidate(v))
                        {
                            sr = CleanPath(v);
                            if (IsValidRoot(sr) && !FileExists(sr) && HasExtension(sr))
                            {
                                StoreResults(cLightning.ROOT_KEY.HKEY_LOCAL_MACHINE, s, v, sr, RESULT_TYPE.DeepSft);
                            }
                        }
                    }
                }
                CurrentPath(REG_HKLMB, s);
                KeyCount();
            }
        }
        #endregion

        #region Phase 12 : MRU Lists
        ///References:                From HKCU -> scan for valid link paths
        ///Method:                    Value type testing for valid entry
        ///Locations:                 HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\RecentDocs
        ///Locations:                 HKEY_CURRENT_USER\Software\Microsoft\Internet Explorer\TypedURLs
        ///Locations:                 HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\RunMRU
        ///Locations:                 HKEY_CURRENT_USER\Software\Microsoft\Search Assistant\ACMru\5603
        ///Locations:                 HKEY_CURRENT_USER\Software\Microsoft\Search Assistant\ACMru\5001
        ///Locations:                 HKEY_CURRENT_USER\Software\Microsoft\Search Assistant\ACMru\5647
        ///Locations:                 HKEY_CURRENT_USER\"Software\Microsoft\Windows\CurrentVersion\Applets\Wordpad\Recent File List"
        ///Locations:                 HKEY_CURRENT_USER\"Software\Microsoft\Windows\CurrentVersion\Applets\Regedit\Favorites"
        ///Locations:                 HKEY_CURRENT_USER\"Software\Microsoft\Windows\CurrentVersion\Applets\Regedit"
        ///Locations:                 HKEY_CURRENT_USER\"Software\Microsoft\Windows\CurrentVersion\Applets\Paint\Recent File List"
        ///Locations:                 HKEY_CURRENT_USER\"Software\Microsoft\Windows\CurrentVersion\Explorer\ComDlg32\LastVisitedMRU"
        ///Locations:                 HKEY_CURRENT_USER\"Software\Microsoft\Windows\CurrentVersion\Explorer\Wallpaper\MRU"
        ///Locations:                 HKEY_CURRENT_USER\"Software\Microsoft\MediaPlayer\Player\RecentFileList"
        ///Locations:                 HKEY_CURRENT_USER\"Software\Microsoft\MediaPlayer\Player\RecentURLList"
        private void MruScan(cLightning.ROOT_KEY Key, string SubKey)
        {
            LabelChange("MRU Scan", "Searching for MRU lists..");
            ArrayList al = _cLightning.EnumKeys(Key, SubKey);
            ArrayList cv = new ArrayList();

            foreach (string k in al)
            {
                cv = _cLightning.EnumValues(Key, SubKey + CHR_BSLASH + k);
                foreach (string v in cv)
                {
                    if (v.Length > 0)
                    {
                        if (MruFilter(v))
                        {
                            string nk = SubKey + CHR_BSLASH + k;
                            StoreResults(cLightning.ROOT_KEY.HKEY_CURRENT_USER, nk, v, STR_EMPTYVALUE, RESULT_TYPE.Mru);
                        }
                    }
                }
                CurrentPath(REG_HKCUB, k);
                KeyCount();
            }
        }
        #endregion

        #region Helpers
        private void AddKeys(cLightning.ROOT_KEY Key, string SubKey, ref ArrayList Keys)
        {
            ArrayList al = _cLightning.EnumKeys(Key, SubKey);
            // scan hkcr keys
            foreach (string s in al)
            {
                Keys.Add(SubKey + CHR_BSLASH + s);
                if (s.Length > 0 && (!s.Contains("Wow64")))//ignore wow key
                {
                    AddKeys(Key, SubKey + CHR_BSLASH + s, ref Keys);
                }
            }
        }

        private string CleanPath(string Path)
        {
            Match mc = _regPath.Match(Path);

            // test fast way first
            if (mc.Success && FileExists(mc.Groups[0].Value))
            {
                return mc.Groups[0].Value;
            }
            else
            {
                // extract path upon failure of regexp
                return ExtractPath(Path);
            }
        }

        private bool DirectoryExists(string Path)
        {
            return (Directory.Exists(Path));
        }

        private string ExtractPath(string Path)
        {
            string sp = Path.ToUpper();

            // test path first
            if (!FileExists(sp) && IsFileCandidate(sp)) //needed?? better spot maybe??
            {
                // trim to drive root
                if (sp.Substring(1, 1) != CHR_COLAN)
                {
                    sp = sp.Substring(sp.IndexOf(CHR_COLAN) - 1);
                }
                // truncate leading path
                if (sp.Substring(3).Contains(STR_PATH))
                {
                    sp = sp.Substring(sp.IndexOf(STR_PATH, 3) - 1);//PathFilter(path);
                }
                // find and trim to extension
                foreach (string s in _aExtensions)
                {
                    if (sp.Contains(s))
                    {
                        sp = sp.Substring(0, sp.IndexOf(s) + s.Length);
                        break;
                    }
                }
                // get the long path
                if (sp.Contains(CHR_TILDE))
                {
                    sp = GetLongName(sp);
                }
            }
            return sp;
        }

        public bool FileExists(string File)
        {
            return (GetFileAttributes(File) != INVALID_HANDLE_VALUE);
        }

        private ArrayList GetFileExtensions()
        {
            ArrayList md = _cLightning.EnumValues(cLightning.ROOT_KEY.HKEY_CLASSES_ROOT, MEDIATYPES);
            ArrayList lt = _cLightning.EnumKeys(cLightning.ROOT_KEY.HKEY_CLASSES_ROOT, "");
            ArrayList rs = new ArrayList();

            foreach (string s in md)
            {
                if (s.StartsWith("."))
                {
                    rs.Add(s.ToUpper());
                }
            }
            foreach (string s in lt)
            {
                if (s.StartsWith("."))
                {
                    if (!rs.Contains(s))
                    {
                        rs.Add(s.ToUpper());
                    }
                }
            }
            return rs;
        }

        private string GetLongName(string Path)
        {
            StringBuilder sb = new StringBuilder(255);
            GetLongPathName(Path, sb, 255);

            return sb.ToString();
        }

        private bool HasExtension(string Path)
        {
            foreach (string s in _aExtensions)
            {
                if (Path.Contains(s))
                {
                    return true;
                }
            }
            return false;
        }

        private string IdConverter(int id)
        {
            switch (id)
            {
                case 1:
                    return "Application ID Paths";
                case 2:
                    return "ProcServer Subkeys";
                case 3:
                    return "Type Libraries";
                case 4:
                    return "Interface Types";
                case 5:
                    return "Interface Proxy IDs";
                case 6:
                    return "Help File Paths";
                case 7:
                    return "Win32 Help Files";
                case 8:
                    return "Class Key Sub Paths";
                case 9:
                    return "Class Key Open Strings";
                case 10:
                    return "Class Key Edit Strings";
                case 11:
                    return "User Software Paths";
                case 12:
                    return "Class Name Registrations";
                case 13:
                    return "Clsid Registrations";
                case 14:
                    return "Icon Paths";
                case 15:
                    return "Font Paths";
                case 16:
                    return "Application Help Files";
                case 17:
                    return "Shared Libraries";
                case 18:
                    return "Startup Application Paths";
                case 19:
                    return "Uninstall Executable Paths";
                case 20:
                    return "Virtual Device Registration";
                case 21:
                    return "History Explorer Strings";
                case 22:
                    return "History Start Paths";
                case 23:
                    return "History Link Strings";
                case 24:
                    return "History Menu Strings";
                case 25:
                    return "Deep Native Software Strings";
                case 26:
                    return "Deep User Software Strings";
                default:
                    return "Most Recently Used Lists";
            }
        }

        private int IdToScope(int id)
        {
            switch (id)
            {
                case 1:
                    return 10;
                case 2:
                    return 10;
                case 3:
                    return 9;
                case 4:
                    return 9;
                case 5:
                    return 6;
                case 6:
                    return 6;
                case 7:
                    return 6;
                case 8:
                    return 10;
                case 9:
                    return 10;
                case 10:
                    return 6;
                case 11:
                    return 9;
                case 12:
                    return 9;
                case 13:
                    return 10;
                case 14:
                    return 7;
                case 15:
                    return 7;
                case 16:
                    return 4;
                case 17:
                    return 10;
                case 18:
                    return 8;
                case 19:
                    return 5;
                case 20:
                    return 4;
                case 21:
                    return 8;
                case 22:
                    return 8;
                case 23:
                    return 5;
                case 24:
                    return 5;
                case 25:
                    return 7;
                case 26:
                    return 8;
                default:
                    return 6;
            }
        }

        private bool IsFileCandidate(string Path)
        {
            if (Path.Contains(STR_PATH) && Path.Contains(CHR_PERIOD))
            {
                return true;
            }
            return false;
        }

        private bool IsValidPath(string Path)
        {
            return (Path.Contains(STR_PATH));
        }

        private bool IsValidRoot(string Path)
        {
            
            string d = "";
            if (Path.Contains(STR_PATH))
            {
                d = Path.Substring(0, Path.IndexOf(CHR_BSLASH) + 1);
                foreach (string s in _aDriveRoot)
                {
                    if (s.Contains(d))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private ArrayList KeyCollector(cLightning.ROOT_KEY Key, string SubKey)
        {
            ArrayList al = new ArrayList();

            al.Add(SubKey);
            AddKeys(Key, SubKey, ref al);
            return al;
        }

        private bool MruFilter(string value)
        {
            string s = value.ToUpper();

            if (IsNumeric(s))
            {
                return true;
            }
            else if (s.Contains("MRU"))
            {
                return true;
            }
            else if (s.Contains("FILE"))
            {
                return true;
            }
            else if (s.Contains("HISTORY"))
            {
                return true;
            }
            else if (s.Contains("LIST"))
            {
                return true;
            }
            else if (s.Contains("URI"))
            {
                return true;
            }
            else if (s.Contains("RECENT"))
            {
                return true;
            }
            else if (s.Contains("LAST"))
            {
                return true;
            }
            return false;
        }

        public bool Is64BitOperatingSystem()
        {
            if (IntPtr.Size == 8)  // 64-bit programs run only on Win64
            {
                return true;
            }
            else  // 32-bit programs run on both 32-bit and 64-bit Windows
            {
                // Detect whether the current process is a 32-bit process 
                // running on a 64-bit system.
                bool flag;
                return ((DoesWin32MethodExist("kernel32.dll", "IsWow64Process") &&
                    IsWow64Process(GetCurrentProcess(), out flag)) && flag);
            }
        }

        private bool DoesWin32MethodExist(string moduleName, string methodName)
        {
            IntPtr moduleHandle = GetModuleHandle(moduleName);
            if (moduleHandle == IntPtr.Zero)
            {
                return false;
            }
            return (GetProcAddress(moduleHandle, methodName) != IntPtr.Zero);
        }

        private bool IsNumeric(string value)
        {
            short x = 0;
            return (Int16.TryParse(value, out x));
        }

        private void StoreCommonPaths()
        {
            _sSystem32Directory = Environment.GetFolderPath(Environment.SpecialFolder.System).ToUpper() + CHR_BSLASH;
            _sUserProgramsDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Programs).ToUpper() + CHR_BSLASH;
            _sWindowsDirectory = _sSystem32Directory.Substring(0, _sSystem32Directory.IndexOf("SYSTEM32"));
            _sFontsDirectory = _sWindowsDirectory + @"FONTS\";
            _sProgramsDirectory = _sWindowsDirectory.Substring(0, _sWindowsDirectory.IndexOf("WINDOWS")) + @"PROGRAM FILES\";
        }

        private void StoreLogicalDrives()
        {
            string[] a = Directory.GetLogicalDrives();
            int ct = 0;
            _aDriveRoot = new string[1];

            foreach (string s in a)
            {
                if (Directory.Exists(s))
                {
                    ct += 1;
                    Array.Resize(ref _aDriveRoot, ct);
                    _aDriveRoot[ct - 1] = s;
                }
            }
        }

        private string TestSystemPaths(string Path)
        {
            string sp = Path.ToUpper();

            if (sp.Contains("%"))
            {
                if (sp.Contains("%PROGRAMFILES%"))
                {
                    sp.Replace("%PROGRAMFILES%", _sProgramsDirectory);
                }
                else if (sp.Contains("%SYSTEM%"))
                {
                    sp.Replace("%SYSTEM%", _sSystem32Directory);
                }
                else if (sp.Contains("%WINDOWS%"))
                {
                    sp.Replace("%WINDOWS%", _sWindowsDirectory);
                }
            }
            return sp;
        }
        #endregion
        #endregion
    }
}

#region Saved
/*

      //  [DllImport("shell32.dll", SetLastError = true)]
      //  private static extern int SHGetSpecialFolderLocation(IntPtr hwndOwner, eFlags nFolder, ref IntPtr ppidl);

     //   [ DllImport( "kernel32.dll" )] 
     //   [return : MarshalAs(UnmanagedType.Bool)]
     //   private static extern bool GetVersionEx(ref OSVERSIONINFO osvi);  
        #region Enums
        [Flags]
        private enum eFlags : int
        {
            CSIDL_DESKTOPDIRECTORY = 0,
            CSIDL_START_PROGRAMS = 2,
            CSIDL_MYDOCUMENTS = 5,
            CSIDL_FAVORITES = 6,
            CSIDL_STARTUP = 7,
            CSIDL_RECENT = 8,
            CSIDL_SENDTO = 9,
            CSIDL_START_MENU = 11,
            CSIDL_MYMUSIC = 13,
            CSIDL_MYVIDEO = 14,
            CSIDL_DESKTOP = 16,
            CSIDL_NETHOOD = 19,
            CSIDL_FONTS = 20,
            CSIDL_TEMPLATES = 21,
            CSIDL_COMMON_STARTMENU = 22,
            CSIDL_COMMON_PROGRAMS = 23,
            CSIDL_COMMON_STARTUP = 24,
            CSIDL_COMMON_DESKTOP = 25,
            CSIDL_APPDATA = 26,
            CSIDL_PRINTHOOD = 27,
            CSIDL_SETTINGS_APPDATA = 28,
            CSIDL_COMMON_FAVORITES = 31,
            CSIDL_INTERNET_CACHE = 32,
            CSIDL_COOKIES = 33,
            CSIDL_HISTORY = 34,
            CSIDL_COMMON_APPDATA = 35,
            CSIDL_WINDOWS = 36,
            CSIDL_SYSTEM = 37,
            CSIDL_PROGRAM_FILES = 38,
            CSIDL_MYPICTURES = 39,
            CSIDL_PROFILE = 40,
            CSIDL_COMMON_SYSTEM = 42,
            CSIDL_COMMON_FILES = 43,
            CSIDL_COMMON_TEMPLATES = 45,
            CSIDL_COMMON_DOCUMENTS = 46,
            CSIDL_COMMON_MUSIC = 53,
            CSIDL_COMMON_PICTURES = 54,
            CSIDL_COMMON_VIDEO = 55,
            CSIDL_RESOURCES = 56,
            CSIDL_CD_BURN_AREA = 56,
        }
        #endregion

        #region Stucts
        private struct SHITEMID {
            internal int cb;
            internal byte abID;
        }

        private struct ITEMIDLIST {
            internal SHITEMID mkid;
            }

        private struct OSVERSIONINFO {
            internal uint dwOSVersionInfoSize;
            internal uint dwMajorVersion;
            internal uint dwMinorVersion;
            internal uint dwBuildNumber;
            internal uint dwPlatformId;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            internal string szCSDVersion;
            internal Int16 wServicePackMajor;
            internal Int16 wServicePackMinor;
            internal Int16 wSuiteMask;
            internal Byte wProductType;
            internal Byte wReserved;
        }
        #endregion

        private string ExtractName(string sFile)
        {
            int lLen = 0;
            char[] ch;
            string path = sFile;

            lLen = path.IndexOf(CHR_PERIOD + 1);
            for (int i = 0; i < lLen; i++)
            {
                ch = path.Substring(i, 1).ToCharArray();
                if (InvalidChar(ch[0]))
                {
                    return path.Substring(1, i - 1);
                }
            }
            return path;
        }

        private bool InvalidChar(char ch)
        {
            if (ch < 65)
            {
                if (ch < 45 || ch > 57)
                {
                    return true;
                }
            }
            else if (ch > 122)
            {
                return true;
            }
            else if(ch > 90 && ch < 97)
            {
               // if (ch != 95)
               // {
               //     return true;
               // }
            }

            return false;
        }

        private string ExtractFileName(string sPath)
        {
            //StringBuilder sBuffer = new StringBuilder("0", 255);

            //sBuffer = new String('0', 255);
            //GetFileTitle(sPath, sBuffer, sBuffer.Length);
            //return sBuffer.ToString().TrimEnd();
            //start at period
            //go until you get an illegal char
            string temp = sPath;
            char[] ch;
            if (temp.Contains(CHR_PERIOD))
            {
                for (int i = temp.IndexOf(CHR_PERIOD) + 1; i < temp.Length; i++)
                {
                    ch = temp.Substring(i, 1).ToCharArray();
                    if (InvalidChar(ch[0]))
                    {
                        temp = temp.Substring(0, i);
                        break;
                    }
                }
            }
            return temp;
        }

        private string PathFilter(string sPath)
        {
            string path = sPath;

            if (path.Contains(APP_START))
            {
                return path.Substring(path.IndexOf(CHR_COLAN) - 1);
            }
            else if (path.Contains(APP_RUN))
            {
                return path.Substring(path.IndexOf(CHR_COLAN) - 1);
            }
            else if (path.Contains(APP_REG))
            {
                return path.Substring(path.IndexOf(CHR_COLAN) - 1);
            }
            else if (path.Contains(APP_MSIE))
            {
                return path.Substring(path.IndexOf(CHR_COLAN) - 1);
            }
            else if (path.IndexOf(CHR_COLAN) != 2)
            {
                return path.Substring(path.IndexOf(CHR_COLAN) - 1);
            }
            return sPath;
        }



*/
#endregion
