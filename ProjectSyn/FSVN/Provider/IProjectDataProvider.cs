using System;
using System.Collections.Generic;
using System.Text;
using FSVN.Data;

namespace FSVN.Provider
{
    /// <summary>
    /// 项目数据存储提供者（包含文件和目录结构）
    /// </summary>
    public interface IProjectDataProvider
    {
        /// <summary>
        /// 初始化提供者
        /// </summary>
        void Initialize();

        #region 版本库数据
        /// <summary>
        /// 获取指定版本库标志的版本库数据
        /// </summary>
        /// <param name="repositoryID">版本库标志</param>
        ProjectRepository GetRepositoryData(string repositoryID);
        
        /// <summary>
        /// 存储库数据
        /// </summary>
        /// <param name="repos">库数据实例</param>
        void StoreRepositoryData(ProjectRepository repos);
        #endregion

        /// <summary>
        /// 判断版本库中是否存在标识号的数据
        /// </summary>
        /// <param name="repositoryID">版本库标志</param>
        /// <param name="identityName">项目数据标识号</param>
        /// <returns>存在则返回为true，不存在为false。</returns>
        bool Exists(string repositoryID, string identityName);

        #region 项目数据
        /// <summary>
        /// 存储项目数据
        /// </summary>
        /// <param name="datArray">项目数据队列集合</param>
        /// <param name="memo">变更备忘</param>
        void Store(ProjectData[] datArray, string memo);

        /// <summary>
        /// 删除项目数据
        /// </summary>
        /// <param name="datArray">项目数据标志队列集合</param>
        /// <param name="memo">变更备忘</param>
        void Delete(ProjectDataID[] datArray, string memo);

        /// <summary>
        /// 获取指定容器标志内的项目数据集合
        /// </summary>
        /// <param name="RepositoryId">版本库标志编号</param>
        /// <param name="ContainerIdentityName">包含容器的项目数据标志号，为null则为根下的项目数据。</param>
        /// <param name="rev">获取库中的特定版本：最新版本为$HEAD$。</param>
        /// <returns>项目数据集合</returns>
        ProjectData[] GetDataList(string RepositoryId, string ContainerIdentityName, string rev);

        /// <summary>
        /// 获取指定容器标志内的项目数据集合
        /// </summary>
        /// <param name="RepositoryId">版本库标志编号</param>
        /// <param name="IdentityName">项目数据标志编号</param>
        /// <param name="rev">获取库中的特定版本：最新版本为$HEAD$。</param>
        /// <returns>如果存在则返回项目数据，否则为null。</returns>
        ProjectData GetProjectData(string RepositoryId, string IdentityName, string rev);
        #endregion

    }
}
