﻿using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Net;
using System;
using System.Threading.Tasks;

namespace ssh_common.Platforms.Windows
{
    public class WindowsSystemProxy : ISystemProxy
    {
        public static WindowsSystemProxy Instance = new();

        private WindowsSystemProxy()
        {
        }

        public static bool UnsetProxy()
        {
            return SetProxy(null, null);
        }

        public static bool SetProxy(string strProxy)
        {
            return SetProxy(strProxy, null);
        }

        public static bool SetProxy(string strProxy, string exceptions)
        {
            InternetPerConnOptionList list = new();

            int optionCount = string.IsNullOrEmpty(strProxy) ? 1 : string.IsNullOrEmpty(exceptions) ? 2 : 3;
            InternetConnectionOption[] options = new InternetConnectionOption[optionCount];
            // USE a proxy server ...
            options[0].m_Option = PerConnOption.INTERNET_PER_CONN_FLAGS;
            options[0].m_Value.m_Int = (int)(optionCount < 2 ? PerConnFlags.PROXY_TYPE_DIRECT : PerConnFlags.PROXY_TYPE_DIRECT | PerConnFlags.PROXY_TYPE_PROXY);
            // use THIS proxy server
            if (optionCount > 1)
            {
                options[1].m_Option = PerConnOption.INTERNET_PER_CONN_PROXY_SERVER;
                options[1].m_Value.m_StringPtr = Marshal.StringToHGlobalAuto(strProxy);
                // except for these addresses ...
                if (optionCount > 2)
                {
                    options[2].m_Option = PerConnOption.INTERNET_PER_CONN_PROXY_BYPASS;
                    options[2].m_Value.m_StringPtr = Marshal.StringToHGlobalAuto(exceptions);
                }
            }

            // default stuff
            list.dwSize = Marshal.SizeOf(list);
            list.szConnection = IntPtr.Zero;
            list.dwOptionCount = options.Length;
            list.dwOptionError = 0;


            int optSize = Marshal.SizeOf(typeof(InternetConnectionOption));
            // make a pointer out of all that ...
            IntPtr optionsPtr = Marshal.AllocCoTaskMem(optSize * options.Length);
            // copy the array over into that spot in memory ...
            for (int i = 0; i < options.Length; ++i)
            {
                IntPtr opt = new(optionsPtr.ToInt64() + i * optSize);
                Marshal.StructureToPtr(options[i], opt, false);
            }

            list.options = optionsPtr;

            // and then make a pointer out of the whole list
            IntPtr ipcoListPtr = Marshal.AllocCoTaskMem(list.dwSize);
            Marshal.StructureToPtr(list, ipcoListPtr, false);

            // and finally, call the API method!
            int returnvalue = NativeMethods.InternetSetOption(IntPtr.Zero,
               InternetOption.INTERNET_OPTION_PER_CONNECTION_OPTION,
               ipcoListPtr, list.dwSize) ? -1 : 0;
            if (returnvalue == 0)
            {  // get the error codes, they might be helpful
                returnvalue = Marshal.GetLastWin32Error();
            }
            // FREE the data ASAP
            Marshal.FreeCoTaskMem(optionsPtr);
            Marshal.FreeCoTaskMem(ipcoListPtr);
            if (returnvalue > 0)
            {  // throw the error codes, they might be helpful
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            return returnvalue < 0;
        }

        public async Task<ProxyResult> SetSocks(string ip, string port)
        {
            try
            {

                return new ProxyResult { Exception = null, Success = SetProxy($"socks={ip}:{port}") };
            }
            catch (Exception e)
            {
                return new ProxyResult { Exception = e, Success = false };
            }
        }

        public async Task<ProxyResult> SetHttp(string ip, string port)
        {
            try
            {
                return new ProxyResult { Exception = null, Success = SetProxy($"http={ip}:{port}") };
            }
            catch (Exception e)
            {
                return new ProxyResult { Exception = e, Success = false };
            }
        }

        public async Task<ProxyResult> SetHttps(string ip, string port)
        {
            try
            {
                return new ProxyResult { Exception = null, Success = SetProxy($"https={ip}:{port}") };
            }
            catch (Exception e)
            {
                return new ProxyResult { Exception = e, Success = false };
            }
        }

        public async Task<ProxyResult> Unset()
        {
            try
            {
                return new ProxyResult { Exception = null, Success = UnsetProxy() };
            }
            catch (Exception e)
            {
                return new ProxyResult { Exception = e, Success = false };
            }
        }

        public string Current()
        {
            try
            {
                var sysProxy = WebRequest.GetSystemWebProxy().GetProxy(new Uri("https://google.com"));
                return sysProxy?.AbsolutePath;
            }
            catch
            {
                return null;
            }
        }
    }

    #region WinInet structures
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    internal struct InternetPerConnOptionList
    {
        public int dwSize;               // size of the INTERNET_PER_CONN_OPTION_LIST struct
        public IntPtr szConnection;         // connection name to set/query options
        public int dwOptionCount;        // number of options to set/query
        public int dwOptionError;           // on error, which option failed
                                            //[MarshalAs(UnmanagedType.)]
        public IntPtr options;
    };

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    internal struct InternetConnectionOption
    {
        static readonly int Size;
        public PerConnOption m_Option;
        public InternetConnectionOptionValue m_Value;
        static InternetConnectionOption()
        {
            Size = Marshal.SizeOf(typeof(InternetConnectionOption));
        }

        // Nested Types
        [StructLayout(LayoutKind.Explicit)]
        public struct InternetConnectionOptionValue
        {
            // Fields
            [FieldOffset(0)]
            public System.Runtime.InteropServices.ComTypes.FILETIME m_FileTime;
            [FieldOffset(0)]
            public int m_Int;
            [FieldOffset(0)]
            public IntPtr m_StringPtr;
        }
    }
    #endregion

    #region WinInet enums
    //
    // options manifests for Internet{Query|Set}Option
    //
    internal enum InternetOption : uint
    {
        INTERNET_OPTION_PER_CONNECTION_OPTION = 75
    }

    //
    // Options used in INTERNET_PER_CONN_OPTON struct
    //
    internal enum PerConnOption
    {
        INTERNET_PER_CONN_FLAGS = 1, // Sets or retrieves the connection type. The Value member will contain one or more of the values from PerConnFlags 
        INTERNET_PER_CONN_PROXY_SERVER = 2, // Sets or retrieves a string containing the proxy servers.  
        INTERNET_PER_CONN_PROXY_BYPASS = 3, // Sets or retrieves a string containing the URLs that do not use the proxy server.  
        INTERNET_PER_CONN_AUTOCONFIG_URL = 4//, // Sets or retrieves a string containing the URL to the automatic configuration script.  

    }

    //
    // PER_CONN_FLAGS
    //
    [Flags]
    internal enum PerConnFlags
    {
        PROXY_TYPE_DIRECT = 0x00000001,  // direct to net
        PROXY_TYPE_PROXY = 0x00000002,  // via named proxy
        PROXY_TYPE_AUTO_PROXY_URL = 0x00000004,  // autoproxy URL
        PROXY_TYPE_AUTO_DETECT = 0x00000008   // use autoproxy detection
    }
    #endregion

    internal static class NativeMethods
    {
        [DllImport("WinInet.dll", SetLastError = true, CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool InternetSetOption(IntPtr hInternet, InternetOption dwOption, IntPtr lpBuffer, int dwBufferLength);
    }
}