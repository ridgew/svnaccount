using System;
using System.Collections.Generic;
using System.Text;

namespace FIPFAPI.Limited
{
    /// <summary>
    /// 实现了新增、获取、更新、删除的API
    /// </summary>
    public interface ICRUDAPI : ICreateAPI, IReadAPI, IModifyAPI, IDeleteAPI
    {

    }

}
