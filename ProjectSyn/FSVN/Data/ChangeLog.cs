using System;
using System.Collections.Generic;
using System.Text;
using FSVN.Config;

namespace FSVN.Data
{
    /// <summary>
    /// 变更日志记录
    /// </summary>
    [Serializable]
    public class ChangeLog : MarshalByRefObject
    {
        private string a;
        /// <summary>
        /// 变更版本的作者
        /// </summary>
        public string Author
        {
            get { return a; }
            set { a = value; }
        }

        private string r;
        /// <summary>
        /// 版本库编号
        /// </summary>
        public string RepositoryId
        {
            get { return r; }
            set { r = value; }
        }

        private string v;
        /// <summary>
        /// 修改后的版本编号
        /// </summary>
        public string ReversionId
        {
            get { return v; }
            set { v = value; }
        }

        private string m;
        /// <summary>
        /// 变更备忘记录消息
        /// </summary>
        /// <value>记录消息</value>
        public string Message
        {
            get { return m; }
            set { m = value; }
        }

        /*
         创建、添加了哪些
         修改、更新了哪些
         删除了哪些
         移动了哪些
         */
        private byte[] s;
        /// <summary>
        /// 详细变更摘要
        /// </summary>
        [ObjectData(ConvertType = typeof(ChangeAction[]))]
        public byte[] Summary
        {
            get { return s; }
            set 
            {
                ObjectDataAttribute.Validate(value, GetType(), "Summary");
                s = value; 
            }
        }

    }

}
