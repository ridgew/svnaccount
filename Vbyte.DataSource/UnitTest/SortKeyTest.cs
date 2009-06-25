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
        public void StoreKeyTest()
        {
            KeyValueFileStore fs = new KeyValueFileStore(localFile);
            fs.StoreKeyData("hi", Encoding.Default.GetBytes("Hello word!"));
            fs.Dispose();
        }

        [Test]
        public void ExistsTest()
        {
            KeyValueFileStore fs = new KeyValueFileStore(localFile);
            Console.WriteLine(fs.ExistsKey("hi"));
            fs.Dispose(); 
        }
    }
}
