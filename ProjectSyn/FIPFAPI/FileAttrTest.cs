using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Collections;
using System.Security.AccessControl;
using System.Net;
using FIPFAPI.Cases;

namespace FIPFAPI
{
    class FileAttrTest
    {
        public static void WriteFileTest()
        {
            FileInfo fi = new FileInfo("syn.fsvn");
            if (fi.Exists && (fi.Attributes & FileAttributes.ReadOnly) != 0)
            {
                fi.Attributes = FileAttributes.Normal;
            }

            if (!fi.Exists)
            {
                using (FileStream fs = fi.Open(FileMode.Create, FileAccess.ReadWrite))
                {
                    byte[] fmqSvn = System.Text.Encoding.ASCII.GetBytes("FSVN 1.0");
                    fs.Write(fmqSvn, 0, fmqSvn.Length);

                    Hashtable svnTab = new Hashtable(StringComparer.InvariantCultureIgnoreCase);
                    svnTab.Add(fi.Name, 1f);
                    byte[] binCont = SerializeHelper.GetBytes(svnTab);
                    fs.Write(binCont, 0, binCont.Length);

                    fs.Close();
                }
            }

            using (FileStream fs = fi.Open(FileMode.Open, FileAccess.ReadWrite))
            {
                if (fs.Length > 8)
                {
                    byte[] fmSvn = new byte[8];
                    fs.Read(fmSvn, 0, 8);

                    Console.WriteLine(System.Text.Encoding.ASCII.GetString(fmSvn));

                    byte[] binCont = new byte[fs.Length - 8];
                    fs.Position = 8;
                    fs.Read(binCont, 0, binCont.Length);

                    Hashtable svnTab = SerializeHelper.GetObject<Hashtable>(binCont);

                    foreach (object key in svnTab.Keys)
                    {
                        Console.WriteLine(string.Format("{0}:{1}", key, svnTab[key]));
                    }
                }
                fs.Close();
            }

            fi.LastWriteTime = DateTime.Today.AddDays(-10.00);
            fi.CreationTime = DateTime.Today.AddDays(-20.00);
            fi.Attributes |= FileAttributes.ReadOnly;
            fi.Refresh();

            DirectoryInfo di = new DirectoryInfo("test");
            if (!di.Exists)
            {
                Directory.CreateDirectory("test");
            }
            else
            {
                if ((di.Attributes & FileAttributes.ReadOnly) != 0)
                {
                    di.Attributes = FileAttributes.Normal;
                }

                
            }

            //取得访问控制列表
            DirectorySecurity dirsecurity = di.GetAccessControl();
            string strDomain = Environment.MachineName; //Dns.GetHostName();
            dirsecurity.AddAccessRule(new FileSystemAccessRule(strDomain + "\\ASPNET",
                FileSystemRights.FullControl, AccessControlType.Allow));
            di.SetAccessControl(dirsecurity);

            di.LastWriteTime = DateTime.Today.AddDays(-10.00);
            di.CreationTime = DateTime.Today.AddDays(-20.00);
            di.Attributes |= FileAttributes.ReadOnly;
            di.Refresh();
        }

        public static void DoAnalyze(string baseDir, string currentDir, List<FIPFAPI.Cases.FmqProject.ProjectFile> files)
        {
            foreach (string fileName in Directory.GetFiles(currentDir))
            {
                FIPFAPI.Cases.FmqProject.ProjectFile file = new FmqProject.ProjectFile();
                file.FileName = fileName.Replace(baseDir, "");
                file.IsOptional = false;
                file.Version = 1.0f;

                files.Add(file);
            }

            foreach (string subDir in Directory.GetDirectories(currentDir))
            {
                DoAnalyze(baseDir, subDir, files);
            }
        }

        public static void PackageProject()
        {
            //ProjectArchive a = new ProjectArchive(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location),
            //    new VistaDBProjectPackage());

            //a.MsgReport = new ReportMessage(delegate(string msg) { Console.WriteLine(msg); });
            //a.ProgressReport = new ReportProgress(delegate(float percent) { Console.WriteLine((percent * 100).ToString("0") + "%"); });

            ////a.FileMatch = new Predicate<FileInfo>(delegate(FileInfo fi)
            ////    {
            ////        return true;
            ////    });

            //a.Package();
            //a.Dispose();

            FmqProject Proj = new FmqProject();
            Proj.Name = "test";
            Proj.Version = 1.0f;

            List<FIPFAPI.Cases.FmqProject.ProjectFile> files = new List<FIPFAPI.Cases.FmqProject.ProjectFile>();
            DoAnalyze(AppDomain.CurrentDomain.BaseDirectory, AppDomain.CurrentDomain.BaseDirectory, files);


            FIPFAPI.Cases.FmqProject.Module module = new FmqProject.Module();
            module.Name = "default";
            module.Version = 1.2f;
            module.Files = files;

            FIPFAPI.Cases.FmqProject.ModuleGroup group = new FmqProject.ModuleGroup();
            group.Name = "defult group";
            group.Version = 1.0f;

            group.Modules = new FmqProject.Module[] { module };
            Proj.ModuleGroups = new FmqProject.ModuleGroup[] {  group };

            //Proj.Files = files;
            SerializeHelper.GetXmlDoc(Proj).Save("test.fproj");

            Console.WriteLine("OK, Project build finished..");

        }

    }
}
