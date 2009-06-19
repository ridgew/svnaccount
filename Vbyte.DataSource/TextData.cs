using System;

namespace Vbyte.DataSource
{
    /// <summary>
    /// 文本数据
    /// </summary>
    [Serializable]
    public class TextData : AbstractData
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

        /// <summary>
        /// 上级数据项
        /// </summary>
        /// <returns></returns>
        public override IDataItem GetParent()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 获取子级数据项
        /// </summary>
        /// <returns></returns>
        public override IDataItem[] GetChildren()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 相关属性数据绑定
        /// </summary>
        public override void DataBind()
        {
            throw new NotImplementedException();
        }
    }
}
