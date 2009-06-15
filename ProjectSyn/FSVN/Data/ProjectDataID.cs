﻿using System;
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
        /// 获取项目标志的目录深度
        /// </summary>
        public int GetDeepth()
        {
            if (GetParentName() == string.Empty) return 1;
            string strID = IdentityName.Replace("/", "\\");

            if (!strID.StartsWith("\\")) strID = "\\" + strID;
            if (strID.EndsWith("\\")) strID = strID.TrimEnd('\\');
            return strID.Split('\\').Length - 1;
        }

        /// <summary>
        /// 获取父级名称，没有父级则为字符空。
        /// </summary>
        public string GetParentName()
        {
            string idName = IdentityName.Replace("/", "\\");
            int idx = idName.LastIndexOf("\\");
            if (idx <= 0)
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
