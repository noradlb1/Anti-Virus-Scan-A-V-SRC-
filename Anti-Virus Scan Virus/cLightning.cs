#region Directives
using System;
using System.Collections;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;
using System.Security.Permissions;
#endregion




namespace VTRegScan
{
    [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
    public class cLightning
    {
        #region Constants
        // access paramaters
        private const int KEY_ALL_ACCESS = 0xF003F;
        private const int KEY_CREATE_LINK = 0x20;
        private const int KEY_CREATE_SUB_KEY = 0x4;
        private const int KEY_ENUMERATE_SUB_KEYS = 0x8;
        private const int KEY_EXECUTE = 0x20019;
        private const int KEY_NOTIFY = 0x10;
        private const int KEY_QUERY_VALUE = 0x1;
        private const int KEY_READ = 0x20019;
        private const int KEY_SET_VALUE = 0x2;
        private const int KEY_WRITE = 0x20006;
        private const int REG_OPTION_NON_VOLATILE = 0x0;
        private const int REG_ERR_OK = 0x0;
        private const int REG_ERR_NOT_EXIST = 0x1;
        private const int REG_ERR_NOT_STRING = 0x2;
        private const int REG_ERR_NOT_DWORD = 0x4;
        private const int REG_FORCE_RESTORE = 0x8;
        private const int KEY_WOW64_64KEY = 0x100;
        private const int KEY_WOW64_32KEY = 0x200;

        // error handling
        private const int ERROR_NONE = 0x0;
        private const int ERROR_BADDB = 0x1;
        private const int ERROR_BADKEY = 0x2;
        private const int ERROR_CANTOPEN = 0x3;
        private const int ERROR_CANTREAD = 0x4;
        private const int ERROR_CANTWRITE = 0x5;//access denied in w7
        private const int ERROR_OUTOFMEMORY = 0x6;
        private const int ERROR_ARENA_TRASHED = 0x7;
        private const int ERROR_ACCESS_DENIED = 0x8;
        private const int ERROR_INVALID_PARAMETERS = 0x57;
        private const int ERROR_MORE_DATA = 0xEA;
        private const int ERROR_NO_MORE_ITEMS = 0x103;

        // token
        private const int SE_PRIVILEGE_ENABLED = 0x2;
        private const int SYNCHRONIZE = 0x100000;
        private const int STANDARD_RIGHTS_REQUIRED = 0xF0000;
        private const int PROCESS_ALL_ACCESS = (STANDARD_RIGHTS_REQUIRED | SYNCHRONIZE | 0xFFF);

        // privilege
        private const string SE_CREATE_TOKEN_NAME = "SeCreateTokenPrivilege";
        private const string SE_ASSIGNPRIMARYTOKEN_NAME = "SeAssignPrimaryTokenPrivilege";
        private const string SE_TCB_NAME = "SeTcbPrivilege";
        private const string SE_SECURITY_NAME = "SeSecurityPrivilege";
        private const string SE_DEBUG_NAME = "SeDebugPrivilege";
        private const string SE_AUDIT_NAME = "SeAuditPrivilege";
        private const int INVALID_HANDLE_VALUE = -1;

        private const int VALID_FILE = 32;
        private const UInt32 Infinite = 0xffffffff;
        private const Int32 Startf_UseStdHandles = 0x00000100;
        private const Int32 StdOutputHandle = -11;
        private const Int32 StdErrorHandle = -12;
        #endregion

        #region Enums
        // root keys
        public enum ROOT_KEY : uint
        {
            HKEY_CLASSES_ROOT = 0x80000000,
            HKEY_CURRENT_USER = 0x80000001,
            HKEY_LOCAL_MACHINE = 0x80000002,
            HKEY_USERS = 0x80000003,
            HKEY_PERFORMANCE_DATA = 0x80000004,
            HKEY_CURRENT_CONFIG = 0x80000005,
            HKEY_DYN_DATA = 0x80000006
        }

        // value types
        public enum VALUE_TYPE : uint
        {
            REG_NONE,
            REG_SZ = 1,
            REG_EXPAND_SZ = 2,
            REG_BINARY = 3,
            REG_DWORD = 4,
            REG_DWORD_LITTLE_ENDIAN = 4,
            REG_DWORD_BIG_ENDIAN = 5,
            REG_LINK = 6,
            REG_MULTI_SZ = 7,
            REG_RESOURCE_LIST = 8,
            REG_FULL_RESOURCE_DESCRIPTOR = 9,
            REG_RESOURCE_REQUIREMENTS_LIST = 10,
            REG_QWORD_LITTLE_ENDIAN = 11
        }

        // token privileges
        private enum ETOKEN_PRIVILEGES : uint
        {
            ASSIGN_PRIMARY = 0x1,
            TOKEN_DUPLICATE = 0x2,
            TOKEN_IMPERSONATE = 0x4,
            TOKEN_QUERY = 0x8,
            TOKEN_QUERY_SOURCE = 0x10,
            TOKEN_ADJUST_PRIVILEGES = 0x20,
            TOKEN_ADJUST_GROUPS = 0x40,
            TOKEN_ADJUST_DEFAULT = 0x80,
            TOKEN_ADJUST_SESSIONID = 0x100
        }

        public enum SHOW_COMMANDS : int
        {
            SW_HIDE = 0,
            SW_SHOWNORMAL = 1,
            SW_NORMAL = 1,
            SW_SHOWMINIMIZED = 2,
            SW_SHOWMAXIMIZED = 3,
            SW_MAXIMIZE = 3,
            SW_SHOWNOACTIVATE = 4,
            SW_SHOW = 5,
            SW_MINIMIZE = 6,
            SW_SHOWMINNOACTIVE = 7,
            SW_SHOWNA = 8,
            SW_RESTORE = 9,
            SW_SHOWDEFAULT = 10,
            SW_FORCEMINIMIZE = 11,
            SW_MAX = 11
        }
        #endregion

        #region Structs
        // filetime (unused)
        [StructLayout(LayoutKind.Sequential)]
        private struct FILETIME
        {
            public int dwLowDateTime;
            public int dwHighDateTime;
        }

        // permissions
        [StructLayout(LayoutKind.Sequential)]
        private struct SECURITY_ATTRIBUTES
        {
            public int nLength;
            public int lpSecurityDescriptor;
            public bool bInheritHandle;
        }

        // perm luid
        [StructLayout(LayoutKind.Sequential)]
        private struct LUID
        {
            public int LowPart;
            public int HighPart;
        }

        // perm attributes
        [StructLayout(LayoutKind.Sequential)]
        private struct LUID_AND_ATTRIBUTES
        {
            public LUID pLuid;
            public int Attributes;
        }

        // perm token
        [StructLayout(LayoutKind.Sequential)]
        private struct TOKEN_PRIVILEGES
        {
            public int PrivilegeCount;
            public LUID_AND_ATTRIBUTES Privileges;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct StartupInfo
        {
            public int cb;
            public String reserved;
            public String desktop;
            public String title;
            public int x;
            public int y;
            public int xSize;
            public int ySize;
            public int xCountChars;
            public int yCountChars;
            public int fillAttribute;
            public int flags;
            public UInt16 showWindow;
            public UInt16 reserved2;
            public byte reserved3;
            public IntPtr stdInput;
            public IntPtr stdOutput;
            public IntPtr stdError;
        }

        private struct ProcessInformation
        {
            public IntPtr process;
            public IntPtr thread;
            public int processId;
            public int threadId;
        }
        #endregion

        #region API
        // enable priviledge decl
        [DllImport("advapi32.dll", EntryPoint = "AdjustTokenPrivileges", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool AdjustTokenPrivileges(IntPtr TokenHandle, int DisableAllPriv, ref TOKEN_PRIVILEGES NewState, int BufferLength, int PreviousState, int ReturnLength);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, EntryPoint = "LookupPrivilegeValueW", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool LookupPrivilegeValue(int lpSystemName, [MarshalAs(UnmanagedType.LPWStr)]string lpName, ref LUID lpLuid);
        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, EntryPoint = "LookupPrivilegeValueA", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool LookupPrivilegeValueA(int lpSystemName, [MarshalAs(UnmanagedType.LPStr)]string lpName, ref LUID lpLuid);

        [DllImport("advapi32.dll", EntryPoint = "OpenProcessToken", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool OpenProcessToken(IntPtr ProcessHandle, uint DesiredAccess, ref IntPtr TokenHandle);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, EntryPoint = "OpenProcess", SetLastError = true)]
        private static extern IntPtr OpenProcess(int dwDesiredAccess, int blnheritHandle, int dwAppProcessId);

        [DllImport("kernel32.dll", EntryPoint = "CloseHandle", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, EntryPoint = "GetCurrentProcessId", SetLastError = true)]
        private static extern int GetCurrentProcessId();

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, EntryPoint = "RtlMoveMemory", SetLastError = true)]
        private static extern int RtlMoveMemory(ref string Destination, ref byte[] Source, int Length);

        // RegQueryValueEx overloads- added ansi decl versions for old platforms [untested AND no methods provided]
        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, EntryPoint = "RegQueryValueExW", SetLastError = true)]
        static extern int RegQueryValueEx(UIntPtr hKey, [MarshalAs(UnmanagedType.LPWStr)]string lpValueName, int lpReserved, out uint lpType, [Optional] System.Text.StringBuilder lpData, ref uint lpcbData);
        [DllImport("advapi32.dll", CharSet = CharSet.Ansi, EntryPoint = "RegQueryValueExA", SetLastError = true)]
        static extern int RegQueryValueExA(UIntPtr hKey, [MarshalAs(UnmanagedType.LPStr)]string lpValueName, int lpReserved, out uint lpType, [Optional] [MarshalAs(UnmanagedType.LPStr)]string lpData, ref uint lpcbData);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, EntryPoint = "RegQueryValueExW", SetLastError = true)]
        static extern int RegQueryValueEx(UIntPtr hKey, [MarshalAs(UnmanagedType.LPWStr)]string lpValueName, int lpReserved, ref uint lpType, [Optional] ref byte lpData, ref uint lpcbData);
        [DllImport("advapi32.dll", CharSet = CharSet.Ansi, EntryPoint = "RegQueryValueExA", SetLastError = true)]
        static extern int RegQueryValueExA(UIntPtr hKey, [MarshalAs(UnmanagedType.LPStr)]string lpValueName, int lpReserved, ref uint lpType, [Optional] ref byte lpData, ref uint lpcbData);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, EntryPoint = "RegQueryValueExW", SetLastError = true)]
        static extern int RegQueryValueEx(UIntPtr hKey, [MarshalAs(UnmanagedType.LPWStr)]string lpValueName, int lpReserved, ref uint lpType, [Optional] ref int lpData, ref uint lpcbData);
        [DllImport("advapi32.dll", CharSet = CharSet.Ansi, EntryPoint = "RegQueryValueExA", SetLastError = true)]
        static extern int RegQueryValueExA(UIntPtr hKey, [MarshalAs(UnmanagedType.LPStr)]string lpValueName, int lpReserved, ref uint lpType, [Optional] ref int lpData, ref uint lpcbData);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, EntryPoint = "RegQueryValueExW", SetLastError = true)]
        static extern int RegQueryValueEx(UIntPtr hKey, [MarshalAs(UnmanagedType.LPWStr)]string lpValueName, int lpReserved, ref uint lpType, [Optional] ref long lpData, ref uint lpcbData);
        [DllImport("advapi32.dll", CharSet = CharSet.Ansi, EntryPoint = "RegQueryValueExA", SetLastError = true)]
        static extern int RegQueryValueExA(UIntPtr hKey, [MarshalAs(UnmanagedType.LPStr)]string lpValueName, int lpReserved, ref uint lpType, [Optional] ref long lpData, ref uint lpcbData);

        // RegSetValueEx overloads
        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, EntryPoint = "RegSetValueExW", SetLastError = true)]
        private static extern int RegSetValueEx(UIntPtr hKey, [MarshalAs(UnmanagedType.LPWStr)]string lpValueName, int Reserved, uint dwType, ref int lpData, int cbData);
        [DllImport("advapi32.dll", CharSet = CharSet.Ansi, EntryPoint = "RegSetValueExA", SetLastError = true)]
        private static extern int RegSetValueExA(UIntPtr hKey, [MarshalAs(UnmanagedType.LPStr)]string lpValueName, int Reserved, uint dwType, ref int lpData, int cbData);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, EntryPoint = "RegSetValueExW", SetLastError = true)]
        private static extern int RegSetValueEx(UIntPtr hKey, [MarshalAs(UnmanagedType.LPWStr)]string lpValueName, int Reserved, uint dwType, ref long lpData, int cbData);
        [DllImport("advapi32.dll", CharSet = CharSet.Ansi, EntryPoint = "RegSetValueExA", SetLastError = true)]
        private static extern int RegSetValueExA(UIntPtr hKey, [MarshalAs(UnmanagedType.LPStr)]string lpValueName, int Reserved, uint dwType, ref long lpData, int cbData);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, EntryPoint = "RegSetValueExW", SetLastError = true)]
        private static extern int RegSetValueEx(UIntPtr hKey, [MarshalAs(UnmanagedType.LPWStr)]string lpValueName, int Reserved, uint dwType, IntPtr lpData, int cbData);
        [DllImport("advapi32.dll", CharSet = CharSet.Ansi, EntryPoint = "RegSetValueEA", SetLastError = true)]
        private static extern int RegSetValueExA(UIntPtr hKey, [MarshalAs(UnmanagedType.LPStr)]string lpValueName, int Reserved, uint dwType, [MarshalAs(UnmanagedType.LPStr)]string lpData, int cbData);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, EntryPoint = "RegSetValueExW", SetLastError = true)]
        private static extern int RegSetValueEx(UIntPtr hKey, [MarshalAs(UnmanagedType.LPWStr)]string lpValueName, int Reserved, uint dwType, ref byte lpData, int cbData);
        [DllImport("advapi32.dll", CharSet = CharSet.Ansi, EntryPoint = "RegSetValueExA", SetLastError = true)]
        private static extern int RegSetValueExA(UIntPtr hKey, [MarshalAs(UnmanagedType.LPStr)]string lpValueName, int Reserved, uint dwType, ref byte lpData, int cbData);

        // registry
        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, EntryPoint = "RegCloseKey", SetLastError = true)]
        private static extern int RegCloseKey(UIntPtr hKey);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, EntryPoint = "RegCreateKeyW", SetLastError = true)]
        private static extern int RegCreateKey(ROOT_KEY hKey, [MarshalAs(UnmanagedType.LPWStr)]string subKey, ref UIntPtr phkResult);
        [DllImport("advapi32.dll", CharSet = CharSet.Ansi, EntryPoint = "RegCreateKeyA", SetLastError = true)]
        private static extern int RegCreateKeyA(ROOT_KEY hKey, [MarshalAs(UnmanagedType.LPStr)]string subKey, ref UIntPtr phkResult);

        [DllImport("advapi32.dll", EntryPoint = "RegOpenKeyEx")]
        private static extern int RegOpenKeyEx(ROOT_KEY hKey, string subKey, int options, int samDesired, ref UIntPtr phkResult);
        [DllImport("advapi32.dll", CharSet = CharSet.Ansi, EntryPoint = "RegOpenKeyExA", SetLastError = true)]
        private static extern int RegOpenKeyExA(ROOT_KEY hKey, [MarshalAs(UnmanagedType.LPStr)]string subKey, int options, int samDesired, ref UIntPtr phkResult);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, EntryPoint = "RegDeleteKeyA", SetLastError = true)]
        private static extern int RegDeleteKeyA(ROOT_KEY hKey, [MarshalAs(UnmanagedType.LPStr)]string subKey);
        [DllImport("advapi32.dll", CharSet = CharSet.Ansi, EntryPoint = "RegDeleteKeyW", SetLastError = true)]
        private static extern int RegDeleteKey(ROOT_KEY hKey, [MarshalAs(UnmanagedType.LPWStr)]string subKey);

        [DllImport("advapi32.dll", EntryPoint = "RegDeleteValue")]
        private static extern int RegDeleteValue(UIntPtr hKey, string lpValueName);
        [DllImport("advapi32.dll", CharSet = CharSet.Ansi, EntryPoint = "RegDeleteValueA", SetLastError = true)]
        private static extern int RegDeleteValueA(UIntPtr hKey, [MarshalAs(UnmanagedType.LPStr)]string lpValueName);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, EntryPoint = "RegCreateKeyExW", SetLastError = true)]
        private static extern int RegCreateKeyEx(ROOT_KEY hKey, [MarshalAs(UnmanagedType.LPWStr)]string subKey, int Reserved, [MarshalAs(UnmanagedType.LPWStr)]string lpClass, int dwOptions, int samDesired, ref SECURITY_ATTRIBUTES lpSecurityAttributes, ref UIntPtr phkResult, ref int lpdwDisposition);
        [DllImport("advapi32.dll", CharSet = CharSet.Ansi, EntryPoint = "RegCreateKeyExA", SetLastError = true)]
        private static extern int RegCreateKeyExA(ROOT_KEY hKey, [MarshalAs(UnmanagedType.LPStr)]string subKey, int Reserved, [MarshalAs(UnmanagedType.LPStr)]string lpClass, int dwOptions, int samDesired, ref SECURITY_ATTRIBUTES lpSecurityAttributes, ref UIntPtr phkResult, ref int lpdwDisposition);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, EntryPoint = "RegEnumKeyExW", SetLastError = true)]
        private static extern int RegEnumKeyEx(UIntPtr hKey, uint index, StringBuilder lpName, ref uint lpcbName, IntPtr reserved, IntPtr lpClass, IntPtr lpcbClass, out long lpftLastWriteTime);
        [DllImport("advapi32.dll", CharSet = CharSet.Ansi, EntryPoint = "RegEnumKeyExA", SetLastError = true)]
        private static extern int RegEnumKeyExA(UIntPtr hKey, uint index, [MarshalAs(UnmanagedType.LPStr)]string lpName, ref uint lpcbName, IntPtr reserved, IntPtr lpClass, IntPtr lpcbClass, out long lpftLastWriteTime);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, EntryPoint = "RegEnumValueW", SetLastError = true)]
        private static extern int RegEnumValue(UIntPtr hKey, uint dwIndex, StringBuilder lpValueName, ref uint lpcValueName, IntPtr lpReserved, IntPtr lpType, IntPtr lpData, IntPtr lpcbData);
        [DllImport("advapi32.dll", CharSet = CharSet.Ansi, EntryPoint = "RegEnumValueA", SetLastError = true)]
        private static extern int RegEnumValueA(UIntPtr hKey, uint dwIndex, [MarshalAs(UnmanagedType.LPStr)]string lpValueName, ref uint lpcValueName, IntPtr lpReserved, IntPtr lpType, IntPtr lpData, IntPtr lpcbData);
        
        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int RegSaveKey(UIntPtr hKey, [MarshalAs(UnmanagedType.LPStr)]string filename, IntPtr samDesired);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetFileAttributes(string lpFileName);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DeleteFile([MarshalAs(UnmanagedType.LPStr)]string lpFileName);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int RegRestoreKey(UIntPtr hKey, [MarshalAs(UnmanagedType.LPStr)]string filename, int dwFlags);

        [DllImport("shell32.dll")]
        private static extern IntPtr ShellExecute(IntPtr hwnd, string lpOperation, string lpFile, string lpParameters, string lpDirectory, SHOW_COMMANDS nShowCmd);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CreateProcessWithLogonW(String userName, String domain, String password, UInt32 logonFlags, String applicationName, String commandLine,
            UInt32 creationFlags, UInt32 environment, String currentDirectory, ref StartupInfo startupInfo, out ProcessInformation processInformation);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetExitCodeProcess(IntPtr process, ref UInt32 exitCode);

        [DllImport("Kernel32.dll", SetLastError = true)]
        private static extern UInt32 WaitForSingleObject(IntPtr handle, UInt32 milliseconds);

        [DllImport("Kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetStdHandle(IntPtr handle);
        #endregion

        #region Permissions
        /// <summary>
        /// Test registry access permissions
        /// </summary>
        /// <param name="RootKey">enum: root key</param>
        /// <param name="SubKey">string: named subkey</param>
        /// <returns>bool</returns>
        public bool AccessTest(ROOT_KEY RootKey, string SubKey)
        {
            UIntPtr pHKey = UIntPtr.Zero;
            int lDeposit = 0;
            SECURITY_ATTRIBUTES tSecAttrib = new SECURITY_ATTRIBUTES();

            try
            {
                // security attributes
                tSecAttrib.nLength = Marshal.SizeOf(tSecAttrib);
                tSecAttrib.lpSecurityDescriptor = 0;
                tSecAttrib.bInheritHandle = true;
                // open key
                return (RegCreateKeyEx(RootKey, SubKey, 0, "", 0, KEY_WRITE, ref tSecAttrib, ref pHKey, ref lDeposit) == ERROR_NONE);
            }
            finally
            {
                // close key
                if (pHKey != UIntPtr.Zero)
                    RegCloseKey(pHKey);
            }
        }

        /// <summary>
        /// Enable/disable debug level access
        /// </summary>
        /// <param name="Enable">bool: enable/disable</param>
        /// <returns>bool</returns>
        public bool EnableAccess(bool Enable)
        {
            IntPtr hToken = IntPtr.Zero;
            IntPtr hProcess = IntPtr.Zero;
            LUID tLuid = new LUID();
            TOKEN_PRIVILEGES NewState = new TOKEN_PRIVILEGES();
            uint uPriv = (uint)(ETOKEN_PRIVILEGES.TOKEN_ADJUST_PRIVILEGES | ETOKEN_PRIVILEGES.TOKEN_QUERY | ETOKEN_PRIVILEGES.TOKEN_QUERY_SOURCE);

            try
            {
                hProcess = OpenProcess(PROCESS_ALL_ACCESS, 0, GetCurrentProcessId());
                if (hProcess == IntPtr.Zero)
                    return false;
                if (OpenProcessToken(hProcess, uPriv, ref hToken))
                    return false;
                // Get the local unique id for the privilege.
                if (LookupPrivilegeValue(0, SE_DEBUG_NAME, ref tLuid))
                    return false;
                // Assign values to the TOKEN_PRIVILEGE structure.
                NewState.PrivilegeCount = 1;
                NewState.Privileges.pLuid = tLuid;
                NewState.Privileges.Attributes = (Enable ? SE_PRIVILEGE_ENABLED : 0);
                // Adjust the token privilege.
                return (AdjustTokenPrivileges(hToken, 0, ref NewState, Marshal.SizeOf(NewState), 0, 0));
            }
            finally
            {
                if (hToken != IntPtr.Zero)
                    CloseHandle(hToken);
                if (hProcess != IntPtr.Zero)
                    CloseHandle(hProcess);
            }
        }

        public bool ShellOpen(string file, SHOW_COMMANDS param)
        {
            if ((int)ShellExecute(IntPtr.Zero, "open", file, "", "", param) > VALID_FILE)
            {
                return true;
            }
            return false;
        }

        public void RunAs(string user, string password, string command)
        {
            string domain  = System.Environment.MachineName;
            StartupInfo startupInfo = new StartupInfo();
            startupInfo.reserved = null;
            startupInfo.flags &= Startf_UseStdHandles;
            startupInfo.stdOutput = (IntPtr)StdOutputHandle;
            startupInfo.stdError = (IntPtr)StdErrorHandle;

            UInt32 exitCode = 123456;
            ProcessInformation processInfo = new ProcessInformation();
            string currentDirectory = System.IO.Directory.GetCurrentDirectory();

            try
            {
                CreateProcessWithLogonW(user, domain, password, (UInt32)1, command, command, (UInt32)0, (UInt32)0, currentDirectory, ref startupInfo, out processInfo);
            } 
            catch(Exception e){ }

            WaitForSingleObject(processInfo.process, Infinite);
            GetExitCodeProcess(processInfo.process, ref exitCode);
            CloseHandle(processInfo.process);
            CloseHandle(processInfo.thread);
        }
        #endregion

        #region BigEndian
        /// <summary>
        /// Read BigEndian integer type
        /// </summary>
        /// <param name="RootKey">enum: root key</param>
        /// <param name="SubKey">string: named subkey</param>
        /// <param name="Value">string: named value</param>
        /// <returns>int: value / -1 on fail</returns>
        public int ReadBigEndian(ROOT_KEY RootKey, string SubKey, string Value)
        {
            UIntPtr hKey = UIntPtr.Zero;
            uint pvSize = 4;
            uint pdwType = (uint)VALUE_TYPE.REG_DWORD_BIG_ENDIAN;
            int pvData = 0;

            try
            {
                // open root key
                if (RegOpenKeyEx(RootKey, SubKey, 0, KEY_READ, ref hKey) == ERROR_NONE)
                {
                    if (RegQueryValueEx(hKey, Value, 0, ref pdwType, ref pvData, ref pvSize) == ERROR_NONE)
                        // return int
                        return pvData;
                }
                return -1;
            }
            finally
            {
                if (hKey != UIntPtr.Zero)
                    RegCloseKey(hKey);
            }
        }

        /// <summary>
        /// Write a BigEndian integer
        /// </summary>
        /// <param name="RootKey">enum: root key</param>
        /// <param name="SubKey">string: named subkey</param>
        /// <param name="Value">string: named value</param>
        /// <param name="Data">int: data</param>
        /// <returns>bool</returns>
        public bool WriteBigEndian(ROOT_KEY RootKey, string SubKey, string Value, int Data)
        {
            UIntPtr hKey = UIntPtr.Zero;
            uint pdwType = (uint)VALUE_TYPE.REG_DWORD_BIG_ENDIAN;

            try
            {
                // open root key
                if (RegOpenKeyEx(RootKey, SubKey, 0, KEY_WRITE, ref hKey) == ERROR_NONE)
                {
                    if (RegSetValueEx(hKey, Value, 0, pdwType, ref Data, 4) == ERROR_NONE)
                        // return status
                        return true;
                }
                return false;
            }
            finally
            {
                if (hKey != UIntPtr.Zero)
                    RegCloseKey(hKey);
            }
        }
        #endregion

        #region Binary
        /// <summary>
        /// Read Binary data
        /// </summary>
        /// <param name="RootKey">enum: root key</param>
        /// <param name="SubKey">string: named subkey</param>
        /// <param name="Value">string: named value</param>
        /// <returns>string: value / empty on fail</returns>
        public string ReadBinary(ROOT_KEY RootKey, string SubKey, string Value)
        {
            UIntPtr hKey = UIntPtr.Zero;
            uint pvSize = 1024;
            uint pdwType = (uint)VALUE_TYPE.REG_BINARY;
            byte[] bBuffer = new byte[1024];
            string sRet = String.Empty;

            try
            {
                // open root key
                if (RegOpenKeyEx(RootKey, SubKey, 0, KEY_READ, ref hKey) == ERROR_NONE)
                {
                    if (RegQueryValueEx(hKey, Value, 0, ref pdwType, ref bBuffer[0], ref pvSize) == ERROR_NONE)
                        // return hex string
                        for (int i = 0; i < (pvSize); i++)
                        {
                            sRet += bBuffer[i].ToString("X");
                        }
                }
                return sRet;
            }
            finally
            {
                if (hKey != UIntPtr.Zero)
                    RegCloseKey(hKey);
            }
        }

        /// <summary>
        /// Write a Binary value
        /// </summary>
        /// <param name="RootKey">enum: root key</param>
        /// <param name="SubKey">string: named subkey</param>
        /// <param name="Value">string: named value</param>
        /// <param name="Data">byte array: data</param>
        /// <returns>bool</returns>
        public bool WriteBinary(ROOT_KEY RootKey, string SubKey, string Value, ref byte[] Data)
        {
            UIntPtr hKey = UIntPtr.Zero;
            uint pdwType = (uint)VALUE_TYPE.REG_BINARY;

            try
            {
                if (RegOpenKeyEx(RootKey, SubKey, 0, KEY_WRITE, ref hKey) == ERROR_NONE)
                {
                    if (RegSetValueEx(hKey, Value, 0, pdwType, ref Data[0], (Data.GetUpperBound(0) + 1)) == ERROR_NONE)
                        return true;
                }
                return false;
            }
            finally
            {
                if (hKey != UIntPtr.Zero)
                    RegCloseKey(hKey);
            }
        }
        #endregion

        #region DWord
        /// <summary>
        /// Read Dword integer type
        /// </summary>
        /// <param name="RootKey">enum: root key</param>
        /// <param name="SubKey">string: named subkey</param>
        /// <param name="Value">string: named value</param>
        /// <returns>int: value / -1 on fail</returns>
        public int ReadDword(ROOT_KEY RootKey, string SubKey, string Value)
        {
            UIntPtr hKey = UIntPtr.Zero;
            uint pvSize = 4;
            uint pdwType = (uint)VALUE_TYPE.REG_DWORD;
            int pvData = 0;

            try
            {
                if (RegOpenKeyEx(RootKey, SubKey, 0, KEY_READ, ref hKey) == ERROR_NONE)
                {
                    if (RegQueryValueEx(hKey, Value, 0, ref pdwType, ref pvData, ref pvSize) == ERROR_NONE)
                        return pvData;
                }
                return -1;
            }
            finally
            {
                if (hKey != UIntPtr.Zero)
                    RegCloseKey(hKey);
            }
        }

        /// <summary>
        /// Write a Dword value
        /// </summary>
        /// <param name="RootKey">enum: root key</param>
        /// <param name="SubKey">string: named subkey</param>
        /// <param name="Value">string: named value</param>
        /// <param name="Data">int: data</param>
        /// <returns>bool</returns>
        public bool WriteDword(ROOT_KEY RootKey, string SubKey, string Value, int Data)
        {
            UIntPtr hKey = UIntPtr.Zero;
            uint pdwType = (uint)VALUE_TYPE.REG_DWORD;

            try
            {
                if (RegOpenKeyEx(RootKey, SubKey, 0, KEY_WRITE, ref hKey) == ERROR_NONE)
                {
                    if (RegSetValueEx(hKey, Value, 0, pdwType, ref Data, 4) == ERROR_NONE)
                        return true;
                }
                return false;
            }
            finally
            {
                if (hKey != UIntPtr.Zero)
                    RegCloseKey(hKey);
            }
        }
        #endregion

        #region Expanded
        /// <summary>
        /// Read Expanded String
        /// </summary>
        /// <param name="RootKey">enum: root key</param>
        /// <param name="SubKey">string: named subkey</param>
        /// <param name="Value">string: named value</param>
        /// <returns>string: value / empty on fail</returns>
        public string ReadExpand(ROOT_KEY RootKey, string SubKey, string Value)
        {
            UIntPtr hKey = UIntPtr.Zero;
            System.Text.StringBuilder pvData = new System.Text.StringBuilder(0);
            uint pvSize = 0;
            uint pdwType = (uint)VALUE_TYPE.REG_EXPAND_SZ;

            try
            {
                if (RegOpenKeyEx(RootKey, SubKey, 0, KEY_READ, ref hKey) == ERROR_NONE)
                {
                    if (RegQueryValueEx(hKey, Value, 0, out pdwType, null, ref pvSize) == ERROR_NONE)
                    {
                        pvData = new System.Text.StringBuilder((int)(pvSize / 2));
                        RegQueryValueEx(hKey, Value, 0, out pdwType, pvData, ref pvSize);
                    }
                }
                return pvData.ToString();
            }
            finally
            {
                if (hKey != UIntPtr.Zero)
                    RegCloseKey(hKey);
            }
        }

        /// <summary>
        /// Write a Expanded String value
        /// </summary>
        /// <param name="RootKey">enum: root key</param>
        /// <param name="SubKey">string: named subkey</param>
        /// <param name="Value">string: named value</param>
        /// <param name="Data">string: data</param>
        /// <returns>bool</returns>
        public bool WriteExpand(ROOT_KEY RootKey, string SubKey, string Value, string Data)
        {
            UIntPtr hKey = UIntPtr.Zero;
            uint pdwType = (uint)VALUE_TYPE.REG_EXPAND_SZ;
            IntPtr pStr = IntPtr.Zero;

            try
            {
                if (RegOpenKeyEx(RootKey, SubKey, 0, KEY_WRITE, ref hKey) == ERROR_NONE)
                {
                    int Size = (Data.Length + 1) * Marshal.SystemDefaultCharSize;
                    pStr = Marshal.StringToHGlobalAuto(Data);
                    if (RegSetValueEx(hKey, Value, 0, pdwType, pStr, Size) == ERROR_NONE)
                        return true;
                }
                return false;
            }
            finally
            {
                if (hKey != UIntPtr.Zero)
                    RegCloseKey(hKey);
                if (pStr != IntPtr.Zero)
                    Marshal.FreeHGlobal(pStr);
            }
        }
        #endregion

        #region Link
        /// <summary>
        /// Read Link data type
        /// </summary>
        /// <param name="RootKey">enum: root key</param>
        /// <param name="SubKey">string: named subkey</param>
        /// <param name="Value">string: named value</param>
        /// <returns>string: value / empty on fail</returns>
        public string ReadLink(ROOT_KEY RootKey, string SubKey, string Value)
        {
            UIntPtr hKey = UIntPtr.Zero;
            uint pvSize = 1024;
            uint pdwType = (uint)VALUE_TYPE.REG_LINK;
            byte[] bBuffer = new byte[1024];
            string sRet = String.Empty;

            try
            {
                if (RegOpenKeyEx(RootKey, SubKey, 0, KEY_READ, ref hKey) == ERROR_NONE)
                {
                    if (RegQueryValueEx(hKey, Value, 0, ref pdwType, ref bBuffer[0], ref pvSize) == ERROR_NONE)
                        for (int i = 0; i < (pvSize); i++)
                        {
                            sRet += bBuffer[i].ToString();
                        }
                }
                return sRet;
            }
            finally
            {
                if (hKey != UIntPtr.Zero)
                    RegCloseKey(hKey);
            }
        }

        /// <summary>
        /// Write a Link value
        /// </summary>
        /// <param name="RootKey">enum: root key</param>
        /// <param name="SubKey">string: named subkey</param>
        /// <param name="Value">string: named value</param>
        /// <param name="Data">byte array: data</param>
        /// <returns>bool</returns>
        public bool WriteLink(ROOT_KEY RootKey, string SubKey, string Value, ref byte[] Data)
        {
            UIntPtr hKey = UIntPtr.Zero;
            uint pdwType = (uint)VALUE_TYPE.REG_LINK;

            try
            {
                if (RegOpenKeyEx(RootKey, SubKey, 0, KEY_WRITE, ref hKey) == ERROR_NONE)
                {
                    if (RegSetValueEx(hKey, Value, 0, pdwType, ref Data[0], (Data.GetUpperBound(0) + 1)) == ERROR_NONE)
                        return true;
                }
                return false;
            }
            finally
            {
                if (hKey != UIntPtr.Zero)
                    RegCloseKey(hKey);
            }
        }
        #endregion

        #region Multi
        /// <summary>
        /// Read Multi String data type
        /// </summary>
        /// <param name="RootKey">enum: root key</param>
        /// <param name="SubKey">string: named subkey</param>
        /// <param name="Value">string: named value</param>
        /// <returns>string: value / empty on fail</returns>
        public string ReadMulti(ROOT_KEY RootKey, string SubKey, string Value)
        {
            UIntPtr hKey = UIntPtr.Zero;
            System.Text.StringBuilder pvData = new System.Text.StringBuilder(0);
            uint pvSize = 0;
            uint pdwType = (uint)VALUE_TYPE.REG_MULTI_SZ;

            try
            {
                if (RegOpenKeyEx(RootKey, SubKey, 0, KEY_READ, ref hKey) == ERROR_NONE)
                {
                    if (RegQueryValueEx(hKey, Value, 0, out pdwType, null, ref pvSize) == ERROR_NONE)
                    {
                        pvData = new System.Text.StringBuilder((int)(pvSize / 2));
                        RegQueryValueEx(hKey, Value, 0, out pdwType, pvData, ref pvSize);
                    }
                }
                return pvData.ToString();
            }
            finally
            {
                if (hKey != UIntPtr.Zero)
                    RegCloseKey(hKey);
            }
        }

        /// <summary>
        /// Write a Multi String value
        /// </summary>
        /// <param name="RootKey">enum: root key</param>
        /// <param name="SubKey">string: named subkey</param>
        /// <param name="Value">string: named value</param>
        /// <param name="Data">string: data</param>
        /// <returns>bool</returns>
        public bool WriteMulti(ROOT_KEY RootKey, string SubKey, string Value, string Data)
        {
            UIntPtr hKey = UIntPtr.Zero;
            uint pdwType = (uint)VALUE_TYPE.REG_MULTI_SZ;
            IntPtr pStr = IntPtr.Zero;

            try
            {
                if (RegOpenKeyEx(RootKey, SubKey, 0, KEY_WRITE, ref hKey) == ERROR_NONE)
                {

                    int Size = (Data.Length + 1) * Marshal.SystemDefaultCharSize;
                    pStr = Marshal.StringToHGlobalAuto(Data);
                    if (RegSetValueEx(hKey, Value, 0, pdwType, pStr, Size) == ERROR_NONE)
                        return true;
                }
                return false;
            }
            finally
            {
                if (hKey != UIntPtr.Zero)
                    RegCloseKey(hKey);
                if (pStr != IntPtr.Zero)
                    Marshal.FreeHGlobal(pStr);
            }
        }
        #endregion

        #region Qword
        /// <summary>
        /// Read Qword large integer type
        /// </summary>
        /// <param name="RootKey">enum: root key</param>
        /// <param name="SubKey">string: named subkey</param>
        /// <param name="Value">string: named value</param>
        /// <returns>long: value / -1 on fail</returns>
        public long ReadQword(ROOT_KEY RootKey, string SubKey, string Value)
        {
            UIntPtr hKey = UIntPtr.Zero;
            uint pvSize = 8;
            uint pdwType = (uint)VALUE_TYPE.REG_QWORD_LITTLE_ENDIAN;
            long pvData = 0;

            try
            {
                if (RegOpenKeyEx(RootKey, SubKey, 0, KEY_READ, ref hKey) == ERROR_NONE)
                {
                    if (RegQueryValueEx(hKey, Value, 0, ref pdwType, ref pvData, ref pvSize) == ERROR_NONE)
                        return pvData;
                }
                return -1;
            }
            finally
            {
                if (hKey != UIntPtr.Zero)
                    RegCloseKey(hKey);
            }
        }

        /// <summary>
        /// Write a Qword value
        /// </summary>
        /// <param name="RootKey">enum: root key</param>
        /// <param name="SubKey">string: named subkey</param>
        /// <param name="Value">string: named value</param>
        /// <param name="Data">long: data</param>
        /// <returns>bool</returns>
        public bool WriteDword(ROOT_KEY RootKey, string SubKey, string Value, long Data)
        {
            UIntPtr hKey = UIntPtr.Zero;
            uint pdwType = (uint)VALUE_TYPE.REG_QWORD_LITTLE_ENDIAN;

            try
            {
                if (RegOpenKeyEx(RootKey, SubKey, 0, KEY_WRITE, ref hKey) == ERROR_NONE)
                {
                    if (RegSetValueEx(hKey, Value, 0, pdwType, ref Data, 8) == ERROR_NONE)
                        return true;
                }
                return false;
            }
            finally
            {
                if (hKey != UIntPtr.Zero)
                    RegCloseKey(hKey);
            }
        }
        #endregion

        #region Resource Descriptor
        /// <summary>
        /// Read Resource Descriptor data type
        /// </summary>
        /// <param name="RootKey">enum: root key</param>
        /// <param name="SubKey">string: named subkey</param>
        /// <param name="Value">string: named value</param>
        /// <returns>string: value / empty on fail</returns>
        public string ReadResourceDescriptor(ROOT_KEY RootKey, string SubKey, string Value)
        {
            UIntPtr hKey = UIntPtr.Zero;
            uint pvSize = 1024;
            uint pdwType = (uint)VALUE_TYPE.REG_FULL_RESOURCE_DESCRIPTOR;
            byte[] bBuffer = new byte[1024];
            string sRet = String.Empty;

            try
            {
                if (RegOpenKeyEx(RootKey, SubKey, 0, KEY_READ, ref hKey) == ERROR_NONE)
                {
                    if (RegQueryValueEx(hKey, Value, 0, ref pdwType, ref bBuffer[0], ref pvSize) == ERROR_NONE)
                        for (int i = 0; i < (pvSize); i++)
                        {
                            sRet += bBuffer[i].ToString();
                        }
                }
                return sRet;
            }
            finally
            {
                if (hKey != UIntPtr.Zero)
                    RegCloseKey(hKey);
            }
        }

        /// <summary>
        /// Write a Resource Descriptor value
        /// </summary>
        /// <param name="RootKey">enum: root key</param>
        /// <param name="SubKey">string: named subkey</param>
        /// <param name="Value">string: named value</param>
        /// <param name="Data">byte array: data</param>
        /// <returns>bool</returns>
        public bool WriteResourceDescriptor(ROOT_KEY RootKey, string SubKey, string Value, ref byte[] Data)
        {
            UIntPtr hKey = UIntPtr.Zero;
            uint pdwType = (uint)VALUE_TYPE.REG_FULL_RESOURCE_DESCRIPTOR;

            try
            {
                if (RegOpenKeyEx(RootKey, SubKey, 0, KEY_WRITE, ref hKey) == ERROR_NONE)
                {
                    if (RegSetValueEx(hKey, Value, 0, pdwType, ref Data[0], (Data.GetUpperBound(0) + 1)) == ERROR_NONE)
                        return true;
                }
                return false;
            }
            finally
            {
                if (hKey != UIntPtr.Zero)
                    RegCloseKey(hKey);
            }
        }
        #endregion

        #region Resource List
        /// <summary>
        /// Read Resource List data type
        /// </summary>
        /// <param name="RootKey">enum: root key</param>
        /// <param name="SubKey">string: named subkey</param>
        /// <param name="Value">string: named value</param>
        /// <returns>string: value / empty on fail</returns>
        public string ReadResourceList(ROOT_KEY RootKey, string SubKey, string Value)
        {
            UIntPtr hKey = UIntPtr.Zero;
            uint pvSize = 1024;
            uint pdwType = (uint)VALUE_TYPE.REG_RESOURCE_LIST;
            byte[] bBuffer = new byte[1024];
            string sRet = String.Empty;

            try
            {
                if (RegOpenKeyEx(RootKey, SubKey, 0, KEY_READ, ref hKey) == ERROR_NONE)
                {
                    if (RegQueryValueEx(hKey, Value, 0, ref pdwType, ref bBuffer[0], ref pvSize) == ERROR_NONE)
                        for (int i = 0; i < (pvSize); i++)
                        {
                            sRet += bBuffer[i].ToString();
                        }
                }
                return sRet;
            }
            finally
            {
                if (hKey != UIntPtr.Zero)
                    RegCloseKey(hKey);
            }
        }

        /// <summary>
        /// Write a Resource List value
        /// </summary>
        /// <param name="RootKey">enum: root key</param>
        /// <param name="SubKey">string: named subkey</param>
        /// <param name="Value">string: named value</param>
        /// <param name="Data">byte array: data</param>
        /// <returns>bool</returns>
        public bool WriteResourceList(ROOT_KEY RootKey, string SubKey, string Value, ref byte[] Data)
        {
            UIntPtr hKey = UIntPtr.Zero;
            uint pdwType = (uint)VALUE_TYPE.REG_RESOURCE_LIST;

            try
            {
                if (RegOpenKeyEx(RootKey, SubKey, 0, KEY_WRITE, ref hKey) == ERROR_NONE)
                {
                    if (RegSetValueEx(hKey, Value, 0, pdwType, ref Data[0], (Data.GetUpperBound(0) + 1)) == ERROR_NONE)
                        return true;
                }
                return false;
            }
            finally
            {
                if (hKey != UIntPtr.Zero)
                    RegCloseKey(hKey);
            }
        }
        #endregion

        #region Resource Requirements
        /// <summary>
        /// Read Requirements data type
        /// </summary>
        /// <param name="RootKey">enum: root key</param>
        /// <param name="SubKey">string: named subkey</param>
        /// <param name="Value">string: named value</param>
        /// <returns>string: value / empty on fail</returns>
        public string ReadResourceRequirements(ROOT_KEY RootKey, string SubKey, string Value)
        {
            UIntPtr hKey = UIntPtr.Zero;
            uint pvSize = 1024;
            uint pdwType = (uint)VALUE_TYPE.REG_RESOURCE_REQUIREMENTS_LIST;
            byte[] bBuffer = new byte[1024];
            string sRet = String.Empty;

            try
            {
                if (RegOpenKeyEx(RootKey, SubKey, 0, KEY_READ, ref hKey) == ERROR_NONE)
                {
                    if (RegQueryValueEx(hKey, Value, 0, ref pdwType, ref bBuffer[0], ref pvSize) == ERROR_NONE)
                        for (int i = 0; i < (pvSize); i++)
                        {
                            sRet += bBuffer[i].ToString();
                        }
                }
                return sRet;
            }
            finally
            {
                if (hKey != UIntPtr.Zero)
                    RegCloseKey(hKey);
            }
        }

        /// <summary>
        /// Write a Resource Requirements value
        /// </summary>
        /// <param name="RootKey">enum: root key</param>
        /// <param name="SubKey">string: named subkey</param>
        /// <param name="Value">string: named value</param>
        /// <param name="Data">byte array: data</param>
        /// <returns>bool</returns>
        public bool WriteResourceRequirements(ROOT_KEY RootKey, string SubKey, string Value, ref byte[] Data)
        {
            UIntPtr hKey = UIntPtr.Zero;
            uint pdwType = (uint)VALUE_TYPE.REG_RESOURCE_REQUIREMENTS_LIST;

            try
            {
                if (RegOpenKeyEx(RootKey, SubKey, 0, KEY_WRITE, ref hKey) == ERROR_NONE)
                {
                    if (RegSetValueEx(hKey, Value, 0, pdwType, ref Data[0], (Data.GetUpperBound(0) + 1)) == ERROR_NONE)
                        return true;
                }
                return false;
            }
            finally
            {
                if (hKey != UIntPtr.Zero)
                    RegCloseKey(hKey);
            }
        }
        #endregion

        #region String
        /// <summary>
        /// Read String
        /// </summary>
        /// <param name="RootKey">enum: root key</param>
        /// <param name="SubKey">string: named subkey</param>
        /// <param name="Value">string: named value</param>
        /// <returns>string: value / empty on fail</returns>
        public string ReadString(ROOT_KEY RootKey, string SubKey, string Value)
        {
            UIntPtr hKey = UIntPtr.Zero;
            System.Text.StringBuilder pvData = new System.Text.StringBuilder(0);
            uint pvSize = 0;
            uint pdwType = (uint)VALUE_TYPE.REG_SZ;

            try
            {
                if (RegOpenKeyEx(RootKey, SubKey, 0, KEY_READ, ref hKey) == ERROR_NONE)
                {
                    if (RegQueryValueEx(hKey, Value, 0, out pdwType, null, ref pvSize) == ERROR_NONE)
                    {
                        pvData = new System.Text.StringBuilder((int)(pvSize / 2));
                        RegQueryValueEx(hKey, Value, 0, out pdwType, pvData, ref pvSize);
                    }
                }
                return pvData.ToString();
            }
            finally
            {
                if (hKey != UIntPtr.Zero)
                    RegCloseKey(hKey);
            }
        }

        /// <summary>
        /// Write a String value
        /// </summary>
        /// <param name="RootKey">enum: root key</param>
        /// <param name="SubKey">string: named subkey</param>
        /// <param name="Value">string: named value</param>
        /// <param name="Data">string: data</param>
        /// <returns>bool</returns>
        public bool WriteString(ROOT_KEY RootKey, string SubKey, string Value, string Data)
        {
            UIntPtr hKey = UIntPtr.Zero;
            uint pdwType = (uint)VALUE_TYPE.REG_SZ;
            IntPtr pStr = IntPtr.Zero;
            try
            {
                if (RegOpenKeyEx(RootKey, SubKey, 0, KEY_WRITE, ref hKey) == ERROR_NONE)
                {

                    int Size = (Data.Length + 1) * Marshal.SystemDefaultCharSize;
                    pStr = Marshal.StringToHGlobalAuto(Data);
                    if (RegSetValueEx(hKey, Value, 0, pdwType, pStr, Size) == ERROR_NONE)
                        return true;
                }
                return false;
            }
            finally
            {
                if (hKey != UIntPtr.Zero)
                    RegCloseKey(hKey);
                if (pStr != IntPtr.Zero)
                    Marshal.FreeHGlobal(pStr);
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Test key presence
        /// </summary>
        /// <param name="RootKey">enum: root key</param>
        /// <param name="SubKey">string: named subkey</param>
        /// <returns>bool</returns>
        public bool KeyExists(ROOT_KEY RootKey, string SubKey)
        {
            UIntPtr hKey = UIntPtr.Zero;
            try
            {
                return (RegOpenKeyEx(RootKey, SubKey, 0, KEY_QUERY_VALUE, ref hKey) == ERROR_NONE);
            }
            finally
            {
                if (hKey != UIntPtr.Zero)
                    RegCloseKey(hKey);
            }
        }

        /// <summary>
        /// Test key for values and subkeys
        /// </summary>
        /// <param name="RootKey">enum: root key</param>
        /// <param name="SubKey">string: named subkey</param>
        /// <returns>bool</returns>
        public bool KeyIsEmpty(ROOT_KEY RootKey, string SubKey)
        {
            
            if ((!KeyHasValues(RootKey, SubKey)) && (!KeyHasSubKeys(RootKey, SubKey)))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Test key for values
        /// </summary>
        /// <param name="RootKey">enum: root key</param>
        /// <param name="SubKey">string: named subkey</param>
        /// <returns>bool</returns>
        public bool KeyHasValues(ROOT_KEY RootKey, string SubKey)
        {
            ArrayList al = EnumValues(RootKey, SubKey);
            return (al.Count > 0);
        }

        /// <summary>
        /// Test key for subkeys
        /// </summary>
        /// <param name="RootKey">enum: root key</param>
        /// <param name="SubKey">string: named subkey</param>
        /// <returns>bool</returns>
        public bool KeyHasSubKeys(ROOT_KEY RootKey, string SubKey)
        {
            ArrayList al = EnumKeys(RootKey, SubKey);
            return (al.Count > 0);
        }

        /// <summary>
        /// Test value presence
        /// </summary>
        /// <param name="RootKey">enum: root key</param>
        /// <param name="SubKey">string: named subkey</param>
        /// <param name="Value">string: named value</param>
        /// <returns>bool</returns>
        public bool ValueExists(ROOT_KEY RootKey, string SubKey, string Value)
        {
            UIntPtr hKey = UIntPtr.Zero;
            uint pvSize = 0;
            uint pdwType = (uint)VALUE_TYPE.REG_NONE;

            try
            {
                if (RegOpenKeyEx(RootKey, SubKey, 0, KEY_QUERY_VALUE, ref hKey) != ERROR_NONE)
                    return false;
                return (RegQueryValueEx(hKey, Value, 0, out pdwType, null, ref pvSize) == ERROR_NONE);
            }
            finally
            {
                if (hKey != UIntPtr.Zero)
                    RegCloseKey(hKey);
            }
        }

        /// <summary>
        /// Create a named key
        /// </summary>
        /// <param name="RootKey">enum: root key</param>
        /// <param name="SubKey">string: named subkey</param>
        /// <returns>bool</returns>
        public bool CreateKey(ROOT_KEY RootKey, string SubKey)
        {
            UIntPtr hKey = UIntPtr.Zero;
            try
            {
                return (RegCreateKey(RootKey, SubKey, ref hKey) == ERROR_NONE);
            }
            finally
            {
                if (hKey != UIntPtr.Zero)
                    RegCloseKey(hKey);
            }
        }

        /// <summary>
        /// Delete a key
        /// </summary>
        /// <param name="RootKey">enum: root key</param>
        /// <param name="SubKey">string: named subkey</param>
        /// <returns>bool</returns>
        public bool DeleteKey(ROOT_KEY RootKey, string SubKey)
        {
            try
            {
                //int I = (RegDeleteKey(RootKey, SubKey));
                return (RegDeleteKeyA(RootKey, SubKey) == ERROR_NONE);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Delete a value
        /// </summary>
        /// <param name="RootKey">enum: root key</param>
        /// <param name="SubKey">string: named subkey</param>
        /// <param name="Value">string: named value</param>
        /// <returns>bool</returns>
        public bool DeleteValue(ROOT_KEY RootKey, string SubKey, string Value)
        {
            UIntPtr hKey = UIntPtr.Zero;
            try
            {
                if (RegOpenKeyEx(RootKey, SubKey, 0, KEY_SET_VALUE, ref hKey) != ERROR_NONE)
                {
                    return false;
                }
                int ret = RegDeleteValue(hKey, Value);
                return (ret == ERROR_NONE);
            }
            finally
            {
                if (hKey != UIntPtr.Zero)
                    RegCloseKey(hKey);
            }
        }

        /// <summary>
        /// Enumerate and collect keys
        /// </summary>
        /// <param name="RootKey">enum: root key</param>
        /// <param name="SubKey">string: named subkey</param>
        /// <returns>ArrayList</returns>
        public ArrayList EnumKeys(ROOT_KEY RootKey, string SubKey)
        {
            uint keyLen = 255;
            uint index = 0;
            int ret = 0;
            long lastWrite = 0;
            UIntPtr hKey = UIntPtr.Zero;
            StringBuilder keyName = new StringBuilder(255);
            ArrayList keyList = new ArrayList();

            try
            {
                if (RegOpenKeyEx(RootKey, SubKey, 0, KEY_ENUMERATE_SUB_KEYS, ref hKey) != ERROR_NONE)
                    return keyList;
                do
                {
                    keyLen = 255;
                    ret = RegEnumKeyEx(hKey, index, keyName, ref keyLen, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, out lastWrite);
                    if (ret == ERROR_NONE)
                    {
                        keyList.Add(keyName.ToString());
                    }
                    index += 1;
                }
                while (ret == 0);
                return keyList;
            }
            finally
            {
                if (hKey != UIntPtr.Zero)
                    RegCloseKey(hKey);
            }
        }

        /// <summary>
        /// Enumerate and collect values
        /// </summary>
        /// <param name="RootKey">enum: root key</param>
        /// <param name="SubKey">string: named subkey</param>
        /// <returns>ArrayList</returns>
        public ArrayList EnumValues(ROOT_KEY RootKey, string SubKey)
        {
            uint valLen = 255;
            uint index = 0;
            int ret = 0;
            UIntPtr hKey = UIntPtr.Zero;
            StringBuilder valName = new StringBuilder(0);
            ArrayList valList = new ArrayList();
            try
            {
                if (RegOpenKeyEx(RootKey, SubKey, 0, KEY_QUERY_VALUE, ref hKey) != ERROR_NONE)
                    return valList;
                do
                {
                    valLen = 255;
                    valName = new StringBuilder(255);
                    ret = RegEnumValue(hKey, index, valName, ref valLen, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
                    if (ret == ERROR_NONE)
                    {
                        valList.Add(valName.ToString());
                    }
                    index += 1;
                }
                while (ret == 0);
                return valList;
            }
            finally
            {
                if (hKey != UIntPtr.Zero)
                    RegCloseKey(hKey);
            }
        }

        /// <summary>
        /// Rot13 text encryption
        /// </summary>
        /// <param name="Text">text to encrypt/decrypt</param>
        /// <returns>String</returns>
        public string Rot13(string Text)
        {
            char[] ac = Text.ToCharArray();

            for (int i = 0; i < ac.Length; i++)
            {
                int n = (int)ac[i];

                if (n >= 'a' && n <= 'z')
                {
                    if (n > 'm')
                    {
                        n -= 13;
                    }
                    else
                    {
                        n += 13;
                    }
                }
                else if (n >= 'A' && n <= 'Z')
                {
                    if (n > 'M')
                    {
                        n -= 13;
                    }
                    else
                    {
                        n += 13;
                    }
                }
                ac[i] = (char)n;
            }
            return new string(ac);

        }

        /// <summary>
        /// Set the Registry Jump key
        /// </summary>
        /// <param name="SubKey">jump key</param>
        /// <returns>Bool</returns>
        public bool SetJumpKey(string SubKey)
        {
            int ret = 0;
            UIntPtr hKey = UIntPtr.Zero;
            string sKey = "";

            sKey = @"Software\Microsoft\Windows\CurrentVersion\Applets\Regedit";
            ret = (RegCreateKey(ROOT_KEY.HKEY_CURRENT_USER, sKey, ref hKey));
            if (ret == ERROR_NONE)
            {
                RegCloseKey(hKey);
                ret = (RegOpenKeyEx(ROOT_KEY.HKEY_CURRENT_USER, sKey, 0, KEY_WRITE, ref hKey));
                if (ret == ERROR_NONE)
                {
                    if (WriteString(ROOT_KEY.HKEY_CURRENT_USER, sKey, "LastKey", SubKey))
                    RegCloseKey(hKey);
                    return true;
                }
            }
            RegCloseKey(hKey);
            return false;
        }

        private bool FileExists(string File)
        {
            return (GetFileAttributes(File) != INVALID_HANDLE_VALUE);
        }

        /// <summary>
        /// Create a copy of a Key
        /// </summary>
        /// <param name="RootKey">enum: root key</param>
        /// <param name="SubKey">string: named subkey</param>
        /// <param name="FileName">string: file name and path</param>
        /// <returns>Bool</returns>
        public bool SaveKey(ROOT_KEY RootKey, string SubKey, string FileName)
        {
            // Save registry key to Snapshot
            UIntPtr hKey = UIntPtr.Zero;

            try
            {
                if (EnableAccess(true))
                {
                    if (RegOpenKeyEx(RootKey, SubKey, 0, KEY_ALL_ACCESS, ref hKey) == ERROR_NONE)
                    {
                        if (FileExists(FileName))
                        {
                            DeleteFile(FileName);
                        }
                        return (RegSaveKey(hKey, FileName, IntPtr.Zero) == 0);
                    }
                }
                return false;
            }
            finally
            {
                if (hKey != UIntPtr.Zero)
                    RegCloseKey(hKey);
            }
        }

        /// <summary>
        /// Restore a registry Key
        /// </summary>
        /// <param name="RootKey">enum: root key</param>
        /// <param name="SubKey">string: named subkey</param>
        /// <param name="FileName">string: file name and path</param>
        /// <returns>Bool</returns>
        public bool RestoreKey(ROOT_KEY RootKey, string SubKey, string FileName)
        {
            // restore registry key from Snapshot
            UIntPtr hKey = UIntPtr.Zero;

            try
            {
                if (EnableAccess(true))
                {
                    if (RegOpenKeyEx(RootKey, SubKey, 0, KEY_ALL_ACCESS, ref hKey) == ERROR_NONE)
                    {
                        if (FileExists(FileName))
                        {
                            return (RegRestoreKey(hKey, FileName, REG_FORCE_RESTORE) == 0);
                        }
                    }
                }
                return false;
            }
            finally
            {
                if (hKey != UIntPtr.Zero)
                    RegCloseKey(hKey);
            }
        }
        #endregion
    }
}
