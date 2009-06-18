using System;

namespace Vbyte.DataSource
{
    /// <summary>
    /// 文本数据
    /// </summary>
    [Serializable]
    public class TextData : BinaryData
    {
        private string _cs;

        /// <summary>
        /// 获取或设置文件数据的字符编码
        /// </summary>
        /// <value>字符编码</value>
        public string Charset
        {
            get 
            {
                if (string.IsNullOrEmpty(_cs)) _cs = System.Text.Encoding.Default.WebName;
                return _cs; 
            }

            set 
            { 
                _cs = value; 
            }

        }

    }
}
