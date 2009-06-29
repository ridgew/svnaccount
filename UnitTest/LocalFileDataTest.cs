using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;

namespace Vbyte.DataSource.UnitTest
{
    [TestFixture]
    public class LocalFileDataTest
    {
        [Test]
        public void StoreFile()
        {
            LocalFileData fDat = new LocalFileData
            {
                IdentityName = "test.html",
                Name = "test",
                CreateDateTimeUTC = DateTime.Now.ToUniversalTime(),
                ModifiedDateTimeUTC = DateTime.Now.ToUniversalTime(),
                IsContainer = false,
                RawData = System.Text.Encoding.Default.GetBytes("Hello Word!")
            };

            FileSystemStorage fStore = new FileSystemStorage();
            fStore.Store(fDat);
        }

        [Test]
        public void ReadFile()
        {
            FileSystemStorage fStore = new FileSystemStorage();
            LocalFileData fDat = fStore.GetData("test.html") as LocalFileData;
            Console.WriteLine("名称：{0}", fDat.Name);
            Console.WriteLine("标识：{0}", fDat.IdentityName);
            Console.WriteLine("创建时间：{0}", fDat.CreateDateTimeUTC.ToLocalTime());
            Console.WriteLine("修改时间：{0}", fDat.ModifiedDateTimeUTC.ToLocalTime());
            Console.WriteLine("文本内容：{0}", System.Text.Encoding.Default.GetString(fDat.RawData));
        }

        [Test]
        public void StoreDir()
        {
            LocalFileData fDat = new LocalFileData
            {
                IdentityName = "test",
                Name = "test",
                CreateDateTimeUTC = DateTime.Now.ToUniversalTime(),
                ModifiedDateTimeUTC = DateTime.Now.ToUniversalTime(),
                IsContainer = true
            };

            FileSystemStorage fStore = new FileSystemStorage();
            fStore.Store(fDat);
        }

        [Test]
        public void ReadDir()
        {
            FileSystemStorage fStore = new FileSystemStorage();
            LocalFileData fDat = fStore.GetData("/test") as LocalFileData;
            Console.WriteLine("名称：{0}", fDat.Name);
            Console.WriteLine("标识：{0}", fDat.IdentityName);
            Console.WriteLine("创建时间：{0}", fDat.CreateDateTimeUTC.ToLocalTime());
            Console.WriteLine("修改时间：{0}", fDat.ModifiedDateTimeUTC.ToLocalTime());
        }
    }
}
