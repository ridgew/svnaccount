using System;
using System.Collections.Generic;
using System.Text;
using FSVN.Config;

namespace FSVN.Data
{
    /// <summary>
    /// 项目数据
    /// </summary>
    [Serializable] 
    public class ProjectData : MarshalByRefObject
    {
        [NonSerialized]
        internal Type instanceType = typeof(ProjectData);

        private string n;
        /// <summary>
        /// 数据名称
        /// </summary>
        public string Name 
        {
            get { return n; }
            set { n = value; }
        }

        private string i;
        /// <summary>
        /// 数据标志名称（在同一版本库内不重复）
        /// </summary>
        public string IdentityName
        {
            get { return i; }
            set { i = value; }
        }

        private string p;
        /// <summary>
        /// 包含本数据的容器标志名称（在同一版本库内不重复）
        /// </summary>
        public string ContainerIdentityName
        {
            get { return p; }
            set { p = value; }
        }

        private int d;
        /// <summary>
        /// 在版本库中的数据深度
        /// </summary>
        public int Deepth
        {
            get { return d; }
            set { d = value; }
        }

        private DateTime c;
        /// <summary>
        /// 创建时间UTC
        /// </summary>
        public DateTime CreateDateTimeUTC
        {
            get { return c; }
            set { c = value; }
        }

        private DateTime m;
        /// <summary>
        /// 修改时间UTC
        /// </summary>
        public DateTime ModifiedDateTimeUTC
        {
            get { return m; }
            set { m = value; }
        }

        private bool b;
        /// <summary>
        /// 是否包含二进制数据
        /// </summary>
        public bool IncludeBinary
        {
            get { return b; }
            set { b = value; }
        }

        private string r;
        /// <summary>
        /// 版本库编号
        /// </summary>
        public string RepositoryId
        {
            get { return r; }
            set { r = value; }
        }

        private string v;
        /// <summary>
        /// 当前数据的版本
        /// </summary>
        public string Reversion
        {
            get { return v; }
            set { v = value; }
        }

        private string a;
        /// <summary>
        /// 当前数据版本的作者
        /// </summary>
        public string Author
        {
            get { return a; }
            set { a = value; }
        }

        private string s;
        /// <summary>
        /// 当前数据的效验值
        /// </summary>
        public string DataSum
        {
            get { return s; }
            set { s = value; }
        }

        private string l;
        /// <summary>
        /// 上一次修改的版本
        /// </summary>
        public string LastModifiedVersion
        {
            get { return l; }
            set { l = value; }
        }

        private byte[] u;
        /// <summary>
        /// 数据模块设置
        /// </summary>
        [ObjectData(ConvertType = typeof(ModuleData))]
        public byte[] ModuleConfig
        {
            get { return u; }
            set 
            {
                ObjectDataAttribute.Validate(value, instanceType, "ModuleConfig");
                u = value; 
            }
        }

        private byte[] w;
        /// <summary>
        /// 原始数据设置
        /// </summary>
        [ObjectData(ConvertType = typeof(RawData))]
        public byte[] RawConfig
        {
            get { return w; }
            set 
            {
                ObjectDataAttribute.Validate(value, instanceType, "RawConfig");
                w = value; 
            }
        }

    }

}
