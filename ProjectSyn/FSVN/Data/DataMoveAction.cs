using System;
using System.Collections.Generic;
using System.Text;

namespace FSVN.Data
{
    /// <summary>
    /// 数据移动或重命名的标志记录
    /// </summary>
    [Serializable]
    public class DataMoveAction : MarshalByRefObject
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
        /// 新的数据标志名称（在同一版本库内不重复）
        /// </summary>
        public string NewIdentityName { get; set; }

        /// <summary>
        /// 辅助函数，判断新旧标志是否属于同一父级对象
        /// </summary>
        /// <returns>如果属于同一父级，则为true，否则为false。</returns>
        public bool IsSameParent()
        {
            ProjectDataID id = new ProjectDataID() { IdentityName = IdentityName, RepositoryId = RepositoryId };
            ProjectDataID newId = new ProjectDataID() { IdentityName = NewIdentityName, RepositoryId = RepositoryId };
            return id.GetParentName().Equals(newId.GetParentName());
        }

	}
}
