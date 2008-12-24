using System;
using System.Configuration;
using System.DirectoryServices;
using System.Web;

namespace SvnAccount.AccountManage
{
    public partial class Password : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void btnModify_Click(object sender, EventArgs e)
        {
            if (tbxNewPwd.Text != tbxNewPwdCfm.Text)
            {
                lblMsg.Text = "两次密码不匹配！";
                return;
            }

            AccountHelper.RunAdminCode(new AccountHelper.ExecuteCode(delegate()
            {
                string currentUser = HttpContext.Current.User.Identity.Name;
                if (currentUser.IndexOf(Environment.MachineName) != -1)
                {
                    currentUser = currentUser.Replace("\\", "/");
                }
                else
                {
                    currentUser = Environment.MachineName + "/" + currentUser;
                }

                DirectoryEntry uEntry = new DirectoryEntry("WinNT://" + currentUser);
                if (uEntry != null && tbxNewPwd.Text == tbxNewPwdCfm.Text)
                {
                    try
                    {
                        //管理员权限操作
                        uEntry.Invoke("SetPassword", tbxNewPwdCfm.Text);
                        //uEntry.Invoke("ChangePassword", new object[] { tbxOldPwd.Text, tbxNewPwdCfm.Text });
                        uEntry.CommitChanges();

                        lblMsg.Text = "修改成功！";
                        lblMsg.ForeColor = System.Drawing.Color.Green;
                    }
                    catch (Exception exp)
                    {
                        lblMsg.Text = "错误：" + exp.Message;
                    }
                }

            }));
        }
    }
}
