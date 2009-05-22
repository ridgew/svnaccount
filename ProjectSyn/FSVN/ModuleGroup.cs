using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace FSVN
{
    /// <summary>
    /// 模块分组实例
    /// </summary>
    [Serializable]
    public class ModuleGroup
    {
        /// <summary>
        /// 模块分组内的所有模块
        /// </summary>
        public Module[] Modules { get; set; }

        /// <summary>
        /// 模块分组版本
        /// </summary>
        [XmlAttribute(AttributeName = "version")]
        public float Version { get; set; }

        /// <summary>
        /// 模块分组名称
        /// </summary>
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
    }
}
