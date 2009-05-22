using System;
using System.Configuration;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.XPath;

namespace FSVN.Config
{
    /// <summary>
    /// Configuration section handler that deserializes connfiguration settings to an object.
    /// </summary>
    /// <remarks>The configuration node must have a type attribute defining the type to deserialize to.</remarks>
    /// <example>
    /// 
    /// &lt;configuration&gt;
    ///  &lt;configSections&gt;
    ///     &lt;section name="(Any section with XmlSerialized)" type="Vbyte.SharpOrm.Config.XmlSerializeSectionHandler, Vbyte.SharpOrm" /&gt;
    ///  &lt;configSections&gt;
    /// &lt;/configSections>
    /// 
    /// </example>
    public class XmlSerializeSectionHandler : IConfigurationSectionHandler
    {
        #region IConfigurationSectionHandler 成员
        /// <summary>
        /// 创建配置节处理程序，该配置节点必须有一个名为type的属性指示序列化类型的数据格式。
        /// </summary>
        /// <param name="parent">父对象。</param>
        /// <param name="configContext">配置上下文对象。</param>
        /// <param name="section">节 XML 节点。</param>
        /// <returns>创建的节处理程序对象。</returns>
        public object Create(object parent, object configContext, XmlNode section)
        {
            XPathNavigator navigator = section.CreateNavigator();
            string typeName = (string)navigator.Evaluate("string(@type)");
            Type type = Type.GetType(typeName, true);
            XmlSerializer serializer = new XmlSerializer(type);
            return serializer.Deserialize(new XmlNodeReader(section));
        }
        #endregion

        /// <summary>
        /// 获取节点实例
        /// </summary>
        /// <typeparam name="T">节点类型</typeparam>
        /// <param name="sectionName">配置区节点名称</param>
        /// <returns></returns>
        public static T GetObject<T>(string sectionName)
        {
            object sectionObj = System.Configuration.ConfigurationManager.GetSection(sectionName);
            if (sectionObj != null)
            {
                return (T)sectionObj;
            }
            else
            {
                return default(T);
            }
        }

    }
}
