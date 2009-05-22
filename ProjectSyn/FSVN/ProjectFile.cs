using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace FSVN
{
    /// <summary>
    /// 项目文件实例
    /// </summary>
    [Serializable]
    public class ProjectFile
    {
        /// <summary>
        /// 获取或设置该文件在项目中是否可选
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is optional; otherwise, <c>false</c>.
        /// </value>
        [XmlAttribute(AttributeName = "Optional")]
        public bool IsOptional { get; set; }

        /// <summary>
        /// 项目文件名，有扩展名的包含扩展名。
        /// </summary>
        /// <value>The name of the file.</value>
        [XmlAttribute(AttributeName = "name")]
        public string FileName { get; set; }

        /// <summary>
        /// 文件的最新(HEAD)版本
        /// </summary>
        /// <value>The version.</value>
        [XmlAttribute(AttributeName = "version")]
        public float Version { get; set; }

    }
}
