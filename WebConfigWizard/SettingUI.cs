using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Management;
using System.ServiceProcess;
using System.Threading;

namespace SvnAccount.WebConfigWizard
{
    public partial class SettingUI : Form
    {
        public SettingUI()
        {
            InitializeComponent();
            TryGetVisualSVNPath();
        }

        private void TryGetVisualSVNPath()
        {
            //string sql = "SELECT PathName from Win32_Service where DisplayName =" + "\"VisualSVN Server\"";
            string svnSName = "VisualSVNServer";
            if (tbxVSVNSName.Text.Trim() != string.Empty)
            {
                svnSName = tbxVSVNSName.Text.Trim();
            }
            //sql = "SELECT PathName from Win32_Service where Name =\"" + ConfigurationManager.AppSettings["ApacheService"] + "\"";
            string sql = "SELECT PathName from Win32_Service where Name =\"" + svnSName + "\"";
            using (ManagementObjectSearcher Searcher = new ManagementObjectSearcher(sql))
            {
                foreach (ManagementObject service in Searcher.Get())
                {
                    tbxSvnWebRoot.Text = service["PathName"].ToString();
                    break;
                }
            }
        }

        private void btnSaveDo_Click(object sender, EventArgs e)
        {

        }

        private void btnResetSevices_Click(object sender, EventArgs e)
        {
           echo("重启服务，请稍等...");

           Thread UIThread = new Thread(new ThreadStart(delegate()
                {

                    ServiceController controller = new ServiceController(tbxVSVNSName.Text.Trim());
                    if (controller.Status == ServiceControllerStatus.Running)
                    {
                        echo("正在运行，准备关闭...");
                        controller.Stop();
                        controller.WaitForStatus(ServiceControllerStatus.Stopped);
                    }

                    echo("正在重新开始运行...");
                    controller.Start();
                    controller.WaitForStatus(ServiceControllerStatus.Running);
                    echo("重启完成！");
                    //echo(null);


                }));

           UIThread.IsBackground = true;
           UIThread.Start();
           UIThread.Join();
        }

        private void echo(string msg)
        {
            if (msg == null)
            {
                lblMsg.Visible = false;
                lblMsg.Text = "";
                return;
            }

            if (lblMsg.Visible == false) lblMsg.Visible = true;
            lblMsg.Location = new Point((this.Width - lblMsg.Width) / 2,
                    (this.Height - lblMsg.Height) / 2);
            lblMsg.Text = msg;
            Application.DoEvents();
        }

        private void lblMsg_Click(object sender, EventArgs e)
        {
            lblMsg.Visible = false;
        }

    }
}
