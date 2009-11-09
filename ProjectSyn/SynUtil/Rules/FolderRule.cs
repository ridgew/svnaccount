using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;

namespace SynUtil
{
    #region Folder Rule
    [Serializable, XmlInclude(typeof(FolderNameRule)), XmlInclude(typeof(FolderAttributeRule))
    , XmlInclude(typeof(FolderCreateTimeRule)), XmlInclude(typeof(FolderChangeTimeRule))]
    public abstract class FolderRuleBase : SynRule
    {
        public abstract bool Actived(DirectoryInfo dInfo);

        /// <summary>
        /// Activeds the specified rt val.
        /// </summary>
        /// <param name="rtVal">The rt val.</param>
        /// <returns></returns>
        public override bool Actived(object rtVal)
        {
            bool blnRet = false;
            DirectoryInfo dInfo = rtVal as DirectoryInfo;
            if (dInfo != null)
            {
                blnRet = Actived(dInfo);
            }
            return blnRet;
        }

    }

    /// <summary>
    /// 文件夹属性规则
    /// </summary>
    [Serializable]
    [ImplementState(CompleteState.OK, "0.1", Description = "针对文件夹属性的过滤规则", ReleaseDateGTM = "Fri, 06 Nov 2009 09:34:58 GMT")]
    public class FolderAttributeRule : FolderRuleBase
    {
        /*
         ReadOnly Hidden System Normal Encrypted
         */
        /// <summary>
        /// Activeds the specified d info.
        /// </summary>
        /// <param name="dInfo">The d info.</param>
        /// <returns></returns>
        public override bool Actived(DirectoryInfo dInfo)
        {
            return GetAttributeActived(() => dInfo.Attributes);
        }
    }

    /// <summary>
    /// 文件夹名称规则
    /// </summary>
    [Serializable]
    public class FolderNameRule : FolderRuleBase
    {

        /// <summary>
        /// Activeds the specified d info.
        /// </summary>
        /// <param name="dInfo">The d info.</param>
        /// <returns></returns>
        public override bool Actived(DirectoryInfo dInfo)
        {
            return GetRegxPatternActived(() => dInfo.Name);
        }
    }

    /// <summary>
    /// 文件夹创建时间规则
    /// </summary>
    [Serializable]
    public class FolderCreateTimeRule : FolderRuleBase
    {

        /// <summary>
        /// Activeds the specified d info.
        /// </summary>
        /// <param name="dInfo">The d info.</param>
        /// <returns></returns>
        public override bool Actived(DirectoryInfo dInfo)
        {
            return GetTimeActived(() => dInfo.CreationTime);
        }
    }

    /// <summary>
    /// 文件夹修改时间规则
    /// </summary>
    [Serializable]
    public class FolderChangeTimeRule : FolderRuleBase
    {

        /// <summary>
        /// Activeds the specified d info.
        /// </summary>
        /// <param name="dInfo">The d info.</param>
        /// <returns></returns>
        public override bool Actived(DirectoryInfo dInfo)
        {
            return GetTimeActived(() => dInfo.LastWriteTime);
        }
    }
    #endregion
}
