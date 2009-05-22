using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.Collections.ObjectModel;

namespace FSVN
{
    /// <summary>
    /// 项目模块实例
    /// </summary>
    [Serializable]
    public class Module
    {
        /// <summary>
        /// 项目模块包含的所有文件集合
        /// </summary>
        public ReadOnlyCollection<ProjectFile> Files { get; set; }

        /// <summary>
        /// 项目模块版本
        /// </summary>
        [XmlAttribute(AttributeName = "version")]
        public float Version { get; set; }

        /// <summary>
        /// 项目模块名称
        /// </summary>
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }

    }
}
