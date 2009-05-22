using System;
using System.Collections.Generic;
using System.Text;

namespace FSVN
{
    /// <summary>
    /// 
    /// </summary>
    public class ProjectInfo
    {
        public string ProjectName { get; set; }

        public DateTime CreateDate { get; set; }

        public DateTime LastUpdate { get; set; }

        public float Version { get; set; }

        public string Comment { get; set; }
    }

}
