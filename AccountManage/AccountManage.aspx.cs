using System;
using System.Collections;
using System.Collections.Specialized;
using System.Configuration;
using System.Data;
using System.DirectoryServices;
using System.Web;
using System.Web.UI.WebControls;

namespace NTLM.Account
{
    public partial class AccountManage : System.Web.UI.Page
    {
        private DataTable DataSource, groupTab;
        private bool IsEmptyFlag = false;
        private bool IsFetchedWindows = false;
        
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!HttpContext.Current.User.Identity.IsAuthenticated)
            {
                Response.Redirect("WinLogin.aspx", true);
                return;
            } 

            StringCollection ug = AccountHelper.GetUserGroup(".", this.User.Identity.Name);
            if (ug.IndexOf("Administrators") == -1)
            {
                Response.Write("没有权限！");
                Response.End();
            }

            if (!Page.IsPostBack)
            {
                InitializeTable();
                BindGridView();
                BindWinGroups();

                ViewState.Add("DS", DataSource);
                ViewState.Add("groups", groupTab);
            }
            else
            {
                DataSource = (DataTable)ViewState["DS"];
                groupTab = (DataTable)ViewState["groups"];
            }
        }

        private void BindWinGroups()
        {
            if (groupTab.Rows.Count > 1)
            {
                dptGroups.DataTextField = "Name";
                dptGroups.DataValueField = "Name"; //Description
                dptGroups.DataSource = groupTab;
                dptGroups.DataBind();

                try
                {
                    dptGroups.Items.FindByValue("Users").Selected = true;
                }
                catch (Exception)
                { 
                
                }
            }
        }

        private void BindGridView()
        {
            if (DataSource == null || DataSource.Rows.Count == 0)
            {
                #region 初始化一条空数据以显示表脚
                gridM.DataSource = CreateDataTableWithSimpleData("Users",
                    new string[] { "ID", "UserName", "FullName", "Description", "LastLogin" },
                    new object[] { 0, "", "", "", "" });
                IsEmptyFlag = true;
                gridM.DataBind();
                #endregion
            }
            else
            {
                gridM.DataSource = DataSource;
                gridM.DataBind();
            }
        }

        private void InitializeTable()
        {
            DataSource = CreateDataTableWithSimpleData("Users",
                    new string[] { "ID", "UserName", "FullName", "Description", "LastLogin" },
                    new object[] { 0, "", "", "", "" });

            groupTab = CreateDataTableWithSimpleData("Groups",
                    new string[] { "ID", "Name", "Description" },
                    new object[] { 0, "", "" });

            if (IsFetchedWindows != true)
            {
                FillWindowsUserAndGroups(ref DataSource, ref groupTab);
            }

            if (DataSource.Rows.Count > 1)
            {
                DataSource.Rows.RemoveAt(0);
            }

            if (groupTab.Rows.Count > 1)
            {
                groupTab.Rows.RemoveAt(0);
            }
        }

        private void FillWindowsUserAndGroups(ref DataTable user, ref DataTable group)
        {

            #region 获取所有组
            //DirectoryEntry m_obDirEntry = new DirectoryEntry("LDAP://" + Environment.MachineName);
            //DirectorySearcher srch = new DirectorySearcher(m_obDirEntry);
            ////srch.Filter = "(objectClass=Group)(guests)";
            //srch.Filter = "(objectClass=Group)";
            //SearchResultCollection results = srch.FindAll();
            //foreach (SearchResult src in results)
            //{
            //    Response.Write(src.GetDirectoryEntry().Name);
            //} 

            //oDE.Invoke("accountDisabled", new Object[] { "true" });

            //Creating Sites and Virtual Directories Using System.DirectoryServices 
            //(http://msdn.microsoft.com/library/default.asp?url=/library/en-us/iissdk/iis/creating_a_virtual_directory_using_system_directoryservices.asp)

            //Using System.DirectoryServices to Configure IIS 
            //http://go.microsoft.com/fwlink/?LinkId=48514
            //(http://msdn.microsoft.com/library/default.asp?url=/library/en-us/iissdk/iis/using_system_directoryservices_to_configure_iis.asp)
            #endregion
            
            DataRow dRow = DataSource.Rows[0];
            DirectoryEntry DirEntry = new DirectoryEntry("WinNT://" + Environment.MachineName);
            try
            {
                foreach (DirectoryEntry ChildEntry in DirEntry.Children)
                {
                    if (DirectoryServicesManager.DirectoryObjectIsOfType(ChildEntry.Path, DirectoryServicesManager.UserSchemaClassName))
                    {
                        user.Rows.Add(new object[] {

                            user.Rows.Count, ChildEntry.Name, 
                             ChildEntry.Properties["FullName"].Value,
                             ChildEntry.Properties["Description"].Value,
                             ChildEntry.Properties["LastLogin"].Value,
                        });
                        //Response.Write(ChildEntry.SchemaEntry.Path);
                        //Response.Write("<br/>");
                    }

                    if (DirectoryServicesManager.DirectoryObjectIsOfType(ChildEntry.Path, DirectoryServicesManager.GroupSchemaClassName))
                    {
                        groupTab.Rows.Add(new object[] {

                            groupTab.Rows.Count, ChildEntry.Name,
                             ChildEntry.Properties["Description"].Value
                        });
                    }
                }
            }
            catch (Exception)
            {

            }
            finally
            {
                DirEntry.Close();
                IsFetchedWindows = true;
            }
        }

        protected void gridM_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            string UserName = gridM.DataKeys[e.RowIndex]["UserName"].ToString();
            DataRow dRow = DataSource.Rows[e.RowIndex];
            if (dRow["Description"].ToString().IndexOf("SVN-WinAuth") == -1)
            {
                lblMsg.Text = "非SVN-WinAuth帐号，请登录控制台修改！";
            }
            else
            {
                AccountHelper.RunAdminCode(new AccountHelper.ExecuteCode(delegate()
                {
                    DirectoryServicesManager.DeleteWindowsUser("WinNT://" + Environment.MachineName,
                        dRow["UserName"].ToString());
                    lblMsg.Text = "操作成功！";
                    lblMsg.ForeColor = System.Drawing.Color.Green;

                    UpdateView();

                }));
            }
        }

        protected void gridM_RowEditing(object sender, GridViewEditEventArgs e)
        {
            gridM.EditIndex = e.NewEditIndex;
            BindGridView();

            DataRow dRow = DataSource.Rows[e.NewEditIndex];
            TextBox tbxFullName = (TextBox)gridM.Rows[e.NewEditIndex].FindControl("tbxFullName");
            if (tbxFullName != null)
            {
                tbxFullName.Text = dRow["FullName"].ToString();
            }
        }

        protected void gridM_RowCreated(object sender, GridViewRowEventArgs e)
        {
            Button btnDel = (Button)e.Row.Cells[e.Row.Cells.Count - 1].FindControl("btnDel");
            if (btnDel != null)
            {
                btnDel.Attributes.Add("onclick", "return confirm(\"确认要删除吗?\");");
            }
        }

        protected void gridM_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            DataRow dRow = DataSource.Rows[e.RowIndex];
            TextBox tbxNewPwd = (TextBox)gridM.Rows[e.RowIndex].FindControl("tbxNewPwd");
            TextBox tbxNewPwdCfm = (TextBox)gridM.Rows[e.RowIndex].FindControl("tbxNewPwdCfm");
            TextBox tbxFullName = (TextBox)gridM.Rows[e.RowIndex].FindControl("tbxFullName");

            Response.Write(dRow["UserName"].ToString());
            Response.Write("<br/>");
            Response.Write(tbxNewPwdCfm.Text.Trim());
            //return;

            AccountHelper.RunAdminCode(new AccountHelper.ExecuteCode(delegate()
            {
                string currentUser = dRow["UserName"].ToString();

                if (currentUser.IndexOf(Environment.MachineName) != -1)
                {
                    currentUser = currentUser.Replace("\\", "/");
                }
                else
                {
                    currentUser = Environment.MachineName + "/" + currentUser;
                }

                DirectoryEntry uEntry = new DirectoryEntry("WinNT://" + currentUser);
                if (uEntry != null && tbxFullName != null
                    && tbxNewPwd != null && tbxNewPwdCfm != null &&
                    tbxNewPwd.Text == tbxNewPwdCfm.Text)
                {
                    try
                    {
                        uEntry.Properties["FullName"].Value = tbxFullName.Text;
                        if (tbxNewPwd.Text.Length > 0)
                        {
                            //修改密码
                            uEntry.Invoke("SetPassword", tbxNewPwdCfm.Text.Trim());
                        }
                        uEntry.CommitChanges();

                        lblMsg.Text = "修改成功！";
                        lblMsg.ForeColor = System.Drawing.Color.Green;
                    }
                    catch (Exception exp)
                    {
                        lblMsg.Text = "错误：" + exp.Message;
                    }
                }
                else
                {
                    lblMsg.Text = "错误：页面控件丢失！";
                }

            }));

            UpdateView();
        }

        private void UpdateView()
        {
            IsFetchedWindows = false;
            InitializeTable();

            gridM.EditIndex = -1;
            BindGridView();

            BindWinGroups();
        }

        protected void gridM_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            gridM.EditIndex = -1;
            BindGridView();
        }

        protected void gridM_DataBound(object sender, EventArgs e)
        {
            if (IsEmptyFlag == true)
            {
                Button btnEdit = (Button)gridM.Rows[0].FindControl("btnEdit");
                if (btnEdit != null) btnEdit.Enabled = false;

                Button btnDelete = (Button)gridM.Rows[0].FindControl("btnDel");
                if (btnDelete != null)  btnDelete.Enabled = false;
            }
        }

        /// <summary>
        /// 创建一个内容数据表格
        /// </summary>
        /// <param name="tableName">表格名称</param>
        /// <param name="columnNames">列名集合</param>
        /// <param name="firstRowData">第一行的值</param>
        /// <remarks>应用范围：填充GridView的数据源，已显示gridM. FooterItem。</remarks>
        public static DataTable CreateDataTableWithSimpleData(string tableName, string[] columnNames, object[] firstRowData)
        {
            DataTable emptyTab = new DataTable(tableName);
            if (columnNames.Length != firstRowData.Length)
            {
                throw new InvalidOperationException("数据和列长度不一致！");
            }
            else
            {
                for (int i = 0, j = columnNames.Length; i < j; i++)
                {
                    emptyTab.Columns.Add(new DataColumn(columnNames[i], firstRowData[i].GetType()));
                }
                DataRow dRow = emptyTab.NewRow();
                dRow.ItemArray = firstRowData;
                emptyTab.Rows.Add(dRow);

                return emptyTab;
            }
        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            if (tbxPassword.Text != tbxPwdCfm.Text)
            {
                lblMsg.Text = "两次密码确认不一致！";
                return;
            }

            if (tbxUserAdd.Text.Trim() == string.Empty)
            {
                return;
            }

           string strGroup = (tbxGroupAdd.Text.Trim() != string.Empty) ? tbxGroupAdd.Text.Trim() : dptGroups.SelectedValue;
           try
           {
               AccountHelper.RunAdminCode(new AccountHelper.ExecuteCode(delegate()
               {
                   if (tbxGroupAdd.Text.Trim() != string.Empty)
                   {
                       DirectoryServicesManager.AddWindowsGroup("WinNT://" + Environment.MachineName,
                           strGroup, "[SVN-WinAuth]创建的分组", 4);
                   }
                   AccountHelper.CreateUserAccount(tbxUserAdd.Text, tbxPwdCfm.Text,
                       tbxUserAdd.Text, strGroup,
                       "[SVN-WinAuth]创建的用户");

                   lblMsg.ForeColor = System.Drawing.Color.Green;
                   lblMsg.Text = "创建完成！";

                   UpdateView();

               }));
           }
           catch (Exception exp)
           {
               lblMsg.Text = "错误：" + exp.Message;
           }
        }

    }
}
