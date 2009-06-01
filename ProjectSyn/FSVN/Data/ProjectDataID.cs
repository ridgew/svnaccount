using System;
using System.Collections.Generic;
using System.Text;

namespace FSVN.Data
{
    /// <summary>
    /// 项目数据的标志
    /// </summary>
    [Serializable]
    public class ProjectDataID : MarshalByRefObject
    {
        /// <summary>
        /// 数据标志名称（在同一版本库内不重复）
        /// </summary>
        public string IdentityName { get; set; }

        /// <summary>
        /// 版本库编号
        /// </summary>
        public string RepositoryId { get; set; }

        /// <summary>
        /// 获取名称（目录或文件名）
        /// </summary>
        public string GetName()
        {
            string idName = IdentityName.Replace("/", "\\");
            int idx = idName.LastIndexOf("\\");
            if (idx == -1)
            {
                return idName;
            }
            else
            {
                return idName.Substring(idx);
            }
        }

        /// <summary>
        /// 获取父级名称
        /// </summary>
        public string GetParentName()
        {
            string idName = IdentityName.Replace("/", "\\");
            int idx = idName.LastIndexOf("\\");
            if (idx == -1)
            {
                return string.Empty;
            }
            else
            {
                return idName.Substring(0, idx);
            }
        }

    }

}
