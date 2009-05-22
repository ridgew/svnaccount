using System;

namespace FSVN.Data
{
    /// <summary>
    /// 项目模块
    /// </summary>
    [Serializable]
    public class ProjectModule : MarshalByRefObject
    {
        /// <summary>
        /// 模块标志编号
        /// </summary>
        public string ModuleID { get; set; }

        /// <summary>
        /// 包含本模块的模块标志编号（父级模块）
        /// </summary>
        public string ContainerModuleID { get; set; }

        /// <summary>
        /// 在模块容器内是否可选
        /// </summary>
        public bool IsOptional { get; set; }

        /// <summary>
        /// 本模块所依赖的模块ID集合
        /// </summary>
        public string[] DependencyModules { get; set; }

        /// <summary>
        /// 模块名称
        /// </summary>
        public string ModuleName { get; set; }

        /// <summary>
        /// 创建时间UTC
        /// </summary>
        public DateTime CreateDateTimeUTC { get; set; }

        /// <summary>
        /// 修改时间UTC
        /// </summary>
        public DateTime ModifiedDateTimeUTC { get; set; }

        /// <summary>
        /// 版本库编号
        /// </summary>
        public string RepositoryId { get; set; }

        /// <summary>
        /// 当前数据的版本
        /// </summary>
        public string Reversion { get; set; }

        /// <summary>
        /// 当前数据版本的作者
        /// </summary>
        public string Author { get; set; }

        /// <summary>
        /// 上一次修改的版本
        /// </summary>
        public string LastModifiedVersion { get; set; }

    }

}
