<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="AccountManage.aspx.cs" Inherits="NTLM.Account.AccountManage" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>帐户管理面板</title>
    <style type="text/css">
        td, div, p {font-size:10.5pt; line-height:150%}
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <asp:Panel ID="palNewUser" runat="server">
        增加:用户名<asp:TextBox ID="tbxUserAdd" runat="server" Columns="10" />
        密码<asp:TextBox ID="tbxPassword" runat="server" Columns="10" TextMode="Password" />
            确认密码<asp:TextBox ID="tbxPwdCfm" runat="server" Columns="10" 
                TextMode="Password" />
            <br />
        用户组：<asp:TextBox ID="tbxGroupAdd" runat="server" Columns="8" />(新组)<br />
                    选择组：<asp:DropDownList ID="dptGroups" runat="server" Width="200">
                    </asp:DropDownList>
            <asp:Button ID="btnSave" runat="server" Text="保存资料" onclick="Button1_Click" />
            &nbsp;<asp:Label ID="lblMsg" runat="server" ForeColor="Red"></asp:Label>
        </asp:Panel>
        <br />
        <asp:GridView ID="gridM" runat="server" AutoGenerateColumns="False" DataKeyNames="UserName" 
            BackColor="White" BorderColor="#999999" BorderStyle="None" BorderWidth="1px" 
            CellPadding="3" GridLines="Vertical" ShowFooter="false" AllowPaging="True" onrowdeleting="gridM_RowDeleting" 
                ondatabound="gridM_DataBound" onrowcancelingedit="gridM_RowCancelingEdit" 
                onrowcreated="gridM_RowCreated" onrowediting="gridM_RowEditing" 
                onrowupdating="gridM_RowUpdating">
            <PagerSettings Mode="NumericFirstLast" />
            <RowStyle BackColor="#EEEEEE" ForeColor="Black" />
            <Columns>
                <asp:TemplateField HeaderText="序号">
                 <ItemTemplate>
                   <%#Eval("ID") %>
                 </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="用户名">
                 <ItemTemplate>
                   <%#Eval("UserName") %>
                 </ItemTemplate>
                 <EditItemTemplate>
                    <%#Eval("UserName") %><br />
                    密码修改:<asp:TextBox ID="tbxNewPwd" Rows="5" runat="server" TextMode="Password" />
                    <asp:TextBox ID="tbxNewPwdCfm" Rows="5" runat="server" TextMode="Password" />
                 </EditItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="全称">
                 <ItemTemplate>
                    <%#Eval("FullName") %>
                 </ItemTemplate>
                 <EditItemTemplate>
                    <asp:TextBox ID="tbxFullName" Rows="15" runat="server" />
                 </EditItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="描述">
                 <ItemTemplate>
                    <%#Eval("Description") %>
                 </ItemTemplate>
                 <EditItemTemplate>
                    <%#Eval("Description") %>
                 </EditItemTemplate>
                 </asp:TemplateField>
                <asp:TemplateField HeaderText="上次登录时间">
                 <ItemTemplate>
                    <%#Eval("LastLogin") %>
                 </ItemTemplate>
                 </asp:TemplateField>
                <asp:TemplateField HeaderText="杂 项">
                 <ItemTemplate>
                    <asp:Button ID="btnEdit" CommandName="Edit" Text="编辑" runat="server" />
                    <asp:Button ID="btnDel" CommandName="Delete" Text="删除" runat="server" />
                 </ItemTemplate>
                 <EditItemTemplate>
                   <asp:Button ID="btnUpdate" CommandName="Update" Text="更新" runat="server" />
                    <asp:Button ID="btnCancel" CommandName="Cancel" Text="取消" runat="server" />
                 </EditItemTemplate>
                </asp:TemplateField>
            </Columns>
            <FooterStyle BackColor="#CCCCCC" ForeColor="Black" />
            <PagerStyle BackColor="#999999" ForeColor="Black" HorizontalAlign="Center" />
            <SelectedRowStyle BackColor="#008A8C" Font-Bold="True" ForeColor="White" />
            <HeaderStyle BackColor="#000084" Font-Bold="True" ForeColor="White" />
            <AlternatingRowStyle BackColor="#DCDCDC" />
        </asp:GridView>
    
        
    </form>
</body>
</html>
