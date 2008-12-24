<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="SvnAccount.AccountManage.HomePage" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>SVN-WinAuth帐户管理系统</title>
        <style type="text/css">
        td, div, p {font-size:10.5pt; line-height:150%}
    </style>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <asp:Label ID="lblUser" runat="server"></asp:Label>
        已登录，请选择下列菜单操作。<br /><br />
        
        <asp:HyperLink ID="hplChg" runat="server" NavigateUrl="~/Password.aspx">修改登录密码</asp:HyperLink>
        <br /><br />
        <asp:HyperLink ID="hplAccount" runat="server" 
            NavigateUrl="~/AccountManage.aspx">管理系统帐户</asp:HyperLink>
    
    </div>
    </form>
</body>
</html>
