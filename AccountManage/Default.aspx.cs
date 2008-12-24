using System;
using System.Security.Principal;
using System.Collections.Specialized;

namespace SvnAccount.AccountManage
{
    public partial class HomePage : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            lblUser.Text = this.User.Identity.Name + "(" + User.Identity.AuthenticationType + ")";
            StringCollection ug = AccountHelper.GetUserGroup(".", this.User.Identity.Name);
            if (ug.IndexOf("Administrators") == -1)
            {
                hplAccount.Visible = false;
            }
        }
    }
}
