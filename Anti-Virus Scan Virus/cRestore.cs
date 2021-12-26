#region Directives
using System;
using System.Text;
using System.Runtime.InteropServices;
using System.Security.Permissions;
#endregion

namespace VTRegScan
{
    [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
    public class cRestore
    {
        #region Constants
        private const int APPLICATION_INSTALL = 0;
        private const int APPLICATION_UNINSTALL = 1;
        private const int DEVICE_DRIVER_INSTALL = 10;
        private const int MODIFY_SETTINGS = 12;
        private const int CANCELLED_OPERATION = 13;
        private const int FO_DELETE = 0x3;
        private const int FOF_ALLOWUNDO = 0x40;
        private const int FOF_NOCONFIRMATION = 0x10;
        private const int BEGIN_SYSTEM_CHANGE = 100;
        private const int END_SYSTEM_CHANGE = 101;
        private const int BEGIN_NESTED_SYSTEM_CHANGE = 102;
        private const int END_NESTED_SYSTEM_CHANGE = 103;
        private const int DESKTOP_SETTING = 2;
        private const int ACCESSIBILITY_SETTING = 3;
        private const int OE_SETTING = 4;
        private const int APPLICATION_RUN = 5;
        private const int WINDOWS_SHUTDOWN = 8;
        private const int WINDOWS_BOOT = 9;
        private const int MAX_DESC = 64;
        private const int MAX_DESC_W = 256;
        private const string RESTORE_KEY = @"Software\Microsoft\Windows NT\CurrentVersion\SystemRestore";
        private const string RESTORE_VALUE = @"SystemRestorePointCreationFrequency";
        #endregion

        #region Enum
        private enum RESTORE_TYPE
        {
            APPLICATION_INSTALL = 0,
            APPLICATION_UNINSTALL = 1,
            MODIFY_SETTINGS = 12,
            CANCELLED_OPERATION = 13,
            RESTORE = 6,
            CHECKPOINT = 7,
            DEVICE_DRIVER_INSTALL = 10,
            FIRSTRUN = 11,
            BACKUP_RECOVERY = 14,
        }
        #endregion

        #region Structs
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        internal struct RESTOREPTINFO 
        {
            public int dwEventType;
            public int dwRestorePtType;
            public Int64 llSequenceNumber;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_DESC_W + 1)]
            public string szDescription;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct SMGRSTATUS 
        {
            public int nStatus;
            public Int64 llSequenceNumber;
        }
        #endregion

        #region API
        [DllImport("srclient.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SRSetRestorePointW(ref RESTOREPTINFO pRestorePtSpec, out SMGRSTATUS pSMgrStatus);
        #endregion

        #region Fields
        private long _lSeqNum = 0;
        private int _iRestInt = 0;
        #endregion

        #region Methods
        public bool StartRestore(string Description)
        {
            int maj = Environment.OSVersion.Version.Major;
            int min = Environment.OSVersion.Version.Minor;
            RESTOREPTINFO tRPI = new RESTOREPTINFO();
            SMGRSTATUS tStatus = new SMGRSTATUS();
            
            // compatability
            if (!(maj == 4 && min == 90 || maj > 4))
            {
                return false;
            }

            tRPI.dwEventType = BEGIN_SYSTEM_CHANGE;
            tRPI.dwRestorePtType = (int)RESTORE_TYPE.MODIFY_SETTINGS;
            tRPI.llSequenceNumber = 0;
            tRPI.szDescription = Description;

            // test for key that defines multiple restores per cycle
            cLightning cl = new cLightning();
            if (cl.ValueExists(cLightning.ROOT_KEY.HKEY_LOCAL_MACHINE, RESTORE_KEY, RESTORE_VALUE))
            {
                _iRestInt = cl.ReadDword(cLightning.ROOT_KEY.HKEY_LOCAL_MACHINE, RESTORE_KEY, RESTORE_VALUE);
            }
            // set to 2 minutes
            cl.WriteDword(cLightning.ROOT_KEY.HKEY_LOCAL_MACHINE, RESTORE_KEY, RESTORE_VALUE, 2);
            if (SRSetRestorePointW(ref tRPI, out tStatus))
            {
                _lSeqNum = tStatus.llSequenceNumber;
                return true;
            }
            return false;
        }

        public bool EndRestore(bool Cancel)
        {
            RESTOREPTINFO tRPI = new RESTOREPTINFO();
            SMGRSTATUS tStatus = new SMGRSTATUS();
            bool success = false;

            tRPI.dwEventType = END_SYSTEM_CHANGE;
            tRPI.llSequenceNumber = _lSeqNum;

            if (Cancel == true)
            {
                tRPI.dwRestorePtType = CANCELLED_OPERATION;
            }

            try
            {
                success = (SRSetRestorePointW(ref tRPI, out tStatus));
            }
            finally 
            {
                // reset
                cLightning cl = new cLightning();
                cl.WriteDword(cLightning.ROOT_KEY.HKEY_LOCAL_MACHINE, RESTORE_KEY, RESTORE_VALUE, _iRestInt);
            }
            return success;
        }
        #endregion
    }
}
