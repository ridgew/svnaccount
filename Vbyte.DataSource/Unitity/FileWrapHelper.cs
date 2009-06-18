using System;
using System.IO;

namespace Vbyte.DataSource.Unitity
{
    /// <summary>
    /// 本地文件封装辅助
    /// </summary>
    public class FileWrapHelper
    {
        public byte[] GetFileBytes(string locFile, out bool IsVDatWrappedFile)
        {
            byte[] fDat = new byte[0];
            if (!File.Exists(locFile))
            {
                IsVDatWrappedFile = false;
            }
            else
            {
                fDat = File.ReadAllBytes(locFile);
                //!VDat => 21 56 44 61 74
                IsVDatWrappedFile = fDat.Length > 5;
            }
            return fDat;
        }
    }

}
