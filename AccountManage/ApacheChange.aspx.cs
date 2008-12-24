using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.DirectoryServices;
using System.Text;
using System.IO;
using System.Configuration;
using System.ServiceProcess;
using System.Diagnostics;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;

namespace NTLM.Account
{
    public partial class ApacheChange : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            //username
            //oldpwd newpwd newpwdCfg
            //ApacheChange.aspx?username=wangqj&oldpwd=2&newpwd=9119&newpwdcfm=9119

            #region 管理员配置密码加解密
            if (Request["PwdGen"] != null)
            {
                Response.Write(new SymmetricMethod().Encrypto(Request["PwdGen"]));
                Response.End();
                return;
            }
            else if (Request["PwdDen"] != null)
            {
                Response.Write(new SymmetricMethod().Decrypto(Request["PwdDen"]));
                Response.End();
                return;
            } 
            #endregion


            #region Test Code
            //StringBuilder sb = new StringBuilder();
            //foreach (string s in Request.Form)
            //{
            //    sb.AppendFormat("{0}={1}\n", s, Request.Form[s]);
            //}
            //foreach (string s in Request.ServerVariables)
            //{
            //    sb.AppendFormat("{0}={1}\n",s, Request.ServerVariables[s]);
            //}
            ////Response.Write(sb.ToString());

            //System.IO.StreamWriter sw = new System.IO.StreamWriter(Server.MapPath("debug.log"));
            //sw.Write(sb.ToString());
            //sw.Close();
            //sw.Dispose();

            //Response.Write("0");
            //Response.End(); 
            #endregion

            string username = Request["username"];
            string oldpwd = Request["oldpwd"];
            string newpwd = Request["newpwd"];
            string newpwdcfm = Request["newpwdcfm"];

            string pwd = Request["pwd"];
            string pwdcfm = Request["pwdcfm"];

            if (username == null)
            {
                Response.Write("SVN-Auth Account API Page.");
                return;
            }

            #region 禁止修改用户名处理
            if (("," + ConfigurationManager.AppSettings["DisabledModifyName"] + ",").ToLower()
                    .Contains("," + username.ToLower() + ","))
            {
                Response.Write("用户名[" + username + "]已被禁止在线修改！");
                Response.End();
                return;
            } 
            #endregion

            bool ApacheAuthMode = IsApacheAuthMode();
 
            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(oldpwd)
                && !string.IsNullOrEmpty(newpwd)
                && newpwd == newpwdcfm)
            {
                #region 修改密码 
                bool blnResult = false;
                if (!ApacheAuthMode)
                {
                    using (IdentityAnalogue ID = new IdentityAnalogue())
                    {
                        if (ID.TryLogonAs(".", username, oldpwd))
                        {
                            blnResult = true;
                        }
                    }
                }
                else
                {
                    //blnResult = TestAuth(username, oldpwd);
                    //在修改时检查原始密码
                    blnResult = true;
                }

                if (blnResult == false)
                {
                    Response.Write("用户密码不正确！");
                }
                else
                {
                    AccountHelper.RunAdminCode(new AccountHelper.ExecuteCode(delegate()
                    {
                        if (!ApacheAuthMode)
                        {
                            string currentUser = currentUser = Environment.MachineName + "/" + username;
                            DirectoryEntry uEntry = new DirectoryEntry("WinNT://" + currentUser);
                            try
                            {
                                uEntry.Invoke("SetPassword", newpwdcfm);
                                uEntry.CommitChanges();

                                Response.Write("0");
                            }
                            catch (Exception exp)
                            {
                                Response.Write("错误：" + exp.Message);
                            }
                        }
                        else
                        {
                            string strChangeResult = ChangeOrCreateAccount(username, oldpwd, newpwdcfm);
                            if (strChangeResult != "0")
                            {
                                Response.Write("错误：" + strChangeResult);
                            }
                            else
                            {
                                Response.Write("0");
                            }
                        }

                    }));

                } 
                #endregion
            }
            else if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(pwd)
                && pwd == pwdcfm)
            {
                #region 创建账号
                if (!ApacheAuthMode)
                {
                    try
                    {
                        AccountHelper.RunAdminCode(new AccountHelper.ExecuteCode(delegate()
                            {
                                AccountHelper.CreateUserAccount(username, pwd, "",
                                    "Users", "[SVN-WinAuth]创建的用户");
                            }));

                        Response.Write("0");
                    }
                    catch (Exception  exp)
                    {
                        Response.Write("错误：" + exp.Message);
                    }
                }
                else
                {
                    string strCreateResult = ChangeOrCreateAccount(username, null, pwd);
                    if (strCreateResult != "0")
                    {
                        Response.Write("错误：" + strCreateResult);
                    }
                    else
                    {
                        Response.Write("0");
                    }
                }
                #endregion
            }
            else
            {
                Response.Write("请提供有效帐户信息！");
            }
            Response.End();

            

        }

        /// <summary>
        /// 检查是否配置为Apache MD5认证
        /// </summary>
        /// <returns></returns>
        private bool IsApacheAuthMode()
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
        /// 测试原始用户名和密码是否正确
        /// </summary>
        public static bool TestAuth(string username, string password)
        {
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
        /// 正确修改/创建密码则返回字符结果为0,创建用户时请指定旧密码为NULL。
        /// </summary>
        public static string ChangeOrCreateAccount(string username, string oldpassWord, string newPasword)
        {
            if (oldpassWord !=null && !TestAuth(username, oldpassWord))
            {
                return "提供的帐户信息不正确！";
            }
            else
            {
                //禁止通过创建修改原有用户的密码
                if (oldpassWord == null && IsExistsApacheUser(username))
                {
                    return "创建用户失败，用户名[" + username +"]已存在！";
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

    }
}
