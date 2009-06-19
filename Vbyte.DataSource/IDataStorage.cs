using System;

namespace Vbyte.DataSource
{
    /// <summary>
    /// 数据存储库实现
    /// </summary>
    public interface IDataStorage
    {
        /// <summary>
        /// 保存特定数据
        /// </summary>
        /// <param name="item">数据实例</param>
        void Store(IDataItem item);

        /// <summary>
        /// 删除指定标志的数据
        /// </summary>
        /// <param name="identityName">数据标志号</param>
        void Remove(string identityName);

        /// <summary>
        /// 获取指定标志的数据
        /// </summary>
        /// <param name="identityName">数据标志号</param>
        /// <returns>如果存在该数据则返回，否则为null。</returns>
        IDataItem GetData(string identityName);

        /// <summary>
        /// 获取满足指定标志的数据集合
        /// </summary>
        /// <param name="containerIdentityName">父级容器标识名称，若为null或空则获取顶层相关数据。</param>
        /// <param name="filter">判断匹配规则</param>
        /// <param name="isMatch">匹配方向：true则匹配，false则为不匹配。</param>
        /// <returns>如果存在则返回集合，否则为null或空数组。</returns>
        IDataItem[] GetDataList(string containerIdentityName, Predicate<IDataItem> filter, bool isMatch);
    }

    /// <summary>
    /// 数据项
    /// </summary>
    public interface IDataItem
    {
        /// <summary>
        /// 名称
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// 标志名称，类型URL地址定位等。
        /// </summary>
        string IdentityName { get; set; }

        /// <summary>
        /// 别名
        /// </summary>
        string[] Alias { get; set; }

        /// <summary>
        /// 是否时数据项容器
        /// </summary>
        bool IsContainer { get; set; }
        
        /// <summary>
        /// 原始二进制数据
        /// </summary>
        byte[] RawData { get; set; }

        /// <summary>
        ///创建时间UTC
        /// </summary>
        DateTime CreateDateTimeUTC { get; set; }

        /// <summary>
        /// 修改时间UTC
        /// </summary>
        DateTime ModifiedDateTimeUTC { get; set; }

        /// <summary>
        /// 上级数据项
        /// </summary>
        IDataItem GetParent();

        /// <summary>
        /// 获取子级数据项
        /// </summary>
        IDataItem[] GetChildren();

        /// <summary>
        /// 相关属性数据绑定
        /// </summary>
        void DataBind();

        ///// <summary>
        ///// 获取或设置数据的存取库
        ///// </summary>
        //IDataStorage Storage { get; set; }
    }

    /// <summary>
    /// 数据仓库
    /// </summary>
    public interface IDataRepository
    {
        /// <summary>
        /// 仓库名称
        /// </summary>
        string RepositoryName { get; set; }

        /// <summary>
        /// 仓库描述
        /// </summary>
        string RepositoryDescription { get; set; }

        /// <summary>
        /// 仓库版本
        /// </summary>
        long RepositoryReversion { get; set; }

        /// <summary>
        /// 仓库内存储的数据
        /// </summary>
        IDataItem[] GetRepositoryItems();
    }

    /// <summary>
    /// 版本控制实现
    /// </summary>
    public interface IVersionControl
    {
        /// <summary>
        /// 当前数据的版本
        /// </summary>
        long GetReversion();

        /// <summary>
        /// 当前数据版本的签名作者
        /// </summary>
        string GetSignedAuthor();

        /// <summary>
        /// 获取所在的版本库名称
        /// </summary>
        string GetRepositoryName();
    }

    /// <summary>
    /// 版本控制的数据存储库
    /// </summary>
    public interface IVersionControlStorage : IDataStorage, IVersionControl
    {
        /// <summary>
        /// 获取相关实例类型的指定版本
        /// </summary>
        /// <param name="identityName">数据标志号</param>
        /// <param name="rev">版本号</param>
        /// <returns>如果存在该数据则返回，否则为null。</returns>
        IDataItem GetReversionData(string identityName, long rev);

        /// <summary>
        /// 获取满足指定标志的数据集合
        /// </summary>
        /// <param name="filter">判断匹配规则</param>
        /// <param name="isMatch">匹配方向：true则匹配，false则为不匹配。</param>
        /// <param name="rev">版本号</param>
        /// <returns>如果存在则返回集合，否则为null或空数组。</returns>
        IDataItem[] GetDataList(Predicate<IDataItem> filter, bool isMatch, long rev);
    }

    /// <summary>
    /// 版本控制的数据项
    /// </summary>
    public interface IVersionControlItem : IDataItem, IVersionControl
    {

    }

    /// <summary>
    /// 数据归档打包
    /// </summary>
    public interface IDataArchive
    {
        /// <summary>
        /// 从指定二进制数据包中抽取
        /// </summary>
        /// <param name="binDat">二进制数据包</param>
        /// <param name="rpt">进度及消息汇报器</param>
        void Extract(byte[] binDat, IWorkReporter rpt);

        /// <summary>
        /// 打包相应仓库数据
        /// </summary>
        /// <param name="repository">数据仓库</param>
        /// <param name="rpt">进度及消息汇报器</param>
        void Archive(IDataRepository repository, IWorkReporter rpt);
    }

    /// <summary>
    /// 工作报告器
    /// </summary>
    public interface IWorkReporter
    {
        /// <summary>
        /// 设置百分比
        /// </summary>
        /// <param name="percent">百分比的小数</param>
        void SetPercent(float percent);

        /// <summary>
        /// 输出消息
        /// </summary>
        /// <param name="msg">消息内容</param>
        void WriteMessage(string msg);
    }

}
