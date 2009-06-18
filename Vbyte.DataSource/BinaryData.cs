using System;
using System.Collections.Generic;
using System.Text;

namespace Vbyte.DataSource
{
    /// <summary>
    /// 二进制数据
    /// </summary>
    [Serializable]
    public class BinaryData : AbstractData
    {
        /// <summary>
        /// 上级数据项(向上公开)
        /// </summary>
        protected IDataItem ParentDataItem = null;

        /// <summary>
        /// 上级数据项
        /// </summary>
        /// <returns></returns>
        public override IDataItem GetParent()
        {
            return ParentDataItem;
        }

        /// <summary>
        /// 获取子级数据项
        /// </summary>
        /// <returns></returns>
        public override IDataItem[] GetChildren()
        {
            throw new NotImplementedException();
        }

    }
}
