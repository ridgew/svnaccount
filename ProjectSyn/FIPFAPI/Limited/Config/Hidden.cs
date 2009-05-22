using System;
using System.Collections.Generic;
using System.Text;

namespace FIPFAPI.Limited.Config
{
    /// <summary>
    /// 存在性：隐藏、消失、失踪
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public class Hidden : Attribute
    {

    }

}
