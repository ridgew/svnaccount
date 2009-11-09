using System;

namespace SynUtil
{
    /// <summary>
    /// 标记接口的实现状态和实现版本
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Method, AllowMultiple = false)]
    public class ImplementStateAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ImplementStateAttribute"/> class.
        /// </summary>
        /// <param name="impState">接口的实现状态</param>
        /// <param name="version">实现版本.</param>
        public ImplementStateAttribute(CompleteState impState, string version)
        {
            ImplementComplete = impState;
            Version = version;
        }

        /// <summary>
        /// 获取或设置实现完成状态
        /// </summary>
        public CompleteState ImplementComplete
        {
            get;
            set;
        }

        /// <summary>
        /// 获取或设置接口实现版本
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// 获取或设置接口实现的描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 发布时间的GTM格式
        /// </summary>
        public string ReleaseDateGTM { get; set; }
    }

    /// <summary>
    /// 接口实现完成状态枚举
    /// </summary>
    public enum CompleteState : byte
    {
        /// <summary>
        /// 草稿状态
        /// </summary>
        Draft,
        /// <summary>
        /// 待实现状态
        /// </summary>
        TODO,
        /// <summary>
        /// 实现调试中状态
        /// </summary>
        DEBUG,
        /// <summary>
        /// 调试了一部分，等待下一步进行。
        /// </summary>
        WAITNEXT,
        /// <summary>
        /// 实现完成状态
        /// </summary>
        OK,
        /// <summary>
        /// 已过时状态，不建议使用。
        /// </summary>
        Obsolete
    }
}
