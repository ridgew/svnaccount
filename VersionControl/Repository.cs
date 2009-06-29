using System;
using System.Collections.Generic;
using System.Text;

namespace Vbyte.DataSource.VersionControl
{
    /// <summary>
    /// 版本控制库
    /// </summary>
    [Serializable]
    public class Repository : AbstractData
    {
        /// <summary>
        /// 上级数据项(无父级对象)
        /// </summary>
        /// <returns></returns>
        public override IDataItem GetParent()
        {
            return null;
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
