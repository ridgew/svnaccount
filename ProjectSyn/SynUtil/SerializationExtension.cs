using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
//using System.Web.Script.Serialization;
using System.Xml;
using System.Xml.Serialization;

namespace SynUtil
{
    public static class SerializationExtension
    {
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
        /// <param name="noNamespaceAttr">属性是否添加默认命名空间</param>
        /// <returns>如果对象实体为Null，则返回结果为Null。</returns>
        public static XmlDocument GetXmlDoc(this  object pObj, bool noNamespaceAttr)
        {
            if (pObj == null) { return null; }

            #region old
            //XmlSerializer serializer = new XmlSerializer(pObj.GetType());
            //StringBuilder sb = new StringBuilder();
            //StringWriter writer = new StringWriter(sb);

            //if (noNamespaceAttr)
            //{
            //    XmlSerializerNamespaces xn = new XmlSerializerNamespaces();
            //    xn.Add("", "");
            //    serializer.Serialize((TextWriter)writer, pObj, xn);
            //}
            //else
            //{
            //    serializer.Serialize((TextWriter)writer, pObj);
            //}

            //XmlDocument document = new XmlDocument();
            //document.LoadXml(sb.ToString());
            //writer.Close();
            //return document;
            #endregion

            using (MemoryStream ms = new MemoryStream())
            {
                XmlSerializer xs = new XmlSerializer(pObj.GetType());
                xs = new XmlSerializer(pObj.GetType(), string.Empty);
                XmlTextWriter xtw = new XmlTextWriter(ms, Encoding.UTF8);
                if (noNamespaceAttr)
                {
                    XmlSerializerNamespaces xn = new XmlSerializerNamespaces();
                    xn.Add("", "");
                    xs.Serialize(xtw, pObj, xn);
                }
                else
                {
                    xs.Serialize(xtw, pObj);
                }
                XmlDocument xml = new XmlDocument();
                xml.LoadXml(Encoding.UTF8.GetString(ms.ToArray()).Trim());
                return xml;
            }

        }

        /// <summary>
        /// 获取对象序列化的XmlDocument版本
        /// </summary>
        /// <param name="pObj">对象实体</param>
        /// <returns>如果对象实体为Null，则返回结果为Null。</returns>
        public static XmlDocument GetXmlDoc(this object pObj)
        {
            return GetXmlDoc(pObj, false);
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

        /// <summary>
        /// 输出带缩进格式的XML文档
        /// </summary>
        /// <param name="xDoc">XML文档对象</param>
        /// <param name="writer">文本输出器</param>
        public static void WriteIndentedContent(this XmlDocument xDoc, TextWriter writer)
        {
            XmlTextWriter xWriter = new XmlTextWriter(writer);
            xWriter.Formatting = Formatting.Indented;
            xDoc.WriteContentTo(xWriter);
        }
        
        /*
        /// <summary>
        /// 将对象转换成JSON字符串
        /// </summary>
        /// <param name="obj">源对象</param>
        /// <returns>JSON字符串</returns>
        public static string ToJSON(this object obj)
        {
            return new JavaScriptSerializer().Serialize(obj);
        }

        /// <summary>
        /// 将对象转换成JSON字符串
        /// </summary>
        /// <param name="obj">源对象</param>
        /// <param name="recursionDepth">对象内嵌级别深度</param>
        /// <returns>JSON字符串</returns>
        public static string ToJSON(this object obj, int recursionDepth)
        {
            JavaScriptSerializer s = new JavaScriptSerializer();
            s.RecursionLimit = recursionDepth;
            return s.Serialize(obj);
        }

        /// <summary>
        /// 从JSON数据获取对象实例
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="jsonData">JSON序列化字符</param>
        /// <returns></returns>
        public static T LoadFromJson<T>(this string jsonData)
        {
            JavaScriptSerializer s = new JavaScriptSerializer();
            return s.Deserialize<T>(jsonData);
        }
        */

        //public static string ToJson<T>(this T obj) where T : class
        //{
        //    DataContractJsonSerializer ser = new DataContractJsonSerializer(obj.GetType());
        //    string output = string.Empty;
        //    using (MemoryStream ms = new MemoryStream())
        //    {
        //        ser.WriteObject(ms, obj);
        //        StringBuilder sb = new StringBuilder();
        //        sb.Append(Encoding.UTF8.GetString(ms.ToArray()));
        //        output = sb.ToString();
        //    }
        //    return output;
        //}

        //public static T FromJson<T>(this string jsonString) where T : class
        //{
        //    T ouput = null;
        //    try
        //    {
        //        DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(T));
        //        using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(jsonString)))
        //        {
        //            ouput = (T)ser.ReadObject(ms);
        //        }
        //    }
        //    catch (Exception) { }
        //    return ouput;
        //}
    }
}
