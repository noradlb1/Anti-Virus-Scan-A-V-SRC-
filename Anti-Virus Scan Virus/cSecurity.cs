#region Directives
using System;
using System.Text;
using System.Runtime.InteropServices;
using System.Security.Permissions;
#endregion

namespace VTRegScan
{
    [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
    public class cSecurity
    {
        #region Constants
        private const int ERROR_SUCCESS = 0x0;
        private const string SE_TAKE_OWNERSHIP_NAME = "SeTakeOwnershipPrivilege";
        // well known rids
        private const int SECURITY_WORLD_SID_AUTHORITY = 0x1;
        private const int SECURITY_NT_AUTHORITY = 0x5;
        private const int SECURITY_BUILTIN_DOMAIN_RID = 0x00000020;
        private const int DOMAIN_ALIAS_RID_ADMINS = 0x00000220;
        private const int DOMAIN_ALIAS_RID_USERS = 0x221;
        private const int SECURITY_LOCAL_SYSTEM_RID = 0x12;
        private const int SECURITY_WORLD_RID = 0x0;
        private const int DOMAIN_USER_RID_ADMIN = 0x1F4;
        private const int DOMAIN_USER_RID_GUEST = 0x1F5;
        private const int DOMAIN_GROUP_RID_ADMINS = 0x200;
        // token
        private const int SE_PRIVILEGE_ENABLED = 0x2;
        private const int PROCESS_ALL_ACCESS = (STANDARD_RIGHTS_REQUIRED | SYNCHRONIZE | 0xFFF);
        private const int LMEM_FIXED = 0x0;
        private const int LMEM_ZEROINIT = 0x40;
        private const int LMEM_INITIALIZED = (LMEM_FIXED & LMEM_ZEROINIT);
        // access rights
        private const int WRITE_OWNER = 0x80000;
        private const int STANDARD_RIGHTS_ALL = 0x001F0000;
        private const int SPECIFIC_RIGHTS_ALL = 0x0000FFFF;
        private const int READ_CONTROL = 0x20000;
        private const int DELETE = 0x10000;
        private const int WRITE_DAC = 0x40000;
        private const int STANDARD_RIGHTS_EXECUTE = READ_CONTROL;
        private const int ACCESS_SYSTEM_SECURITY = 0x1000000;
        private const int MAXIMUM_ALLOWED = 0x2000000;
        // descriptor flags
        private const int DACL_SECURITY_INFORMATION = 0x4;
        private const int SECURITY_DESCRIPTOR_REVISION = 1;
        private const int SECURITY_DESCRIPTOR_MIN_LENGTH = 20;
        private const int ACL_REVISION = 2;
        private const uint MAXDWORD = 0xFFFFFFFF;
        // inherit flags of an ace header
        private const int OBJECT_INHERIT_ACE = 0x1;
        private const int CONTAINER_INHERIT_ACE = 0x2;
        private const int NO_PROPAGATE_INHERIT_ACE = 0x4;
        private const int INHERIT_ONLY_ACE = 0x8;
        private const int INHERITED_ACE = 0x10;
        private const int NO_INHERITANCE = 0x0;
        // security descriptor flags.
        private const int SE_DACL_AUTO_INHERIT_REQ = 0x100;
        private const int SE_DACL_AUTO_INHERITED = 0x400;
        private const int SE_DACL_PROTECTED = 0x1000;
        // ACE being added.
        private const int ACCESS_ALLOWED_ACE_TYPE = 0x0;
        private const int ACCESS_DENIED_ACE_TYPE = 0x1;
        // folder specific access rights
        private const int INVALID_HANDLE_VALUE = -1;
        private const int OPEN_EXISTING = 3;
        private const int FILE_FLAG_BACKUP_SEMANTICS = 0x2000000;
        private const int FILE_NO_ACCESS = 0x0;
        private const int FILE_LIST_DIRECTORY = 0x1;
        private const int FILE_ADD_FILE = 0x2;
        private const int FILE_ADD_SUBDIRECTORY = 0x4;
        private const int FILE_TRAVERSE = 0x20;
        private const int FILE_DELETE_CHILD = 0x40;
        private const int FILE_READ_DATA = 0x1;
        private const int FILE_WRITE_DATA = 0x2;
        private const int FILE_APPEND_DATA = 0x4;
        private const int FILE_EXECUTE = 0x20;
        private const int FILE_READ_EA = 0x8;
        private const int FILE_WRITE_EA = 0x10;
        private const int FILE_READ_ATTRIBUTES = 0x80;
        private const int FILE_WRITE_ATTRIBUTES = 0x100;
        // generic access masks for files
        private const int STANDARD_RIGHTS_REQUIRED = 0xF0000;
        private const int SYNCHRONIZE = 0x100000;
        private const int STANDARD_RIGHTS_READ = READ_CONTROL;
        private const int STANDARD_RIGHTS_WRITE = READ_CONTROL;
        //private const int STANDARD_RIGHTS_ALL = 0x1F0000;
        //private const int SPECIFIC_RIGHTS_ALL = 0xFFFF;
        private const int FILE_ALL_ACCESS =  (STANDARD_RIGHTS_REQUIRED | SYNCHRONIZE | 511);
        private const int FILE_GENERIC_READ = (STANDARD_RIGHTS_READ | FILE_READ_DATA |FILE_READ_ATTRIBUTES | FILE_READ_EA | SYNCHRONIZE);
        private const int FILE_GENERIC_WRITE = (STANDARD_RIGHTS_WRITE | FILE_WRITE_DATA | FILE_APPEND_DATA | SYNCHRONIZE);
        private const int FILE_GENERIC_EXECUTE = (STANDARD_RIGHTS_EXECUTE | FILE_READ_ATTRIBUTES | FILE_EXECUTE | SYNCHRONIZE);
        // registry access masks
        private const int KEY_READ = 0x20019;
        private const int KEY_WRITE = 0x20006;
        private const int KEY_EXECUTE = 0x20019;
        private const int KEY_ALL_ACCESS = 0xF003F;
        private const int KEY_CREATE_LINK = 0x0020;
        private const int KEY_CREATE_SUB_KEY = 0x0004;
        private const int KEY_ENUMERATE_SUB_KEYS = 0x0008;
        private const int KEY_NOTIFY = 0x0010;
        private const int KEY_SET_VALUE = 0x0002;
        private const int KEY_WOW64_32KEY = 0x0200;
        private const int KEY_WOW64_64KEY = 0x0100;
        #endregion

        #region Structs
        [StructLayout(LayoutKind.Sequential)]
        private struct ACL 
        {
            public byte AclRevision;
            public byte Sbz1;
            public short AclSize;
            public short AceCount;
            public short Sbz2;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct ACL_SIZE_INFORMATION 
        {
            public int AceCount;
            public int AclBytesInUse;
            public int AclBytesFree;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct ACE_HEADER 
        {
            public byte AceType;
            public byte AceFlags;
            public short AceSize;
        } 

        [StructLayout(LayoutKind.Sequential)]
        private struct ACE 
        {
            public ACE_HEADER Header;
            public int Mask;
            public int SidStart;
        }

        [StructLayoutAttribute(LayoutKind.Sequential)]
        private struct SID_IDENTIFIER_AUTHORITY
        {
            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 6, ArraySubType = UnmanagedType.I1)]
            public byte[] Value;
        }

        private struct ACCOUNT_PERM
        {
            public string AccountName;
            public uint AccessMask;
            public byte AceFlags;
            public byte AceType;
            public IntPtr pSid;
            public bool SidPassedByCaller;
        }

        private struct MEM_DATA
        {
            public string AccountName;
            public int AccessMask;
            public byte AceFlags;
            public IntPtr pSd;
            public IntPtr pAcl;
        }

        [StructLayoutAttribute(LayoutKind.Sequential)]
        private struct SECURITY_DESCRIPTOR 
        {
            public byte revision;
            public byte size;
            public short control;
            public IntPtr owner;
            public IntPtr group;
            public IntPtr sacl;
            public IntPtr dacl;
        }

        [StructLayoutAttribute(LayoutKind.Sequential)]
        private struct LUID_AND_ATTRIBUTES
        {
            public LUID Luid;
            public uint Attributes;
        }

        [StructLayoutAttribute(LayoutKind.Sequential)]
        private struct LUID
        {
            public uint LowPart;
            public int HighPart;
        }

        [StructLayoutAttribute(LayoutKind.Sequential)]
        private struct TOKEN_PRIVILEGES
        {
            public uint PrivilegeCount;
            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 1, ArraySubType = UnmanagedType.Struct)]
            public LUID_AND_ATTRIBUTES[] Privileges;
        }

        [StructLayoutAttribute(LayoutKind.Sequential)]
        private struct TRUSTEE
        {
            public System.IntPtr pMultipleTrustee;
            public MULTIPLE_TRUSTEE_OPERATION MultipleTrusteeOperation;
            public TRUSTEE_FORM TrusteeForm;
            public TRUSTEE_TYPE TrusteeType;
            public IntPtr ptstrName;
        }

        [StructLayoutAttribute(LayoutKind.Sequential)]
        private struct EXPLICIT_ACCESS
        {
            public ACCESS_MASK grfAccessPermissions;
            public ACCESS_MODE grfAccessMode;
            public uint grfInheritance;
            public TRUSTEE Trustee;
        }
        #endregion

        #region Public Enums
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

        public enum SE_OBJECT_TYPE 
        {
            SE_UNKNOWN_OBJECT_TYPE = 0,
            SE_FILE_OBJECT,
            SE_SERVICE,
            SE_PRINTER,
            SE_REGISTRY_KEY,
            SE_LMSHARE,
            SE_KERNEL_OBJECT,
            SE_WINDOW_OBJECT,
            SE_DS_OBJECT,
            SE_DS_OBJECT_ALL,
            SE_PROVIDER_DEFINED_OBJECT,
            SE_WMIGUID_OBJECT,
            SE_REGISTRY_WOW64_32KEY 
        }

        public enum eAccessMask : int
        {
            Delete_Only = DELETE | READ_CONTROL,
            Execute_Only = STANDARD_RIGHTS_EXECUTE | STANDARD_RIGHTS_READ,
            Read_Only = STANDARD_RIGHTS_READ,
            Read_Write = READ_CONTROL | STANDARD_RIGHTS_WRITE,
            Read_Write_Delete = STANDARD_RIGHTS_READ | STANDARD_RIGHTS_WRITE | DELETE,
            Read_Write_Execute = STANDARD_RIGHTS_READ | STANDARD_RIGHTS_WRITE | STANDARD_RIGHTS_EXECUTE,
            Read_Write_Execute_Delete = STANDARD_RIGHTS_READ | STANDARD_RIGHTS_WRITE | STANDARD_RIGHTS_EXECUTE | DELETE,
            Standard_Rights = STANDARD_RIGHTS_ALL,
            Full_Control = MAXIMUM_ALLOWED,
            System_Control = MAXIMUM_ALLOWED | ACCESS_SYSTEM_SECURITY
        }

        public enum eFolderPermissions : int
        {
            // generic permissions structures
            Folder_Read = FILE_GENERIC_READ,
            Folder_Read_Execute = FILE_GENERIC_READ | FILE_GENERIC_EXECUTE,
            Folder_Read_Write = FILE_GENERIC_READ | FILE_GENERIC_WRITE,
            Folder_Read_Write_List = FILE_GENERIC_READ | FILE_GENERIC_WRITE | FILE_LIST_DIRECTORY,
            Folder_Read_Write_Execute = FILE_GENERIC_READ | FILE_GENERIC_WRITE | FILE_GENERIC_EXECUTE,
            Folder_Read_Write_Execute_List = FILE_GENERIC_READ | FILE_GENERIC_WRITE | FILE_GENERIC_EXECUTE | FILE_LIST_DIRECTORY,
            Folder_Read_Execute_List = FILE_GENERIC_READ | FILE_GENERIC_EXECUTE | FILE_LIST_DIRECTORY,
            Folder_Read_List = FILE_GENERIC_READ | FILE_LIST_DIRECTORY,
            // specific attributes
            Folder_List = FILE_LIST_DIRECTORY,
            Folder_Delete = FILE_DELETE_CHILD,
            Folder_Execute = FILE_GENERIC_EXECUTE,
            Folder_Full_Control = FILE_ALL_ACCESS,
            Folder_No_Access = FILE_NO_ACCESS
        }

        public enum eInheritenceFlags : int
        {
            // singular inheritence attributes first ~/
            /// <summary>
            /// folder and and future subfolders inherit attributes
            /// </summary>
            Container_Inherit = CONTAINER_INHERIT_ACE,
            /// <summary>
            /// folder and future files inherit attributes
            /// </summary>
            Object_Inherit = OBJECT_INHERIT_ACE,
            /// <summary>
            /// just the direct children
            /// </summary>
            Non_Propogate = NO_PROPAGATE_INHERIT_ACE,
            /// <summary>
            /// ace applies not to this object, but to child objects
            /// </summary>
            Inherit_Only = INHERIT_ONLY_ACE,
            /// <summary>
            /// apply to parent and child
            /// </summary>
            Inherit_Ace = INHERITED_ACE,
            // compound inheritence structures ~/
            /// <summary>
            /// folder, and future subfolders and files inherit attributes
            /// </summary>
            Object_Container_Inherit = OBJECT_INHERIT_ACE | CONTAINER_INHERIT_ACE,
            /// <summary>
            /// subfolders, files, children only, one level
            /// </summary>
            Child_Inherit_Level = OBJECT_INHERIT_ACE | CONTAINER_INHERIT_ACE | NO_PROPAGATE_INHERIT_ACE,
            /// <summary>
            /// subfolders, files, children only
            /// </summary>
            Child_Container_Inherit = OBJECT_INHERIT_ACE | CONTAINER_INHERIT_ACE | INHERIT_ONLY_ACE,
            /// <summary>
            /// subfolders, files, parent and children
            /// </summary>
            Family_Container_Inherit = OBJECT_INHERIT_ACE | CONTAINER_INHERIT_ACE | INHERITED_ACE
        }

        public enum eAccessType
        {
            // permissive dacl
            Access_Allowed,
            // deny dacl
            Access_Denied
        }

        public enum eRegistryAccess : uint
        {
            // unique rights
            Registry_Read = KEY_READ,
            Registry_Write = KEY_WRITE,
            Registry_Execute = KEY_EXECUTE,
            Registry_Full_Control = KEY_ALL_ACCESS,
            // compound rights
            Registry_Read_Write = KEY_READ | KEY_WRITE,
            Registry_Read_Execute = KEY_READ | KEY_EXECUTE,
            Registry_Read_Write_Execute = KEY_READ | KEY_WRITE | KEY_EXECUTE
        }


        #endregion

        #region Private Enums
        [Flags]
        private enum SECURITY_INFORMATION : uint
        {
            OWNER_SECURITY_INFORMATION = 0x00000001,
            GROUP_SECURITY_INFORMATION = 0x00000002,
            DACL_SECURITY_INFORMATION = 0x00000004,
            SACL_SECURITY_INFORMATION = 0x00000008,
            LABEL_SECURITY_INFORMATION = 0x00000010,
            ATTRIBUTE_SECURITY_INFORMATION = 0x00000020,
            SCOPE_SECURITY_INFORMATION = 0x00000040
        }

        private enum TOKEN_PRIVILEGES_ENUM : uint
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
        [Flags]
        private enum ACCESS_MASK : uint
        {
            GENERIC_ALL = 0x10000000,
            GENERIC_READ = 0x80000000
        }

        private enum ACCESS_MODE 
        {
            NOT_USED_ACCESS,
            GRANT_ACCESS,
            SET_ACCESS,
            DENY_ACCESS,
            REVOKE_ACCESS,
            SET_AUDIT_SUCCESS,
            SET_AUDIT_FAILURE
        }
        private enum SID_NAME_USE 
        {
            SidTypeUser = 1,
            SidTypeGroup,
            SidTypeDomain,
            SidTypeAlias,
            SidTypeWellKnownGroup,
            SidTypeDeletedAccount,
            SidTypeInvalid,
            SidTypeUnknown,
            SidTypeComputer
        }

        private enum ACL_INFORMATION_CLASS
        {
            AclRevisionInformation = 1,
            AclSizeInformation
        }

        private enum MULTIPLE_TRUSTEE_OPERATION
        {
            NO_MULTIPLE_TRUSTEE,
            TRUSTEE_IS_IMPERSONATE
        }
 
        private enum TRUSTEE_FORM
        {
            TRUSTEE_IS_SID,
            TRUSTEE_IS_NAME,
            TRUSTEE_BAD_FORM,
            TRUSTEE_IS_OBJECTS_AND_SID,
            TRUSTEE_IS_OBJECTS_AND_NAME 
        }
         
        private enum TRUSTEE_TYPE
        {
            TRUSTEE_IS_UNKNOWN,
            TRUSTEE_IS_USER,
            TRUSTEE_IS_GROUP,
            TRUSTEE_IS_DOMAIN,
            TRUSTEE_IS_ALIAS,
            TRUSTEE_IS_WELL_KNOWN_GROUP,
            TRUSTEE_IS_DELETED,
            TRUSTEE_IS_INVALID,
            TRUSTEE_IS_COMPUTER 
        }

        public enum EXTENDED_NAME_FORMAT
        {
            NameUnknown = 0,
            NameFullyQualifiedDN = 1,
            NameSamCompatible = 2,
            NameDisplay = 3,
            NameUniqueId = 6,
            NameCanonical = 7,
            NameUserPrincipal = 8,
            NameCanonicalEx = 9,
            NameServicePrincipal = 10,
            NameDnsDomain = 12
        }
        #endregion

        #region API
        [DllImport("Kernel32.dll", SetLastError = false)]
        private static extern void RtlMoveMemory(IntPtr dest, IntPtr src, int size);
        [DllImport("Kernel32.dll", SetLastError = false)]
        private static extern void RtlMoveMemory(ref ACE dest, IntPtr src, int size);
        [DllImport("Kernel32.dll", SetLastError = false)]
        private static extern void RtlMoveMemory(IntPtr dest, ref ACE src, int size);

        [DllImport("kernel32.dll")]
        private static extern IntPtr LocalAlloc(int uFlags, int uBytes);

        [DllImportAttribute("kernel32.dll", EntryPoint = "LocalFree")]
        private static extern IntPtr LocalFree(IntPtr hMem);

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern int RegGetKeySecurity(IntPtr hKey, int SecurityInformation, IntPtr pSecurityDescriptor, ref int lpcbSecurityDescriptor);

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern int RegSetKeySecurity(IntPtr hKey, int SecurityInformation, IntPtr pSecurityDescriptor);

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern int InitializeSecurityDescriptor(IntPtr pSecurityDescriptor, int dwRevision);

        [DllImport("advapi32.dll", SetLastError=true)]
        private static extern bool LookupAccountName(string lpSystemName, string lpAccountName, IntPtr Sid,
            ref int cbSid, StringBuilder ReferencedDomainName, ref int cchReferencedDomainName, out int peUse);

        [DllImport("advapi32.dll")]
        private static extern int GetLengthSid(IntPtr pSid);

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool InitializeAcl(IntPtr pAcl, int nAclLength, int dwAclRevision);

        [DllImport("advapi32.dll", SetLastError=true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetSecurityDescriptorDacl(SECURITY_DESCRIPTOR sd, bool daclPresent, IntPtr dacl, bool daclDefaulted);

        [DllImport("advapi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetSecurityDescriptorDacl(IntPtr sd, int daclPresent, IntPtr dacl, int daclDefaulted);

        [DllImport("advapi32.dll")]
        private static extern int GetAce(IntPtr aclPtr, int aceIndex, out IntPtr acePtr);

        [DllImport("advapi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetSecurityDescriptorDacl(IntPtr pSecurityDescriptor, [MarshalAs(UnmanagedType.Bool)] out bool bDaclPresent,
            ref IntPtr pDacl, [MarshalAs(UnmanagedType.Bool)] out bool bDaclDefaulted);

        [DllImport("advapi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetSecurityDescriptorDacl(IntPtr pSecurityDescriptor, out int bDaclPresent,
            out IntPtr pDacl, out int bDaclDefaulted);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetAclInformation(IntPtr pAcl, ref ACL_SIZE_INFORMATION pAclInformation, int nAclInformationLength,
            ACL_INFORMATION_CLASS dwAclInformationClass);

        [DllImport("advapi32.dll", SetLastError=true)]
        private static extern int GetSecurityDescriptorControl(IntPtr pSecurityDescriptor, out int pControl, out int lpdwRevision);

        [DllImport("advapi32.dll", SetLastError=true)]
        private static extern int SetSecurityDescriptorControl(IntPtr pSecurityDescriptor, int pControl, int lpdwRevision);
        
        [DllImport("advapi32.dll", SetLastError=true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool EqualSid(IntPtr pSid1, IntPtr pSid2);

        [DllImport("advapi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool AddAce(IntPtr pAcl, int dwAceRevision, uint dwStartingAceIndex, IntPtr pAceList, int nAceListLength);

        [DllImportAttribute("advapi32.dll", EntryPoint = "AllocateAndInitializeSid")]
        [return: MarshalAsAttribute(UnmanagedType.Bool)]
        private static extern bool AllocateAndInitializeSid([InAttribute] ref SID_IDENTIFIER_AUTHORITY pIdentifierAuthority, byte nSubAuthorityCount,
            uint nSubAuthority0, uint nSubAuthority1, uint nSubAuthority2, uint nSubAuthority3, uint nSubAuthority4, uint nSubAuthority5,
            uint nSubAuthority6, uint nSubAuthority7, ref IntPtr pSid);

        [DllImportAttribute("advapi32.dll", EntryPoint = "FreeSid")]
        private static extern IntPtr FreeSid([InAttribute] IntPtr pSid);

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern int GetKernelObjectSecurity(IntPtr hObject, int RequestedInformation, out IntPtr pSecurityDescriptor, int nLength, out int lpnLengthNeeded);

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern int SetKernelObjectSecurity(IntPtr hObject, int SecurityInformation, IntPtr pSecurityDescriptor);

        [DllImport("kernel32.dll")]
        private static extern IntPtr CreateFile(string lpFileName, int dwDesiredAccess, int dwShareMode, IntPtr SecurityAttributes, int dwCreationDisposition, int dwFlagsAndAttributes, IntPtr hTemplateFile);

        [DllImport("advapi32.dll", EntryPoint = "RegOpenKeyEx")]
        private static extern int RegOpenKeyEx(ROOT_KEY hKey, string subKey, int options, int samDesired, ref IntPtr phkResult);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, EntryPoint = "RegCloseKey", SetLastError = true)]
        private static extern int RegCloseKey(IntPtr hKey);

        [DllImportAttribute("advapi32.dll", CharSet = CharSet.Ansi, EntryPoint = "LookupPrivilegeValueA")]
        [return: MarshalAsAttribute(UnmanagedType.Bool)]
        private static extern bool LookupPrivilegeValueA([InAttribute] [MarshalAsAttribute(UnmanagedType.LPStr)] string lpSystemName,
            [InAttribute] [MarshalAsAttribute(UnmanagedType.LPStr)] string lpName, [OutAttribute] out LUID lpLuid);

        [DllImportAttribute("advapi32.dll", EntryPoint = "AdjustTokenPrivileges")]
        [return: MarshalAsAttribute(UnmanagedType.Bool)]
        private static extern bool AdjustTokenPrivileges([InAttribute()] IntPtr TokenHandle, [MarshalAsAttribute(UnmanagedType.Bool)] bool DisableAllPrivileges,
            [InAttribute()] ref TOKEN_PRIVILEGES NewState, uint BufferLength, IntPtr PreviousState, IntPtr ReturnLength);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto)]
        private static extern int SetNamedSecurityInfo([MarshalAs(UnmanagedType.LPTStr)] string pObjectName, SE_OBJECT_TYPE ObjectType, SECURITY_INFORMATION SecurityInfo,
            IntPtr psidOwner, IntPtr psidGroup, IntPtr pDacl, IntPtr pSacl);

        [DllImportAttribute("advapi32.dll", EntryPoint = "OpenProcessToken")]
        [return: MarshalAsAttribute(UnmanagedType.Bool)]
        private static extern bool OpenProcessToken([InAttribute] IntPtr ProcessHandle, uint DesiredAccess, out IntPtr TokenHandle);
       
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr OpenProcess(int dwDesiredAccess, int blnheritHandle, int dwAppProcessId);

        [DllImportAttribute("kernel32.dll", EntryPoint = "CloseHandle")]
        [return: MarshalAsAttribute(UnmanagedType.Bool)]
        private static extern bool CloseHandle([InAttribute] IntPtr hObject);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern int GetCurrentProcessId();

        [DllImportAttribute("kernel32.dll", EntryPoint = "GetCurrentProcess")]
        private static extern IntPtr GetCurrentProcess();

        [DllImport("Advapi32.dll", EntryPoint = "SetEntriesInAclA", CallingConvention = CallingConvention.Winapi, SetLastError = true, CharSet = CharSet.Ansi)]
        private static extern int SetEntriesInAcl(int CountofExplicitEntries, ref EXPLICIT_ACCESS ea, IntPtr OldAcl, out IntPtr NewAcl);

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool GetUserName(StringBuilder sb, ref int length);

        [DllImport("secur32.dll", CharSet = CharSet.Auto)]
        public static extern int GetUserNameEx(int nameFormat, StringBuilder userName, ref int userNameSize);
        #endregion

        #region Methods
        public string UserName()
        {
            StringBuilder name = new StringBuilder(64);
            int size = name.Capacity;

            try
            {
                GetUserName(name, ref size);
            }
            finally { }
            return name.ToString();
        }

        public String UserName(EXTENDED_NAME_FORMAT NameFormat)
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                StringBuilder name = new StringBuilder(1024);
                int size = name.Capacity;
                try
                {
                    if (0 != GetUserNameEx((int)NameFormat, name, ref size))
                    {
                        return name.ToString();
                    }
                }
                finally { }
            }
            return null;
        }

        public bool ChangeKeyPermissions(ROOT_KEY RootKey, string SubKey, string AccountName, eRegistryAccess AccessMask, eAccessType AccessType, eInheritenceFlags Inheritence)
        {
            // set key permissions (gate)
            IntPtr lKey = IntPtr.Zero;
            ACCOUNT_PERM tAccount = new ACCOUNT_PERM();
            SID_IDENTIFIER_AUTHORITY tAuthority = new SID_IDENTIFIER_AUTHORITY();

            try
            {
                // default account
                tAccount.AccountName = "";
                tAccount.AccessMask = (uint)ACCESS_MASK.GENERIC_READ;
                tAccount.AceFlags = (byte)CONTAINER_INHERIT_ACE;
                tAccount.AceType = (byte)ACCESS_ALLOWED_ACE_TYPE;
                tAccount.pSid = IntPtr.Zero;
                tAuthority.Value = new byte[] { 0, 0, 0, 0, 0, (byte)SECURITY_WORLD_SID_AUTHORITY };

                // test access
                if (AllocateAndInitializeSid(ref tAuthority, (byte)1, (int)SECURITY_WORLD_RID, 0, 0, 0, 0, 0, 0, 0, ref tAccount.pSid) == true)
                {
                    // set up account
                    tAccount.AccountName = AccountName;
                    tAccount.AccessMask = (uint)AccessMask;
                    tAccount.AceFlags = (byte)Inheritence;
                    tAccount.AceType = (byte)AccessType;
                    tAccount.pSid = IntPtr.Zero;
                    tAccount.SidPassedByCaller = false;
                    // apply change to key
                    if ((RegOpenKeyEx(RootKey, SubKey, 0, (int)(READ_CONTROL | WRITE_DAC), ref lKey) == 0))
                    {
                        return SetKeyPermissions(lKey, tAccount);
                    }
                }
                return false;
            }
            finally
            {
                // cleanup
                if (lKey != IntPtr.Zero)
                {
                    RegCloseKey(lKey);
                }
                if ((tAccount.pSid != IntPtr.Zero) && (tAccount.SidPassedByCaller == true))
                {
                    FreeSid(tAccount.pSid);
                    tAccount.pSid = IntPtr.Zero;
                }
            }
        }

        private bool SetKeyPermissions(IntPtr hKey, ACCOUNT_PERM Account)
        {
            // apply key permissions
            int iLength = 0;
            int iReturn = 0;
            IntPtr pOldSD = IntPtr.Zero;
            IntPtr pSec = IntPtr.Zero;
            MEM_DATA tMem = new MEM_DATA();

            try
            {
                tMem.pAcl = IntPtr.Zero;
                tMem.pSd = IntPtr.Zero;

                // allocate space
                iReturn = RegGetKeySecurity(hKey, DACL_SECURITY_INFORMATION, pOldSD, ref iLength);
                if (iLength > 0)
                {
                    pOldSD = (LocalAlloc(LMEM_INITIALIZED, iLength));
                    if (pOldSD != IntPtr.Zero)
                    {
                        // load structure
                        if ((RegGetKeySecurity(hKey, DACL_SECURITY_INFORMATION, pOldSD, ref iLength) == 0))
                        {
                            // create dacl
                            if (CreateKeyDescriptor(pOldSD, Account, ref tMem))
                            {
                                // set descriptor
                                return (RegSetKeySecurity(hKey, DACL_SECURITY_INFORMATION, tMem.pSd) == ERROR_SUCCESS);
                            }
                        }
                    }
                }
                return false;
            }

            finally
            {
                // cleanup
                if (pOldSD != IntPtr.Zero)
                {
                    LocalFree(pOldSD);
                    pOldSD = IntPtr.Zero;
                }
                if (tMem.pSd != IntPtr.Zero)
                {
                    LocalFree(tMem.pSd);
                    tMem.pSd = IntPtr.Zero;
                }
                if (tMem.pAcl != IntPtr.Zero)
                {
                    LocalFree(tMem.pAcl);
                    tMem.pAcl = IntPtr.Zero;
                }
            }
        }

        private bool CreateKeyDescriptor(IntPtr pOldSD, ACCOUNT_PERM Account, ref MEM_DATA MemData)
        {
            // reconstruct security descriptor
            bool bDefault = false;
            bool bPresent = false;
            int iControlBits = 0;
            int iControlSet = 0;
            int iDomain = 0;
            int iFlag = 0;
            int iLength = 0;
            int iRevision = 0;
            int iSidLen = 0;
            int iTotal = 0;
            int iUse = 0;
            StringBuilder sDomain = new StringBuilder(256);
            IntPtr pNewACL = IntPtr.Zero;
            IntPtr pAcl = IntPtr.Zero;
            IntPtr pSid = IntPtr.Zero;
            IntPtr pPnt = IntPtr.Zero;
            ACL_SIZE_INFORMATION tAclSize = new ACL_SIZE_INFORMATION();
            ACL tTempACL = new ACL();
            ACE tTempAce = new ACE();

            try
            {
                MemData.pAcl = IntPtr.Zero;
                MemData.pSd = IntPtr.Zero;
                // get size
                pSid = LocalAlloc(LMEM_INITIALIZED, SECURITY_DESCRIPTOR_MIN_LENGTH);
                if (pSid == IntPtr.Zero)
                {
                    return false;
                }
                // store pointer
                MemData.pSd = pSid;

                // init descriptor
                if (InitializeSecurityDescriptor(pSid, SECURITY_DESCRIPTOR_REVISION) == 0)
                {
                    return false;
                }

                // check for existing sd
                if (pOldSD != IntPtr.Zero)
                {
                    if (GetSecurityDescriptorDacl(pOldSD, out bPresent, ref pAcl, out bDefault) == true)
                    {
                        // extract dacl
                        if ((bPresent == true) && (pAcl != IntPtr.Zero))
                        {
                            if (GetAclInformation(pAcl, ref tAclSize, Marshal.SizeOf(tAclSize), ACL_INFORMATION_CLASS.AclSizeInformation) == false)
                            {
                                return false;
                            }
                            else
                            {
                                iTotal = tAclSize.AclBytesInUse;
                            }
                        }
                        else
                        {
                            iTotal = Marshal.SizeOf(tTempACL);
                        }
                    }
                    else
                    {
                        return false;
                    }
                }

                // allocate sid //
                // get callers sid
                if (Account.pSid == IntPtr.Zero)
                {
                    iDomain = 256;
                    // get size
                    LookupAccountName(null, Account.AccountName, IntPtr.Zero, ref iSidLen, sDomain, ref iDomain, out iUse);
                    Account.pSid = LocalAlloc(LMEM_INITIALIZED, iSidLen);
                    if (Account.pSid == IntPtr.Zero)
                    {
                        return false;
                    }
                    // get the sid
                    if (LookupAccountName(null, Account.AccountName, Account.pSid, ref iSidLen, sDomain, ref iDomain, out iUse) == false)
                    {
                        return false;
                    }
                }

                // ace buffer
                iLength = (Marshal.SizeOf(tTempAce) + GetLengthSid(Account.pSid)) - 4;
                iTotal += iLength;
                pNewACL = LocalAlloc(LMEM_INITIALIZED, iTotal);
                if (pNewACL == IntPtr.Zero)
                {
                    return false;
                }
                // store pointer
                MemData.pAcl = pNewACL;

                // init acl
                if (InitializeAcl(pNewACL, iTotal, ACL_REVISION) == false)
                {
                    return false;
                }

                // build dacl in sequence
                if (Account.AceType == ACCESS_DENIED_ACE_TYPE)
                {
                    if (BuildACE(pNewACL, Account.AceType, Account.AceFlags, Account.AccessMask, Account.pSid) == false)
                    {
                        return false;
                    }
                }

                // copy non-inherited ace
                if ((bPresent == true) && (pAcl != IntPtr.Zero) && (tAclSize.AceCount > 0))
                {
                    // combine old and new ACE entries
                    for (int count = 0; count < tAclSize.AceCount; count++)
                    {
                        // next ace
                        GetAce(pAcl, count, out pPnt);
                        if (pPnt == IntPtr.Zero)
                        {
                            return false;
                        }
                        RtlMoveMemory(ref tTempAce, pPnt, Marshal.SizeOf(tTempAce));
                        // exit on inherited ace
                        if (((int)tTempAce.Header.AceFlags & INHERITED_ACE) == INHERITED_ACE)
                        {
                            break;
                        }
                        int x = (int)pPnt + 8;
                        IntPtr pPt = new IntPtr(x);
                        // check ace value
                        if (!SidIsEqual(Account.pSid, pPt))
                        {
                            // add ace
                            AddAce(pNewACL, ACL_REVISION, MAXDWORD, pPnt, tTempAce.Header.AceSize);
                        }
                    }
                }

                // add explicit permit
                if (Account.AceType == ACCESS_ALLOWED_ACE_TYPE)
                {
                    BuildACE(pNewACL, Account.AceType, Account.AceFlags, Account.AccessMask, Account.pSid);
                }

                // enties with inheritence flag
                if ((bPresent == true) && (pAcl != IntPtr.Zero) && (tAclSize.AceCount > 0))
                {
                    for (int count = 0; count < tAclSize.AceCount; count++)
                    {
                        GetAce(pAcl, count, out pPnt);
                        RtlMoveMemory(ref tTempAce, pPnt, Marshal.SizeOf(tTempAce));
                        AddAce(pNewACL, ACL_REVISION, MAXDWORD, pPnt, tTempAce.Header.AceSize);
                    }
                }

                // descriptor flags
                if (pOldSD != IntPtr.Zero)
                {
                    if (GetSecurityDescriptorControl(pOldSD, out iFlag, out iRevision) != 0)
                    {
                        if ((iFlag & SE_DACL_AUTO_INHERITED) == SE_DACL_AUTO_INHERITED)
                        {
                            iControlBits = SE_DACL_AUTO_INHERIT_REQ | SE_DACL_AUTO_INHERITED;
                            iControlSet = iControlBits;
                        }
                        else if ((iFlag & SE_DACL_PROTECTED) == SE_DACL_PROTECTED)
                        {
                            iControlBits = SE_DACL_PROTECTED;
                            iControlSet = iControlBits;
                        }
                        if (iControlSet != 0)
                        {
                            SetSecurityDescriptorControl(pSid, iControlBits, iControlSet);
                        }
                    }
                }
                // add dacl
                return SetSecurityDescriptorDacl(pSid, 1, pNewACL, 0);
            }

            finally
            {
                if (Account.pSid != IntPtr.Zero)
                {
                    LocalFree(Account.pSid);
                    Account.pSid = IntPtr.Zero;
                }
            }
        }

        private bool BuildACE(IntPtr lAclId, byte bType, byte bFlags, uint lMask, IntPtr lPointer)
        {
            // build an ace entry
            int iAceLen = 0;
            int iSidLen = 0;
            IntPtr pAce = IntPtr.Zero;
            ACE tTempAce = new ACE();

            try
            {
                // get len
                iSidLen = GetLengthSid(lPointer);
                iAceLen = Marshal.SizeOf(tTempAce) + iSidLen - 4;
                // allocate space
                pAce = LocalAlloc(LMEM_INITIALIZED, iAceLen);

                if (pAce != IntPtr.Zero)
                {
                    // ace struct
                    tTempAce.Header = new ACE_HEADER();
                    tTempAce.Header.AceType = bType;
                    tTempAce.Header.AceFlags = bFlags;
                    tTempAce.Header.AceSize = (short)iAceLen;
                    tTempAce.Mask = (int)lMask;

                    // copy to buffer
                    int dDt = (int)pAce + 8;
                    IntPtr pDt = new IntPtr(dDt);
                    RtlMoveMemory(pAce, ref tTempAce, Marshal.SizeOf(tTempAce));
                    RtlMoveMemory(pDt, lPointer, iSidLen);

                    // add to acl
                    return AddAce(lAclId, ACL_REVISION, MAXDWORD, pAce, iAceLen);
                }
                return false;
            }
            finally
            {
                if (pAce != IntPtr.Zero)
                {
                    LocalFree(pAce);
                }
            }
        }

        private bool SidIsEqual(IntPtr pSid1, IntPtr pSid2)
        {
            return (EqualSid(pSid1, pSid2));
        }

        public bool ChangeObjectOwnership(string ObjectName, SE_OBJECT_TYPE ObjectType)
        {
            bool success = false;
            IntPtr pSidAdmin = IntPtr.Zero;
            IntPtr pAcl = IntPtr.Zero;
            string name = ObjectName;
            SID_IDENTIFIER_AUTHORITY sidNTAuthority = new SID_IDENTIFIER_AUTHORITY() { Value = new byte[] { 0, 0, 0, 0, 0, 5 } };

            success = AllocateAndInitializeSid(ref sidNTAuthority, (byte)2, SECURITY_BUILTIN_DOMAIN_RID, DOMAIN_ALIAS_RID_ADMINS, 0, 0, 0, 0, 0, 0, ref pSidAdmin);

            if (ObjectName.StartsWith("HKEY_CLASSES_ROOT"))
            {
                name = ObjectName.Replace("HKEY_CLASSES_ROOT", "CLASSES_ROOT");
            }
            else if (ObjectName.StartsWith("HKEY_CURRENT_USER"))
            {
                name = ObjectName.Replace("HKEY_CURRENT_USER", "CURRENT_USER");
            }
            else if (ObjectName.StartsWith("HKEY_LOCAL_MACHINE"))
            {
                name = ObjectName.Replace("HKEY_LOCAL_MACHINE", "MACHINE");
            }
            else if (ObjectName.StartsWith("HKEY_USERS"))
            {
                name = ObjectName.Replace("HKEY_USERS", "USERS");
            }

            if (success)
            {
                EXPLICIT_ACCESS[] explicitAccesss = new EXPLICIT_ACCESS[1];
                explicitAccesss[0].grfAccessPermissions = ACCESS_MASK.GENERIC_ALL;
                explicitAccesss[0].grfAccessMode = ACCESS_MODE.SET_ACCESS;
                explicitAccesss[0].grfInheritance = NO_INHERITANCE;
                explicitAccesss[0].Trustee.TrusteeForm = TRUSTEE_FORM.TRUSTEE_IS_SID;
                explicitAccesss[0].Trustee.TrusteeType = TRUSTEE_TYPE.TRUSTEE_IS_GROUP;
                explicitAccesss[0].Trustee.ptstrName = pSidAdmin;
                //modify dacl
                SetEntriesInAcl(1, ref explicitAccesss[0], IntPtr.Zero, out pAcl);

                success = SetPrivilege(SE_TAKE_OWNERSHIP_NAME, true);
                if (success)
                {
                    // set admin as owner
                    int p = SetNamedSecurityInfo(name, ObjectType, SECURITY_INFORMATION.OWNER_SECURITY_INFORMATION, pSidAdmin, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
                    success = SetPrivilege(SE_TAKE_OWNERSHIP_NAME, false);
                    if (success)
                    {
                        SetNamedSecurityInfo(name, ObjectType, SECURITY_INFORMATION.DACL_SECURITY_INFORMATION, IntPtr.Zero, IntPtr.Zero, pAcl, IntPtr.Zero);
                    }
                }
            }

            if (pSidAdmin != IntPtr.Zero)
            {
                FreeSid(pSidAdmin);
            }
            if (pAcl != IntPtr.Zero)
            {
                LocalFree(pAcl);
            }
            return success;
        }

        private bool SetPrivilege(string privilege, bool allow)
        {
            bool success = false;
            IntPtr token = IntPtr.Zero;
            TOKEN_PRIVILEGES tokenPrivileges = new TOKEN_PRIVILEGES();

            success = OpenProcessToken(GetCurrentProcess(), (uint)TOKEN_PRIVILEGES_ENUM.TOKEN_ADJUST_PRIVILEGES | (uint)TOKEN_PRIVILEGES_ENUM.TOKEN_QUERY, out token);
            if (success)
            {
                if (allow)
                {
                    LUID luid;
                    LookupPrivilegeValueA(null, privilege, out luid);
                    tokenPrivileges.PrivilegeCount = 1;
                    tokenPrivileges.Privileges = new LUID_AND_ATTRIBUTES[1];
                    tokenPrivileges.Privileges[0].Luid = luid;
                    tokenPrivileges.Privileges[0].Attributes = SE_PRIVILEGE_ENABLED;
                }
                success = AdjustTokenPrivileges(token, false, ref tokenPrivileges, 0, IntPtr.Zero, IntPtr.Zero);
            }
            if (token != IntPtr.Zero)
            {
                CloseHandle(token);
            }
            return success;
        }
        #endregion
    }
}
