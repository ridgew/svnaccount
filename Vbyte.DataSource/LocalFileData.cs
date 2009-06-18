using System;
using System.IO;

namespace Vbyte.DataSource
{
    /// <summary>
    /// 本地文件数据
    /// </summary>
    public sealed class LocalFileData : BinaryData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LocalFileData"/> class.
        /// </summary>
        public LocalFileData()
            : base()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalFileData"/> class.
        /// </summary>
        /// <param name="filePathName">本地文件路径</param>
        public LocalFileData(string filePathName)
        {
            IdentityName = filePathName;
        }

        private bool _isDirectory = false;

        /// <summary>
        /// 标志名称，类型URL地址定位等。
        /// </summary>
        /// <value></value>
        public override string IdentityName
        {
            get
            {
                return base.IdentityName;
            }
            set
            {
                _isDirectory = Directory.Exists(value);

                base.IdentityName = value;
            }
        }

        /// <summary>
        /// 原始二进制数据
        /// </summary>
        /// <value></value>
        public override byte[] RawData
        {
            get
            {
                return base.RawData;
            }
            set
            {
                base.RawData = value;
            }
        }

        /// <summary>
        /// 获取子级数据项
        /// </summary>
        /// <returns></returns>
        public override IDataItem[] GetChildren()
        {
            return base.GetChildren();
        }

        /// <summary>
        /// 上级数据项
        /// </summary>
        /// <returns></returns>
        public override IDataItem GetParent()
        {
            return base.GetParent();
        }

    }
}
