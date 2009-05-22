using System;

namespace FIPFAPI.Limited.Config
{
    /// <summary>
    /// 存在性：新增
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited=true, AllowMultiple = false)]
    public class New : Attribute
    {
    }
}
