using System.Collections;
using System.Collections.Specialized;
using System.DirectoryServices;
using System;
using System.Configuration;

namespace NTLM.Account
{
    public static class AccountHelper
    {
        /// <summary>
        /// 获取用户所在的组名
        /// </summary>
        /// <param name="strDomain">机器或域名</param>
        /// <param name="strUser">用户名</param>
        /// <returns></returns>
        public static StringCollection GetUserGroup(string strDomain, string strUser)
        {
            if (strUser.IndexOf(Environment.MachineName) != -1)
            {
                strUser = strUser.Replace("\\", "/");
            }
            else
            {
                strUser = Environment.MachineName + "/" + strUser;
            }
            StringCollection userGps = new StringCollection();
            DirectoryEntry obDirEnt = new DirectoryEntry("WinNT://" + strUser);
            object obGps = obDirEnt.Invoke("Groups");
            if (null != obGps)
            {
                foreach (object obGp in (IEnumerable)obGps)
                {
                    DirectoryEntry obGpEnt = new DirectoryEntry(obGp);
                    userGps.Add(obGpEnt.Name);
                }
            }
            return userGps;
        }

        /// <summary>
        /// 获取用户组所有的用户
        /// </summary>
        /// <param name="strDomain">机器或域名</param>
        /// <param name="strGroup">用户组名</param>
        /// <returns></returns>
        public static StringCollection GetGroupUsers(string strDomain, string strGroup)
        {
            StringCollection oUsers = new StringCollection();
            DirectoryEntry obDirEnt = new DirectoryEntry("WinNT://" + strDomain + "/" + strGroup);
            object obGps = obDirEnt.Invoke("Members");
            if (null != obGps)
            {
                foreach (object obGp in (IEnumerable)obGps)
                {
                    DirectoryEntry obGpEnt = new DirectoryEntry(obGp);
                    oUsers.Add(obGpEnt.Name);
                }
            }
            return oUsers;
        }

        /*
        * SetPassword' requires admin rights to execute - which is not something you probably want to do. 
        * 'ChangePassword' does not and can be used by the end user themselves.  
        * It takes the old password and new password as arguments (do a search in this forum for 'ChangePassword' to see examples).
        * This would be the preferred way of executing this and it would also verify their identity for you without a database 
        * lookup (or at least verify that the user knows their old password).
        */
        public static void CreateUserAccount(string login, string password, string fullName, string groupName, string description)
        {
            DirectoryEntry dirEntry = new DirectoryEntry("WinNT://" + Environment.MachineName);
            DirectoryEntries entries = dirEntry.Children;
            DirectoryEntry newUser = entries.Add(login, "user");
            newUser.Properties["FullName"].Add(fullName);
            if (description != null)
            {
                newUser.Properties["Description"].Add(description);
            }
            newUser.Invoke("SetPassword", password);
            newUser.CommitChanges();

            if (groupName == null) groupName = "Guests";
            DirectoryEntry grp = dirEntry.Children.Find(groupName, "group");
            //DirectoryEntry grp = new DirectoryEntry("WinNT://" + Environment.MachineName + "/" + groupName);
            if (grp != null) { grp.Invoke("Add", new object[] { newUser.Path.ToString() }); }
            grp.CommitChanges();

        }

        public delegate void ExecuteCode();

        /// <summary>
        /// 以管理员身份运行相关代码
        /// </summary>
        /// <param name="exec"></param>
        /// <returns></returns>
        public static bool RunAdminCode(ExecuteCode exec)
        {
            bool blnResult = false;
            using (IdentityAnalogue ID = new IdentityAnalogue())
            {
                if (ID.TryLogonAs(".", ConfigurationManager.AppSettings["AnalogueID"],
                    new SymmetricMethod().Decrypto(ConfigurationManager.AppSettings["AnaloguePWD"])))
                {
                    exec();
                    blnResult = true;
                }
            }
            return blnResult;
        }
    }
}
