using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Vbyte.DataSource.Utility;

namespace Vbyte.DataSource.UnitTest
{
    [TestFixture]
    public class SortKeyTest
    {
        private readonly string localFile = AppDomain.CurrentDomain.BaseDirectory + "\\KValue.dat";

        [Test]
        public void temp()
        {
            SortedList<string, long> sList = new SortedList<string, long>(StringComparer.Ordinal);
            sList.Add("a", 11);
            sList.Add("z", 22);
            sList.Add("B", 55);
            sList.Add("b", 33);
            Console.WriteLine(sList.IndexOfKey("b"));
        }

        [Test]
        public void GetKeyTest()
        {
            KeyValueFileStore fs = new KeyValueFileStore(localFile);

            string[] keys = fs.GetAllKeys();
            foreach (string k in keys)
            {
                //string k = "debugtest";
                if (k.StartsWith("debug"))
                {
                    byte[] kDat = fs.GetKeyData(k);
                    if (kDat.Length > 0)
                    {
                        Console.WriteLine("Key: {0}, Data: [{1}]", k, Encoding.Default.GetString(kDat));
                    }
                    else
                    {
                        Console.WriteLine("Key: {0}, Len: [{1}]", k, kDat.Length);
                    }
                }
            }
            fs.Dispose();
        }

        [Test]
        public void StoreKeyTest()
        {
            KeyValueFileStore fs = new KeyValueFileStore(localFile);
            for (int i = 0; i < 1; i++)
            {
                //int i = 0;
                //fs.StoreKeyData("debugtest", Encoding.Default.GetBytes("Hello word! (" + i + ")"));

                //Console.WriteLine(i);
                fs.StoreKeyData(i + "test" + DateTime.Now.Ticks, Encoding.Default.GetBytes("Hello word! (" + i + ")"));
            }
            fs.Dispose();
        }

        [Test]
        public void ExistsTest()
        {
            KeyValueFileStore fs = new KeyValueFileStore(localFile);
            Console.WriteLine(fs.ExistsKey("hi"));
            fs.Dispose();
        }

        [Test]
        public void IndexDataViewTest()
        {
            KeyValueFileStore fs = new KeyValueFileStore(localFile);
            SortedList<string, KeyValueState> idxObject = fs.GetIndexObject(0);
            if (idxObject != null)
            {
                Console.WriteLine("Not Null, {0}", idxObject.Count);
                foreach (string k in idxObject.Keys)
                {
                    if (k.StartsWith("debug"))
                    {
                        Console.WriteLine("Key: {0}, IDX: {1}, Len: {2}, ChipSize: {3}", k,
                            idxObject[k].DataIndex,
                            idxObject[k].Length,
                            idxObject[k].ChipSize);
                    }
                    //Console.WriteLine(" a " + k);
                }
            }
            fs.Dispose();
        }

        [Test]
        public void DirtyBlockViewTest()
        {
            KeyValueFileStore fs = new KeyValueFileStore(localFile);
            SortedList<long, DirtyBlock> dObjs = fs.GetStoreDirtyData();
            if (dObjs != null)
            {
                Console.WriteLine("Not Null, {0}", dObjs.Count);
                foreach (long k in dObjs.Keys)
                {
                    Console.WriteLine("Key: {0}, IDX: {1}, Len: {2}", k,
                        dObjs[k].DataIndex,
                        dObjs[k].Length);
                    //Console.WriteLine(" a " + k);
                }
            }
            fs.Dispose();
        }

        [Test]
        public void UpdateKeyDataTest()
        {
            KeyValueFileStore fs = new KeyValueFileStore(localFile);
            fs.StoreKeyData("debugtest3", Encoding.Default.GetBytes("---+++"));
            fs.Dispose();
        }

        [Test]
        public void RemoveKeyDataTest()
        {
            KeyValueFileStore fs = new KeyValueFileStore(localFile);
            Console.WriteLine(fs.RemoveData("debugtest1"));
            fs.Dispose();
        }

        [Test]
        public void StoreSummary()
        {
            KeyValueFileStore fs = new KeyValueFileStore(localFile);
            Console.WriteLine("Idx Size: {0}", fs.GetIndexSize());
            Console.WriteLine("Real Size: {0}", fs.GetIndexRealSize());
            Console.WriteLine("Next Write Idx: {0}", fs.GetNextDataWriteIndex());
            Console.WriteLine("Dat Offset：{0}", fs.GetDataReadOffset());
            Console.WriteLine("Dirty Size: {0}", fs.GetDirtyBlockRealSize());
            Console.WriteLine("Version: {0}", fs.GetStoreVersion());
            Console.WriteLine("Keys Count: {0}", fs.GetKeyCount());
            //Console.WriteLine("Keys: {0}", string.Join("\n", fs.GetAllKeys()));
            fs.Dispose();
        }

    }
}
