using System;

namespace FIPFAPI.Limited.Config
{
    /// <summary>
    /// 存在性：移除
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public class Remove : Attribute
    {
        
    }
}
