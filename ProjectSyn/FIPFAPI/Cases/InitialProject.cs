using System;
using System.Collections.Generic;
using System.Text;
using VistaDB.Provider;
using System.IO;
using System.Security.Cryptography;

namespace FIPFAPI.Cases
{
    public class InitialProject
    {
        /// <summary>
        /// 获取本地文件MD5哈希值
        /// </summary>
        /// <param name="FilePath">本地文件完整路径</param>
        /// <remarks>
        /// 如果文件不存在则返回字符N/A
        /// </remarks>
        public static string GetMD5Hash(string FilePath)
        {
            if (!File.Exists(FilePath))
            {
                return "N/A";
            }
            else
            {
                System.IO.FileStream fs = new FileStream(FilePath, FileMode.Open, FileAccess.Read);
                System.Security.Cryptography.MD5 md = new MD5CryptoServiceProvider();
                md.Initialize();
                byte[] b = md.ComputeHash(fs);
                return ByteArrayToHexStr(b, true);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="binDat"></param>
        /// <returns></returns>
        public static string GetMD5Hash(byte[] binDat)
        {
            System.Security.Cryptography.MD5 md = new MD5CryptoServiceProvider();
            md.Initialize();
            byte[] b = md.ComputeHash(binDat);
            return ByteArrayToHexStr(b, true);
        }

        /// <summary>
        /// 获取指定文件的二进制数据
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns></returns>
        public static byte[] GetFileBytes(string filePath)
        {
            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                int totalLen = (int)fs.Length;
                byte[] fDat = new byte[totalLen];
                fs.Read(fDat, 0, totalLen);
                return fDat;
            }
        }

        /// <summary>
        /// 二进制流的16进制字符串表达形式
        /// </summary>
        /// <param name="bytHash">二进制流</param>
        /// <param name="IsLowerCase">小写16进制字母</param>
        public static string ByteArrayToHexStr(byte[] bytHash, bool IsLowerCase)
        {
            string sTemp = "";
            for (int i = 0; i < bytHash.Length; i++)
            {
                sTemp += bytHash[i].ToString("X").PadLeft(2, '0');
                // {0:x2}
            }
            return (IsLowerCase) ? sTemp.ToLower() : sTemp.ToUpper();
        }

        public static void Test()
        {
            FileInfo fi = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "\\Cases\\InitialProject.cs");
            
            
            VistaDBConnection conn = new VistaDBConnection("Data Source=" + AppDomain.CurrentDomain.BaseDirectory + "\\App_Data\\FmqStore.vdb3");
            conn.Open();

            string sql = "insert into [FileSystem](FileName,FileDir,FileSize,HashCode,BinData,FileVersion,CreateDate,LastChangeDate) values("
                    +"@FileName,@FileDir,@FileSize,@HashCode,@BinData,1, @CreateDate, @LastChangeDate"
                    +")";
            VistaDBCommand cmd = new VistaDBCommand(sql, conn);
            cmd.Parameters.AddWithValue("@FileName", fi.Name);
            cmd.Parameters.AddWithValue("@FileDir", "/cases");
            cmd.Parameters.AddWithValue("@FileSize", fi.Length);

            byte[] fBin = GetFileBytes(fi.FullName);

            cmd.Parameters.AddWithValue("@HashCode", GetMD5Hash(fBin));
            cmd.Parameters.AddWithValue("@BinData", fBin);
            cmd.Parameters.AddWithValue("@CreateDate", fi.CreationTimeUtc);
            cmd.Parameters.AddWithValue("@LastChangeDate", fi.LastWriteTimeUtc);

            cmd.ExecuteNonQuery();
            
            conn.Close();
            conn.Dispose();
        }
    }
}
