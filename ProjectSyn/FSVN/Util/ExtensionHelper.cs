using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Security.Cryptography;
using System.Xml;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;

namespace FSVN.Util
{
    public static class ExtensionHelper
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
            //using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            //{
            //    int totalLen = (int)fs.Length;
            //    byte[] fDat = new byte[totalLen];
            //    fs.Read(fDat, 0, totalLen);
            //    return fDat;
            //}
            return File.ReadAllBytes(filePath);
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

        /// <summary>
        /// 获取对象序列化的二进制版本
        /// </summary>
        /// <param name="pObj">对象实体</param>
        /// <returns>如果对象实体为Null，则返回结果为Null。</returns>
        public static byte[] GetBytes(this object pObj)
        {
            if (pObj == null) { return null; }
            MemoryStream serializationStream = new MemoryStream();
            new BinaryFormatter().Serialize(serializationStream, pObj);
            serializationStream.Position = 0L;
            byte[] buffer = new byte[serializationStream.Length];
            serializationStream.Read(buffer, 0, buffer.Length);
            serializationStream.Close();
            return buffer;
        }

        /// <summary>
        /// 获取对象序列化的XmlDocument版本
        /// </summary>
        /// <param name="pObj">对象实体</param>
        /// <returns>如果对象实体为Null，则返回结果为Null。</returns>
        public static XmlDocument GetXmlDoc(this object pObj)
        {
            if (pObj == null) { return null; }
            XmlSerializer serializer = new XmlSerializer(pObj.GetType());
            StringBuilder sb = new StringBuilder();
            StringWriter writer = new StringWriter(sb);
            serializer.Serialize((TextWriter)writer, pObj);
            XmlDocument document = new XmlDocument();
            document.LoadXml(sb.ToString());
            writer.Close();
            return document;
        }

        /// <summary>
        /// 从已序列化数据中(byte[])获取对象实体
        /// </summary>
        /// <typeparam name="T">要返回的数据类型</typeparam>
        /// <param name="binData">二进制数据</param>
        /// <returns>对象实体</returns>
        public static T GetObject<T>(this byte[] binData)
        {
            if (binData == null) { return default(T); }
            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream serializationStream = new MemoryStream(binData);
            return (T)formatter.Deserialize(serializationStream);
        }

        /// <summary>
        /// 从已序列化数据中(byte[])获取对象实体
        /// </summary>
        /// <param name="binData">二进制数据</param>
        /// <returns>对象实体</returns>
        public static object GetObject(this byte[] binData)
        {
            if (binData == null) return null;
            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream serializationStream = new MemoryStream(binData);
            return formatter.Deserialize(serializationStream);
        }

        /// <summary>
        /// 从已序列化数据(XmlDocument)中获取对象实体
        /// </summary>
        /// <typeparam name="T">要返回的数据类型</typeparam>
        /// <param name="xmlDoc">已序列化的文档对象</param>
        /// <returns>对象实体</returns>
        public static T GetObject<T>(this XmlDocument xmlDoc)
        {
            if (xmlDoc == null) { return default(T); }
            XmlNodeReader xmlReader = new XmlNodeReader(xmlDoc.DocumentElement);
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            return (T)serializer.Deserialize(xmlReader);
        }
    }
}
