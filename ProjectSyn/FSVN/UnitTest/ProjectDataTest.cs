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
        
        }

        [Test]
        public void TempRename()
        { 
        
        }

        [Test]
        public void DataEntityTest()
        {
            ProjectData dat = new ProjectData { RawConfig = new RawData { IsText = false, IsCompressed = false }.GetBytes() };
            //ProjectData dat2 = new ProjectData { RawConfig = Encoding.UTF8.GetBytes("Hello Word!") };
        }
    }
}
#endif