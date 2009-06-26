﻿using System;
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
        public void StoreKeyTest()
        {
            KeyValueFileStore fs = new KeyValueFileStore(localFile);
            for (int i = 0; i < 100; i++)
            {
                fs.StoreKeyData( i + "test" + DateTime.Now.Ticks, Encoding.Default.GetBytes("Hello word! (" + i + ")"));
            }
            //fs.StoreKeyData("hi", Encoding.Default.GetBytes("Hello word!"));
            //fs.StoreKeyData("h1", Encoding.Default.GetBytes("Hello word! 100 A"));
            //fs.StoreKeyData("h2", Encoding.Default.GetBytes("Hello word! 200 B"));
            //fs.StoreKeyData("h3", Encoding.Default.GetBytes("Hello word! 300 C"));
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
                    Console.WriteLine("Key: {0}, IDX: {1}, Len: {2}, ChipSize: {3}", k,
                        idxObject[k].DataIndex,
                        idxObject[k].Length,
                        idxObject[k].ChipSize);
                    //Console.WriteLine(" a " + k);
                }
            }
            fs.Dispose();
        }

        [Test]
        public void GetKeyTest()
        {
            KeyValueFileStore fs = new KeyValueFileStore(localFile);
            string[] keys = fs.GetAllKeys();
            foreach (string k in keys)
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
            fs.Dispose();
        }

        [Test]
        public void StoreSummary()
        {
            KeyValueFileStore fs = new KeyValueFileStore(localFile);
            Console.WriteLine("Idx Size: {0}", fs.GetIndexSize());
            Console.WriteLine("Real Size: {0}", fs.GetIndexRealSize());
            Console.WriteLine("Version: {0}", fs.GetStoreVersion());
            Console.WriteLine("Keys Count: {0}", fs.GetKeyCount());
            Console.WriteLine("Keys: {0}", string.Join("\n", fs.GetAllKeys()));
            fs.Dispose();
        }

    }
}