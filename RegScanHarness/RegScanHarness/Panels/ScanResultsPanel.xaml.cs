using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VTRegScan;

namespace ScanX.Panels
{
    public partial class ScanResultsPanel : UserControl
    {
        #region Constants
        private const string APP_DESC = "The Control registration key contains invalid data. This is usually the result of an incomplete uninstallation of the parent application. It is recommended that residual references of this nature be removed, as they can lead to system slowdowns.";
        private const string PROC_DESC = "The Control registration key references a control path that can not be found on the system. This is usually the result of an incomplete uninstallation of the parent application. It is recommended that residual references of this nature be removed, as they can lead to system slowdowns.";
        private const string TYPE_DESC = "The TypeLib reference contains invalid data. This may be caused by a faulty or incomplete installation of an application software, or by an incomplete uninstallation. It is recommended that this value be removed, as it can lead to system slowdowns and application instability.";
        private const string HELP_DESC = "The Help Directory Key contains invalid data. This may be caused by an incomplete application installation, or by unistallation of the parent application. It is recommended that the value be removed, as it can lead to system slowdowns.";
        private const string WIN32_DESC = "The Win32 Key contains invalid data. This is usually the result of an incomplete uninstallation of the parent software. It is recommended that this value be removed, as it can lead to system slowdowns, and application instability.";
        private const string SHELL_DESC = "The Shell Key contains invalid data. This is usually the result of an incomplete uninstallation of the parent software. It is recommended that this value be removed, as it can lead to system slowdowns, and application instability.";
        private const string SUB_DESC = "The value points to an invalid shell reference. This reference can not be found on the system. This is usually the result of an incomplete uninstallation of the parent software. It is recommended that this value be removed, as it can lead to system slowdowns, and application instability.";
        private const string EDIT_DESC = "The shell edit key contains invalid data. This is usually the result of an incomplete uninstallation of the parent software. It is recommended that this value be removed, as it can lead to system slowdowns, and application instability.";
        private const string USER_DESC = "The value points to the file that can not be found on the system. The file may have been moved or the parent application was uninstalled. It is recommended that this value be removed, as it can lead to system slowdowns.";
        private const string CLSID_DESC = "The class id registration contains invalid data. This is usually the result of an incomplete uninstallation of the parent software. It is recommended that this value be removed, as it can lead to system slowdowns.";
        private const string FONT_DESC = "The Registered Font can not be found on the system. This could be caused by an incomplete software installation, or a system settings change. It is recommended that the font path value be removed.";
        private const string HELP32_DESC = "The Application Help file can not be found on the system. This could be caused by an incomplete software installation, or a system settings change. It is recommended that the font path value be removed.";
        private const string SHARED_DESC = "The Shared file reference is an invalid entry. This is usually the result of a missing dependency file. or software location change. It is recommended that the file path value be removed.";
        private const string START_DESC = "The Startup file path is an invalid entry. The file referenced can not be found. It is recommended that the reference be removed, as it can lead to slow operating system startup.";
        private const string INSTALL_DESC = "The Uninstall string is an invalid entry. The path to the uninstallation file can not be located, and the software may have already been removed. It is recommended that the reference be removed, as it is an unneeded entry.";
        private const string VDF_DESC = "The Virtual Device setting has an error. The reference value needs to be removed. The value references an obsolete virtual machine string, and should be removed.";
        private const string HISTORY_DESC = "The History value references a file that can not be found on the system. This file may have been moved or deleted, and the reference is no longer needed. The value references a file that no longer exists, and should be removed.";
        private const string PATH_DESC = "The File Path references a file that can not be found on the system. This file may have been moved or deleted, and the reference is no longer needed. The value references a file that no longer exists, and should be removed.";
        private const string MRU_DESC = "MRUs are lists that keep track of the applications you use, and the data that they use. For issues of privacy, these entries can be removed.";
        #endregion

        #region Constructor
        public ScanResultsPanel()
        {
            InitializeComponent();
        }
        #endregion

        #region Control Events
        private void CheckBox_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            // bug: somehow checkboxes got broken..
            ScanData d = (ScanData)lstResults.Items.CurrentItem;
            if (d != null)
            {
                d.Check = !d.Check;
            }
        }

        private void MenuItem_Clicked(object sender, RoutedEventArgs e)
        {
            MenuItem m = (MenuItem)sender;
            if (m != null)
            {
                ScanData s = (ScanData)lstResults.Items.CurrentItem;
                if (s != null)
                {
                    switch (m.Header.ToString())
                    {
                        case "Select Item":
                            {
                                s.Check = true;
                                break;
                            }
                        case "Deselect Item":
                            {
                                s.Check = false;
                                break;
                            }
                        case "Select All":
                            {
                                CheckItems(true);
                                break;
                            }
                        case "Deselect All":
                            {
                                CheckItems(false);
                                break;
                            }
                        case "Go to Key":
                            {
                                OpenKey(s);
                                break;
                            }
                        case "Go to Folder":
                            {
                                OpenFolder(s);
                                break;
                            }
                        case "Item Details":
                            {
                                wndDetails w = new wndDetails();
                                ScanData d = (ScanData)lstResults.Items.CurrentItem;
                                w.txtType.Text = d.Name;
                                w.txtRoot.Text = d.Root.ToString();
                                w.txtSubkey.Text = d.Key;
                                w.txtValue.Text = d.Value;
                                w.txtDetails.Text = IdToDescription(d.Id);
                                w.ShowDialog();
                                break;
                            }
                        case "Help":
                            {
                                OpenApp("http://www.vtdev.com/");
                                break;
                            }
                    }
                }
            }
        }

        private void Start_Clicked(object sender, RoutedEventArgs e)
        {
            //
        }

        #endregion

        #region Helpers
        private void CheckItems(bool check)
        {
            if (lstResults.Items != null)
            {
                foreach (ScanData o in lstResults.Items)
                {
                    o.Check = check;
                }
            }
        }

        private string IdToDescription(int id)
        {
            switch (id)
            {
                case 1:     //ControlAppID
                case 2:     //ControlProcServer
                    return APP_DESC;
                case 3:     //ControlTypeLib
                    return TYPE_DESC;
                case 4:     //ControlInterfaceType
                case 5:     //ControlInterfaceProxy
                    return PROC_DESC;
                case 6:     //ControlTypeHelp
                    return HELP_DESC;
                case 7:     //ControlTypeWin32
                    return WIN32_DESC;
                case 8:     //ControlClassSubExt
                    return SHELL_DESC;
                case 9:     //ControlClassSubOpen
                    return SUB_DESC;
                case 10:    //ControlClassSubEdit
                    return EDIT_DESC;
                case 11:    //User
                    return USER_DESC;
                case 12:    //FullClassName
                case 13:    //FullClsid
                case 14:    //FullIcon
                    return CLSID_DESC;
                case 15:    //Font
                    return FONT_DESC;
                case 16:    //Help
                    return HELP32_DESC;
                case 17:    //Shared
                    return SHARED_DESC;
                case 18:    //Startup
                    return START_DESC;
                case 19:    //Uninstall
                    return INSTALL_DESC;
                case 20:    //Vdf
                    return VDF_DESC;
                case 21:    //HistoryExplorer
                case 22:    //HistoryStart
                case 23:    //HistoryLink
                case 24:    //HistoryMenu
                    return HISTORY_DESC;
                case 25:    //DeepMs
                case 26:    //DeepSft
                    return PATH_DESC;
                default:    //Mru
                    return MRU_DESC;
            }
        }

        private void OpenApp(string path)
        {
            cLightning lgt = new cLightning();
            lgt.ShellOpen(path, cLightning.SHOW_COMMANDS.SW_NORMAL);
        }

        private void OpenKey(ScanData data)
        {
            string key = data.Key;
            if (key.Length > 0)
            {
                key = data.Root + @"\" + data.Key;
                cLightning cr = new VTRegScan.cLightning();
                if (cr.SetJumpKey(key))
                {
                    cr.ShellOpen("regedit", VTRegScan.cLightning.SHOW_COMMANDS.SW_NORMAL);
                }
            }
        }

        private void OpenFolder(ScanData data)
        {
            string folder = data.Value;
            if (folder.Length > 0 && folder.Contains(@":\"))
            {
                try
                {
                    folder = folder.Substring(0, folder.LastIndexOf(@"\") + 1);
                    cLightning cr = new VTRegScan.cLightning();
                    cr.ShellOpen(folder, VTRegScan.cLightning.SHOW_COMMANDS.SW_NORMAL);
                }
                finally { }
            }
        }
        #endregion
    }
}
