/**********************************************
 * $Id$
 * $Author$
 * $Revision$
 * $LastChangedRevision$
 * $LastChangedDate$
 ***********************************************/
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Security.Principal;

namespace SvnAccount.AccountManage
{
    /// <summary>
    /// Windows身份模拟
    /// </summary>
    public class IdentityAnalogue : IDisposable
    {
        /// <summary>
        /// 新建Windows身份模拟实例
        /// </summary>
        public IdentityAnalogue()
        {

        }

        /// <summary>
        /// win32api引用
        /// </summary>
        /// <param name="lpszUserName"></param>
        /// <param name="lpszDomain"></param>
        /// <param name="lpszPassword"></param>
        /// <param name="dwLogonType"></param>
        /// <param name="dwLogonProvider"></param>
        /// <param name="phToken"></param>
        /// <returns></returns>
        [DllImport("advapi32.dll", CharSet = CharSet.Auto)] 
        public static extern int LogonUser(string lpszUserName, string lpszDomain, string lpszPassword,
            int dwLogonType, int dwLogonProvider, ref IntPtr phToken);

        /// <summary>
        ///
        /// </summary>
        /// <param name="hToken"></param>
        /// <param name="impersonationLevel"></param>
        /// <param name="hNewToken"></param>
        /// <returns></returns>
        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int DuplicateToken(IntPtr hToken, int impersonationLevel, ref IntPtr hNewToken);

        /// <summary>
        /// 终止当前用户标识的模拟，并返回原始线程标记。
        /// </summary>
        /// <returns></returns>
        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool RevertToSelf();

        /// <summary>
        ///
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern bool CloseHandle(IntPtr handle);

        /// <summary>
        /// 模拟指定用户时使用的常量定义
        /// </summary>
        public const int LOGON32_LOGON_INTERACTIVE = 2;
        public const int LOGON32_PROVIDER_DEFAULT = 0;
        private IntPtr token = new IntPtr(0);
        private IntPtr tokenDuplicate = new IntPtr(0);

        /// <summary>
        /// 模拟应用上下文
        /// </summary>
        private WindowsImpersonationContext impersonationContext;

        /// <summary>
        /// 模拟指定的用户身份
        /// </summary>
        /// <param name="domain">登录域，本地计算机使用字符"."表示</param>
        /// <param name="userName">用户名</param>
        /// <param name="password">密码</param>
        /// <returns></returns>
        public bool TryLogonAs(string domain, string userName, string password)
        {
            WindowsIdentity tempWinID;
            //if (RevertToSelf())
            //{
                if (LogonUser(userName, domain, password, LOGON32_LOGON_INTERACTIVE, LOGON32_PROVIDER_DEFAULT, ref token) != 0)
                {
                    if (DuplicateToken(token, 2, ref tokenDuplicate) != 0)
                    {
                        tempWinID = new WindowsIdentity(tokenDuplicate);
                        impersonationContext = tempWinID.Impersonate();
                        return (impersonationContext != null);
                    }
                }
            //}
            return false;
        }

        #region IDisposable 成员

        /// <summary>
        /// 取消模拟
        /// </summary>
        public void Dispose()
        {
            if (impersonationContext != null)  
                    impersonationContext.Undo();

            if (token != IntPtr.Zero)
                CloseHandle(token);

            if (tokenDuplicate != IntPtr.Zero)
                CloseHandle(tokenDuplicate);
        }

        #endregion
    }


}