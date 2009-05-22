using System;

namespace FSVN.Provider
{
    /// <summary>
    /// 原始二进制数据效验(确定唯一性)提供者实现
    /// </summary>
    public interface IRawSumProvider
    {
        /// <summary>
        /// 获取二进制数据的数据效验
        /// </summary>
        /// <param name="binDat">原始二进制</param>
        /// <returns>数据效验标志字符</returns>
        string GetCRC(byte[] binDat);

    }
}
