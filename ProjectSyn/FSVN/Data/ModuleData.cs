using System;

namespace FSVN.Data
{
    /// <summary>
    /// 判断数据是否是模块数据
    /// </summary>
    [Serializable]
    public class ModuleData : MarshalByRefObject
    {
        private string m;
        /// <summary>
        /// 模块编号
        /// </summary>
        public string ModuleID
        {
            get { return m; }
            set { m = value; }
        }

        private bool o;
        /// <summary>
        /// 在模块内是否可选
        /// </summary>
        public bool IsOptional
        {
            get { return o; }
            set { o = value; }
        }

    }
}
