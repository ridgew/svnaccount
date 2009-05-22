using System;

namespace FSVN.Data
{
    /// <summary>
    /// 项目原始数据
    /// </summary>
    [Serializable]
    public class RawData : MarshalByRefObject
    {
        private bool t;
        /// <summary>
        /// 是否是文本数据
        /// </summary>
        public bool IsText
        {
            get { return t; }
            set { t = value; }
        }

        private bool c;
        /// <summary>
        /// 标志该数据是否已压缩
        /// </summary>
        public bool IsCompressed
        {
            get { return c; }
            set { c = value; }
        }

        private string s;
        /// <summary>
        /// 字符集编号
        /// </summary>
        public string Charset
        {
            get { return s; }
            set { s = value; }
        }

        private byte[] b;
        /// <summary>
        /// 原始二进制数据
        /// </summary>
        public byte[] BinaryData
        {
            get { return b; }
            set { b = value; }
        }
    }
}
