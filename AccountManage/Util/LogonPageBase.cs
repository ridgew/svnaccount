using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Security;
using System.Text;


namespace SvnAccount.AccountManage.Util
{
    public abstract class LogonPageBase : System.Web.UI.Page
    {
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            HttpCookie UserCookie = Request.Cookies["SvnAuth"];
            #region 验证当前用户信息
            if (Request.Headers["Authorization"] == null || UserCookie == null)
            {
                ToAuthenticate();
            }
            else
            {
                if (Request.Headers["Authorization"] != null)
                {
                    #region 设置认证凭据
                    string strAuth = Request.Headers["Authorization"];
                    if (strAuth.StartsWith("Basic ") && strAuth.Length > 6)
                    {
                        string accountInfo = Encoding.ASCII.GetString(Convert.FromBase64String(strAuth.Substring(6)));
                        if (accountInfo.Length < 3)
                        {
                            ToAuthenticate();
                        }
                        else
                        {
                            string[] IDPair = accountInfo.Split(':');
                            if (AccountHelper.TestAuth(IDPair[0], IDPair[1]))
                            {
                                HttpCookie cookie = new HttpCookie("SvnAuth");
                                cookie.Value = IDPair[0];
                                cookie.HttpOnly = true;
                                cookie.Secure = Request.IsSecureConnection;
                                cookie.Path = "/";
                                Response.Cookies.Add(cookie);
                            }
                            else
                            {
                                ToAuthenticate();
                            }
                        }
                    }
                    else
                    {
                        ToAuthenticate();
                    }
                    #endregion
                }

                try
                {
                    UserCookie = HttpSecureCookie.Decode(UserCookie);
                }
                catch (Exception)
                {
                    Response.Write("无效的Cookie信息");
                    Response.End();
                }
            }
            #endregion

        }


        private void ToAuthenticate()
        {
            Response.Clear();
            Response.Charset = "utf-8";
            Response.AppendHeader("WWW-Authenticate", "Basic realm=\"[SVN Account Verify]\"");
            Response.Status = "401 Authorization Required";
            Response.Write("401 Authorization Required");
            Response.End();
        }


    }
}
