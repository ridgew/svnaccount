<%@ Page language="C#" Codebehind="WebLogin.aspx.cs" AutoEventWireup="false" Inherits="NTLM.Account.WebLogin" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" >
<html>
<head>
	<title>SVN-WinAuth帐户管理系统</title>
</head>
<body>
<form id="MainForm" method="post" runat="server">
<table align="center" border="0">
<tr>
	<th colspan="2" align="center">请输入帐户信息登录</th>
</tr>
<tr>
	<td>用户名:</td>
	<td><asp:textbox id="UserName" runat="server" /></td>
</tr>
<tr>
	<td>登录密码:</td>
	<td><asp:textbox id="Password" runat="server" textmode="Password" /></td>
</tr>
<tr>
	<td colspan="2" align="center"><asp:button id="Login" runat="server" text="登录" />
        &nbsp;<asp:Label ID="lblMsg" runat="server" ForeColor="Red"></asp:Label>
    </td>
</tr>
</table>
</form>
</body>
</html>
