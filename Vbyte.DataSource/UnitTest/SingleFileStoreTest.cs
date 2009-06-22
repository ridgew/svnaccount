using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using NUnit.Framework;
using Vbyte.DataSource.Unitity;

namespace Vbyte.DataSource.UnitTest
{
    [TestFixture]
    public class SingleFileStoreTest
    {
        private readonly string localFile = AppDomain.CurrentDomain.BaseDirectory +  "\\fsvn.dat";
        private readonly string localStruct = AppDomain.CurrentDomain.BaseDirectory + "\\fsvn.db";

        [Test]
        public void SimpleWrite()
        { 
            Dictionary<string, List<string>> dbDict = new Dictionary<string, List<string>>(StringComparer.InvariantCultureIgnoreCase);
            dbDict.Add("/", new List<string>{ "test", "test.html", "js", "css", "index.html" });

            uint version = 105;  //max:4294967295
            byte[] fileBytes = Unitity.FileWrapHelper.GetBytes(dbDict);

            IdentityFileStore fStore = new IdentityFileStore(localFile);
            fStore.WriteReversion(version, fileBytes);
            fStore.Dispose();
        }

        [Test]
        public void SimpleGet()
        {
            /*
                版本：1
                索引：2048
                长度：2107


                版本：3
                索引：4155
                长度：2107


                版本：45
                索引：6262
                长度：2107


                版本：99
                索引：8369
                长度：2107


                版本：102
                索引：10476
                长度：2107


                版本：105
                索引：12583
                长度：2107
             */
            IdentityFileStore fStore = new IdentityFileStore(localFile);

            byte[] fData = fStore.ReadReversion(100);
            File.WriteAllBytes(localFile.Replace("fsvn.dat", "dump.dat"), fData);

            fStore.Dispose();
        }

        [Test]
        public void RefactTest()
        {
            IdentityFileStore fStore = new IdentityFileStore(localFile);
            fStore.RefactHeadIndex(100);
            fStore.Dispose();
        }

        [Test]
        public void GetAllVersion()
        {
            IdentityFileStore fStore = new IdentityFileStore(localFile);
            StoreSnippet[] vers = fStore.GetAllVersions();
            foreach (StoreSnippet spt in vers)
            {
                Console.WriteLine("版本：{0}", spt.Version);
                Console.WriteLine("索引：{0}", spt.StoreIndex);
                Console.WriteLine("长度：{0}", spt.FileLength);
                Console.WriteLine();
            }
            fStore.Dispose();
        }

        [Test]
        public void SimpleStructure()
        {
            //版本文件
            Dictionary<string, List<string>> dbDict = new Dictionary<string, List<string>>(StringComparer.InvariantCultureIgnoreCase);
            dbDict.Add("/", new List<string>{ "test", "test.html", "js", "css", "index.html" });
            System.IO.File.WriteAllBytes(localStruct, Unitity.FileWrapHelper.GetBytes(dbDict));
        }

        [Test]
        public void MoveStructure()
        {
            //版本文件
            Dictionary<string, string> movDict = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
            movDict.Add("mian", "main");
            System.IO.File.WriteAllBytes(localStruct.Replace("fsvn.db", "mov.db"), Unitity.FileWrapHelper.GetBytes(movDict));
        }

        [Test]
        public void DeleteStructure()
        {
            //单一文件 自带版本
            Dictionary<long, List<string>> dbDict = new Dictionary<long, List<string>>();
            dbDict.Add(5, new List<string> { "test", "test.html", "js", "css", "index.html" });
            System.IO.File.WriteAllBytes(localStruct.Replace("fsvn.db", "del.db"), Unitity.FileWrapHelper.GetBytes(dbDict));
        }

    }
}
