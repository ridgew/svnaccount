﻿using System;
using System.Collections.Generic;
using System.Text;
using Vbyte.DataSource.VersionControl;

namespace Vbyte.DataSource
{
    /// <summary>
    /// 文件系统存储(版本控制)
    /// </summary>
    public sealed class VersionedFSStorage : FileSystemStorage, IVersionControlStorage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VersionedFSStorage"/> class.
        /// </summary>
        public VersionedFSStorage()
            : base()
        { 
        
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VersionedFSStorage"/> class.
        /// </summary>
        /// <param name="baseDir">The base dir.</param>
        public VersionedFSStorage(string baseDir)
            : base(baseDir)
        { 
        
        }

        /// <summary>
        /// 获取所有的版本控制库
        /// </summary>
        /// <returns></returns>
        public Repository[] GetRepositories()
        {
            
            return null;
        }

        #region IVersionControlStorage 成员

        /// <summary>
        /// 获取相关实例类型的指定版本
        /// </summary>
        /// <param name="repositoryName">版本仓库名</param>
        /// <param name="identityName">数据标志号</param>
        /// <param name="rev">版本号</param>
        /// <returns>如果存在该数据则返回，否则为null。</returns>
        public IDataItem GetReversionData(string repositoryName, string identityName, long rev)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 获取满足指定标志的数据集合
        /// </summary>
        /// <param name="repositoryName">版本仓库名</param>
        /// <param name="containerIdentityName">父级容器标识名称，若为null或空则获取顶层相关数据。</param>
        /// <param name="filter">判断匹配规则</param>
        /// <param name="isMatch">匹配方向：true则匹配，false则为不匹配。</param>
        /// <param name="rev">版本号</param>
        /// <returns>如果存在则返回集合，否则为null或空数组。</returns>
        public IDataItem[] GetDataList(string repositoryName, string containerIdentityName, Predicate<IDataItem> filter, bool isMatch, long rev)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
