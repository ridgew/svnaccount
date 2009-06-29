using System;
using System.Collections.Generic;
using System.Text;

namespace Vbyte.DataSource
{
    /// <summary>
    /// 抽象数据基类
    /// </summary>
    [Serializable]
    public abstract class AbstractData : IDataItem
    {
        #region IDataItem 成员

        private string _n;
        /// <summary>
        /// 名称
        /// </summary>
        /// <value></value>
        public string Name
        {
            get
            {
                return _n;
            }
            set
            {
                _n = value;
            }
        }

        private string _i;
        /// <summary>
        /// 标志名称，类型URL地址定位等(始终用/分隔目录)。
        /// </summary>
        /// <value></value>
        public virtual string IdentityName
        {
            get
            {
                return _i;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentNullException("数据标识名称必须为有效数据！");
                }

                _i = value.Replace('\\', '/');
            }
        }

        private string[] _a = new string[0];
        /// <summary>
        /// 别名
        /// </summary>
        /// <value></value>
        public string[] Alias
        {
            get
            {
                return _a;
            }
            set
            {
                _a = value;
            }
        }

        private bool _c = false;
        /// <summary>
        /// 是否时数据项容器
        /// </summary>
        /// <value></value>
        public bool IsContainer
        {
            get
            {
                return _c;
            }
            set
            {
                _c = value;
            }
        }

        private byte[] _b = new byte[0];
        /// <summary>
        /// 原始二进制数据，默认为0自己数组。
        /// </summary>
        /// <value></value>
        public virtual byte[] RawData
        {
            get
            {
                return _b;
            }
            set
            {
                _b = value;
            }
        }

        private DateTime _cd;
        /// <summary>
        /// 创建时间UTC
        /// </summary>
        /// <value></value>
        public virtual DateTime CreateDateTimeUTC
        {
            get
            {
                return _cd;
            }
            set
            {
                _cd = value;
            }
        }

        private DateTime _md;
        /// <summary>
        /// 修改时间UTC
        /// </summary>
        /// <value></value>
        public virtual DateTime ModifiedDateTimeUTC
        {
            get
            {
                return _md;
            }
            set
            {
                _md = value;
            }
        }

        /// <summary>
        /// 上级数据项
        /// </summary>
        /// <returns></returns>
        public abstract IDataItem GetParent();

        /// <summary>
        /// 获取子级数据项
        /// </summary>
        /// <returns></returns>
        public abstract IDataItem[] GetChildren();

        /// <summary>
        /// 相关属性数据绑定
        /// </summary>
        public abstract void DataBind();

        //[NonSerialized]
        //private IDataStorage _storage = null;

        ///// <summary>
        ///// 获取或设置数据的存取库
        ///// </summary>
        //public virtual IDataStorage Storage 
        //{
        //    get { return _storage; }
        //    set { _storage = value; }
        //}

        #endregion
    }
}
