using System;
using System.Configuration;
using System.DirectoryServices;

namespace SvnAccount.AccountManage
{
    public partial class ApacheChange : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

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

            bool ApacheAuthMode = AccountHelper.IsApacheAuthMode();
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
                            string strChangeResult = AccountHelper.ApacheChangeOrCreateAccount(username, oldpwd, newpwdcfm);
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
                    string strCreateResult = AccountHelper.ApacheChangeOrCreateAccount(username, null, pwd);
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

    }
}
