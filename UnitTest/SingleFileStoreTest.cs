using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using NUnit.Framework;
using Vbyte.DataSource.Utility;

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

            uint version = 108;  //max:4294967295
            byte[] fileBytes = Utility.FileWrapHelper.GetBytes(dbDict);

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

            Console.WriteLine("Ver：{0}", fStore.GetFileVersion());
            Console.WriteLine("IDX Size：{0}", fStore.GetIndexSize());
            Console.WriteLine("Dat Offset：{0}", fStore.GetDataOffset());
            Console.WriteLine("HEAD：{0}", fStore.GetHeadVersion());
            Console.WriteLine("FOOT：{0}", fStore.GetFootVersion());

            byte[] fData = fStore.ReadReversion(106);
            File.WriteAllBytes(localFile.Replace("fsvn.dat", "dump.dat"), fData);

            fStore.Dispose();
        }

        [Test]
        public void RefactTest()
        {
            IdentityFileStore fStore = new IdentityFileStore(localFile);
            fStore.RefactHeadIndex(0);
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
                Console.WriteLine("时间：{0}", spt.CreateTimeUTC.ToLocalTime());
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
            System.IO.File.WriteAllBytes(localStruct, Utility.FileWrapHelper.GetBytes(dbDict));
        }

        [Test]
        public void MoveStructure()
        {
            //版本文件
            Dictionary<string, string> movDict = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
            movDict.Add("mian", "main");
            System.IO.File.WriteAllBytes(localStruct.Replace("fsvn.db", "mov.db"), Utility.FileWrapHelper.GetBytes(movDict));
        }

        [Test]
        public void DeleteStructure()
        {
            //单一文件 自带版本
            Dictionary<long, List<string>> dbDict = new Dictionary<long, List<string>>();
            dbDict.Add(5, new List<string> { "test", "test.html", "js", "css", "index.html" });
            System.IO.File.WriteAllBytes(localStruct.Replace("fsvn.db", "del.db"), Utility.FileWrapHelper.GetBytes(dbDict));
        }

    }
}
