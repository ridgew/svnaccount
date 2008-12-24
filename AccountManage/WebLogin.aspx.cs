using System;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

namespace SvnAccount.AccountManage
{
	public class WebLogin : System.Web.UI.Page
	{
		protected System.Web.UI.WebControls.TextBox UserName;
		protected System.Web.UI.WebControls.TextBox Password;
		protected System.Web.UI.WebControls.Button Login;
        protected System.Web.UI.WebControls.Label lblMsg;
	
		private void Page_Load(object sender, System.EventArgs e) {
			// Put user code to initialize the page here
		}

		#region Web Form Designer generated code
		override protected void OnInit(EventArgs e) {
			InitializeComponent();
			base.OnInit(e);
		}
		
		private void InitializeComponent() {    
			this.Login.Click += new System.EventHandler(this.Login_Click);
			this.Load += new System.EventHandler(this.Page_Load);

		}
		#endregion

		private void Login_Click(object sender, System.EventArgs e) 
        {
            using (IdentityAnalogue ID = new IdentityAnalogue())
            {
                if (ID.TryLogonAs(".", UserName.Text, Password.Text))
                {
                    string userName = UserName.Text;
                    FormsAuthentication.SetAuthCookie(userName, false);

                    string returnUrl;
                    if (this.Request.Cookies[Global.ReturnUrl] == null)
                    {
                        returnUrl = this.Request.ApplicationPath;
                    }
                    else
                    {
                        returnUrl = this.Request.Cookies[Global.ReturnUrl].Value;
                    }
                    this.Response.Redirect(returnUrl);
                }
                else
                {
                    lblMsg.Visible = true;
                    lblMsg.Text = "*µÇÂ¼Ê§°Ü";
                }
            }
		}
	}
}
