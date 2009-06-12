using System;
using System.Text.RegularExpressions;
using FSVN.Provider;

namespace FSVN.Data
{
    /// <summary>
    /// 项目版本库
    /// </summary>
    [Serializable]
    public class ProjectRepository : MarshalByRefObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectRepository"/> class.
        /// </summary>
        public ProjectRepository()
            : base()
        {
            Provider = new FileSystemDataProvider();
            Provider.Initialize();
        }

        [NonSerialized]
        private IProjectDataProvider Provider = null;

        private string _rid;
        /// <summary>
        /// 版本库编号
        /// </summary>
        public string RepositoryId 
        {
            get { return _rid; }
            set { _rid = value; }
        }

        private string _rn;
        /// <summary>
        /// 版本库名称
        /// </summary>
        public string Name
        {
            get { return _rn; }
            set { _rn = value; }
        }

        private string _rm;
        /// <summary>
        /// 版本库备注
        /// </summary>
        public string Memo
        {
            get { return _rm; }
            set { _rm = value; }
        }

        private string _rv;
        /// <summary>
        /// 当前数据的版本
        /// </summary>
        public string Reversion
        {
            get { return _rv; }
            set { _rv = value; }
        }

        private string _zz;
        /// <summary>
        /// 当前数据版本的作者
        /// </summary>
        public string Author
        {
            get { return _zz; }
            set { _zz = value; }
        }

        private DateTime _cdt;
        /// <summary>
        /// 创建时间UTC
        /// </summary>
        public DateTime CreateDateTimeUTC
        {
            get { return _cdt; }
            set { _cdt = value; }
        }

        private DateTime _mdt;
        /// <summary>
        /// 修改时间UTC
        /// </summary>
        public DateTime ModifiedDateTimeUTC
        {
            get { return _mdt; }
            set { _mdt = value; }
        }

        #region 提供者实现配置
        private string _d;
        /// <summary>
        /// 数据提供者的实现名称
        /// </summary>
        public string DataProviderTypeName
        {
            get { return _d; }
            set { _d = value; }
        }

        private string _s;
        /// <summary>
        /// 数据同步提供者的实现名称
        /// </summary>
        public string SynProviderTypeName
        {
            get { return _s; }
            set { _s = value; }
        }

        private string _b;
        /// <summary>
        /// 获取或设置原始二进制实现提供者的实现名称
        /// </summary>
        public string RawProviderTypeName
        {
            get { return _b; }
            set { _b = value; }
        }

        private string _h;
        /// <summary>
        /// 数据变更钩子提供者的实现名称
        /// </summary>
        public string HookProviderTypeName
        {
            get { return _h; }
            set { _h = value; }
        }

        private string _c;
        /// <summary>
        /// 原始二进制压缩实现提供者的实现名称
        /// </summary>
        public string CompressProviderTypeName
        {
            get { return _c; }
            set { _c = value; }
        }

        private string _cs;
        /// <summary>
        /// 原始二进制效验实现提供者的实现名称
        /// </summary>
        public string CheckSumProviderTypeName
        {
            get { return _cs; }
            set { _cs = value; }
        }
        #endregion

        #region 方法调用API

        #region GET
        /// <summary>
        /// 获取数据库的下一个版本号
        /// </summary>
        /// <returns></returns>
        public string GetNextReversionID()
        {
            ProjectRepository repos = Provider.GetRepositoryData(RepositoryId);
            if (repos == null)
            {
                Console.WriteLine("Null");
                return "1";
            }
            else
            {
                return (Convert.ToInt64(repos.Reversion) + 1).ToString();
            }
        }

        /// <summary>
        /// 获取指定容器标志内的所有项目数据(最新版本)
        /// </summary>
        /// <param name="ContainerIdentityName">容器标志</param>
        /// <returns>指定库版本的项目数据</returns>
        public ProjectData[] GetDataList(string ContainerIdentityName)
        {
            return GetDataList(ContainerIdentityName, "$HEAD$");
        }

        /// <summary>
        /// 获取指定容器标志内的所有项目数据
        /// </summary>
        /// <param name="ContainerIdentityName">容器标志</param>
        /// <param name="rev">库版本</param>
        /// <returns>指定库版本的项目数据</returns>
        public ProjectData[] GetDataList(string ContainerIdentityName, string rev)
        {
            return new ProjectData[0];
        }

        /// <summary>
        /// 获取最新版本的标志项目数据
        /// </summary>
        /// <param name="IdentityName">项目标志名称</param>
        /// <returns>项目数据，没有找到则为null。</returns>
        public ProjectData GetProjectData(string IdentityName)
        {
            return GetProjectData(IdentityName, "$HEAD$");
        }

        /// <summary>
        /// 获取指定版本的标志项目数据
        /// </summary>
        /// <param name="IdentityName">项目标志名称</param>
        /// <param name="rev">库版本</param>
        /// <returns>项目数据，没有找到则为null。</returns>
        public ProjectData GetProjectData(string IdentityName, string rev)
        {
            return Provider.GetProjectData(RepositoryId, IdentityName, rev);
        }
        #endregion

        /// <summary>
        /// 判断版本库中是否存在标识号的数据
        /// </summary>
        /// <param name="identityName">项目数据标识号</param>
        /// <returns>存在则返回为true，不存在为false。</returns>
        public bool Exists(string identityName)
        {
            return Provider.Exists(RepositoryId, identityName);
        }

        #region Commit

        /// <summary>
        /// 提交项目数据修改
        /// </summary>
        /// <param name="dat">项目数据</param>
        /// <returns>如提交成功则返回提交之后的最新版本号</returns>
        public string Commit(ProjectData dat)
        {
            return Commit(new ProjectData[] { dat }, string.Empty);
        }

        /// <summary>
        /// 提交项目数据修改
        /// </summary>
        /// <param name="dat">项目数据</param>
        /// <param name="memo">备注</param>
        /// <returns>如提交成功则返回提交之后的最新版本号</returns>
        public string Commit(ProjectData dat, string memo)
        {
            return Commit(new ProjectData[] { dat }, memo);
        }

        /// <summary>
        /// 提交项目数据队列修改
        /// </summary>
        /// <param name="datArray">项目数据队列</param>
        /// <returns>如提交成功则返回提交之后的最新版本号</returns>
        public string Commit(ProjectData[] datArray)
        {
            return Commit(datArray, string.Empty);
        }

        /// <summary>
        /// 提交项目数据队列修改
        /// </summary>
        /// <param name="datArray">项目数据队列</param>
        /// <param name="memo">备注</param>
        /// <returns>如提交成功则返回提交之后的最新版本号</returns>
        public string Commit(ProjectData[] datArray, string memo)
        {
            string Rev = GetNextReversionID();
            Provider.Store(datArray, memo);

            //更新版本库数据
            this.Reversion = Rev;
            this.Author = "ridge";
            this.ModifiedDateTimeUTC = DateTime.Now.ToUniversalTime();
            Provider.StoreRepositoryData(this);

            return Rev;
        }

        /// <summary>
        /// 提交删除的项目数据队列
        /// </summary>
        /// <param name="delArray">项目数据标识集</param>
        /// <param name="memo">操作备忘</param>
        /// <returns>如提交成功则返回提交之后的最新版本号</returns>
        public string Commit(ProjectDataID[] delArray, string memo)
        {
            string Rev = GetNextReversionID();
            Provider.Delete(delArray, memo);

            //更新版本库数据
            this.Reversion = Rev;
            this.Author = "ridge";
            this.ModifiedDateTimeUTC = DateTime.Now.ToUniversalTime();
            Provider.StoreRepositoryData(this);

            return Rev;
        }
        #endregion

        #region 移动

        /// <summary>
        /// 移动（重命名）的项目数据对列
        /// </summary>
        /// <param name="removArray">项目数据变更标识集</param>
        /// <param name="memo">操作备忘</param>
        /// <returns>如提交成功则返回提交之后的最新版本号</returns>
        public string Remove(DataMoveAction[] removArray, string memo)
        {
            string Rev = GetNextReversionID();
            Provider.Remove(removArray, memo);

            ////更新版本库数据
            //this.Reversion = Rev;
            //this.Author = "ridge";
            //this.ModifiedDateTimeUTC = DateTime.Now.ToUniversalTime();
            //Provider.StoreRepositoryData(this);

            return Rev;
        }

        #endregion


        /// <summary>
        /// 同步两个版本库
        /// </summary>
        /// <param name="repository">模板版本库</param>
        /// <param name="synConfig">同步选项配置</param>
        /// <returns>同步结果</returns>
        public string SynWithRepository(ProjectRepository repository, SynOption synConfig)
        {
            return "";
        }

        /// <summary>
        /// 绑定库属性
        /// </summary>
        public void DataBind()
        {
            ProjectRepository repos = Provider.GetRepositoryData(RepositoryId);
            if (repos == null)
            {
                Provider.StoreRepositoryData(this);
            }
            else
            {
                Reversion = repos.Reversion;
                Author = repos.Author;
            }
        }

        #endregion

        /// <summary>
        /// 获取变更到指定版本的变更日志
        /// </summary>
        /// <param name="startRev">起始版本</param>
        /// <param name="rev">获取库中的特定版本：最新版本为$HEAD$。</param>
        /// <returns>变更日志数据集合</returns>
        public ChangeLog[] GetReversionLogs(long startRev, string rev)
        {
            return Provider.GetReversionLogs(RepositoryId, startRev, rev);
        }

        private T GetProviderInstance<T>(string providerName)
            where T : new()
        {
            //DataProviderTypeName
            return new T();
        }

    }
}
