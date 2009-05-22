using System;
using System.Collections.Generic;
using System.Text;

namespace FSVN.Provider
{
    /// <summary>
    /// 原始二进制数据获取提供者实现
    /// </summary>
    public interface IRawDataProvider
    {
        /// <summary>
        /// 获取原始二进制数据
        /// </summary>
        /// <param name="RepositoryId">版本库编号</param>
        /// <param name="IdentityName">数据标志名称（在同一版本库内不重复）</param>
        /// <returns>指定版本库内特定标志的二进制数据</returns>
        byte[] GetBinaryData(string RepositoryId, string IdentityName);

        /// <summary>
        /// 存储原始二进制数据
        /// </summary>
        /// <param name="RepositoryId">版本库编号</param>
        /// <param name="IdentityName">数据标志名称（在同一版本库内不重复）</param>
        /// <param name="binDat">存储的二进制数据</param>
        void StoreBinaryData(string RepositoryId, string IdentityName, byte[] binDat);
    }
}
