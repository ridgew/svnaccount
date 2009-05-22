using System;
using System.Text;

namespace FSVN.Data
{
    /// <summary>
    /// 变更动作
    /// </summary>
    [Serializable]
    public abstract class ChangeAction : MarshalByRefObject
    {
        /// <summary>
        /// 获取或设置此次变更中受影响的标志集合
        /// </summary>
        /// <value>项目数据标志集合</value>
        public string[] IdentityNames { get; set; }

        /// <summary>
        /// 获取可阅读的变更摘要
        /// </summary>
        public abstract string GetSummary();
    }

    /// <summary>
    /// 添加变更
    /// </summary>
    [Serializable]
    public class AddAction : ChangeAction
    {
        public AddAction()
            : base()
        { }

        /// <summary>
        /// 获取可阅读的变更摘要
        /// </summary>
        public override string GetSummary()
        {
            if (base.IdentityNames != null && base.IdentityNames.Length > 0)
            {
                StringBuilder sb = new StringBuilder();
                foreach (string id in IdentityNames)
                {
                    sb.AppendFormat("ADD {0}", id);
                    sb.AppendLine();
                }
                return sb.ToString();
            }
            else
            {
                return string.Empty;
            }
        }

    }

    /// <summary>
    /// 修改变更
    /// </summary>
    [Serializable]
    public class UpdateAction : ChangeAction
    {
        public UpdateAction()
            : base()
        { }

        /// <summary>
        /// 获取可阅读的变更摘要
        /// </summary>
        public override string GetSummary()
        {
            if (base.IdentityNames != null && base.IdentityNames.Length > 0)
            {
                StringBuilder sb = new StringBuilder();
                foreach (string id in IdentityNames)
                {
                    sb.AppendFormat("UPDATE {0}", id);
                    sb.AppendLine();
                }
                return sb.ToString();
            }
            else
            {
                return string.Empty;
            }
        }

    }

    /// <summary>
    /// 修改变更
    /// </summary>
    [Serializable]
    public class DeleteAction : ChangeAction
    {
        public DeleteAction()
            : base()
        { }

        /// <summary>
        /// 获取可阅读的变更摘要
        /// </summary>
        public override string GetSummary()
        {
            if (base.IdentityNames != null && base.IdentityNames.Length > 0)
            {
                StringBuilder sb = new StringBuilder();
                foreach (string id in IdentityNames)
                {
                    sb.AppendFormat("DELETE {0}", id);
                    sb.AppendLine();
                }
                return sb.ToString();
            }
            else
            {
                return string.Empty;
            }
        }

    }

    /// <summary>
    /// 移动变更
    /// </summary>
    [Serializable]
    public class MoveAction : ChangeAction
    {
        public MoveAction()
            : base()
        { }

        /// <summary>
        /// 获取或设置此次变更中对应的目标标志集合
        /// </summary>
        /// <value>项目数据标志集合</value>
        public string[] TargetIdentityNames { get; set; }

        /// <summary>
        /// 获取可阅读的变更摘要
        /// </summary>
        public override string GetSummary()
        {
            if (base.IdentityNames != null && base.IdentityNames.Length > 0)
            {
                if (TargetIdentityNames == null || TargetIdentityNames.Length != IdentityNames.Length)
                {
                    throw new InvalidOperationException("移动目标信息设置不正确！");
                }
                else
                {
                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < IdentityNames.Length; i++)
                    {
                        sb.AppendFormat("MOVE {0} to {1}", IdentityNames[i], TargetIdentityNames[i]);
                        sb.AppendLine(); 
                    }
                    return sb.ToString();
                }
            }
            else
            {
                return string.Empty;
            }
        }

    }

}
