#if UnitTest
using System;
using System.Collections.Generic;
using System.Text;
using FSVN.Data;
using FSVN.Config;
using FSVN.Provider;
using FSVN.Util;
using NUnit.Framework;

namespace FSVN.UnitTest
{
    [TestFixture]
    public class ProjectDataTest
    {
        private ProjectRepository Repos = new ProjectRepository { RepositoryId = "90c25f5f-239d-45bc-b764-ff020ee3d6f9" };

        [SetUp]
        public void SetUp()
        {
            Repos.DataBind(); 
            Console.WriteLine("配置版本库ReposID:{0}", Repos.RepositoryId);
        }
        
        [Test]
        public void SampleStore()
        {
            Console.WriteLine("下一个版本号：{0}", Repos.GetNextReversionID());

            RawData bin = new RawData() { Charset = "utf-8", IsCompressed = false, IsText = true,
                BinaryData = Encoding.UTF8.GetBytes("Hello World!") };

            //if (Repos.Exists("test.htm"))
            //{ 
                
            //}

            ProjectData sample = new ProjectData()
            {
                Author = "test",
                ContainerIdentityName = string.Empty,
                DataSum = ExtensionHelper.GetMD5Hash(bin.BinaryData),
                Deepth = 0,
                Name = "test",
                IdentityName = "test.htm",
                IncludeBinary = true,
                CreateDateTimeUTC = DateTime.Now.ToUniversalTime(),
                ModifiedDateTimeUTC = DateTime.Now.ToUniversalTime(),
                ModuleConfig = new byte[0],
                RawConfig = bin.GetBytes(),
                RepositoryId = Repos.RepositoryId
            };

            Repos.Commit(sample, string.Empty);

        }


        [Test]
        public void GetReposNextReversion()
        {
            Console.WriteLine("版本库下一个版本:{0}", Repos.GetNextReversionID());

            //System.IO.Directory.Move(@"D:\FSVNRepositories\90c25f5f-239d-45bc-b764-ff020ee3d6f9", @"D:\FSVNRepositories\aaa");
        }

        [Test]
        public void StoreDataWithDir()
        {
            ProjectData sample = new ProjectData()
            {
                Author = "test",
                ContainerIdentityName = string.Empty,
                CreateDateTimeUTC = DateTime.Now.ToUniversalTime(),
                Deepth = 0,
                Name = "js",
                IdentityName = "js",
                IncludeBinary = false,
                LastModifiedVersion = null,
                ModifiedDateTimeUTC = DateTime.Now.ToUniversalTime(),
                RepositoryId = Repos.RepositoryId,
                Reversion = "1"
            };

            Repos.Commit(sample, string.Empty);
        }

        [Test]
        public void SampleGet()
        {
            ProjectData dat = Repos.GetProjectData("test.htm");

            Assert.IsNotNull(dat);

            Console.WriteLine("标志名称：{0}", dat.IdentityName);
            Console.WriteLine("数据版本：{0}", dat.Reversion);
            Console.WriteLine("最近修改版本：{0}", dat.LastModifiedVersion);

            RawData rDat = null; 
            if (dat.IncludeBinary)
            {
                rDat = dat.RawConfig.GetObject<RawData>();

                Console.WriteLine("数据大小：{0}byte", rDat.BinaryData.Length);

                //Repos.CheckSumProviderTypeName
                Console.WriteLine("数据效验：{0}(MD5)", dat.DataSum);
            }

            Console.WriteLine("创建日期：{0}", dat.CreateDateTimeUTC.ToLocalTime());
            Console.WriteLine("修改日期：{0}", dat.ModifiedDateTimeUTC.ToLocalTime());

            if (rDat != null)
            {
                Console.WriteLine("----------------");
                if (rDat.IsText)
                {
                    Console.WriteLine("{0}", Encoding.GetEncoding(rDat.Charset).GetString(rDat.BinaryData));
                }
                Console.WriteLine("----------------");
            }


        }

        [Test]
        public void TempDelete()
        {
            Repos.Commit(new ProjectDataID[] { 
                new ProjectDataID { IdentityName = "js", RepositoryId = Repos.RepositoryId }
            }, string.Empty);
        }

        [Test]
        public void TempRemove()
        {
            Repos.Commit(new ProjectDataID[] { 
                new ProjectDataID { IdentityName = "test.htm", RepositoryId = Repos.RepositoryId }
            }, "移动测试");
        }

        [Test]
        public void TempRename()
        { 
        
        }

        [Test]
        public void GetAllLog()
        {
            ChangeLog[] logs = Repos.GetReversionLogs(1, "$HEAD$");
            foreach (ChangeLog log in logs)
            {
                Console.WriteLine("Rev:{1}, Msg:{0}", log.Message, log.ReversionId);
                ChangeAction[] chs = (ChangeAction[])log.Summary.GetObject<ChangeAction[]>();
                if (chs != null && chs.Length > 0)
                {
                    foreach (ChangeAction c in chs)
                    {
                        Console.WriteLine("ID:{0}\r\n---------------------------------------------\r\n Msg:{1}", string.Join("\n", c.IdentityNames), c.GetSummary());
                    }
                }
                Console.WriteLine();
            }
        }

        [Test]
        public void DataEntityTest()
        {
            ProjectData dat = new ProjectData { RawConfig = new RawData { IsText = false, IsCompressed = false }.GetBytes() };
            //ProjectData dat2 = new ProjectData { RawConfig = Encoding.UTF8.GetBytes("Hello Word!") };
        }

        [Test]
        public void 新建库测试()
        { 
        
        }

        [Test]
        public void 现有文件系统创建库测试()
        {

        }

        [Test]
        public void 所有操作测试()
        {
            /*
             1. 新建库(存在则清空)
             2. 添加文件testInRoot1.html、testInRoot2.html、testInRoot3.html
             3. 添加目录js
             4. 添加文件js/main.js
                   获取js目录的目录树结构

             5. 添加文件 css/test.css (自动创建目录)
                   获取css目录的目录树结构

             6. 获取testInRoot2.html 修改文件 testInRoot2.html 并提交
             7. 创建目录bak
             8. 移动testInRoot1.html、testInRoot3.html到bak目录
             9. 重命名bak/testInRoot1.html 为 bak/1.html
                重命名bak/testInRoot3.html 为 bak/3.html
             10. 删除文件bak/3.html
             11. 添加文件bak/3.html 新内容为"I'm come back."
             12. 删除目录bak
             13. 获取最新的目录树结构
             */
        }

        [Test]
        public void 获取指定版测试()
        {
            /*
             Sub CreateCab(ByVal CabFileName, ByVal MakeSignable, ByVal ExtraSpace, ByVal Use10Format)
             Sub AddFile(ByVal FileName, ByVal FileNameInCab)
             Sub CopyFile(ByVal CabName, ByVal FileNameInCab)
             Sub CloseCab
             */

            //获取指定版本为cab文件包
            #region CLR
            //Type cabType = Type.GetTypeFromProgID("MakeCab.MakeCab.1", false);
            //if (cabType == null) return;

            //Object cabInstance = Activator.CreateInstance(cabType);

            //cabType.InvokeMember("CreateCab", System.Reflection.BindingFlags.InvokeMethod, Type.DefaultBinder, cabInstance,
            //    new object[] { @"c:\a.cab", false, false, false });

            //cabType.InvokeMember("AddFile", System.Reflection.BindingFlags.InvokeMethod, null, cabInstance,
            //    new object[] { @"c:\bootbak.ini", "bootbak.ini" });

            //cabType.InvokeMember("CloseCab", System.Reflection.BindingFlags.InvokeMethod, null, cabInstance,new object[0]);

            //while (System.Runtime.InteropServices.Marshal.ReleaseComObject(cabInstance) > 0) ;
            //cabInstance = null;
            #endregion

            #region COM Wrap
            //IMakeCab cabTool = (IMakeCab)COMWrapper.CreateInstance(typeof(IMakeCab));
            //cabTool.CreateCab(@"c:\c.cab", false, false, false);
            //cabTool.AddFile(@"c:\bootbak.ini", "a\\1.ini");
            //cabTool.CloseCab();

            ////cabTool.CopyFile(@"c:\b.cab", "2.ini");
            
            //cabTool.Dispose();
            #endregion

        }

        [Test]
        public void _完善版本测试()
        { 
            //Client.IFSVNClient client = 
        }

    }
}
#endif