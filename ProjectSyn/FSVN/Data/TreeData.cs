using System;
using System.Collections.Generic;
using System.Text;

namespace FSVN.Data
{
    /// <summary>
    /// 结构树数据项
    /// </summary>
    [Serializable]
	public class TreeItem
	{
        private string _i;
        /// <summary>
        /// 数据标志名称（在同一版本库内不重复）
        /// </summary>
        public string IdentityName
        {
            get { return _i; }
            set { _i = value; }
        }

        private string _r;
        /// <summary>
        /// 标志在库中的修改版本
        /// </summary>
        public string Reversion
        {
            get { return _r; }
            set { _r = value; }
        }

        //private string _p;
        ///// <summary>
        ///// 版本库编号
        ///// </summary>
        //public string RepositoryId
        //{
        //    get { return _p; }
        //    set { _p = value; }
        //}

        //private string c;
        ///// <summary>
        ///// 包含本数据的容器标志名称（在同一版本库内不重复）
        ///// </summary>
        //public string ContainerIdentityName
        //{
        //    get { return c; }
        //    set { c = value; }
        //}

        private bool d;
        /// <summary>
        /// 是否是目录结构
        /// </summary>
        public bool IsDirectory
        {
            get { return d; }
            set { d = value; }
        }

        //private TreeItem[] _c = new TreeItem[0];
        ///// <summary>
        ///// 自己树数据项
        ///// </summary>
        //public TreeItem[] ChildItems
        //{
        //    get { return _c; }
        //    set { _c = value; }
        //}

	}

    ///// <summary>
    ///// 结构树数据项根
    ///// </summary>
    //[Serializable]
    //public class RootTreeItem
    //{
    //    private TreeItem[] _i = new TreeItem[0];
    //    /// <summary>
    //    /// 数据项集合
    //    /// </summary>
    //    public TreeItem[] Items
    //    {
    //        get { return _i; }
    //        set { _i = value; }
    //    }
    //}
}
