using System.Collections;
using System.Collections.Specialized;
using System.DirectoryServices;
using System;
using System.Configuration;
using System.Diagnostics;
using System.ServiceProcess;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.IO;
using System.Xml;

namespace SvnAccount.AccountManage
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


        /// <summary>
        /// 测试原始用户名和密码是否正确
        /// </summary>
        public static bool TestAuth(string username, string password)
        {
            #region Windows集成帐户认证
            if (!IsApacheAuthMode())
            {
                bool blnResult = false;
                using (IdentityAnalogue ID = new IdentityAnalogue())
                {
                    if (ID.TryLogonAs(".", username, password))
                    {
                        blnResult = true;
                    }
                }
                return blnResult;
            }
            #endregion

            string url = ConfigurationManager.AppSettings["AuthURL4Pass"];
            WebClient wc = new WebClient();
            WebHeaderCollection headers = new WebHeaderCollection();
            headers.Add(HttpRequestHeader.UserAgent, "Mozilla/5.0 (Windows; U; Windows NT 5.2; zh-CN; rv:1.8.1.19) Gecko/20081201 Firefox/2.0.0.19 (.NET CLR 3.5.30729)");
            wc.Headers = headers;

            wc.Credentials = new NetworkCredential(username, password);

            //System.Net.ServicePointManager.CertificatePolicy = new TrustAllCertificatePolicy(); 
            ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(
                delegate(object o, X509Certificate cert, X509Chain chain, SslPolicyErrors errors)
                {
                    return true;
                });

            try
            {

                MemoryStream ms = new MemoryStream();
                using (Stream rms = wc.OpenRead(url))
                {
                    int bt = rms.ReadByte();
                    while (bt != -1)
                    {
                        ms.WriteByte(Convert.ToByte(bt));
                        bt = rms.ReadByte();
                    }
                    rms.Close();
                }

                //Console.WriteLine("读取响应流完成，输出响应头...");
                //for (int i = 0; i < wc.ResponseHeaders.Count; i++)
                //{
                //    Console.WriteLine("{0}:{1}", wc.ResponseHeaders.AllKeys[i], wc.ResponseHeaders[i]);
                //}
                //Console.WriteLine(Encoding.UTF8.GetString(ms.ToArray()));

                ms.Close();
                ms.Dispose();

                return true;
            }
            catch (WebException)
            {
                return false;
            }
        }

        /// <summary>
        /// 判断是否存在Apache MD5认证的用户名
        /// </summary>
        public static bool IsExistsApacheUser(string username)
        {
            string htpasswdFile = ConfigurationManager.AppSettings["htpasswdFile"];
            bool found = false;
            using (StreamReader sr = new StreamReader(htpasswdFile))
            {
                string lineStr;
                while ((lineStr = sr.ReadLine()) != null)
                {
                    lineStr = lineStr.Trim();
                    if (lineStr.ToLower().StartsWith(username.ToLower() + ":"))
                    {
                        found = true;
                        break;
                    }
                }
                sr.Close();
            }
            return found;
        }

        /// <summary>
        /// 检查是否配置为Apache MD5认证
        /// </summary>
        /// <returns></returns>
        public static bool IsApacheAuthMode()
        {
            #region 检查配置
            /*
               -- Windows集成认证
               AuthType Basic
               AuthBasicProvider visualsvn
               AuthzVisualSVNAccessFile "D:/Repositories/authz-windows"
               AuthnVisualSVNUPN Off

               -- Apache MD5认证
               AuthType Basic
               AuthBasicProvider file
               AuthUserFile "D:/Repositories/htpasswd"
               AuthzSVNAccessFile "D:/Repositories/authz"
             
             */
            StreamReader sr = new StreamReader(ConfigurationManager.AppSettings["SvnServerConf"]);
            string lineStr, providerStr = string.Empty;
            bool found = false;
            while ((lineStr = sr.ReadLine()) != null)
            {
                if (found) break;

                lineStr = lineStr.Trim();
                if (lineStr.StartsWith("AuthBasicProvider"))
                {
                    providerStr = lineStr.Substring("AuthBasicProvider".Length + 1);
                    found = true;
                }

            }
            #endregion

            return (providerStr == "file");

        }

        public static void RestartApache()
        {
            ServiceController controller = new ServiceController(ConfigurationManager.AppSettings["ApacheService"]);
            if (controller.Status == ServiceControllerStatus.Running)
            {
                Console.WriteLine("正在运行，准备关闭...");
                controller.Stop();
                controller.WaitForStatus(ServiceControllerStatus.Stopped);
            }

            Console.WriteLine("正在重新开始运行...");
            controller.Start();
            controller.WaitForStatus(ServiceControllerStatus.Running);
            Console.WriteLine("重启完成！");
        }

        /// <summary>
        /// 正确修改/创建密码则返回字符结果为0,创建用户时请指定旧密码为NULL。
        /// </summary>
        public static string ApacheChangeOrCreateAccount(string username, string oldpassWord, string newPasword)
        {
            if (oldpassWord != null && !TestAuth(username, oldpassWord))
            {
                return "提供的帐户信息不正确！";
            }
            else
            {
                //禁止通过创建修改原有用户的密码
                if (oldpassWord == null && IsExistsApacheUser(username))
                {
                    return "创建用户失败，用户名[" + username + "]已存在！";
                }

                #region 密码更改/创建命令

                //C:\Program Files\VisualSVN Server\bin>htpasswd
                //Usage:
                //        htpasswd [-cmdpsD] passwordfile username
                //        htpasswd -b[cmdpsD] passwordfile username password

                //        htpasswd -n[mdps] username
                //        htpasswd -nb[mdps] username password
                // -c  Create a new file.
                // -n  Don't update file; display results on stdout.
                // -m  Force MD5 encryption of the password (default).
                // -d  Force CRYPT encryption of the password.
                // -p  Do not encrypt the password (plaintext).
                // -s  Force SHA encryption of the password.
                // -b  Use the password from the command line rather than prompting for it.
                // -D  Delete the specified user.
                //On Windows, NetWare and TPF systems the '-m' flag is used by default.
                //On all other systems, the '-p' flag will probably not work.

                #endregion

                string htpasswdPath = ConfigurationManager.AppSettings["htpasswdPath"];
                string htpasswdFile = ConfigurationManager.AppSettings["htpasswdFile"];
                string args = string.Format("-b {0} {1} {2}", htpasswdFile, username, newPasword);

                Process proc = new Process();
                ProcessStartInfo psInfo = new ProcessStartInfo(htpasswdPath, args);
                psInfo.UseShellExecute = false;
                psInfo.RedirectStandardError = true;
                psInfo.RedirectStandardOutput = true;
                psInfo.RedirectStandardInput = true;
                psInfo.WindowStyle = ProcessWindowStyle.Hidden;
                psInfo.WorkingDirectory = Path.GetDirectoryName(htpasswdPath);

                proc.StartInfo = psInfo;
                proc.Start();
                string result = "";
                while (!proc.HasExited)
                {
                    result = proc.StandardOutput.ReadToEnd().Replace("\r", "");
                    System.Threading.Thread.Sleep(500);
                }
                proc.Close();
                proc.Dispose();

                Console.WriteLine(result);

                return "0";
            }
        }


        /// <summary>
        /// 基于web.config模型的AppSettings设置
        /// </summary>
        /// <param name="configPath">配置文件路径，相对或完整路径。</param>
        /// <param name="key">健</param>
        /// <param name="Value">健的值</param>
        /// <returns>成功则为0，失败则返回异常信息。</returns>
        public static string SetAppSettings(string configPath, string key, string Value)
        {
            string configFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, configPath);
            try
            {
                System.Configuration.ConfigXmlDocument xmlConfig = new System.Configuration.ConfigXmlDocument();
                xmlConfig.Load(configFile);

                System.Xml.XmlNode node = xmlConfig.SelectSingleNode("configuration/appSettings/add[@key='" + key + "']");
                if (node != null)
                {
                    node.Attributes["value"].Value = Value;
                }
                else
                {
                    XmlElement element = xmlConfig.CreateElement("add");
                    element.SetAttribute("key", key);
                    element.SetAttribute("value", Value);
                    node = xmlConfig.SelectSingleNode("configuration/appSettings");
                    node.AppendChild(element);
                }
                xmlConfig.Save(configFile);
                return "0";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

    }
}
