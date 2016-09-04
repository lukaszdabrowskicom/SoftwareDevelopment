using System;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Principal;

namespace SoftwareDevelopment.Programming.CSharp.Utilities
{
    /// <summary>
    /// Util class for common low level Windows operations.
    /// </summary>
    [Obfuscation(ApplyToMembers = true, Exclude = false, StripAfterObfuscation = true)]
    public static class WindowsUtils
    {
        //this method allows for obtaining user token
        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool LogonUser([MarshalAs(UnmanagedType.LPWStr)]string pszUsername, [MarshalAs(UnmanagedType.LPWStr)]string pszDomain, [MarshalAs(UnmanagedType.LPWStr)]string pszPassword, int dwLogonType, int dwLogonProvider, ref IntPtr phToken);

        // this method allows for closing open handles returned by LogonUser method
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private extern static bool CloseHandle(IntPtr handle);

        const int LOGON32_LOGON_INTERACTIVE = 9;
        const int LOGON32_PROVIDER_DEFAULT = 0;

        private static WindowsImpersonationContext _impersonationContext = null;
        private static IntPtr _shareLocationUserHandle = IntPtr.Zero;

        private static LogOperationTypeEnum _logOperationType = LogOperationTypeEnum.INFO;
        private static bool _includeInvocationTime = true;
        private static bool _userWasImpersonated = false;
        

        /// <summary>
        /// Elevates user privilages so that it gains access to read/write operations to a shared location.
        /// </summary>
        /// <param name="shareLocationDomain">domain of a shared location</param>
        /// <param name="shareLocationUrl">shared location url</param>
        /// <param name="shareLocationLogin">share login</param>
        /// <param name="shareLocationPassword">share password</param>
        /// <param name="logOperationType">specifies type of operation</param>
        /// <param name="includeInvocationTime">specifies whether to include timestamp</param>
        public static bool BeginImpersonationOfTheCurrentUser(string shareLocationDomain, string shareLocationUrl, string shareLocationLogin, string shareLocationPassword, LogOperationTypeEnum logOperationType, bool includeInvocationTime = true)
        {
            _logOperationType = logOperationType;
            _includeInvocationTime = includeInvocationTime;

            // call LogonUser to get a token for the user
            bool isLoggedOn = LogonUser(shareLocationLogin, shareLocationDomain, shareLocationPassword, LOGON32_LOGON_INTERACTIVE, LOGON32_PROVIDER_DEFAULT, ref _shareLocationUserHandle);

            if (isLoggedOn)
            {
                try
                {
                    //here actual impersonation takes place
                    _impersonationContext = WindowsIdentity.Impersonate(_shareLocationUserHandle);
                    _userWasImpersonated = true;
                }
                catch (ArgumentException ae)
                {
                    LogUtils.Log("User {0} could not be impersonated successfully. The error is: {1}", true, _logOperationType, _includeInvocationTime, shareLocationLogin, ae.Message);
                    CloseAcquiredResources();
                }
                catch (UnauthorizedAccessException uae)
                {
                    LogUtils.Log("UnauthorizedAccessException has occured: {0}", true, _logOperationType, _includeInvocationTime, uae.Message);
                    CloseAcquiredResources();
                }
                catch (Win32Exception w32e)
                {
                    LogUtils.Log("Win32Exception has occured: {0}", true, _logOperationType, _includeInvocationTime, w32e.Message);
                    CloseAcquiredResources();
                }
                catch (Exception e)
                {
                    LogUtils.Log("Unknown exception has occured: {0}", true, _logOperationType, _includeInvocationTime, e.Message);
                    CloseAcquiredResources();
                }
            }
            else
            {
                int errorCode = Marshal.GetLastWin32Error();
                LogUtils.Log("User {0} could not log on. The Win32 error code is: {1}", true, _logOperationType, _includeInvocationTime, shareLocationLogin, errorCode.ToString());
            }

            return _userWasImpersonated;
        }

        /// <summary>
        /// Downgrades user privilages to default user privilages. Body of the method is only invoked if user impersonation [ BeginImpersonationOfTheCurrentUser(...) ] actually took place.
        /// Otherwise invocation of this method acts like an empty method invocation.
        /// </summary>
        public static void EndImpersonationOfTheCurrentUser()
        {
            if (_userWasImpersonated)
            {
                try
                {
                    // here actual clean up process takes place
                    if (_impersonationContext != null)
                    {
                        _impersonationContext.Undo();
                    }
                }
                catch (Exception exception)
                {
                    LogUtils.Log("Unknown exception has occured: {0}", true, _logOperationType, _includeInvocationTime, exception.Message);
                }
                finally
                {
                    CloseAcquiredResources();
                }
            }
        }

        private static void CloseAcquiredResources()
        {
            if (_shareLocationUserHandle != IntPtr.Zero)
            {
                CloseHandle(_shareLocationUserHandle);
            }
        }
    }
}
