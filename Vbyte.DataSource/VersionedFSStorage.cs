using System;
using System.Collections.Generic;
using System.Text;

namespace Vbyte.DataSource
{
    /// <summary>
    /// 文件系统存储(版本控制)
    /// </summary>
    public class VersionedFSStorage : FileSystemStorage, IVersionControlStorage
    {
        #region IVersionControlStorage 成员

        /// <summary>
        /// 获取相关实例类型的指定版本
        /// </summary>
        /// <param name="identityName">数据标志号</param>
        /// <param name="rev">版本号</param>
        /// <returns>如果存在该数据则返回，否则为null。</returns>
        public IDataItem GetReversionData(string identityName, long rev)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 获取满足指定标志的数据集合
        /// </summary>
        /// <param name="filter">判断匹配规则</param>
        /// <param name="isMatch">匹配方向：true则匹配，false则为不匹配。</param>
        /// <param name="rev">版本号</param>
        /// <returns>如果存在则返回集合，否则为null或空数组。</returns>
        public IDataItem[] GetDataList(Predicate<IDataItem> filter, bool isMatch, long rev)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IVersionControl 成员

        /// <summary>
        /// 当前数据的版本
        /// </summary>
        /// <returns></returns>
        public long GetReversion()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 当前数据版本的签名作者
        /// </summary>
        /// <returns></returns>
        public string GetSignedAuthor()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 获取所在的版本库名称
        /// </summary>
        /// <returns></returns>
        public string GetRepositoryName()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
