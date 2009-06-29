using System;

namespace Vbyte.DataSource.Configuration
{
    /// <summary>
    /// 实现功能版本号
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class ImplementVersionAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ImplementVersionAttribute"/> class.
        /// </summary>
        /// <param name="ver">The version.</param>
        public ImplementVersionAttribute(string ver)
            : base()
        {
            Version = ver;
        }

        /// <summary>
        /// 实现的相关版本
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// 相关描述
        /// </summary>
        public string Description { get; set; }
    }
}
