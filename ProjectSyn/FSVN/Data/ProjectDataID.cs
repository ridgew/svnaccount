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

    }

}
