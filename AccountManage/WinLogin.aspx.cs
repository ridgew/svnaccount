using System;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

namespace NTLM.Account
{
	public class WinLogin : System.Web.UI.Page
	{
		private void Page_Load(object sender, System.EventArgs e) {
			string userName = this.Request.ServerVariables["LOGON_USER"];
			FormsAuthentication.RedirectFromLoginPage(userName, false);
		}

		#region Web Form Designer generated code
		override protected void OnInit(EventArgs e) {
			InitializeComponent();
			base.OnInit(e);
		}
		
		private void InitializeComponent() {    
			this.Load += new System.EventHandler(this.Page_Load);
		}
		#endregion
	}
}
