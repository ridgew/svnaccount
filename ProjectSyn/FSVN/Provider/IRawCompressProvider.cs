using System;

namespace FSVN.Provider
{
    /// <summary>
    /// 针对原始二进制数据的压缩实现
    /// </summary>
    public interface IRawCompressProvider
    {
        /// <summary>
        /// 获取压缩之后的二进制数据
        /// </summary>
        /// <param name="rawDat">原始二进制数据</param>
        /// <returns>压缩之后的二进制数据</returns>
        byte[] GetCompressed(byte[] rawDat);

        /// <summary>
        /// 获取解压缩之后的原始二进制数据
        /// </summary>
        /// <param name="cmpDat">压缩之后的二进制数据</param>
        /// <returns>原始二进制数据</returns>
        byte[] GetRawData(byte[] cmpDat);
    }
}
