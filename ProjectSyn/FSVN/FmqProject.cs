using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.IO;

namespace FSVN
{
    /// <summary>
    /// 项目实例（序列化文件名为*.fproj）
    /// </summary>
    [Serializable]
    public class FmqProject
    {
        /// <summary>
        /// 获取或设置项目版本
        /// </summary>
        [XmlAttribute(AttributeName = "version")]
        public float Version { get; set; }

        /// <summary>
        /// 获取或设置项目名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 获取或设置项目的模块分组
        /// </summary>
        [XmlAttribute(AttributeName = "groups")]
        public ModuleGroup[] ModuleGroups { get; set; }



        #region 常用封装
        /// <summary>
        /// 
        /// </summary>
        /// <param name="projRootDir"></param>
        /// <returns></returns>
        public static FmqProject CreateFromDirectory(DirectoryInfo projRootDir)
        {
            return new FmqProject { Name = projRootDir.Name, Version = 1 };
        }
        #endregion

    }
}
