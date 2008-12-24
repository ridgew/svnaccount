<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Password.aspx.cs" Inherits="NTLM.Account.Password" %>
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
    <strong>修改登录密码</strong>
         <hr size="1" noshade="noshade" width="75%" align="left" />
        <!--//
        旧密码：<asp:TextBox ID="tbxOldPwd" runat="server" TextMode="Password"></asp:TextBox>
        -->
        <br />
        新密码：<asp:TextBox ID="tbxNewPwd" runat="server" TextMode="Password"></asp:TextBox>
        <br />
        密码确认：<asp:TextBox ID="tbxNewPwdCfm" runat="server" TextMode="Password"></asp:TextBox>
            <br /><br />
            <asp:Button ID="btnModify" runat="server" Text="更新" onclick="btnModify_Click" />
            &nbsp;<asp:Label ID="lblMsg" runat="server" ForeColor="Red"></asp:Label>
            <input type="button" value="关闭窗口" onclick="javascript:window.opener=null;window.close();" />
    </div>
    </form>
</body>
</html>
