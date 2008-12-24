using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.DirectoryServices;

public partial class WebForm1 : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {

    }
    //列出所有用户信息
    protected void btnListAllUser_Click(object sender, EventArgs e)
    {
        DirectoryEntry AD = new DirectoryEntry(@"WinNT://" + Environment.MachineName + ",computer");
        foreach (DirectoryEntry child in AD.Children)
        {
            //列出所有用户信息
            switch (child.SchemaClassName)
            {
                case "User":
                    try
                    {
                        //列出用户信息
                        DirectoryEntry entryUser = new DirectoryEntry("WinNT://" + Environment.MachineName + "/" + child.Name + ",User");
                        Response.Write("<br>");
                        Response.Write("&nbsp;" + entryUser.Name);
                        Response.Write("<br>");
                        Response.Write("&nbsp;" + "&nbsp;" + entryUser.Properties["Description"].Value);
                        Response.Write("<br>");
                        Response.Write("<br>");
                        Response.Write("<br>");

                    }
                    catch (Exception ex)
                    {
                        Response.Write("发生错误:&nbsp;" + ex.Message);
                        Response.Write("<br>");
                    }
                    finally
                    {

                    }
                    break;
            }
        }

    }

    //列出一组中的成员
    protected void btnGroupUser_Click(object sender, EventArgs e)
    {
        string GroupName = "Users"; //组名
        //string GroupName = "Administrators";
        DirectoryEntry entryGroup = new DirectoryEntry(@"WinNT://" + Environment.MachineName + "/" + GroupName + ",Group");
        Object members = entryGroup.Invoke("Members", null);
        try
        {
            foreach (object member in (IEnumerable)members)
            {
                DirectoryEntry x = new DirectoryEntry(member);
                Response.Write(x.Name + "<br>");                //用户名称
                try
                {
                    Response.Write("&nbsp;" + "&nbsp;" + x.Properties["Description"].Value);    //用户描述
                    Response.Write(x.Name + "<br>");                //用户名称
                }
                catch
                {

                }
            }

        }
        catch (Exception ex)
        {
            Response.Write("发生错误:&nbsp;" + ex.Message + "<br>");
        }
        finally
        {

        }


    }

    //AD所有成员
    protected void btnAllChildren_Click(object sender, EventArgs e)
    {
        DirectoryEntry AD = new DirectoryEntry(@"WinNT://" + Environment.MachineName + ",computer");
        foreach (DirectoryEntry child in AD.Children)
        {

            //这里会列出所有组和服务的信息
            Response.Write(child.Name);
            Response.Write("<br>");
            Response.Write(child.SchemaClassName);
            Response.Write("<br>");
            Response.Write("<br>");
        }

    }

    //添加用户
    protected void btnAddUser_Click(object sender, EventArgs e)
    {
        try
        {
            DirectoryEntry AD = new DirectoryEntry(@"WinNT://" + Environment.MachineName + ",computer");
            //添加用户，用户名：NewUser
            DirectoryEntry NewUser = AD.Children.Add("NewUser", "User");
            //设置密码，密码：mypassword
            NewUser.Invoke("SetPassword", new object[] { "mypassword" });
            NewUser.Invoke("Put", new object[] { "Description", "myDescription" });
            //提交修改
            NewUser.CommitChanges();

            //将用户添加到users组
            object[] objNewUser = new object[] { NewUser.Path };
            DirectoryEntry groupUser = AD.Children.Find("Users", "group");
            groupUser.Invoke("Add", objNewUser);
        }
        catch (Exception ex)
        {
            Response.Write("添加用户时发生错误：" + ex.Message + "<br>");

        }
    }

    //修改用户密码
    protected void btnChangpwd_Click(object sender, EventArgs e)
    {
        DirectoryEntry AD = new DirectoryEntry(@"WinNT://" + Environment.MachineName + ",computer");
        DirectoryEntry Cuser = AD.Children.Find("NewUser");
        try
        {
            Cuser.Invoke("SetPassword", new object[] { "myNewpassword" });
            Cuser.CommitChanges();
        }
        catch (Exception ex)
        {
            Response.Write("修改密码时发生错误：" + ex.Message + "<br>");
        }
    }

    //把用户从组中移除
    protected void btnRemoveUserfromGroup_Click(object sender, EventArgs e)
    {
        DirectoryEntry AD = new DirectoryEntry(@"WinNT://" + Environment.MachineName + ",computer");
        DirectoryEntry entryUser = AD.Children.Find("NewUser", "User");
        object[] objUser = new object[] { entryUser.Path };
        try
        {
            //查找users组
            DirectoryEntry grpUsers = AD.Children.Find("Users", "group");
            //从User组中移除
            grpUsers.Invoke("remove", objUser);
        }
        catch (Exception ex)
        {
            Response.Write("将用户从组中移除时发生错误：" + ex.Message + "<br>");
        }
    }

    //删除用户
    protected void btnDeleteUser_Click(object sender, EventArgs e)
    {
        DirectoryEntry AD = new DirectoryEntry(@"WinNT://" + Environment.MachineName + ",computer");
        try
        {
            DirectoryEntry ChildUser = AD.Children.Find("NewUser", "User");
            if (ChildUser.Name != null && ChildUser.Name != "")
            {
                AD.Children.Remove(ChildUser);
            }
        }
        catch (Exception ex)
        {
            Response.Write("将用户删除时发生错误：" + ex.Message + "<br>");
        }
    }
}
