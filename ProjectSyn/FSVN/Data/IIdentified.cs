using System;
using System.Collections.Generic;
using System.Text;

namespace FSVN.Data
{
    /// <summary>
    /// 表示实例类型可标志识别
    /// </summary>
    public interface IIdentified
    {
        /// <summary>
        /// 获取或设置该实例的标志
        /// </summary>
        string IID { get; set; }
    }
}
