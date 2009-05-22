using System;

namespace FIPFAPI.Limited.Config
{
    /// <summary>
    /// 存在性：更新、修改、替换
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public class Update : Attribute
    {
    }
}
