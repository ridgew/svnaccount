using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.IO;

namespace FIPFAPI.Cases
{
    public class ProjectSynAPI
    {
        /*
         获取最新的版本及What's New,可选择更新。
        
         核心运行基础模块
         其他插件模块


         -u[pdate] -fl[force local]/fr[force remote]




         */

        

    }

    [Serializable]
    public class FmqProject
    {
        [XmlAttribute(AttributeName = "version")]
        public float Version { get; set; }
        public string Name { get; set; }

        public List<ProjectFile> Files { get; set; }

        [XmlAttribute(AttributeName = "groups")]
        public ModuleGroup[] ModuleGroups { get; set; }

        [Serializable]
        public class Module
        {
            public List<ProjectFile> Files { get; set; }

            [XmlAttribute(AttributeName = "version")]
            public float Version { get; set; }

            [XmlAttribute(AttributeName = "name")]
            public string Name { get; set; }
        }

        [Serializable]
        public class ModuleGroup
        {
            public Module[] Modules { get; set; }

            [XmlAttribute(AttributeName = "version")]
            public float Version { get; set; }

            [XmlAttribute(AttributeName = "name")]
            public string Name { get; set; }
        }

        [Serializable]
        public class ProjectFile
        {
            /// <summary>
            /// Gets or sets a value indicating whether this instance is optional.
            /// </summary>
            /// <value>
            /// 	<c>true</c> if this instance is optional; otherwise, <c>false</c>.
            /// </value>
            [XmlAttribute(AttributeName = "Optional")]
            public bool IsOptional { get; set; }

            /// <summary>
            /// Gets or sets the name of the file.
            /// </summary>
            /// <value>The name of the file.</value>
            [XmlAttribute(AttributeName = "name")]
            public string FileName { get; set; }

            /// <summary>
            /// Gets or sets the version.
            /// </summary>
            /// <value>The version.</value>
            [XmlAttribute(AttributeName = "version")]
            public float Version { get; set; }

        }
    }
}
