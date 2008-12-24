using System;
using System.Web;
using System.Web.SessionState;

namespace SvnAccount.AccountManage 
{
	public class Global : System.Web.HttpApplication
	{
		public const string ReturnUrl = "MixedSecurity.ReturnUrl";

		protected void Application_Start(Object sender, EventArgs e) {}
		protected void Session_Start(Object sender, EventArgs e) {}
		protected void Application_BeginRequest(Object sender, EventArgs e) {}

		protected void Application_AuthenticateRequest(Object sender, EventArgs e) {
			if (!this.Request.IsAuthenticated) {
				int start = this.Request.Path.LastIndexOf("/");
				string path = this.Request.Path.Substring(start + 1);
				if (path.ToUpper() != "WEBLOGIN.ASPX") {
					this.Response.Cookies[Global.ReturnUrl].Value = this.Request.Path;
				}
			}

            //System.Text.StringBuilder sb = new System.Text.StringBuilder();
            //foreach (string s in Request.Headers)
            //{
            //    sb.AppendFormat("{0}={1}\n", s, Request.Headers[s]);
            //}
            //foreach (string s in Request.ServerVariables)
            //{
            //    sb.AppendFormat("{0}={1}\n", s, Request.ServerVariables[s]);
            //}
            ////Response.Write(sb.ToString());

            //System.IO.StreamWriter sw = new System.IO.StreamWriter(Server.MapPath("debug.log"));
            //sw.Write(sb.ToString());
            //sw.Close();
            //sw.Dispose();
		}

		protected void Application_AuthorizeRequest(Object sender, EventArgs e) {}
		protected void Application_EndRequest(Object sender, EventArgs e) {}
		protected void Session_End(Object sender, EventArgs e) {}
		protected void Application_End(Object sender, EventArgs e) {}
		protected void Application_Error(Object sender, EventArgs e) {}
	}
}

