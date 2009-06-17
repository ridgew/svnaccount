using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace FSVNClient
{
    public partial class frmWorkBench : Form
    {
        public frmWorkBench()
        {
            InitializeComponent();
        }

        private void fileMon_Changed(object sender, FileSystemEventArgs e)
        {

        }

        private void fileMon_Created(object sender, FileSystemEventArgs e)
        {

        }

        private void fileMon_Deleted(object sender, FileSystemEventArgs e)
        {

        }

        private void fileMon_Renamed(object sender, RenamedEventArgs e)
        {

        }
    }
}
